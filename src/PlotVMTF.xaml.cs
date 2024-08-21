using ScottPlot.Drawing.Colormaps;
using ScottPlot.Plottable;
using ScottPlot;
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
    /// PlotVMTF.xaml の相互作用ロジック
    /// </summary>
    public abstract partial class PlotVMTF : Window
    {
        protected List<double> m_periods = new List<double>();
        protected List<double> m_coherenceHz = new List<double>();
        protected List<double> m_ReTzx = new List<double>();
        protected List<double> m_ImTzx = new List<double>();
        protected List<double> m_ReTzy = new List<double>();
        protected List<double> m_ImTzy = new List<double>();
        protected List<double> m_errorTzx = new List<double>();
        protected List<double> m_errorTzy = new List<double>();
        private ScatterPlot m_plotCoherenceHz;
        private ScatterPlot m_plotReTzx;
        private ScatterPlot m_plotImTzx;
        private ScatterPlot m_plotReTzy;
        private ScatterPlot m_plotImTzy;
        private ErrorBar m_plotErrorBarReTzx;
        private ErrorBar m_plotErrorBarImTzx;
        private ErrorBar m_plotErrorBarReTzy;
        private ErrorBar m_plotErrorBarImTzy;

        public PlotVMTF()
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
        }

        public abstract bool ReadCsvFile(string csvFileName);

        public void Plot()
        {
            double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
            m_plotCoherenceHz = SquaredCoherence.Plot.AddScatter(logPeriods, m_coherenceHz.ToArray(), color: System.Drawing.Color.Black);
            SquaredCoherence.Plot.Title("Coherence squared");
            SquaredCoherence.Plot.XLabel("Period (sec)");
            SquaredCoherence.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
            SquaredCoherence.Plot.XAxis.MinorLogScale(true);
            SquaredCoherence.Plot.AddHorizontalLine(y: 0.0, color: System.Drawing.Color.Black, width: 1);
            SquaredCoherence.Plot.AddHorizontalLine(y: 1.0, color: System.Drawing.Color.Black, width: 1);
            SquaredCoherence.Plot.YAxis.SetBoundary(0.0, 1.0);
            SquaredCoherence.Configuration.DoubleClickBenchmark = false;
            SquaredCoherence.Refresh();

            // ZX
            m_plotReTzx = VMTF.Plot.AddScatter(logPeriods, m_ReTzx.ToArray(), System.Drawing.Color.Red, lineStyle: LineStyle.Dot);
            m_plotErrorBarReTzx = VMTF.Plot.AddErrorBars(logPeriods, m_ReTzx.ToArray(), null, m_errorTzx.ToArray(), System.Drawing.Color.Red);
            m_plotImTzx = VMTF.Plot.AddScatter(logPeriods, m_ImTzx.ToArray(), System.Drawing.Color.Magenta, lineStyle: LineStyle.Dot);
            m_plotErrorBarImTzx = VMTF.Plot.AddErrorBars(logPeriods, m_ImTzx.ToArray(), null, m_errorTzx.ToArray(), System.Drawing.Color.Magenta);
            // ZY
            m_plotReTzy = VMTF.Plot.AddScatter(logPeriods, m_ReTzy.ToArray(), System.Drawing.Color.Blue, lineStyle: LineStyle.Dot);
            m_plotErrorBarReTzy = VMTF.Plot.AddErrorBars(logPeriods, m_ReTzy.ToArray(), null, m_errorTzy.ToArray(), System.Drawing.Color.Blue);
            m_plotImTzy = VMTF.Plot.AddScatter(logPeriods, m_ImTzy.ToArray(), System.Drawing.Color.Cyan, lineStyle: LineStyle.Dot);
            m_plotErrorBarImTzy = VMTF.Plot.AddErrorBars(logPeriods, m_ImTzy.ToArray(), null, m_errorTzy.ToArray(), System.Drawing.Color.Cyan);
            List<double> minVMTF = new List<double>();
            minVMTF.Add(m_ReTzx.Min());
            minVMTF.Add(m_ImTzx.Min());
            minVMTF.Add(m_ReTzy.Min());
            minVMTF.Add(m_ImTzy.Min());
            List<double> maxVMTF = new List<double>();
            maxVMTF.Add(m_ReTzx.Max());
            maxVMTF.Add(m_ImTzx.Max());
            maxVMTF.Add(m_ReTzy.Max());
            maxVMTF.Add(m_ImTzy.Max());
            VMTF.Plot.YAxis.SetBoundary(minVMTF.Min() - 0.5, maxVMTF.Max() + 0.5);
            VMTF.Plot.Title("VMTF (Tipper)");
            VMTF.Plot.XLabel("Period (sec)");
            VMTF.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
            VMTF.Plot.XAxis.MinorLogScale(true);
            VMTF.Configuration.DoubleClickBenchmark = false;
            VMTF.Refresh();
        }

        private void CheckBoxReTzx_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotReTzx = VMTF.Plot.AddScatter(logPeriods, m_ReTzx.ToArray(), System.Drawing.Color.Red, lineStyle: LineStyle.Dot);
                m_plotErrorBarReTzx = VMTF.Plot.AddErrorBars(logPeriods, m_ReTzx.ToArray(), null, m_errorTzx.ToArray(), System.Drawing.Color.Red);
                VMTF.Refresh();
            }
        }

        private void CheckBoxImTzx_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotImTzx = VMTF.Plot.AddScatter(logPeriods, m_ImTzx.ToArray(), System.Drawing.Color.Magenta, lineStyle: LineStyle.Dot);
                m_plotErrorBarImTzx = VMTF.Plot.AddErrorBars(logPeriods, m_ImTzx.ToArray(), null, m_errorTzx.ToArray(), System.Drawing.Color.Magenta);
                VMTF.Refresh();
            }
        }

        private void CheckBoxReTzy_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotReTzy = VMTF.Plot.AddScatter(logPeriods, m_ReTzy.ToArray(), System.Drawing.Color.Blue, lineStyle: LineStyle.Dot);
                m_plotErrorBarReTzy = VMTF.Plot.AddErrorBars(logPeriods, m_ReTzy.ToArray(), null, m_errorTzy.ToArray(), System.Drawing.Color.Blue);
                VMTF.Refresh();
            }
        }

        private void CheckBoxImTzy_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotImTzy = VMTF.Plot.AddScatter(logPeriods, m_ImTzy.ToArray(), System.Drawing.Color.Cyan, lineStyle: LineStyle.Dot);
                m_plotErrorBarImTzy = VMTF.Plot.AddErrorBars(logPeriods, m_ImTzy.ToArray(), null, m_errorTzy.ToArray(), System.Drawing.Color.Cyan);
                VMTF.Refresh();
            }
        }

        private void CheckBoxReTzx_UnChecked(object sender, RoutedEventArgs e)
        {
            VMTF.Plot.Remove(m_plotReTzx);
            VMTF.Plot.Remove(m_plotErrorBarReTzx);
            VMTF.Refresh();
        }

        private void CheckBoxImTzx_UnChecked(object sender, RoutedEventArgs e)
        {
            VMTF.Plot.Remove(m_plotImTzx);
            VMTF.Plot.Remove(m_plotErrorBarImTzx);
            VMTF.Refresh();
        }

        private void CheckBoxReTzy_UnChecked(object sender, RoutedEventArgs e)
        {
            VMTF.Plot.Remove(m_plotReTzy);
            VMTF.Plot.Remove(m_plotErrorBarReTzy);
            VMTF.Refresh();
        }

        private void CheckBoxImTzy_UnChecked(object sender, RoutedEventArgs e)
        {
            VMTF.Plot.Remove(m_plotImTzy);
            VMTF.Plot.Remove(m_plotErrorBarImTzy);
            VMTF.Refresh();
        }

        private void SquaredCoherence_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var copy = SquaredCoherence.Plot.Copy();
            copy.Title("Coherence squared");
            copy.XLabel("Period (sec)");
            copy.XAxis.TickLabelFormat(Util.logTickLabels);
            copy.XAxis.MinorLogScale(true);
            copy.XAxis.MajorGrid(true);
            copy.XAxis.MinorGrid(true);
            copy.YAxis.SetBoundary(0.0, 1.0);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(copy);
            wpfPlotViewer.Title = "Coherence squared";
            wpfPlotViewer.Show();
        }

        private void VMTF_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var copy = VMTF.Plot.Copy();
            copy.Title("VMTF (Tipper)");
            copy.XLabel("Period (sec)");
            copy.XAxis.TickLabelFormat(Util.logTickLabels);
            copy.XAxis.MinorLogScale(true);
            copy.XAxis.MajorGrid(true);
            copy.XAxis.MinorGrid(true);
            copy.YAxis.SetBoundary(-180, 180);
            copy.YAxis.ManualTickSpacing(45);
            List<double> minVMTF = new List<double>();
            minVMTF.Add(m_ReTzx.Min());
            minVMTF.Add(m_ImTzx.Min());
            minVMTF.Add(m_ReTzy.Min());
            minVMTF.Add(m_ImTzy.Min());
            List<double> maxVMTF = new List<double>();
            maxVMTF.Add(m_ReTzx.Max());
            maxVMTF.Add(m_ImTzx.Max());
            maxVMTF.Add(m_ReTzy.Max());
            maxVMTF.Add(m_ImTzy.Max());
            copy.YAxis.SetBoundary(minVMTF.Min() - 0.5, maxVMTF.Max() + 0.5);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(copy);
            wpfPlotViewer.Title = "VMTF (Tipper)";
            wpfPlotViewer.Show();
        }

    }
}
