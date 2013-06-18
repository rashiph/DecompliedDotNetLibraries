namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    internal static class Ports
    {
        public static Guid GetGuidFromTransactionId(string transactionId)
        {
            Guid guid;
            int num = "urn:uuid:".Length + 0x20;
            if (((transactionId.Length >= num) && transactionId.StartsWith("urn:uuid:", StringComparison.OrdinalIgnoreCase)) && System.ServiceModel.DiagnosticUtility.Utility.TryCreateGuid(transactionId.Substring("urn:uuid:".Length), out guid))
            {
                return guid;
            }
            HashAlgorithm algorithm = SHA256.Create();
            using (algorithm)
            {
                algorithm.Initialize();
                byte[] bytes = Encoding.UTF8.GetBytes(transactionId);
                byte[] sourceArray = algorithm.ComputeHash(bytes);
                byte[] destinationArray = new byte[0x10];
                Array.Copy(sourceArray, destinationArray, destinationArray.Length);
                return new Guid(destinationArray);
            }
        }

        public static string TryGetAddress(Proxy proxy)
        {
            if (proxy == null)
            {
                return "unknown";
            }
            return proxy.To.Uri.AbsoluteUri;
        }

        public static bool TryGetEnlistment(Message message, out Guid enlistmentId)
        {
            ControlProtocol protocol;
            return TryGetEnlistment(message, out enlistmentId, out protocol);
        }

        public static bool TryGetEnlistment(Message message, out Guid enlistmentId, out ControlProtocol protocol)
        {
            enlistmentId = Guid.Empty;
            protocol = ControlProtocol.None;
            try
            {
                if (!EnlistmentHeader.ReadFrom(message, out enlistmentId, out protocol))
                {
                    return false;
                }
            }
            catch (InvalidEnlistmentHeaderException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                return false;
            }
            return true;
        }

        public static string TryGetFromAddress(Message message)
        {
            EndpointAddress replyToHeader = Library.GetReplyToHeader(message.Headers);
            if (replyToHeader != null)
            {
                string absoluteUri = replyToHeader.Uri.AbsoluteUri;
                if (absoluteUri != null)
                {
                    return absoluteUri;
                }
            }
            return "unknown";
        }
    }
}

