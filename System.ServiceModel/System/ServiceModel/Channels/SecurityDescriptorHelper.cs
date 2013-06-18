namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.ServiceModel.Activation;

    internal static class SecurityDescriptorHelper
    {
        private static byte[] worldCreatorOwnerWithReadAndWriteDescriptorDenyNetwork = FromSecurityIdentifiersFull(null, -1073741824);
        private static byte[] worldCreatorOwnerWithReadDescriptorDenyNetwork = FromSecurityIdentifiersFull(null, -2147483648);

        internal static byte[] FromSecurityIdentifiers(List<SecurityIdentifier> allowedSids, int accessRights)
        {
            if (allowedSids == null)
            {
                if (accessRights == -1073741824)
                {
                    return worldCreatorOwnerWithReadAndWriteDescriptorDenyNetwork;
                }
                if (accessRights == -2147483648)
                {
                    return worldCreatorOwnerWithReadDescriptorDenyNetwork;
                }
            }
            return FromSecurityIdentifiersFull(allowedSids, accessRights);
        }

        private static byte[] FromSecurityIdentifiersFull(List<SecurityIdentifier> allowedSids, int accessRights)
        {
            int capacity = (allowedSids == null) ? 3 : (2 + allowedSids.Count);
            DiscretionaryAcl discretionaryAcl = new DiscretionaryAcl(false, false, capacity);
            discretionaryAcl.AddAccess(AccessControlType.Deny, new SecurityIdentifier(WellKnownSidType.NetworkSid, null), 0x10000000, InheritanceFlags.None, PropagationFlags.None);
            int accessMask = GenerateClientAccessRights(accessRights);
            if (allowedSids == null)
            {
                discretionaryAcl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.WorldSid, null), accessMask, InheritanceFlags.None, PropagationFlags.None);
            }
            else
            {
                for (int i = 0; i < allowedSids.Count; i++)
                {
                    SecurityIdentifier sid = allowedSids[i];
                    discretionaryAcl.AddAccess(AccessControlType.Allow, sid, accessMask, InheritanceFlags.None, PropagationFlags.None);
                }
            }
            discretionaryAcl.AddAccess(AccessControlType.Allow, GetProcessLogonSid(), accessRights, InheritanceFlags.None, PropagationFlags.None);
            CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor(false, false, ControlFlags.None, null, null, null, discretionaryAcl);
            byte[] binaryForm = new byte[descriptor.BinaryLength];
            descriptor.GetBinaryForm(binaryForm, 0);
            return binaryForm;
        }

        private static int GenerateClientAccessRights(int accessRights)
        {
            int num = accessRights;
            if ((num & 0x40000000) != 0)
            {
                num &= -1073741825;
                num |= 0x112;
            }
            return (num & -5);
        }

        private static SecurityIdentifier GetProcessLogonSid()
        {
            return Utility.GetLogonSidForPid(Process.GetCurrentProcess().Id);
        }
    }
}

