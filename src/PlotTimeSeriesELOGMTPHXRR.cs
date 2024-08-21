using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class PlotTimeSeriesELOGMTPHXRR : PlotTimeSeries7Channels
    {
        private int m_samplingFrequencyInt = 0;

        public PlotTimeSeriesELOGMTPHXRR(int samplingFrequency)
        {
            m_samplingFrequencyInt = samplingFrequency;
            m_samplingFrequency = (double)samplingFrequency;
        }

        public bool ReadTimeSeries(string directoryName, string directoryNameRR)
        {
            {// Local data 
                if (!Directory.Exists(@directoryName))
                {
                    MessageBox.Show("Folder" + directoryName + " does not exist.", "Error", MessageBoxButton.OK);
                    return false;
                }
                var dir = new DirectoryInfo(@directoryName);
                string fileNameCompared = "*_" + m_samplingFrequencyInt.ToString() + "*.dat";
                FileInfo[] files = dir.GetFiles(fileNameCompared);
                DateTime dateTimeMin = DateTime.MaxValue;
                DateTime dateTimeMax = DateTime.MinValue;
                if (!files.Any())
                {
                    MessageBox.Show("There is no proper dat file.", "Error", MessageBoxButton.OK);
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
            }
            {// Remote reference data 
                if (!Directory.Exists(@directoryNameRR))
                {
                    MessageBox.Show("Folder" + directoryNameRR + " does not exist.", "Error", MessageBoxButton.OK);
                    return false;
                }
                var dir = new DirectoryInfo(@directoryNameRR);
                string fileNameCompared = "*_" + m_samplingFrequencyInt.ToString() + "*.dat";
                FileInfo[] files = dir.GetFiles(fileNameCompared);
                DateTime dateTimeMin = DateTime.MaxValue;
                DateTime dateTimeMax = DateTime.MinValue;
                if (!files.Any())
                {
                    MessageBox.Show("There is no proper dat file.", "Error", MessageBoxButton.OK);
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
                for (int ch = 5; ch < 7; ch++)
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
                    if (!Util.ReadOneELOGMTFileHxHyOnly(file.FullName, m_samplingFrequencyInt * (int)offset.TotalSeconds, m_samplingFrequencyInt, ref m_values[5], ref m_values[6]))
                    {
                        return false;
                    }
                }
                for (int ch = 5; ch < 7; ch++)
                {
                    m_directoryName[ch] = directoryNameRR;
                }
            } 
            return true;
        }

        protected override bool ReadFilesOfYesterday()
        {
            // Local
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfYesterday(yyyyMMDDCur);
            string directoryNameYesterday = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            // Remote
            string yyyyMMDDCurRR = Util.SubstringRight(m_directoryName[5], 8);
            string yyyyMMDDModRR = Util.GetDateStringOfYesterday(yyyyMMDDCurRR);
            string directoryNameYesterdayRR = m_directoryName[5].Replace(yyyyMMDDCurRR, yyyyMMDDModRR);
            if (!ReadTimeSeries(directoryNameYesterday, directoryNameYesterdayRR))
            {
                return false;
            }
            return true;
        }

        protected override bool ReadFilesOfTomorrow()
        {
            // Local
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfTomorrow(yyyyMMDDCur);
            string directoryNameTomorrow = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            // Remote
            string yyyyMMDDCurRR = Util.SubstringRight(m_directoryName[5], 8);
            string yyyyMMDDModRR = Util.GetDateStringOfTomorrow(yyyyMMDDCurRR);
            string directoryNameTomorrowRR = m_directoryName[5].Replace(yyyyMMDDCurRR, yyyyMMDDModRR);
            if (!ReadTimeSeries(directoryNameTomorrow, directoryNameTomorrowRR))
            {
                return false;
            }
            return true;
        }

        protected override bool RereadCurrentFiles()
        {
            if (!ReadTimeSeries(m_directoryName[0], m_directoryName[5]))
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
            var responseFunctionEstimation = new ResponseFunctionEstimationELOGMTPHX(m_directoryName, m_fileName, m_samplingFrequencyInt, startIndex, endIndex);
            responseFunctionEstimation.ShowDialog();
        }
    }
}
