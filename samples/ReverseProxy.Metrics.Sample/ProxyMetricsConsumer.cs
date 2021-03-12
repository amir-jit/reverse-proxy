// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Yarp.ReverseProxy.Telemetry.Consumption;

namespace Yarp.Sample
{
    public sealed class ProxyMetricsConsumer : IProxyMetricsConsumer
    {
        public void OnProxyMetrics(ProxyMetrics oldMetrics, ProxyMetrics newMetrics)
        {
            var elapsed = newMetrics.Timestamp - oldMetrics.Timestamp;
            var newRequests = newMetrics.RequestsStarted - oldMetrics.RequestsStarted;
            Console.Title = $"Proxied {newMetrics.RequestsStarted} requests ({newRequests} in the last {(int)elapsed.TotalMilliseconds} ms)";
        }
    }
}
