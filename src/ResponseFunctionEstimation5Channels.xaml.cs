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
    /// ResponseFunctionEstimation5Channels.xaml の相互作用ロジック
    /// </summary>
    public abstract partial class ResponseFunctionEstimation5Channels : Window
    {
        protected int m_numOfChannels = 5;
        protected string[] m_directoryName;
        protected string[] m_fileName;
        protected int m_samplingFrequency = 0;
        protected int[] m_startIndex;
        protected int[] m_endIndex;

        protected string m_calibrationFileForEx = "calib_ex.txt";
        protected string m_calibrationFileForEy = "calib_ey.txt";

        public ResponseFunctionEstimation5Channels(string[] directoryName, string[] fileName, int samplingFrequency, int[] startIndex, int[] endIndex)
        {
            m_directoryName = directoryName;
            m_fileName = fileName;
            m_samplingFrequency = samplingFrequency;
            m_startIndex = startIndex;
            m_endIndex = endIndex;
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
            sbuf = Util.ReadFromSettingIni("TextBoxCoilCalHz.Text");
            if (sbuf != null)
            {
                TextBoxCoilCalHz.Text = sbuf;
            }
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

        private void ButtonCoilCalHz_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "TXT file|*.TXT|All(*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxCoilCalHz.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxCoilCalHz.Text", TextBoxCoilCalHz.Text);
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

        abstract protected void MakeInputFileForTRACMT();

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
            if ((bool)RadioButtonImpedanceAndTipper.IsChecked)
            {
                var apparentResistivityAndPhase = new PlotApparentResistivityAndPhaseFromThreeOutputAnalysis();
                if (apparentResistivityAndPhase.ReadCsvFile(filePath))
                {
                    apparentResistivityAndPhase.Plot();
                    apparentResistivityAndPhase.Show();
                }
                var vmtf = new PlotVMTFFromThreeOutputAnalysis();
                if (vmtf.ReadCsvFile(filePath))
                {
                    vmtf.Plot();
                    vmtf.Show();
                }
            }
            else if ((bool)RadioButtonImpedance.IsChecked)
            {
                var apparentResistivityAndPhase = new PlotApparentResistivityAndPhaseFromTwoOutputAnalysis();
                if (apparentResistivityAndPhase.ReadCsvFile(filePath))
                {
                    apparentResistivityAndPhase.Plot();
                    apparentResistivityAndPhase.ShowDialog();
                }
            }
            else if ((bool)RadioButtonTipper.IsChecked)
            {
                var vmtf = new PlotVMTFFromOneOutputAnalysis();
                if (vmtf.ReadCsvFile(filePath))
                {
                    vmtf.Plot();
                    vmtf.ShowDialog();
                }
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

        private void TextBoxCoilCalHz_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxCoilCalHz.Text", TextBoxCoilCalHz.Text);
        }

        private void TextBoxWorkFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxWorkFolder.Text", TextBoxWorkFolder.Text);
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

        protected void MakeCalibrationFilesForExAndEy()
        {
            using (var writer = new StreamWriter(System.IO.Path.Combine(TextBoxWorkFolder.Text, m_calibrationFileForEx)))
            {
                writer.WriteLine(TextBoxDipoleLengthNS.Text);
                writer.WriteLine(0);
            }
            using (var writer = new StreamWriter(System.IO.Path.Combine(TextBoxWorkFolder.Text, m_calibrationFileForEy)))
            {
                writer.WriteLine(TextBoxDipoleLengthEW.Text);
                writer.WriteLine(0);
            }
        }
        protected void WriteMessage(string message)
        {
            TextBlockMessage.Text = message;
        }
    }
}
