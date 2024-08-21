using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class PlotTimeSeriesELOGMTADURR : PlotTimeSeries7Channels
    {
        private int m_samplingFrequencyInt = 0;

        public PlotTimeSeriesELOGMTADURR(int samplingFrequency)
        {
            m_samplingFrequencyInt = samplingFrequency;
            m_samplingFrequency = (double)samplingFrequency;
        }

        public bool ReadTimeSeries(string directoryName, string fileNameATSHrxFull)
        {
            if (!Directory.Exists(@directoryName))
            {
                MessageBox.Show("Folder (" + directoryName +") does not exists.", "Error", MessageBoxButton.OK);
                return false;
            }
            var dir = new DirectoryInfo(@directoryName);
            string fileNameCompared = "*_" + m_samplingFrequencyInt.ToString() + "*.dat";
            FileInfo[] files = dir.GetFiles(fileNameCompared);
            DateTime dateTimeMin = DateTime.MaxValue;
            DateTime dateTimeMax = DateTime.MinValue;
            if (!files.Any())
            {
                MessageBox.Show("There is no target files.", "Error", MessageBoxButton.OK);
                return false;
            }
            foreach (var file in files)
            {
                DateTime dateDimeCur = Util.ConvELOGFileNameToDateTime(file.FullName);
                if (dateDimeCur > dateTimeMax)
                {
                    dateTimeMax = dateDimeCur;
                }
                if (dateDimeCur < dateTimeMin)
                {
                    dateTimeMin = dateDimeCur;
                }
            }
            dateTimeMax = dateTimeMax.AddHours(1);
            TimeSpan diff = dateTimeMax - dateTimeMin;
            int numSamples = m_samplingFrequencyInt * (int)diff.TotalSeconds;
            for (int ch = 0; ch < 5; ch++)
            {
                m_startDateTime[ch] = dateTimeMin;
                m_timeSpan[ch] = diff;
                if (m_values[ch] == null || m_values[ch].Length != numSamples)
                {
                    m_values[ch] = new double[numSamples];
                }
            }
            foreach (var file in files)
            {
                DateTime dateDimeCur = Util.ConvELOGFileNameToDateTime(file.FullName);
                TimeSpan offset = dateDimeCur - dateTimeMin;
                if (!Util.ReadOneELOGMTFile(file.FullName, m_samplingFrequencyInt * (int)offset.TotalSeconds, m_samplingFrequencyInt, ref m_values[0], ref m_values[1], ref m_values[2], ref m_values[3], ref m_values[4]))
                {
                    return false;
                }
            }
            for (int ch = 0; ch < 5; ch++)
            {
                m_directoryName[ch] = directoryName;
            }
            int samplingFrequency = 0;
            DateTime startDateTime;
            TimeSpan timeSpan;
            // Hrx
            {
                int ch = 5;
                if (!Util.ReadATSFile(fileNameATSHrxFull, ref m_values[ch], out startDateTime, out timeSpan, out samplingFrequency))
                {
                    return false;
                }
                if (samplingFrequency != m_samplingFrequencyInt)
                {
                    MessageBox.Show("Sampling frequency of Ch#" + ch.ToString() + "(" + samplingFrequency.ToString() + ") is different from that of Ch#0(" + m_samplingFrequencyInt.ToString() + ").", "Error", MessageBoxButton.OK);
                    return false;
                }
                m_startDateTime[ch] = startDateTime;
                m_timeSpan[ch] = timeSpan;
                m_directoryName[ch] = System.IO.Path.GetDirectoryName(fileNameATSHrxFull);
                m_fileName[ch] = System.IO.Path.GetFileName(fileNameATSHrxFull);
                if (!m_fileName[ch].Contains("_C02_"))
                {
                    MessageBox.Show("File name does not contain '_C02_'.", "Error", MessageBoxButton.OK);
                    this.Close();
                    return false;
                }
                if (!m_fileName[ch].Contains("_THx_"))
                {
                    MessageBox.Show("File name does not contain '_THx_'.", "Error", MessageBoxButton.OK);
                    this.Close();
                    return false;
                }
            }
            // Hry
            {
                int ch = 6;
                m_directoryName[ch] = m_directoryName[ch - 1];
                string fileNameHry = m_fileName[ch - 1].Replace("_C02_", "_C03_");
                fileNameHry = fileNameHry.Replace("_THx_", "_THy_");
                m_fileName[ch] = fileNameHry;
                string fileNameATSHryFull = System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]);
                if (!Util.ReadATSFile(fileNameATSHryFull, ref m_values[ch], out startDateTime, out timeSpan, out samplingFrequency))
                {
                    return false;
                }
                m_startDateTime[ch] = startDateTime;
                m_timeSpan[ch] = timeSpan;
                if (samplingFrequency != m_samplingFrequencyInt)
                {
                    MessageBox.Show("Sampling frequency of Ch#" + ch.ToString() + "(" + samplingFrequency.ToString() + ") is different from that of Ch#0(" + m_samplingFrequencyInt.ToString() + ").", "Error", MessageBoxButton.OK);
                    return false;
                }
            }
            return true;
        }

        protected override bool ReadFilesOfYesterday()
        {
            // Hrx
            string previousFileNameCh5 = Util.countdownRunNumberOfATS(m_fileName[5]);
            if (String.IsNullOrEmpty(previousFileNameCh5))
            {
                MessageBox.Show("File previous to " + m_fileName[5] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string previousFileNameCh5Full = System.IO.Path.Combine(m_directoryName[5], previousFileNameCh5);
            // ELOG-MT
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfYesterday(yyyyMMDDCur);
            string directoryNameYesterday = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            if (!ReadTimeSeries(directoryNameYesterday, previousFileNameCh5Full))
            {
                return false;
            }
            return true;
        }

        protected override bool ReadFilesOfTomorrow()
        {
            // Hrx
            string nextFileNameCh5 = Util.countupRunNumberOfATS(m_fileName[5]);
            if (String.IsNullOrEmpty(nextFileNameCh5))
            {
                MessageBox.Show("File next to " + m_fileName[5] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string nextFileNameCh5Full = System.IO.Path.Combine(m_directoryName[5], nextFileNameCh5);
            // ELOG-MT
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfTomorrow(yyyyMMDDCur);
            string directoryNameTomorrow = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            if (!ReadTimeSeries(directoryNameTomorrow, nextFileNameCh5Full))
            {
                return false;
            }
            return true;
        }

        protected override bool RereadCurrentFiles()
        {
            string fileNameCh5Full = System.IO.Path.Combine(m_directoryName[5], m_fileName[5]);
            if (!ReadTimeSeries(m_directoryName[0], fileNameCh5Full))
            {
                return false;
            }
            return true;
        }


        protected override void ButtonResponseFunctionEstimation_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RadioButtonFree.IsChecked)
            {
                MessageBoxResult result = MessageBox.Show("Do you use all data? The calculation cost may be expensive.", "Warning", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        break;
                    case MessageBoxResult.No:
                        return;
                }
            }
            int[] startIndex = new int[m_numOfChannels];
            int[] endIndex = new int[m_numOfChannels];
            int offset = 0;
            for (int ch = 0; ch < m_numOfChannels; ch++)
            {
                startIndex[ch] = m_startIndexIncludingNegative[ch];
                endIndex[ch] = m_endIndex[ch];
                if (m_startIndexIncludingNegative[ch] < offset)
                {
                    offset = m_startIndexIncludingNegative[ch];
                }
            }
            for (int ch = 0; ch < m_numOfChannels; ch++)
            {
                startIndex[ch] -= offset;
                if (startIndex[ch] >= endIndex[ch])
                {
                    MessageBox.Show("Data length used for the estimation is less than zero.", "Error", MessageBoxButton.OK);
                    return;
                }
            }
            var responseFunctionEstimation = new ResponseFunctionEstimationELOGMTADURR(m_directoryName, m_fileName, m_samplingFrequencyInt, startIndex, endIndex);
            responseFunctionEstimation.ShowDialog();
        }
    }
}
