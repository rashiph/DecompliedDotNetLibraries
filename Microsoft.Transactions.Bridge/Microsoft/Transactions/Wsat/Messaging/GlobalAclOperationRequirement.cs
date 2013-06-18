namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class GlobalAclOperationRequirement : ServiceAuthorizationManager
    {
        private ProtocolVersion protocolVersion;
        private Dictionary<Claim, Claim> sids;
        private Dictionary<Claim, Claim> thumbprints;

        public GlobalAclOperationRequirement(List<string> windowsIdentities, List<string> x509Thumbprints, ProtocolVersion protocolVersion)
        {
            this.BuildSidDictionary((windowsIdentities != null) ? windowsIdentities : new List<string>());
            this.BuildThumbprintDictionary((x509Thumbprints != null) ? x509Thumbprints : new List<string>());
            this.protocolVersion = protocolVersion;
        }

        private bool AccessCheck(AuthorizationContext authzContext, MessageProperties messageProperties, string binding)
        {
            bool result = false;
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "AccessCheck for binding {0}", binding);
            }
            if (string.Compare(binding, BindingStrings.InteropBindingQName(this.protocolVersion), StringComparison.Ordinal) == 0)
            {
                result = this.AccessCheck(authzContext, messageProperties, ClaimTypes.Thumbprint, this.thumbprints);
            }
            else if ((string.Compare(binding, BindingStrings.NamedPipeBindingQName, StringComparison.Ordinal) == 0) || (string.Compare(binding, BindingStrings.WindowsBindingQName, StringComparison.Ordinal) == 0))
            {
                result = this.AccessCheck(authzContext, messageProperties, ClaimTypes.Sid, this.sids);
            }
            else
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Unknown binding " + binding);
            }
            this.TraceAccessCheckResult(result, messageProperties);
            return result;
        }

        private bool AccessCheck(AuthorizationContext authzContext, MessageProperties messageProperties, string claimType, Dictionary<Claim, Claim> dictionary)
        {
            if (authzContext != null)
            {
                foreach (ClaimSet set in authzContext.ClaimSets)
                {
                    foreach (Claim claim in set.FindClaims(claimType, Rights.PossessProperty))
                    {
                        if (dictionary.ContainsKey(claim))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "No authzContext was passed into AccessCheck");
            }
            return false;
        }

        public bool AccessCheckReply(Message reply, string binding)
        {
            return this.AccessCheck(reply.Properties.Security.ServiceSecurityContext.AuthorizationContext, reply.Properties, binding);
        }

        private void BuildSidDictionary(List<string> windowsIdentities)
        {
            this.sids = new Dictionary<Claim, Claim>(windowsIdentities.Count, Claim.DefaultComparer);
            foreach (string str in windowsIdentities)
            {
                Exception exception = null;
                try
                {
                    NTAccount account = new NTAccount(str);
                    SecurityIdentifier sid = (SecurityIdentifier) account.Translate(typeof(SecurityIdentifier));
                    Claim key = Claim.CreateWindowsSidClaim(sid);
                    if (!this.sids.ContainsKey(key))
                    {
                        this.sids.Add(key, key);
                    }
                }
                catch (ArgumentException exception2)
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                    exception = exception2;
                }
                catch (IdentityNotMappedException exception3)
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                    exception = exception3;
                }
                catch (SystemException exception4)
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Warning);
                    exception = exception4;
                }
                if ((exception != null) && DebugTrace.Warning)
                {
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.Trace(TraceLevel.Warning, "Could not add account {0} to SID table: {1}", str, exception.Message);
                    }
                    else
                    {
                        DebugTrace.Trace(TraceLevel.Warning, "Could not add account to SID table: {0}", exception.GetType().Name);
                    }
                }
            }
        }

        private void BuildThumbprintDictionary(List<string> x509Thumbprints)
        {
            this.thumbprints = new Dictionary<Claim, Claim>(x509Thumbprints.Count, Claim.DefaultComparer);
            foreach (string str in x509Thumbprints)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    byte[] thumbprint = DecodeThumbprint(str);
                    if (thumbprint == null)
                    {
                        DebugTrace.Trace(TraceLevel.Warning, "Could not decode thumbprint {0}", str);
                    }
                    else
                    {
                        Claim key = Claim.CreateThumbprintClaim(thumbprint);
                        if (!this.thumbprints.ContainsKey(key))
                        {
                            this.thumbprints.Add(key, key);
                        }
                    }
                }
            }
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            return this.AccessCheck(operationContext.ServiceSecurityContext.AuthorizationContext, operationContext.IncomingMessageProperties, operationContext.EndpointDispatcher.ChannelDispatcher.BindingName);
        }

        private static byte[] DecodeThumbprint(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint) || (thumbprint.Length != 40))
            {
                return null;
            }
            byte[] buffer = new byte[20];
            for (int i = 0; i < buffer.Length; i++)
            {
                if (!byte.TryParse(thumbprint.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out buffer[i]))
                {
                    return null;
                }
            }
            return buffer;
        }

        private void TraceAccessCheckResult(bool result, MessageProperties messageProperties)
        {
            if (result)
            {
                if (DebugTrace.Info)
                {
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Info, "Access granted to {0} by global ACL", CoordinationServiceSecurity.GetSenderName(messageProperties));
                    }
                    else
                    {
                        DebugTrace.Trace(TraceLevel.Info, "Access granted by global ACL");
                    }
                }
            }
            else if (DebugTrace.Warning)
            {
                if (DebugTrace.Pii)
                {
                    DebugTrace.TracePii(TraceLevel.Warning, "Access denied to {0} by global ACL", CoordinationServiceSecurity.GetSenderName(messageProperties));
                }
                else
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Access denied by global ACL");
                }
            }
        }
    }
}

