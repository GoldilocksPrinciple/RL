using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http;
using System.Security.Cryptography;
using System.IO;
using NLog;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        #region Global variables
        System.Net.CookieContainer myCookies = new System.Net.CookieContainer();
        private MainWindow mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        private static Logger log = NLog.LogManager.GetLogger("AppLog");
        #endregion

        #region Constructor
        public LoginPage()
        {
            InitializeComponent();

            // fill user's login credentials
            FillUserInfo();
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            AutoLogin();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Login button clicked.");
            Dispatcher.Invoke((Action)(() =>
            {
                LoginButton.Visibility = Visibility.Hidden;
                CreateAccountButton.Visibility = Visibility.Hidden;
                LoadingSpinner.Visibility = Visibility.Visible;
            }));

            Login();
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Create account button clicked.");
            this.NavigationService.Navigate(new RegisterPage());
            Dispatcher.Invoke((Action)(() =>
            {
                mainWindow.LogoutButton.Content = "REGISTER";
            }));
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Forgot password button clicked.");
            this.NavigationService.Navigate(new PasswordRecoveryPage());
            Dispatcher.Invoke((Action)(() =>
            {
                mainWindow.LogoutButton.Content = "PASSWORD RECOVERY";
            }));
        }
        #endregion

        #region Login function
        /// <summary>
        /// Send login request to web server
        /// </summary>
        private async void Login()
        {
            log.Info("Sending login request...");

            HttpClient _client = new HttpClient();

            try
            {
                // set base address for request
                _client.BaseAddress = new Uri("http://142.44.142.178");

                var values = new Dictionary<string, string>
                {
                    { "username", LoginUsernameBox.Text     },
                    { "password", LoginPasswordBox.Password }
                };
                var content = new FormUrlEncodedContent(values);

                // send POST request with username and password and get the response from server
                var response = await _client.PostAsync("/api/auth.php?username=" + LoginUsernameBox.Text
                                                                  + "&password=" + LoginPasswordBox.Password, content);

                // convert response from server to string 
                var responseString = await response.Content.ReadAsStringAsync();
                var responseStringSplit = responseString.Split(',');
                var responseCode = responseStringSplit[0].Split(':')[1]; // split string to "code" and code number
                var responseMess = responseStringSplit[1].Split('"')[3]; // split string to "message", ":", and message content

                // reponse code 200 = OK, username + password are correct
                if (responseCode == "200")
                {
                    log.Info("Login successfully!");
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        log.Info("Encrypting user credential.");
                        try
                        {
                            using (RijndaelManaged myRijndael = new RijndaelManaged())
                            {

                                byte[] key = Encoding.ASCII.GetBytes(HardwareID.Hwid);
                                myRijndael.GenerateIV();
                                byte[] iv = myRijndael.IV;

                                Console.WriteLine(LoginPasswordBox.Password);

                                // Encrypt the string to an array of bytes. 
                                byte[] encryptedPassword = EncryptStringToBytes(LoginPasswordBox.Password, key, myRijndael.IV);

                                UserInfoRegistry.SaveUserLoginInfo(LoginUsernameBox.Text, encryptedPassword, iv);
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: {0}", e.Message);
                            log.Error(e.ToString());
                        }
                    }
                    else
                    {
                        log.Info("Saving user credential.");
                        byte[] pswd = new byte[0];
                        byte[] iv = new byte[0];
                        UserInfoRegistry.SaveUserLoginInfo("", pswd, iv);
                    }

                    // split string to "token", ":", and token value
                    string responseToken = responseStringSplit[2].Split('"')[3];
                    UserInfoRegistry.LoginToken = responseToken.Replace(@"\", ""); // "\/" is not valid. Correct format should be "/" only, "\" acts as an escape character
                    
                    this.NavigationService.Navigate(new MainGamePage(), myCookies);
                    
                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginButton.Visibility = Visibility.Visible;
                        CreateAccountButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                        mainWindow.LogoutButton.Content = "LOGOUT (" + LoginUsernameBox.Text.ToUpper() + ")";
                        mainWindow._nIcon.Text = "Requiem Network Launcher (" + LoginUsernameBox.Text + ")";
                    }));
                    
                }

                // response code 500 = wrong username or password
                else if (responseCode == "500")
                {
                    // Update login status on UI thread (main thread)
                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginButton.Visibility = Visibility.Visible;
                        CreateAccountButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                        LoginPasswordBox.Focus();
                        LoginPasswordBox.Password = "";
                        LoginNotificationBox.Text = "Incorrect username or password.\nPlease try again or register first!";
                        LoginNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }

                // for any other response code that is not 200 or 500
                else
                {
                    // Update login status on UI thread(main thread)
                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginButton.Visibility = Visibility.Visible;
                        CreateAccountButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                        LoginNotificationBox.Text = "Login failed!\nError code: " + responseCode + "\nPlease contact staff for more help.";
                        LoginNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
            }
            catch (Exception e)
            {
                if (e is HttpRequestException)
                {
                    log.Error(e.ToString());
                    System.Windows.MessageBox.Show(e.Message, "Connection error");
                    
                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginNotificationBox.Text = "Login failed!\nPlease check your internet connection first.";
                        LoginNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
                else
                {
                    log.Error(e.ToString());
                    System.Windows.MessageBox.Show(e.Message, "Error");
                    
                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginNotificationBox.Text = "An unexpected error occurred!\nPlease check your internet connection first.";
                        LoginNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
            }
        }
        #endregion

        #region Auto login function
        /// <summary>
        /// Auto login if found user's credential
        /// </summary>
        private void AutoLogin()
        {
            log.Info("Auto login.");

            if (LoginUsernameBox.Text != "")
            {
                Console.WriteLine(LoginUsernameBox.Text);
                Login();
            }
            else
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    LoginUsernameBox.IsReadOnly = false;

                    LoginPasswordBox.IsEnabled = true;

                    LoginButton.Visibility = Visibility.Visible;
                    CreateAccountButton.Visibility = Visibility.Visible;
                    LoadingSpinner.Visibility = Visibility.Hidden;
                }));
            }
        }
        #endregion

        #region Fill user info on init
        /// <summary>
        /// Auto fill user's credential on start
        /// </summary>
        private void FillUserInfo()
        {
            log.Info("Auto fill user login detail.");
            if (UserInfoRegistry.Username != "")
            {
                string Password = "";
                try
                {
                    using (RijndaelManaged myRijndael = new RijndaelManaged())
                    {
                        UserInfoRegistry.GetUserLoginInfo();
                        byte[] key = Encoding.ASCII.GetBytes(HardwareID.Hwid);
                        byte[] iv = UserInfoRegistry.IV;
                        byte[] pwd = UserInfoRegistry.EncryptedPassword;

                        // Encrypt the string to an array of bytes. 
                        Password = DecryptStringFromBytes(pwd, key, iv);
                    }

                }
                catch (Exception e1)
                {
                    Console.WriteLine("Error: {0}", e1.Message);
                    log.Error(e1.ToString());
                }

                Dispatcher.Invoke((Action)(() =>
                {
                    LoginUsernameBox.Text = UserInfoRegistry.Username;
                    LoginUsernameBox.IsReadOnly = true;

                    LoginPasswordBox.Password = Password;
                    LoginPasswordBox.IsEnabled = false;

                    LoginButton.Visibility = Visibility.Hidden;
                    CreateAccountButton.Visibility = Visibility.Hidden;
                    LoadingSpinner.Visibility = Visibility.Visible;
                }));
            }

            if (LoginUsernameBox.Text == "")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    LoginPasswordBox.Password = "";
                }));
            }
        }
        #endregion

        #region Password encryption using Rijndael algorithm
        /// <summary>
        /// Password encryption
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        #endregion

        #region Password decryption using Rijndael algorithm
        /// <summary>
        /// Password decryption
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }

        #endregion
        
        #region ENTER key pressed event handlers
        /// <summary>
        /// Handle ENTER key pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginUsernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Login();
            }
        }

        private void LoginPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Login();
            }
        }
        #endregion

    }
}
