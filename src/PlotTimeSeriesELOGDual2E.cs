using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class PlotTimeSeriesELOGDual2E : PlotTimeSeries2Channels
    {
        private int m_samplingFrequencyInt = 0;

        public PlotTimeSeriesELOGDual2E(int samplingFrequency)
        {
            m_samplingFrequencyInt = samplingFrequency;
            m_samplingFrequency = (double)samplingFrequency;
        }

        public bool ReadTimeSeries(string directoryName)
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
            return true;
        }
        protected override bool ReadFilesOfYesterday()
        {
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfYesterday(yyyyMMDDCur);
            string directoryNameYesterday = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            if (!ReadTimeSeries(directoryNameYesterday))
            {
                return false;
            }
            return true;
        }

        protected override bool ReadFilesOfTomorrow()
        {
            string yyyyMMDDCur = Util.SubstringRight(m_directoryName[0], 8);
            string yyyyMMDDMod = Util.GetDateStringOfTomorrow(yyyyMMDDCur);
            string directoryNameTomorrow = m_directoryName[0].Replace(yyyyMMDDCur, yyyyMMDDMod);
            if (!ReadTimeSeries(directoryNameTomorrow))
            {
                return false;
            }
            return true;
        }

        protected override bool RereadCurrentFiles()
        {
            if (!ReadTimeSeries(m_directoryName[0]))
            {
                return false;
            }
            return true;
        }

        protected override void ButtonResponseFunctionEstimation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You need to select magnetic field data to estimate response functions.", "Error", MessageBoxButton.OK);
            return;
        }
    }
}
