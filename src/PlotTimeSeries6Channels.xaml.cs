using ScottPlot;
using ScottPlot.Plottable;
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
using static PlotTimeSeries.AppliedFilter;

namespace PlotTimeSeries
{
    /// <summary>
    /// PlotTimeSeries6Channels.xaml の相互作用ロジック
    /// </summary>
    public abstract partial class PlotTimeSeries6Channels : Window
    {
        protected static int m_numOfLocalChannels = 4;
        protected static int m_numOfRRChannels = 2;
        protected static int m_numOfChannels = m_numOfLocalChannels + m_numOfRRChannels;
        protected bool m_plotted = false;
        protected double m_samplingFrequency = 0;
        protected DateTime[] m_startDateTime;
        protected TimeSpan[] m_timeSpan;
        protected double[][] m_values;
        protected SignalPlot[] m_signalPlot;
        protected int[] m_startIndex;
        protected int[] m_endIndex;
        protected int[] m_startIndexIncludingNegative;
        protected string[] m_directoryName;
        protected string[] m_fileName;
        protected string[] m_fileNameFullRR;
        protected string m_title;

        public PlotTimeSeries6Channels()
        {
            m_startDateTime = new DateTime[m_numOfChannels];
            m_timeSpan = new TimeSpan[m_numOfChannels];
            m_values = new double[m_numOfChannels][];
            m_signalPlot = new SignalPlot[m_numOfChannels];
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                m_signalPlot[ch] = null;
            }
            m_startIndex = new int[m_numOfChannels];
            m_endIndex = new int[m_numOfChannels];
            m_startIndexIncludingNegative = new int[m_numOfChannels];
            m_directoryName = new string[m_numOfChannels];
            m_fileName = new string[m_numOfChannels];
            m_fileNameFullRR = new string[m_numOfRRChannels];
            AppliedFilter.ClearDigitalFilters();
            InitializeComponent();
            m_title = Title + "   " + Common.VERSION;
            Title = m_title;
        }

        public bool Plot()
        {
            if (m_plotted)
            {
                if (m_signalPlot[0] != null)
                {
                    TimeSeriesCh0.Plot.Remove(m_signalPlot[0]);
                }
                if (m_signalPlot[1] != null)
                {
                    TimeSeriesCh1.Plot.Remove(m_signalPlot[1]);
                }
                if (m_signalPlot[2] != null)
                {
                    TimeSeriesCh2.Plot.Remove(m_signalPlot[2]);
                }
                if (m_signalPlot[3] != null)
                {
                    TimeSeriesCh3.Plot.Remove(m_signalPlot[3]);
                }
                if (m_signalPlot[4] != null)
                {
                    TimeSeriesCh4.Plot.Remove(m_signalPlot[4]);
                }
                if (m_signalPlot[5] != null)
                {
                    TimeSeriesCh5.Plot.Remove(m_signalPlot[5]);
                }
            }
            DateTime startDateMax = m_startDateTime[0];
            DateTime endDateMin = m_startDateTime[0] + m_timeSpan[0];
            for (int ch = 1; ch < m_numOfChannels; ++ch)
            {
                DateTime st = m_startDateTime[ch];
                if (st > startDateMax)
                {
                    startDateMax = st;
                }
                DateTime ed = m_startDateTime[ch] + m_timeSpan[ch];
                if (ed < endDateMin)
                {
                    endDateMin = ed;
                }
            }
            if (startDateMax > endDateMin)
            {
                MessageBox.Show("There is no common period for all channels.", "Error", MessageBoxButton.OK);
                this.Close();
                return false;
            }
            double startDate = startDateMax.ToOADate();
            double endDate = endDateMin.ToOADate();
            if (!(bool)RadioButtonFree.IsChecked)
            {
                TimeSpan span;
                if ((bool)RadioButtonFixTimeSpan.IsChecked)
                {
                    span = new TimeSpan(0, Convert.ToInt32(ComboBoxTimeSpanHour.Text), Convert.ToInt32(ComboBoxTimeSpanMinute.Text), Convert.ToInt32(ComboBoxTimeSpanSecond.Text), Convert.ToInt32(TextBoxMilliSecond.Text));
                    if (span == TimeSpan.Zero)
                    {
                        MessageBox.Show("Zero time interval.", "Error", MessageBoxButton.OK);
                        return false;
                    }
                }
                else
                {
                    double numOfData = Convert.ToDouble(ComboBoxDataNumber.Text);
                    if ((int)numOfData < 1)
                    {
                        MessageBox.Show("Number of data is zero.", "Error", MessageBoxButton.OK);
                        return false;
                    }
                    int numOfDataMilliSeconds = (int)(numOfData * 1000 / m_samplingFrequency);
                    span = new TimeSpan(0, 0, 0, 0, numOfDataMilliSeconds);
                }
                double value = SliderForStartTime.Value;
                double start = startDateMax.ToOADate();
                double end = endDateMin.ToOADate();
                double shiftDays = (end - start) * value / 100.0;
                startDate = startDateMax.ToOADate() + shiftDays;
                endDate = (startDateMax + span).ToOADate() + shiftDays;
                DateTime stateDateUpperLimit = endDateMin - span;
                if (startDate > stateDateUpperLimit.ToOADate())
                {
                    startDate = stateDateUpperLimit.ToOADate();
                    endDate = endDateMin.ToOADate();
                }
            }
            double [] offsetX = new double[m_numOfChannels];
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                int startIndex = (int)Math.Ceiling((startDate - m_startDateTime[ch].ToOADate()) * Util.DAYS_TO_SECONDS * m_samplingFrequency);
                m_startIndexIncludingNegative[ch] = startIndex;
                offsetX[ch] = startDate;
                if (startIndex < 0)
                {
                    startIndex = 0;
                    offsetX[ch] = m_startDateTime[ch].ToOADate();
                }
                if (startIndex > m_values[ch].Length)
                {
                    startIndex = m_values[ch].Length;
                }
                m_startIndex[ch] = startIndex;
                int endIndex = (int)Math.Ceiling((endDate - m_startDateTime[ch].ToOADate()) * Util.DAYS_TO_SECONDS * m_samplingFrequency);
                if (endIndex < 0)
                {
                    endIndex = 0;
                }
                if (endIndex > m_values[ch].Length)
                {
                    endIndex = m_values[ch].Length;
                }
                m_endIndex[ch] = endIndex;
            }
            TextBlockStartDay.Text = DateTime.FromOADate(startDate).ToString("yyyy/MM/dd");
            TextBlockStartTime.Text = DateTime.FromOADate(startDate).ToString("HH:mm:ss.FFF");
            TextBlockEndDay.Text = DateTime.FromOADate(endDate).ToString("yyyy/MM/dd");
            TextBlockEndTime.Text = DateTime.FromOADate(endDate).ToString("HH:mm:ss.FFF");
            // Ch#0
            if (m_endIndex[0] > m_startIndex[0])
            {
                TimeSeriesCh0.Plot.XAxis.DateTimeFormat(true);
                TimeSeriesCh0.Plot.XAxis.SetBoundary(startDate, endDate);
                TimeSeriesCh0.Plot.XAxis.Ticks(false);
                TimeSeriesCh0.Plot.YAxis.Ticks(false);
                TextBlockCh0Min.Text = (m_values[0][m_startIndex[0]..m_endIndex[0]].Min()).ToString("G4");
                TextBlockCh0Max.Text = (m_values[0][m_startIndex[0]..m_endIndex[0]].Max()).ToString("G4");
                m_signalPlot[0] = TimeSeriesCh0.Plot.AddSignal(m_values[0][m_startIndex[0]..m_endIndex[0]], sampleRate: Util.DAYS_TO_SECONDS * m_samplingFrequency);
                m_signalPlot[0].MarkerSize = 0;
                m_signalPlot[0].LineWidth = 1;
                m_signalPlot[0].OffsetX = offsetX[0];
                TimeSeriesCh0.Configuration.DoubleClickBenchmark = false;
                TimeSeriesCh0.Refresh();
            }
            else
            {
                m_signalPlot[0] = null;
            }
            // Ch#1
            if (m_endIndex[1] > m_startIndex[1])
            {
                TimeSeriesCh1.Plot.XAxis.DateTimeFormat(true);
                TimeSeriesCh1.Plot.XAxis.SetBoundary(startDate, endDate);
                TimeSeriesCh1.Plot.XAxis.Ticks(false);
                TimeSeriesCh1.Plot.YAxis.Ticks(false);
                TextBlockCh1Min.Text = (m_values[1][m_startIndex[1]..m_endIndex[1]].Min()).ToString("G4");
                TextBlockCh1Max.Text = (m_values[1][m_startIndex[1]..m_endIndex[1]].Max()).ToString("G4");
                m_signalPlot[1] = TimeSeriesCh1.Plot.AddSignal(m_values[1][m_startIndex[1]..m_endIndex[1]], sampleRate: Util.DAYS_TO_SECONDS * m_samplingFrequency);
                m_signalPlot[1].MarkerSize = 0;
                m_signalPlot[1].LineWidth = 1;
                m_signalPlot[1].OffsetX = offsetX[1];
                TimeSeriesCh1.Configuration.DoubleClickBenchmark = false;
                TimeSeriesCh1.Refresh();
            }
            else
            {
                m_signalPlot[1] = null;
            }
            // Ch#2
            if (m_endIndex[2] > m_startIndex[2])
            {
                TimeSeriesCh2.Plot.XAxis.DateTimeFormat(true);
                TimeSeriesCh2.Plot.XAxis.SetBoundary(startDate, endDate);
                TimeSeriesCh2.Plot.XAxis.Ticks(false);
                TimeSeriesCh2.Plot.YAxis.Ticks(false);
                TextBlockCh2Min.Text = (m_values[2][m_startIndex[2]..m_endIndex[2]].Min()).ToString("G4");
                TextBlockCh2Max.Text = (m_values[2][m_startIndex[2]..m_endIndex[2]].Max()).ToString("G4");
                m_signalPlot[2] = TimeSeriesCh2.Plot.AddSignal(m_values[2][m_startIndex[2]..m_endIndex[2]], sampleRate: Util.DAYS_TO_SECONDS * m_samplingFrequency);
                m_signalPlot[2].MarkerSize = 0;
                m_signalPlot[2].LineWidth = 1;
                m_signalPlot[2].OffsetX = offsetX[2];
                TimeSeriesCh2.Configuration.DoubleClickBenchmark = false;
                TimeSeriesCh2.Refresh();
            }
            else
            {
                m_signalPlot[2] = null;
            }
            // Ch#3
            if (m_endIndex[3] > m_startIndex[3])
            {
                TimeSeriesCh3.Plot.XAxis.DateTimeFormat(true);
                TimeSeriesCh3.Plot.XAxis.SetBoundary(startDate, endDate);
                TimeSeriesCh3.Plot.XAxis.Ticks(false);
                TimeSeriesCh3.Plot.XAxis.Ticks(false);
                TimeSeriesCh3.Plot.YAxis.Ticks(false);
                TextBlockCh3Min.Text = (m_values[3][m_startIndex[3]..m_endIndex[3]].Min()).ToString("G4");
                TextBlockCh3Max.Text = (m_values[3][m_startIndex[3]..m_endIndex[3]].Max()).ToString("G4");
                m_signalPlot[3] = TimeSeriesCh3.Plot.AddSignal(m_values[3][m_startIndex[3]..m_endIndex[3]], sampleRate: Util.DAYS_TO_SECONDS * m_samplingFrequency);
                m_signalPlot[3].MarkerSize = 0;
                m_signalPlot[3].LineWidth = 1;
                m_signalPlot[3].OffsetX = offsetX[3];
                TimeSeriesCh3.Configuration.DoubleClickBenchmark = false;
                TimeSeriesCh3.Refresh();
            }
            else
            {
                m_signalPlot[3] = null;
            }
            // Ch#4
            if (m_endIndex[4] > m_startIndex[4])
            {
                TimeSeriesCh4.Plot.XAxis.DateTimeFormat(true);
                TimeSeriesCh4.Plot.XAxis.SetBoundary(startDate, endDate);
                TimeSeriesCh4.Plot.XAxis.Ticks(false);
                TimeSeriesCh4.Plot.XAxis.Ticks(false);
                TimeSeriesCh4.Plot.YAxis.Ticks(false);
                TextBlockCh4Min.Text = (m_values[4][m_startIndex[4]..m_endIndex[4]].Min()).ToString("G4");
                TextBlockCh4Max.Text = (m_values[4][m_startIndex[4]..m_endIndex[4]].Max()).ToString("G4");
                m_signalPlot[4] = TimeSeriesCh4.Plot.AddSignal(m_values[4][m_startIndex[4]..m_endIndex[4]], sampleRate: Util.DAYS_TO_SECONDS * m_samplingFrequency);
                m_signalPlot[4].MarkerSize = 0;
                m_signalPlot[4].LineWidth = 1;
                m_signalPlot[4].OffsetX = offsetX[4];
                TimeSeriesCh4.Configuration.DoubleClickBenchmark = false;
                TimeSeriesCh4.Refresh();
            }
            else
            {
                m_signalPlot[4] = null;
            }
            // Ch#5
            if (m_endIndex[5] > m_startIndex[5])
            {
                TimeSeriesCh5.Plot.XAxis.DateTimeFormat(true);
                TimeSeriesCh5.Plot.XAxis.SetBoundary(startDate, endDate);
                TimeSeriesCh5.Plot.XAxis.Ticks(false);
                TimeSeriesCh5.Plot.XAxis.Ticks(false);
                TimeSeriesCh5.Plot.YAxis.Ticks(false);
                TextBlockCh5Min.Text = (m_values[5][m_startIndex[5]..m_endIndex[5]].Min()).ToString("G4");
                TextBlockCh5Max.Text = (m_values[5][m_startIndex[5]..m_endIndex[5]].Max()).ToString("G4");
                m_signalPlot[5] = TimeSeriesCh5.Plot.AddSignal(m_values[5][m_startIndex[5]..m_endIndex[5]], sampleRate: Util.DAYS_TO_SECONDS * m_samplingFrequency);
                m_signalPlot[5].MarkerSize = 0;
                m_signalPlot[5].LineWidth = 1;
                m_signalPlot[5].OffsetX = offsetX[5];
                TimeSeriesCh5.Configuration.DoubleClickBenchmark = false;
                TimeSeriesCh5.Refresh();
            }
            else
            {
                m_signalPlot[5] = null;
            }
            // Save parameters
            m_plotted = true;
            return true;
        }

        private void ComboBoxTimeSpanHour_DropDownClosed(object sender, EventArgs e)
        {
            if (m_plotted)
            {
                if (!(bool)RadioButtonFixTimeSpan.IsChecked)
                {
                    RadioButtonFixTimeSpan.IsChecked = true;
                }
                Plot();
            }
        }

        private void ComboBoxTimeSpanMinute_DropDownClosed(object sender, EventArgs e)
        {
            if (m_plotted)
            {
                if (!(bool)RadioButtonFixTimeSpan.IsChecked)
                {
                    RadioButtonFixTimeSpan.IsChecked = true;
                }
                Plot();
            }
        }

        private void ComboBoxTimeSpanSecond_DropDownClosed(object sender, EventArgs e)
        {
            if (m_plotted)
            {
                if (!(bool)RadioButtonFixTimeSpan.IsChecked)
                {
                    RadioButtonFixTimeSpan.IsChecked = true;
                }
                Plot();
            }
        }

        private void SliderForStartTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_plotted)
            {
                if ((bool)RadioButtonFixTimeSpan.IsChecked || (bool)RadioButtonFixDataNumber.IsChecked)
                {
                    Plot();
                }
            }
        }

        private void TextBoxMilliSecond_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_plotted)
            {
                if (!(bool)RadioButtonFixTimeSpan.IsChecked)
                {
                    RadioButtonFixTimeSpan.IsChecked = true;
                }
                Plot();
            }
        }

        private void ButtonPlotPowerSpectrum_Click(object sender, RoutedEventArgs e)
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
            PlotPowerSpectrum6Channels plotPowerSpectrum = new PlotPowerSpectrum6Channels(m_samplingFrequency);
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                plotPowerSpectrum.CaluculatePowerSpectrum(ch, m_values[ch][m_startIndex[ch]..m_endIndex[ch]]);
            }
            plotPowerSpectrum.Plot();
            plotPowerSpectrum.ShowDialog();
        }

        private void ButtonIncreaseTime_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)RadioButtonFixTimeSpan.IsChecked && !(bool)RadioButtonFixDataNumber.IsChecked)
            {
                MessageBox.Show("You cannot use this button when 'Type' is 'All'. ", "Error", MessageBoxButton.OK);
                return;
            }
            double value = SliderForStartTime.Value;
            TimeSpan span = new TimeSpan(0, Convert.ToInt32(ComboBoxTimeSpanHour.Text), Convert.ToInt32(ComboBoxTimeSpanMinute.Text), Convert.ToInt32(ComboBoxTimeSpanSecond.Text), Convert.ToInt32(TextBoxMilliSecond.Text));
            if ((bool)RadioButtonFixDataNumber.IsChecked)
            {
                double numOfData = Convert.ToDouble(ComboBoxDataNumber.Text);
                int numOfDataMilliSeconds = (int)(numOfData * 1000 / m_samplingFrequency);
                span = new TimeSpan(0, 0, 0, 0, numOfDataMilliSeconds);
            }
            double start = m_startDateTime[0].ToOADate();
            double end = (m_startDateTime[0] + m_timeSpan[0]).ToOADate();
            value += span.TotalDays / (end - start) * 100;
            if (value > 100.0)
            {
                value = 100.0;
            }
            SliderForStartTime.Value = value;
            Plot();
        }

        private void ButtonDecreaseTime_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)RadioButtonFixTimeSpan.IsChecked && !(bool)RadioButtonFixDataNumber.IsChecked)
            {
                MessageBox.Show("You cannot use this button when 'Type' is 'All'. ", "Error", MessageBoxButton.OK);
                return;
            }
            double value = SliderForStartTime.Value;
            TimeSpan span = new TimeSpan(0, Convert.ToInt32(ComboBoxTimeSpanHour.Text), Convert.ToInt32(ComboBoxTimeSpanMinute.Text), Convert.ToInt32(ComboBoxTimeSpanSecond.Text), Convert.ToInt32(TextBoxMilliSecond.Text));
            if ((bool)RadioButtonFixDataNumber.IsChecked)
            {
                double numOfData = Convert.ToDouble(ComboBoxDataNumber.Text);
                int numOfDataMilliSeconds = (int)(numOfData * 1000 / m_samplingFrequency);
                span = new TimeSpan(0, 0, 0, 0, numOfDataMilliSeconds);
            }
            double start = m_startDateTime[0].ToOADate();
            double end = (m_startDateTime[0] + m_timeSpan[0]).ToOADate();
            value -= span.TotalDays / (end - start) * 100;
            if (value < 0.0)
            {
                value = 0.0;
            }
            SliderForStartTime.Value = value;
            Plot();
        }
        private void ComboBoxDataNumber_DropDownClosed(object sender, EventArgs e)
        {
            if (m_plotted)
            {
                if (!(bool)RadioButtonFixDataNumber.IsChecked)
                {
                    RadioButtonFixDataNumber.IsChecked = true;
                }
                Plot();
            }
        }
        private void RadioButtonFree_Checked(object sender, RoutedEventArgs e)
        {
            if (m_plotted)
            {
                Plot();
            }
        }
        private void RadioButtonFixDataNumber_Checked(object sender, RoutedEventArgs e)
        {
            if (m_plotted)
            {
                Plot();
            }
        }

        private void RadioButtonFixTimeSpan_Checked(object sender, RoutedEventArgs e)
        {
            if (m_plotted)
            {
                Plot();
            }
        }

        protected abstract void ButtonResponseFunctionEstimation_Click(object sender, RoutedEventArgs e);

        private void ButtonLPF_Click(object sender, RoutedEventArgs e)
        {
            double cutoffFrequency = 0;
            if (String.IsNullOrEmpty(TextBoxLPF.Text))
            {
                return;
            }
            bool canConvert = double.TryParse(TextBoxLPF.Text, out cutoffFrequency);
            if (!canConvert)
            {
                MessageBox.Show(TextBoxLPF.Text + " cannot be recognized a number.", "Error", MessageBoxButton.OK);
                return;
            }
            double nyquistFrequency = m_samplingFrequency / 2;
            if (cutoffFrequency >= nyquistFrequency)
            {
                MessageBox.Show(TextBoxLPF.Text + " is higher than the Nyquist frequency(" + nyquistFrequency.ToString() + " Hz).", "Error", MessageBoxButton.OK);
                return;
            }
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                Util.applyLowPassFilter(m_samplingFrequency, cutoffFrequency, m_values[ch].Length, m_values[ch]);
            }
            AppliedFilter.AddDigitalFilter((int)AppliedFilter.TypeOfDigitalFilter.LOWPASS_FILTER, cutoffFrequency);
            TextBoxLPF.Text = "";
            Plot();
        }

        private void ButtonHPF_Click(object sender, RoutedEventArgs e)
        {
            double cutoffFrequency = 0;
            if (String.IsNullOrEmpty(TextBoxHPF.Text))
            {
                return;
            }
            bool canConvert = double.TryParse(TextBoxHPF.Text, out cutoffFrequency);
            if (!canConvert)
            {
                MessageBox.Show(TextBoxHPF.Text + " cannot be recognized a number.", "Error", MessageBoxButton.OK);
                return;
            }
            double nyquistFrequency = m_samplingFrequency / 2;
            if (cutoffFrequency >= nyquistFrequency)
            {
                MessageBox.Show(TextBoxHPF.Text + " is higher than the Nyquist frequency(" + nyquistFrequency.ToString() + " Hz).", "Error", MessageBoxButton.OK);
                return;
            }
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                Util.applyHighPassFilter(m_samplingFrequency, cutoffFrequency, m_values[ch].Length, m_values[ch]);
            }
            AppliedFilter.AddDigitalFilter((int)AppliedFilter.TypeOfDigitalFilter.HIGHPASS_FILTER, cutoffFrequency);
            TextBoxHPF.Text = "";
            Plot();
        }

        private void ButtonNotch_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxNotch.Text))
            {
                return;
            }
            string[] cutOfFrequencies = TextBoxNotch.Text.Split(',');
            foreach (string strCutOfFrequency in cutOfFrequencies)
            {
                if (String.IsNullOrEmpty(strCutOfFrequency))
                {
                    continue;
                }
                double cutoffFrequency = 0;
                bool canConvert = double.TryParse(strCutOfFrequency, out cutoffFrequency);
                if (!canConvert)
                {
                    MessageBox.Show(strCutOfFrequency + " cannot be recognized a number.", "Error", MessageBoxButton.OK);
                    return;
                }
                double nyquistFrequency = m_samplingFrequency / 2;
                if (cutoffFrequency >= nyquistFrequency)
                {
                    MessageBox.Show(strCutOfFrequency + " is higher than the Nyquist frequency(" + nyquistFrequency.ToString() + " Hz).", "Error", MessageBoxButton.OK);
                    return;
                }
                for (int ch = 0; ch < m_numOfChannels; ++ch)
                {
                    Util.applyNotchFilter(m_samplingFrequency, cutoffFrequency, m_values[ch].Length, m_values[ch]);
                }
                AppliedFilter.AddDigitalFilter((int)AppliedFilter.TypeOfDigitalFilter.NOTCH_FILTER, cutoffFrequency);
            }
            TextBoxNotch.Text = "";
            Plot();
        }

        private void TimeSeriesCh0_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_signalPlot[0] != null)
            {
                (double mouseCoordX, double mouseCoordY) = TimeSeriesCh0.GetMouseCoordinates();
                (double date, double value, int pointIndex) = m_signalPlot[0].GetPointNearestX(mouseCoordX);
                this.Title = m_title + "　" + DateTime.FromOADate(date).ToString("yyyy/MM/dd") + " " + DateTime.FromOADate(date).ToString("HH:mm:ss.FFF");
            }
        }

        private void TimeSeriesCh1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_signalPlot[1] != null)
            {
                (double mouseCoordX, double mouseCoordY) = TimeSeriesCh1.GetMouseCoordinates();
                (double date, double value, int pointIndex) = m_signalPlot[1].GetPointNearestX(mouseCoordX);
                this.Title = m_title + "　" + DateTime.FromOADate(date).ToString("yyyy/MM/dd") + " " + DateTime.FromOADate(date).ToString("HH:mm:ss.FFF");
            }
        }

        private void TimeSeriesCh2_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_signalPlot[2] != null)
            {
                (double mouseCoordX, double mouseCoordY) = TimeSeriesCh2.GetMouseCoordinates();
                (double date, double value, int pointIndex) = m_signalPlot[2].GetPointNearestX(mouseCoordX);
                this.Title = m_title + "　" + DateTime.FromOADate(date).ToString("yyyy/MM/dd") + " " + DateTime.FromOADate(date).ToString("HH:mm:ss.FFF");
            }
        }

        private void TimeSeriesCh3_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_signalPlot[3] != null)
            {
                (double mouseCoordX, double mouseCoordY) = TimeSeriesCh3.GetMouseCoordinates();
                (double date, double value, int pointIndex) = m_signalPlot[3].GetPointNearestX(mouseCoordX);
                this.Title = m_title + "　" + DateTime.FromOADate(date).ToString("yyyy/MM/dd") + " " + DateTime.FromOADate(date).ToString("HH:mm:ss.FFF");
            }
        }

        private void TimeSeriesCh4_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_signalPlot[4] != null)
            {
                (double mouseCoordX, double mouseCoordY) = TimeSeriesCh4.GetMouseCoordinates();
                (double date, double value, int pointIndex) = m_signalPlot[4].GetPointNearestX(mouseCoordX);
                this.Title = m_title + "　" + DateTime.FromOADate(date).ToString("yyyy/MM/dd") + " " + DateTime.FromOADate(date).ToString("HH:mm:ss.FFF");
            }
        }

        private void TimeSeriesCh5_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_signalPlot[5] != null)
            {
                (double mouseCoordX, double mouseCoordY) = TimeSeriesCh5.GetMouseCoordinates();
                (double date, double value, int pointIndex) = m_signalPlot[5].GetPointNearestX(mouseCoordX);
                this.Title = m_title + "　" + DateTime.FromOADate(date).ToString("yyyy/MM/dd") + " " + DateTime.FromOADate(date).ToString("HH:mm:ss.FFF");
            }
        }

        private void TimeSeriesCh0_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Title = m_title;
        }

        private void TimeSeriesCh1_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Title = m_title;
        }

        private void TimeSeriesCh2_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Title = m_title;
        }

        private void TimeSeriesCh3_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Title = m_title;
        }

        private void TimeSeriesCh4_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Title = m_title;
        }

        private void TimeSeriesCh5_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Title = m_title;
        }

        private void ButtonToYesterday_Click(object sender, RoutedEventArgs e)
        {
            if (ReadFilesOfYesterday())
            {
                ApplyFilters();
                Plot();
            }
        }

        private void ButtonToTomorrow_Click(object sender, RoutedEventArgs e)
        {
            if (ReadFilesOfTomorrow())
            {
                ApplyFilters();
                Plot();
            }
        }

        private void ButtonToYesterdayEnd_Click(object sender, RoutedEventArgs e)
        {
            if (ReadFilesOfYesterday())
            {
                ApplyFilters();
                SliderForStartTime.Value = 100;
                Plot();
            }
        }

        private void ButtonToTomorrowStart_Click(object sender, RoutedEventArgs e)
        {
            if (ReadFilesOfTomorrow())
            {
                ApplyFilters();
                SliderForStartTime.Value = 0;
                Plot();
            }
        }
        
        protected abstract bool ReadFilesOfYesterday();

        protected abstract bool ReadFilesOfTomorrow();

        protected abstract bool RereadCurrentFiles();


        private void TimeSeriesCh0_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = TimeSeriesCh0.Plot.Copy();
            tsCopy.XAxis.DateTimeFormat(true);
            tsCopy.XAxis.Ticks(true);
            tsCopy.YAxis.Ticks(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Time series Ex";
            wpfPlotViewer.Show();
        }

        private void TimeSeriesCh1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = TimeSeriesCh1.Plot.Copy();
            tsCopy.XAxis.DateTimeFormat(true);
            tsCopy.XAxis.Ticks(true);
            tsCopy.YAxis.Ticks(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Time series Ey";
            wpfPlotViewer.Show();
        }

        private void TimeSeriesCh2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = TimeSeriesCh2.Plot.Copy();
            tsCopy.XAxis.DateTimeFormat(true);
            tsCopy.XAxis.Ticks(true);
            tsCopy.YAxis.Ticks(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Time series Hx";
            wpfPlotViewer.Show();
        }

        private void TimeSeriesCh3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = TimeSeriesCh3.Plot.Copy();
            tsCopy.XAxis.DateTimeFormat(true);
            tsCopy.XAxis.Ticks(true);
            tsCopy.YAxis.Ticks(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Time series Hy";
            wpfPlotViewer.Show();
        }

        private void TimeSeriesCh4_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = TimeSeriesCh4.Plot.Copy();
            tsCopy.XAxis.DateTimeFormat(true);
            tsCopy.XAxis.Ticks(true);
            tsCopy.YAxis.Ticks(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Time series Hrx";
            wpfPlotViewer.Show();
        }

        private void TimeSeriesCh5_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = TimeSeriesCh5.Plot.Copy();
            tsCopy.XAxis.DateTimeFormat(true);
            tsCopy.XAxis.Ticks(true);
            tsCopy.YAxis.Ticks(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Time series Hry";
            wpfPlotViewer.Show();
        }

        private void ButtonFilterClear_Click(object sender, RoutedEventArgs e)
        {
            AppliedFilter.ClearDigitalFilters();
            if (RereadCurrentFiles())
            {
                Plot();
            }
        }

        private void ButtonFilterHistory_Click(object sender, RoutedEventArgs e)
        {
            var appliedFilter = new AppliedFilter();
            if (appliedFilter.CaluculateFrequencyCharacteristic(m_samplingFrequency, m_values[0].Length))
            {
                appliedFilter.Plot();
                appliedFilter.ShowDialog();
            }
            else
            {
                MessageBox.Show("Some problem occured in the calculation of filter characteristics.", "Error", MessageBoxButton.OK);
            }
            return;
        }

        private void ApplyFilters()
        {
            int numOfAppliedFilters = AppliedFilter.getNumberOfAppliedFilters();
            for (int i = 0; i < numOfAppliedFilters; i++)
            {
                switch (AppliedFilter.getAppliedFilterType(i))
                {
                    case (int)TypeOfDigitalFilter.LOWPASS_FILTER:
                        for (int ch = 0; ch < m_numOfChannels; ++ch)
                        {
                            Util.applyLowPassFilter(m_samplingFrequency, AppliedFilter.getCutoffFrequency(i), m_values[ch].Length, m_values[ch]);
                        }
                        break;
                    case (int)TypeOfDigitalFilter.HIGHPASS_FILTER:
                        for (int ch = 0; ch < m_numOfChannels; ++ch)
                        {
                            Util.applyHighPassFilter(m_samplingFrequency, AppliedFilter.getCutoffFrequency(i), m_values[ch].Length, m_values[ch]);
                        }
                        break;
                    case (int)TypeOfDigitalFilter.NOTCH_FILTER:
                        for (int ch = 0; ch < m_numOfChannels; ++ch)
                        {
                            Util.applyNotchFilter(m_samplingFrequency, AppliedFilter.getCutoffFrequency(i), m_values[ch].Length, m_values[ch]);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
