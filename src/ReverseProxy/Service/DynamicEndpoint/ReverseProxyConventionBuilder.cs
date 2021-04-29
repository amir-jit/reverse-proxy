// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.RuntimeModel;

namespace Yarp.ReverseProxy
{
    public class ReverseProxyConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<Action<EndpointBuilder>> _conventions;

        internal ReverseProxyConventionBuilder(List<Action<EndpointBuilder>> conventions)
        {
            _conventions = conventions;
        }

        /// <summary>
        /// Adds the specified convention to the builder. Conventions are used to customize <see cref="EndpointBuilder"/> instances.
        /// </summary>
        /// <param name="convention">The convention to add to the builder.</param>
        public void Add(Action<EndpointBuilder> convention)
        {
            _ = convention ?? throw new ArgumentNullException(nameof(convention));

            _conventions.Add(convention);
        }

        /// <summary>
        /// Configures the endpoints for all routes 
        /// </summary>
        /// <param name="convention">The convention to add to the builder.</param>
        /// <returns></returns>
        public ReverseProxyConventionBuilder ConfigureEndpoints(Action<IEndpointConventionBuilder> convention)
        {
            _ = convention ?? throw new ArgumentNullException(nameof(convention));

            void Action(EndpointBuilder endpointBuilder)
            {
                var conventionBuilder = new EndpointBuilderConventionBuilder(endpointBuilder);
                convention(conventionBuilder);
            }

            Add(Action);

            return this;
        }

        /// <summary>
        /// Configures the endpoints for all routes 
        /// </summary>
        /// <param name="convention">The convention to add to the builder.</param>
        /// <returns></returns>
        public ReverseProxyConventionBuilder ConfigureEndpoints(Action<IEndpointConventionBuilder, RouteConfig> convention)
        {
            _ = convention ?? throw new ArgumentNullException(nameof(convention));

            void Action(EndpointBuilder endpointBuilder)
            {
                var route = endpointBuilder.Metadata.OfType<RouteModel>().Single();
                var conventionBuilder = new EndpointBuilderConventionBuilder(endpointBuilder);
                convention(conventionBuilder, route.Config);
            }

            Add(Action);

            return this;
        }

        /// <summary>
        /// Configures the endpoints for all routes 
        /// </summary>
        /// <param name="convention">The convention to add to the builder.</param>
        /// <returns></returns>
        public ReverseProxyConventionBuilder ConfigureEndpoints(Action<IEndpointConventionBuilder, RouteConfig, Cluster> convention)
        {
            _ = convention ?? throw new ArgumentNullException(nameof(convention));

            void Action(EndpointBuilder endpointBuilder)
            {
                var route = endpointBuilder.Metadata.OfType<RouteModel>().Single();

                var cluster = route.Cluster?.Config.Options;
                var proxyRoute = route.Config;
                var conventionBuilder = new EndpointBuilderConventionBuilder(endpointBuilder);
                convention(conventionBuilder, proxyRoute, cluster);
            }

            Add(Action);

            return this;
        }

        private class EndpointBuilderConventionBuilder : IEndpointConventionBuilder
        {
            private readonly EndpointBuilder _endpointBuilder;

            public EndpointBuilderConventionBuilder(EndpointBuilder endpointBuilder)
            {
                _endpointBuilder = endpointBuilder;
            }

            public void Add(Action<EndpointBuilder> convention)
            {
                convention(_endpointBuilder);
            }
        }
    }
}
