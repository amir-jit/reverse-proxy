// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.ReverseProxy.Configuration.Contract;
using Microsoft.ReverseProxy.Service;

namespace Microsoft.ReverseProxy.Configuration
{
    /// <summary>
    /// Reacts to configuration changes for type <see cref="ConfigurationData"/>
    /// via <see cref="IOptionsMonitor{TOptions}"/>, and applies configurations
    /// to the Reverse Proxy core.
    /// When configs are loaded from appsettings.json, this takes care of hot updates
    /// when appsettings.json is modified on disk.
    /// </summary>
    internal class ConfigurationConfigProvider : IProxyConfigProvider, IDisposable
    {
        private readonly object _lockObject = new object();
        private readonly ILogger<ConfigurationConfigProvider> _logger;
        private readonly IOptionsMonitor<ConfigurationData> _optionsMonitor;
        private readonly ICertificateConfigLoader _certificateConfigLoader;
        private readonly LinkedList<WeakReference<X509Certificate>> _certificates = new LinkedList<WeakReference<X509Certificate>>();
        private ConfigurationSnapshot _snapshot;
        private CancellationTokenSource _changeToken;
        private bool _disposed;
        private IDisposable _subscription;

        public ConfigurationConfigProvider(
            ILogger<ConfigurationConfigProvider> logger,
            IOptionsMonitor<ConfigurationData> dataMonitor,
            ICertificateConfigLoader certificateConfigLoader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _optionsMonitor = dataMonitor ?? throw new ArgumentNullException(nameof(dataMonitor));
            _certificateConfigLoader = certificateConfigLoader ?? throw new ArgumentNullException(nameof(certificateConfigLoader));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach(var certificateRef in _certificates)
                {
                    if (certificateRef.TryGetTarget(out var certificate))
                    {
                        certificate.Dispose();
                    }
                }
                _subscription?.Dispose();
                _changeToken?.Dispose();
                _disposed = true;
            }
        }

        public IProxyConfig GetConfig()
        {
            // First time load
            if (_snapshot == null)
            {
                _subscription = _optionsMonitor.OnChange(UpdateSnapshot);
                UpdateSnapshot(_optionsMonitor.CurrentValue);
            }
            return _snapshot;
        }

        private void UpdateSnapshot(ConfigurationData data)
        {
            // Prevent overlapping updates, especially on startup.
            lock (_lockObject)
            {
                Log.LoadData(_logger);
                var oldToken = _changeToken;
                _changeToken = new CancellationTokenSource();
                try
                {
                    var newSnapshot = new ConfigurationSnapshot()
                    {
                        Routes = data.Routes.Select(r => Convert(r)).ToList().AsReadOnly(),
                        Clusters = data.Clusters.Select(c => Convert(c.Key, c.Value)).ToList().AsReadOnly(),
                        ChangeToken = new CancellationChangeToken(_changeToken.Token)
                    };
                    PurgeCertificateList();
                    _snapshot = newSnapshot;
                }
                catch (Exception ex)
                {
                    Log.ConfigurationDataConversionFailed(_logger, ex);

                    // Re-throw on the first time load to prevent app from starting.
                    if (_snapshot == null)
                    {
                        throw;
                    }
                }

                try
                {
                    oldToken?.Cancel(throwOnFirstException: false);
                }
                catch (Exception ex)
                {
                    Log.ErrorSignalingChange(_logger, ex);
                }
            }
        }

        private void PurgeCertificateList()
        {
            var next = _certificates.First;
            while (next != null)
            {
                var current = next;
                next = next.Next;
                if (!current.Value.TryGetTarget(out var _))
                {
                    _certificates.Remove(current);
                }
            }
        }

        private Abstractions.Cluster Convert(string clusterId, ClusterData data)
        {
            var cluster = new Abstractions.Cluster
            {
                // The Object style config binding puts the id as the key in the dictionary, but later we want it on the
                // cluster object as well.
                Id = clusterId,
                CircuitBreakerOptions = Convert(data.CircuitBreakerData),
                QuotaOptions = Convert(data.QuotaData),
                PartitioningOptions = Convert(data.PartitioningData),
                LoadBalancing = Convert(data.LoadBalancing),
                SessionAffinity = Convert(data.SessionAffinity),
                HealthCheckOptions = Convert(data.HealthCheckData),
                HttpClientOptions = Convert(data.HttpClientData),
                Metadata = data.Metadata?.DeepClone(StringComparer.OrdinalIgnoreCase)
            };
            foreach(var destination in data.Destinations)
            {
                cluster.Destinations.Add(destination.Key, Convert(destination.Value));
            }
            return cluster;
        }

        private Abstractions.ProxyRoute Convert(ProxyRouteData data)
        {
            var route = new Abstractions.ProxyRoute
            {
                RouteId = data.RouteId,
                Order = data.Order,
                ClusterId = data.ClusterId,
                AuthorizationPolicy = data.AuthorizationPolicy,
                CorsPolicy = data.CorsPolicy,
                Metadata = data.Metadata?.DeepClone(StringComparer.OrdinalIgnoreCase),
                Transforms = data.Transforms?.Select(d => new Dictionary<string, string>(d, StringComparer.OrdinalIgnoreCase)).ToList<IDictionary<string, string>>(),
            };
            Convert(route.Match, data.Match);
            return route;
        }

        private void Convert(Abstractions.ProxyMatch proxyMatch, ProxyMatchData data)
        {
            if (data == null)
            {
                return;
            }

            proxyMatch.Methods = data.Methods?.ToArray();
            proxyMatch.Hosts = data.Hosts?.ToArray();
            proxyMatch.Path = data.Path;
        }

        private Abstractions.CircuitBreakerOptions Convert(CircuitBreakerData data)
        {
            if(data == null)
            {
                return null;
            }

            return new Abstractions.CircuitBreakerOptions
            {
                MaxConcurrentRequests = data.MaxConcurrentRequests,
                MaxConcurrentRetries = data.MaxConcurrentRetries,
            };
        }

        private Abstractions.QuotaOptions Convert(QuotaData data)
        {
            if (data == null)
            {
                return null;
            }

            return new Abstractions.QuotaOptions
            {
                Average = data.Average,
                Burst = data.Burst,
            };
        }

        private Abstractions.ClusterPartitioningOptions Convert(ClusterPartitioningData data)
        {
            if (data == null)
            {
                return null;
            }

            return new Abstractions.ClusterPartitioningOptions
            {
                PartitionCount = data.PartitionCount,
                PartitionKeyExtractor = data.PartitionKeyExtractor,
                PartitioningAlgorithm = data.PartitioningAlgorithm,
            };
        }

        private Abstractions.LoadBalancingOptions Convert(LoadBalancingData data)
        {
            if (data == null)
            {
                return null;
            }

            return new Abstractions.LoadBalancingOptions
            {
                Mode = Enum.Parse<Abstractions.LoadBalancingMode>(data.Mode),
            };
        }

        private Abstractions.SessionAffinityOptions Convert(SessionAffinityData data)
        {
            if (data == null)
            {
                return null;
            }

            return new Abstractions.SessionAffinityOptions
            {
                Enabled = data.Enabled,
                Mode = data.Mode,
                FailurePolicy = data.FailurePolicy,
                Settings = data.Settings?.DeepClone(StringComparer.OrdinalIgnoreCase)
            };
        }

        private Abstractions.HealthCheckOptions Convert(HealthCheckData data)
        {
            if (data == null)
            {
                return null;
            }

            return new Abstractions.HealthCheckOptions
            {
                Enabled = data.Enabled,
                Interval = data.Interval,
                Timeout = data.Timeout,
                Port = data.Port,
                Path = data.Path,
            };
        }

        private Abstractions.ProxyHttpClientOptions Convert(ProxyHttpClientData data)
        {
            if (data == null)
            {
                return null;
            }

            var clientCertificate = data.ClientCertificate != null ? _certificateConfigLoader.LoadCertificate(data.ClientCertificate) : null;

            if (clientCertificate != null)
            {
                _certificates.AddLast(new WeakReference<X509Certificate>(clientCertificate));
            }

            SslProtocols? sslProtocols = null;
            if (data.SslProtocols != null && data.SslProtocols.Count > 0)
            {
                foreach (var protocolConfig in data.SslProtocols)
                {
                    sslProtocols = sslProtocols == null ? protocolConfig : sslProtocols | protocolConfig;
                }
            }

            return new Abstractions.ProxyHttpClientOptions
            {
                SslProtocols = sslProtocols,
                DangerousAcceptAnyServerCertificate = data.DangerousAcceptAnyServerCertificate,
                ClientCertificate = clientCertificate,
                MaxConnectionsPerServer = data.MaxConnectionsPerServer
            };
        }

        private Abstractions.Destination Convert(DestinationData data)
        {
            if (data == null)
            {
                return null;
            }

            return new Abstractions.Destination
            {
                Address = data.Address,
                Metadata = data.Metadata?.DeepClone(StringComparer.OrdinalIgnoreCase),
            };
        }

        private static class Log
        {
            private static readonly Action<ILogger, Exception> _errorSignalingChange = LoggerMessage.Define(
                LogLevel.Error,
                EventIds.ErrorSignalingChange,
                "An exception was thrown from the change notification.");

            private static readonly Action<ILogger, Exception> _loadData = LoggerMessage.Define(
                LogLevel.Information,
                EventIds.LoadData,
                "Loading proxy data from config.");

            private static readonly Action<ILogger, Exception> _configurationDataConversionFailed = LoggerMessage.Define(
                LogLevel.Error,
                EventIds.ConfigurationDataConversionFailed,
                "Configuration data conversion failed.");

            public static void ErrorSignalingChange(ILogger logger, Exception exception)
            {
                _errorSignalingChange(logger, exception);
            }

            public static void LoadData(ILogger logger)
            {
                _loadData(logger, null);
            }

            public static void ConfigurationDataConversionFailed(ILogger logger, Exception exception)
            {
                _configurationDataConversionFailed(logger, exception);
            }
        }
    }
}
