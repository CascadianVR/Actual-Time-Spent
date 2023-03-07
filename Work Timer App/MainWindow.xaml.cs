using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Work_Timer_App
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

        Timer tmr = new Timer();
        Stopwatch stopwatch = new Stopwatch();
        List<string> SelectedProcesses = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            GetProcesses();
            //AllocConsole();

            tmr.Interval = 200;
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
            PauseButton.Content = "Pause";
        }

        private void StopTimer(object sender, RoutedEventArgs e)
        {
            tmr.Stop();
            PauseButton.Content = "Paused";
        }

        private void Tmr_Tick(Object source, ElapsedEventArgs e)
        {
            uint procId;
            IntPtr handle = GetForegroundWindow();
            GetWindowThreadProcessId(handle, out procId);
            if (procId == 0) return;
            Process proc = Process.GetProcessById((int)procId);
            string name = proc.ProcessName;

            bool constainsName = ContainsName(name);

            if (constainsName && !stopwatch.IsRunning) stopwatch.Start();
            else stopwatch.Stop();
            //Console.WriteLine(constainsName + " | " + System.MathF.Round((float)stopwatch.Elapsed.TotalSeconds));
            Action act = () => { ElapsedTime.Content = System.MathF.Round((float)stopwatch.Elapsed.TotalSeconds) + " Seconds"; };
            ElapsedTime.Dispatcher.Invoke(act);
            //Console.WriteLine(ElapsedTime.Content.ToString());
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
