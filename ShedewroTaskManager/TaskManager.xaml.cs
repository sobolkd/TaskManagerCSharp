using System;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using ShedewroTaskManager.Models;

namespace ShedewroTaskManager
{
    public partial class TaskManager : Window
    {
        public TaskManager()
        {
            InitializeComponent();
            WaitForData();
        }

        private async void WaitForData()
        {
            Loading.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                GetInfoProc();
                SetRamUsage();
            });
       
            Loading.Visibility = Visibility.Collapsed;
            ProcInfo.Visibility = Visibility.Visible;
            RAMPieChart.Visibility = Visibility.Visible;
            ProcPieChart.Visibility = Visibility.Visible;
            NetworkPieChart.Visibility = Visibility.Visible;
            GraphicsCardPieChart.Visibility = Visibility.Visible;
            RAM.Visibility = Visibility.Visible;
        }
        private void GetInfoProc()
        {
            StringBuilder resultBuilder = new StringBuilder();

            // Processor information
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject processorObj in processorSearcher.Get())
            {
                ProcessorInfo processorInfo = new ProcessorInfo(processorObj);
                resultBuilder.AppendLine("PROCESSOR INFORMATION");
                resultBuilder.AppendLine(processorInfo.GetProcessorInfoString());
            }

            // RAM information
            ManagementObjectSearcher memorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            foreach (ManagementObject memoryObj in memorySearcher.Get())
            {
                RamInfo ramInfo = new RamInfo(memoryObj);
                resultBuilder.AppendLine("");
                resultBuilder.AppendLine($"RAM INFORMATION");
                resultBuilder.AppendLine(ramInfo.GetRamInfoString());
            }

            // Graphics card information
            ManagementObjectSearcher videoSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject videoObj in videoSearcher.Get())
            {
                GraphicsCardInfo graphicsCardInfo = new GraphicsCardInfo(videoObj);
                resultBuilder.AppendLine("");
                resultBuilder.AppendLine("GRAPHICS CARD INFORMATION");
                resultBuilder.AppendLine(graphicsCardInfo.GetGraphicsCardInfoString());
            }

            // Update UI using Dispatcher.Invoke
            Dispatcher.Invoke(() =>
            {
                ProcInfo.Text = resultBuilder.ToString();
            });
        }

        private void SetRamUsage()
        {
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Committed Bytes");
            PerformanceCounter availableCounter = new PerformanceCounter("Memory", "Available Bytes");

            // Get info about RAM
            long totalMemoryBytes = (long)ramCounter.NextValue();
            long availableMemoryBytes = (long)availableCounter.NextValue();

            // Convert in MB
            double totalMemory = totalMemoryBytes / (1024.0 * 1024.0);
            double availableMemory = availableMemoryBytes / (1024.0 * 1024.0);
            double usedMemory = totalMemory - availableMemory;

            // Creating SeriesCollection
            SeriesCollection seriesCollection = new SeriesCollection();
            Dispatcher.Invoke(() =>
            {
                seriesCollection.Add(new PieSeries
                {
                    Title = $"Available memory {availableMemory} (MB)",
                    Values = new ChartValues<double> { availableMemory }
                });
                seriesCollection.Add(new PieSeries
                {
                    Title = $"Used memory {usedMemory} (MB)",
                    Values = new ChartValues<double> { usedMemory }
                });
                RAMPieChart.Series = seriesCollection;
            });

            
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
