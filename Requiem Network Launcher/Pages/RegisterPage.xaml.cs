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
    public partial class RegisterPage : Page
    {
        #region Global variables
        private MainWindow mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public RegisterPage()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Register button clicked.");
            Dispatcher.Invoke((Action)(() =>
            {
                RegisterButton.Visibility = Visibility.Hidden;
                BackToLoginButton.Visibility = Visibility.Hidden;
                LoadingSpinner.Visibility = Visibility.Visible;
            }));

            Register();
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LoginPage());
            Dispatcher.Invoke((Action)(() =>
            {
                mainWindow.LogoutButton.Content = "LOGIN";
            }));
        }
        #endregion

        #region Register Function
        /// <summary>
        /// Send request to web server to register a new account
        /// </summary>
        private async void Register()
        {
            log.Info("Sending register request.");

            HttpClient _client = new HttpClient();

            try
            {
                // set base address for request
                _client.BaseAddress = new Uri("http://142.44.142.178");

                var values = new Dictionary<string, string>
                {
                    { "username", RegisterUsernameBox.Text },
                    { "password", RegisterPasswordBox.Password },
                    { "email"   , RegisterEmailBox.Text    },
                };
                var content = new FormUrlEncodedContent(values);
                var response = await _client.PostAsync("/api/registration.php?username=" + RegisterUsernameBox.Text
                                                                          + "&password=" + RegisterPasswordBox.Password
                                                                          + "&email=" + RegisterEmailBox.Text, content);
                // convert response from server to string 
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode == true)
                {
                    log.Info("Register successfully!");
                    Dispatcher.Invoke((Action)(() =>
                    {
                        RegisterButton.Visibility = Visibility.Visible;
                        BackToLoginButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                        RegisterNotificationBox.Text = "Register successfully!\nPlease return to login tab to login.";
                        RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.LawnGreen);
                    }));
                }
                else if (response.IsSuccessStatusCode == false)
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        RegisterButton.Visibility = Visibility.Visible;
                        BackToLoginButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                    }));

                    var responseStringSplit = responseString.Split(',');
                    var responseCode = responseStringSplit[0].Split(':')[1]; // split string to "code" and code number
                    var responseMess = responseStringSplit[1].Split('"')[3]; // split string to "message", ":", and message content
                    Console.WriteLine(responseMess);
                    if (responseMess.Contains("email"))
                    {
                        if (responseMess.Contains("already exists"))
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                RegisterNotificationBox.Text = "Email has already been used.\nPlease use another email address.";
                                RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                            }));
                        }
                        else
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                RegisterNotificationBox.Text = "Email is not valid.\nPlease enter a valid email address.";
                                RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                            }));
                        }
                    }
                    else if (responseMess.Contains("username"))
                    {
                        if (responseMess.Contains("already exists"))
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                RegisterNotificationBox.Text = "Username has already been used.\nPlease use another username.";
                                RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                            }));
                        }
                        else
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                RegisterNotificationBox.Text = "Username is not valid.\nPlease choose a valid username.";
                                RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                            }));
                        }
                    }
                    else if (responseMess.Contains("Invalid request"))
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            RegisterNotificationBox.Text = "Password is not valid.\nPlease choose a valid password.";
                            RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
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
                        RegisterNotificationBox.Text = "Register failed!\nPlease contact staff for more help.";
                        RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
                else
                {
                    System.Windows.MessageBox.Show(e.Message, "Requiem - Connection error");
                    log.Error(e.ToString());
                    Dispatcher.Invoke((Action)(() =>
                    {
                        RegisterNotificationBox.Text = "An unexpected error happened!\nPlease contact staff for more help.";
                        RegisterNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
            }
        }
        #endregion

        #region ENTER key pressed event handlers
        private void RegisterUsernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Register();
            }
        }

        private void RegisterEmailBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Register();
            }
        }

        private void RegisterPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Register();
            }
        }
        #endregion
    }
}
