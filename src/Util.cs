using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace PlotTimeSeries
{
    public static class Util
    {
        public enum TypeOfResponseFunctions
        {
            NONE = -1,
            IMPEDANCE_TENSOR_AND_TIPPER = 0,
            IMPEDANCE_TENSOR,
            TIPPER,
        }

        public static readonly int DAYS_TO_SECONDS = 24 * 3600;

        public static string logTickLabels(double x) => Math.Pow(10, x).ToString("G2");

        static public bool ReadATSFile(string atsFileName, ref double[] values, out DateTime startDateTime, out TimeSpan timeSpan, out int samplingFrequnecy)
        {
            samplingFrequnecy = 0;
            startDateTime = DateTime.MinValue;
            timeSpan = TimeSpan.Zero;
            if (String.IsNullOrEmpty(atsFileName))
            {
                return false;
            }
            try
            {
                using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(atsFileName)))
                {
                    byte[] buff2bytes = new byte[2];
                    byte[] buff4bytes = new byte[4];
                    byte[] buff8bytes = new byte[8];
                    reader.Read(buff2bytes, 0, 2);
                    Int16 headerLength = BytesToInt16(buff2bytes);
                    reader.Read(buff2bytes, 0, 2);
                    Int16 headerVers = BytesToInt16(buff2bytes);
                    reader.Read(buff4bytes, 0, 4);
                    Int32 numSamples = BytesToInt32(buff4bytes);
                    if (numSamples <= 0)
                    {
                        MessageBox.Show("サンプル数がゼロ以下です", "Error", MessageBoxButton.OK);
                        return false;
                    }
                    reader.Read(buff4bytes, 0, 4);
                    Single samplingFreq = BytesToSingle(buff4bytes);
                    samplingFrequnecy = Convert.ToInt32(samplingFreq);
                    if(samplingFrequnecy < 0)
                    {
                        MessageBox.Show("サンプリング周波数が負です", "Error", MessageBoxButton.OK);
                        return false;
                    }
                    reader.Read(buff4bytes, 0, 4);
                    Int32 startDate = BytesToInt32(buff4bytes);
                    reader.Read(buff8bytes, 0, 8);
                    Double lsbmv = BytesToDouble(buff8bytes);
                    reader.ReadBytes(headerLength - 24);
                    byte[] data = new byte[numSamples * 4];
                    if (values == null || values.Length != numSamples)
                    {
                        values = new double[numSamples];
                    }
                    reader.Read(data, 0, numSamples * 4);
                    for (int i = 0; i < numSamples; ++i)
                    {
                        Int32 lbuf = BytesToInt32(data[i * 4], data[i * 4 + 1], data[i * 4 + 2], data[i * 4 + 3]);
                        values[i] = (double)lbuf * lsbmv;
                    }
                    startDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
                    TimeSpan shift = new TimeSpan(0, 0, 0, startDate);
                    startDateTime += shift;
                    int spanMilliSeconds = numSamples / samplingFrequnecy * 1000;
                    timeSpan = new TimeSpan(0, 0, 0, 0, spanMilliSeconds);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        static public bool ReadTextFile(string fileName, double samplingFrequnecy, ref double[] values, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }
            try
            {
                int numSamples = 0;
                using (StreamReader sr = new StreamReader(fileName))
                {                    
                    while (!sr.EndOfStream)
                    {
                        sr.ReadLine();
                        numSamples++;
                    }
                }
                if (numSamples <= 0)
                {
                    MessageBox.Show("サンプル数がゼロ以下です", "Error", MessageBoxButton.OK);
                    return false;
                }
                int spanMilliSeconds = (int)(numSamples * 1000.0 / samplingFrequnecy);
                timeSpan = new TimeSpan(0, 0, 0, 0, spanMilliSeconds);
                if (values == null || values.Length != numSamples)
                {
                    values = new double[numSamples];
                }
                int icount = 0;
                string line;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        values[icount] = double.Parse(line);
                        icount++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        static public bool ReadOneELOGDualFile(string datFileName, int offset, int samplingFrequency, ref double[] valuesEx, ref double[] valuesEy)
        {
            if (String.IsNullOrEmpty(datFileName))
            {
                return false;
            }
            try
            {
                using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(datFileName)))
                {
                    int AD_CH = 2;
                    int AD_BYTES = 3;
                    int dataSize = AD_CH * samplingFrequency * AD_BYTES;
                    byte[] data = new byte[dataSize];
                    int counter = 0;
                    double factor = 2500.0 / Math.Pow(2, 23);// Dynamic range +/- 2500 mV
                    while (true)
                    {
                        // Read header of each block
                        if (reader.Read(data, 0, 3) == 0)
                        {
                            break;
                        }
                        // Read AD data of each block
                        if (reader.Read(data, 0, dataSize) == 0)
                        {
                            break;
                        }
                        for (int j = 0; j < samplingFrequency; ++j, ++counter)
                        {
                            // Ex
                            int lbufx = Util.BytesToInt32(data[j * AD_BYTES * AD_CH], data[j * AD_BYTES * AD_CH + 1], data[j * AD_BYTES * AD_CH + 2]);
                            valuesEx[counter + offset] = lbufx * factor;
                            // Ey
                            int lbufy = Util.BytesToInt32(data[j * AD_BYTES * AD_CH + 3], data[j * AD_BYTES * AD_CH + 4], data[j * AD_BYTES * AD_CH + 5]);
                            valuesEy[counter + offset] = lbufy * factor;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        static public bool ReadOneELOGMTFile(string datFileName, int offset, int samplingFrequency, ref double[] valuesEx, ref double[] valuesEy, ref double[] valuesHx, ref double[] valuesHy, ref double[] valuesHz)
        {
            if (String.IsNullOrEmpty(datFileName))
            {
                return false;
            }
            try
            {
                using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(datFileName)))
                {
                    int AD_CH = 5;
                    int AD_BYTES = 3;
                    double MV_LSB_E = 2500.0 / 8388608.0;
                    double MV_LSB_H = 10000.0 / 8388608.0;
                    int dataSize = AD_CH * samplingFrequency * AD_BYTES;
                    byte[] data = new byte[dataSize];
                    int counter = 0;
                    double factor = 2500.0 / Math.Pow(2, 23);// Dynamic range +/- 2500 mV
                    while (true)
                    {
                        // Read header of each block
                        if (reader.Read(data, 0, 3) == 0)
                        {
                            break;
                        }
                        // Read AD data of each block
                        if (reader.Read(data, 0, dataSize) == 0)
                        {
                            break;
                        }
                        for (int j = 0; j < samplingFrequency; ++j, ++counter)
                        {
                            int lbuf = 0;
                            // Ex
                            lbuf = Util.BytesToInt32(data[j * AD_BYTES * AD_CH               ], data[j * AD_BYTES * AD_CH                + 1], data[j * AD_BYTES * AD_CH                + 2]);
                            valuesEx[counter + offset] = lbuf * MV_LSB_E;
                            // Ey
                            lbuf = Util.BytesToInt32(data[j * AD_BYTES * AD_CH + AD_BYTES    ], data[j * AD_BYTES * AD_CH + AD_BYTES     + 1], data[j * AD_BYTES * AD_CH + AD_BYTES     + 2]);
                            valuesEy[counter + offset] = lbuf * MV_LSB_E;
                            // Hx
                            lbuf = Util.BytesToInt32(data[j * AD_BYTES * AD_CH + AD_BYTES * 2], data[j * AD_BYTES * AD_CH + AD_BYTES * 2 + 1], data[j * AD_BYTES * AD_CH + AD_BYTES * 2 + 2]);
                            valuesHx[counter + offset] = lbuf * MV_LSB_H;
                            // Hy
                            lbuf = Util.BytesToInt32(data[j * AD_BYTES * AD_CH + AD_BYTES * 3], data[j * AD_BYTES * AD_CH + AD_BYTES * 3 + 1], data[j * AD_BYTES * AD_CH + AD_BYTES * 3 + 2]);
                            valuesHy[counter + offset] = lbuf * MV_LSB_H;
                            // Hz
                            lbuf = Util.BytesToInt32(data[j * AD_BYTES * AD_CH + AD_BYTES * 4], data[j * AD_BYTES * AD_CH + AD_BYTES * 4 + 1], data[j * AD_BYTES * AD_CH + AD_BYTES * 4 + 2]);
                            valuesHz[counter + offset] = lbuf * MV_LSB_H;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        static public bool ReadOneELOGMTFileHxHyOnly(string datFileName, int offset, int samplingFrequency, ref double[] valuesHx, ref double[] valuesHy)
        {
            if (String.IsNullOrEmpty(datFileName))
            {
                return false;
            }
            try
            {
                using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(datFileName)))
                {
                    int AD_CH = 5;
                    int AD_BYTES = 3;
                    double MV_LSB_E = 2500.0 / 8388608.0;
                    double MV_LSB_H = 10000.0 / 8388608.0;
                    int dataSize = AD_CH * samplingFrequency * AD_BYTES;
                    byte[] data = new byte[dataSize];
                    int counter = 0;
                    double factor = 2500.0 / Math.Pow(2, 23);// Dynamic range +/- 2500 mV
                    while (true)
                    {
                        // Read header of each block
                        if (reader.Read(data, 0, 3) == 0)
                        {
                            break;
                        }
                        // Read AD data of each block
                        if (reader.Read(data, 0, dataSize) == 0)
                        {
                            break;
                        }
                        for (int j = 0; j < samplingFrequency; ++j, ++counter)
                        {
                            int lbuf = 0;
                            // Hx
                            lbuf = Util.BytesToInt32(data[j * AD_BYTES * AD_CH + AD_BYTES * 2], data[j * AD_BYTES * AD_CH + AD_BYTES * 2 + 1], data[j * AD_BYTES * AD_CH + AD_BYTES * 2 + 2]);
                            valuesHx[counter + offset] = lbuf * MV_LSB_H;
                            // Hy
                            lbuf = Util.BytesToInt32(data[j * AD_BYTES * AD_CH + AD_BYTES * 3], data[j * AD_BYTES * AD_CH + AD_BYTES * 3 + 1], data[j * AD_BYTES * AD_CH + AD_BYTES * 3 + 2]);
                            valuesHy[counter + offset] = lbuf * MV_LSB_H;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File open error.", "Error", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        static public Int32 BytesToInt32(byte b0, byte b1, byte b2)
        {
            byte sign = (byte)(b2 & 0b1000_0000);
            // Sign bit is 1 => negative value
            byte b3 = 0b1111_1111;
            if (sign == 0b0000_0000)
            {
                // Sign bit is 0 => positive value
                b3 = 0;
            }
            byte[] bytes = { b0, b1, b2, b3 };
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Int32 result = BitConverter.ToInt32(bytes, 0);
            return result;
        }
        static public Int32 BytesToInt32(byte b0, byte b1, byte b2, byte b3)
        {
            byte[] bytes = { b0, b1, b2, b3 };
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Int32 result = BitConverter.ToInt32(bytes, 0);
            return result;
        }
        static public Int16 BytesToInt16(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Int16 result = BitConverter.ToInt16(bytes, 0);
            return result;
        }
        static public Int32 BytesToInt32(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Int32 result = BitConverter.ToInt32(bytes, 0);
            return result;
        }
        static public Single BytesToSingle(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Single result = BitConverter.ToSingle(bytes, 0);
            return result;
        }
        static public Double BytesToDouble(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Double result = BitConverter.ToDouble(bytes, 0);
            return result;
        }
        static public void FourierTransform(Complex[] cdata)
        {
            int num = cdata.Length;
            int j = 0;
            for (int i = 0; i < num; ++i)
            {
                if (j > i)
                {
                    Complex temp = cdata[j];
                    cdata[j] = cdata[i];
                    cdata[i] = temp;
                }
                int m = num / 2;
                while (j >= m)
                {
                    j -= m;
                    m /= 2;
                    if (m < 2)
                    {
                        break;
                    }
                }
                j += m;
            }

            int kmax = 1;
            int isign = -1;
            while (num > kmax)
            {
                int istep = kmax * 2;
                for (int k = 0; k < kmax; ++k)
                {
                    double theta = Math.PI * (double)(isign * k) / (double)kmax;
                    for (int i = k; i < num; i += istep)
                    {
                        Complex temp = new Complex(Math.Cos(theta), Math.Sin(theta));
                        int m = i + kmax;
                        temp *= cdata[m];
                        cdata[m] = cdata[i] - temp;
                        cdata[i] = cdata[i] + temp;
                    }
                }
                kmax = istep;
            }

            double factor = 1.0 / (double)num;
            for (int i = 0; i < num; ++i)
            {
                cdata[i] *= factor;
            }
        }
        static public void WriteToSettingIni(string keyword, string value)
        {
            var filePath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "setting.ini");
            if (File.Exists(filePath))
            {
                List<string> writeLines = new List<string>();
                string[] readLines = File.ReadAllLines(filePath);
                foreach (string readLine in readLines)
                {
                    var splited = readLine.Split(",");
                    if (!splited[0].Equals(keyword))
                    {
                        writeLines.Add(readLine);
                    }
                }
                writeLines.Add(keyword + "," + value);
                File.WriteAllLines(filePath, writeLines);
            }
            else
            {
                // Create setting.ini
                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(keyword + "," + value);
                }

            }
        }

        static public string ReadFromSettingIni(string keyword)
        {
            var filePath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "setting.ini");
            if (File.Exists(filePath))
            {
                List<string> writeLines = new List<string>();
                string[] readLines = File.ReadAllLines(filePath);
                foreach (string readLine in readLines)
                {
                    var splited = readLine.Split(",");
                    if (splited[0].Equals(keyword))
                    {
                        return splited[1];
                    }
                }
            }
            return null;
        }

        // Calculate apparent resistivity
        static public double calcApparentResistivity(double freq, Complex Z)
        {
            return 0.2 * Z.Magnitude * Z.Magnitude / freq;
        }

        // Calculate error of apparent resistivity
        static public double calcApparentResistivityError(double freq, Complex Z, double dZ)
        {
            return 0.4 * Z.Magnitude * dZ / freq;
        }

        // Calculate phase
        static public double calcPhase(Complex Z)
        {
            return Z.Phase * 180.0 / Math.PI;
        }

        // Calculate error of phase
        static public double calcPhaseError(Complex Z, double dZ)
        {
            double ratio = dZ / Z.Magnitude;
            if (ratio < 1.0)
            {
                return Math.Asin(ratio) * 180.0 / Math.PI;
            }
            else
            {
                return 360.0;
            }
        }

        // Apply high-pass filter
        static public void applyHighPassFilter( double samplingFreq, double cutoffFreq, int numData, double[] data ){
            double[] dataOrg = new double[numData];
            double lamda = Math.Tan(Math.PI * cutoffFreq / samplingFreq);
            double beta = 1.0 / (lamda + 1.0);
            double alpha = (lamda - 1.0) / (lamda + 1.0);
	        for(int i = 0; i<numData; ++i ){
		        dataOrg[i] = data[i];
	        }
            data[0] = dataOrg[0];
	        double yPre1 = dataOrg[0];
	        for(int i = 1; i<numData; ++i ){
		        data[i] = beta* dataOrg[i] - beta* dataOrg[i - 1] - alpha* yPre1;
                yPre1 = data[i];
	        }
        }

        // Apply low-pass filter
        static public void applyLowPassFilter( double samplingFreq, double cutoffFreq, int numData, double[] data ){
	        double[] dataOrg = new double[numData];
            double Q = 1.0 / Math.Sqrt(2.0);
            double lamda = Math.Tan(Math.PI * cutoffFreq / samplingFreq);
            double delta = lamda * lamda + lamda / Q + 1.0;
            double beta = lamda * lamda / delta;
            double alpha1 = 2.0 * (lamda * lamda - 1.0) / delta;
            double alpha2 = 1.0 - 2.0 * lamda / Q / delta;
            for (int i = 0; i < numData; ++i)
            {
                dataOrg[i] = data[i];
            }
            data[0] = dataOrg[0];
            data[1] = dataOrg[1];
            double yPre2 = dataOrg[0];// 2-2=0
            double yPre1 = dataOrg[1];// 2-1=1
            for (int i = 2; i < numData; ++i)
            {
                data[i] = beta * dataOrg[i] + 2.0 * beta * dataOrg[i - 1] + beta * dataOrg[i - 2] - alpha1 * yPre1 - alpha2 * yPre2;
                yPre2 = yPre1;
                yPre1 = data[i];
            }
        }

        // Apply notch filter
        static public void applyNotchFilter( double samplingFreq, double cutoffFreq, int numData, double[] data ){
            double[] dataOrg = new double[numData];
            double Q = 1.0 / Math.Sqrt(2.0); 
            double lamda = Math.Tan(Math.PI * cutoffFreq / samplingFreq);
            double delta = lamda * lamda + lamda / Q + 1.0;
            double beta0 = (lamda * lamda + 1.0) / delta;
            double beta1 = 2.0 * (lamda * lamda - 1.0) / delta;
            double beta2 = beta0;
            double alpha1 = beta1;
            double alpha2 = 1.0 - 2.0 * lamda / Q / delta;
            for (int i = 0; i < numData; ++i)
            {
                dataOrg[i] = data[i];
            }
            data[0] = dataOrg[0];
            data[1] = dataOrg[1];
            double yPre2 = dataOrg[0];// 2-2=0
            double yPre1 = dataOrg[1];// 2-1=1
            for (int i = 2; i < numData; ++i)
            {
                data[i] = beta0 * dataOrg[i] + beta1 * dataOrg[i - 1] + beta2 * dataOrg[i - 2] - alpha1 * yPre1 - alpha2 * yPre2;
                yPre2 = yPre1;
                yPre1 = data[i];
            }
        }

        static public string countupRunNumberOfATS(string currentFileName)
        {
            int index = currentFileName.IndexOf("_R");
            if(index < 0)
            {
                return null;
            }
            string currentRunNumber = currentFileName.Substring(index + 2, 3);
            int runNumber = -1;
            if (!int.TryParse(currentRunNumber, out runNumber))
            {
                return null;
            }
            runNumber += 1;
            string nextRunNumber = runNumber.ToString("000");
            string nextFileName = currentFileName.Replace("_R" + currentRunNumber, "_R" + nextRunNumber);
            return nextFileName;
        }

        static public string countdownRunNumberOfATS(string currentFileName)
        {
            int index = currentFileName.IndexOf("_R");
            if (index < 0)
            {
                return null;
            }
            string currentRunNumber = currentFileName.Substring(index + 2, 3);
            int runNumber = -1;
            if(!int.TryParse(currentRunNumber, out runNumber)){
                return null;
            }
            runNumber -= 1;
            string previousRunNumber = runNumber.ToString("000");
            string previousFileName = currentFileName.Replace("_R" + currentRunNumber, "_R" + previousRunNumber);
            return previousFileName;
        }

        static public DateTime ConvELOGFileNameToDateTime(string datFileName)
        {
            var path = @datFileName;
            string fileName = System.IO.Path.GetFileName(path);
            int year = int.Parse(fileName.Substring(0, 4));
            int month = int.Parse(fileName.Substring(4, 2));
            int day = int.Parse(fileName.Substring(6, 2));
            int hour = int.Parse(fileName.Substring(9, 2));
            int miniute = int.Parse(fileName.Substring(11, 2));
            int second = int.Parse(fileName.Substring(13, 2));
            return new DateTime(year, month, day, hour, miniute, second);
        }

        static public string GetDateStringOfYesterday(string yyyMMdd) {
            DateTime currentDate = GetDateFromString(yyyMMdd);
            DateTime yesterday = currentDate.AddDays(-1);
            return yesterday.ToString("yyyyMMdd");
        }

        static public string GetDateStringOfTomorrow(string yyyyMMdd)
        {
            DateTime currentDate = GetDateFromString(yyyyMMdd);
            DateTime tomorrow = currentDate.AddDays(1);
            return tomorrow.ToString("yyyyMMdd");
        }

        static public DateTime GetDateFromString(string yyyyMMdd)
        {
            int year = int.Parse(yyyyMMdd.Substring(0, 4));
            int month = int.Parse(yyyyMMdd.Substring(4, 2));
            int day = int.Parse(yyyyMMdd.Substring(6, 2));
            return new DateTime(year, month, day);
        }

        public static string SubstringRight(string str, int length)
        {
            if (length < 0)
            {
                return null;
            }
            if (str == null)
            {
                return null;
            }
            if (str.Length <= length)
            {
                return str;
            }
            return str.Substring(str.Length - length, length);
        }

        // Calculate frequency characteristics of IIR high-pass filter
        public static Complex calculateFrequencyCharacteristicsOfIIRHighPassFilter( double freq,  double samplingFrequency,  double cutoffFreq ){
            double omega = 2.0 * Math.PI * freq / samplingFrequency;
            double lamda = Math.Tan(Math.PI * cutoffFreq / samplingFrequency);
            double beta = 1.0 / (lamda + 1.0);
            double alpha = (lamda - 1.0) / (lamda + 1.0);
            double arg = -omega;
            Complex numerator = new Complex(beta, 0.0);
            numerator -= new Complex(Math.Cos(arg) * beta, Math.Sin(arg) * beta);
            Complex denominator = new Complex(1.0, 0.0);
            denominator += new Complex(Math.Cos(arg) * alpha, Math.Sin(arg) * alpha);
	        return numerator / denominator;
        }

        // Calculate frequency characteristics of IIR low-pass filter
        public static Complex calculateFrequencyCharacteristicsOfIIRLowPassFilter(  double freq,  double samplingFrequency,  double cutoffFreq){
            double Q = 1.0 / Math.Sqrt(2.0);
            double omega = 2.0 * Math.PI * freq / samplingFrequency;
            double lamda = Math.Tan(Math.PI * cutoffFreq / samplingFrequency);
            double delta = lamda * lamda + lamda / Q + 1.0;
            double beta0 = lamda * lamda / delta;
            double alpha1 = 2.0 * (lamda * lamda - 1.0) / delta;
            double alpha2 = 1.0 - 2.0 * lamda / Q / delta;
            double[] beta = { 2.0 * beta0, beta0 };
            double[] alpha = { alpha1, alpha2 };
            Complex numerator = new Complex(beta0, 0.0);
            for (int i = 0; i < 2; ++i)
            {
                double arg = - (double)(i + 1) * omega;
                numerator += new Complex(Math.Cos(arg) * beta[i], Math.Sin(arg) * beta[i]);
            }
            Complex  denominator = new Complex(1.0, 0.0);
            for (int i = 0; i < 2; ++i)
            {
                double arg = - (double)(i + 1) * omega;
                denominator += new Complex(Math.Cos(arg) * alpha[i], Math.Sin(arg) * alpha[i]);
            }
            return numerator / denominator;
        }

        // Calculate frequency characteristics of notch filter
        public static Complex calculateFrequencyCharacteristicsOfNotchFilter( double freq, double samplingFrequency, double cutoffFreq)
        {
            double Q = 1.0 / Math.Sqrt(2.0);
            double omega = 2.0 * Math.PI * freq / samplingFrequency;
            double lamda = Math.Tan(Math.PI * cutoffFreq / samplingFrequency);
            double delta = lamda * lamda + lamda / Q + 1.0;
            double beta0 = (lamda * lamda + 1.0) / delta;
            double beta1 = 2.0 * (lamda * lamda - 1.0) / delta;
            double beta2 = beta0;
            double alpha1 = beta1;
            double alpha2 = 1.0 - 2.0 * lamda / Q / delta;
            double[] beta = { beta1, beta2 };
            double[] alpha = { alpha1, alpha2 };
            Complex numerator = new Complex(beta0, 0.0);
            for (int i = 0; i < 2; ++i)
            {
                double arg = -(double)(i + 1) * omega;
                numerator += new Complex(Math.Cos(arg) * beta[i], Math.Sin(arg) * beta[i]);
            }
            Complex denominator = new Complex(1.0, 0.0);
            for (int i = 0; i < 2; ++i)
            {
                double arg = -(double)(i + 1) * omega;
                denominator += new Complex(Math.Cos(arg) * alpha[i], Math.Sin(arg) * alpha[i]);
            }

            return numerator / denominator;

        }

        // Apply Hanning window
        public static void HanningWindow(int num, ref double[] data)
        {
            for (int i = 0; i < num; ++i)
            {
                double ratio = (double)(i) / (double)(num - 1);
                data[i] *= 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * ratio));
            }
        }

        public static void HanningWindow( int num, ref Complex[] data )
        {
	        for(int i = 0; i < num; ++i ){
		        double ratio = (double)(i) / (double)(num - 1);
                data[i] *= 0.5 * ( 1.0 - Math.Cos(2.0 * Math.PI * ratio) );
	        }
        }

    }
}
