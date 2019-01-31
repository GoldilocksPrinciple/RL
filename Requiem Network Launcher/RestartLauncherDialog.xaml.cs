using System;
using System.Windows;
using System.Diagnostics;


namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for RestartLauncherDialog.xaml
    /// </summary>
    public partial class RestartLauncherDialog 
    {
        public RestartLauncherDialog()
        {
            InitializeComponent();
        }

        private void RestartLauncherButton_Click(object sender, RoutedEventArgs e)
        {
            string launcherUpdaterPath = System.IO.Path.Combine(((MainWindow)Application.Current.MainWindow).rootDirectory, "updater.exe");
            Process launcherUpdater = new Process();
            launcherUpdater.StartInfo.FileName = launcherUpdaterPath;
            launcherUpdater.Start();
            Environment.Exit(0);
        }
    }
}
