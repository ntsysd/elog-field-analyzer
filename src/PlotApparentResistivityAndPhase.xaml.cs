using ScottPlot.Plottable;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
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
using static System.Formats.Asn1.AsnWriter;

namespace PlotTimeSeries
{
    /// <summary>
    /// PlotApparentResistivityAndPhase.xaml の相互作用ロジック
    /// </summary>
    public abstract partial class PlotApparentResistivityAndPhase : Window
    {
        protected List<double> m_periods = new List<double>();
        protected List<double> m_coherenceX = new List<double>();
        protected List<double> m_coherenceY = new List<double>();
        protected List<double> m_logRhoaXX = new List<double>();
        protected List<double> m_logRhoaXY = new List<double>();
        protected List<double> m_logRhoaYX = new List<double>();
        protected List<double> m_logRhoaYY = new List<double>();
        protected List<double> m_phsXX = new List<double>();
        protected List<double> m_phsXY = new List<double>();
        protected List<double> m_phsYX = new List<double>();
        protected List<double> m_phsYY = new List<double>();
        protected List<double> m_errorLogRhoaXXPos = new List<double>();
        protected List<double> m_errorLogRhoaXYPos = new List<double>();
        protected List<double> m_errorLogRhoaYXPos = new List<double>();
        protected List<double> m_errorLogRhoaYYPos = new List<double>();
        protected List<double> m_errorLogRhoaXXNeg = new List<double>();
        protected List<double> m_errorLogRhoaXYNeg = new List<double>();
        protected List<double> m_errorLogRhoaYXNeg = new List<double>();
        protected List<double> m_errorLogRhoaYYNeg = new List<double>();
        protected List<double> m_errorPhsXX = new List<double>();
        protected List<double> m_errorPhsXY = new List<double>();
        protected List<double> m_errorPhsYX = new List<double>();
        protected List<double> m_errorPhsYY = new List<double>();
        private ScatterPlot m_plotCoherenceEx;
        private ScatterPlot m_plotCoherenceEy;
        private ScatterPlot m_plotRhoaXX;
        private ScatterPlot m_plotRhoaXY;
        private ScatterPlot m_plotRhoaYX;
        private ScatterPlot m_plotRhoaYY;
        private ErrorBar m_plotRhoaErrorBarXX;
        private ErrorBar m_plotRhoaErrorBarXY;
        private ErrorBar m_plotRhoaErrorBarYX;
        private ErrorBar m_plotRhoaErrorBarYY;
        private ScatterPlot m_plotPhsXX;
        private ScatterPlot m_plotPhsXY;
        private ScatterPlot m_plotPhsYX;
        private ScatterPlot m_plotPhsYY;
        private ErrorBar m_plotPhsErrorBarXX;
        private ErrorBar m_plotPhsErrorBarXY;
        private ErrorBar m_plotPhsErrorBarYX;
        private ErrorBar m_plotPhsErrorBarYY;

        public PlotApparentResistivityAndPhase()
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
        }
        public abstract bool ReadCsvFile(string csvFileName);

        public void Plot()
        {
            double[] periods = m_periods.Select(x => Math.Log10(x)).ToArray();
            m_plotCoherenceEx = SquaredCoherence.Plot.AddScatter(periods, m_coherenceX.ToArray(), color: System.Drawing.Color.Red);
            m_plotCoherenceEy = SquaredCoherence.Plot.AddScatter(periods, m_coherenceY.ToArray(), color: System.Drawing.Color.Blue);
            SquaredCoherence.Plot.Title("Coherence squared");
            SquaredCoherence.Plot.XLabel("Period (sec)");
            SquaredCoherence.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
            SquaredCoherence.Plot.XAxis.MinorLogScale(true);
            SquaredCoherence.Plot.AddHorizontalLine(y: 0.0, color: System.Drawing.Color.Black, width: 1);
            SquaredCoherence.Plot.AddHorizontalLine(y: 1.0, color: System.Drawing.Color.Black, width: 1);
            SquaredCoherence.Plot.YAxis.SetBoundary(0.0, 1.0);
            SquaredCoherence.Configuration.DoubleClickBenchmark = false;
            SquaredCoherence.Refresh();

            // XX
            m_plotRhoaXX = ApparentResistivity.Plot.AddScatter(periods, m_logRhoaXX.ToArray(), System.Drawing.Color.Orange, lineStyle: LineStyle.Dot);
            m_plotRhoaErrorBarXX = ApparentResistivity.Plot.AddErrorBars(periods, m_logRhoaXX.ToArray(), null, null, m_errorLogRhoaXXPos.ToArray(), m_errorLogRhoaXXNeg.ToArray(), System.Drawing.Color.Orange);
            // XY
            m_plotRhoaXY = ApparentResistivity.Plot.AddScatter(periods, m_logRhoaXY.ToArray(), System.Drawing.Color.Red, lineStyle: LineStyle.Dot);
            m_plotRhoaErrorBarXY = ApparentResistivity.Plot.AddErrorBars(periods, m_logRhoaXY.ToArray(), null, null, m_errorLogRhoaXYPos.ToArray(), m_errorLogRhoaXYNeg.ToArray(), System.Drawing.Color.Red);
            // YX
            m_plotRhoaYX = ApparentResistivity.Plot.AddScatter(periods, m_logRhoaYX.ToArray(), System.Drawing.Color.Blue, lineStyle: LineStyle.Dot);
            m_plotRhoaErrorBarYX = ApparentResistivity.Plot.AddErrorBars(periods, m_logRhoaYX.ToArray(), null, null, m_errorLogRhoaYXPos.ToArray(), m_errorLogRhoaYXNeg.ToArray(), System.Drawing.Color.Blue);
            // YY
            m_plotRhoaYY = ApparentResistivity.Plot.AddScatter(periods, m_logRhoaYY.ToArray(), System.Drawing.Color.Green, lineStyle: LineStyle.Dot);
            m_plotRhoaErrorBarYY = ApparentResistivity.Plot.AddErrorBars(periods, m_logRhoaYY.ToArray(), null, null, m_errorLogRhoaYYPos.ToArray(), m_errorLogRhoaYYNeg.ToArray(), System.Drawing.Color.Green);
            ApparentResistivity.Plot.Title("Apparent resistivity (Ohm-m)");
            ApparentResistivity.Plot.XLabel("Period (sec)");
            ApparentResistivity.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
            ApparentResistivity.Plot.XAxis.MinorLogScale(true);
            ApparentResistivity.Plot.YAxis.TickLabelFormat(Util.logTickLabels);
            ApparentResistivity.Plot.YAxis.MinorLogScale(true);
            List<double> minLogRhoas = new List<double>();
            minLogRhoas.Add(m_logRhoaXX.Min());
            minLogRhoas.Add(m_logRhoaXY.Min());
            minLogRhoas.Add(m_logRhoaYX.Min());
            minLogRhoas.Add(m_logRhoaYY.Min());
            List<double> maxLogRhoas = new List<double>();
            maxLogRhoas.Add(m_logRhoaXX.Max());
            maxLogRhoas.Add(m_logRhoaXY.Max());
            maxLogRhoas.Add(m_logRhoaYX.Max());
            maxLogRhoas.Add(m_logRhoaYY.Max());
            ApparentResistivity.Plot.YAxis.SetBoundary(minLogRhoas.Min() - 1, maxLogRhoas.Max() + 1);
            ApparentResistivity.Configuration.DoubleClickBenchmark = false;
            ApparentResistivity.Refresh();

            // XX
            m_plotPhsXX = Phase.Plot.AddScatter(periods, m_phsXX.ToArray(), System.Drawing.Color.Orange, lineStyle: LineStyle.Dot);
            m_plotPhsErrorBarXX = Phase.Plot.AddErrorBars(periods, m_phsXX.ToArray(), null, m_errorPhsXX.ToArray(), System.Drawing.Color.Orange);
            // XY
            m_plotPhsXY = Phase.Plot.AddScatter(periods, m_phsXY.ToArray(), System.Drawing.Color.Red, lineStyle: LineStyle.Dot);
            m_plotPhsErrorBarXY = Phase.Plot.AddErrorBars(periods, m_phsXY.ToArray(), null, m_errorPhsXY.ToArray(), System.Drawing.Color.Red);
            // YX
            m_plotPhsYX = Phase.Plot.AddScatter(periods, m_phsYX.ToArray(), System.Drawing.Color.Blue, lineStyle: LineStyle.Dot);
            m_plotPhsErrorBarYX = Phase.Plot.AddErrorBars(periods, m_phsYX.ToArray(), null, m_errorPhsYX.ToArray(), System.Drawing.Color.Blue);
            // YY
            m_plotPhsYY = Phase.Plot.AddScatter(periods, m_phsYY.ToArray(), System.Drawing.Color.Green, lineStyle: LineStyle.Dot);
            m_plotPhsErrorBarYY = Phase.Plot.AddErrorBars(periods, m_phsYY.ToArray(), null, m_errorPhsYY.ToArray(), System.Drawing.Color.Green);
            Phase.Plot.YAxis.SetBoundary(-180, 180);
            Phase.Plot.YAxis.ManualTickSpacing(45);
            Phase.Plot.Title("Phase (deg)");
            Phase.Plot.XLabel("Period (sec)");
            Phase.Plot.XAxis.TickLabelFormat(Util.logTickLabels);
            Phase.Plot.XAxis.MinorLogScale(true);
            Phase.Configuration.DoubleClickBenchmark = false;
            Phase.Refresh();
        }

        private void CheckBoxEx_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotCoherenceEx = SquaredCoherence.Plot.AddScatter(logPeriods, m_coherenceX.ToArray(), color: System.Drawing.Color.Red);
                SquaredCoherence.Refresh();
            }
        }

        private void CheckBoxEx_UnChecked(object sender, RoutedEventArgs e)
        {
            SquaredCoherence.Plot.Remove(m_plotCoherenceEx);
            SquaredCoherence.Refresh();
        }

        private void CheckBoxEy_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotCoherenceEy = SquaredCoherence.Plot.AddScatter(logPeriods, m_coherenceY.ToArray(), color: System.Drawing.Color.Blue);
                SquaredCoherence.Refresh();
            }
        }

        private void CheckBoxEy_UnChecked(object sender, RoutedEventArgs e)
        {
            SquaredCoherence.Plot.Remove(m_plotCoherenceEy);
            SquaredCoherence.Refresh();
        }

        private void CheckBoxZxx_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotRhoaXX = ApparentResistivity.Plot.AddScatter(logPeriods, m_logRhoaXX.ToArray(), System.Drawing.Color.Orange, lineStyle: LineStyle.Dot);
                m_plotRhoaErrorBarXX = ApparentResistivity.Plot.AddErrorBars(logPeriods, m_logRhoaXX.ToArray(), null, null, m_errorLogRhoaXXPos.ToArray(), m_errorLogRhoaXXNeg.ToArray(), System.Drawing.Color.Orange);
                m_plotPhsXX = Phase.Plot.AddScatter(logPeriods, m_phsXX.ToArray(), System.Drawing.Color.Orange, lineStyle: LineStyle.Dot);
                m_plotPhsErrorBarXX = Phase.Plot.AddErrorBars(logPeriods, m_phsXX.ToArray(), null, m_errorPhsXX.ToArray(), System.Drawing.Color.Orange);
                ApparentResistivity.Refresh();
                Phase.Refresh();
            }
        }

        private void CheckBoxZxy_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotRhoaXY = ApparentResistivity.Plot.AddScatter(logPeriods, m_logRhoaXY.ToArray(), System.Drawing.Color.Red, lineStyle: LineStyle.Dot);
                m_plotRhoaErrorBarXY = ApparentResistivity.Plot.AddErrorBars(logPeriods, m_logRhoaXY.ToArray(), null, null, m_errorLogRhoaXYPos.ToArray(), m_errorLogRhoaXYNeg.ToArray(), System.Drawing.Color.Red);
                m_plotPhsXY = Phase.Plot.AddScatter(logPeriods, m_phsXY.ToArray(), System.Drawing.Color.Red, lineStyle: LineStyle.Dot);
                m_plotPhsErrorBarXY = Phase.Plot.AddErrorBars(logPeriods, m_phsXY.ToArray(), null, m_errorPhsXY.ToArray(), System.Drawing.Color.Red);
                ApparentResistivity.Refresh();
                Phase.Refresh();
            }
        }

        private void CheckBoxZyx_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotRhoaYX = ApparentResistivity.Plot.AddScatter(logPeriods, m_logRhoaYX.ToArray(), System.Drawing.Color.Blue, lineStyle: LineStyle.Dot);
                m_plotRhoaErrorBarYX = ApparentResistivity.Plot.AddErrorBars(logPeriods, m_logRhoaYX.ToArray(), null, null, m_errorLogRhoaYXPos.ToArray(), m_errorLogRhoaYXNeg.ToArray(), System.Drawing.Color.Blue);
                m_plotPhsYX = Phase.Plot.AddScatter(logPeriods, m_phsYX.ToArray(), System.Drawing.Color.Blue, lineStyle: LineStyle.Dot);
                m_plotPhsErrorBarYX = Phase.Plot.AddErrorBars(logPeriods, m_phsYX.ToArray(), null, m_errorPhsYX.ToArray(), System.Drawing.Color.Blue);
                ApparentResistivity.Refresh();
                Phase.Refresh();
            }
        }

        private void CheckBoxZyy_Checked(object sender, RoutedEventArgs e)
        {
            if (m_periods.Any())
            {
                double[] logPeriods = m_periods.Select(x => Math.Log10(x)).ToArray();
                m_plotRhoaYY = ApparentResistivity.Plot.AddScatter(logPeriods, m_logRhoaYY.ToArray(), System.Drawing.Color.Green, lineStyle: LineStyle.Dot);
                m_plotRhoaErrorBarYY = ApparentResistivity.Plot.AddErrorBars(logPeriods, m_logRhoaYY.ToArray(), null, null, m_errorLogRhoaYYPos.ToArray(), m_errorLogRhoaYYNeg.ToArray(), System.Drawing.Color.Green);
                m_plotPhsYY = Phase.Plot.AddScatter(logPeriods, m_phsYY.ToArray(), System.Drawing.Color.Green, lineStyle: LineStyle.Dot);
                m_plotPhsErrorBarYY = Phase.Plot.AddErrorBars(logPeriods, m_phsYY.ToArray(), null, m_errorPhsYY.ToArray(), System.Drawing.Color.Green);
                ApparentResistivity.Refresh();
                Phase.Refresh();
            }
        }

        private void CheckBoxZxx_UnChecked(object sender, RoutedEventArgs e)
        {
            ApparentResistivity.Plot.Remove(m_plotRhoaXX);
            ApparentResistivity.Plot.Remove(m_plotRhoaErrorBarXX);
            Phase.Plot.Remove(m_plotPhsXX);
            Phase.Plot.Remove(m_plotPhsErrorBarXX);
            ApparentResistivity.Refresh();
            Phase.Refresh();
        }

        private void CheckBoxZxy_UnChecked(object sender, RoutedEventArgs e)
        {
            ApparentResistivity.Plot.Remove(m_plotRhoaXY);
            ApparentResistivity.Plot.Remove(m_plotRhoaErrorBarXY);
            Phase.Plot.Remove(m_plotPhsXY);
            Phase.Plot.Remove(m_plotPhsErrorBarXY);
            ApparentResistivity.Refresh();
            Phase.Refresh();
        }

        private void CheckBoxZyx_UnChecked(object sender, RoutedEventArgs e)
        {
            ApparentResistivity.Plot.Remove(m_plotRhoaYX);
            ApparentResistivity.Plot.Remove(m_plotRhoaErrorBarYX);
            Phase.Plot.Remove(m_plotPhsYX);
            Phase.Plot.Remove(m_plotPhsErrorBarYX);
            ApparentResistivity.Refresh();
            Phase.Refresh();
        }

        private void CheckBoxZyy_UnChecked(object sender, RoutedEventArgs e)
        {
            ApparentResistivity.Plot.Remove(m_plotRhoaYY);
            ApparentResistivity.Plot.Remove(m_plotRhoaErrorBarYY);
            Phase.Plot.Remove(m_plotPhsYY);
            Phase.Plot.Remove(m_plotPhsErrorBarYY);
            ApparentResistivity.Refresh();
            Phase.Refresh();
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

        private void ApparentResistivity_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var copy = ApparentResistivity.Plot.Copy();
            copy.Title("Apparent resistivity (Ohm-m)");
            copy.XLabel("Period (sec)");
            copy.XAxis.TickLabelFormat(Util.logTickLabels);
            copy.XAxis.MinorLogScale(true);
            copy.XAxis.MajorGrid(true);
            copy.XAxis.MinorGrid(true);
            copy.YAxis.TickLabelFormat(Util.logTickLabels);
            copy.YAxis.MinorLogScale(true);
            List<double> minLogRhoas = new List<double>();
            minLogRhoas.Add(m_logRhoaXX.Min());
            minLogRhoas.Add(m_logRhoaXY.Min());
            minLogRhoas.Add(m_logRhoaYX.Min());
            minLogRhoas.Add(m_logRhoaYY.Min());
            List<double> maxLogRhoas = new List<double>();
            maxLogRhoas.Add(m_logRhoaXX.Max());
            maxLogRhoas.Add(m_logRhoaXY.Max());
            maxLogRhoas.Add(m_logRhoaYX.Max());
            maxLogRhoas.Add(m_logRhoaYY.Max());
            copy.YAxis.SetBoundary(minLogRhoas.Min() - 1, maxLogRhoas.Max() + 1);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(copy);
            wpfPlotViewer.Title = "Apparent resistivity";
            wpfPlotViewer.Show();
        }

        private void Phase_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var copy = Phase.Plot.Copy();
            copy.Title("Phase (deg)");
            copy.XLabel("Period (sec)");
            copy.XAxis.TickLabelFormat(Util.logTickLabels);
            copy.XAxis.MinorLogScale(true);
            copy.XAxis.MajorGrid(true);
            copy.XAxis.MinorGrid(true);
            copy.YAxis.SetBoundary(-180, 180);
            copy.YAxis.ManualTickSpacing(45);
            WpfPlotViewer wpfPlotViewer = new WpfPlotViewer(copy);
            wpfPlotViewer.Title = "Phase";
            wpfPlotViewer.Show();
        }
    }
}
