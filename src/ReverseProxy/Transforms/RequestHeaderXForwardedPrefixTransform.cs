// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Yarp.ReverseProxy.Transforms
{
    /// <summary>
    /// Sets or appends the X-Forwarded-Prefix header with the request's original PathBase.
    /// </summary>
    public class RequestHeaderXForwardedPrefixTransform : RequestTransform
    {
        public RequestHeaderXForwardedPrefixTransform(string headerName, ForwardedTransformActions action)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentException($"'{nameof(headerName)}' cannot be null or empty.", nameof(headerName));
            }

            HeaderName = headerName;
            TransformAction = action;
        }

        internal string HeaderName { get; }
        internal ForwardedTransformActions TransformAction { get; }

        /// <inheritdoc/>
        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (TransformAction == ForwardedTransformActions.Off)
            {
                return default;
            }

            var existingValues = TakeHeader(context, HeaderName);

            var pathBase = context.HttpContext.Request.PathBase;

            switch (TransformAction)
            {
                case ForwardedTransformActions.Set:
                    if (pathBase.HasValue)
                    {
                        AddHeader(context, HeaderName, pathBase.ToUriComponent());
                    }
                    break;
                case ForwardedTransformActions.Append:
                    if (!pathBase.HasValue)
                    {
                        if (!string.IsNullOrEmpty(existingValues))
                        {
                            AddHeader(context, HeaderName, existingValues);
                        }
                    }
                    else
                    {
                        var values = StringValues.Concat(existingValues, pathBase.ToUriComponent());
                        AddHeader(context, HeaderName, values);
                    }
                    break;
                case ForwardedTransformActions.Remove:
                    RemoveHeader(context, HeaderName);
                    break;
                default:
                    throw new NotImplementedException(TransformAction.ToString());
            }

            return default;
        }
    }
}
