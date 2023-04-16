namespace WorkstationState
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (StreamReader resource = new StreamReader(Utils.GetResource("Pings")))
            {
                Pinger pinger = new Pinger(resource.ReadToEnd().Replace("\r", string.Empty).Split('\n'));
                pinger.StartAndForget(stoppingToken);
            }

            using (StreamReader resource = new StreamReader(Utils.GetResource("HTTP")))
            {
                Http http = new Http(resource.ReadToEnd().Replace("\r", string.Empty).Split('\n'));
                http.StartAndForget(stoppingToken);
            }

            using (StreamReader resource = new StreamReader(Utils.GetResource("Processes")))
            {
                WorkstationConfig workstation = new WorkstationConfig(resource.ReadToEnd().Replace("\r", string.Empty).Split('\n'));
                workstation.StartAndForget();
            }
        }
    }
}