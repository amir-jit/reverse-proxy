// <copyright file="LabelsParserTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.ReverseProxy.Abstractions;
using Xunit;

namespace Microsoft.ReverseProxy.ServiceFabricIntegration.Tests
{
    public class LabelsParserTests
    {
        private static readonly Uri TestServiceName = new Uri("fabric:/App1/Svc1");

        [Fact]
        public void BuildCluster_CompleteLabels_Works()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Enable", "true" },
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Backend.CircuitBreaker.MaxConcurrentRequests", "42" },
                { "IslandGateway.Backend.CircuitBreaker.MaxConcurrentRetries", "5" },
                { "IslandGateway.Backend.Quota.Average", "1.2" },
                { "IslandGateway.Backend.Quota.Burst", "2.3" },
                { "IslandGateway.Backend.Partitioning.Count", "5" },
                { "IslandGateway.Backend.Partitioning.KeyExtractor", "Header('x-ms-organization-id')" },
                { "IslandGateway.Backend.Partitioning.Algorithm", "SHA256" },
                { "IslandGateway.Backend.Healthcheck.Enabled", "true" },
                { "IslandGateway.Backend.Healthcheck.Interval", "PT5S" },
                { "IslandGateway.Backend.Healthcheck.Timeout", "PT5S" },
                { "IslandGateway.Backend.Healthcheck.Port", "8787" },
                { "IslandGateway.Backend.Healthcheck.Path", "/api/health" },
                { "IslandGateway.Backend.Metadata.Foo", "Bar" },
            };

            // Act
            var cluster = LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            Cluster expectedCluster = new Cluster
            {
                Id = "MyCoolClusterId",
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    MaxConcurrentRequests = 42,
                    MaxConcurrentRetries = 5,
                },
                QuotaOptions = new QuotaOptions
                {
                    Average = 1.2,
                    Burst = 2.3,
                },
                PartitioningOptions = new ClusterPartitioningOptions
                {
                    PartitionCount = 5,
                    PartitionKeyExtractor = "Header('x-ms-organization-id')",
                    PartitioningAlgorithm = "SHA256",
                },
                LoadBalancingOptions = new LoadBalancingOptions(),
                HealthCheckOptions = new HealthCheckOptions
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(5),
                    Timeout = TimeSpan.FromSeconds(5),
                    Port = 8787,
                    Path = "/api/health",
                },
                TlsOptions = new ClusterTlsOptions
                {
                    ValidationMode = ClusterTlsCertificateValidationMode.IntraCommAllowList,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "Foo", "Bar" },
                },
            };
            cluster.Should().BeEquivalentTo(expectedCluster);
        }

        [Fact]
        public void BuildCluster_IncompleteLabels_UsesDefaultValues()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
            };

            // Act
            var cluster = LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            Cluster expectedCluster = new Cluster
            {
                Id = "MyCoolClusterId",
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    MaxConcurrentRequests = LabelsParser.DefaultCircuitbreakerMaxConcurrentRequests,
                    MaxConcurrentRetries = LabelsParser.DefaultCircuitbreakerMaxConcurrentRetries,
                },
                QuotaOptions = new QuotaOptions
                {
                    Average = LabelsParser.DefaultQuotaAverage,
                    Burst = LabelsParser.DefaultQuotaBurst,
                },
                PartitioningOptions = new ClusterPartitioningOptions
                {
                    PartitionCount = LabelsParser.DefaultPartitionCount,
                    PartitionKeyExtractor = LabelsParser.DefaultPartitionKeyExtractor,
                    PartitioningAlgorithm = LabelsParser.DefaultPartitioningAlgorithm,
                },
                LoadBalancingOptions = new LoadBalancingOptions(),
                HealthCheckOptions = new HealthCheckOptions
                {
                    Enabled = false,
                    Interval = TimeSpan.Zero,
                    Timeout = TimeSpan.Zero,
                    Port = null,
                    Path = null,
                },
                TlsOptions = new ClusterTlsOptions
                {
                    ValidationMode = ClusterTlsCertificateValidationMode.IntraCommAllowList,
                },
                Metadata = new Dictionary<string, string>(),
            };
            cluster.Should().BeEquivalentTo(expectedCluster);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("TRUE", true)]
        [InlineData("false", false)]
        [InlineData("False", false)]
        [InlineData("FALSE", false)]
        public void BuildCluster_HealthCheckOptions_Enabled_Valid(string label, bool expected)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Backend.Healthcheck.Enabled", label },
            };

            // Act
            var cluster = LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            cluster.HealthCheckOptions.Enabled.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void BuildCluster_HealthCheckOptions_Enabled_Invalid(string label)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Backend.Healthcheck.Enabled", label },
            };

            // Act
            Action action = () => LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            action.Should().Throw<IslandGatewayConfigException>();
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("443", 443)]
        [InlineData("0x1bc", 444)]
        [InlineData("  80 ", 80)]
        [InlineData("65000", 65000)]
        public void BuildCluster_HealthCheckOptions_Port_Valid(string label, int? expected)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Backend.Healthcheck.Port", label },
            };

            // Act
            var cluster = LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            cluster.HealthCheckOptions.Port.Should().Be(expected);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123a")]
        public void BuildCluster_HealthCheckOptions_Port_Invalid(string label)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Backend.Healthcheck.Port", label },
            };

            // Act
            Action action = () => LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            action.Should().Throw<IslandGatewayConfigException>();
        }

        [Fact]
        public void BuildCluster_MissingBackendId_UsesServiceName()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.Quota.Burst", "2.3" },
                { "IslandGateway.Backend.Partitioning.Count", "5" },
                { "IslandGateway.Backend.Partitioning.KeyExtractor", "Header('x-ms-organization-id')" },
                { "IslandGateway.Backend.Partitioning.Algorithm", "SHA256" },
                { "IslandGateway.Backend.Healthcheck.Interval", "PT5S" },
            };

            // Act
            var cluster = LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            cluster.Id.Should().Be(TestServiceName.ToString());
        }

        [Theory]
        [InlineData("IslandGateway.Backend.CircuitBreaker.MaxConcurrentRequests", "this is no number boi")]
        [InlineData("IslandGateway.Backend.CircuitBreaker.MaxConcurrentRetries", "P=NP")]
        [InlineData("IslandGateway.Backend.Quota.Average", "average should be a double")]
        [InlineData("IslandGateway.Backend.Quota.Burst", "")]
        [InlineData("IslandGateway.Backend.Partitioning.Count", "$#%+@`~áéÉ")]
        [InlineData("IslandGateway.Backend.Healthcheck.Interval", "1S")]
        [InlineData("IslandGateway.Backend.Healthcheck.Timeout", "foobar")]
        [InlineData("IslandGateway.Backend.Healthcheck.Port", "should be an int")]
        public void BuildCluster_InvalidValues_Throws(string key, string invalidValue)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { key, invalidValue },
            };

            // Act
            Func<Cluster> func = () => LabelsParser.BuildCluster(TestServiceName, labels);

            // Assert
            func.Should().Throw<IslandGatewayConfigException>().WithMessage($"Could not convert label {key}='{invalidValue}' *");
        }

        [Fact]
        public void BuildRoutes_SingleRoute_Works()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Rule", "Host('example.com')" },
                { "IslandGateway.Routes.MyRoute.Transformations", "StripPrefix('/a')" },
                { "IslandGateway.Routes.MyRoute.Priority", "2" },
                { "IslandGateway.Routes.MyRoute.Metadata.Foo", "Bar" },
            };

            // Act
            var routes = LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            List<GatewayRoute> expectedRoutes = new List<GatewayRoute>
            {
                new GatewayRoute
                {
                    RouteId = "MyCoolClusterId:MyRoute",
                    Rule = "Host('example.com')",
                    Transformations = "StripPrefix('/a')",
                    Priority = 2,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string>
                    {
                        { "Foo", "Bar" },
                    },
                },
            };
            routes.Should().BeEquivalentTo(expectedRoutes);
        }

        [Fact]
        public void BuildRoutes_IncompleteRoute_UsesDefaults()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Rule", "Host('example.com')" },
            };

            // Act
            var routes = LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            List<GatewayRoute> expectedRoutes = new List<GatewayRoute>
            {
                new GatewayRoute
                {
                    RouteId = "MyCoolClusterId:MyRoute",
                    Rule = "Host('example.com')",
                    Transformations = null,
                    Priority = LabelsParser.DefaultRoutePriority,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string>(),
                },
            };
            routes.Should().BeEquivalentTo(expectedRoutes);
        }

        /// <summary>
        /// The LabelParser is not expected to invoke route parsing logic, and should treat the objects as plain data containers.
        /// </summary>
        [Fact]
        public void BuildRoutes_SingleRouteWithSemanticallyInvalidRule_WorksAndDoesNotThrow()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Rule", "Host('this invalid thing" },
            };

            // Act
            var routes = LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            List<GatewayRoute> expectedRoutes = new List<GatewayRoute>
            {
                new GatewayRoute
                {
                    RouteId = "MyCoolClusterId:MyRoute",
                    Rule = "Host('this invalid thing",
                    Priority = LabelsParser.DefaultRoutePriority,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string>(),
                },
            };
            routes.Should().BeEquivalentTo(expectedRoutes);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void BuildRoutes_MissingBackendId_UsesServiceName(int scenario)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Routes.MyRoute.Rule", "Host('example.com')" },
                { "IslandGateway.Routes.MyRoute.Priority", "2" },
            };

            if (scenario == 1)
            {
                labels.Add("IslandGateway.Backend.BackendId", string.Empty);
            }

            // Act
            var routes = LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            List<GatewayRoute> expectedRoutes = new List<GatewayRoute>
            {
                new GatewayRoute
                {
                    RouteId = $"{Uri.EscapeDataString(TestServiceName.ToString())}:MyRoute",
                    Rule = "Host('example.com')",
                    Priority = 2,
                    ClusterId = TestServiceName.ToString(),
                    Metadata = new Dictionary<string, string>(),
                },
            };
            routes.Should().BeEquivalentTo(expectedRoutes);
        }

        [Fact]
        public void BuildRoutes_MissingRule_Throws()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Priority", "2" },
                { "IslandGateway.Routes.MyRoute.Metadata.Foo", "Bar" },
            };

            // Act
            Func<List<GatewayRoute>> func = () => LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            func.Should().Throw<IslandGatewayConfigException>().WithMessage("Missing IslandGateway.Routes.MyRoute.Rule.");
        }

        [Fact]
        public void BuildRoutes_InvalidPriority_Throws()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Rule", "Host('example1.com') && Path('v2/{**rest}')" },
                { "IslandGateway.Routes.MyRoute.Priority", "this is no number" },
            };

            // Act
            Func<List<GatewayRoute>> func = () => LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            func.Should()
                .Throw<IslandGatewayConfigException>()
                .WithMessage("Could not convert label IslandGateway.Routes.MyRoute.Priority='this is no number' *");
        }

        [Theory]
        [InlineData("justcharacters")]
        [InlineData("UppercaseCharacters")]
        [InlineData("numbers1234")]
        [InlineData("Under_Score")]
        [InlineData("Hyphen-Hyphen")]
        public void BuildRoutes_ValidRouteName_Works(string routeName)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { $"IslandGateway.Routes.{routeName}.Rule", "Host('example.com')" },
                { $"IslandGateway.Routes.{routeName}.Priority", "2" },
            };

            // Act
            var routes = LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            List<GatewayRoute> expectedRoutes = new List<GatewayRoute>
            {
                new GatewayRoute
                {
                    RouteId = $"MyCoolClusterId:{routeName}",
                    Rule = "Host('example.com')",
                    Priority = 2,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string>(),
                },
            };
            routes.Should().BeEquivalentTo(expectedRoutes);
        }

        [Theory]
        [InlineData("IslandGateway.Routes..Priority", "that was an empty route name")]
        [InlineData("IslandGateway.Routes..Rule", "that was an empty route name")]
        [InlineData("IslandGateway.Routes.  .Rule", "that was an empty route name")]
        [InlineData("IslandGateway.Routes..", "that was an empty route name")]
        [InlineData("IslandGateway.Routes...", "that was an empty route name")]
        [InlineData("IslandGateway.Routes.FunnyChars!.Rule", "some value")]
        [InlineData("IslandGateway.Routes.'FunnyChars'.Priority", "some value")]
        [InlineData("IslandGateway.Routes.FunnyChárs.Metadata", "some value")]
        [InlineData("IslandGateway.Routes.Funny+Chars.Rule", "some value")]
        public void BuildRoutes_InvalidRouteName_Throws(string invalidKey, string value)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Rule", "Host('example.com')" },
                { "IslandGateway.Routes.MyRoute.Priority", "2" },
                { "IslandGateway.Routes.MyRoute.Metadata.Foo", "Bar" },
            };
            labels[invalidKey] = value;

            // Act
            Func<List<GatewayRoute>> func = () => LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            func.Should()
                .Throw<IslandGatewayConfigException>()
                .WithMessage($"Invalid route name '*', should only contain alphanumerical characters, underscores or hyphens.");
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("NotEven.TheNamespace", "some value")]
        [InlineData("IslandGateway.", "some value")]
        [InlineData("Routes.", "some value")]
        [InlineData("IslandGateway.Routes.", "some value")]
        [InlineData("IslandGateway.Routes", "some value")]
        [InlineData("IslandGateway..Routes.", "some value")]
        [InlineData("IslandGateway.....Routes.", "some value")]
        public void BuildRoutes_InvalidLabelKeys_IgnoresAndDoesNotThrow(string invalidKey, string value)
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Rule", "Host('example.com')" },
                { "IslandGateway.Routes.MyRoute.Priority", "2" },
                { "IslandGateway.Routes.MyRoute.Metadata.Foo", "Bar" },
            };
            labels[invalidKey] = value;

            // Act
            var routes = LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            List<GatewayRoute> expectedRoutes = new List<GatewayRoute>
            {
                new GatewayRoute
                {
                    RouteId = "MyCoolClusterId:MyRoute",
                    Rule = "Host('example.com')",
                    Priority = 2,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string>
                    {
                        { "Foo", "Bar" },
                    },
                },
            };
            routes.Should().BeEquivalentTo(expectedRoutes);
        }

        [Fact]
        public void BuildRoutes_MultipleRoutes_Works()
        {
            // Arrange
            var labels = new Dictionary<string, string>()
            {
                { "IslandGateway.Backend.BackendId", "MyCoolClusterId" },
                { "IslandGateway.Routes.MyRoute.Rule", "Host('example1.com') && Path('v2/{**rest}')" },
                { "IslandGateway.Routes.MyRoute.Priority", "1" },
                { "IslandGateway.Routes.MyRoute.Metadata.Foo", "Bar" },
                { "IslandGateway.Routes.CoolRoute.Rule", "Host('example2.com')" },
                { "IslandGateway.Routes.CoolRoute.Priority", "2" },
                { "IslandGateway.Routes.EvenCoolerRoute.Rule", "Host('example3.com')" },
                { "IslandGateway.Routes.EvenCoolerRoute.Priority", "3" },
            };

            // Act
            var routes = LabelsParser.BuildRoutes(TestServiceName, labels);

            // Assert
            List<GatewayRoute> expectedRoutes = new List<GatewayRoute>
            {
                new GatewayRoute
                {
                    RouteId = "MyCoolClusterId:MyRoute",
                    Rule = "Host('example1.com') && Path('v2/{**rest}')",
                    Priority = 1,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string> { { "Foo", "Bar" } },
                },
                new GatewayRoute
                {
                    RouteId = "MyCoolClusterId:CoolRoute",
                    Rule = "Host('example2.com')",
                    Priority = 2,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string>(),
                },
                new GatewayRoute
                {
                    RouteId = "MyCoolClusterId:EvenCoolerRoute",
                    Rule = "Host('example3.com')",
                    Priority = 3,
                    ClusterId = "MyCoolClusterId",
                    Metadata = new Dictionary<string, string>(),
                },
            };
            routes.Should().BeEquivalentTo(expectedRoutes);
        }
    }
}
