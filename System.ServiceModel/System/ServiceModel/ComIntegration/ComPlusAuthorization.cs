namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal sealed class ComPlusAuthorization
    {
        private Dictionary<System.ServiceModel.ComIntegration.LUID, bool> accessCheckCache = new Dictionary<System.ServiceModel.ComIntegration.LUID, bool>();
        private string[] contractRoleMembers;
        private string[] operationRoleMembers;
        private CommonSecurityDescriptor securityDescriptor;
        private string[] serviceRoleMembers;
        private static SecurityIdentifier sidAdministrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

        public ComPlusAuthorization(string[] serviceRoleMembers, string[] contractRoleMembers, string[] operationRoleMembers)
        {
            this.serviceRoleMembers = serviceRoleMembers;
            this.contractRoleMembers = contractRoleMembers;
            this.operationRoleMembers = operationRoleMembers;
        }

        private void BuildSecurityDescriptor()
        {
            NTAccount account;
            SecurityIdentifier identifier;
            CommonAce ace;
            RawAcl rawAcl = new RawAcl(GenericAcl.AclRevision, 1);
            int index = 0;
            if (this.operationRoleMembers != null)
            {
                foreach (string str in this.operationRoleMembers)
                {
                    account = new NTAccount(str);
                    identifier = (SecurityIdentifier) account.Translate(typeof(SecurityIdentifier));
                    ace = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, 1, identifier, false, null);
                    rawAcl.InsertAce(index, ace);
                    index++;
                }
            }
            if (this.contractRoleMembers != null)
            {
                foreach (string str2 in this.contractRoleMembers)
                {
                    account = new NTAccount(str2);
                    identifier = (SecurityIdentifier) account.Translate(typeof(SecurityIdentifier));
                    ace = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, 1, identifier, false, null);
                    rawAcl.InsertAce(index, ace);
                    index++;
                }
            }
            if (this.serviceRoleMembers != null)
            {
                foreach (string str3 in this.serviceRoleMembers)
                {
                    account = new NTAccount(str3);
                    identifier = (SecurityIdentifier) account.Translate(typeof(SecurityIdentifier));
                    ace = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, 1, identifier, false, null);
                    rawAcl.InsertAce(index, ace);
                    index++;
                }
            }
            DiscretionaryAcl discretionaryAcl = new DiscretionaryAcl(true, false, rawAcl);
            this.securityDescriptor = new CommonSecurityDescriptor(true, false, ControlFlags.DiscretionaryAclPresent, sidAdministrators, sidAdministrators, null, discretionaryAcl);
        }

        private void CacheAccessCheck(System.ServiceModel.ComIntegration.LUID luidModifiedID, bool isAccessAllowed)
        {
            if (this.accessCheckCache == null)
            {
                throw Fx.AssertAndThrowFatal("AcessCheckCache must not be NULL");
            }
            lock (this)
            {
                this.accessCheckCache[luidModifiedID] = isAccessAllowed;
            }
        }

        private void CheckAccess(WindowsIdentity clientIdentity, out bool IsAccessAllowed)
        {
            if (this.securityDescriptor == null)
            {
                throw Fx.AssertAndThrowFatal("Security Descriptor must not be NULL");
            }
            IsAccessAllowed = false;
            byte[] binaryForm = new byte[this.securityDescriptor.BinaryLength];
            this.securityDescriptor.GetBinaryForm(binaryForm, 0);
            SafeCloseHandle newToken = null;
            SafeCloseHandle token = new SafeCloseHandle(clientIdentity.Token, false);
            try
            {
                if (System.ServiceModel.ComIntegration.SecurityUtils.IsPrimaryToken(token) && !SafeNativeMethods.DuplicateTokenEx(token, TokenAccessLevels.Query, IntPtr.Zero, SecurityImpersonationLevel.Identification, System.ServiceModel.ComIntegration.TokenType.TokenImpersonation, out newToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    Utility.CloseInvalidOutSafeHandle(newToken);
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("DuplicateTokenExFailed", new object[] { error })));
                }
                GENERIC_MAPPING genericMapping = new GENERIC_MAPPING();
                PRIVILEGE_SET structure = new PRIVILEGE_SET();
                uint privilegeSetLength = (uint) Marshal.SizeOf(structure);
                uint grantedAccess = 0;
                if (!SafeNativeMethods.AccessCheck(binaryForm, (newToken != null) ? newToken : token, 1, genericMapping, out structure, ref privilegeSetLength, out grantedAccess, out IsAccessAllowed))
                {
                    int num4 = Marshal.GetLastWin32Error();
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num4, System.ServiceModel.SR.GetString("AccessCheckFailed", new object[] { num4 })));
                }
            }
            finally
            {
                if (newToken != null)
                {
                    newToken.Dispose();
                }
            }
        }

        private bool IsAccessCached(System.ServiceModel.ComIntegration.LUID luidModifiedID, out bool isAccessAllowed)
        {
            if (this.accessCheckCache == null)
            {
                throw Fx.AssertAndThrowFatal("AcessCheckCache must not be NULL");
            }
            lock (this)
            {
                return this.accessCheckCache.TryGetValue(luidModifiedID, out isAccessAllowed);
            }
        }

        public bool IsAuthorizedForOperation(WindowsIdentity clientIdentity)
        {
            bool isAccessAllowed = false;
            if (clientIdentity == null)
            {
                throw Fx.AssertAndThrow("NULL Identity");
            }
            if (IntPtr.Zero == clientIdentity.Token)
            {
                throw Fx.AssertAndThrow("Token handle cannot be zero");
            }
            lock (this)
            {
                if (this.securityDescriptor == null)
                {
                    this.BuildSecurityDescriptor();
                }
            }
            System.ServiceModel.ComIntegration.LUID modifiedIDLUID = System.ServiceModel.ComIntegration.SecurityUtils.GetModifiedIDLUID(new SafeCloseHandle(clientIdentity.Token, false));
            if (!this.IsAccessCached(modifiedIDLUID, out isAccessAllowed))
            {
                this.CheckAccess(clientIdentity, out isAccessAllowed);
                this.CacheAccessCheck(modifiedIDLUID, isAccessAllowed);
            }
            return isAccessAllowed;
        }

        public string[] ContractRoleMembers
        {
            get
            {
                return this.contractRoleMembers;
            }
        }

        public string[] OperationRoleMembers
        {
            get
            {
                return this.operationRoleMembers;
            }
        }

        public CommonSecurityDescriptor SecurityDescriptor
        {
            get
            {
                return this.securityDescriptor;
            }
        }

        public string[] ServiceRoleMembers
        {
            get
            {
                return this.serviceRoleMembers;
            }
        }
    }
}

