using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    class PlotTimeSeriesText : PlotTimeSeries5Channels
    {

        public PlotTimeSeriesText(double samplingFrequency, DateTime startDateTime) { 
            m_samplingFrequency = samplingFrequency;
            for (int ch =0; ch < m_numOfChannels; ++ch)
            {
                m_startDateTime[ch] = startDateTime;
            }
        }

        public bool ReadTimeSeries(string [] fileNameFull)
        {
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                TimeSpan timeSpan;
                if (!Util.ReadTextFile(fileNameFull[ch], m_samplingFrequency, ref m_values[ch], out timeSpan))
                {
                    return false;
                }
                m_timeSpan[ch] = timeSpan;
                m_directoryName[ch] = System.IO.Path.GetDirectoryName(fileNameFull[ch]);
                m_fileName[ch] = System.IO.Path.GetFileName(fileNameFull[ch]);
            }
            return true;
        }

        protected override void ButtonResponseFunctionEstimation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This function is not supported.", "Error", MessageBoxButton.OK);
            return;
        }

        protected override bool ReadFilesOfYesterday()
        {
            MessageBox.Show("This function is not supported.", "Error", MessageBoxButton.OK);
            return false;
        }

        protected override bool ReadFilesOfTomorrow()
        {
            MessageBox.Show("This function is not supported.", "Error", MessageBoxButton.OK);
            return false;
        }

        protected override bool RereadCurrentFiles()
        {
            MessageBox.Show("This function is not supported.", "Error", MessageBoxButton.OK);
            return false;
        }

    }
}
