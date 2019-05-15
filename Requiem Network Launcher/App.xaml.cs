using System;
using System.Collections.Generic;
using System.Windows;
using MahApps.Metro;
using Microsoft.Shell;
using NLog;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "Requiem_Network_Launcher_UwU";
        private static Logger log = NLog.LogManager.GetLogger("AppLog");

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {

                var application = new App();

                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // Bring window to foreground
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.Show();
                this.MainWindow.WindowState = WindowState.Normal;
            }

            return true;
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            log.Info("=============  Started Logging  =============");

            AppDomain.CurrentDomain.UnhandledException +=
                  new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // add custom accent and theme resource dictionaries to the ThemeManager
            // you should replace MahAppsMetroThemesSample with your application name
            // and correct place where your custom accent lives
            ThemeManager.AddAccent("CustomTheme", new Uri("pack://application:,,,/Themes/CustomTheme.xaml"));

            // get the current app style (theme and accent) from the application
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

            // now change app style to the custom accent and current theme
            ThemeManager.ChangeAppStyle(Application.Current,
                                        ThemeManager.GetAccent("CustomTheme"),
                                        theme.Item1);
            
            base.OnStartup(e);
        }

        private void Application_DispatcherUnhandledException(object sender,
                               System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            log.Error("Unexpected error");
            log.Error(e.Exception.ToString());
            //Handling the exception within the UnhandledException handler.
            MessageBox.Show(e.Exception.Message, "Requiem - Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            log.Error("Unexpected error");
            log.Error(ex.ToString());
            MessageBox.Show(ex.Message, "Requiem - Unexpected Error Occured",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
