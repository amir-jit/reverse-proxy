﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

using k8s;
using k8s.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Client;

public interface IAnyResourceKind
{
    //
    // Summary:
    //     list or watch cluster scoped custom objects
    //
    // Parameters:
    //   group:
    //     The custom resource's group name
    //
    //   version:
    //     The custom resource's version
    //
    //   plural:
    //     The custom resource's plural name. For TPRs this would be lowercase plural kind.
    //
    //   continueParameter:
    //     The continue option should be set when retrieving more results from the server.
    //     Since this value is server defined, clients may only use the continue value from
    //     a previous query result with identical query parameters (except for the value
    //     of continue) and the server may reject a continue value it does not recognize.
    //     If the specified continue value is no longer valid whether due to expiration
    //     (generally five to fifteen minutes) or a configuration change on the server,
    //     the server will respond with a 410 ResourceExpired error together with a continue
    //     token. If the client needs a consistent list, it must restart their list without
    //     the continue field. Otherwise, the client may send another list request with
    //     the token received with the 410 error, the server will respond with a list starting
    //     from the next key, but from the latest snapshot, which is inconsistent from the
    //     previous list results - objects that are created, modified, or deleted after
    //     the first list request will be included in the response, as long as their keys
    //     are after the "next key". This field is not supported when watch is true. Clients
    //     may start a watch from the last resourceVersion value returned by the server
    //     and not miss any modifications.
    //
    //   fieldSelector:
    //     A selector to restrict the list of returned objects by their fields. Defaults
    //     to everything.
    //
    //   labelSelector:
    //     A selector to restrict the list of returned objects by their labels. Defaults
    //     to everything.
    //
    //   limit:
    //     limit is a maximum number of responses to return for a list call. If more items
    //     exist, the server will set the `continue` field on the list metadata to a value
    //     that can be used with the same initial query to retrieve the next set of results.
    //     Setting a limit may return fewer than the requested amount of items (up to zero
    //     items) in the event all requested objects are filtered out and clients should
    //     only use the presence of the continue field to determine whether more results
    //     are available. Servers may choose not to support the limit argument and will
    //     return all of the available results. If limit is specified and the continue field
    //     is empty, clients may assume that no more results are available. This field is
    //     not supported if watch is true. The server guarantees that the objects returned
    //     when using continue will be identical to issuing a single list call without a
    //     limit - that is, no objects created, modified, or deleted after the first request
    //     is issued will be included in any subsequent continued requests. This is sometimes
    //     referred to as a consistent snapshot, and ensures that a client that is using
    //     limit to receive smaller chunks of a very large result can ensure they see all
    //     possible objects. If objects are updated during a chunked list the version of
    //     the object that was present at the time the first list result was calculated
    //     is returned.
    //
    //   resourceVersion:
    //     When specified with a watch call, shows changes that occur after that particular
    //     version of a resource. Defaults to changes from the beginning of history. When
    //     specified for list: - if unset, then the result is returned from remote storage
    //     based on quorum-read flag; - if it's 0, then we simply return what we currently
    //     have in cache, no guarantee; - if set to non zero, then the result is at least
    //     as fresh as given rv.
    //
    //   timeoutSeconds:
    //     Timeout for the list/watch call. This limits the duration of the call, regardless
    //     of any activity or inactivity.
    //
    //   watch:
    //     Watch for changes to the described resources and return them as a stream of add,
    //     update, and remove notifications.
    //
    //   pretty:
    //     If 'true', then the output is pretty printed.
    //
    //   customHeaders:
    //     The headers that will be added to request.
    //
    //   cancellationToken:
    //     The cancellation token.
    Task<HttpOperationResponse<KubernetesList<TResource>>> ListClusterAnyResourceKindWithHttpMessagesAsync<TResource>(string group, string version, string plural, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, bool? pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default) where TResource : k8s.IKubernetesObject;


    /// <summary>
    /// Creates a namespace scoped Custom object
    /// </summary>
    /// <param name='body'>
    /// The JSON schema of the Resource to create.
    /// </param>
    /// <param name='group'>
    /// The custom resource's group name
    /// </param>
    /// <param name='version'>
    /// The custom resource's version
    /// </param>
    /// <param name='namespaceParameter'>
    /// The custom resource's namespace
    /// </param>
    /// <param name='plural'>
    /// The custom resource's plural name. For TPRs this would be lowercase plural
    /// kind.
    /// </param>
    /// <param name='dryRun'>
    /// When present, indicates that modifications should not be persisted. An
    /// invalid or unrecognized dryRun directive will result in an error response
    /// and no further processing of the request. Valid values are: - All: all dry
    /// run stages will be processed
    /// </param>
    /// <param name='fieldManager'>
    /// fieldManager is a name associated with the actor or entity that is making
    /// these changes. The value must be less than or 128 characters long, and only
    /// contain printable characters, as defined by
    /// https://golang.org/pkg/unicode/#IsPrint.
    /// </param>
    /// <param name='pretty'>
    /// If 'true', then the output is pretty printed.
    /// </param>
    /// <param name='customHeaders'>
    /// Headers that will be added to request.
    /// </param>
    /// <param name='cancellationToken'>
    /// The cancellation token.
    /// </param>
    /// <exception cref="HttpOperationException">
    /// Thrown when the operation returned an invalid status code
    /// </exception>
    /// <exception cref="SerializationException">
    /// Thrown when unable to deserialize the response
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when a required parameter is null
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when a required parameter is null
    /// </exception>
    /// <return>
    /// A response object containing the response body and response headers.
    /// </return>
    public Task<HttpOperationResponse<object>> CreateAnyResourceKindWithHttpMessagesAsync<TResource>(TResource body, string group, string version, string namespaceParameter, string plural, string dryRun = default(string), string fieldManager = default(string), string pretty = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken)) where TResource : IKubernetesObject;


    /// <summary>
    /// patch the specified namespace scoped custom object
    /// </summary>
    /// <param name='body'>
    /// The JSON schema of the Resource to patch.
    /// </param>
    /// <param name='group'>
    /// the custom resource's group
    /// </param>
    /// <param name='version'>
    /// the custom resource's version
    /// </param>
    /// <param name='namespaceParameter'>
    /// The custom resource's namespace
    /// </param>
    /// <param name='plural'>
    /// the custom resource's plural name. For TPRs this would be lowercase plural
    /// kind.
    /// </param>
    /// <param name='name'>
    /// the custom object's name
    /// </param>
    /// <param name='dryRun'>
    /// When present, indicates that modifications should not be persisted. An
    /// invalid or unrecognized dryRun directive will result in an error response
    /// and no further processing of the request. Valid values are: - All: all dry
    /// run stages will be processed
    /// </param>
    /// <param name='fieldManager'>
    /// fieldManager is a name associated with the actor or entity that is making
    /// these changes. The value must be less than or 128 characters long, and only
    /// contain printable characters, as defined by
    /// https://golang.org/pkg/unicode/#IsPrint. This field is required for apply
    /// requests (application/apply-patch) but optional for non-apply patch types
    /// (JsonPatch, MergePatch, StrategicMergePatch).
    /// </param>
    /// <param name='force'>
    /// Force is going to "force" Apply requests. It means user will re-acquire
    /// conflicting fields owned by other people. Force flag must be unset for
    /// non-apply patch requests.
    /// </param>
    /// <param name='customHeaders'>
    /// Headers that will be added to request.
    /// </param>
    /// <param name='cancellationToken'>
    /// The cancellation token.
    /// </param>
    /// <exception cref="HttpOperationException">
    /// Thrown when the operation returned an invalid status code
    /// </exception>
    /// <exception cref="SerializationException">
    /// Thrown when unable to deserialize the response
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when a required parameter is null
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when a required parameter is null
    /// </exception>
    /// <return>
    /// A response object containing the response body and response headers.
    /// </return>
    public Task<HttpOperationResponse<object>> PatchAnyResourceKindWithHttpMessagesAsync(V1Patch body, string group, string version, string namespaceParameter, string plural, string name, string dryRun = default(string), string fieldManager = default(string), bool? force = default(bool?), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));


    /// <summary>
    /// Deletes the specified namespace scoped custom object
    /// </summary>
    /// <param name='group'>
    /// the custom resource's group
    /// </param>
    /// <param name='version'>
    /// the custom resource's version
    /// </param>
    /// <param name='namespaceParameter'>
    /// The custom resource's namespace
    /// </param>
    /// <param name='plural'>
    /// the custom resource's plural name. For TPRs this would be lowercase plural
    /// kind.
    /// </param>
    /// <param name='name'>
    /// the custom object's name
    /// </param>
    /// <param name='body'>
    /// </param>
    /// <param name='gracePeriodSeconds'>
    /// The duration in seconds before the object should be deleted. Value must be
    /// non-negative integer. The value zero indicates delete immediately. If this
    /// value is nil, the default grace period for the specified type will be used.
    /// Defaults to a per object value if not specified. zero means delete
    /// immediately.
    /// </param>
    /// <param name='orphanDependents'>
    /// Deprecated: please use the PropagationPolicy, this field will be deprecated
    /// in 1.7. Should the dependent objects be orphaned. If true/false, the
    /// "orphan" finalizer will be added to/removed from the object's finalizers
    /// list. Either this field or PropagationPolicy may be set, but not both.
    /// </param>
    /// <param name='propagationPolicy'>
    /// Whether and how garbage collection will be performed. Either this field or
    /// OrphanDependents may be set, but not both. The default policy is decided by
    /// the existing finalizer set in the metadata.finalizers and the
    /// resource-specific default policy.
    /// </param>
    /// <param name='dryRun'>
    /// When present, indicates that modifications should not be persisted. An
    /// invalid or unrecognized dryRun directive will result in an error response
    /// and no further processing of the request. Valid values are: - All: all dry
    /// run stages will be processed
    /// </param>
    /// <param name='customHeaders'>
    /// Headers that will be added to request.
    /// </param>
    /// <param name='cancellationToken'>
    /// The cancellation token.
    /// </param>
    /// <exception cref="HttpOperationException">
    /// Thrown when the operation returned an invalid status code
    /// </exception>
    /// <exception cref="SerializationException">
    /// Thrown when unable to deserialize the response
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when a required parameter is null
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when a required parameter is null
    /// </exception>
    /// <return>
    /// A response object containing the response body and response headers.
    /// </return>
    public Task<HttpOperationResponse<object>> DeleteAnyResourceKindWithHttpMessagesAsync(string group, string version, string namespaceParameter, string plural, string name, V1DeleteOptions body = default(V1DeleteOptions), int? gracePeriodSeconds = default(int?), bool? orphanDependents = default(bool?), string propagationPolicy = default(string), string dryRun = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

}
