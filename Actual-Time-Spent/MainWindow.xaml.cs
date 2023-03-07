using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Actual_Time_Spent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hwnd, StringBuilder ss, int count);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        Timer tmr = new Timer();
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch stopwatchiDLE = new Stopwatch();
        List<string> SelectedProcesses = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            GetProcesses();
            AllocConsole();

            tmr.Interval = 10;
            tmr.Elapsed += Tmr_Tick;
        }

        private void GetProcesses(object sender = null, RoutedEventArgs e = null)
        {
            FullProcessList.Items.Clear();

            var running_apps = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();

            foreach (Process p in running_apps)
            {
                FullProcessList.Items.Add(p.ProcessName);
            }
        }
        
        private void ExitApp(object sender = null, RoutedEventArgs e = null)
        {
            Application.Current.Shutdown();
        }

        private void AddSelectedProcess(object sender, RoutedEventArgs e)
        {
            if (FullProcessList.SelectedItem == null) return;
            SelectedProcessList.Items.Add(FullProcessList.SelectedItem);
            SelectedProcesses.Add(FullProcessList.SelectedItem.ToString());
        }

        private void RemoveSelectedProcess(object sender, RoutedEventArgs e)
        {
            SelectedProcessList.Items.Remove(SelectedProcessList.SelectedItem);
            SelectedProcesses.Remove(FullProcessList.SelectedItem.ToString());
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void StartTimer(object sender, RoutedEventArgs e)
        {
            tmr.Start();
            stopwatchiDLE.Start();
            PauseButton.Content = "Pause";
        }

        private void StopTimer(object sender, RoutedEventArgs e)
        {
            tmr.Stop();
            stopwatchiDLE.Stop();
            PauseButton.Content = "Paused";
        }

        Point lastPoint;
        private void Tmr_Tick(Object source, ElapsedEventArgs e)
        {
            uint procId;
            IntPtr handle = GetForegroundWindow();
            GetWindowThreadProcessId(handle, out procId);
            if (procId == 0) return;
            Process proc = Process.GetProcessById((int)procId);
            string name = proc.ProcessName;

            Point point = new Point();
            GetCursorPos(ref point);

            bool constainsName = ContainsName(name);

            if (constainsName && !stopwatch.IsRunning) stopwatch.Start();
            else if (!constainsName)
            {
                stopwatch.Stop();
                return;
            }

            double timeout = 10;
            Action to = () => { timeout = Convert.ToDouble(Timeout.Text); };
            ElapsedTime.Dispatcher.Invoke(to);

            if (point.X == lastPoint.X)
            {
                if (stopwatchiDLE.Elapsed.TotalSeconds >= timeout)
                {
                    stopwatch.Stop();
                    stopwatchiDLE.Stop();
                }
            }
            else if (point.X != lastPoint.X)
            {
                stopwatchiDLE.Reset();
                stopwatchiDLE.Start();
                stopwatch.Start();
            }

            lastPoint = point;

            Action act = () => { ElapsedTime.Content = stopwatch.Elapsed.Hours + " : " + stopwatch.Elapsed.Minutes + " : " + stopwatch.Elapsed.Seconds; };
            ElapsedTime.Dispatcher.Invoke(act);
        }

        private bool ContainsName(string name)
        {
            foreach (string pname in SelectedProcesses)
            {
                if (pname == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
