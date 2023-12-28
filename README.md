# Prometheus workstation exporter


## Features
- [x] HTTP check
- [x] Ping check
- [x] Execute software check
- [x] Disk avalable check
- [x] DNS servers check
- [x] Uptime check
- [ ] CPU check
- [ ] RAM check
- [ ] Network check

## Default config
```
PingInterval = TimeSpan.FromSeconds(20),
PingTimeout = TimeSpan.FromSeconds(5),
PingPacketSizeBytes = 1024,
HTTPInterval = TimeSpan.FromSeconds(20),
HTTPTimeout = TimeSpan.FromSeconds(10),
WorkstationIPv6MetricsEnabled = false,
Listen = IPAddress.Any,
MetricPath = "/healthmetrics",
MetricPort = 9111
```
Avalable on http://localhost:9111/healthmetrics

## Publish
> Please, preconfigure project before build.
>>Settings.cs
```
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained
```
