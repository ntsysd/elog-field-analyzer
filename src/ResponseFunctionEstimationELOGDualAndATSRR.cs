﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class ResponseFunctionEstimationELOGDualAndATSRR : ResponseFunctionEstimationELOGDualAndATS
    {

        public ResponseFunctionEstimationELOGDualAndATSRR(string[] directoryName, string[] fileName, int samplingFrequency, int[] startIndex, int[] endIndex, bool isELOG1K)
            : base(directoryName, fileName, samplingFrequency, startIndex, endIndex, isELOG1K)
        {
            m_numOfChannels = 6;
            RadioButtonRRMS.IsHitTestVisible = true;
            RadioButtonRRMS.IsTabStop = true;
        }

        override protected void MakeInputFileForTRACMT()
        {
            bool isSuccess = false;
            int totalDataLength = 0;
            List<int> segmentLengths = DetermineSegmentLengths(out isSuccess, out totalDataLength);
            if (!isSuccess)
            {
                return;
            }
            if (String.IsNullOrEmpty(TextBoxWorkFolder.Text))
            {
                MessageBox.Show("No work folder is selected.", "Error", MessageBoxButton.OK);
                return;
            }
            var filePath = System.IO.Path.Combine(TextBoxWorkFolder.Text, "param.dat");
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("NUM_OUT");
                writer.WriteLine("2");
                writer.WriteLine("NUM_RR");
                writer.WriteLine(2);
                writer.WriteLine("SAMPLING_FREQ");
                writer.WriteLine(m_samplingFrequencyInt);
                writer.WriteLine("NUM_SECTION");
                writer.WriteLine(1);
                writer.WriteLine("SEGMENT");
                writer.WriteLine(segmentLengths.Count);
                foreach (var segmentLength in segmentLengths)
                {
                    if ((bool)CheckBoxPrewhitening.IsChecked)
                    {
                        writer.WriteLine(segmentLength.ToString() + " 2 3 4");
                    }
                    else
                    {
                        writer.WriteLine(segmentLength.ToString() + " 2 8 12");
                    }
                }
                if ((bool)CheckBoxDownsampling.IsChecked)
                {
                    writer.WriteLine("DECIMATION");
                    writer.WriteLine(Convert.ToInt32(TextBoxDownsamplingRate.Text));
                    writer.WriteLine(100);
                    writer.WriteLine(0.5);
                }
                writer.WriteLine("OVERLAP");
                writer.WriteLine("0.5");
                writer.WriteLine("AZIMUTH");
                double azimuth = double.Parse(TextBoxDecliniation.Text);
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine("ROTATION");
                writer.WriteLine(TextBoxRotation.Text);
                writer.WriteLine("ATS_BINARY");
                writer.WriteLine("MFS_CAL");
                writer.WriteLine(TextBoxDipoleLengthNS.Text);
                writer.WriteLine(TextBoxDipoleLengthEW.Text);
                if (String.IsNullOrEmpty(TextBoxCoilCalHx.Text))
                {
                    MessageBox.Show("Calibration file for Hx is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                if (String.IsNullOrEmpty(TextBoxCoilCalHy.Text))
                {
                    MessageBox.Show("Calibration file for Hy is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                writer.WriteLine(TextBoxCoilCalHx.Text);
                writer.WriteLine(TextBoxCoilCalHy.Text);
                writer.WriteLine(1);
                writer.WriteLine(1);
                writer.WriteLine("ELOGDUAL_BINARY");
                writer.WriteLine("ELOGDUAL_CAL");
                if (m_isELOG1K)
                {
                    writer.WriteLine(0);
                }
                else
                {
                    writer.WriteLine(1);
                }
                if (String.IsNullOrEmpty(TextBoxELOGCal.Text))
                {
                    MessageBox.Show("ELOG calibration file is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                writer.WriteLine(TextBoxELOGCal.Text);
                writer.WriteLine("5.17");
                if (m_isELOG1K)
                {
                    writer.WriteLine("ELOGDUAL_READ_OPTION");
                    writer.WriteLine(1);
                }
                writer.WriteLine("LOGGER_CAL_DIRECTORY");
                if (String.IsNullOrEmpty(TextBoxLoggerCalFilesFolder.Text))
                {
                    MessageBox.Show("Folder storing logger calibration files is not selected.", "Error", MessageBoxButton.OK);
                    return;
                }
                writer.WriteLine(TextBoxLoggerCalFilesFolder.Text);
                writer.WriteLine("PROCEDURE");
                if ((bool)RadioButtonRRMS.IsChecked)
                {
                    writer.WriteLine(1);
                }
                else
                {
                    writer.WriteLine(0);
                    writer.WriteLine("MESTIMATORS");
                    if ((bool)RadioButtonOLS.IsChecked)
                    {
                        writer.WriteLine(-1);
                        writer.WriteLine(-1);
                    }
                    else if ((bool)RadioButtonMestimator.IsChecked)
                    {
                        writer.WriteLine(0);
                        writer.WriteLine(1);
                    }
                }
                writer.WriteLine("ERROR_ESTIMATION");
                writer.WriteLine(0);
                if ((bool)CheckBoxPrewhitening.IsChecked)
                {
                    writer.WriteLine("PREWHITENING");
                    writer.WriteLine(0);
                    writer.WriteLine(8);
                    writer.WriteLine(1);
                }
                if (AppliedFilter.getNumberOfHighPassFilters() > 0)
                {
                    writer.WriteLine("HIGH_PASS");
                    writer.WriteLine(AppliedFilter.getHighestCuttoffFrequencyOfHighPassFilters());
                }
                if (AppliedFilter.getNumberOfLowPassFilters() > 0)
                {
                    writer.WriteLine("LOW_PASS");
                    writer.WriteLine(AppliedFilter.getLowestCuttoffFrequencyOfLowPassFilters());
                }
                if (AppliedFilter.getNumberOfNotchFilters() > 0)
                {
                    writer.WriteLine("NOTCH");
                    writer.WriteLine(AppliedFilter.getNumberOfNotchFilters());
                    double[] cutoffFrequencies = AppliedFilter.getCuttoffFrequenciesOfNotchFilters();
                    foreach (double frequency in cutoffFrequencies)
                    {
                        writer.WriteLine(frequency);
                    }
                }
                writer.WriteLine("DATA_FILES");
                writer.WriteLine(totalDataLength);
                if (String.IsNullOrEmpty(m_directoryName[0]))
                {
                    MessageBox.Show("ELOG data folder is not specified.", "Error", MessageBoxButton.OK);
                    return;
                }
                for (int ch = 0; ch < 2; ch++)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], "*.dat"));
                    writer.WriteLine(m_startIndex[ch]);
                }
                for (int ch = 2; ch < 6; ch++)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], m_fileName[ch]));
                    writer.WriteLine(m_startIndex[ch]);
                }
                writer.WriteLine("END");
            }
            WriteMessage("Input file was created: " + filePath);
        }
    }
}
