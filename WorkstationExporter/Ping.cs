using System.Net.NetworkInformation;

namespace WorkstationState
{
    /// <summary>
    /// Ping diagnostic class
    /// </summary>
    internal class Pinger : IDiagnostic
    {
        /// <summary>
        /// Ping resources 
        /// Format:
        /// 8.8.8.8
        /// ya.ru
        /// </summary>
        public IEnumerable<string> Resources { get; set; }

        /// <summary>
        /// Create class instance
        /// </summary>
        /// <param name="resources">
        /// Resources format: 8.8.8.8, ya.ru</param>
        public Pinger(IEnumerable<string> resources)
        {
            Resources = resources;
        }

        /// <summary>
        /// Static constructor for create metrics
        /// </summary>
        static Pinger()
        {
            // Create metrics
            Prometheus.AddMetricGaugeLabels("ping_result_ms", "Ping result time with resources", "resource");
            Prometheus.AddMetricCounterLabels("ping_result_status", "Ping result status with resources", null, "resource", "status");
        }

        /// <summary>
        /// Start ping task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for ping task</param>
        public void StartAndForget(CancellationToken cancellationToken = default)
        {
            Task.Factory.StartNew(async () =>
            {
                byte[] buffer = new byte[Settings.Instance.PingPacketSizeBytes];
                Random.Shared.NextBytes(buffer);
                using Ping ping = new Ping();
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (string resource in Resources)
                    {
                        // Send ping
                        PingReply result = await ping.SendPingAsync(resource, Settings.Instance.PingTimeout.Seconds, buffer);

                        // Update metrics
                        Prometheus.UpdateMetricGaugeLabels("ping_result_ms", resource, result.RoundtripTime);
                        Prometheus.IncMetricCounterLabels("ping_result_status", 1d, resource, result.Status.ToString());
                    }
                    await Task.Delay(Settings.Instance.PingInterval);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }

}
