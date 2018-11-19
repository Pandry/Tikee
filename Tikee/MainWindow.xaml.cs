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
using System.Windows.Threading;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace Tikee
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Colors from https://flatuicolors.com/palette/de
        SolidColorBrush GreenBackground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#26de81"));
        SolidColorBrush BlueBackground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#3867d6")); //Blue when user is in pause
        SolidColorBrush RedBackground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ff4757"));
        SolidColorBrush OrangeBackground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#fa8231"));
        SolidColorBrush GreyBackground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#4b6584"));

        const string defaultTimeString = "01:00:00";
        const string defaultPauseString = "00:15:00";
        const string idleDisplayTresholdString = "00:00:10";

        TimeSpan defaultPauseTimeSpan = TimeSpan.Parse(defaultPauseString);
        TimeSpan idleDisplayTresholdTimeSpan = TimeSpan.Parse(idleDisplayTresholdString);

        int[] backgroundPopup = new int[] {5};

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

        private Point LatestMousePosition;

        //Main timer Init
        System.Windows.Threading.DispatcherTimer mainTimer = new System.Windows.Threading.DispatcherTimer();

        private TimeSpan CurrentTimespan;
        private TimeSpan SettedTimespan;

        private TimeSpan CurrentMouseIdleTime = new TimeSpan(0, 0, 0);
        private bool IsIdle = false;


        private void mainTimer_Tick(object sender, EventArgs e)
        {
            CurrentTimespan -= new TimeSpan(0, 0, 1);
            //Determine if is idle
            if (CurrentMouseIdleTime > defaultPauseTimeSpan)
            {
                //It seems like that's a pause
                IsIdle = true;
                HomeWindow.Background = BlueBackground;
            }
            if (IsIdle || CurrentMouseIdleTime > idleDisplayTresholdTimeSpan)
            {
                if (IsIdle)
                {
                    //If is idle 
                    this.Topmost = true;
                    this.Activate();
                    this.BringIntoView();
                    this.Focus();
                    this.Topmost = false;
                }
                HomeWindow.Background = BlueBackground;
                ClockTxt.Text = CurrentMouseIdleTime.ToString(@"hh\:mm\:ss");
            }
            else
            {
                HomeWindow.Background = GreenBackground;
                ClockTxt.Text = CurrentTimespan.ToString(@"hh\:mm\:ss");
            }
            var newMousePosition = GetMousePosition();
            if (LatestMousePosition == newMousePosition)
            {
                CurrentMouseIdleTime += new TimeSpan(0, 0, 1);
            }
            else
            {
                LatestMousePosition = newMousePosition;
                CurrentMouseIdleTime = new TimeSpan(0, 0, 0);
                if (IsIdle)
                {
                    //user is back, was in idle
                    //Restart timer
                    CurrentTimespan = SettedTimespan;
                    HomeWindow.Background = GreenBackground;
                }
                IsIdle = false;
            }
            if (!IsIdle && CurrentTimespan.TotalSeconds <= 0)
            {
                if (CurrentTimespan.TotalSeconds == 0 ||
                    backgroundPopup.Any(x => CurrentTimespan.TotalSeconds % x == 0))
                {
                    this.Topmost = true;
                    this.Activate();
                    this.BringIntoView();
                    this.Focus();
                    this.Topmost = false;
                }
                HomeWindow.Background = RedBackground;
            }
        }




        public MainWindow()
        {
            InitializeComponent();
            HomeWindow.Background = GreenBackground;
            MainBtn.Content = "START";
            mainTimer.Tick += new EventHandler(mainTimer_Tick);
            HomeWindow.Background = GreyBackground;
            LatestMousePosition = GetMousePosition();

            ClockTxt.Text = defaultTimeString;
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
            LatestMousePosition = GetMousePosition();
            if (mainTimer.IsEnabled)
            {

                mainTimer.Stop();
                ClockTxt.Text = defaultTimeString;
                MainBtn.Content = "START";
                ClockTxt.IsReadOnly = false;
                HomeWindow.Background = GreyBackground;
            }
            else
            {
                TimeSpan ts;
                try
                {
                    ts = TimeSpan.Parse(ClockTxt.Text);
                }
                catch (ArgumentNullException ex)
                {
                    MessageBox.Show("String empty! Blah blah");
                    ClockTxt.Text = defaultTimeString;
                    return;
                }
                catch (FormatException ex)
                {
                    MessageBox.Show("The format isn't fine! Blah blah");

                    ClockTxt.Text = defaultTimeString;
                    return;
                }
                catch (Exception ex)
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

                SettedTimespan = ts;
                CurrentTimespan = ts;
                mainTimer.Start();


                ClockTxt.IsReadOnly = true;
                MainBtn.Content = "RESET";
                HomeWindow.Background = GreenBackground;

                //MessageBox.Show("YA BETTER SHTAP!");
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Activate();
            this.BringIntoView();
            this.Focus();
            this.Topmost = false;
        }

        private void OnMinimizeBtnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            
        }
    }
}
