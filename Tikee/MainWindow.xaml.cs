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
using System.Linq.Expressions;
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

        ResourceManager[] configResourcesArray =
            {Tikee.Resources.UI.DefaultUIValues.ResourceManager, DefaultSettingsValues.ResourceManager};

        private bool PauseIsOver = false;

        #region Get Idle time 

        [DllImport("user32.dll")]
        //https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getlastinputinfo
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        //https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-taglastinputinfo
        /*
         typedef struct tagLASTINPUTINFO {
              UINT  cbSize;
              DWORD dwTime;
            } LASTINPUTINFO, *PLASTINPUTINFO;
         */

        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            public UInt32 cbSize; //this structure size
            
            public UInt32 dwTime; //tick count  when last event was received
            // it is actually a ulong: https://docs.microsoft.com/en-us/windows/desktop/winprog/windows-data-types
            // DWORD 	A 32-bit unsigned integer. The range is 0 through 4294967295 decimal. 
            //This type is declared in IntSafe.h as follows:
            //typedef unsigned long DWORD;
        }

        static TimeSpan GetIdleTimeSpan()
        {
            ulong idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint) Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;
            ulong envTicks = (ulong) Environment.TickCount;
            if (GetLastInputInfo(ref lastInputInfo))
            {
                ulong lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;

            }
            // For some freacking funny funky reason, when passing the ticks to the timespan, they needs to be multiplied by TimeSpan.TicksPerMillisecond
            return new TimeSpan((long)idleTime * TimeSpan.TicksPerMillisecond);
        }




        #endregion

        private Point latestMousePosition;

        //Main timer Init
        System.Windows.Threading.DispatcherTimer mainTimer = new System.Windows.Threading.DispatcherTimer();

        private TimeSpan timerCounterTimespan;
        private TimeSpan settedTimespan;//Latest settet timespan

        //private TimeSpan currentMouseIdleTime = new TimeSpan(0, 0, 0);
        //private bool isIdle = false;

        #region Configuration reading/settting/getting/helpers

        public List<int> textToArray(string text, Func<int, bool> lmbFunc = null)
        {
            lmbFunc = lmbFunc != null? lmbFunc : (_ =>true );
            var splits = text.Split(',');
            var res = new List<int>();
            foreach (var number in splits)
            {
                int x;
                if (int.TryParse(number, out x))
                {
                    if (lmbFunc(x))
                    {
                        res.Add(x);
                    }
                }
            }
            return res;
        }
        
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
            return null;
        }

        private void readDefaultConfig()
        {
            foreach (ResourceManager rm in configResourcesArray)
            {
                if (rm == null || configResourcesArray == null)
                {
                    throw new Exception();
                }
                var tstst = rm.GetResourceSet(CultureInfo.InvariantCulture, true, false);
                foreach (DictionaryEntry v in tstst)
                    defaultConfigArray[(string) v.Key] = (string) v.Value;
            }
        }

        private void readConfig(string configPath = null, bool defaultFallback = false)
        {
            if (defaultFallback || configPath == null)
                configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tikee", getConfigValue("ConfigFileName"));


            if (defaultFallback)
            {
                //Load UI values
                foreach (var rm in configResourcesArray)
                    foreach (DictionaryEntry v in rm.GetResourceSet(CultureInfo.InvariantCulture, true, false))
                        mainConfigArray[(string)v.Key] = (string)v.Value;
            }
            else if (File.Exists(configPath))
            {
                IniData data = new FileIniDataParser().ReadFile(configPath);
                var enums = new[] {data["UI"].GetEnumerator(), data["Tresholds"].GetEnumerator(), data["Settings"].GetEnumerator() };
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
                        sw.Write(getConfigValue("ConfigFileContent"));
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
        

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            //Decrement the timespan that acts like a coutdown
            timerCounterTimespan -= new TimeSpan(0, 0, 1);

            // Let's get the idle time
            var currentIdleTime = GetIdleTimeSpan();

            //If there's no longer time
            if (!(PauseIsOver && !(currentIdleTime > TimeSpan.Parse(getConfigValue("DefaultPauseString")))) &&  timerCounterTimespan <= new TimeSpan(0L) )
            {
                // The time is over
                // Need to bring the window into view
                //  But that just if it's a thing to do
                bool timeToSnooze = textToArray(getConfigValue("BackgroundPopupIntervals"), (x => x >= 0))
                    .Any(x => timerCounterTimespan.TotalSeconds % x == 0);
                //If it's time to snooze, pop in
                if (timeToSnooze && !PauseIsOver)
                {
                    this.Topmost = true;
                    this.Activate();
                    this.BringIntoView();
                    this.Focus();
                    //If it's in addiction mode, enter in addicted mode and do not let the popup resize, otherwise, do the opposite
                    if (getConfigValue("BackgroundPopupIntervals") == "false")
                        this.Topmost = false;
                }
                // Now it's time to set the background color
                // Now, let's see if we should set the background to the pause state state:

                //Was the user in pause and the pause is over?
                if (currentIdleTime > TimeSpan.Parse(getConfigValue("DefaultPauseString")))
                {
                    // Ok, the user is in a pause state, but the pause period is over
                    PauseIsOver = true;
                    // We can change again the color
                    HomeWindow.Background = hexToBrush(getConfigValue("PauseOverBackground"));
                    //And exit of a possible addicted mode
                    setAddictedMode(false);
                    //And set the current labed to the time passed from the end of the idle time
                    ClockTxt.Text = (TimeSpan.Parse(getConfigValue("DefaultPauseString")) - currentIdleTime).ToString(@"hh\:mm\:ss");

                }
                //Is the user in idle at least
                else if (currentIdleTime > TimeSpan.Parse(getConfigValue("IdleDisplayTresholdString")))
                {
                    //Well, the computer is idle
                    HomeWindow.Background = hexToBrush(getConfigValue("IdleBackground"));
                    if (getConfigValue("BackgroundPopupIntervals") != "false")
                        setAddictedMode(true);
                    //And set the current labed to the idle time
                    ClockTxt.Text = (TimeSpan.Parse(getConfigValue("DefaultPauseString")) - currentIdleTime).ToString(@"hh\:mm\:ss");
                }
                else
                {
                    // The time is over, but the computer is not in a idle state :c 
                    HomeWindow.Background = hexToBrush(getConfigValue("timeOverBackground"));
                    if (getConfigValue("BackgroundPopupIntervals") != "false")
                        setAddictedMode(true);
                    //And set the current labed to the passed time
                    ClockTxt.Text = (timerCounterTimespan).ToString(@"hh\:mm\:ss");
                }
            }
            else
            {
                if (PauseIsOver)
                {
                    PauseIsOver = false;
                    this.Topmost = false;
                    //reset timer 
                    timerCounterTimespan = settedTimespan;
                }


                //Well, time is passing by normally,TimerRunningBackground
                HomeWindow.Background = hexToBrush(getConfigValue("TimerRunningBackground"));
                ClockTxt.Text = (timerCounterTimespan).ToString(@"hh\:mm\:ss");
            }
            
        }

        
        public MainWindow()
        {
            //configResourcesArray = new [] { Tikee.Resources.UI.DefaultUIValues.ResourceManager, DefaultSettingsValues.ResourceManager };
            readDefaultConfig();
            readConfig();
            //readConfig(null, true);

            InitializeComponent();
            HomeWindow.Background = hexToBrush(getConfigValue("timerRunningBackground"));
            MainBtn.Content = "START";
            mainTimer.Tick += new EventHandler(mainTimer_Tick);
            HomeWindow.Background = hexToBrush(getConfigValue("defaultBackground"));
            //latestMousePosition = GetMousePosition();

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
            setAddictedMode(false);
            this.Close();
        }

        private void OnMainBtnClick(object sender, RoutedEventArgs e)
        {
            //latestMousePosition = GetMousePosition();
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
                PauseIsOver = false;
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
                timerCounterTimespan = ts;
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

        //TODO if PC shutdown, permit 
        private void AbortClosing(object sender,CancelEventArgs args)
        {
            args.Cancel = true;
        }

        private void setAddictedMode(bool enable)
        {
            if (enable)
            {
                Closing += AbortClosing;

                CloseBtn.Visibility = Visibility.Hidden;
                MinimizeBtn.Visibility = Visibility.Hidden;
                this.WindowState = WindowState.Maximized;
                MainBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                Closing -= AbortClosing;
                
                CloseBtn.Visibility = Visibility.Visible;
                MinimizeBtn.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Normal;
                MainBtn.Visibility = Visibility.Visible;
            }
        }

    }
}
