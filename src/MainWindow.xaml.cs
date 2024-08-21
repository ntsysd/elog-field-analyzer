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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlotTimeSeries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title += "   " + Common.VERSION;
        }

        private void SelectType_Click(object sender, RoutedEventArgs e)
        {
            if(ELOGMTADU.IsChecked == true)
            {
                var selectFiles = new SelectFileForELOGMTADU();
                selectFiles.ShowDialog();
            }
            else if (ELOGMTPHX.IsChecked == true)
            {
                var selectFiles = new SelectFileForELOGMTPHX();
                selectFiles.ShowDialog();
            }
            else if (ELOGDualADU.IsChecked == true)
            {
                var selectFiles = new SelectFileForELOGDualADU(false);
                selectFiles.ShowDialog();
            }
            else if (ELOGDualPHX.IsChecked == true)
            {
                var selectFiles = new SelectFileForELOGDualPHX();
                selectFiles.ShowDialog();
            }
            else if (ELOGDualADUATS.IsChecked == true)
            {
                var selectFiles = new SelectFileForELOGDualADUATS(false);
                selectFiles.ShowDialog();
            }
            else if (ELOG1K.IsChecked == true)
            {
                var selectFiles = new SelectFileForELOGDualADU(true);
                selectFiles.ShowDialog();
            }
            else if (ELOG1KATS.IsChecked == true)
            {
                var selectFiles = new SelectFileForELOGDualADUATS(true);
                selectFiles.ShowDialog();
            }
#if false
            else if (ATS.IsChecked == true)
            {
                var selectFiles = new SelectFileForATS();
                selectFiles.ShowDialog();
            }
            else if (Text.IsChecked == true)
            {
                var selectFiles = new SelectFileForText();
                selectFiles.ShowDialog();
            }
#endif
        }
    }
}
