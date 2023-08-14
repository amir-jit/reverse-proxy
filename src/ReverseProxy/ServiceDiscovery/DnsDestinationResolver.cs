// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.ReverseProxy.ServiceDiscovery;

/// <summary>
/// Implementation of <see cref="IDestinationResolver"/> which resolves host names to IP addresses using DNS.
/// </summary>
public class DnsDestinationResolver : IDestinationResolver
{
    private readonly IOptionsMonitor<DnsDestinationResolverOptions> _options;
    private readonly ILogger<DnsDestinationResolver> _logger;

    /// <summary>
    /// Initializes a new <see cref="DnsDestinationResolver"/> instance.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public DnsDestinationResolver(IOptionsMonitor<DnsDestinationResolverOptions> options, ILogger<DnsDestinationResolver> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<ResolvedDestinationCollection> ResolveDestinationsAsync(IReadOnlyDictionary<string, DestinationConfig> destinations, CancellationToken cancellationToken)
    {
        try
        {
            var options = _options.CurrentValue;
            Dictionary<string, DestinationConfig> results = new();
            var tasks = new List<Task<List<(string Name, DestinationConfig Config)>>>(destinations.Count);
            foreach (var (destinationId, destinationConfig) in destinations)
            {
                tasks.Add(ResolveHostAsync(options, destinationId, destinationConfig, cancellationToken));
            }

            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                var configs = await task;
                foreach (var (name, config) in configs)
                {
                    results[name] = config;
                }
            }

            var changeToken = options.RefreshPeriod switch
            {
                { } refreshPeriod when refreshPeriod > TimeSpan.Zero => new CancellationChangeToken(new CancellationTokenSource(refreshPeriod).Token),
                _ => null,
            };

            return new ResolvedDestinationCollection(results, changeToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving destinations");
            throw;
        }
    }

    private static async Task<List<(string Name, DestinationConfig Config)>> ResolveHostAsync(
        DnsDestinationResolverOptions options,
        string originalName,
        DestinationConfig originalConfig,
        CancellationToken cancellationToken)
    {
        var originalUri = new Uri(originalConfig.Address);
        var originalHost = originalUri.Host;
        var addresses = options.AddressFamily switch
        {
            { } addressFamily => await Dns.GetHostAddressesAsync(originalUri.DnsSafeHost, addressFamily, cancellationToken).ConfigureAwait(false),
            null => await Dns.GetHostAddressesAsync(originalUri.DnsSafeHost, cancellationToken).ConfigureAwait(false)
        };
        var results = new List<(string Name, DestinationConfig Config)>(addresses.Length);
        var uriBuilder = new UriBuilder(originalUri);
        var healthUriBuilder = originalConfig.Health is { Length: > 0 } health ? new UriBuilder(health) : null;
        foreach (var address in addresses)
        {
            var addressString = address.ToString();
            uriBuilder.Host = addressString;
            var resolvedAddress = uriBuilder.ToString();
            var healthAddress = originalConfig.Health;
            if (healthUriBuilder is not null)
            {
                healthUriBuilder.Host = addressString;
                healthAddress = healthUriBuilder.ToString();
            }

            var name = $"{originalName}[{results.Count}]";
            var config = originalConfig with { Host = originalHost, Address = resolvedAddress, Health = healthAddress };
            results.Add((name, config));
        }

        return results;
    }
}
