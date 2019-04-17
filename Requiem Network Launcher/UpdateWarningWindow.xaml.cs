using System;
using System.Windows;
using log4net;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for UpdateWarningWindow.xaml
    /// </summary>
    public partial class UpdateWarningWindow
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateWarningWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
        }

        private void AgreeUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Agree large update.");
            this.Close();
        }

        private void DeclineUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Decline large update.");
            log.Info("Closing launcher.\n");
            Environment.Exit(0);
        }
    }
}
