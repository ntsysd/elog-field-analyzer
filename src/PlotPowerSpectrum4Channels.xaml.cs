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
    /// PlotPowerSpectrum4Channels.xaml の相互作用ロジック
    /// </summary>
    public partial class PlotPowerSpectrum4Channels : Window
    {
        private bool m_plotted = false;
        private static int m_numOfChannels = 4;
        private double m_samplingFrequency = 0;
        private double[] m_slope;
        private double[] m_intercept;
        private double[][] m_logFreq;
        private double[][] m_logPower;
        private ScatterPlot[] m_plot;

        public PlotPowerSpectrum4Channels(double samplingFrequency)
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
            m_samplingFrequency = samplingFrequency;
            m_slope = new double[m_numOfChannels];
            m_intercept = new double[m_numOfChannels];
            m_logFreq = new double[m_numOfChannels][];
            m_logPower = new double[m_numOfChannels][];
            m_plot = new ScatterPlot[m_numOfChannels];
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                m_slope[ch] = 0.0;
                m_intercept[ch] = 0.0;
                m_plot[ch] = null;
            }
        }

        public void CaluculatePowerSpectrum(int ch, double[] timeSeries)
        {
            if (timeSeries.Any())
            {
                CaluculatePowerSpectrum(timeSeries, ref m_logFreq[ch], ref m_logPower[ch], ref m_slope[ch], ref m_intercept[ch]);
            }
        }

        private void CaluculatePowerSpectrum(double[] data, ref double[] logFreqs, ref double[] logPowers, ref double slope, ref double intercept)
        {
            int numData = data.Length;
            int numSegments = 10;
            int halfStackNumberPlusOne = numSegments / 2 + 1;
            int numDataOfEachSegment = numData / halfStackNumberPlusOne;
            int shiftLength = numDataOfEachSegment / 2;
            int numDataForFFT = 0;
            for (int i = 1; i < 1000; ++i)
            {
                int vpow2 = (int)Math.Pow(2, i);
                if (vpow2 >= numDataOfEachSegment)
                {
                    numDataForFFT = vpow2;
                    break;
                }
            }
            if (numDataForFFT <= 2)
            {
                MessageBox.Show("Selected time length is too  short.", "Error", MessageBoxButton.OK);
                return;
            }
            double average = data.Average();
            var power = new double[numDataForFFT];
            Array.Fill(power, 0.0);
            for (int iSeg = 0; iSeg < numSegments; ++iSeg)
            {
                var cdata = new Complex[numDataForFFT];
                Array.Fill(cdata, 0.0);
                int offset = iSeg * shiftLength;
                for (int i = 0; i < numDataOfEachSegment; ++i)
                {
                    cdata[i] = data[i + offset] - average;
                }
                Util.HanningWindow(numDataForFFT, ref cdata);
                Util.FourierTransform(cdata);
                for (int i = 0; i < numDataForFFT; ++i)
                {
                    double factor = (i == 0 || i == numDataForFFT / 2) ? 1.0 : 2.0;
                    power[i] += factor * Math.Pow(cdata[i].Magnitude, 2);
                }
            }
            double T = (double)numDataForFFT / (double)m_samplingFrequency;
            for (int i = 0; i < numDataForFFT; ++i)
            {
                // Averaging and adjustment of the scaling factor for the loss due to the Hanning tapering
                power[i] *= 8.0 / 3.0 / (double)numSegments * T;
            }
            double startOutputFrequency = 1.0 / (double)(numDataForFFT) * (double)m_samplingFrequency;
            double endOutputFrequency = 0.5 * (double)m_samplingFrequency;//Nyquist
            int numOfOutputFrequency = 10000;
            double logFreqStart = Math.Log10(startOutputFrequency);
            double logFreqEnd = Math.Log10(endOutputFrequency);
            double logFreqInc = (logFreqEnd - logFreqStart) / (double)numOfOutputFrequency;
            int outputFreqIndexPre = -1;
            List<double> logPowerList = new List<double>();
            List<double> logFreqsList = new List<double>();
            for (int i = 1; i < numDataForFFT / 2 + 1; ++i)
            {
                double freq = (double)i / (double)(numDataForFFT) * (double)m_samplingFrequency;
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
                logPowerList.Add(Math.Log10(power[i]));
            }
            logFreqs = logFreqsList.ToArray();
            logPowers = logPowerList.ToArray();
            // Calculate slope and intercept
            double logFreqsAverage = logFreqsList.Average();
            double logPowerAverage = logPowerList.Average();
            double denominator = 0.0;
            double numerator = 0.0;
            int numFreqs = logFreqs.Length;
            for (int i = 0; i < numFreqs; ++i)
            {
                denominator += (logFreqs[i] - logFreqsAverage) * (logFreqs[i] - logFreqsAverage);
                numerator += (logFreqs[i] - logFreqsAverage) * (logPowerList[i] - logPowerAverage);
            }
            slope = numerator / denominator;
            intercept = logPowerAverage - slope * logFreqsAverage;
        }

        public void Plot()
        {
            if (m_plotted)
            {
                PowerSpectrumCh0.Plot.Clear();
                PowerSpectrumCh1.Plot.Clear();
                PowerSpectrumCh2.Plot.Clear();
                PowerSpectrumCh3.Plot.Clear();
            }
            double lowerLogFrequency = 0;
            double upperLogFrequency = 0;
            if (m_logFreq[0].Any())
            {
                lowerLogFrequency = m_logFreq[0].Min();
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
                upperLogFrequency = m_logFreq[0].Max();
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
            for (int ch = 0; ch < m_numOfChannels; ++ch)
            {
                if (m_logFreq[ch] != null && m_logFreq[ch].Any())
                {
                    int num = m_logFreq[ch].Length;
                    var logFreqFreqBounded = new List<double>();
                    var logFreqPowerBounded = new List<double>();
                    double slope = 0.0;
                    if ((bool)CheckBoxDetrend.IsChecked)
                    {
                        slope = m_slope[ch];
                    }
                    for (int i = 0; i < num; i++)
                    {
                        if (m_logFreq[ch][i] >= lowerLogFrequency && m_logFreq[ch][i] <= upperLogFrequency)
                        {
                            logFreqFreqBounded.Add(m_logFreq[ch][i]);
                            logFreqPowerBounded.Add(m_logPower[ch][i] - slope * m_logFreq[ch][i]);
                        }
                    }
                    TextBlockMinimumFrequency.Text = (Math.Pow(10, lowerLogFrequency)).ToString("G3");
                    TextBlockMaximumFrequency.Text = (Math.Pow(10, upperLogFrequency)).ToString("G3");
                    List<double> log10FreqIndexes = new List<double>();
                    for (int i = (int)Math.Ceiling(lowerLogFrequency); i <= (int)Math.Floor(upperLogFrequency); i++)
                    {
                        log10FreqIndexes.Add(i);
                    }
                    switch (ch)
                    {
                        case 0:
                            m_plot[ch] = PowerSpectrumCh0.Plot.AddScatter(logFreqFreqBounded.ToArray(), logFreqPowerBounded.ToArray(), markerSize: 0, lineWidth: 1);
                            foreach (double log10Freq in log10FreqIndexes)
                            {
                                PowerSpectrumCh0.Plot.AddVerticalLine(x: log10Freq, color: System.Drawing.Color.Gray, width: 1);
                            }
                            PowerSpectrumCh0.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh0.Plot.XAxis.MinorLogScale(true);
                            PowerSpectrumCh0.Plot.XAxis.MajorGrid(true);
                            PowerSpectrumCh0.Plot.XAxis.MinorGrid(true);
                            PowerSpectrumCh0.Plot.YAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh0.Plot.YAxis.MinorLogScale(true);
                            PowerSpectrumCh0.Plot.XAxis.Ticks(false);
                            PowerSpectrumCh0.Plot.YAxis.Ticks(false);
                            PowerSpectrumCh0.Configuration.DoubleClickBenchmark = false;
                            PowerSpectrumCh0.Refresh();
                            TextBlockCh0Min.Text = (Math.Pow(10, logFreqPowerBounded.Min())).ToString("G4");
                            TextBlockCh0Max.Text = (Math.Pow(10, logFreqPowerBounded.Max())).ToString("G4");
                            break;
                        case 1:
                            m_plot[ch] = PowerSpectrumCh1.Plot.AddScatter(logFreqFreqBounded.ToArray(), logFreqPowerBounded.ToArray(), markerSize: 0, lineWidth: 1);
                            foreach (double log10Freq in log10FreqIndexes)
                            {
                                PowerSpectrumCh1.Plot.AddVerticalLine(x: log10Freq, color: System.Drawing.Color.Gray, width: 1);
                            }
                            PowerSpectrumCh1.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh1.Plot.XAxis.MinorLogScale(true);
                            PowerSpectrumCh1.Plot.XAxis.MajorGrid(true);
                            PowerSpectrumCh1.Plot.XAxis.MinorGrid(true);
                            PowerSpectrumCh1.Plot.YAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh1.Plot.YAxis.MinorLogScale(true);
                            PowerSpectrumCh1.Plot.XAxis.Ticks(false);
                            PowerSpectrumCh1.Plot.YAxis.Ticks(false);
                            PowerSpectrumCh1.Configuration.DoubleClickBenchmark = false;
                            PowerSpectrumCh1.Refresh();
                            TextBlockCh1Min.Text = (Math.Pow(10, logFreqPowerBounded.Min())).ToString("G4");
                            TextBlockCh1Max.Text = (Math.Pow(10, logFreqPowerBounded.Max())).ToString("G4");
                            break;
                        case 2:
                            m_plot[ch] = PowerSpectrumCh2.Plot.AddScatter(logFreqFreqBounded.ToArray(), logFreqPowerBounded.ToArray(), markerSize: 0, lineWidth: 1);
                            foreach (double log10Freq in log10FreqIndexes)
                            {
                                PowerSpectrumCh2.Plot.AddVerticalLine(x: log10Freq, color: System.Drawing.Color.Gray, width: 1);
                            }
                            PowerSpectrumCh2.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh2.Plot.XAxis.MinorLogScale(true);
                            PowerSpectrumCh2.Plot.XAxis.MajorGrid(true);
                            PowerSpectrumCh2.Plot.XAxis.MinorGrid(true);
                            PowerSpectrumCh2.Plot.YAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh2.Plot.YAxis.MinorLogScale(true);
                            PowerSpectrumCh2.Plot.XAxis.Ticks(false);
                            PowerSpectrumCh2.Plot.YAxis.Ticks(false);
                            PowerSpectrumCh2.Configuration.DoubleClickBenchmark = false;
                            PowerSpectrumCh2.Refresh();
                            TextBlockCh2Min.Text = (Math.Pow(10, logFreqPowerBounded.Min())).ToString("G4");
                            TextBlockCh2Max.Text = (Math.Pow(10, logFreqPowerBounded.Max())).ToString("G4");
                            break;
                        case 3:
                            m_plot[ch] = PowerSpectrumCh3.Plot.AddScatter(logFreqFreqBounded.ToArray(), logFreqPowerBounded.ToArray(), markerSize: 0, lineWidth: 1);
                            foreach (double log10Freq in log10FreqIndexes)
                            {
                                PowerSpectrumCh3.Plot.AddVerticalLine(x: log10Freq, color: System.Drawing.Color.Gray, width: 1);
                            }
                            PowerSpectrumCh3.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh3.Plot.XAxis.MinorLogScale(true);
                            PowerSpectrumCh3.Plot.XAxis.MajorGrid(true);
                            PowerSpectrumCh3.Plot.XAxis.MinorGrid(true);
                            PowerSpectrumCh3.Plot.YAxis.TickLabelFormat(Util.logTickLabels);
                            PowerSpectrumCh3.Plot.YAxis.MinorLogScale(true);
                            PowerSpectrumCh3.Plot.XAxis.Ticks(false);
                            PowerSpectrumCh3.Plot.YAxis.Ticks(false);
                            PowerSpectrumCh3.Configuration.DoubleClickBenchmark = false;
                            PowerSpectrumCh3.Refresh();
                            TextBlockCh3Min.Text = (Math.Pow(10, logFreqPowerBounded.Min())).ToString("G4");
                            TextBlockCh3Max.Text = (Math.Pow(10, logFreqPowerBounded.Max())).ToString("G4");
                            break;
                    }
                }
                else
                {
                    m_plot[ch] = null;
                }
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

        private void CheckBoxDetrend_Checked(object sender, RoutedEventArgs e)
        {
            if (m_plotted)
            {
                Plot();
            }
        }

        private void CheckBoxDetrend_Unchecked(object sender, RoutedEventArgs e)
        {
            if (m_plotted)
            {
                Plot();
            }
        }

        private void PowerSpectrumCh0_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_plot[0] != null)
            {
                (double mouseCoordX, double mouseCoordY) = PowerSpectrumCh0.GetMouseCoordinates();
                double ratio = PowerSpectrumCh0.Plot.XAxis.Dims.PxPerUnit / PowerSpectrumCh0.Plot.YAxis.Dims.PxPerUnit;
                (double log10Freq, double log10Power, int pointIndex) = m_plot[0].GetPointNearest(mouseCoordX, mouseCoordY, ratio);
                TextBlockMousePointFrequency.Text = (Math.Pow(10, log10Freq)).ToString("G3");
            }
        }

        private void PowerSpectrumCh1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_plot[1] != null)
            {
                (double mouseCoordX, double mouseCoordY) = PowerSpectrumCh1.GetMouseCoordinates();
                double ratio = PowerSpectrumCh1.Plot.XAxis.Dims.PxPerUnit / PowerSpectrumCh1.Plot.YAxis.Dims.PxPerUnit;
                (double log10Freq, double log10Power, int pointIndex) = m_plot[1].GetPointNearest(mouseCoordX, mouseCoordY, ratio);
                TextBlockMousePointFrequency.Text = (Math.Pow(10, log10Freq)).ToString("G3");
            }
        }

        private void PowerSpectrumCh2_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_plot[2] != null)
            {
                (double mouseCoordX, double mouseCoordY) = PowerSpectrumCh2.GetMouseCoordinates();
                double ratio = PowerSpectrumCh2.Plot.XAxis.Dims.PxPerUnit / PowerSpectrumCh2.Plot.YAxis.Dims.PxPerUnit;
                (double log10Freq, double log10Power, int pointIndex) = m_plot[2].GetPointNearest(mouseCoordX, mouseCoordY, ratio);
                TextBlockMousePointFrequency.Text = (Math.Pow(10, log10Freq)).ToString("G3");
            }
        }

        private void PowerSpectrumCh3_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_plot[3] != null)
            {
                (double mouseCoordX, double mouseCoordY) = PowerSpectrumCh3.GetMouseCoordinates();
                double ratio = PowerSpectrumCh3.Plot.XAxis.Dims.PxPerUnit / PowerSpectrumCh3.Plot.YAxis.Dims.PxPerUnit;
                (double log10Freq, double log10Power, int pointIndex) = m_plot[3].GetPointNearest(mouseCoordX, mouseCoordY, ratio);
                TextBlockMousePointFrequency.Text = (Math.Pow(10, log10Freq)).ToString("G3");
            }
        }

        private void PowerSpectrumCh0_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlockMousePointFrequency.Text = "";
        }

        private void PowerSpectrumCh1_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlockMousePointFrequency.Text = "";
        }

        private void PowerSpectrumCh2_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlockMousePointFrequency.Text = "";
        }

        private void PowerSpectrumCh3_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlockMousePointFrequency.Text = "";
        }

        private void PowerSpectrumCh0_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = PowerSpectrumCh0.Plot.Copy();
            tsCopy.XAxis.Ticks(true);
            tsCopy.XAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.XAxis.MinorLogScale(true);
            tsCopy.XAxis.MajorGrid(true);
            tsCopy.XAxis.MinorGrid(true);
            tsCopy.YAxis.Ticks(true);
            tsCopy.YAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.YAxis.MinorLogScale(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Power spectrum Ex";
            wpfPlotViewer.Show();
        }

        private void PowerSpectrumCh1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = PowerSpectrumCh1.Plot.Copy();
            tsCopy.XAxis.Ticks(true);
            tsCopy.XAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.XAxis.MinorLogScale(true);
            tsCopy.XAxis.MajorGrid(true);
            tsCopy.XAxis.MinorGrid(true);
            tsCopy.YAxis.Ticks(true);
            tsCopy.YAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.YAxis.MinorLogScale(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Power spectrum Ey";
            wpfPlotViewer.Show();
        }

        private void PowerSpectrumCh2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = PowerSpectrumCh2.Plot.Copy();
            tsCopy.XAxis.Ticks(true);
            tsCopy.XAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.XAxis.MinorLogScale(true);
            tsCopy.XAxis.MajorGrid(true);
            tsCopy.XAxis.MinorGrid(true);
            tsCopy.YAxis.Ticks(true);
            tsCopy.YAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.YAxis.MinorLogScale(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Power spectrum Hx";
            wpfPlotViewer.Show();
        }

        private void PowerSpectrumCh3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tsCopy = PowerSpectrumCh3.Plot.Copy();
            tsCopy.XAxis.Ticks(true);
            tsCopy.XAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.XAxis.MinorLogScale(true);
            tsCopy.XAxis.MajorGrid(true);
            tsCopy.XAxis.MinorGrid(true);
            tsCopy.YAxis.Ticks(true);
            tsCopy.YAxis.TickLabelFormat(Util.logTickLabels);
            tsCopy.YAxis.MinorLogScale(true);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(tsCopy);
            wpfPlotViewer.Title = "Power spectrum Hy";
            wpfPlotViewer.Show();
        }

    }
}
