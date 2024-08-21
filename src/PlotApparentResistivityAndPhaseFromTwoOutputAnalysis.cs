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
    class PlotApparentResistivityAndPhaseFromTwoOutputAnalysis : PlotApparentResistivityAndPhase
    {
        public PlotApparentResistivityAndPhaseFromTwoOutputAnalysis()
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
            // Read csv file
            try {
                using (StreamReader reader = new StreamReader(@csvFileName))
                {
                    double errorFactor = 1.96;
                    m_periods.Clear();
                    m_coherenceX.Clear();
                    m_coherenceY.Clear();
                    m_logRhoaXX.Clear();
                    m_logRhoaXY.Clear();
                    m_logRhoaYX.Clear();
                    m_logRhoaYY.Clear();
                    m_errorLogRhoaXXPos.Clear();
                    m_errorLogRhoaXYPos.Clear();
                    m_errorLogRhoaYXPos.Clear();
                    m_errorLogRhoaYYPos.Clear();
                    m_errorLogRhoaXXNeg.Clear();
                    m_errorLogRhoaXYNeg.Clear();
                    m_errorLogRhoaYXNeg.Clear();
                    m_errorLogRhoaYYNeg.Clear();
                    m_phsXX.Clear();
                    m_phsXY.Clear();
                    m_phsYX.Clear();
                    m_phsYY.Clear();
                    m_errorPhsXX.Clear();
                    m_errorPhsXY.Clear();
                    m_errorPhsYX.Clear();
                    m_errorPhsYY.Clear();
                    string line = reader.ReadLine();// Read header
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        string[] values = line.Split(',');
                        double frequency = Convert.ToDouble(values[0]);
                        m_periods.Add(Convert.ToDouble(values[1]));
                        var Zxx = new Complex(Convert.ToDouble(values[2]), Convert.ToDouble(values[3]));
                        var Zxy = new Complex(Convert.ToDouble(values[4]), Convert.ToDouble(values[5]));
                        m_coherenceX.Add(Convert.ToDouble(values[6]));
                        var Zyx = new Complex(Convert.ToDouble(values[7]), Convert.ToDouble(values[8]));
                        var Zyy = new Complex(Convert.ToDouble(values[9]), Convert.ToDouble(values[10]));
                        m_coherenceY.Add(Convert.ToDouble(values[11]));
                        var dZxx = errorFactor * Convert.ToDouble(values[12]);
                        var dZxy = errorFactor * Convert.ToDouble(values[13]);
                        var dZyx = errorFactor * Convert.ToDouble(values[14]);
                        var dZyy = errorFactor * Convert.ToDouble(values[15]);
                        var rhoaXX = Util.calcApparentResistivity(frequency, Zxx);
                        var rhoaXY = Util.calcApparentResistivity(frequency, Zxy);
                        var rhoaYX = Util.calcApparentResistivity(frequency, Zyx);
                        var rhoaYY = Util.calcApparentResistivity(frequency, Zyy);
                        var errorRhoaXX = Util.calcApparentResistivityError(frequency, Zxx, dZxx);
                        var errorRhoaXY = Util.calcApparentResistivityError(frequency, Zxy, dZxy);
                        var errorRhoaYX = Util.calcApparentResistivityError(frequency, Zyx, dZyx);
                        var errorRhoaYY = Util.calcApparentResistivityError(frequency, Zyy, dZyy);
                        m_logRhoaXX.Add(Math.Log10(rhoaXX));
                        m_logRhoaXY.Add(Math.Log10(rhoaXY));
                        m_logRhoaYX.Add(Math.Log10(rhoaYX));
                        m_logRhoaYY.Add(Math.Log10(rhoaYY));
                        m_errorLogRhoaXXPos.Add(Math.Log10(rhoaXX + errorRhoaXX) - Math.Log10(rhoaXX));
                        m_errorLogRhoaXYPos.Add(Math.Log10(rhoaXY + errorRhoaXY) - Math.Log10(rhoaXY));
                        m_errorLogRhoaYXPos.Add(Math.Log10(rhoaYX + errorRhoaYX) - Math.Log10(rhoaYX));
                        m_errorLogRhoaYYPos.Add(Math.Log10(rhoaYY + errorRhoaYY) - Math.Log10(rhoaYY));
                        m_errorLogRhoaXXNeg.Add(rhoaXX - errorRhoaXX > 0 ? Math.Log10(rhoaXX) - Math.Log10(rhoaXX - errorRhoaXX) : 10);
                        m_errorLogRhoaXYNeg.Add(rhoaXY - errorRhoaXY > 0 ? Math.Log10(rhoaXY) - Math.Log10(rhoaXY - errorRhoaXY) : 10);
                        m_errorLogRhoaYXNeg.Add(rhoaYX - errorRhoaYX > 0 ? Math.Log10(rhoaYX) - Math.Log10(rhoaYX - errorRhoaYX) : 10);
                        m_errorLogRhoaYYNeg.Add(rhoaYY - errorRhoaYY > 0 ? Math.Log10(rhoaYY) - Math.Log10(rhoaYY - errorRhoaYY) : 10);
                        m_phsXX.Add(Util.calcPhase(Zxx));
                        m_phsXY.Add(Util.calcPhase(Zxy));
                        m_phsYX.Add(Util.calcPhase(Zyx));
                        m_phsYY.Add(Util.calcPhase(Zyy));
                        m_errorPhsXX.Add(Util.calcPhaseError(Zxx, dZxx));
                        m_errorPhsXY.Add(Util.calcPhaseError(Zxy, dZxy));
                        m_errorPhsYX.Add(Util.calcPhaseError(Zyx, dZyx));
                        m_errorPhsYY.Add(Util.calcPhaseError(Zyy, dZyy));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK);
                this.Close();
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
