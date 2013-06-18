namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel.ComIntegration;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;

    public class UpnEndpointIdentity : EndpointIdentity
    {
        private bool hasUpnSidBeenComputed;
        private object thisLock;
        private SecurityIdentifier upnSid;
        private WindowsIdentity windowsIdentity;

        public UpnEndpointIdentity(Claim identity)
        {
            this.thisLock = new object();
            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }
            if (!identity.ClaimType.Equals(ClaimTypes.Upn))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("UnrecognizedClaimTypeForIdentity", new object[] { identity.ClaimType, ClaimTypes.Upn }));
            }
            base.Initialize(identity);
        }

        internal UpnEndpointIdentity(WindowsIdentity windowsIdentity)
        {
            this.thisLock = new object();
            if (windowsIdentity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");
            }
            this.windowsIdentity = windowsIdentity;
            this.upnSid = windowsIdentity.User;
            this.hasUpnSidBeenComputed = true;
        }

        public UpnEndpointIdentity(string upnName)
        {
            this.thisLock = new object();
            if (upnName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upnName");
            }
            base.Initialize(Claim.CreateUpnClaim(upnName));
            this.hasUpnSidBeenComputed = false;
        }

        internal override void EnsureIdentityClaim()
        {
            if (this.windowsIdentity != null)
            {
                lock (this.thisLock)
                {
                    if (this.windowsIdentity != null)
                    {
                        base.Initialize(Claim.CreateUpnClaim(this.GetUpnFromWindowsIdentity(this.windowsIdentity)));
                        this.windowsIdentity.Dispose();
                        this.windowsIdentity = null;
                    }
                }
            }
        }

        private string GetUpnFromDownlevelName(string downlevelName)
        {
            if (downlevelName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("downlevelName");
            }
            int index = downlevelName.IndexOf('\\');
            if (((index < 0) || (index == 0)) || (index == (downlevelName.Length - 1)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("DownlevelNameCannotMapToUpn", new object[] { downlevelName })));
            }
            string input = downlevelName.Substring(0, index + 1);
            string str2 = downlevelName.Substring(index + 1);
            uint size = 50;
            StringBuilder outputString = new StringBuilder((int) size);
            if (!SafeNativeMethods.TranslateName(input, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical, outputString, out size))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0x7a)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new Win32Exception(error));
                }
                outputString = new StringBuilder((int) size);
                if (!SafeNativeMethods.TranslateName(input, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical, outputString, out size))
                {
                    error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new Win32Exception(error));
                }
            }
            string str3 = outputString.Remove(outputString.Length - 1, 1).ToString();
            return (str2 + "@" + str3);
        }

        private string GetUpnFromWindowsIdentity(WindowsIdentity windowsIdentity)
        {
            string downlevelName = null;
            string upnFromDownlevelName = null;
            try
            {
                downlevelName = windowsIdentity.Name;
                upnFromDownlevelName = this.GetUpnFromDownlevelName(downlevelName);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
            }
            return (upnFromDownlevelName ?? downlevelName);
        }

        internal SecurityIdentifier GetUpnSid()
        {
            if (!this.hasUpnSidBeenComputed)
            {
                lock (this.thisLock)
                {
                    string resource = (string) base.IdentityClaim.Resource;
                    if (!this.hasUpnSidBeenComputed)
                    {
                        try
                        {
                            NTAccount account = new NTAccount(resource);
                            this.upnSid = account.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            if (exception is NullReferenceException)
                            {
                                throw;
                            }
                            SecurityTraceRecordHelper.TraceSpnToSidMappingFailure(resource, exception);
                        }
                        finally
                        {
                            this.hasUpnSidBeenComputed = true;
                        }
                    }
                }
            }
            return this.upnSid;
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteElementString(XD.AddressingDictionary.Upn, XD.AddressingDictionary.IdentityExtensionNamespace, (string) base.IdentityClaim.Resource);
        }
    }
}

