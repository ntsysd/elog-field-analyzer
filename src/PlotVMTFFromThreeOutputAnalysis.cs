using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class PlotVMTFFromThreeOutputAnalysis : PlotVMTF
    {
        public PlotVMTFFromThreeOutputAnalysis()
        {
        }

        override public bool ReadCsvFile(string csvFileName)
        {
            if (!File.Exists(csvFileName))
            {
                MessageBox.Show("There is not result csv file.", "Error", MessageBoxButton.OK);
                this.Close();
                return false;
            }
            try
            {
                // Read csv file
                using (StreamReader reader = new StreamReader(@csvFileName))
                {
                    double errorFactor = 1.96;
                    m_periods.Clear();
                    m_coherenceHz.Clear();
                    m_ReTzx.Clear();
                    m_ImTzx.Clear();
                    m_ReTzy.Clear();
                    m_ImTzy.Clear();
                    m_errorTzx.Clear();
                    m_errorTzy.Clear();
                    string line = reader.ReadLine();// Read header
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        string[] values = line.Split(',');
                        double frequency = Convert.ToDouble(values[0]);
                        m_periods.Add(Convert.ToDouble(values[1]));
                        m_ReTzx.Add(Convert.ToDouble(values[12]));
                        m_ImTzx.Add(Convert.ToDouble(values[13]));
                        m_ReTzy.Add(Convert.ToDouble(values[14]));
                        m_ImTzy.Add(Convert.ToDouble(values[15]));
                        m_coherenceHz.Add(Convert.ToDouble(values[16]));
                        m_errorTzx.Add(errorFactor * Convert.ToDouble(values[21]));
                        m_errorTzy.Add(errorFactor * Convert.ToDouble(values[22]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK); this.Close();

                return false;
            }
            if (!m_periods.Any())
            {
                MessageBox.Show("Number of frequencies is zero.", "Error", MessageBoxButton.OK);
                this.Close();
                return false;
            }
            return true;
        }
    }

}
