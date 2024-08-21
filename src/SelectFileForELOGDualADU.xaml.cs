using Microsoft.Win32;
using Forms = System.Windows.Forms;
using System;
using System.Collections.Generic;
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
    /// SelectFileForELOGDualADU.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFileForELOGDualADU : Window
    {
        private bool m_isELOG1K = false;

        public SelectFileForELOGDualADU(bool isELOG1K)
        {
            InitializeComponent();
            if (isELOG1K)
            {
                TextBlockELOGDual.Text = "ELOG1K";
            }
            TextBoxELOGMT.Background = Brushes.LightGray;
            TextBoxHrx.Background = Brushes.LightGray;
            Title += "   " + Common.VERSION;
            string sbuf = Util.ReadFromSettingIni("TextBoxELOGDual.Text");
            if (sbuf != null)
            {
                TextBoxELOGDual.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxELOGMT.Text");
            if (sbuf != null)
            {
                TextBoxELOGMT.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxHrx.Text");
            if (sbuf != null)
            {
                TextBoxHrx.Text = sbuf;
            }
            m_isELOG1K = isELOG1K;
        }

        private void ReadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxELOGDual.Text))
            {
                MessageBox.Show("Data folder of ELOG-DUAL is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            int samplingFrequency = 32;
            if ((bool)RadioBotton1024Hz.IsChecked)
            {
                samplingFrequency = 1024;
            }
            if ((bool)CheckBoxMagField.IsChecked)
            {
                if (String.IsNullOrEmpty(TextBoxELOGMT.Text))
                {
                    MessageBox.Show("Data folder of ELOG-MT is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                if ((bool)CheckBoxRR.IsChecked)
                {
                    if (String.IsNullOrEmpty(TextBoxHrx.Text))
                    {
                        MessageBox.Show("Name of Hrx file is not selected.", "Error", MessageBoxButton.OK);
                        return;
                    }
                    var plotTS = new PlotTimeSeriesELOGDualADURR(samplingFrequency);
                    if (plotTS.ReadTimeSeries(TextBoxELOGDual.Text, TextBoxELOGMT.Text, TextBoxHrx.Text))
                    {
                        if (plotTS.Plot())
                        {
                            plotTS.ShowDialog();
                        }
                    }
                }
                else
                {
                    var plotTS = new PlotTimeSeriesELOGDual(samplingFrequency, true);
                    if (plotTS.ReadTimeSeries(TextBoxELOGDual.Text, TextBoxELOGMT.Text))
                    {
                        if (plotTS.Plot())
                        {
                            plotTS.ShowDialog();
                        }
                    }
                }
            }
            else
            {
                var plotTS = new PlotTimeSeriesELOGDual2E(samplingFrequency);
                if (plotTS.ReadTimeSeries(TextBoxELOGDual.Text))
                {
                    if (plotTS.Plot())
                    {
                        plotTS.ShowDialog();
                    }
                }
            }
        }

        private void ButtonELOGDual_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxELOGDual.Text = dialog.SelectedPath;
                    Util.WriteToSettingIni("TextBoxELOGDual.Text", TextBoxELOGDual.Text);
                }
            }
        }

        private void ButtonELOGMT_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxMagField.IsChecked)
            {
                using (var dialog = new Forms.FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        TextBoxELOGMT.Text = dialog.SelectedPath;
                        Util.WriteToSettingIni("TextBoxELOGMT.Text", TextBoxELOGMT.Text);
                    }
                }
            }
        }


        private void ButtonHrx_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxRR.IsChecked)
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "ats file|*.ats|All (*.*)|*.*";
                if (dialog.ShowDialog() == true)
                {
                    TextBoxHrx.Text = dialog.FileName;
                    Util.WriteToSettingIni("TextBoxHrx.Text", TextBoxHrx.Text);
                }
            }
        }

        private void TextBoxELOGDual_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGDual.Text", TextBoxELOGDual.Text);
        }

        private void TextBoxELOGMT_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGMT.Text", TextBoxELOGMT.Text);
        }

        private void TextBoxHrx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHrx.Text", TextBoxHrx.Text);
        }

        private void CheckBoxMagField_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxELOGMT.Background = Brushes.White;
            TextBoxELOGMT.IsEnabled = true;
            TextBoxELOGMT.IsReadOnly = false;
            CheckBoxRR.IsEnabled = true;
        }

        private void CheckBoxMagField_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxELOGMT.Background = Brushes.LightGray;
            TextBoxELOGMT.IsEnabled = false;
            TextBoxELOGMT.IsReadOnly = true;
            CheckBoxRR.IsEnabled = false;
            CheckBoxRR.IsChecked = false;
            TextBoxHrx.IsEnabled = false;
            TextBoxHrx.IsReadOnly = true;
        }

        private void CheckBoxRR_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxMagField.IsChecked)
            {
                TextBoxHrx.Background = Brushes.White;
                TextBoxHrx.IsEnabled = true;
                TextBoxHrx.IsReadOnly = false;
            }
        }

        private void CheckBoxRR_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxHrx.Background = Brushes.LightGray;
            TextBoxHrx.IsEnabled = false;
            TextBoxHrx.IsReadOnly = true;
        }
    }
}
