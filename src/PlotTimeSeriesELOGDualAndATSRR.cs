using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class PlotTimeSeriesELOGDualAndATSRR : PlotTimeSeries6Channels
    {
        private int m_samplingFrequencyInt = 0;
        private bool m_isELOG1K = false;

        public PlotTimeSeriesELOGDualAndATSRR(int samplingFrequency, bool isELOG1K)
        {
            m_samplingFrequencyInt = samplingFrequency;
            m_isELOG1K = isELOG1K;
            m_samplingFrequency = (double)samplingFrequency;
        }

        public bool ReadTimeSeries(string directoryName, string fileNameATSHxFull, string fileNameATSHrxFull)
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
            m_startDateTime[0] = dateTimeMin;
            m_startDateTime[1] = dateTimeMin;
            m_timeSpan[0] = diff;
            m_timeSpan[1] = diff;
            int numSamples = m_samplingFrequencyInt * (int)diff.TotalSeconds;
            for (int ch = 0; ch < 2; ch++)
            {
                if (m_values[ch] == null || m_values[ch].Length != numSamples)
                {
                    m_values[ch] = new double[numSamples];
                }
            }
            foreach (var file in files)
            {
                DateTime dateDimeCur = Util.ConvELOGFileNameToDateTime(file.FullName);
                TimeSpan offset = dateDimeCur - dateTimeMin;
                if (!Util.ReadOneELOGDualFile(file.FullName, m_samplingFrequencyInt * (int)offset.TotalSeconds, m_samplingFrequencyInt, ref m_values[0], ref m_values[1]))
                {
                    return false;
                }
            }
            m_directoryName[0] = directoryName;
            m_directoryName[1] = directoryName;
            int samplingFrequency = 0;
            DateTime startDateTime;
            TimeSpan timeSpan;
            // Hy, Hry
            for (int ch = 2; ch < 6; ch += 2)
            {
                string fileNameFull = null;
                switch(ch)
                {
                    case 2:
                        fileNameFull = fileNameATSHxFull;
                        break;
                    case 4:
                        fileNameFull = fileNameATSHrxFull;
                        break;
                    default:
                        break;
                }
                if (!Util.ReadATSFile(fileNameFull, ref m_values[ch], out startDateTime, out timeSpan, out samplingFrequency))
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
                m_directoryName[ch] = System.IO.Path.GetDirectoryName(fileNameFull);
                m_fileName[ch] = System.IO.Path.GetFileName(fileNameFull);
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
            // Hy, Hry
            for (int ch = 3; ch < 6; ch += 2)
            {
                m_directoryName[ch] = m_directoryName[ch - 1];
                string fileNameHry = m_fileName[ch - 1].Replace("_C02_", "_C03_");
                fileNameHry = fileNameHry.Replace("_THx_", "_THy_");
                m_fileName[ch] = fileNameHry;
                string fileNameFull = System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]);
                if (!Util.ReadATSFile(fileNameFull, ref m_values[ch], out startDateTime, out timeSpan, out samplingFrequency))
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
            // Hx
            string previousFileNameCh2 = Util.countdownRunNumberOfATS(m_fileName[2]);
            if (String.IsNullOrEmpty(previousFileNameCh2))
            {
                MessageBox.Show("File previous to " + m_fileName[2] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string previousFileNamCh2Full = System.IO.Path.Combine(m_directoryName[2], previousFileNameCh2);
            // Hrx
            string previousFileNameCh4 = Util.countdownRunNumberOfATS(m_fileName[4]);
            if (String.IsNullOrEmpty(previousFileNameCh4))
            {
                MessageBox.Show("File previous to " + m_fileName[4] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string previousFileNameCh4Full = System.IO.Path.Combine(m_directoryName[4], previousFileNameCh4);
            // ELOG1K
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfYesterday(yyyyMMDDCur);
            string directoryNameYesterday = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            if (!ReadTimeSeries(directoryNameYesterday, previousFileNamCh2Full, previousFileNameCh4Full))
            {
                return false;
            }
            return true;
        }

        protected override bool ReadFilesOfTomorrow()
        {
            // Hx
            string nextFileNameCh2 = Util.countupRunNumberOfATS(m_fileName[2]);
            if (String.IsNullOrEmpty(nextFileNameCh2))
            {
                MessageBox.Show("File next to " + m_fileName[2] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string nextFileNameCh2Full = System.IO.Path.Combine(m_directoryName[2], nextFileNameCh2);
            // Hrx
            string nextFileNameCh4 = Util.countupRunNumberOfATS(m_fileName[4]);
            if (String.IsNullOrEmpty(nextFileNameCh4))
            {
                MessageBox.Show("File next to " + m_fileName[4] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string nextFileNameCh4Full = System.IO.Path.Combine(m_directoryName[4], nextFileNameCh4);
            // ELOG1K
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfTomorrow(yyyyMMDDCur);
            string directoryNameTomorrow = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            if (!ReadTimeSeries(directoryNameTomorrow, nextFileNameCh2Full, nextFileNameCh4Full))
            {
                return false;
            }
            return true;
        }

        protected override bool RereadCurrentFiles()
        {
            string fileNameCh2Full = System.IO.Path.Combine(m_directoryName[2], m_fileName[2]);
            string fileNameCh4Full = System.IO.Path.Combine(m_directoryName[4], m_fileName[4]);
            if (!ReadTimeSeries(m_directoryName[0], fileNameCh2Full, fileNameCh4Full))
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
            ResponseFunctionEstimationELOGDualAndATSRR responseFunctionEstimation = new ResponseFunctionEstimationELOGDualAndATSRR(m_directoryName, m_fileName, m_samplingFrequencyInt, startIndex, endIndex, m_isELOG1K);
            responseFunctionEstimation.ShowDialog();
        }
    }
}
