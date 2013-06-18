namespace System.ServiceModel
{
    using System;
    using System.DirectoryServices;
    using System.IdentityModel.Claims;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    public class SpnEndpointIdentity : EndpointIdentity
    {
        private static DirectoryEntry directoryEntry;
        private bool hasSpnSidBeenComputed;
        private static TimeSpan spnLookupTime = TimeSpan.FromMinutes(1.0);
        private SecurityIdentifier spnSid;
        private object thisLock;
        private static object typeLock = new object();

        public SpnEndpointIdentity(Claim identity)
        {
            this.thisLock = new object();
            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }
            if (!identity.ClaimType.Equals(ClaimTypes.Spn))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("UnrecognizedClaimTypeForIdentity", new object[] { identity.ClaimType, ClaimTypes.Spn }));
            }
            base.Initialize(identity);
        }

        public SpnEndpointIdentity(string spnName)
        {
            this.thisLock = new object();
            if (spnName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("spnName");
            }
            base.Initialize(Claim.CreateSpnClaim(spnName));
        }

        private static DirectoryEntry GetDirectoryEntry()
        {
            if (directoryEntry == null)
            {
                lock (typeLock)
                {
                    if (directoryEntry == null)
                    {
                        DirectoryEntry entry = new DirectoryEntry("LDAP://" + System.ServiceModel.Security.SecurityUtils.GetPrimaryDomain());
                        entry.RefreshCache(new string[] { "name" });
                        Thread.MemoryBarrier();
                        directoryEntry = entry;
                    }
                }
            }
            return directoryEntry;
        }

        internal SecurityIdentifier GetSpnSid()
        {
            if (!this.hasSpnSidBeenComputed)
            {
                lock (this.thisLock)
                {
                    if (!this.hasSpnSidBeenComputed)
                    {
                        string spn = null;
                        try
                        {
                            if (ClaimTypes.Dns.Equals(base.IdentityClaim.ClaimType))
                            {
                                spn = "host/" + ((string) base.IdentityClaim.Resource);
                            }
                            else
                            {
                                spn = (string) base.IdentityClaim.Resource;
                            }
                            if (spn != null)
                            {
                                spn = spn.Replace("*", @"\*").Replace("(", @"\(").Replace(")", @"\)");
                            }
                            using (DirectorySearcher searcher = new DirectorySearcher(GetDirectoryEntry()))
                            {
                                searcher.CacheResults = true;
                                searcher.ClientTimeout = SpnLookupTime;
                                searcher.Filter = "(&(objectCategory=Computer)(objectClass=computer)(servicePrincipalName=" + spn + "))";
                                searcher.PropertiesToLoad.Add("objectSid");
                                SearchResult result = searcher.FindOne();
                                if (result != null)
                                {
                                    byte[] binaryForm = (byte[]) result.Properties["objectSid"][0];
                                    this.spnSid = new SecurityIdentifier(binaryForm, 0);
                                }
                                else
                                {
                                    SecurityTraceRecordHelper.TraceSpnToSidMappingFailure(spn, null);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            if ((exception is NullReferenceException) || (exception is SEHException))
                            {
                                throw;
                            }
                            SecurityTraceRecordHelper.TraceSpnToSidMappingFailure(spn, exception);
                        }
                        finally
                        {
                            this.hasSpnSidBeenComputed = true;
                        }
                    }
                }
            }
            return this.spnSid;
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteElementString(XD.AddressingDictionary.Spn, XD.AddressingDictionary.IdentityExtensionNamespace, (string) base.IdentityClaim.Resource);
        }

        public static TimeSpan SpnLookupTime
        {
            get
            {
                return spnLookupTime;
            }
            set
            {
                if (value.Ticks < 0L)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value.Ticks, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                spnLookupTime = value;
            }
        }
    }
}

