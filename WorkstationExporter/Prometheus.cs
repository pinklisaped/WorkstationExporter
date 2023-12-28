using Prometheus.Client;
using Prometheus.Client.MetricServer;

using System.Collections.Concurrent;

namespace WorkstationState
{
    /// <summary>
    /// Exporter class
    /// </summary>
    public static class Prometheus
    {
        private static ConcurrentDictionary<string, IGauge> MetricsGauge { get; }
        private static ConcurrentDictionary<string, IMetricFamily<IGauge>> MetricsGaugeFamily { get; }
        private static ConcurrentDictionary<string, ICounter> MetricsCounter { get; }
        private static ConcurrentDictionary<string, IMetricFamily<ICounter>> MetricsCounterFamily { get; }
        static Prometheus()
        {
            MetricsGauge = new ConcurrentDictionary<string, IGauge>();
            MetricsGaugeFamily = new ConcurrentDictionary<string, IMetricFamily<IGauge>>();
            MetricsCounter = new ConcurrentDictionary<string, ICounter>();
            MetricsCounterFamily = new ConcurrentDictionary<string, IMetricFamily<ICounter>>();
            MetricServerOptions options = new MetricServerOptions()
            {
                Host = Settings.Instance.Listen.ToString(),
                Port = (int)Settings.Instance.MetricPort,
                MapPath = Settings.Instance.MetricPath
            };
            IMetricServer metricServer = new MetricServer(options);
            metricServer.Start();
        }

        #region Gauge
        public static IMetricFamily<IGauge> AddMetricGaugeLabels(string name, string helpText, IDictionary<string, double> startValues = null, params string[] labelNames)
        {
            IMetricFamily<IGauge> gauge = AddMetricGaugeLabels(name, helpText, labelNames);

            if (startValues != null)
            {
                foreach ((string key, double value) in startValues)
                    gauge.WithLabels(key).Set(value);
            }

            return gauge;
        }
        public static IMetricFamily<IGauge> AddMetricGaugeLabels(string name, string helpText, params string[] labelNames)
        {
            IMetricFamily<IGauge> gauge = Metrics.DefaultFactory.CreateGauge(name, helpText, labelNames);
            MetricsGaugeFamily.TryAdd(name, gauge);
            return gauge;
        }
        public static IGauge AddMetricGaugeSingle(string name, string helpText, double startValue = 0)
        {
            IGauge gauge = Metrics.DefaultFactory.CreateGauge(name, helpText);
            MetricsGauge.TryAdd(name, gauge);
            gauge.Set(startValue);

            return gauge;
        }

        public static void UpdateMetricGaugeSingle(string name, double value)
        {
            if (!MetricsGauge.TryGetValue(name, out IGauge? gauge))
                return;
            gauge.Set(value);
        }

        public static void UpdateMetricGaugeLabels(string name, IDictionary<string, double> values)
        {

            if (!MetricsGaugeFamily.TryGetValue(name, out IMetricFamily<IGauge>? gauge))
                return;

            foreach ((string key, double value) in values)
                gauge.WithLabels(key).Set(value);
        }
        public static void UpdateMetricGaugeLabels(string name, string label, double value)
        {
            if (!MetricsGaugeFamily.TryGetValue(name, out IMetricFamily<IGauge>? gauge))
                return;
            gauge.WithLabels(label).Set(value);
        }
        public static void UpdateMetricGaugeLabels(string name, double value, params string[] labels)
        {
            if (!MetricsGaugeFamily.TryGetValue(name, out IMetricFamily<IGauge>? gauge))
                return;
            gauge.WithLabels(labels).Set(value);
        }
        public static void IncMetricGaugeSingle(string name, double value = 1)
        {
            if (!MetricsGauge.TryGetValue(name, out IGauge? gauge))
                return;
            gauge.Inc(value);
        }

        public static void IncMetricGaugeLabels(string name, IDictionary<string, double> values)
        {
            if (!MetricsGaugeFamily.TryGetValue(name, out IMetricFamily<IGauge>? gauge))
                return;

            foreach ((string key, double value) in values)
                gauge.WithLabels(key).Inc(value);
        }
        public static void IncMetricGaugeLabels(string name, string label, double value = 1)
        {

            if (!MetricsGaugeFamily.TryGetValue(name, out IMetricFamily<IGauge>? gauge))
                return;

            gauge.WithLabels(label).Inc(value);
        }
        public static void DecMetricGaugeSingle(string name, double value = 1)
        {
            if (!MetricsGauge.TryGetValue(name, out IGauge? gauge))
                return;
            gauge.Dec(value);
        }

        public static void DecMetricGaugeLabels(string name, IDictionary<string, double> values)
        {

            if (!MetricsGaugeFamily.TryGetValue(name, out IMetricFamily<IGauge>? gauge))
                return;

            foreach ((string key, double value) in values)
                gauge.WithLabels(key).Dec(value);
        }
        public static void DecMetricGaugeLabels(string name, string label, double value = 1)
        {

            if (!MetricsGaugeFamily.TryGetValue(name, out IMetricFamily<IGauge>? gauge))
                return;

            gauge.WithLabels(label).Dec(value);
        }
        #endregion


        #region Counter
        public static IMetricFamily<ICounter> AddMetricCounterLabels(string name, string helpText, IDictionary<string, double> startValues = null, params string[] labelNames)
        {
            IMetricFamily<ICounter> counter = Metrics.DefaultFactory.CreateCounter(name, helpText, labelNames);
            MetricsCounterFamily.TryAdd(name, counter);
            if (startValues != null)
            {
                foreach ((string key, double value) in startValues)
                    counter.WithLabels(key).Inc(value);
            }

            return counter;
        }
        public static ICounter AddMetricCounterSingle(string name, string helpText, double startValue = 0)
        {
            ICounter counter = Metrics.DefaultFactory.CreateCounter(name, helpText);
            MetricsCounter.TryAdd(name, counter);
            if (startValue is double value)
                counter.Inc(value);

            return counter;
        }

        public static void IncMetricCounterSingle(string name, double value = 1)
        {
            if (!MetricsCounter.TryGetValue(name, out ICounter? counter))
                return;
            counter.Inc(value);
        }

        public static void IncMetricCounterLabels(string name, IDictionary<string, double> values)
        {

            if (!MetricsCounterFamily.TryGetValue(name, out IMetricFamily<ICounter>? counter))
                return;

            foreach ((string key, double value) in values)
                counter.WithLabels(key).Inc(value);
        }
        public static void IncMetricCounterLabels(string name, double value = 1, params string[] labels)
        {

            if (!MetricsCounterFamily.TryGetValue(name, out IMetricFamily<ICounter>? counter))
                return;
            counter.WithLabels(labels).Inc(value);
        }
        #endregion



    }
}
