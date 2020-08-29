// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.ReverseProxy.Abstractions
{
    public sealed class ProxyHttpClientOptions
    {
        public List<string> SslApplicationProtocols { get; set; }

        public X509RevocationMode? RevocationCheckMode { get; set; }

        public List<TlsCipherSuite> CipherSuitesPolicy { get; set; }

        public List<SslProtocols> SslProtocols { get; set; }

        public EncryptionPolicy? EncryptionPolicy { get; set; }

        public bool ValidateRemoteCertificate { get; set; } = true;

        public X509Certificate ClientCertificate { get; set; }

        public int? MaxConnectionsPerServer { get; set; }

        // TODO: Add this property once we have migrated to SDK version that supports it.
        //public bool? EnableMultipleHttp2Connections { get; set; }

        internal ProxyHttpClientOptions DeepClone()
        {
            return new ProxyHttpClientOptions
            {
                SslApplicationProtocols = SslApplicationProtocols.CloneList(),
                RevocationCheckMode = RevocationCheckMode,
                CipherSuitesPolicy = CipherSuitesPolicy.CloneList(),
                SslProtocols = SslProtocols.CloneList(),
                EncryptionPolicy = EncryptionPolicy,
                ValidateRemoteCertificate = ValidateRemoteCertificate,
                // TODO: Clone certificate?
                ClientCertificate = ClientCertificate,
                MaxConnectionsPerServer = MaxConnectionsPerServer
            };
        }
    }
}
