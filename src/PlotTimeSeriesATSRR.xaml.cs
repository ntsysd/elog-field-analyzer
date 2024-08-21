using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class PlotTimeSeriesATSRR : PlotTimeSeries7Channels
    {
        int m_samplingFrequencyInt = 0;

        public bool ReadTimeSeries(string fileNameCh0Full, string fileNameHrxFull)
        {
            int samplingFrequency = 0;
            DateTime startDateTime;
            TimeSpan timeSpan;
            if (!Util.ReadATSFile(fileNameCh0Full, ref m_values[0], out startDateTime, out timeSpan, out samplingFrequency))
            {
                return false;
            }
            m_startDateTime[0] = startDateTime;
            m_timeSpan[0] = timeSpan;
            m_samplingFrequencyInt = samplingFrequency;
            m_samplingFrequency = (double)samplingFrequency;
            m_directoryName[0] = System.IO.Path.GetDirectoryName(fileNameCh0Full);
            m_fileName[0] = System.IO.Path.GetFileName(fileNameCh0Full);
            if (!m_fileName[0].Contains("_C00_"))
            {
                MessageBox.Show("File name does not contain '_C00_'.", "Error", MessageBoxButton.OK);
                this.Close();
                return false;
            }
            if (!m_fileName[0].Contains("_TEx_"))
            {
                MessageBox.Show("File name does not contain '_TEx_'.", "Error", MessageBoxButton.OK);
                this.Close();
                return false;
            }
            for (int ch = 1; ch < m_numOfLocalChannels; ++ch)
            {
                m_directoryName[ch] = m_directoryName[0];
                string fileName = m_fileName[0].Replace("_C00_", "_C0" + ch.ToString() + "_");
                switch (ch)
                {
                    case 1:
                        fileName = fileName.Replace("_TEx_", "_TEy_");
                        break;
                    case 2:
                        fileName = fileName.Replace("_TEx_", "_THx_");
                        break;
                    case 3:
                        fileName = fileName.Replace("_TEx_", "_THy_");
                        break;
                    case 4:
                        fileName = fileName.Replace("_TEx_", "_THz_");
                        break;
                }
                m_fileName[ch] = fileName;
                if (!Util.ReadATSFile(System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]), ref m_values[ch], out startDateTime, out timeSpan, out samplingFrequency))
                {
                    return false;
                }
                m_startDateTime[ch] = startDateTime;
                m_timeSpan[ch] = timeSpan;
                if (samplingFrequency != m_samplingFrequencyInt)
                {
                    MessageBox.Show("Sampling frequency of Ch#" + ch.ToString() + "(" + samplingFrequency.ToString() + ") is different from that of Ch#0(" + m_samplingFrequencyInt.ToString() + ").", "Error", MessageBoxButton.OK);
                    this.Close();
                    return false;
                }
            }
            // Hrx
            {
                int rr = 0;
                int ch = m_numOfLocalChannels + rr;
                m_fileNameFullRR[rr] = fileNameHrxFull;
                if (!Util.ReadATSFile(fileNameHrxFull, ref m_values[ch], out startDateTime, out timeSpan, out samplingFrequency))
                {
                    return false;
                }
                if (samplingFrequency != m_samplingFrequencyInt)
                {
                    MessageBox.Show("Sampling frequency of Ch#" + ch.ToString() + "(" + samplingFrequency.ToString() + ") is different from that of Ch#0(" + m_samplingFrequencyInt.ToString() + ").", "Error", MessageBoxButton.OK);
                    this.Close();
                    return false;
                }
                m_directoryName[ch] = System.IO.Path.GetDirectoryName(fileNameHrxFull);
                m_fileName[ch] = System.IO.Path.GetFileName(fileNameHrxFull);
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
                m_startDateTime[ch] = startDateTime;
                m_timeSpan[ch] = timeSpan;
            }
            // Hry
            {
                int rr = 1;
                int ch = m_numOfLocalChannels + rr;
                m_directoryName[ch] = m_directoryName[ch - 1];
                string fileNameHry = m_fileName[ch-1].Replace("_C02_", "_C03_");
                fileNameHry = fileNameHry.Replace("_THx_", "_THy_");
                m_fileName[ch] = fileNameHry;
                m_fileNameFullRR[rr] = System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]);
                if (!Util.ReadATSFile(m_fileNameFullRR[rr], ref m_values[ch], out startDateTime, out timeSpan, out samplingFrequency))
                {
                    return false;
                }
                m_startDateTime[ch] = startDateTime;
                m_timeSpan[ch] = timeSpan;
                if (samplingFrequency != m_samplingFrequencyInt)
                {
                    MessageBox.Show("Sampling frequency of Ch#" + ch.ToString() + "(" + samplingFrequency.ToString() + ") is different from that of Ch#0(" + m_samplingFrequencyInt.ToString() + ").", "Error", MessageBoxButton.OK);
                    this.Close();
                    return false;
                }
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
            string[] fileNamesFull = new string[m_numOfChannels];
            for (int ch = 0; ch < m_numOfChannels; ch++)
            {
                fileNamesFull[ch] = System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]);
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
            ResponseFunctionEstimationATSRR responseFunctionEstimation = new ResponseFunctionEstimationATSRR(m_directoryName, m_fileName, m_samplingFrequencyInt, startIndex, endIndex);
            responseFunctionEstimation.ShowDialog();
        }
  
        protected override bool ReadFilesOfYesterday()
        {
            string previousFileNameCh0 = Util.countdownRunNumberOfATS(m_fileName[0]);
            if (String.IsNullOrEmpty(previousFileNameCh0))
            {
                MessageBox.Show("File previous to " + m_fileName[0] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string previousFileNameCh0Full = System.IO.Path.Combine(m_directoryName[0], previousFileNameCh0);
            int ch = m_numOfLocalChannels; 
            string fileNameRR = System.IO.Path.GetFileName(m_fileNameFullRR[0]);
            string previousFileNameRR = Util.countdownRunNumberOfATS(fileNameRR);
            if (String.IsNullOrEmpty(previousFileNameRR))
            {
                MessageBox.Show("File previous to " + fileNameRR + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string previousFileNameFullRR = System.IO.Path.Combine(m_directoryName[ch], previousFileNameRR);
            if (!ReadTimeSeries(previousFileNameCh0Full, previousFileNameFullRR))
            {
                return false;
            }
            return true;
        }

        protected override bool ReadFilesOfTomorrow()
        {
            string nextFileNameCh0 = Util.countupRunNumberOfATS(m_fileName[0]);
            if (String.IsNullOrEmpty(nextFileNameCh0))
            {
                MessageBox.Show("File next to " + m_fileName[0] + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string nextFileNameCh0Full = System.IO.Path.Combine(m_directoryName[0], nextFileNameCh0);
            int ch = m_numOfLocalChannels;
            string fileNameRR = System.IO.Path.GetFileName(m_fileNameFullRR[0]);
            string nextFileNameRR = Util.countupRunNumberOfATS(fileNameRR);
            if (String.IsNullOrEmpty(nextFileNameRR))
            {
                MessageBox.Show("File next to " + fileNameRR + " cannot be found.", "Error", MessageBoxButton.OK);
                return false;
            }
            string nextFileNameFullRR = System.IO.Path.Combine(m_directoryName[ch], nextFileNameRR);
            if (!ReadTimeSeries(nextFileNameCh0Full, nextFileNameFullRR))
            {
                return false;
            }
            return true;
        }

        protected override bool RereadCurrentFiles()
        {
            string fileNameCh0Full = System.IO.Path.Combine(m_directoryName[0], m_fileName[0]);
            if (!ReadTimeSeries(fileNameCh0Full, m_fileNameFullRR[0]))
            {
                return false;
            }
            return true;
        }
    }
}
