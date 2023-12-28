using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace WorkstationState
{
    /// <summary>
    /// Workstation monitoring class
    /// </summary>
    internal class WorkstationConfig : IDiagnostic
    {
        /// <summary>
        /// Last DNS list
        /// </summary>
        private List<IPAddress> _OldDNS = new List<IPAddress>();

        /// <summary>
        /// Program-check resources 
        /// Format:
        /// csgo
        /// doka2
        /// </summary>
        public IEnumerable<string> Resources { get; set; }

        /// <summary>
        /// Create class instance
        /// </summary>
        /// <param name="resources">
        /// Resources format: csgo, doka2</param>
        public WorkstationConfig(IEnumerable<string> resources)
        {
            Resources = resources;
        }

        /// <summary>
        /// Static constructor for create metrics
        /// </summary>
        static WorkstationConfig()
        {
            Prometheus.AddMetricGaugeLabels("workstation_dns_address", "Computer DNS actual config", "dns");
            Prometheus.AddMetricGaugeSingle("workstation_uptime", "Computer uptime");
            Prometheus.AddMetricGaugeSingle("workstation_process_count", "Computer process count");
            Prometheus.AddMetricGaugeLabels("workstation_process_execute", "Computer process execute", "process_name");
            Prometheus.AddMetricGaugeSingle("workstation_cpu_count", "CPU count on workstation");
            Prometheus.AddMetricGaugeSingle("workstation_ram_bytes", "RAM bytes on workstation");
            Prometheus.AddMetricGaugeLabels("workstation_disk_bytes", "Storage bytes on workstation", "device");
            Prometheus.AddMetricGaugeLabels("workstation_disk_avalable_bytes", "Storage avalable bytes on workstation", "device");
        }

        /// <summary>
        /// Reload dns list
        /// </summary>
        public void ReloadDNS()
        {
            List<IPAddress> dnsList = new List<IPAddress>(GetDnsAddress());
            foreach (IPAddress ipDNS in dnsList)
                Prometheus.UpdateMetricGaugeLabels("workstation_dns_address", ipDNS.ToString(), 1);

            foreach (IPAddress ipDNS in _OldDNS)
            {
                if (!dnsList.Contains(ipDNS))
                    Prometheus.UpdateMetricGaugeLabels("workstation_dns_address", ipDNS.ToString(), 0);
            }
            _OldDNS = dnsList;
        }

        /// <summary>
        /// Load static metrics as disk, cpu count, etc...
        /// </summary>
        public void LoadStaticMetrics()
        {
            Prometheus.UpdateMetricGaugeSingle("workstation_cpu_count", Environment.ProcessorCount);
            foreach (DriveInfo disk in DriveInfo.GetDrives())
            {
                if (disk.DriveType != DriveType.Fixed) continue;
                Prometheus.UpdateMetricGaugeLabels("workstation_disk_bytes", disk.Name, disk.TotalSize);
            }
        }

        /// <summary>
        /// Reload dynamic metrics as uptime, disk usage, etc...
        /// </summary>
        public void ReloadDynamicMetrics()
        {
            Prometheus.UpdateMetricGaugeSingle("workstation_uptime", Environment.TickCount64 / 1000);
            foreach (DriveInfo disk in DriveInfo.GetDrives())
            {
                if (disk.DriveType != DriveType.Fixed) continue;
                Prometheus.UpdateMetricGaugeLabels("workstation_disk_avalable_bytes", disk.Name, disk.AvailableFreeSpace);
            }
        }

        /// <summary>
        /// Processes exists
        /// </summary>
        private readonly Dictionary<string, int> _Processes = new Dictionary<string, int>(100);

        /// <summary>
        /// Reload processes list
        /// </summary>
        public void ReloadProcessesMetrics()
        {
            foreach ((string key, int value) in _Processes)
                _Processes[key] = 0;

            // Discovery processes exist
            Process[] processes = Process.GetProcesses();
            Prometheus.UpdateMetricGaugeSingle("workstation_process_count", processes.Length);
            foreach (Process process in processes)
            {
                foreach (string resource in Resources)
                {
                    if (process.ProcessName.Contains(resource))
                    {
                        if (_Processes.ContainsKey(resource))
                            _Processes[resource]++;
                        else
                            _Processes[resource] = 1;

                        Prometheus.UpdateMetricGaugeLabels("workstation_process_execute", resource, 1d);
                    }
                }
            }

            // Set zero for exited processes
            if (_Processes.Count > 0)
            {
                int max = _Processes.Values.Max();
                foreach ((string key, int value) in _Processes)
                {
                    if (_Processes[key] == 0)
                    {
                        Prometheus.UpdateMetricGaugeLabels("workstation_process_execute", key, 0);
                        _Processes.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Read dns address-list
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<IPAddress> GetDnsAddress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;

                    foreach (IPAddress dnsAddress in dnsAddresses)
                    {
                        if (dnsAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && !Settings.Instance.WorkstationIPv6MetricsEnabled)
                            continue;

                        yield return dnsAddress;
                    }
                }
            }
        }

        /// <summary>
        /// Start processes-check, and reload params task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for task</param>
        public void StartAndForget(CancellationToken cancellationToken = default)
        {
            LoadStaticMetrics();

            Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ReloadProcessesMetrics();
                    ReloadDynamicMetrics();
                    ReloadDNS();
                    await Task.Delay(5000);
                }
            });
        }
    }
}
