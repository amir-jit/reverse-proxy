// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using Xunit;
using Yarp.ReverseProxy.Abstractions.ClusterDiscovery.Contract;
using Yarp.ReverseProxy.Service.LoadBalancing;
using Yarp.ReverseProxy.Service.Proxy;

namespace Yarp.ReverseProxy.Abstractions.Tests
{
    public class ClusterTests
    {
        [Fact]
        public void Equals_Same_Value_Returns_True()
        {
            var options1 = new ClusterConfig
            {
                ClusterId = "cluster1",
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "destinationA",
                        new DestinationConfig
                        {
                            Address = "https://localhost:10000/destA",
                            Health = "https://localhost:20000/destA",
                            Metadata = new Dictionary<string, string> { { "destA-K1", "destA-V1" }, { "destA-K2", "destA-V2" } }
                        }
                    },
                    {
                        "destinationB",
                        new DestinationConfig
                        {
                            Address = "https://localhost:10000/destB",
                            Health = "https://localhost:20000/destB",
                            Metadata = new Dictionary<string, string> { { "destB-K1", "destB-V1" }, { "destB-K2", "destB-V2" } }
                        }
                    }
                },
                HealthCheck = new HealthCheckConfig
                {
                    Passive = new PassiveHealthCheckConfig
                    {
                        Enabled = true,
                        Policy = "FailureRate",
                        ReactivationPeriod = TimeSpan.FromMinutes(5)
                    },
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(4),
                        Timeout = TimeSpan.FromSeconds(6),
                        Policy = "Any5xxResponse",
                        Path = "healthCheckPath"
                    }
                },
                LoadBalancingPolicy = LoadBalancingPolicies.Random,
                SessionAffinity = new SessionAffinityConfig
                {
                    Enabled = true,
                    FailurePolicy = "Return503Error",
                    Mode = "Cookie",
                    Settings = new Dictionary<string, string> { { "affinity1-K1", "affinity1-V1" }, { "affinity1-K2", "affinity1-V2" } }
                },
                HttpClient = new HttpClientConfig
                {
                    SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12,
                    MaxConnectionsPerServer = 10,
                    DangerousAcceptAnyServerCertificate = true,
                    ActivityContextHeaders = ActivityContextHeaders.CorrelationContext,
#if NET
                    RequestHeaderEncoding = Encoding.UTF8
#endif
                },
                HttpRequest = new RequestProxyConfig
                {
                    Timeout = TimeSpan.FromSeconds(60),
                    Version = Version.Parse("1.0"),
#if NET
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
#endif
                },
                Metadata = new Dictionary<string, string> { { "cluster1-K1", "cluster1-V1" }, { "cluster1-K2", "cluster1-V2" } }
            };

            var options2 = new ClusterConfig
            {
                ClusterId = "cluster1",
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "destinationA",
                        new DestinationConfig
                        {
                            Address = "https://localhost:10000/destA",
                            Health = "https://localhost:20000/destA",
                            Metadata = new Dictionary<string, string> { { "destA-K1", "destA-V1" }, { "destA-K2", "destA-V2" } }
                        }
                    },
                    {
                        "destinationB",
                        new DestinationConfig
                        {
                            Address = "https://localhost:10000/destB",
                            Health = "https://localhost:20000/destB",
                            Metadata = new Dictionary<string, string> { { "destB-K1", "destB-V1" }, { "destB-K2", "destB-V2" } }
                        }
                    }
                },
                HealthCheck = new HealthCheckConfig
                {
                    Passive = new PassiveHealthCheckConfig
                    {
                        Enabled = true,
                        Policy = "FailureRate",
                        ReactivationPeriod = TimeSpan.FromMinutes(5)
                    },
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(4),
                        Timeout = TimeSpan.FromSeconds(6),
                        Policy = "Any5xxResponse",
                        Path = "healthCheckPath"
                    }
                },
                LoadBalancingPolicy = LoadBalancingPolicies.Random,
                SessionAffinity = new SessionAffinityConfig
                {
                    Enabled = true,
                    FailurePolicy = "Return503Error",
                    Mode = "Cookie",
                    Settings = new Dictionary<string, string> { { "affinity1-K1", "affinity1-V1" }, { "affinity1-K2", "affinity1-V2" } }
                },
                HttpClient = new HttpClientConfig
                {
                    SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12,
                    MaxConnectionsPerServer = 10,
                    DangerousAcceptAnyServerCertificate = true,
                    ActivityContextHeaders = ActivityContextHeaders.CorrelationContext,
#if NET
                    RequestHeaderEncoding = Encoding.UTF8
#endif
                },
                HttpRequest = new RequestProxyConfig
                {
                    Timeout = TimeSpan.FromSeconds(60),
                    Version = Version.Parse("1.0"),
#if NET
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
#endif
                },
                Metadata = new Dictionary<string, string> { { "cluster1-K1", "cluster1-V1" }, { "cluster1-K2", "cluster1-V2" } }
            };

            var equals = options1.Equals(options2);

            Assert.True(equals);

            Assert.True(options1.Equals(options1 with { })); // Clone
        }

        [Fact]
        public void Equals_Different_Value_Returns_False()
        {
            var options1 = new ClusterConfig
            {
                ClusterId = "cluster1",
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "destinationA",
                        new DestinationConfig
                        {
                            Address = "https://localhost:10000/destA",
                            Health = "https://localhost:20000/destA",
                            Metadata = new Dictionary<string, string> { { "destA-K1", "destA-V1" }, { "destA-K2", "destA-V2" } }
                        }
                    },
                    {
                        "destinationB",
                        new DestinationConfig
                        {
                            Address = "https://localhost:10000/destB",
                            Health = "https://localhost:20000/destB",
                            Metadata = new Dictionary<string, string> { { "destB-K1", "destB-V1" }, { "destB-K2", "destB-V2" } }
                        }
                    }
                },
                HealthCheck = new HealthCheckConfig
                {
                    Passive = new PassiveHealthCheckConfig
                    {
                        Enabled = true,
                        Policy = "FailureRate",
                        ReactivationPeriod = TimeSpan.FromMinutes(5)
                    },
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(4),
                        Timeout = TimeSpan.FromSeconds(6),
                        Policy = "Any5xxResponse",
                        Path = "healthCheckPath"
                    }
                },
                LoadBalancingPolicy = LoadBalancingPolicies.Random,
                SessionAffinity = new SessionAffinityConfig
                {
                    Enabled = true,
                    FailurePolicy = "Return503Error",
                    Mode = "Cookie",
                    Settings = new Dictionary<string, string> { { "affinity1-K1", "affinity1-V1" }, { "affinity1-K2", "affinity1-V2" } }
                },
                HttpClient = new HttpClientConfig
                {
                    SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12,
                    MaxConnectionsPerServer = 10,
                    DangerousAcceptAnyServerCertificate = true,
                    ActivityContextHeaders = ActivityContextHeaders.CorrelationContext,
                },
                HttpRequest = new RequestProxyConfig
                {
                    Timeout = TimeSpan.FromSeconds(60),
                    Version = Version.Parse("1.0"),
#if NET
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
#endif
                },
                Metadata = new Dictionary<string, string> { { "cluster1-K1", "cluster1-V1" }, { "cluster1-K2", "cluster1-V2" } }
            };

            Assert.False(options1.Equals(options1 with { ClusterId = "different" }));
            Assert.False(options1.Equals(options1 with { Destinations = new Dictionary<string, DestinationConfig>() }));
            Assert.False(options1.Equals(options1 with { HealthCheck = new HealthCheckConfig() }));
            Assert.False(options1.Equals(options1 with { LoadBalancingPolicy = "different" }));
            Assert.False(options1.Equals(options1 with
            {
                SessionAffinity = new SessionAffinityConfig
                {
                    Enabled = true,
                    FailurePolicy = "Return503Error",
                    Mode = "Cookie",
                    Settings = new Dictionary<string, string> { { "affinity1-K1", "affinity1-V1" } }
                }
            }));
            Assert.False(options1.Equals(options1 with
            {
                HttpClient = new HttpClientConfig
                {
                    SslProtocols = SslProtocols.Tls12,
                    MaxConnectionsPerServer = 10,
                    DangerousAcceptAnyServerCertificate = true,
                    ActivityContextHeaders = ActivityContextHeaders.CorrelationContext,
                }
            }));
            Assert.False(options1.Equals(options1 with { HttpRequest = new RequestProxyConfig() { } }));
            Assert.False(options1.Equals(options1 with { Metadata = null }));
        }

        [Fact]
        public void Equals_Second_Null_Returns_False()
        {
            var options1 = new ClusterConfig();

            var equals = options1.Equals(null);

            Assert.False(equals);
        }
    }
}
