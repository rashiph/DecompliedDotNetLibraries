namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml;

    internal static class SignedXmlDebugLog
    {
        private const string NullString = "(null)";
        private static bool? s_informationLogging;
        private static TraceSource s_traceSource = new TraceSource("System.Security.Cryptography.Xml.SignedXml");
        private static bool? s_verboseLogging;

        private static string FormatBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return "(null)";
            }
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte num in bytes)
            {
                builder.Append(num.ToString("x2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        private static string GetKeyName(object key)
        {
            ICspAsymmetricAlgorithm algorithm = key as ICspAsymmetricAlgorithm;
            X509Certificate certificate = key as X509Certificate;
            X509Certificate2 certificate2 = key as X509Certificate2;
            string str = null;
            if ((algorithm != null) && (algorithm.CspKeyContainerInfo.KeyContainerName != null))
            {
                str = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[] { algorithm.CspKeyContainerInfo.KeyContainerName });
            }
            else if (certificate2 != null)
            {
                str = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[] { certificate2.GetNameInfo(X509NameType.SimpleName, false) });
            }
            else if (certificate != null)
            {
                str = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[] { certificate.Subject });
            }
            else
            {
                str = key.GetHashCode().ToString("x8", CultureInfo.InvariantCulture);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { key.GetType().Name, str });
        }

        private static string GetObjectId(object o)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { o.GetType().Name, o.GetHashCode().ToString("x8", CultureInfo.InvariantCulture) });
        }

        private static string GetOidName(Oid oid)
        {
            string friendlyName = oid.FriendlyName;
            if (string.IsNullOrEmpty(friendlyName))
            {
                friendlyName = oid.Value;
            }
            return friendlyName;
        }

        internal static void LogBeginCanonicalization(SignedXml signedXml, Transform canonicalizationTransform)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_BeginCanonicalization"), new object[] { canonicalizationTransform.Algorithm, canonicalizationTransform.GetType().Name });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.BeginCanonicalization, data);
            }
            if (VerboseLoggingEnabled)
            {
                string str2 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_CanonicalizationSettings"), new object[] { canonicalizationTransform.Resolver.GetType(), canonicalizationTransform.BaseURI });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.BeginCanonicalization, str2);
            }
        }

        internal static void LogBeginCheckSignatureFormat(SignedXml signedXml, Func<SignedXml, bool> formatValidator)
        {
            if (InformationLoggingEnabled)
            {
                MethodInfo method = formatValidator.Method;
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_CheckSignatureFormat"), new object[] { method.Module.Assembly.FullName, method.DeclaringType.FullName, method.Name });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.BeginCheckSignatureFormat, data);
            }
        }

        internal static void LogBeginCheckSignedInfo(SignedXml signedXml, SignedInfo signedInfo)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_CheckSignedInfo"), new object[] { (signedInfo.Id != null) ? signedInfo.Id : "(null)" });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.BeginCheckSignedInfo, data);
            }
        }

        internal static void LogBeginSignatureComputation(SignedXml signedXml, XmlElement context)
        {
            if (InformationLoggingEnabled)
            {
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.BeginSignatureComputation, SecurityResources.GetResourceString("Log_BeginSignatureComputation"));
            }
            if (VerboseLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_XmlContext"), new object[] { (context != null) ? context.OuterXml : "(null)" });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.BeginSignatureComputation, data);
            }
        }

        internal static void LogBeginSignatureVerification(SignedXml signedXml, XmlElement context)
        {
            if (InformationLoggingEnabled)
            {
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.BeginSignatureVerification, SecurityResources.GetResourceString("Log_BeginSignatureVerification"));
            }
            if (VerboseLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_XmlContext"), new object[] { (context != null) ? context.OuterXml : "(null)" });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.BeginSignatureVerification, data);
            }
        }

        internal static void LogCanonicalizedOutput(SignedXml signedXml, Transform canonicalizationTransform)
        {
            if (VerboseLoggingEnabled)
            {
                using (StreamReader reader = new StreamReader(canonicalizationTransform.GetOutput(typeof(Stream)) as Stream))
                {
                    string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_CanonicalizedOutput"), new object[] { reader.ReadToEnd() });
                    WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.CanonicalizedData, data);
                }
            }
        }

        internal static void LogFormatValidationResult(SignedXml signedXml, bool result)
        {
            if (InformationLoggingEnabled)
            {
                string data = result ? SecurityResources.GetResourceString("Log_FormatValidationSuccessful") : SecurityResources.GetResourceString("Log_FormatValidationNotSuccessful");
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.FormatValidationResult, data);
            }
        }

        internal static void LogNamespacePropagation(SignedXml signedXml, XmlNodeList namespaces)
        {
            if (InformationLoggingEnabled)
            {
                if (namespaces != null)
                {
                    foreach (XmlAttribute attribute in namespaces)
                    {
                        string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_PropagatingNamespace"), new object[] { attribute.Name, attribute.Value });
                        WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.NamespacePropagation, data);
                    }
                }
                else
                {
                    WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.NamespacePropagation, SecurityResources.GetResourceString("Log_NoNamespacesPropagated"));
                }
            }
        }

        internal static Stream LogReferenceData(Reference reference, Stream data)
        {
            if (VerboseLoggingEnabled)
            {
                MemoryStream stream = new MemoryStream();
                byte[] buffer = new byte[0x1000];
                int count = 0;
                do
                {
                    count = data.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, count);
                }
                while (count == buffer.Length);
                string str = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_TransformedReferenceContents"), new object[] { Encoding.UTF8.GetString(stream.ToArray()) });
                WriteLine(reference, TraceEventType.Verbose, SignedXmlDebugEvent.ReferenceData, str);
                stream.Seek(0L, SeekOrigin.Begin);
                return stream;
            }
            return data;
        }

        internal static void LogSigning(SignedXml signedXml, KeyedHashAlgorithm key)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_SigningHmac"), new object[] { key.GetType().Name });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.Signing, data);
            }
        }

        internal static void LogSigning(SignedXml signedXml, object key, SignatureDescription signatureDescription, HashAlgorithm hash, AsymmetricSignatureFormatter asymmetricSignatureFormatter)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_SigningAsymmetric"), new object[] { GetKeyName(key), signatureDescription.GetType().Name, hash.GetType().Name, asymmetricSignatureFormatter.GetType().Name });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.Signing, data);
            }
        }

        internal static void LogSigningReference(SignedXml signedXml, Reference reference)
        {
            if (VerboseLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_SigningReference"), new object[] { GetObjectId(reference), reference.Uri, reference.Id, reference.Type, reference.DigestMethod, CryptoConfig.CreateFromName(reference.DigestMethod).GetType().Name });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.SigningReference, data);
            }
        }

        internal static void LogVerificationFailure(SignedXml signedXml, string failureLocation)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_VerificationFailed"), new object[] { failureLocation });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.VerificationFailure, data);
            }
        }

        internal static void LogVerificationResult(SignedXml signedXml, object key, bool verified)
        {
            if (InformationLoggingEnabled)
            {
                string format = verified ? SecurityResources.GetResourceString("Log_VerificationWithKeySuccessful") : SecurityResources.GetResourceString("Log_VerificationWithKeyNotSuccessful");
                string data = string.Format(CultureInfo.InvariantCulture, format, new object[] { GetKeyName(key) });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.SignatureVerificationResult, data);
            }
        }

        internal static void LogVerifyKeyUsage(SignedXml signedXml, X509Certificate certificate, X509KeyUsageExtension keyUsages)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_KeyUsages"), new object[] { keyUsages.KeyUsages, GetOidName(keyUsages.Oid), GetKeyName(certificate) });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.X509Verification, data);
            }
        }

        internal static void LogVerifyReference(SignedXml signedXml, Reference reference)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_VerifyReference"), new object[] { GetObjectId(reference), reference.Uri, reference.Id, reference.Type });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.VerifyReference, data);
            }
        }

        internal static void LogVerifyReferenceHash(SignedXml signedXml, Reference reference, byte[] actualHash, byte[] expectedHash)
        {
            if (VerboseLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_ReferenceHash"), new object[] { GetObjectId(reference), reference.DigestMethod, CryptoConfig.CreateFromName(reference.DigestMethod).GetType().Name, FormatBytes(actualHash), FormatBytes(expectedHash) });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.VerifyReference, data);
            }
        }

        internal static void LogVerifySignedInfo(SignedXml signedXml, KeyedHashAlgorithm mac, byte[] actualHashValue, byte[] signatureValue)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_VerifySignedInfoHmac"), new object[] { mac.GetType().Name });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.VerifySignedInfo, data);
            }
            if (VerboseLoggingEnabled)
            {
                string str2 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_ActualHashValue"), new object[] { FormatBytes(actualHashValue) });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.VerifySignedInfo, str2);
                string str3 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_RawSignatureValue"), new object[] { FormatBytes(signatureValue) });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.VerifySignedInfo, str3);
            }
        }

        internal static void LogVerifySignedInfo(SignedXml signedXml, AsymmetricAlgorithm key, SignatureDescription signatureDescription, HashAlgorithm hashAlgorithm, AsymmetricSignatureDeformatter asymmetricSignatureDeformatter, byte[] actualHashValue, byte[] signatureValue)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_VerifySignedInfoAsymmetric"), new object[] { GetKeyName(key), signatureDescription.GetType().Name, hashAlgorithm.GetType().Name, asymmetricSignatureDeformatter.GetType().Name });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.VerifySignedInfo, data);
            }
            if (VerboseLoggingEnabled)
            {
                string str2 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_ActualHashValue"), new object[] { FormatBytes(actualHashValue) });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.VerifySignedInfo, str2);
                string str3 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_RawSignatureValue"), new object[] { FormatBytes(signatureValue) });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.VerifySignedInfo, str3);
            }
        }

        internal static void LogVerifyX509Chain(SignedXml signedXml, X509Chain chain, X509Certificate certificate)
        {
            if (InformationLoggingEnabled)
            {
                string data = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_BuildX509Chain"), new object[] { GetKeyName(certificate) });
                WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.X509Verification, data);
            }
            if (VerboseLoggingEnabled)
            {
                string str2 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_RevocationMode"), new object[] { chain.ChainPolicy.RevocationFlag });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.X509Verification, str2);
                string str3 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_RevocationFlag"), new object[] { chain.ChainPolicy.RevocationFlag });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.X509Verification, str3);
                string str4 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_VerificationFlag"), new object[] { chain.ChainPolicy.VerificationFlags });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.X509Verification, str4);
                string str5 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_VerificationTime"), new object[] { chain.ChainPolicy.VerificationTime });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.X509Verification, str5);
                string str6 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_UrlTimeout"), new object[] { chain.ChainPolicy.UrlRetrievalTimeout });
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.X509Verification, str6);
            }
            if (InformationLoggingEnabled)
            {
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    if (status.Status != X509ChainStatusFlags.NoError)
                    {
                        string str7 = string.Format(CultureInfo.InvariantCulture, SecurityResources.GetResourceString("Log_X509ChainError"), new object[] { status.Status, status.StatusInformation });
                        WriteLine(signedXml, TraceEventType.Information, SignedXmlDebugEvent.X509Verification, str7);
                    }
                }
            }
            if (VerboseLoggingEnabled)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(SecurityResources.GetResourceString("Log_CertificateChain"));
                X509ChainElementEnumerator enumerator = chain.ChainElements.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509ChainElement current = enumerator.Current;
                    builder.AppendFormat(CultureInfo.InvariantCulture, " {0}", new object[] { GetKeyName(current.Certificate) });
                }
                WriteLine(signedXml, TraceEventType.Verbose, SignedXmlDebugEvent.X509Verification, builder.ToString());
            }
        }

        private static void WriteLine(object source, TraceEventType eventType, SignedXmlDebugEvent eventId, string data)
        {
            s_traceSource.TraceEvent(eventType, (int) eventId, "[{0}, {1}] {2}", new object[] { GetObjectId(source), eventId, data });
        }

        private static bool InformationLoggingEnabled
        {
            get
            {
                if (!s_informationLogging.HasValue)
                {
                    s_informationLogging = new bool?(s_traceSource.Switch.ShouldTrace(TraceEventType.Information));
                }
                return s_informationLogging.Value;
            }
        }

        private static bool VerboseLoggingEnabled
        {
            get
            {
                if (!s_verboseLogging.HasValue)
                {
                    s_verboseLogging = new bool?(s_traceSource.Switch.ShouldTrace(TraceEventType.Verbose));
                }
                return s_verboseLogging.Value;
            }
        }

        internal enum SignedXmlDebugEvent
        {
            BeginCanonicalization,
            BeginCheckSignatureFormat,
            BeginCheckSignedInfo,
            BeginSignatureComputation,
            BeginSignatureVerification,
            CanonicalizedData,
            FormatValidationResult,
            NamespacePropagation,
            ReferenceData,
            SignatureVerificationResult,
            Signing,
            SigningReference,
            VerificationFailure,
            VerifyReference,
            VerifySignedInfo,
            X509Verification
        }
    }
}

