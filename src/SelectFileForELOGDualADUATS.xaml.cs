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
    /// SelectFileForELOG1KATS.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFileForELOGDualADUATS : Window
    {
        private bool m_isELOG1K = false;

        public SelectFileForELOGDualADUATS(bool isELOG1K)
        {
            InitializeComponent();
            if (isELOG1K)
            {
                TextBlockELOGDual.Text = "ELOG1K";
            }
            Title += "   " + Common.VERSION;
            TextBoxHx.Background = Brushes.LightGray;
            TextBoxHrx.Background = Brushes.LightGray;
            string sbuf = Util.ReadFromSettingIni("TextBoxELOGDual.Text");
            if (sbuf != null)
            {
                TextBoxELOGDual.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxHx.Text");
            if (sbuf != null)
            {
                TextBoxHx.Text = sbuf;
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
                MessageBox.Show("Data folder of ELOG1K is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            int samplingFrequency = 32;
            if ((bool)RadioBotton1024Hz.IsChecked)
            {
                samplingFrequency = 1024;
            }
            if ((bool)CheckBoxMagField.IsChecked)
            {
                if (String.IsNullOrEmpty(TextBoxHx.Text))
                {
                    MessageBox.Show("Name of Hx file is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                if ((bool)CheckBoxRR.IsChecked)
                {
                    if (String.IsNullOrEmpty(TextBoxHrx.Text))
                    {
                        MessageBox.Show("Name of Hrx file is not selected.", "Error", MessageBoxButton.OK);
                        return;
                    }
                    PlotTimeSeriesELOGDualAndATSRR plotTS = new PlotTimeSeriesELOGDualAndATSRR(samplingFrequency, m_isELOG1K);
                    if (plotTS.ReadTimeSeries(TextBoxELOGDual.Text, TextBoxHx.Text, TextBoxHrx.Text))
                    {
                        if (plotTS.Plot())
                        {
                            plotTS.ShowDialog();
                        }
                    }
                }
                else
                {
                    PlotTimeSeriesELOGDualAndATS plotTS = new PlotTimeSeriesELOGDualAndATS(samplingFrequency, m_isELOG1K);
                    if (plotTS.ReadTimeSeries(TextBoxELOGDual.Text, TextBoxHx.Text))
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

        private void ButtonELOG_Click(object sender, RoutedEventArgs e)
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

        private void ButtonHx_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxMagField.IsChecked)
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "ats file|*.ats|All (*.*)|*.*";
                if (dialog.ShowDialog() == true)
                {
                    TextBoxHx.Text = dialog.FileName;
                    Util.WriteToSettingIni("TextBoxHx.Text", TextBoxHx.Text);
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

        private void TextBoxELOG_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGDual.Text", TextBoxELOGDual.Text);
        }

        private void TextBoxHx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHx.Text", TextBoxHx.Text);
        }

        private void TextBoxHrx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHrx.Text", TextBoxHrx.Text);
        }

        private void CheckBoxMagField_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxHx.Background = Brushes.White;
            TextBoxHx.IsEnabled = true;
            TextBoxHx.IsReadOnly = false;
            CheckBoxRR.IsEnabled = true;
        }

        private void CheckBoxMagField_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxHx.Background = Brushes.LightGray;
            TextBoxHx.IsEnabled = false;
            TextBoxHx.IsReadOnly = true;
            CheckBoxRR.IsEnabled = false;
            CheckBoxRR.IsChecked = false;
            TextBoxHrx.IsEnabled = false;
            TextBoxHrx.IsReadOnly = true;
        }

        private void CheckBoxRR_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxMagField.IsChecked) {
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
