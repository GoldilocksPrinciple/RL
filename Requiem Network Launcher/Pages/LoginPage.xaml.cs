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
using System.Net.Http;
using System.Threading;
using System.Security.Cryptography;
using System.IO;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            FillUserInfo();
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            AutoLogin();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
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
            this.NavigationService.Navigate(new RegisterPage());
            var mainWindow = Application.Current.MainWindow as MainWindow;
            Dispatcher.Invoke((Action)(() =>
            {
                mainWindow.LogoutButton.Content = "REGISTER";
            }));
        }

        #region Login function
        /// <summary>
        /// Send login request to web server
        /// </summary>
        private async void Login()
        {
            HttpClient _client = new HttpClient();

            try
            {
                // set base address for request
                _client.BaseAddress = new Uri("http://142.44.142.178");

                var values = new Dictionary<string, string>
                {
                    { "username", LoginUsernameBox.Text },
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
                    if (RememberMeCheckBox.IsChecked == true)
                    {
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
                        }
                    }

                    // split string to "token", ":", and token value
                    string responseToken = responseStringSplit[2].Split('"')[3];
                    UserInfoRegistry.LoginToken = responseToken.Replace(@"\", ""); // "\/" is not valid. Correct format should be "/" only, "\" acts as an escape character
                    
                    this.NavigationService.Navigate(new MainGamePage());

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginButton.Visibility = Visibility.Visible;
                        CreateAccountButton.Visibility = Visibility.Visible;
                        LoadingSpinner.Visibility = Visibility.Hidden;
                        mainWindow.LogoutButton.Content = "LOGOUT (" + LoginUsernameBox.Text.ToUpper() + ")";
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
                    System.Windows.MessageBox.Show(e.Message, "Connection error");

                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginNotificationBox.Text = "Login failed!\nPlease check your internet connection first.\nContact staff for more help.";
                        LoginNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
                else
                {
                    System.Windows.MessageBox.Show(e.Message, "Error");
                    Dispatcher.Invoke((Action)(() =>
                    {
                        LoginNotificationBox.Text = "An unexpected error occurred!\nPlease check your internet connection first.\nContact staff for more help.";
                        LoginNotificationBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
            }
        }
        #endregion

        #region Auto login function
        private void AutoLogin()
        {
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
        private void FillUserInfo()
        {
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
