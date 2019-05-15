using System;
using System.Windows;
using NLog;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for UpdateWarningWindow.xaml
    /// </summary>
    public partial class UpdateWarningWindow
    {
        private static Logger log = NLog.LogManager.GetLogger("AppLog");

        public UpdateWarningWindow()
        {
            InitializeComponent();
        }

        private void AgreeUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Agree large update.");
            this.Close();
        }

        private void DeclineUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Decline large update.");
            ((MainWindow)Application.Current.MainWindow).discordRpcClient.Dispose();
            log.Info("Closing launcher.\n");
            Environment.Exit(0);
        }
    }
}
