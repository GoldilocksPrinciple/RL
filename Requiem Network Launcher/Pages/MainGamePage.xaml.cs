using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Bleak;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Controls;
using System.Threading;
using System.IO;
using System.Net;
using Ionic.Zip;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.ServiceModel.Syndication;
using System.ComponentModel;
using System.Timers;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for MainGamePage.xaml
    /// </summary>
    public partial class MainGamePage : Page
    {
        #region Global variables
        private Injector _injector = new Injector();
        private Process _vindictus;
        private double _updateFileSize;
        private string _updatePath;
        private string _updateDownloadID;
        private string _versionTxtCheck = "yes";
        private string _continueSign = "continue";
        private string _currentVersionLocal;
        private string _launcherUpdatePath;
        private bool playing = false;
        private bool updating = false;
        private CookieContainer myCookies = new System.Net.CookieContainer();
        private Stopwatch sw = new Stopwatch();
        #endregion

        #region Constructor
        /// <summary>
        /// Main page initialization
        /// </summary>
        public MainGamePage()
        {
            InitializeComponent();
            CheckFilesPath();
            new Thread(delegate ()
            {
                try
                {
                    LoginForum();
                    PullRSSFeed("http://requiemnetwork.com/forum/44-server-news.xml/", "http://requiemnetwork.com/forum/44-server-news/");
                }
                catch (WebException e )
                {
                    Console.WriteLine(e.Message);
                }
                
            }).Start();
            
            System.Timers.Timer aTimer = new System.Timers.Timer(600000); // (ms) 600000 = 10 mins
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;

            // display updating status notice
            ProgressDetails.Visibility = Visibility.Hidden;
            ProgressBar.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Start game 
        /// <summary>
        /// Handle start/launch game procedure
        /// </summary>
        private async void StartGame()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            _vindictus = new Process();

            try
            {
                _vindictus.EnableRaisingEvents = true;
                _vindictus.Exited += _process_Exited;
                _vindictus.StartInfo.FileName = mainWindow.processPath;
                _vindictus.StartInfo.Arguments = " -lang zh-TW -token " + UserInfoRegistry.LoginToken; // if token is null -> server under maintenance error
                _vindictus.Start();
                playing = true;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("There is already an instance of the game running...", "Error");
            }

            // inject winnsi.dll to Vindictus.exe 
            _injector.CreateRemoteThread(mainWindow.dllPath, _vindictus.Id);

            Dispatcher.Invoke((Action)(() =>
            {
                // disable start game button after the game start
                StartGameButton.Content = "PLAYING";
                StartGameButton.IsEnabled = false;
            }));

            // close the launcher
            await Task.Delay(2000);

            mainWindow.WindowState = WindowState.Minimized;
        }

        private void _process_Exited(object sender, EventArgs e)
        {
            playing = false;
            Dispatcher.Invoke((Action)(() =>
            {
                // re-enable start game button after the game is closed
                StartGameButton.Content = "START GAME";
                StartGameButton.IsEnabled = true;
                StartGameButton.Foreground = new SolidColorBrush(Colors.Black);
            }));
        }
        #endregion

        #region Check game version
        /// <summary>
        /// Handle game version checking
        /// </summary>
        private async void CheckGameVersion()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            // update small version info at bottom left corner
            Dispatcher.Invoke((Action)(() =>
            {
                ShowHideDownloadInfo(DownloadInfoBox.Height, "1line");
                DownloadInfoBox.Text = "Checking game version...";
                DownloadInfoBox.Foreground = new SolidColorBrush(Colors.LawnGreen);

                // disable start game button when check for game version
                StartGameButton.Content = "UPDATING";
                StartGameButton.IsEnabled = false;
                StartGameButton.Foreground = new SolidColorBrush(Colors.Silver);
            }));

            // read info from version.txt file in main game folder
            var versionTextLocal = System.IO.File.ReadAllText(mainWindow.versionPath);
            var versionTextLocalSplit = versionTextLocal.Split(',');
            var currentVersionLocal = versionTextLocalSplit[0].Split('"')[3];
            var currentVersionDate = versionTextLocalSplit[1].Split('"')[3];

            // update small version info at bottom left corner
            Dispatcher.Invoke((Action)(() =>
            {
                VersionDisplayLabel.Content = "Game version: " + currentVersionLocal + " - Release Date: " + currentVersionDate;
            }));

            // work with .net framework 4.5 and above
            HttpClient _client = new HttpClient();

            try
            {
                // read info from version.txt on the server
                var versionTextServer = await _client.GetStringAsync("http://requiemnetwork.com/launcher/version.txt");
                var versionTextServerSplit = versionTextServer.Split(',');
                var currentVersionServer = versionTextServerSplit[0].Split('"')[3];

                // check if player has updated their game yet or not
                if (currentVersionLocal != currentVersionServer)
                {
                    // store variable so that it can be accessed outside of async method
                    _currentVersionLocal = currentVersionLocal;
                    updating = true;
                    _continueSign = "continue";
                    Update();
                }
                else
                {
                    // display notice
                    Dispatcher.Invoke((Action)(() =>
                    {
                        updating = false;
                        ShowHideDownloadInfo(DownloadInfoBox.Height, "1line");
                        DownloadInfoBox.Text = "Your game is up-to-date!";
                        DownloadInfoBox.Foreground = new SolidColorBrush(Colors.LawnGreen);

                        if (playing == false)
                        {
                            // re-enable start game button for player
                            StartGameButton.Content = "START GAME";
                            StartGameButton.IsEnabled = true;
                            StartGameButton.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            StartGameButton.Content = "PLAYING";
                        }

                    }));

                }
            }
            catch (Exception e)
            {
                if (e is HttpRequestException)
                {
                    System.Windows.MessageBox.Show(e.Message, "Requiem - Connection error");

                    Dispatcher.Invoke((Action)(() =>
                    {
                        ShowHideDownloadInfo(DownloadInfoBox.Height, "3lines");
                        DownloadInfoBox.Text = "Cannot connect to server!\nPlease check your network connection first.\nContact staff for more help.";
                        DownloadInfoBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
                else
                {
                    System.Windows.MessageBox.Show(e.Message, "Error");

                    Dispatcher.Invoke((Action)(() =>
                    {
                        ShowHideDownloadInfo(DownloadInfoBox.Height, "3lines");
                        DownloadInfoBox.Text = e.Message;
                        DownloadInfoBox.Foreground = new SolidColorBrush(Colors.Red);
                    }));
                }
            }
        }
        #endregion

        #region Update game
        /// <summary>
        /// Handle game updating
        /// </summary>
        private async void Update()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            
            // update ui for downloading
            Dispatcher.Invoke((Action)(() =>
            {
                ShowHideDownloadInfo(DownloadInfoBox.Height, "1line");
                DownloadInfoBox.Text = "Preparing update...";
                DownloadInfoBox.Foreground = new SolidColorBrush(Colors.LawnGreen);

                // disable start game button when check for game version
                StartGameButton.Content = "UPDATING";
                StartGameButton.IsEnabled = false;
                StartGameButton.Foreground = new SolidColorBrush(Colors.Silver);

            }));

            foreach (var process in Process.GetProcessesByName("Vindictus"))
            {
                process.Kill();
            }
            
            if (_versionTxtCheck == "not found")
            {
                HttpClient _client = new HttpClient();

                // get download links from server
                var updateDownload = await _client.GetStringAsync("http://requiemnetwork.com/launcher/update_02.txt");
                var updateDownloadSplit = updateDownload.Split(',');

                // download information for people who update their game regularly
                var updateDowndloadLink = updateDownloadSplit[0].Split('"');

                _updateDownloadID = updateDowndloadLink[3];
                GetDownloadFileSize(updateDowndloadLink[5]);
            }
            else if (_versionTxtCheck == "yes")
            {

                HttpClient _client = new HttpClient();

                // get download links from server
                var updateDownload = await _client.GetStringAsync("http://requiemnetwork.com/launcher/update_01.txt");
                var updateDownloadSplit = updateDownload.Split(',');

                // download information for people who update their game regularly
                var updateDowndloadOld = updateDownloadSplit[0].Split('"');

                // download information for people who has not updated their game for a while
                var updateDowndloadNew = updateDownloadSplit[1].Split('"');

                // get download link based on their game's current version
                if (_currentVersionLocal == updateDowndloadOld[1]) // if player has the latest patch
                {
                    _updateDownloadID = updateDowndloadOld[3];
                    GetDownloadFileSize(updateDowndloadOld[5]);
                }
                else // if player has an outdate patch 
                {
                    _updateDownloadID = updateDowndloadNew[3];
                    GetDownloadFileSize(updateDowndloadNew[5]);
                }
            }

            // create temporary zip file from download
            _updatePath = System.IO.Path.Combine(mainWindow.rootDirectory, "UpdateTemporary.zip");
            //_updatePath = @"D:\test\UpdateTemp.zip";

            // Start stopwatch
            sw.Start();

            // download update (zip) 
            try
            {
                using (CookieAwareWebClient webClient = new CookieAwareWebClient())
                {
                    if (_updateFileSize <= 100000000) // if file is < 100 MB -> no virus scan page -> can download directly
                    {
                        // download the update
                        var uri = new Uri("https://drive.google.com/uc?export=download&id=" + _updateDownloadID);
                        webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                        webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                        await webClient.DownloadFileTaskAsync(uri, _updatePath);
                    }
                    else // more than 100 MB -> have to bypass virus scanning page
                    {
                        // sometimes google drive returns an NID cookie instead of a download warning cookie at first attempt
                        // it will works in the second attempt
                        for (int i = 0; i < 2; i++)
                        {
                            // download page content
                            string DownloadString = await webClient.DownloadStringTaskAsync("https://drive.google.com/uc?export=download&id=" + _updateDownloadID);

                            // get confirm code from page content
                            Match match = Regex.Match(DownloadString, @"confirm=([0-9A-Za-z]+)");

                            if (_continueSign == "stop")
                            {
                                break;

                            }
                            else
                            {
                                // construct new download link with confirm code
                                string updateDownloadLinkNew = "https://drive.google.com/uc?export=download&" + match.Value + "&id=" + _updateDownloadID;
                                var uri = new Uri(updateDownloadLinkNew);
                                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                                await webClient.DownloadFileTaskAsync(uri, _updatePath);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error");
            }
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            // Stop stopwatch
            sw.Stop();

            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            // get the size of the downloaded file
            long length = new System.IO.FileInfo(_updatePath).Length;

            if (length < 60000) // if the downloaded file is a web page content (usually has size smaller than 60KB)
            {
                // delete the file and re-send the request
                File.Delete(_updatePath);
            }
            else // if the file is valid, extract it
            {
                _continueSign = "stop";

                Dispatcher.Invoke((Action)(() =>
                {
                    ShowHideDownloadInfo(DownloadInfoBox.Height, "1line");
                    // display updating status notice
                    DownloadInfoBox.Text = "Extracting files...";
                }));

                // perform unzip in a new thread to prevent UI from freezing
                new Thread(delegate ()
                {
                    // extract update to main game folder and overwrite all existing files
                    using (ZipFile zip = ZipFile.Read(_updatePath))
                    {
                        zip.ExtractProgress += Zip_ExtractProgress;
                        //zip.ExtractAll(@"D:\test\", ExtractExistingFileAction.OverwriteSilently);
                        zip.ExtractAll(mainWindow.rootDirectory, ExtractExistingFileAction.OverwriteSilently);
                    }

                    // delete the temporary zip file after finish extracting
                    File.Delete(_updatePath);

                    Dispatcher.Invoke((Action)(() =>
                    {
                        // display updating status notice
                        ProgressDetails.Visibility = Visibility.Hidden;
                        ProgressBar.Visibility = Visibility.Hidden;
                    }));

                    _versionTxtCheck = "yes";

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        CheckGameVersion();
                    }));
                }).Start();
            }
        }

        private void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.TotalBytesToTransfer > 0)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    // display updating status notice
                    ProgressBar.Value = System.Convert.ToInt32(100 * e.BytesTransferred / e.TotalBytesToTransfer);
                    ProgressDetails.Text = "Extracting: " + e.CurrentEntry.FileName;
                }));
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double totalSize = ((double)e.BytesReceived / (double)_updateFileSize) * 100;
            if (_continueSign == "continue")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    // display updating status notice
                    StartGameButton.IsEnabled = false;
                    StartGameButton.Foreground = new SolidColorBrush(Colors.Silver);
                    ProgressDetails.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBar.Value = totalSize;
                    ProgressDetails.Text = FileSizeType(e.BytesReceived) + " / " + FileSizeType(_updateFileSize);

                    ShowHideDownloadInfo(DownloadInfoBox.Height, "2lines");
                    DownloadInfoBox.Text = "Dowloading update... " + totalSize.ToString("F2") + "%\n" +
                                           "Speed: " + DownloadSpeedConverter(e.BytesReceived);

                }));
            }
        }

        private void GetDownloadFileSize(string fileSize)
        {
            _updateFileSize = System.Convert.ToInt64(fileSize);
        }

        private string DownloadSpeedConverter(double bytesReceived)
        {
            string totalSpeed;
            double speedInKB = ((double)bytesReceived / 1024d / sw.Elapsed.TotalSeconds);
            double speedInMB = ((double)bytesReceived / 1024d / 1024d / sw.Elapsed.TotalSeconds);
            double speedInGB = ((double)bytesReceived / 1024d / 1024d / 1024d / sw.Elapsed.TotalSeconds);

            if (speedInKB > 1000)
            {
                totalSpeed = string.Format("{0} MB/s", speedInMB.ToString("0.00"));
            }
            else if (speedInKB > 1000000)
            {
                totalSpeed = string.Format("{0} GB/s", speedInGB.ToString("0.00"));
            }
            else
            {
                totalSpeed = string.Format("{0} KB/s", speedInKB.ToString("0.00"));
            }

            return totalSpeed;
        }

        private string FileSizeType(double bytes)
        {
            string fileSizeType;
            if ((bytes / 1024d / 1024d / 1024d) > 1)
            {
                fileSizeType = string.Format("{0} GB", (bytes / 1024d / 1024d / 1024d).ToString("0.00"));
            }
            else
            {
                fileSizeType = string.Format("{0} MB", (bytes / 1024d / 1024d).ToString("0.00"));
            }

            return fileSizeType;
        }
        #endregion

        #region Check for launcher update
        /// <summary>
        /// Check for launcher update
        /// </summary>
        private async void CheckForLauncherUpdate()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            var launcherInfoFile = System.IO.File.ReadAllText(mainWindow.launcherInfoPath);
            var launcherInfoSplit = launcherInfoFile.Split('=');
            var launcherInfoLocal = launcherInfoSplit[1];

            // work with .net framework 4.5 and above
            HttpClient _client = new HttpClient();

            try
            {
                // read info from version.txt on the server
                var launcherInfoServer = await _client.GetStringAsync("http://requiemnetwork.com/launcher/info.txt");
                var launcherInfoServerSplit = launcherInfoServer.Split('=');

                // check if player has updated their game yet or not
                if (launcherInfoLocal != launcherInfoServerSplit[1])
                {
                    if (mainWindow.waitingForRestart == false)
                    {
                        UpdateLauncher();
                        mainWindow.waitingForRestart = true;
                    }

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Requiem - Connection error");
            }

        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (updating == false)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    CheckForLauncherUpdate();
                }));
            }
        }
        #endregion

        #region Update launcher
        /// <summary>
        /// Handle downloading and update launcher
        /// </summary>
        private async void UpdateLauncher()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            _launcherUpdatePath = System.IO.Path.Combine(mainWindow.rootDirectory, "update.exe");

            Dispatcher.Invoke((Action)(() =>
            {
                // display updating status notice
                ProgressBar.Visibility = Visibility.Visible;
                ShowHideDownloadInfo(DownloadInfoBox.Height, "1line");
                DownloadInfoBox.Text = "Downloading new launcher...";
                DownloadInfoBox.Foreground = new SolidColorBrush(Colors.LawnGreen);

                // disable start game button when check for game version
                StartGameButton.Content = "UPDATING";
                StartGameButton.IsEnabled = false;
                StartGameButton.Foreground = new SolidColorBrush(Colors.Silver);
            }));

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    var uri = new Uri("http://requiemnetwork.com/launcher/update.exe");
                    Console.WriteLine(uri);
                    webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged1;
                    webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted1;
                    await webClient.DownloadFileTaskAsync(uri, _launcherUpdatePath);
                }
            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show(e1.Message, "Checking for launcher update failed");

            }
        }

        private void WebClient_DownloadFileCompleted1(object sender, AsyncCompletedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            Dispatcher.Invoke((Action)(() =>
            {
                // display updating status notice
                ProgressBar.Visibility = Visibility.Hidden;
                ShowHideDownloadInfo(DownloadInfoBox.Height, "2lines");
                DownloadInfoBox.Text = "New launcher downloaded.\nRestart required.";

                // disable start game button when check for game version
                StartGameButton.Content = "DOWNLOADED";
                StartGameButton.IsEnabled = false;
                StartGameButton.Foreground = new SolidColorBrush(Colors.Silver);
            }));
            mainWindow.Hide();
            RestartLauncherDialog dialog = new RestartLauncherDialog();
            dialog.ShowDialog();
        }

        private void WebClient_DownloadProgressChanged1(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                // display updating status notice
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = e.ProgressPercentage;
            }));
        }

        #endregion
        
        #region Rss feed handler
        /// <summary>
        /// Login on forum with Launcher account and handle RSS feed construction 
        /// </summary>
        private void LoginForum()
        {
            var test = "name='csrfKey' value='([0-9A-Za-z]+)'".Replace("'", "\"");
            HttpWebResponse myResponse = CustomHttpMethod.Get("http://requiemnetwork.com/login/", "http://requiemnetwork.com/login/", ref myCookies);
            string pageSrc;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                pageSrc = sr.ReadToEnd();
            }
            Match match = Regex.Match(pageSrc, test);
            string csrfKey = GetBetween(match.Value, "value=\"", "\"");

            string username = "Launcher";
            string password = "C7qSTldIVpOZQ9jk6QbO";

            string postData = "csrfKey=" + csrfKey + "&auth=" + username + "&password=" + password + "&remember_me=1" + "&_processLogin=usernamepassword&_processLogin=usernamepassword";

            bool result = CustomHttpMethod.Post("http://requiemnetwork.com/login/", postData, "http://requiemnetwork.com/login/", myCookies);

            if (result)
                Console.WriteLine("Valid!");
            else
                Console.WriteLine("NOPE!");
        }

        static string GetBetween(string message, string start, string end)
        {
            int startIndex = message.IndexOf(start) + start.Length;
            int stopIndex = message.LastIndexOf(end);
            return message.Substring(startIndex, stopIndex - startIndex);
        }

        private void PullRSSFeed(string url, string reference)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                LoadingSpinner.Visibility = Visibility.Visible;
            }));
            List<Feed> rssFeed = new List<Feed>();
            try
            {
                HttpWebResponse rssFeedStream = CustomHttpMethod.Get(url, reference, ref myCookies);
                XmlReader reader = XmlReader.Create(rssFeedStream.GetResponseStream());
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                foreach (SyndicationItem item in feed.Items)
                {
                    string description = item.Summary.Text;

                    description = HTMLToText.ConvertHtml(description);
                    description = description.Replace("Spoiler", "");


                    // , Feed_description = description, Feed_publishDate = item.PublishDate.ToString(), Feed_forumLink = item.Links[0].Uri.ToString()
                    rssFeed.Add(new Feed
                    {
                        Feed_title = item.Title.Text + "\n",
                        Feed_description = description,
                        Feed_publishDate = "Posted on: " + item.PublishDate.ToString(),
                        Feed_forumLink = item.Links[0].Uri.ToString()
                    });

                }
                Dispatcher.Invoke((Action)(() =>
                {
                    LoadingSpinner.Visibility = Visibility.Hidden;
                    RSSFeed.ItemsSource = rssFeed;
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        #region Rss feed object
        private class Feed
        {
            private string feed_title;
            private string feed_description;
            private string feed_publishDate;
            private string feed_forumLink;

            public string Feed_title { get => feed_title; set => feed_title = value; }
            public string Feed_description { get => feed_description; set => feed_description = value; }
            public string Feed_publishDate { get => feed_publishDate; set => feed_publishDate = value; }
            public string Feed_forumLink { get => feed_forumLink; set => feed_forumLink = value; }
        }
        #endregion

        #endregion

        #region Check file path 
        /// <summary>
        /// Validating file path
        /// </summary>
        private void CheckFilesPath()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            UpdateWarningWindow dialog = new UpdateWarningWindow();

            var ngclientFilePath = System.IO.Path.Combine(mainWindow.rootDirectory, "NGClient.aes");

            /*_dllPath = @"C:\Requiem\ko-KR\winnsi.dll";
            _processPath = @"C:\Requiem\ko-KR\Vindictus.exe";
            _versionPath = @"D:\test\version.txt";*/

            if (!File.Exists(mainWindow.versionPath))
            {
                _versionTxtCheck = "not found";
                dialog.ShowDialog();
                Update();
            }
            else if (!File.Exists(mainWindow.dllPath))
            {
                _versionTxtCheck = "not found";
                dialog.ShowDialog();
                Update();
            }
            else if (!File.Exists(mainWindow.processPath))
            {
                _versionTxtCheck = "not found";
                dialog.ShowDialog();
                Update();
            }
            else if (!File.Exists(ngclientFilePath))
            {
                ShowHideDownloadInfo(DownloadInfoBox.Height, "2lines");
                System.Windows.MessageBox.Show("Cannot find winnsi.dll. Please make sure launcher is in your main game folder!", "Missing files", MessageBoxButton.OK, MessageBoxImage.Error);
                // update small version info at bottom left corner
                Dispatcher.Invoke((Action)(() =>
                {
                    DownloadInfoBox.Text = "Missing files.\nContact staff for more help.";
                    DownloadInfoBox.Foreground = new SolidColorBrush(Colors.Red);

                    StartGameButton.IsEnabled = false;
                    StartGameButton.Foreground = new SolidColorBrush(Colors.Silver);
                }));
            }
            else
            {
                CheckGameVersion();
            }
        }
        #endregion

        #region DownloadInfoBox animation
        /// <summary>
        /// Create animation for information box
        /// </summary>
        /// <param name="from"></param>
        /// <param name="signal"></param>
        private void ShowHideDownloadInfo(double from, string signal)
        {
            var da = new DoubleAnimation();
            da.Duration = TimeSpan.FromSeconds(0.5);
            da.From = from;
            if (signal == "1line")
            {
                if (from != 40)
                {
                    da.To = 40;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        DownloadInfoBox.BeginAnimation(HeightProperty, da);
                    }));
                }
            }
            else if (signal == "2lines")
            {
                if (from != 70)
                {
                    da.To = 70;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        DownloadInfoBox.BeginAnimation(HeightProperty, da);
                    }));
                }
            }
            else if (signal == "3lines")
            {
                if (from != 105)
                {
                    da.To = 105;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        DownloadInfoBox.BeginAnimation(HeightProperty, da);
                    }));
                }
            }
            else
            {
                da.To = 0;
                Dispatcher.Invoke((Action)(() =>
                {
                    DownloadInfoBox.BeginAnimation(HeightProperty, da);
                }));
            }
        }
        #endregion

        #region All button click events of Main Game Page
        /// <summary>
        /// Buttons click events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                ShowHideDownloadInfo(DownloadInfoBox.Height, "reset");
            }));
            StartGame();
        }

        private void NewsButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItemSetTransitions(0);
            new Thread(delegate ()
            {
                PullRSSFeed("http://requiemnetwork.com/forum/44-server-news.xml/", "http://requiemnetwork.com/forum/44-server-news/");
            }).Start();
        }

        private void PatchNotesButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItemSetTransitions(1);
            new Thread(delegate ()
            {
                PullRSSFeed("http://requiemnetwork.com/forum/39-patch-notes.xml/", "http://requiemnetwork.com/forum/39-patch-notes/");
            }).Start();
        }

        private void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItemSetTransitions(2);
            CheckFilesPath();
        }

        private void DropRateCalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItemSetTransitions(3);
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            if (!File.Exists(mainWindow.dropRateCalculatorPath))
            {
                System.Windows.MessageBox.Show("Cannot find DropRateCalculator in the current folder. Make sure the launcher is in main game folder!",
                                                                                             "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Process dropCalculator = new Process();
                try
                {
                    dropCalculator.EnableRaisingEvents = true;
                    dropCalculator.Exited += DropCalculator_Exited; ;
                    dropCalculator.StartInfo.FileName = mainWindow.dropRateCalculatorPath;
                    dropCalculator.Start();
                }
                catch (Exception e1)
                {
                    System.Windows.MessageBox.Show(e1.Message);
                }
            }

        }

        private void DyeSystemButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://requiemnetwork.com/dye");
        }

        private void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://requiemnetwork.com");
        }

        private void DropCalculator_Exited(object sender, EventArgs e)
        {
            MenuItemSetTransitions(0);
        }
        #endregion

        #region Transition for menu of items in Main Game Page
        /// <summary>
        /// Handle launcher button menu
        /// </summary>
        /// <param name="itemIndex"></param>
        private void MenuItemSetTransitions(int itemIndex)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                NewsButton.Content = "NEWS";
                PatchNotesButton.Content = "PATCH NOTES";
                CheckForUpdatesButton.Content = "CHECK FOR UPDATES";
                DropRateCalculatorButton.Content = "CALCULATOR";
                DyeSystemButton.Content = "DYE SYSTEM";
                WebsiteButton.Content = "WEBSITE";

                NewsButton.Width = 380;
                PatchNotesButton.Width = 380;
                CheckForUpdatesButton.Width = 380;
                DropRateCalculatorButton.Width = 380;
                DyeSystemButton.Width = 380;
                WebsiteButton.Width = 380;

                NewsButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#7FFFFFFF");
                PatchNotesButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#7FFFFFFF");
                CheckForUpdatesButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#7FFFFFFF");
                DropRateCalculatorButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#7FFFFFFF");
                DyeSystemButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#7FFFFFFF");
                WebsiteButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#7FFFFFFF");

            }));

            switch (itemIndex)
            {
                case 1:
                    Dispatcher.Invoke((Action)(() =>
                    {
                        PatchNotesButton.Content = ">PATCH NOTES";
                        PatchNotesButton.Width = 415;
                        PatchNotesButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#BFFFFFFF");
                    }));
                    break;

                case 2:
                    Dispatcher.Invoke((Action)(() =>
                    {
                        CheckForUpdatesButton.Content = ">CHECK FOR UPDATES";
                        CheckForUpdatesButton.Width = 415;
                        CheckForUpdatesButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#BFFFFFFF");
                    }));
                    break;

                case 3:
                    Dispatcher.Invoke((Action)(() =>
                    {
                        DropRateCalculatorButton.Content = ">CALCULATOR";
                        DropRateCalculatorButton.Width = 415;
                        DropRateCalculatorButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#BFFFFFFF");
                    }));
                    break;

                /*case 4:
                    Dispatcher.Invoke((Action)(() =>
                    {
                        DyeSystemButton.Content = ">DYE SYSTEM";
                        DyeSystemButton.Width = 415;
                        DyeSystemButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#BFFFFFFF");
                    }));
                    break;

                case 5:
                    Dispatcher.Invoke((Action)(() =>
                    {
                        WebsiteButton.Content = ">WEBSITE";
                        WebsiteButton.Width = 415;
                        WebsiteButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#BFFFFFFF");
                    }));
                    break; */

                default:
                    Dispatcher.Invoke((Action)(() =>
                    {
                        NewsButton.Content = ">NEWS";
                        NewsButton.Width = 415;
                        NewsButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#BFFFFFFF");
                    }));
                    break;
            }
        }

        #endregion

    }
}
