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
    /// SelectFileForELOGMTPHX.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFileForELOGMTPHX : Window
    {
        public SelectFileForELOGMTPHX()
        {
            InitializeComponent();
            TextBoxELOGMTRR.Background = Brushes.LightGray;
            Title += "   " + Common.VERSION;
            string sbuf = Util.ReadFromSettingIni("TextBoxELOGMT.Text");
            if (sbuf != null)
            {
                TextBoxELOGMT.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxELOGMTRR.Text");
            if (sbuf != null)
            {
                TextBoxELOGMTRR.Text = sbuf;
            }
        }

        private void ReadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxELOGMT.Text))
            {
                MessageBox.Show("Data folder for reference data is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            int samplingFrequency = 15;
            if ((bool)RadioBotton2400Hz.IsChecked)
            {
                samplingFrequency = 204;
            }
            else if ((bool)RadioBotton150Hz.IsChecked)
            {
                samplingFrequency = 150;
            }
            if ((bool)CheckBoxRR.IsChecked)
            {
                if (String.IsNullOrEmpty(TextBoxELOGMTRR.Text))
                {
                    MessageBox.Show("Data folder for reference data is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                var plotTS = new PlotTimeSeriesELOGMTPHXRR(samplingFrequency);
                if (plotTS.ReadTimeSeries(TextBoxELOGMT.Text, TextBoxELOGMTRR.Text))
                {
                    if (plotTS.Plot())
                    {
                        plotTS.ShowDialog();
                    }
                }
            }
            else
            {
                var plotTS = new PlotTimeSeriesELOGMTPHX(samplingFrequency);
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

        private void ButtonELOGMTRR_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxRR.IsChecked)
            {
                using (var dialog = new Forms.FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        TextBoxELOGMTRR.Text = dialog.SelectedPath;
                        Util.WriteToSettingIni("TextBoxELOGMTRR.Text", TextBoxELOGMTRR.Text);
                    }
                }
            }
        }

        private void TextBoxELOGMT_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGMT.Text", TextBoxELOGMT.Text);
        }

        private void TextBoxELOGMTRR_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGMTRR.Text", TextBoxELOGMTRR.Text);
        }

        private void CheckBoxRR_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxELOGMTRR.Background = Brushes.White;
            TextBoxELOGMTRR.IsEnabled = true;
            TextBoxELOGMTRR.IsReadOnly = false;
            TextBoxELOGMTRR.IsEnabled = true;
            TextBoxELOGMTRR.IsReadOnly = false;
        }

        private void CheckBoxRR_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxELOGMTRR.Background = Brushes.LightGray;
            TextBoxELOGMTRR.IsEnabled = false;
            TextBoxELOGMTRR.IsReadOnly = true;
            TextBoxELOGMTRR.IsEnabled = false;
            TextBoxELOGMTRR.IsReadOnly = true;
        }
    }
}
