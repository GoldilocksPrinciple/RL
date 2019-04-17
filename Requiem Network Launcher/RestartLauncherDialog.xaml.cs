using System;
using System.Windows;
using System.Diagnostics;
using log4net;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for RestartLauncherDialog.xaml
    /// </summary>
    public partial class RestartLauncherDialog 
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public RestartLauncherDialog()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
        }

        private void RestartLauncherButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Restart launcher button clicked.");
            ((MainWindow)Application.Current.MainWindow).waitingForRestart = false;
            string launcherUpdaterPath = System.IO.Path.Combine(((MainWindow)Application.Current.MainWindow).rootDirectory, "updater.exe");
            Process launcherUpdater = new Process();
            launcherUpdater.StartInfo.FileName = launcherUpdaterPath;
            launcherUpdater.Start();
            log.Info("Running updater.exe");
            log.Info("Closing launcher.\n");
            Environment.Exit(0);
        }
    }
}
