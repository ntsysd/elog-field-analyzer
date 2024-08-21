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
    /// SelectFileForELOGMTADU.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFileForELOGMTADU : Window
    {
        public SelectFileForELOGMTADU()
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
            TextBoxHrx.Background = Brushes.LightGray;
            string sbuf = Util.ReadFromSettingIni("TextBoxELOGMT.Text");
            if (sbuf != null)
            {
                TextBoxELOGMT.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxHrx.Text");
            if (sbuf != null)
            {
                TextBoxHrx.Text = sbuf;
            }
        }

        private void ReadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxELOGMT.Text))
            {
                MessageBox.Show("Data folder of ELOG-MT is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            int samplingFrequency = 32;
            if ((bool)RadioBotton1024Hz.IsChecked)
            {
                samplingFrequency = 1024;
            }
            if ((bool)CheckBoxRR.IsChecked)
            {
                if (String.IsNullOrEmpty(TextBoxHrx.Text))
                {
                    MessageBox.Show("Name of Hrx file is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                PlotTimeSeriesELOGMTADURR plotTS = new PlotTimeSeriesELOGMTADURR(samplingFrequency);
                if (plotTS.ReadTimeSeries(TextBoxELOGMT.Text, TextBoxHrx.Text))
                {
                    if (plotTS.Plot())
                    {
                        plotTS.ShowDialog();
                    }
                }
            }
            else
            {
                PlotTimeSeriesELOGMTADU plotTS = new PlotTimeSeriesELOGMTADU(samplingFrequency);
                if (plotTS.ReadTimeSeries(TextBoxELOGMT.Text))
                {
                    if (plotTS.Plot())
                    {
                        plotTS.ShowDialog();
                    }
                }
            }
        }

        private void ButtonELOGMT_Click(object sender, RoutedEventArgs e)
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

        private void TextBoxELOGMT_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGMT.Text", TextBoxELOGMT.Text);
        }

        private void TextBoxHrx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHrx.Text", TextBoxHrx.Text);
        }

        private void CheckBoxRR_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxHrx.Background = Brushes.White;
            TextBoxHrx.IsEnabled = true;
            TextBoxHrx.IsReadOnly = false;
        }

        private void CheckBoxRR_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxHrx.Background = Brushes.LightGray;
            TextBoxHrx.IsEnabled = false;
            TextBoxHrx.IsReadOnly = true;
        }
    }
}
