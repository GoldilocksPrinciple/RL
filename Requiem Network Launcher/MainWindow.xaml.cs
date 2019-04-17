using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Requiem_Network_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Global variables
        public string rootDirectory;
        public string dllPath;
        public string processPath;
        public string versionPath;
        public string updatePath;
        public string dropRateCalculatorPath;
        public string currentVersionLocal;
        public string launcherInfoPath;
        public bool waitingForRestart = false;
        private NotifyIcon _nIcon;
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += Window_SourceInitialized;
            NotifyIconSetup();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUserInfo();
            launcherInfoPath = rootDirectory + "\\info.txt";
            try
            {
                File.WriteAllText(launcherInfoPath, "LauncherVersion=" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                MainFrame.Content = new LoginPage();
            }
            catch (ArgumentException e1)
            {
                System.Windows.MessageBox.Show(e1.Message);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }
        #endregion

        #region Check user info: DotNet version, hwid, user login info, file path
        private void CheckUserInfo()
        {
            CheckDotNetVersion.Get45PlusFromRegistry();

            if (CheckDotNetVersion.DotnetKey != "Henri")
            {
                System.Windows.MessageBox.Show("You are using .NET Framework version: " + CheckDotNetVersion.DotnetKey + ".\nPlease update to 4.6.1 or later to use the Launcher!",
                                                                                                ".NET Framework Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Process.Start("https://dotnet.microsoft.com/download/dotnet-framework-runtime");
                this.Close();
            }
            else if (CheckDotNetVersion.DotnetKey == "")
            {
                System.Windows.MessageBox.Show("You are using a very outdated .NET Framework version!\nPlease install version 4.6.1 or later to use the Launcher!",
                                                                                                ".NET Framework Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Process.Start("https://dotnet.microsoft.com/download/dotnet-framework-runtime");
                this.Close();
            }
            
            HardwareID.GetHardwareID();
            UserInfoRegistry.GetUserLoginInfo();
           
            // get current directory of the Launcher
            rootDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            // set path for winnsi.dll and Vindictus.exe, version.txt and LauncherUpdater.exe
            try
            {
                dllPath = System.IO.Path.Combine(rootDirectory, "winnsi.dll");
                processPath = System.IO.Path.Combine(rootDirectory, "Vindictus.exe");
                versionPath = System.IO.Path.Combine(rootDirectory, "version.txt");
                dropRateCalculatorPath = System.IO.Path.Combine(rootDirectory, "DropRatesCalculator.exe");
            }
            catch (ArgumentException e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region Setup system tray icon
        private void NotifyIconSetup()
        {
            _nIcon = new NotifyIcon();
            _nIcon.Icon = new System.Drawing.Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/launcher_icon.ico")).Stream);
            _nIcon.Visible = true;
            _nIcon.DoubleClick += delegate (object sender, EventArgs e) { this.Show(); this.WindowState = WindowState.Normal; };
            _nIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _nIcon.ContextMenuStrip.Items.Add("Forum", null, this.Forum_Click);
            _nIcon.ContextMenuStrip.Items.Add("Drop Rates Calculator", null, this.DRC_Click);
            _nIcon.ContextMenuStrip.Items.Add("Dye System", null, this.DyeSite_Click);
            _nIcon.ContextMenuStrip.Items.Add("Logout", null, this.LogoutMenu_Click);
            _nIcon.ContextMenuStrip.Items.Add("Exit", null, this.MenuExit_Click);
            _nIcon.Text = "Requiem Network Launcher";
        }

        private void Forum_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://requiemnetwork.com");
        }

        private void DRC_Click(object sender, EventArgs e)
        {
            if (!File.Exists(dropRateCalculatorPath))
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
                    dropCalculator.StartInfo.FileName = dropRateCalculatorPath;
                    dropCalculator.Start();
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("There is already an instance of the game running...", "Error");
                }
            }
        }

        private void DyeSite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://requiemnetwork.com/dye");
        }
        
        private void MenuExit_Click(object sender, EventArgs e)
        {
            _nIcon.Visible = false;
            this.Close();
        }

        private void LogoutMenu_Click(object sender, EventArgs e)
        {
            Logout();
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                _nIcon.BalloonTipText = "Launcher has been minimized to system tray";
                _nIcon.BalloonTipTitle = "Requiem Network";
                _nIcon.ShowBalloonTip(1000);
                this.Hide();
            }
            base.OnStateChanged(e);
        }
        
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            _nIcon.Visible = false;
            Environment.Exit(0);
        }

        #endregion

        #region Logout function
        private void Logout()
        {
            if (LogoutButton.Content.ToString().Contains("LOGOUT"))
            {
                UserInfoRegistry.ClearUserLoginInfo();

                MainFrame.Content = new LoginPage();

                Dispatcher.Invoke((Action)(() =>
                {
                    LogoutButton.Content = "LOGIN";
                }));
            }
        }
        #endregion

        #region Navigating Animation
        /// <summary>
        /// Custom navigation animation for navigation between pages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainFrame_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            
            var da = new DoubleAnimation();
            da.Duration = TimeSpan.FromSeconds(0.7);
            if (e.NavigationMode == NavigationMode.New)
            {
                da.From = 0;
                da.To = 1;
            }
            Dispatcher.Invoke((Action)(() =>
            {
                (e.Content as Page).BeginAnimation(OpacityProperty, da);
            }));
            if (e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
            }
        }
        #endregion

        #region Custom window resize - auto calculate ratio
        private double _aspectRatio;
        private bool? _adjustingHeight = null;

        internal enum SWP
        {
            NOMOVE = 0x0002
        }
        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046,
            EXITSIZEMOVE = 0x0232,
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static Point GetMousePosition() // mouse position relative to screen
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);

            _aspectRatio = this.Width / this.Height;
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                        if ((pos.flags & (int)SWP.NOMOVE) != 0)
                            return IntPtr.Zero;

                        Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                            return IntPtr.Zero;

                        // determine what dimension is changed by detecting the mouse position relative to the 
                        // window bounds. if gripped in the corner, either will work.
                        if (!_adjustingHeight.HasValue)
                        {
                            Point p = GetMousePosition();

                            double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                            double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

                            _adjustingHeight = diffHeight > diffWidth;
                        }

                        if (_adjustingHeight.Value)
                            pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
                        else
                            pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    break;
                case WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }

            return IntPtr.Zero;
        }
        #endregion
    }
}
