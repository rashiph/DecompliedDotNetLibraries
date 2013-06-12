namespace System.Security.AccessControl
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    internal static class Win32
    {
        internal const int TRUE = 1;

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal static int ConvertSdToSddl(byte[] binaryForm, int requestedRevision, SecurityInfos si, out string resultSddl)
        {
            int num;
            IntPtr ptr;
            uint resultStringLength = 0;
            if (1 != Win32Native.ConvertSdToStringSd(binaryForm, (uint) requestedRevision, (uint) si, out ptr, ref resultStringLength))
            {
                num = Marshal.GetLastWin32Error();
            }
            else
            {
                resultSddl = Marshal.PtrToStringUni(ptr);
                Win32Native.LocalFree(ptr);
                return 0;
            }
            resultSddl = null;
            if (num == 8)
            {
                throw new OutOfMemoryException();
            }
            return num;
        }

        [SecurityCritical, HandleProcessCorruptedStateExceptions]
        internal static int GetSecurityInfo(ResourceType resourceType, string name, SafeHandle handle, AccessControlSections accessControlSections, out RawSecurityDescriptor resultSd)
        {
            IntPtr ptr5;
            resultSd = null;
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            SecurityInfos infos = 0;
            Privilege privilege = null;
            if ((accessControlSections & AccessControlSections.Owner) != AccessControlSections.None)
            {
                infos |= SecurityInfos.Owner;
            }
            if ((accessControlSections & AccessControlSections.Group) != AccessControlSections.None)
            {
                infos |= SecurityInfos.Group;
            }
            if ((accessControlSections & AccessControlSections.Access) != AccessControlSections.None)
            {
                infos |= SecurityInfos.DiscretionaryAcl;
            }
            if ((accessControlSections & AccessControlSections.Audit) != AccessControlSections.None)
            {
                infos |= SecurityInfos.SystemAcl;
                privilege = new Privilege("SeSecurityPrivilege");
            }
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                int num;
                IntPtr ptr;
                IntPtr ptr2;
                IntPtr ptr3;
                IntPtr ptr4;
                if (privilege != null)
                {
                    try
                    {
                        privilege.Enable();
                    }
                    catch (PrivilegeNotHeldException)
                    {
                    }
                }
                if (name != null)
                {
                    num = (int) Win32Native.GetSecurityInfoByName(name, (uint) resourceType, (uint) infos, out ptr, out ptr2, out ptr3, out ptr4, out ptr5);
                }
                else
                {
                    if (handle == null)
                    {
                        throw new SystemException();
                    }
                    if (handle.IsInvalid)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSafeHandle"), "handle");
                    }
                    num = (int) Win32Native.GetSecurityInfoByHandle(handle, (uint) resourceType, (uint) infos, out ptr, out ptr2, out ptr3, out ptr4, out ptr5);
                }
                if ((num == 0) && IntPtr.Zero.Equals(ptr5))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoSecurityDescriptor"));
                }
                if ((num == 0x514) || (num == 0x522))
                {
                    throw new PrivilegeNotHeldException("SeSecurityPrivilege");
                }
                if ((num == 5) || (num == 0x543))
                {
                    throw new UnauthorizedAccessException();
                }
                if (num != 0)
                {
                    if (num == 8)
                    {
                        throw new OutOfMemoryException();
                    }
                    return num;
                }
            }
            catch
            {
                if (privilege != null)
                {
                    privilege.Revert();
                }
                throw;
            }
            finally
            {
                if (privilege != null)
                {
                    privilege.Revert();
                }
            }
            uint securityDescriptorLength = Win32Native.GetSecurityDescriptorLength(ptr5);
            byte[] destination = new byte[securityDescriptorLength];
            Marshal.Copy(ptr5, destination, 0, (int) securityDescriptorLength);
            Win32Native.LocalFree(ptr5);
            resultSd = new RawSecurityDescriptor(destination, 0);
            return 0;
        }

        [SecuritySafeCritical, HandleProcessCorruptedStateExceptions]
        internal static int SetSecurityInfo(ResourceType type, string name, SafeHandle handle, SecurityInfos securityInformation, SecurityIdentifier owner, SecurityIdentifier group, GenericAcl sacl, GenericAcl dacl)
        {
            byte[] binaryForm = null;
            byte[] buffer2 = null;
            byte[] buffer3 = null;
            byte[] buffer4 = null;
            Privilege privilege = null;
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            if (owner != null)
            {
                binaryForm = new byte[owner.BinaryLength];
                owner.GetBinaryForm(binaryForm, 0);
            }
            if (group != null)
            {
                buffer2 = new byte[group.BinaryLength];
                group.GetBinaryForm(buffer2, 0);
            }
            if (dacl != null)
            {
                buffer4 = new byte[dacl.BinaryLength];
                dacl.GetBinaryForm(buffer4, 0);
            }
            if (sacl != null)
            {
                buffer3 = new byte[sacl.BinaryLength];
                sacl.GetBinaryForm(buffer3, 0);
            }
            if ((securityInformation & SecurityInfos.SystemAcl) != 0)
            {
                privilege = new Privilege("SeSecurityPrivilege");
            }
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                int num;
                if (privilege != null)
                {
                    try
                    {
                        privilege.Enable();
                    }
                    catch (PrivilegeNotHeldException)
                    {
                    }
                }
                if (name != null)
                {
                    num = (int) Win32Native.SetSecurityInfoByName(name, (uint) type, (uint) securityInformation, binaryForm, buffer2, buffer4, buffer3);
                }
                else
                {
                    if (handle == null)
                    {
                        throw new InvalidProgramException();
                    }
                    if (handle.IsInvalid)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSafeHandle"), "handle");
                    }
                    num = (int) Win32Native.SetSecurityInfoByHandle(handle, (uint) type, (uint) securityInformation, binaryForm, buffer2, buffer4, buffer3);
                }
                if ((num == 0x514) || (num == 0x522))
                {
                    throw new PrivilegeNotHeldException("SeSecurityPrivilege");
                }
                if ((num == 5) || (num == 0x543))
                {
                    throw new UnauthorizedAccessException();
                }
                if (num != 0)
                {
                    if (num == 8)
                    {
                        throw new OutOfMemoryException();
                    }
                    return num;
                }
            }
            catch
            {
                if (privilege != null)
                {
                    privilege.Revert();
                }
                throw;
            }
            finally
            {
                if (privilege != null)
                {
                    privilege.Revert();
                }
            }
            return 0;
        }
    }
}

