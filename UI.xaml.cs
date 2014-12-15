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
using KSPE3Lib;

namespace MountingCommutationScheme
{
    /// <summary>
    /// Логика взаимодействия для UI.xaml
    /// </summary>
    public partial class UI : Window
    {
        private E3ApplicationInfo applicationInfo;

        public UI()
        {
            applicationInfo = new E3ApplicationInfo();
            InitializeComponent();
            MinHeight = Height;
            MinWidth = Width;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            if (applicationInfo.Status == SelectionStatus.Selected)
                richTextBox.AppendText(applicationInfo.MainWindowTitle);
            else
            {
                richTextBox.AppendText(applicationInfo.StatusReasonDescription);
                DoButton.IsEnabled = false;
            }
        }

        private void DoButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            Script.Main(applicationInfo.ProcessId);
            Cursor = Cursors.Arrow;
        }


    }
}
