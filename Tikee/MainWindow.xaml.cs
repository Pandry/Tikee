using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Threading;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Interop;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;
using System.Resources;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Tikee.Resources.Errors;
using Tikee.Resources.Core;
using Tikee.Resources.UI;


namespace Tikee
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StringDictionary mainConfigArray = new StringDictionary();
        StringDictionary defaultConfigArray = new StringDictionary();

        readonly ResourceManager[] configResourcesArray  = { DefaultUIValues.ResourceManager, DefaultSettingsValues.ResourceManager };

       

        TimeSpan defaultPauseTimeSpan = TimeSpan.Parse(DefaultSettingsValues.DefaultPauseString);
        TimeSpan idleDisplayTresholdTimeSpan = TimeSpan.Parse(DefaultSettingsValues.IdleDisplayTresholdString);

        int[] backgroundPopup = new int[] {5};

        private bool addictionMode = false;


        #region Get Cursor Posion func

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        #endregion

        private Point latestMousePosition;

        //Main timer Init
        System.Windows.Threading.DispatcherTimer mainTimer = new System.Windows.Threading.DispatcherTimer();

        private TimeSpan currentTimespan;
        private TimeSpan settedTimespan;

        private TimeSpan currentMouseIdleTime = new TimeSpan(0, 0, 0);
        private bool isIdle = false;
        #region Configuration reading/settting/getting
        public string getConfigValue(string configKey)

        {
            configKey = configKey.ToLower();
            if (mainConfigArray[configKey] != null)
            {
                return mainConfigArray[configKey];
            }
            if (defaultConfigArray[configKey] != null)
            {
                return defaultConfigArray[configKey];
            }
            throw new ArgumentException("The key does not have a value");
        }

        private void readDefaultConfig()
        {
            foreach (var rm in configResourcesArray)
                foreach (DictionaryEntry v in rm.GetResourceSet(CultureInfo.InvariantCulture, false, false))
                    defaultConfigArray[(string)v.Key] = (string)v.Value;
        }

        private void readConfig(string configPath = null, bool defaultFallback = false)
        {
            if (defaultFallback || configPath == null)
                configPath = getConfigValue("defaultConfigFileLocation");


            if (defaultFallback)
            {
                //Load UI values
                foreach (var rm in configResourcesArray)
                    foreach (DictionaryEntry v in rm.GetResourceSet(CultureInfo.InvariantCulture, false, false))
                        mainConfigArray[(string)v.Key] = (string)v.Value;
            }
            else if (File.Exists(configPath))
            {
                IniData data = new FileIniDataParser().ReadFile(configPath);
                var enums = new[] {data["UI"].GetEnumerator(), data["Tresholds"].GetEnumerator()};
                foreach (var en in enums)
                    do
                    {
                        var val = en.Current;
                        if (val != null)
                            mainConfigArray[val.KeyName] = val.Value;
                    } while ((en.MoveNext()));
            }
            else
            {
                if (!Directory.Exists(configPath))
                {
                    #region ConfigDirectoryCreation

                    try
                    {
                        //Creates directory recursively
                        Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                    }
                    catch (NotSupportedException)
                    {
                        MessageBox.Show(Tikee.Resources.Errors.ErrorStrings.ConfigDirectoryCreation_NotSupportedException);
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return;
                    }
                    catch
                    {
                        return;
                    }

                    #endregion
                }
                //I cannot write the config file vie the library since it doesn't support comment writing yet

                #region ConfigFileGeneration

                try
                {
                    using (var sw = new StreamWriter(configPath))
                    {
                        sw.Write(DefaultSettingsValues.ConfigFileContent);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    return;
                }
                catch (IOException)
                {
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    readConfig(null, true);
                    return;
                }
                catch (System.Security.SecurityException)
                {
                    return;
                }
                catch
                {
                    return;
                }

                #endregion

                //TODO implement error window
                readConfig(configPath);
            }
        }

#endregion

        private Brush hexToBrush(string color)
        {
            var c = (Color) ColorConverter.ConvertFromString(color);
            return new SolidColorBrush(c);
        }

        //TODO use windows native API to see idle time and/or last input
        //https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getlastinputinfo
        //

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            currentTimespan -= new TimeSpan(0, 0, 1);
            //Determine if is idle
            if (currentMouseIdleTime > defaultPauseTimeSpan)
            {
                //It seems like that's a pause
                isIdle = true;
                HomeWindow.Background = hexToBrush(getConfigValue("idleBackground"));
            }
            if (isIdle || currentMouseIdleTime > idleDisplayTresholdTimeSpan)
            {
                //If there's a pause (mouse don't move from a bit (NOT PAUSE))
                if (isIdle)
                {
                    //If is idle 
                    this.Topmost = true;
                    this.Activate();
                    this.BringIntoView();
                    this.Focus();
                    this.Topmost = false;
                }
                if (defaultPauseTimeSpan >= currentMouseIdleTime)
                {
                    HomeWindow.Background = hexToBrush(getConfigValue("idleBackground"));
                    //ClockTxt.Text = currentMouseIdleTime.ToString(@"hh\:mm\:ss");
                    ClockTxt.Text = (defaultPauseTimeSpan - currentMouseIdleTime).ToString(@"hh\:mm\:ss");
                }
                else
                {
                    HomeWindow.Background = hexToBrush(getConfigValue("PauseOverBackground"));
                    //ClockTxt.Text = currentMouseIdleTime.ToString(@"hh\:mm\:ss");
                    ClockTxt.Text = (defaultPauseTimeSpan - currentMouseIdleTime).ToString(@"hh\:mm\:ss");
                }
            }
            else
            {
                //The timer is running normally
                HomeWindow.Background = hexToBrush(getConfigValue("timerRunningBackground"));
                ClockTxt.Text = currentTimespan.ToString(@"hh\:mm\:ss");
            }
            var newMousePosition = GetMousePosition();
            if (latestMousePosition == newMousePosition)
            {
                currentMouseIdleTime += new TimeSpan(0, 0, 1);
            }
            else
            {
                latestMousePosition = newMousePosition;
                currentMouseIdleTime = new TimeSpan(0, 0, 0);
                if (isIdle)
                {
                    //user is back, was in idle
                    //Restart timer
                    currentTimespan = settedTimespan;
                    HomeWindow.Background = hexToBrush(getConfigValue("timerRunningBackground"));

                    if (addictionMode)
                    {
                        Closing -= OnClosing;

                        CloseBtn.Visibility = Visibility.Visible;
                        MinimizeBtn.Visibility = Visibility.Visible;
                        MainBtn.Visibility = Visibility.Visible;
                        this.WindowState = WindowState.Normal;
                    }
                }
                isIdle = false;
            }
            if (!isIdle && currentTimespan.TotalSeconds <= 0)
            {
                if (currentTimespan.TotalSeconds == 0 ||
                    backgroundPopup.Any(x => currentTimespan.TotalSeconds % x == 0))
                {
                    this.Topmost = true;
                    this.Activate();
                    this.BringIntoView();
                    this.Focus();
                    if (addictionMode)
                    {
                        setAddictedMode(true);
                        MainBtn.Visibility = Visibility.Hidden;
                    }
                    this.Topmost = false;
                }
                if (HomeWindow.Background != hexToBrush(getConfigValue("idleBackground")));
                    HomeWindow.Background = hexToBrush(getConfigValue("timeOverBackground"));
            }
        }

        
        public MainWindow()
        {
            readDefaultConfig();
            readConfig(null, true);

            InitializeComponent();
            HomeWindow.Background = hexToBrush(getConfigValue("timerRunningBackground"));
            MainBtn.Content = "START";
            mainTimer.Tick += new EventHandler(mainTimer_Tick);
            HomeWindow.Background = hexToBrush(getConfigValue("defaultBackground"));
            latestMousePosition = GetMousePosition();

            ClockTxt.Text = getConfigValue("defaultTimeString");



        }
        
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void OnCloseBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnMainBtnClick(object sender, RoutedEventArgs e)
        {
            latestMousePosition = GetMousePosition();
            if (mainTimer.IsEnabled)
            {
                setAddictedMode(false);
                MainBtn.Visibility = Visibility.Visible;
                mainTimer.Stop();
                ClockTxt.Text = getConfigValue("defaultTimeString");
                MainBtn.Content = "START";
                ClockTxt.IsReadOnly = false;
                HomeWindow.Background = hexToBrush(getConfigValue("defaultBackground"));
            }
            else
            {
                TimeSpan ts;
                try
                {
                    ts = TimeSpan.Parse(ClockTxt.Text);
                }
                catch (ArgumentNullException)
                {
                    MessageBox.Show("String empty! Blah blah");
                    ClockTxt.Text = getConfigValue("defaultTimeString");
                    return;
                }
                catch (FormatException)
                {
                    MessageBox.Show("The format isn't fine! Blah blah");

                    ClockTxt.Text = getConfigValue("defaultTimeString");
                    return;
                }
                catch (Exception)
                {
                    MessageBox.Show("Something bad happened...");
                    return;
                }

                if (ts > new TimeSpan(23, 59, 59))
                {
                    MessageBox.Show("Something bad happened...");
                    return;
                }

                ClockTxt.Text = ts.ToString(@"hh\:mm\:ss");
                mainTimer.Interval = new TimeSpan(0, 0, 1);

                settedTimespan = ts;
                currentTimespan = ts;
                mainTimer.Start();


                ClockTxt.IsReadOnly = true;
                MainBtn.Content = "RESET";
                HomeWindow.Background = hexToBrush(getConfigValue("timerRunningBackground"));

                //MessageBox.Show("YA BETTER SHTAP!");
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnMinimizeBtnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            
        }

        #region Extreme cases functions - not used
        //https://stackoverflow.com/a/16611142
        private void setTaskManager(bool enable)
        {
            RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            if (enable && objRegistryKey.GetValue("DisableTaskMgr") != null)
                objRegistryKey.DeleteValue("DisableTaskMgr");
            else
                objRegistryKey.SetValue("DisableTaskMgr", "1");
            objRegistryKey.Close();
        }

        [DllImport("user32", EntryPoint = "SetWindowsHookExA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, int hMod, int dwThreadId);
        [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int UnhookWindowsHookEx(int hHook);
        public delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        [DllImport("user32", EntryPoint = "CallNextHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        public const int WH_KEYBOARD_LL = 13;

        /*code needed to disable start menu*/
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        public static int intLLKey;

        public int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            bool blnEat = false;

            switch (wParam)
            {
                case 256:
                case 257:
                case 260:
                case 261:
                    //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key,
                    blnEat = ((lParam.vkCode == 9) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 0)) | ((lParam.vkCode == 91) && (lParam.flags == 1)) | ((lParam.vkCode == 92) && (lParam.flags == 1)) | ((lParam.vkCode == 73) && (lParam.flags == 0));
                    break;
            }

            if (blnEat == true)
            {
                return 1;
            }
            else
            {
                return CallNextHookEx(0, nCode, wParam, ref lParam);
            }
        }
        public void KillStartMenu()
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_HIDE);
        }


        public void KillCtrlAltDelete()
        {
            RegistryKey regkey;
            string keyValueInt = "1";
            string subKey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";

            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion


        private void OnClosing(object sender,CancelEventArgs args)
        {
            args.Cancel = true;
        }

        private void setAddictedMode(bool enable)
        {
            if (enable)
            {
                Closing += OnClosing;

                CloseBtn.Visibility = Visibility.Hidden;
                MinimizeBtn.Visibility = Visibility.Hidden;
                this.WindowState = WindowState.Maximized;
                //setTaskManager(false);
            }
            else
            {
                Closing -= OnClosing;
                
                CloseBtn.Visibility = Visibility.Visible;
                MinimizeBtn.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Normal;
                //setTaskManager(true);
            }
        }

    }
}
