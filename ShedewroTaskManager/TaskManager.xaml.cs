using LiveCharts;
using LiveCharts.Wpf;
using ShedewroTaskManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ShedewroTaskManager
{
    public partial class TaskManager : Window
    {
        private ObservableCollection<ProcessData> processesData = new ObservableCollection<ProcessData>();
        private DispatcherTimer loadingTimer;
        private int loadingDotCount = 0;
        string processName;
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
                SetDiskUsage();
                SetNetworkUsage();
            }
        }
        private void MakeVisible()
        {
            Loading.Visibility = Visibility.Collapsed;
            ProcInfo.Visibility = Visibility.Visible;
            RAMPieChart.Visibility = Visibility.Visible;
            ProcPieChart.Visibility = Visibility.Visible;
            NetworkPieChart.Visibility = Visibility.Visible;
            DiscPieChart.Visibility = Visibility.Visible;
            RAM.Visibility = Visibility.Visible;
            CPU.Visibility = Visibility.Visible;
            Disk.Visibility = Visibility.Visible;
            Network.Visibility = Visibility.Visible;
            ShowProcessesButton.Visibility = Visibility.Visible;
            processesDataGrid.Visibility = Visibility.Collapsed;
            EndProcess_Button.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Collapsed;
        }
        private void MakeCollapsed()
        {
            Loading.Visibility = Visibility.Visible;
            ProcInfo.Visibility = Visibility.Collapsed;
            RAMPieChart.Visibility = Visibility.Collapsed;
            ProcPieChart.Visibility = Visibility.Collapsed;
            NetworkPieChart.Visibility = Visibility.Collapsed;
            DiscPieChart.Visibility = Visibility.Collapsed;
            RAM.Visibility = Visibility.Collapsed;
            CPU.Visibility = Visibility.Collapsed;
            Disk.Visibility = Visibility.Collapsed;
            Network.Visibility = Visibility.Collapsed;
            ShowProcessesButton.Visibility = Visibility.Collapsed;
            processesDataGrid.Visibility = Visibility.Visible;
            EndProcess_Button.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
        }
        private async void WaitForData()
        {
            loadingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            loadingTimer.Tick += LoadingTimer_Tick;
            loadingTimer.Start();

            Loading.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                GetInfoProc();
                SetRamUsage();
                SetCpuUsage();
                SetDiskUsage();
                SetNetworkUsage();
            });

            await Task.Delay(1000);
            loadingTimer.Stop();
            MakeVisible();
        }
        private void LoadingTimer_Tick(object sender, EventArgs e)
        {
            loadingDotCount = (loadingDotCount + 1) % 4;
            Loading.Text = "Loading" + new string('.', loadingDotCount);
        }
        private void GetInfoProc()
        {
            StringBuilder resultBuilder = new StringBuilder();

            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject processorObj in processorSearcher.Get())
            {
                ProcessorInfo processorInfo = new ProcessorInfo(processorObj);
                resultBuilder.AppendLine("PROCESSOR INFORMATION");
                resultBuilder.AppendLine(processorInfo.GetProcessorInfoString());
            }

            ManagementObjectSearcher memorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            foreach (ManagementObject memoryObj in memorySearcher.Get())
            {
                RamInfo ramInfo = new RamInfo(memoryObj);
                resultBuilder.AppendLine("");
                resultBuilder.AppendLine($"RAM INFORMATION");
                resultBuilder.AppendLine(ramInfo.GetRamInfoString());
            }

            ManagementObjectSearcher videoSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject videoObj in videoSearcher.Get())
            {
                GraphicsCardInfo graphicsCardInfo = new GraphicsCardInfo(videoObj);
                resultBuilder.AppendLine("");
                resultBuilder.AppendLine("GRAPHICS CARD INFORMATION");
                resultBuilder.AppendLine(graphicsCardInfo.GetGraphicsCardInfoString());
            }

            Dispatcher.Invoke(() =>
            {
                ProcInfo.Text = resultBuilder.ToString();
            });
        }
        private async Task SetCpuUsage()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            await Task.Delay(1000);
            float cpuUsage = cpuCounter.NextValue();

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
        private async Task SetNetworkUsage()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (interfaces.Length == 0)
            {
                MessageBox.Show("No network interfaces found.");
                return;
            }

            NetworkInterface networkInterface = interfaces.FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up);
            if (networkInterface == null)
            {
                MessageBox.Show("No active network interfaces found.");
                return;
            }

            IPv4InterfaceStatistics stats = networkInterface.GetIPv4Statistics();

            long bytesSentInitial = stats.BytesSent;
            long bytesReceivedInitial = stats.BytesReceived;

            await Task.Delay(1000);

            stats = networkInterface.GetIPv4Statistics();
            long bytesSentFinal = stats.BytesSent;
            long bytesReceivedFinal = stats.BytesReceived;

            double sentBytesPerSecond = (bytesSentFinal - bytesSentInitial) / 1.0;
            double receivedBytesPerSecond = (bytesReceivedFinal - bytesReceivedInitial) / 1.0;

            double sentBitsPerSecond = sentBytesPerSecond * 8;
            double receivedBitsPerSecond = receivedBytesPerSecond * 8;

            double sentMbps = sentBitsPerSecond / 1000000;
            double receivedMbps = receivedBitsPerSecond / 1000000;

            double networkCapacityMbps = networkInterface.Speed / 1000000.0;
            networkCapacityMbps -= (sentMbps + receivedMbps);

            Dispatcher.Invoke(() =>
            {
                SeriesCollection seriesCollection = new SeriesCollection
        {
            new PieSeries
            {
                Title = $"Sent {sentMbps:F2} Mbps",
                Values = new ChartValues<double> { sentMbps }
            },
            new PieSeries
            {
                Title = $"Received {receivedMbps:F2} Mbps",
                Values = new ChartValues<double> { receivedMbps }
            },
            new PieSeries
            {
                Title = $"Free network bandwidth: {networkCapacityMbps:F2} Mbps",
                Values = new ChartValues<double> { networkCapacityMbps }
            }
        };
                NetworkPieChart.Series = seriesCollection;
            });
        }
        private async Task SetDiskUsage()
        {
            PerformanceCounter diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
            PerformanceCounter diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");

            diskReadCounter.NextValue();
            diskWriteCounter.NextValue();

            await Task.Delay(1000);

            float diskReadBytesPerSecond = diskReadCounter.NextValue();
            float diskWriteBytesPerSecond = diskWriteCounter.NextValue();

            double diskReadMBPerSecond = diskReadBytesPerSecond / (1024.0 * 1024.0);
            double diskWriteMBPerSecond = diskWriteBytesPerSecond / (1024.0 * 1024.0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                SeriesCollection seriesCollection = new SeriesCollection
        {
            new PieSeries
            {
                Title = $"Read {diskReadMBPerSecond:F2} MB/s",
                Values = new ChartValues<double> { diskReadMBPerSecond }
            },
            new PieSeries
            {
                Title = $"Write {diskWriteMBPerSecond:F2} MB/s",
                Values = new ChartValues<double> { diskWriteMBPerSecond }
            },
        };

                DiscPieChart.Series = seriesCollection;
            });
        }
        private async void SetRamUsage()
        {
            ulong totalMemoryBytes = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalMemoryBytes = (ulong)obj["TotalVisibleMemorySize"] * 1024;
                }
            }
            await Task.Delay(1000);

            PerformanceCounter availableCounter = new PerformanceCounter("Memory", "Available Bytes");
            ulong availableMemoryBytes = (ulong)availableCounter.RawValue;

            double totalMemory = totalMemoryBytes / (1024.0 * 1024.0);
            double availableMemory = availableMemoryBytes / (1024.0 * 1024.0);
            double usedMemory = totalMemory - availableMemory;

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
        private async void ShowProcess_Click(object sender, RoutedEventArgs e)
        {
            MakeCollapsed();
            loadingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            loadingTimer.Tick += LoadingTimer_Tick;
            loadingTimer.Start();
            await Task.Run(() =>
            {
                GetDataProcesses();
            });
            LoadDataLoop();
            loadingTimer.Stop();
        }
        private async void LoadDataLoop()
        {
            while (true)
            {
                GetDataProcesses();
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        private void GetDataProcesses()
        {
            int[] processIds = GetTopProcessIds(100);

            try
            {
                List<ProcessInfo> processesInfoList = new List<ProcessInfo>();

                foreach (var id in processIds)
                {
                    ProcessInfo processInfo = GetProcessInfoById(id);
                    processesInfoList.Add(processInfo);
                }
                Dispatcher.Invoke(() =>
                {                   
                    var sortDescriptions = processesDataGrid.Items.SortDescriptions.ToList();
                    var currentSortColumn = processesDataGrid.Columns.FirstOrDefault(c => c.SortDirection.HasValue);
                    ListSortDirection? sortDirection = currentSortColumn?.SortDirection;
                    
                    processesDataGrid.ItemsSource = processesInfoList;

                    processesDataGrid.Items.SortDescriptions.Clear();
                    foreach (var sortDescription in sortDescriptions)
                    {
                        processesDataGrid.Items.SortDescriptions.Add(sortDescription);
                    }

                    if (currentSortColumn != null && sortDirection.HasValue)
                    {
                        currentSortColumn.SortDirection = sortDirection;
                    }

                    if (sortDescriptions.Any())
                    {
                        processesDataGrid.Items.Refresh();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        static int[] GetTopProcessIds(int count)
        {
            var processes = Process.GetProcesses()
                                   .OrderByDescending(p => p.WorkingSet64)
                                   .Take(count);

            return processes.Select(p => p.Id).ToArray();
        }
        static ProcessInfo GetProcessInfoById(int id)
        {
            Process process = Process.GetProcessById(id);
            var memoryUsageBytes = process.WorkingSet64;
            var memoryUsageMB = memoryUsageBytes / (1024 * 1024);
            var cpuUsage = process.TotalProcessorTime;
            var totalCpuUsage = (cpuUsage.TotalMilliseconds / Environment.ProcessorCount) / (Environment.TickCount - process.StartTime.Ticks) * 100;
            string cpuUsage_ = totalCpuUsage.ToString();
            cpuUsage_ = cpuUsage_.Substring(1);
            double cpu;
            double.TryParse(cpuUsage_, out cpu);
            var name = process.ProcessName;

            return new ProcessInfo(name, memoryUsageMB, cpu);
        }
        private void processesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (processesDataGrid.SelectedCells.Count > 0)
            {
                DataGridCellInfo selectedCell = processesDataGrid.SelectedCells[0];

                if (selectedCell.Item != null && selectedCell.Item != DependencyProperty.UnsetValue && selectedCell.Item != " ")
                {
                    processName = (selectedCell.Column.GetCellContent(selectedCell.Item) as TextBlock)?.Text;
                }
            }
        }
        private void EndProcessButton_CLick(object sender, EventArgs e)
        {
            try
            {
                Process[] ps1 = System.Diagnostics.Process.GetProcessesByName($"{processName}");
                foreach (Process p1 in ps1)
                {
                    Console.WriteLine("Closing process...{0}", p1.ProcessName);
                    p1.Kill();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BackButton_Click(object sender, EventArgs e)
        {
            MakeVisible();
        }
        private void CreatedBy_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Сreated by Kyryl Sobol, a student of the Lviv Polytechnic, group KI-303");
        }
    }
}
