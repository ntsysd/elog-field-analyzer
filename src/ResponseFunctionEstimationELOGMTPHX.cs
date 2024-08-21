﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;

namespace PlotTimeSeries
{
    internal class ResponseFunctionEstimationELOGMTPHX : ResponseFunctionEstimation5Channels
    {
        public ResponseFunctionEstimationELOGMTPHX(string[] directoryName, string[] fileName, int samplingFrequency, int[] startIndex, int[] endIndex)
            : base(directoryName, fileName, samplingFrequency, startIndex, endIndex)
        {
            RadioButtonRRMS.IsHitTestVisible = false;
            RadioButtonRRMS.IsTabStop = false;
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
                if ((bool)RadioButtonImpedanceAndTipper.IsChecked)
                {
                    writer.WriteLine("3");
                }
                else if ((bool)RadioButtonImpedance.IsChecked)
                {
                    writer.WriteLine("2");
                }
                else
                {
                    writer.WriteLine("1");
                }
                writer.WriteLine("NUM_RR");
                writer.WriteLine(2);
                writer.WriteLine("SAMPLING_FREQ");
                writer.WriteLine(m_samplingFrequency);
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
                if ((bool)RadioButtonImpedanceAndTipper.IsChecked)
                {
                    writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                    writer.WriteLine("0.0");
                }
                else if ((bool)RadioButtonImpedance.IsChecked)
                {
                    writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                }
                else
                {
                    writer.WriteLine("0.0");
                }
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine(azimuth.ToString() + " " + (azimuth + 90.0).ToString());
                writer.WriteLine("ROTATION");
                writer.WriteLine(TextBoxRotation.Text);
                writer.WriteLine("ELOGMT_BINARY");
                writer.WriteLine("ELOGMT_READ_OPTION");
                if ((bool)RadioButtonImpedanceAndTipper.IsChecked)
                {
                    writer.WriteLine(0);
                }
                else if ((bool)RadioButtonImpedance.IsChecked)
                {
                    writer.WriteLine(1);
                }
                else
                {
                    writer.WriteLine(2);
                }
                writer.WriteLine("CAL_FILES");
                if ((bool)RadioButtonImpedanceAndTipper.IsChecked)
                {
                    MakeCalibrationFilesForExAndEy();
                    writer.WriteLine(m_calibrationFileForEx);
                    writer.WriteLine(m_calibrationFileForEy);
                    if (String.IsNullOrEmpty(TextBoxCoilCalHz.Text))
                    {
                        MessageBox.Show("Calibration file for Hz is not selected.", "Error", MessageBoxButton.OK);
                        return;
                    }
                    writer.WriteLine(TextBoxCoilCalHz.Text);
                }
                else if ((bool)RadioButtonImpedance.IsChecked)
                {
                    MakeCalibrationFilesForExAndEy();
                    writer.WriteLine(m_calibrationFileForEx);
                    writer.WriteLine(m_calibrationFileForEy);
                }
                else
                {
                    if (String.IsNullOrEmpty(TextBoxCoilCalHz.Text))
                    {
                        MessageBox.Show("Calibration file for Hz is not selected.", "Error", MessageBoxButton.OK);
                        return;
                    }
                    writer.WriteLine(TextBoxCoilCalHz.Text);
                }
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
                writer.WriteLine(TextBoxCoilCalHx.Text);
                writer.WriteLine(TextBoxCoilCalHy.Text);
                writer.WriteLine("PROCEDURE");
                if ((bool)RadioButtonRRMS.IsChecked)
                {
                    MessageBox.Show("RRMS estimator cannot be used for single-site data.", "Error", MessageBoxButton.OK);
                    return;
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
                if ((bool)RadioButtonImpedanceAndTipper.IsChecked || (bool)RadioButtonImpedance.IsChecked)
                {
                    for (int ch = 0; ch < 2; ch++)
                    {
                        writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], "*.dat"));
                        writer.WriteLine(m_startIndex[ch]);
                    }
                }
                if ((bool)RadioButtonImpedanceAndTipper.IsChecked || (bool)RadioButtonTipper.IsChecked)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[4], "*.dat"));
                    writer.WriteLine(m_startIndex[4]);
                }
                for (int ch = 2; ch < 4; ch++)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], "*.dat"));
                    writer.WriteLine(m_startIndex[ch]);
                }
                for (int ch = 2; ch < 4; ch++)
                {
                    writer.WriteLine(System.IO.Path.Combine(m_directoryName[ch], "*.dat"));
                    writer.WriteLine(m_startIndex[ch]);
                }
                writer.WriteLine("END");
            }
            WriteMessage("Input file was created: " + filePath);
        }
    }
}