namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal static class HttpTransportSecurityHelpers
    {
        private static RemoteCertificateValidationCallback chainedServerCertValidationCallback = null;
        private static Dictionary<HttpWebRequest, string> serverCertMap = new Dictionary<HttpWebRequest, string>();
        private static bool serverCertValidationCallbackInstalled = false;
        private static Dictionary<string, int> targetNameCounter = new Dictionary<string, int>();

        public static bool AddIdentityMapping(Uri via, EndpointAddress target)
        {
            string spnFromIdentity;
            string absoluteUri = via.AbsoluteUri;
            EndpointIdentity identity = target.Identity;
            if ((identity != null) && !(identity is X509CertificateEndpointIdentity))
            {
                spnFromIdentity = System.ServiceModel.Security.SecurityUtils.GetSpnFromIdentity(identity, target);
            }
            else
            {
                spnFromIdentity = System.ServiceModel.Security.SecurityUtils.GetSpnFromTarget(target);
            }
            lock (targetNameCounter)
            {
                int num = 0;
                if (targetNameCounter.TryGetValue(absoluteUri, out num))
                {
                    if (!AuthenticationManager.CustomTargetNameDictionary.ContainsKey(absoluteUri) || (AuthenticationManager.CustomTargetNameDictionary[absoluteUri] != spnFromIdentity))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("HttpTargetNameDictionaryConflict", new object[] { absoluteUri, spnFromIdentity })));
                    }
                    targetNameCounter[absoluteUri] = num + 1;
                }
                else
                {
                    if (AuthenticationManager.CustomTargetNameDictionary.ContainsKey(absoluteUri) && (AuthenticationManager.CustomTargetNameDictionary[absoluteUri] != spnFromIdentity))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("HttpTargetNameDictionaryConflict", new object[] { absoluteUri, spnFromIdentity })));
                    }
                    AuthenticationManager.CustomTargetNameDictionary[absoluteUri] = spnFromIdentity;
                    targetNameCounter.Add(absoluteUri, 1);
                }
            }
            return true;
        }

        public static void AddServerCertMapping(HttpWebRequest request, EndpointAddress to)
        {
            X509CertificateEndpointIdentity identity = to.Identity as X509CertificateEndpointIdentity;
            if (identity != null)
            {
                AddServerCertMapping(request, identity.Certificates[0].Thumbprint);
            }
        }

        private static void AddServerCertMapping(HttpWebRequest request, string thumbprint)
        {
            lock (serverCertMap)
            {
                if (!serverCertValidationCallbackInstalled)
                {
                    chainedServerCertValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(HttpTransportSecurityHelpers.OnValidateServerCertificate);
                    serverCertValidationCallbackInstalled = true;
                }
                serverCertMap.Add(request, thumbprint);
            }
        }

        private static bool OnValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            HttpWebRequest key = sender as HttpWebRequest;
            if (key != null)
            {
                string str;
                lock (serverCertMap)
                {
                    serverCertMap.TryGetValue(key, out str);
                }
                if (str != null)
                {
                    try
                    {
                        ValidateServerCertificate(certificate, str);
                    }
                    catch (SecurityNegotiationException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                        return false;
                    }
                }
            }
            if (chainedServerCertValidationCallback == null)
            {
                return (sslPolicyErrors == SslPolicyErrors.None);
            }
            return chainedServerCertValidationCallback(sender, certificate, chain, sslPolicyErrors);
        }

        public static void RemoveIdentityMapping(Uri via, EndpointAddress target, bool validateState)
        {
            string spnFromIdentity;
            string absoluteUri = via.AbsoluteUri;
            EndpointIdentity identity = target.Identity;
            if ((identity != null) && !(identity is X509CertificateEndpointIdentity))
            {
                spnFromIdentity = System.ServiceModel.Security.SecurityUtils.GetSpnFromIdentity(identity, target);
            }
            else
            {
                spnFromIdentity = System.ServiceModel.Security.SecurityUtils.GetSpnFromTarget(target);
            }
            lock (targetNameCounter)
            {
                int num = targetNameCounter[absoluteUri];
                if (num == 1)
                {
                    targetNameCounter.Remove(absoluteUri);
                }
                else
                {
                    targetNameCounter[absoluteUri] = num - 1;
                }
                if (validateState && (!AuthenticationManager.CustomTargetNameDictionary.ContainsKey(absoluteUri) || (AuthenticationManager.CustomTargetNameDictionary[absoluteUri] != spnFromIdentity)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("HttpTargetNameDictionaryConflict", new object[] { absoluteUri, spnFromIdentity })));
                }
            }
        }

        public static void RemoveServerCertMapping(HttpWebRequest request)
        {
            lock (serverCertMap)
            {
                serverCertMap.Remove(request);
            }
        }

        private static void ValidateServerCertificate(X509Certificate certificate, string thumbprint)
        {
            string certHashString = certificate.GetCertHashString();
            if (!thumbprint.Equals(certHashString))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("HttpsServerCertThumbprintMismatch", new object[] { certificate.Subject, certHashString, thumbprint })));
            }
        }
    }
}

