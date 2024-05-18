using System;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
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
            Cycle();
        }
        private async void Cycle()
        {
            while (true) 
            {
                await Task.Delay(5000);
                SetCpuUsage();
                SetRamUsage();
            }
        }
        private async void WaitForData()
        {
            Loading.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                GetInfoProc();
                SetRamUsage();
                SetCpuUsage();
            });
            await Task.Delay(1000);
            Loading.Visibility = Visibility.Collapsed;
            ProcInfo.Visibility = Visibility.Visible;
            RAMPieChart.Visibility = Visibility.Visible;
            ProcPieChart.Visibility = Visibility.Visible;
            NetworkPieChart.Visibility = Visibility.Visible;
            GraphicsCardPieChart.Visibility = Visibility.Visible;
            RAM.Visibility = Visibility.Visible;
            CPU.Visibility = Visibility.Visible;
        }
        private async Task SetCpuUsage()
        {
            // Create a PerformanceCounter to get CPU usage
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            // First reading (initialization)
            cpuCounter.NextValue();
            await Task.Delay(1000); // Wait a second to get a valid reading

            // Second reading (actual value)
            float cpuUsage = cpuCounter.NextValue();

            // Use Dispatcher to update the UI
            Dispatcher.Invoke(() =>
            {
                SeriesCollection seriesCollection = new SeriesCollection();

                seriesCollection.Add(new PieSeries
                {
                    Title = $"Processor load {cpuUsage:F2}%",
                    Values = new ChartValues<double> { cpuUsage }
                });

                float freeCpu = 100 - cpuUsage;
                seriesCollection.Add(new PieSeries
                {
                    Title = $"Amount of free space {freeCpu:F2}%",
                    Values = new ChartValues<double> { freeCpu }
                });

                ProcPieChart.Series = seriesCollection;
            });
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
        private async void SetRamUsage()
        {
            // Get total physical memory
            ulong totalMemoryBytes = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalMemoryBytes = (ulong)obj["TotalVisibleMemorySize"] * 1024; // Value in KB, converting to bytes
                }
            }
            await Task.Delay(1000); // Wait a second to get a valid reading
            // Get available memory
            PerformanceCounter availableCounter = new PerformanceCounter("Memory", "Available Bytes");
            ulong availableMemoryBytes = (ulong)availableCounter.RawValue;

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
                    Title = $"Available memory {availableMemory:F2} MB",
                    Values = new ChartValues<double> { availableMemory }
                });
                seriesCollection.Add(new PieSeries
                {
                    Title = $"Used memory {usedMemory:F2} MB",
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
