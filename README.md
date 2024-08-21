# ELOG Field Analyzer
This software enables us to estimate the impedance tensor and tipper from the data measured by ELOG-MT, ELOG1K, and ELOG-DUAL in the field.

It utilizes TRACMT (https://github.com/yoshiya-usui/TRACMT) developed by Dr. Yoshiya Usui as the solver to estimate response functions.

# Operating Environment
.NET Framework 6.0 running Windows 10 or higher.

# Installation
You should install .NET 6.0 Desktop Runtime and Visual C++ redistributable libraries.

You can install this software using the installer “ELOGFieldAnalyzerSetup.exe”.

If the installer cannot work, please copy all files under the “bin” folder to an arbitrary install folder on your PC. 

# Functional overview
ELOG Field Analyzer has a lot of functions to analyze the data measured by ELOG series.

**Time Series Plot**
Even high sampling data (e.g., 1024 Hz sampling) can be visualized quickly.
You just have to click a button to move to the next day or the previous day.
Lowpass, highpass, and notch filters can be applied to time series.

**Power Spectrum Plot**
Power spectra of all channels can be plotted.
The frequency at the cursor position is easily identifiable.

**Response Function Estimation**
Power interface with TRACMT. You can control it by simple GUI operations.
All required information for the estimation can be inputted in a window.
Robust estimators, prewhitenting, and down-sampling are supported (of course, the ordinary lease square also can be used).

**Response Function Plot**
You can plot resultant response functions (impedance tensor and tipper) simply by pushing a button.

*Please send technical questions about this software to Dr. Yoshiya Usui (https://github.com/yoshiya-usui). 
