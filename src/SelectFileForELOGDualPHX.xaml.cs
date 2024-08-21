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
    /// SelectFileForELOGDualPHX.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFileForELOGDualPHX : Window
    {
        public SelectFileForELOGDualPHX()
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
            TextBoxELOGMT.Background = Brushes.LightGray;
            TextBoxELOGMTRR.Background = Brushes.LightGray;
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
            sbuf = Util.ReadFromSettingIni("TextBoxELOGMTRR.Text");
            if (sbuf != null)
            {
                TextBoxELOGMTRR.Text = sbuf;
            }
        }

        private void ReadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxELOGDual.Text))
            {
                MessageBox.Show("Data folder of ELOG-DUAL is not selected.", "Error", MessageBoxButton.OK);
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
            if ((bool)CheckBoxMagField.IsChecked)
            {
                if (String.IsNullOrEmpty(TextBoxELOGMT.Text))
                {
                    MessageBox.Show("Data folder of ELOG-MT (for Hx, Hy) is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                if ((bool)CheckBoxRR.IsChecked)
                {
                    if (String.IsNullOrEmpty(TextBoxELOGMTRR.Text))
                    {
                        MessageBox.Show("Data folder of ELOG-MT (for Hrx, Hry) is not selected.", "Error", MessageBoxButton.OK);
                        return;
                    }
                    var plotTS = new PlotTimeSeriesELOGDualPHXRR(samplingFrequency);
                    if (plotTS.ReadTimeSeries(TextBoxELOGDual.Text, TextBoxELOGMT.Text, TextBoxELOGMTRR.Text))
                    {
                        if (plotTS.Plot())
                        {
                            plotTS.ShowDialog();
                        }
                    }
                }
                else
                {
                    var plotTS = new PlotTimeSeriesELOGDual(samplingFrequency, false);
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

        private void ButtonELOGMTRR_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxMagField.IsChecked)
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


        private void TextBoxELOGDual_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGDual.Text", TextBoxELOGDual.Text);
        }

        private void TextBoxELOGMT_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGMT.Text", TextBoxELOGMT.Text);
        }

        private void TextBoxELOGMTRR_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxELOGMTRR.Text", TextBoxELOGMTRR.Text);
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
            TextBoxELOGMTRR.IsEnabled = false;
            TextBoxELOGMTRR.IsReadOnly = true;
        }

        private void CheckBoxRR_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxMagField.IsChecked)
            {
                TextBoxELOGMTRR.Background = Brushes.White;
                TextBoxELOGMTRR.IsEnabled = true;
                TextBoxELOGMTRR.IsReadOnly = false;
            }
        }

        private void CheckBoxRR_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxELOGMTRR.Background = Brushes.LightGray;
            TextBoxELOGMTRR.IsEnabled = false;
            TextBoxELOGMTRR.IsReadOnly = true;
        }
    }
}
