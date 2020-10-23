// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.ReverseProxy.RuntimeModel;
using Microsoft.ReverseProxy.Service.Management;
using System;

namespace Microsoft.ReverseProxy.Service.HealthChecks
{
    internal class ReactivationScheduler : IReactivationScheduler, IDisposable
    {
        private readonly EntityActionScheduler<DestinationInfo> _scheduler;
        private readonly ILogger<ReactivationScheduler> _logger;

        public ReactivationScheduler(ILogger<ReactivationScheduler> logger)
        {
            _scheduler = new EntityActionScheduler<DestinationInfo>(Reactivate, autoStart: true, runOnce: true);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Schedule(DestinationInfo destination, TimeSpan reactivationPeriod)
        {
            _scheduler.ScheduleEntity(destination, reactivationPeriod);
            Log.UnhealthyDestinationIsScheduledForReactivation(_logger, destination.DestinationId, reactivationPeriod);
        }

        public void Dispose()
        {
            _scheduler.Dispose();
        }

        private void Reactivate(DestinationInfo destination)
        {
            var state = destination.DynamicState;
            if (state.Health.Passive == DestinationHealth.Unhealthy)
            {
                destination.DynamicState = new DestinationDynamicState(state.Health.ChangePassive(DestinationHealth.Unknown));
                Log.PassiveDestinationHealthResetToUnkownState(_logger, destination.DestinationId);
            }
        }

        private static class Log
        {
            private static readonly Action<ILogger, string, TimeSpan, Exception> _unhealthyDestinationIsScheduledForReactivation = LoggerMessage.Define<string, TimeSpan>(
                LogLevel.Information,
                EventIds.UnhealthyDestinationIsScheduledForReactivation,
                "Destination `{destinationId}` marked as 'unhealthy` by the passive health check is scheduled for a reactivation in `{reactivationPeriod}`.");

            private static readonly Action<ILogger, string, Exception> _passiveDestinationHealthResetToUnkownState = LoggerMessage.Define<string>(
                LogLevel.Information,
                EventIds.PassiveDestinationHealthResetToUnkownState,
                "Passive health state of the destination `{destinationId}` is reset to 'unknown`.");

            public static void UnhealthyDestinationIsScheduledForReactivation(ILogger logger, string destinationId, TimeSpan reactivationPeriod)
            {
                _unhealthyDestinationIsScheduledForReactivation(logger, destinationId, reactivationPeriod, null);
            }

            public static void PassiveDestinationHealthResetToUnkownState(ILogger logger, string destinationId)
            {
                _passiveDestinationHealthResetToUnkownState(logger, destinationId, null);
            }
        }
    }
}
