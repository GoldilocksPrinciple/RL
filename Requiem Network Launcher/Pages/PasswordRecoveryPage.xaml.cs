using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http;
using log4net;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class PasswordRecoveryPage : Page
    {
        #region Global variables
        private MainWindow mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool _requestSent = false;
        #endregion

        #region Constructor
        public PasswordRecoveryPage()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
        }

        private void RecoverySubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_requestSent == false)
            {
                log.Info("Recovery submit button clicked.");
                Dispatcher.Invoke((Action)(() =>
                {
                    RecoverySubmitButton.Visibility = Visibility.Hidden;
                    LoadingSpinner.Visibility = Visibility.Visible;
                }));

                if (RecoveryPasswordBox.Password == RecoveryConfirmPasswordBox.Password)
                {
                    Recovery();
                }
                else
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        RecoverySubmitButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                        RecoveryNotificationBox.Text = "New Password and Confirm New Password do not match!";
                        RecoveryNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
            }
            else
            {
                _requestSent = false; // set bool back to normal
                this.NavigationService.Navigate(new LoginPage());
                Dispatcher.Invoke((Action)(() =>
                {
                    mainWindow.LogoutButton.Content = "LOGIN";
                    RecoverySubmitButton.Content = "SUBMIT";
                }));
            }
        }
        #endregion
        
        #region Recovery Function
        /// <summary>
        /// Send request to web server to register a new account
        /// </summary>
        private async void Recovery()
        {
            log.Info("Sending recovery request.");

            HttpClient _client = new HttpClient();

            try
            {
                // set base address for request
                _client.BaseAddress = new Uri("http://142.44.142.178");

                var values = new Dictionary<string, string>
                {
                    { "username"      , RecoveryUsernameBox.Text       },
                    { "email"         , RecoveryEmailBox.Text          },
                    { "newpassword"   , RecoveryPasswordBox.Password   }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await _client.PostAsync("/api/recovery.php?username=" + RecoveryUsernameBox.Text
                                                                      + "&email=" + RecoveryEmailBox.Text
                                                                      + "&newpassword=" + RecoveryPasswordBox.Password, content);
                // convert response from server to string 
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode == true)
                {
                    log.Info("Recovery request sent successfully!");
                    _requestSent = true;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        RecoverySubmitButton.Content = "BACK TO LOGIN";
                        RecoverySubmitButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                        RecoveryNotificationBox.Text = "An email has been sent to your email address to complete the request.";
                        RecoveryNotificationBox.Foreground = new SolidColorBrush(Colors.LawnGreen);
                    }));
                }
                else if (response.IsSuccessStatusCode == false)
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        RecoverySubmitButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                    }));

                    var responseStringSplit = responseString.Split(',');
                    var responseCode = responseStringSplit[0].Split(':')[1]; // split string to "code" and code number
                    var responseMess = responseStringSplit[1].Split('"')[3]; // split string to "message", ":", and message content
                    if (responseMess.Contains("Invalid request"))
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            RecoveryNotificationBox.Text = "Invalid request " + responseStringSplit[2];
                            RecoveryNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            RecoveryNotificationBox.Text = responseMess;
                            RecoveryNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                if (e is HttpRequestException)
                {
                    System.Windows.MessageBox.Show(e.Message, "Requiem - Connection error");
                    log.Error(e.ToString());

                    Dispatcher.Invoke((Action)(() =>
                    {
                        RecoveryNotificationBox.Text = "Failed to send request!\nPlease contact staff for more help.";
                        RecoveryNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
                else
                {
                    System.Windows.MessageBox.Show(e.Message, "Requiem - Connection error");
                    log.Error(e.ToString());
                    Dispatcher.Invoke((Action)(() =>
                    {
                        RecoveryNotificationBox.Text = "An unexpected error happened!\nPlease contact staff for more help.";
                        RecoveryNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
            }
        }
        #endregion

        #region ENTER key pressed event handlers
        private void RecoveryUsernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Recovery();
            }
        }

        private void RecoveryEmailBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Recovery();
            }
        }

        private void RecoveryPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Recovery();
            }
        }

        private void RecoveryConfirmPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Recovery();
            }
        }
        #endregion
    }
}
