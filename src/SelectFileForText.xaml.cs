using Microsoft.Win32;
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
    /// SelectFileForText.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFileForText : Window
    {
        public SelectFileForText()
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
            string sbuf = Util.ReadFromSettingIni("TextBoxEx.Text");
            if (sbuf != null)
            {
                TextBoxEx.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxEy.Text");
            if (sbuf != null)
            {
                TextBoxEy.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxHx.Text");
            if (sbuf != null)
            {
                TextBoxHx.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxHy.Text");
            if (sbuf != null)
            {
                TextBoxHy.Text = sbuf;
            }
            sbuf = Util.ReadFromSettingIni("TextBoxHz.Text");
            if (sbuf != null)
            {
                TextBoxHz.Text = sbuf;
            }
        }

        private void ReadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxEx.Text))
            {
                MessageBox.Show("File for Ex is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            if (String.IsNullOrEmpty(TextBoxEy.Text))
            {
                MessageBox.Show("File for Ey is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            if (String.IsNullOrEmpty(TextBoxHx.Text))
            {
                MessageBox.Show("File for Hx is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            if (String.IsNullOrEmpty(TextBoxHy.Text))
            {
                MessageBox.Show("File for Hy is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            if (String.IsNullOrEmpty(TextBoxHz.Text))
            {
                MessageBox.Show("File for Hz is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            string[] fileNameFull = new string[5];
            fileNameFull[0] = TextBoxEx.Text;
            fileNameFull[1] = TextBoxEy.Text;
            fileNameFull[2] = TextBoxHx.Text;
            fileNameFull[3] = TextBoxHy.Text;
            fileNameFull[4] = TextBoxHz.Text;
            double samplingFrequency = 0;
            if (!double.TryParse(TextBoxSamplingFrequency.Text, out samplingFrequency))
            {
                MessageBox.Show("Sampling frequency("+ TextBoxSamplingFrequency.Text+") cannot be converted to a real number.", "Error", MessageBoxButton.OK);
                return;
            }
            int milliSecond = 0;
            if (!Int32.TryParse(TextBoxMilliSecond.Text, out milliSecond))
            {
                MessageBox.Show("Inputted millisecond("+ TextBoxMilliSecond.Text + ") cannot be converted to a real number.", "Error", MessageBoxButton.OK);
                return;
            }
            DateTime startDateTime = new DateTime(Convert.ToInt32(ComboBoxTimeSpanYear.Text), Convert.ToInt32(ComboBoxTimeSpanMonth.Text), Convert.ToInt32(ComboBoxTimeSpanDay.Text),
                Convert.ToInt32(ComboBoxTimeSpanHour.Text),Convert.ToInt32(ComboBoxTimeSpanMinute.Text), Convert.ToInt32(ComboBoxTimeSpanSecond.Text), milliSecond);
            PlotTimeSeriesText plotTS = new PlotTimeSeriesText(samplingFrequency, startDateTime);
            if (plotTS.ReadTimeSeries(fileNameFull))
            {
                if (plotTS.Plot())
                {
                    plotTS.ShowDialog();
                }
            }
        }

        private void ButtonEx_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt file|*.txt|All (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxEx.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxEx.Text", TextBoxEx.Text);
            }
        }

        private void ButtonEy_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt file|*.txt|All (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxEy.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxEy.Text", TextBoxEy.Text);
            }
        }

        private void ButtonHx_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt file|*.txt|All (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxHx.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxHx.Text", TextBoxHx.Text);
            }
        }

        private void ButtonHy_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt file|*.txt|All (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxHy.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxHy.Text", TextBoxHy.Text);
            }
        }

        private void ButtonHz_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt file|*.txt|All (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                TextBoxHz.Text = dialog.FileName;
                Util.WriteToSettingIni("TextBoxHz.Text", TextBoxHz.Text);
            }
        }


        private void TextBoxEx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxEx.Text", TextBoxEx.Text);
        }

        private void TextBoxEy_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxEy.Text", TextBoxEy.Text);
        }

        private void TextBoxHx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHx.Text", TextBoxHx.Text);
        }

        private void TextBoxHy_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHy.Text", TextBoxHy.Text);
        }

        private void TextBoxHz_TextChanged(object sender, TextChangedEventArgs e)
        {
            Util.WriteToSettingIni("TextBoxHz.Text", TextBoxHz.Text);
        }
    }
}
