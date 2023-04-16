using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WorkstationState
{
    internal class Settings
    {
        static Settings _Instance;
        public static Settings Instance { get { return _Instance; } }
        public TimeSpan PingInterval { get; private set; }
        public TimeSpan PingTimeout { get; private set; }
        public uint PingPacketSizeBytes { get; private set; }
        public TimeSpan HTTPInterval { get; private set; }
        public TimeSpan HTTPTimeout { get; private set; }
        public bool WorkstationIPv6MetricsEnabled { get; private set; }
        public IPAddress Listen { get; private set; }
        public string MetricPath { get; private set; }
        public uint MetricPort { get; private set; }
        static Settings() 
        {
            _Instance = new Settings()
            {
                PingInterval = TimeSpan.FromSeconds(20),
                PingTimeout = TimeSpan.FromSeconds(5),
                PingPacketSizeBytes = 1024,
                HTTPInterval = TimeSpan.FromSeconds(20),
                HTTPTimeout = TimeSpan.FromSeconds(10),
                WorkstationIPv6MetricsEnabled = false,
                Listen = IPAddress.Any,
                MetricPath = "/healthmetrics",
                MetricPort = 9111
            };
        }
    }
}
