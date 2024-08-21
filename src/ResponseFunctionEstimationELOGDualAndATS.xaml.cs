using Microsoft.Win32;
using Forms = System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlotTimeSeries
{
    /// <summary>
    /// ResponseFunctionEstimationELOGDualAndATS.xaml の相互作用ロジック
    /// </summary>
    public partial class ResponseFunctionEstimationELOGDualAndATS : Window
    {
        static protected int m_numOfChannels = 4;
        protected string[] m_directoryName;
        protected string[] m_fileName;
        protected int m_samplingFrequencyInt;
        protected int[] m_startIndex;
        protected int[] m_endIndex;
        protected bool m_isELOG1K = false;

        public ResponseFunctionEstimationELOGDualAndATS(string[] directoryName, string[] fileName, int samplingFrequency, int[] startIndex, int[] endIndex, bool isELOG1K)
        {
            m_directoryName = directoryName;
            m_fileName = fileName;
            m_samplingFrequencyInt = samplingFrequency;
            m_startIndex = startIndex;
            m_endIndex = endIndex;
            m_isELOG1K = isELOG1K;
            InitializeComponent();
            Title += "   " + Common.VERSION;
            string sbuf = Util.ReadFromSettingIni("TextBoxWorkFolder.Text");
            if (sbuf != null)
            {
                TextBoxWorkFolder.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxCoilCalHx.Text");
            if (sbuf != null)
            {
                TextBoxCoilCalHx.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxCoilCalHy.Text");
            if (sbuf != null)
            {
                TextBoxCoilCalHy.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxELOGCal.Text");
            if (sbuf != null)
            {
                TextBoxELOGCal.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxLoggerCalFilesFolder.Text");
            if (sbuf != null)
            {
                TextBoxLoggerCalFilesFolder.Text = sbuf;
            }
            RadioButtonRRMS.IsHitTestVisible = false;
            RadioButtonRRMS.IsTabStop = false;
        }

        private void ButtonCoilCalHx_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "TXT file|*.TXT|All(*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxCoilCalHx.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxCoilCalHx.Text", TextBoxCoilCalHx.Text);
            }
        }

        private void ButtonCoilCalHy_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "TXT file|*.TXT|All(*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxCoilCalHy.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxCoilCalHy.Text", TextBoxCoilCalHy.Text);
            }
        }

        private void ButtonWorkFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxWorkFolder.Text = dialog.SelectedPath;
                    Util.WriteToSettingIni("TextBoxWorkFolder.Text", TextBoxWorkFolder.Text);
                }
            }
        }

        private void ButtonELOGCal_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "TXT file|*.TXT|All(*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxELOGCal.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxELOGCal.Text", TextBoxELOGCal.Text);
            }
        }

        private void ButtonLoggerCalFilesFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxLoggerCalFilesFolder.Text = dialog.SelectedPath;
                    Util.WriteToSettingIni("TextBoxLoggerCalFilesFolder.Text", TextBoxLoggerCalFilesFolder.Text);
                }
            }
        }

        private void ButtonMakeInputFile_Click(object sender, RoutedEventArgs e)
        {
            MakeInputFileForTRACMT();
        }

        protected List<int> DetermineSegmentLengths(out bool isSuccess, out int totalDataLength)
        {
            List<int> segmentLengths = new List<int>();
            isSuccess = true;
            totalDataLength = 0;
            int downsamplingRate = 1;
            if ((bool)CheckBoxDownsampling.IsChecked)
            {
                if (String.IsNullOrEmpty(TextBoxDownsamplingRate.Text))
                {
                    MessageBox.Show("You need to specify downsampling rate", "Error", MessageBoxButton.OK);
                    isSuccess = false;
                    return segmentLengths;
                }
                if (!int.TryParse(TextBoxDownsamplingRate.Text, out downsamplingRate))
                {
                    MessageBox.Show(TextBoxDownsamplingRate.Text + " cannot be regarded as an integer.", "Error", MessageBoxButton.OK);
                    isSuccess = false;
                    return segmentLengths;
                }
                if (downsamplingRate <= 1)
                {
                    MessageBox.Show("Downsampling rate is too small.", "Error", MessageBoxButton.OK);
                    isSuccess = false;
                    return segmentLengths;
                }
            }
            totalDataLength = m_endIndex[0] - m_startIndex[0];
            if (totalDataLength < (int)System.Math.Pow(2, 5))
            {
                MessageBox.Show("Selected time length is too  short.", "Error", MessageBoxButton.OK);
                isSuccess = false;
                return segmentLengths;
            }
            for (int ch = 1; ch < m_numOfChannels; ch++)
            {
                int length = m_endIndex[ch] - m_startIndex[ch];
                if (length < totalDataLength)
                {
                    totalDataLength = length;
                }
            }
            for (int i = 5; i < 18; i++)
            {
                int segmentLength = (int)System.Math.Pow(2, i);
                int totalDataLengthMod = totalDataLength;
                if ((bool)CheckBoxDownsampling.IsChecked)
                {
                    totalDataLengthMod = (totalDataLength - 100) / downsamplingRate;
                }
                if (segmentLength > totalDataLengthMod / 32)
                {
                    break;
                }
                segmentLengths.Add(segmentLength);
            }
            if (segmentLengths.Count < 1)
            {
                MessageBox.Show("Selected time length is too  short.", "Error", MessageBoxButton.OK);
                isSuccess = false;
                return segmentLengths;
            }
            segmentLengths.Reverse();
            return segmentLengths;
        }

        virtual protected void MakeInputFileForTRACMT()
        {
            bool isSuccess = false;
            int totalDataLength = 0;
            List<int> segmentLengths = DetermineSegmentLengths(out isSuccess, out totalDataLength);
            if (!isSuccess)
            {
                return;
            }
            if (String.IsNullOrEmpty(TextBoxWorkFolder.Text))
            {
                MessageBox.Show("No work folder is selected.", "Error", MessageBoxButton.OK);
                return;
            }
            var filePath = System.IO.Path.Combine(TextBoxWorkFolder.Text, "param.dat");
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("NUM_OUT");
                writer.WriteLine("2");
                writer.WriteLine("NUM_RR");
                writer.WriteLine(2);
                writer.WriteLine("SAMPLING_FREQ");
                writer.WriteLine(m_samplingFrequencyInt);
                writer.WriteLine("NUM_SECTION");
                writer.WriteLine(1);
                writer.WriteLine("SEGMENT");
                writer.WriteLine(segmentLengths.Count);
                foreach (var segmentLength in segmentLengths)
                {
                    if ((bool)CheckBoxPrewhitening.IsChecked)
                    {
                        writer.WriteLine(segmentLength.ToString() + " 2 3 4");
                    }
                    else
                    {
                        writer.WriteLine(segmentLength.ToString() + " 2 8 12");
                    }
                }
                if ((bool)CheckBoxDownsampling.IsChecked)
                {
                    writer.WriteLine("DECIMATION");
                    writer.WriteLine(Convert.ToInt32(TextBoxDownsamplingRate.Text));
                    writer.WriteLine(100);
                    writer.WriteLine(0.5);
                }
                writer.WriteLine("OVERLAP");
                writer.WriteLine("0.5");
                writer.WriteLine("AZIMUTH");
                double azimuth = double.Parse(TextBoxDecliniation.Text);
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine("ROTATION");
                writer.WriteLine(TextBoxRotation.Text);
                writer.WriteLine("ATS_BINARY");
                writer.WriteLine("MFS_CAL");
                writer.WriteLine(TextBoxDipoleLengthNS.Text);
                writer.WriteLine(TextBoxDipoleLengthEW.Text);
                if (String.IsNullOrEmpty(TextBoxCoilCalHx.Text))
                {
                    MessageBox.Show("Calibration file for Hx is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                if (String.IsNullOrEmpty(TextBoxCoilCalHy.Text))
                {
                    MessageBox.Show("Calibration file for Hy is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                writer.WriteLine(TextBoxCoilCalHx.Text);
                writer.WriteLine(TextBoxCoilCalHy.Text);
                writer.WriteLine(TextBoxCoilCalHx.Text);
                writer.WriteLine(TextBoxCoilCalHy.Text);
                writer.WriteLine("ELOGDUAL_BINARY");
                writer.WriteLine("ELOGDUAL_CAL");
                if (m_isELOG1K)
                {
                    writer.WriteLine(0);
                }
                else
                {
                    writer.WriteLine(1);
                }
                if (String.IsNullOrEmpty(TextBoxELOGCal.Text))
                {
                    MessageBox.Show("ELOG calibration file is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                writer.WriteLine(TextBoxELOGCal.Text);
                writer.WriteLine("5.17");
                writer.WriteLine("LOGGER_CAL_DIRECTORY");
                if (String.IsNullOrEmpty(TextBoxLoggerCalFilesFolder.Text))
                {
                    MessageBox.Show("Folder storing logger calibration files is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                writer.WriteLine(TextBoxLoggerCalFilesFolder.Text);
                writer.WriteLine("PROCEDURE");
                if ((bool)RadioButtonRRMS.IsChecked)
                {
                    MessageBox.Show("RRMS estimator cannot be used for single-site data.", "Error", MessageBoxButton.OK);
                    return;
                }
                else
                {
                    writer.WriteLine(0);
                    writer.WriteLine("MESTIMATORS");
                    if ((bool)RadioButtonOLS.IsChecked)
                    {
                        writer.WriteLine(-1);
                        writer.WriteLine(-1);
                    }
                    else if ((bool)RadioButtonMestimator.IsChecked)
                    {
                        writer.WriteLine(0);
                        writer.WriteLine(1);
                    }
                }
                writer.WriteLine("ERROR_ESTIMATION");
                writer.WriteLine(0);
                if ((bool)CheckBoxPrewhitening.IsChecked)
                {
                    writer.WriteLine("PREWHITENING");
                    writer.WriteLine(0);
                    writer.WriteLine(8);
                    writer.WriteLine(1);
                }
                if (AppliedFilter.getNumberOfHighPassFilters() > 0)
                {
                    writer.WriteLine("HIGH_PASS");
                    writer.WriteLine(AppliedFilter.getHighestCuttoffFrequencyOfHighPassFilters());
                }
                if (AppliedFilter.getNumberOfLowPassFilters() > 0)
                {
                    writer.WriteLine("LOW_PASS");
                    writer.WriteLine(AppliedFilter.getLowestCuttoffFrequencyOfLowPassFilters());
                }
                if (AppliedFilter.getNumberOfNotchFilters() > 0)
                {
                    writer.WriteLine("NOTCH");
                    writer.WriteLine(AppliedFilter.getNumberOfNotchFilters());
                    double[] cutoffFrequencies = AppliedFilter.getCuttoffFrequenciesOfNotchFilters();
                    foreach (double frequency in cutoffFrequencies)
                    {
                        writer.WriteLine(frequency);
                    }
                }
                writer.WriteLine("DATA_FILES");
                writer.WriteLine(totalDataLength);
                if (String.IsNullOrEmpty(m_directoryName[0]))
                {
                    MessageBox.Show("ELOG data folder is not specified.", "Error", MessageBoxButton.OK);
                    return;
                }
                for (int ch = 0; ch < 2; ch++)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], "*.dat"));
                    writer.WriteLine(m_startIndex[ch]);
                }
                for (int ch = 2; ch < 4; ch++)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]));
                    writer.WriteLine(m_startIndex[ch]);
                }
                for (int ch = 2; ch < 4; ch++)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]));
                    writer.WriteLine(m_startIndex[ch]);
                }
                writer.WriteLine("END");
            }
        }

        private void ButtonRunAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var currentDirectoryOrg = System.Environment.CurrentDirectory;
            if (String.IsNullOrEmpty(TextBoxWorkFolder.Text))
            {
                MessageBox.Show("No work folder is selected.", "Error", MessageBoxButton.OK);
                return;
            }
            WriteMessage("Start estimation.");
            var process = new System.Diagnostics.Process();
            process.StartInfo.WorkingDirectory = TextBoxWorkFolder.Text;
            process.StartInfo.FileName = "TRACMT.exe";
            process.StartInfo.Arguments = "-cout";
            process.Start();
            process.WaitForExit();
            WriteMessage("Estimation is finished.");
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", System.IO.Path.Combine(TextBoxWorkFolder.Text, "param.dat"));
        }

        private void ButtonPlot_Click(object sender, RoutedEventArgs e)
        {
            var filePath = System.IO.Path.Combine(TextBoxWorkFolder.Text, "response_functions.csv");
            if (!File.Exists(filePath))
            {
                MessageBox.Show("There is not result csv file.", "Error", MessageBoxButton.OK);
                return;
            }
            var apparentResistivityAndPhase = new PlotApparentResistivityAndPhaseFromTwoOutputAnalysis();
            if (apparentResistivityAndPhase.ReadCsvFile(filePath))
            {
                apparentResistivityAndPhase.Plot();
                apparentResistivityAndPhase.ShowDialog();
            }
        }

        private void TextBoxCoilCalHx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxCoilCalHx.Text", TextBoxCoilCalHx.Text);
        }

        private void TextBoxCoilCalHy_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxCoilCalHy.Text", TextBoxCoilCalHy.Text);
        }

        private void TextBoxWorkFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxWorkFolder.Text", TextBoxWorkFolder.Text);
        }

        private void TextBoxELOGCal_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGCal.Text", TextBoxELOGCal.Text);
        }

        private void TextBoxLoggerCalFilesFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxLoggerCalFilesFolder.Text", TextBoxLoggerCalFilesFolder.Text);
        }
    

        private void CheckBoxDownsampling_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxDownsamplingRate.IsEnabled = true;
            TextBoxDownsamplingRate.IsReadOnly = false;
        }

        private void CheckBoxDownsampling_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxDownsamplingRate.Clear();
            TextBoxDownsamplingRate.IsEnabled = false;
            TextBoxDownsamplingRate.IsReadOnly = true;
        }
        protected void WriteMessage(string message)
        {
            TextBlockMessage.Text = message;
        }
    }
}
