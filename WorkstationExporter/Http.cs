using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace WorkstationState
{
    /// <summary>
    /// Http diagnostic class
    /// </summary>
    internal class Http : IDiagnostic
    { 
        /// <summary>
        /// Http-check resources 
        /// Format:
        /// http://ya.ru
        /// https://google.com
        /// </summary>
        public IEnumerable<string> Resources { get; set; }

        /// <summary>
        /// Create class instance
        /// </summary>
        /// <param name="resources">
        /// Resources format: http://ya.ru, https://google.com</param>
        public Http(IEnumerable<string> resources)
        {
            foreach (string resource in resources)
            {
                if (!resource.Contains("http"))
                    throw new ArgumentException($"Resource \"{resource}\" is not valid format for http config", nameof(resources));
            }
            Resources = resources;
        }

        /// <summary>
        /// Static constructor for create metrics
        /// </summary>
        static Http()
        {
            // Create metrics
            Prometheus.AddMetricGaugeLabels("http_result_ms", "Http result time with resources", "resource");
            Prometheus.AddMetricCounterLabels("http_result_status", "Http result status with resources", null, "resource", "status");
        }

        /// <summary>
        /// Start http-check task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for task</param>
        public void StartAndForget(CancellationToken cancellationToken = default)
        {
            Task.Factory.StartNew(async () =>
            {
                // Create http client
                using SocketsHttpHandler handler = new SocketsHttpHandler()
                {
                    ConnectTimeout = Settings.Instance.HTTPTimeout,
                    UseCookies = false,
                    UseProxy = false,
                    Proxy = null,
                    
                };
                handler.SslOptions.CertificateRevocationCheckMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;
                using HttpClient httpClient = new HttpClient(handler);
                byte[] buffer = new byte[Settings.Instance.PingPacketSizeBytes];
                Random.Shared.NextBytes(buffer);

                Stopwatch sw = new Stopwatch();
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (string resource in Resources)
                    {
                        sw.Restart();
                        Uri uri = new Uri(resource);
                        string host = uri.Host;
                        try
                        {
                            // Check GET htt request
                            using (HttpResponseMessage result = await httpClient.GetAsync(uri, cancellationToken))
                            {
                                sw.Stop();
                                Prometheus.UpdateMetricGaugeLabels("http_result_ms", host, sw.ElapsedMilliseconds);
                                Prometheus.IncMetricCounterLabels("http_result_status", 1d, host, ((int)result.StatusCode).ToString());
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            // Handle http error
                            Prometheus.UpdateMetricGaugeLabels("http_result_ms", host, 0);
                            Prometheus.IncMetricCounterLabels("http_result_status", 1d, host, ((int)(ex.StatusCode ?? HttpStatusCode.RequestTimeout)).ToString());
                        }
                        catch { } // Skip other errors
                    }
                    await Task.Delay(Settings.Instance.HTTPTimeout);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }

}
