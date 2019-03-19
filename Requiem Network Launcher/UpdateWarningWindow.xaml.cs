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

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for UpdateWarningWindow.xaml
    /// </summary>
    public partial class UpdateWarningWindow
    {
        public UpdateWarningWindow()
        {
            InitializeComponent();
        }

        private void AgreeUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeclineUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
