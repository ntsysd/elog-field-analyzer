using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    /// AppliedFilter.xaml の相互作用ロジック
    /// </summary>
    public partial class AppliedFilter : Window
    {

        public enum TypeOfDigitalFilter
        {
            LOWPASS_FILTER = 0, 
            HIGHPASS_FILTER = 1,
            NOTCH_FILTER =2,
        }

        public struct DigitalFilter {
            public int typeOfDigitalFilter { get; set; }
            public double cuttoffFrequency { get; set; }
            public DigitalFilter(int type, double freq)
            {
                typeOfDigitalFilter = type;
                cuttoffFrequency = freq;
            }
        }

       
        private static List<DigitalFilter> m_digitalFilterList = new List<DigitalFilter>();
        private bool m_plotted = false;
        private double[] m_logFreq;
        private Complex[] m_frequencyCharacteristcs;
        private ScatterPlot m_plot;

        public AppliedFilter()
        {
            InitializeComponent();
            m_plotted = false;
            Title += "   " + Common.VERSION;
            foreach (var filter in m_digitalFilterList)
            {
                switch (filter.typeOfDigitalFilter)
                {
                    case (int)TypeOfDigitalFilter.LOWPASS_FILTER:
                        TextBoxLog.AppendText("Lowpass filter: Cutoff frequency " + filter.cuttoffFrequency.ToString() + " (Hz) \n");
                        break;
                    case (int)TypeOfDigitalFilter.HIGHPASS_FILTER:
                        TextBoxLog.AppendText("Highpass filter: Cutoff frequency " + filter.cuttoffFrequency.ToString() + " (Hz) \n");
                        break;
                    case (int)TypeOfDigitalFilter.NOTCH_FILTER:
                        TextBoxLog.AppendText("Notch filter: Cutoff frequency " + filter.cuttoffFrequency.ToString() + " (Hz) \n");
                        break;
                    default:
                        break;
                }
            }
        }

        public static void AddDigitalFilter( int type, double cutoffFrequency )
        {
            DigitalFilter df = new DigitalFilter();
            df.typeOfDigitalFilter = type;
            df.cuttoffFrequency = cutoffFrequency;
            m_digitalFilterList.Add(df);
        }

        public static void ClearDigitalFilters()
        {
            m_digitalFilterList.Clear();
        }

        public bool CaluculateFrequencyCharacteristic(double samplingFrequency, int numOfData)
        {
            try
            {
                double startOutputFrequency = 1.0 / (double)(numOfData) * (double)samplingFrequency;
                double endOutputFrequency = 0.5 * (double)samplingFrequency;//Nyquist
                int numOfOutputFrequency = 1000;
                double logFreqStart = Math.Log10(startOutputFrequency);
                double logFreqEnd = Math.Log10(endOutputFrequency);
                double logFreqInc = (logFreqEnd - logFreqStart) / (double)numOfOutputFrequency;
                int outputFreqIndexPre = -1;
                List<double> logFreqsList = new List<double>();
                List<Complex> frequencyCharacteristcList = new List<Complex>();
                for (int i = 1; i < numOfData / 2 + 1; ++i)
                {
                    double freq = (double)i / (double)(numOfData) * (double)samplingFrequency;
                    if (freq < startOutputFrequency || freq > endOutputFrequency)
                    {
                        continue;
                    }
                    double logFreq = Math.Log10(freq);
                    int outputFreqIndexCur = (int)(Math.Floor((logFreq - logFreqStart) / logFreqInc));
                    if (outputFreqIndexCur <= outputFreqIndexPre)
                    {
                        continue;
                    }
                    outputFreqIndexPre = outputFreqIndexCur;
                    logFreqsList.Add(logFreq);
                    Complex frequencyCharacteristc = 1.0;
                    foreach (var filter in m_digitalFilterList)
                    {
                        switch (filter.typeOfDigitalFilter)
                        {
                            case (int)TypeOfDigitalFilter.LOWPASS_FILTER:
                                frequencyCharacteristc *= Util.calculateFrequencyCharacteristicsOfIIRLowPassFilter(freq, samplingFrequency, filter.cuttoffFrequency);
                                break;
                            case (int)TypeOfDigitalFilter.HIGHPASS_FILTER:
                                frequencyCharacteristc *= Util.calculateFrequencyCharacteristicsOfIIRHighPassFilter(freq, samplingFrequency, filter.cuttoffFrequency);
                                break;
                            case (int)TypeOfDigitalFilter.NOTCH_FILTER:
                                frequencyCharacteristc *= Util.calculateFrequencyCharacteristicsOfNotchFilter(freq, samplingFrequency, filter.cuttoffFrequency);
                                break;
                            default:
                                break;
                        }
                    }
                    frequencyCharacteristcList.Add(frequencyCharacteristc);
                }
                m_logFreq = logFreqsList.ToArray();
                m_frequencyCharacteristcs = frequencyCharacteristcList.ToArray(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK);
                this.Close();

                return false;
            }
            return true;
        }

        public void Plot()
        {
            if (m_plotted)
            {
                FrequencyCharacteristic.Plot.Clear();
            }
            double lowerLogFrequency = 0;
            double upperLogFrequency = 0;
            if (m_logFreq.Any())
            {
                lowerLogFrequency = m_logFreq.Min();
                if (TextBoxLowerFrequency.Text != "")
                {
                    double freq;
                    if (double.TryParse(TextBoxLowerFrequency.Text, out freq))
                    {
                        if (freq <= 0.0)
                        {
                            MessageBox.Show("Lower bound of frequency is less than zero.", "Error", MessageBoxButton.OK);
                            return;
                        }
                        else
                        {
                            lowerLogFrequency = Math.Log10(freq);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lower bound of frequency(" + TextBoxLowerFrequency.Text + ") cannot be convered to a real number.", "Error", MessageBoxButton.OK);
                        return;
                    }
                }
                else
                {
                    TextBoxLowerFrequency.Text = (Math.Pow(10, lowerLogFrequency)).ToString("G2");
                }
                upperLogFrequency = m_logFreq.Max();
                if (TextBoxUpperFrequency.Text != "")
                {
                    double freq;
                    if (double.TryParse(TextBoxUpperFrequency.Text, out freq))
                    {
                        if (freq <= 0.0)
                        {
                            MessageBox.Show("Upper bound of frequency is less than zero.", "Error", MessageBoxButton.OK);
                            return;
                        }
                        else
                        {
                            upperLogFrequency = Math.Log10(freq);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Upper bound of frequency(" + TextBoxUpperFrequency.Text + ") cannot be convered to a real number.", "Error", MessageBoxButton.OK);
                        return;
                    }
                }
                else
                {
                    TextBoxUpperFrequency.Text = (Math.Pow(10, upperLogFrequency)).ToString("G2");
                }
                if (lowerLogFrequency >= upperLogFrequency)
                {
                    MessageBox.Show("Lower bound is higher than upper bound.", "Error", MessageBoxButton.OK);
                    return;
                }
            }
            else
            {
                return;
            }
            if (m_logFreq != null && m_logFreq.Any())
            {
                int num = m_logFreq.Length;
                var logFreqBounded = new List<double>();
                var logAmplitudeBounded = new List<double>();
                for (int i = 0; i < num; i++)
                {
                    if (m_logFreq[i] >= lowerLogFrequency && m_logFreq[i] <= upperLogFrequency)
                    {
                        logFreqBounded.Add(m_logFreq[i]);
                        double amplitude = m_frequencyCharacteristcs[i].Magnitude;
                        if (amplitude < 1.0e-10)
                        {
                            amplitude = 1.0e-10;
                        }
                        logAmplitudeBounded.Add(Math.Log10(amplitude));
                    }
                }
                m_plot = FrequencyCharacteristic.Plot.AddScatter(logFreqBounded.ToArray(), logAmplitudeBounded.ToArray(), markerSize: 0, lineWidth: 1);
                FrequencyCharacteristic.Plot.XLabel("Period (sec)");
                FrequencyCharacteristic.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
                FrequencyCharacteristic.Plot.XAxis.MinorLogScale(true);
                FrequencyCharacteristic.Plot.XAxis.MajorGrid(true);
                FrequencyCharacteristic.Plot.XAxis.MinorGrid(true);
                FrequencyCharacteristic.Plot.XAxis.Ticks(true);
                FrequencyCharacteristic.Plot.YLabel("Amplitude");
                FrequencyCharacteristic.Plot.YAxis.TickLabelFormat(Util.logTickLabels);
                FrequencyCharacteristic.Plot.YAxis.MinorLogScale(true);
                FrequencyCharacteristic.Plot.YAxis.MajorGrid(true);
                FrequencyCharacteristic.Plot.YAxis.MinorGrid(true);
                FrequencyCharacteristic.Plot.YAxis.Ticks(true);
                FrequencyCharacteristic.Configuration.DoubleClickBenchmark = false;
                FrequencyCharacteristic.Refresh();
            }
            else
            {
                m_plot = null;
            }
            m_plotted = true;
        }

        private void ButtonChangeFrequencyRange_Click(object sender, RoutedEventArgs e)
        {
            if (m_plotted)
            {
                Plot();
            }
        }

        private void FrequencyCharacteristic_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = FrequencyCharacteristic.Plot.Copy();
            tsCopy.XLabel("Period (sec)");
            tsCopy.XAxis.Ticks(true);
            tsCopy.XAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.XAxis.MinorLogScale(true);
            tsCopy.XAxis.MajorGrid(true);
            tsCopy.XAxis.MinorGrid(true);
            tsCopy.XAxis.Ticks(true);
            tsCopy.YLabel("Amplitude");
            tsCopy.YAxis.Ticks(true);
            tsCopy.YAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.YAxis.MinorLogScale(true);
            tsCopy.YAxis.MajorGrid(true);
            tsCopy.YAxis.MinorGrid(true);
            tsCopy.YAxis.Ticks(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Amplitude characteristic";
            wpfPlotViewer.Show();
        }

        public static int getNumberOfAppliedFilters()
        {
            return m_digitalFilterList.Count;
        }

        public static int getAppliedFilterType(int i)
        {
            return m_digitalFilterList[i].typeOfDigitalFilter;
        }

        public static double getCutoffFrequency(int i)
        {
            return m_digitalFilterList[i].cuttoffFrequency;
        }

        public static int getNumberOfHighPassFilters()
        {
            int count = 0;
            foreach (var filter in m_digitalFilterList)
            {
                if (filter.typeOfDigitalFilter == (int)TypeOfDigitalFilter.HIGHPASS_FILTER)
                {
                    ++count;
                }
            }
            return count;
       }

        public static int getNumberOfLowPassFilters()
        {
            int count = 0;
            foreach (var filter in m_digitalFilterList)
            {
                if (filter.typeOfDigitalFilter == (int)TypeOfDigitalFilter.LOWPASS_FILTER)
                {
                    ++count;
                }
            }
            return count;
       }

        public static int getNumberOfNotchFilters()
        {
            int count = 0;
            foreach (var filter in m_digitalFilterList)
            {
                if (filter.typeOfDigitalFilter == (int)TypeOfDigitalFilter.NOTCH_FILTER)
                {
                    ++count;
                }
            }
            return count;
        }

        public static double getHighestCuttoffFrequencyOfHighPassFilters()
        {
            if (getNumberOfHighPassFilters() == 0)
            {
                return 0.0;
            }
            double freq = 0.0;
            foreach (var filter in m_digitalFilterList)
            {
                if (filter.typeOfDigitalFilter == (int)TypeOfDigitalFilter.HIGHPASS_FILTER)
                {
                    if (filter.cuttoffFrequency > freq)
                    {
                        freq = filter.cuttoffFrequency;
                    }
                }
            }
            return freq;
        }

        public static double getLowestCuttoffFrequencyOfLowPassFilters()
        {
            if (getNumberOfLowPassFilters() == 0)
            {
                return 0.0;
            }
            double freq = 1e+10;
            foreach (var filter in m_digitalFilterList)
            {
                if (filter.typeOfDigitalFilter == (int)TypeOfDigitalFilter.LOWPASS_FILTER)
                {
                    if (filter.cuttoffFrequency < freq)
                    {
                        freq = filter.cuttoffFrequency;
                    }
                }
            }
            return freq;
        }

        public static double [] getCuttoffFrequenciesOfNotchFilters()
        {
            int numOfNotch = getNumberOfNotchFilters();
            if (numOfNotch == 0)
            {
                return null;
            }
            double [] frequencies = new double[numOfNotch];
            int count = 0;
            foreach (var filter in m_digitalFilterList)
            {
                if (filter.typeOfDigitalFilter == (int)TypeOfDigitalFilter.NOTCH_FILTER)
                {
                    frequencies[count] = filter.cuttoffFrequency;
                    ++count;
                }
            }
            return frequencies;
        }

    }
}
