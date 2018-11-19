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

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Colors from https://flatuicolors.com/palette/de
        SolidColorBrush GreenBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26de81"));
        SolidColorBrush RedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff4757"));
        SolidColorBrush OrangeBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fa8231"));

        string defaultTimeString = "01:00:00";
        int[] backgroundPopup = new int[] {1,5};
        
        
        //Main timer Init
        System.Windows.Threading.DispatcherTimer mainTimer = new System.Windows.Threading.DispatcherTimer();

        private TimeSpan CurrestTimespan;

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            CurrestTimespan -= new TimeSpan(0, 0, 1);
            ClockTxt.Text = CurrestTimespan.ToString(@"hh\:mm\:ss");
            if (CurrestTimespan.TotalSeconds < 0)
            {
                if (backgroundPopup.Any(x => CurrestTimespan.TotalSeconds % x == 0))
                {
                    this.Activate();
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
            if (mainTimer.IsEnabled)
            {

                mainTimer.Stop();
                ClockTxt.Text = defaultTimeString;
                MainBtn.Content = "START";
                HomeWindow.Background = OrangeBackground;
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

                CurrestTimespan = ts;
                mainTimer.Start();



                MainBtn.Content = "STOP";
                HomeWindow.Background = GreenBackground;

                //MessageBox.Show("YA BETTER SHTAP!");
            }
        }
    }
}
