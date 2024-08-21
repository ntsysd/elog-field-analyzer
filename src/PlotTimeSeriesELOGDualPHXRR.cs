using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class PlotTimeSeriesELOGDualPHXRR : PlotTimeSeries6Channels
    {
        private int m_samplingFrequencyInt = 0;

        public PlotTimeSeriesELOGDualPHXRR(int samplingFrequency)
        {
            m_samplingFrequencyInt = samplingFrequency;
            m_samplingFrequency = (double)samplingFrequency;
        }

        public bool ReadTimeSeries(string directoryName, string directoryNameMag, string directoryNameRR)
        {
            { // Ex and Ey (ELOG-DUAL)
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
            }
            {// Hx and Hy (ELOG-MT)
                if (!Directory.Exists(@directoryNameMag))
                {
                    MessageBox.Show("Folder" + directoryNameMag + " does not exist.", "Error", MessageBoxButton.OK);
                    return false;
                }
                var dir = new DirectoryInfo(@directoryNameMag);
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
                for (int ch = 2; ch < 4; ch++)
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
                    if (!Util.ReadOneELOGMTFileHxHyOnly(file.FullName, m_samplingFrequencyInt * (int)offset.TotalSeconds, m_samplingFrequencyInt, ref m_values[2], ref m_values[3]))
                    {
                        return false;
                    }
                }
                for (int ch = 2; ch < 4; ch++)
                {
                    m_directoryName[ch] = directoryNameMag;
                }
            }

            {// Hrx and Hry (ELOG-MT)
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
                for (int ch = 4; ch < 6; ch++)
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
                    if (!Util.ReadOneELOGMTFileHxHyOnly(file.FullName, m_samplingFrequencyInt * (int)offset.TotalSeconds, m_samplingFrequencyInt, ref m_values[4], ref m_values[5]))
                    {
                        return false;
                    }
                }
                for (int ch = 4; ch < 6; ch++)
                {
                    m_directoryName[ch] = directoryNameRR;
                }
            }
            return true;
        }

        protected override bool ReadFilesOfYesterday()
        {
            // Ex and Ey (ELOG-DUAL)
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfYesterday(yyyyMMDDCur);
            string directoryNameYesterday = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            // Hx and Hy (ELOG-MT)
            string yyyyMMDDCurMag = Util.SubstringRight(m_directoryName[2], 8);
            string yyyyMMDDModMag = Util.GetDateStringOfYesterday(yyyyMMDDCurMag);
            string directoryNameYesterdayMag = m_directoryName[2].Replace(yyyyMMDDCurMag, yyyyMMDDModMag);
            // Hrx and Hry (ELOG-MT)
            string yyyyMMDDCurRR = Util.SubstringRight(m_directoryName[4], 8);
            string yyyyMMDDModRR = Util.GetDateStringOfYesterday(yyyyMMDDCurRR);
            string directoryNameYesterdayRR = m_directoryName[4].Replace(yyyyMMDDCurRR, yyyyMMDDModRR);
            if (!ReadTimeSeries(directoryNameYesterday, directoryNameYesterdayMag, directoryNameYesterdayRR))
            {
                return false;
            }
            return true;
        }

        protected override bool ReadFilesOfTomorrow()
        {
            // Ex and Ey (ELOG-DUAL)
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfTomorrow(yyyyMMDDCur);
            string directoryNameTomorrow = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            // Hx and Hy (ELOG-MT)
            string yyyyMMDDCurMag = Util.SubstringRight(m_directoryName[2], 8);
            string yyyyMMDDModMag = Util.GetDateStringOfTomorrow(yyyyMMDDCurMag);
            string directoryNameTomorrowMag = m_directoryName[2].Replace(yyyyMMDDCurMag, yyyyMMDDModMag);
            // Hrx and Hry (ELOG-MT)
            string yyyyMMDDCurRR = Util.SubstringRight(m_directoryName[4], 8);
            string yyyyMMDDModRR = Util.GetDateStringOfTomorrow(yyyyMMDDCurRR);
            string directoryNameTomorrowRR = m_directoryName[4].Replace(yyyyMMDDCurRR, yyyyMMDDModRR);
            if (!ReadTimeSeries(directoryNameTomorrow, directoryNameTomorrowMag, directoryNameTomorrowRR))
            {
                return false;
            }
            return true;
        }

        protected override bool RereadCurrentFiles()
        {
            if (!ReadTimeSeries(m_directoryName[0], m_directoryName[2], m_directoryName[4]))
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
            var responseFunctionEstimation = new ResponseFunctionEstimationELOGDualRR(m_directoryName, m_fileName, m_samplingFrequencyInt, startIndex, endIndex, false);
            responseFunctionEstimation.ShowDialog();
        }
    }
}
