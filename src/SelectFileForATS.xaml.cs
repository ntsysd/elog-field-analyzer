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
    /// SelectFileForATS.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFileForATS : Window
    {
        public SelectFileForATS()
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
            TextBoxHrx.Background = Brushes.LightGray;
            string sbuf = Util.ReadFromSettingIni("TextBoxCh0.Text");
            if (sbuf != null)
            {
                TextBoxCh0.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxHrx.Text");
            if (sbuf != null)
            {
                TextBoxHrx.Text = sbuf;
            }
        }

        private void ReadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxCh0.Text))
            {
                MessageBox.Show("ats file is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            if ((bool)CheckBoxRR.IsChecked)
            {
                if (String.IsNullOrEmpty(TextBoxHrx.Text))
                {
                    MessageBox.Show("Name of Hrx file is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                PlotTimeSeriesATSRR plotTS = new PlotTimeSeriesATSRR();
                if (plotTS.ReadTimeSeries(TextBoxCh0.Text, TextBoxHrx.Text))
                {
                    if (plotTS.Plot())
                    {
                        plotTS.ShowDialog();
                    }
                }
            }
            else 
            {
                PlotTimeSeriesATS plotTS = new PlotTimeSeriesATS();
                if (plotTS.ReadTimeSeries(TextBoxCh0.Text))
                {
                    plotTS.Plot();
                    plotTS.ShowDialog();
                }
            }
        }
        private void ButtonCh0_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "ats file|*.ats|All (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxCh0.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxCh0.Text", TextBoxCh0.Text);
            }
        }
        private void ButtonHrx_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBoxRR.IsChecked) {
                var dialog = new OpenFileDialog();
                dialog.Filter = "ats file|*.ats|All (*.*)|*.*";
                if (dialog.ShowDialog() == true)
                {
                    TextBoxHrx.Text = dialog.FileName;
                    Util.WriteToSettingIni("TextBoxHrx.Text", TextBoxHrx.Text);
                }
            }
        }

        private void TextBoxCh0_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxCh0.Text", TextBoxCh0.Text);
        }

        private void TextBoxHrx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHrx.Text", TextBoxHrx.Text);
        }

        private void CheckBoxRR_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxHrx.IsEnabled = true;
            TextBoxHrx.IsReadOnly = false;
            TextBoxHrx.Background = Brushes.White;
        }

        private void CheckBoxRR_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxHrx.IsEnabled = false;
            TextBoxHrx.IsReadOnly = true;
            TextBoxHrx.Background = Brushes.LightGray;
        }
    }
}
