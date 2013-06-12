namespace System.Net.Mail
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Permissions;
    using System.Text;

    [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal static class IisPickupDirectory
    {
        private const int InfiniteTimeout = -1;
        private const int MaxPathSize = 260;
        private const int MetadataMaxNameLen = 0x100;

        internal static unsafe string GetPickupDirectory()
        {
            uint requiredDataLen = 0;
            string path = string.Empty;
            IMSAdminBase base2 = null;
            IntPtr zero = IntPtr.Zero;
            StringBuilder builder = new StringBuilder(0x100);
            uint num4 = 0x410;
            byte[] buffer = new byte[num4];
            try
            {
                base2 = new MSAdminBase() as IMSAdminBase;
                if (base2.OpenKey(IntPtr.Zero, "LM/SmtpSvc", MBKeyAccess.Read, -1, ref zero) >= 0)
                {
                    MetadataRecord data = new MetadataRecord();
                    try
                    {
                        fixed (byte* numRef = buffer)
                        {
                            int num;
                            int enumKeyIndex = 0;
                        Label_0080:
                            num = base2.EnumKeys(zero, "", builder, enumKeyIndex);
                            if (num == -2147024637)
                            {
                                goto Label_01AE;
                            }
                            if (num < 0)
                            {
                                goto Label_02D0;
                            }
                            data.Identifier = 0x3f8;
                            data.Attributes = 0;
                            data.UserType = 1;
                            data.DataType = 1;
                            data.DataTag = 0;
                            data.DataBuf = (IntPtr) numRef;
                            data.DataLen = num4;
                            num = base2.GetData(zero, builder.ToString(), ref data, ref requiredDataLen);
                            if (num < 0)
                            {
                                if ((num == -2146646015) || (num == -2147024891))
                                {
                                    goto Label_01A3;
                                }
                                goto Label_02D0;
                            }
                            if (Marshal.ReadInt32((IntPtr) numRef) == 2)
                            {
                                data.Identifier = 0x9010;
                                data.Attributes = 0;
                                data.UserType = 1;
                                data.DataType = 2;
                                data.DataTag = 0;
                                data.DataBuf = (IntPtr) numRef;
                                data.DataLen = num4;
                                num = base2.GetData(zero, builder.ToString(), ref data, ref requiredDataLen);
                                if (num < 0)
                                {
                                    goto Label_02D0;
                                }
                                path = Marshal.PtrToStringUni((IntPtr) numRef);
                                goto Label_01AE;
                            }
                        Label_01A3:
                            enumKeyIndex++;
                            goto Label_0080;
                        Label_01AE:
                            if (num != -2147024637)
                            {
                                goto Label_02D0;
                            }
                            int num6 = 0;
                        Label_01BC:
                            num = base2.EnumKeys(zero, "", builder, num6);
                            if ((num == -2147024637) || (num < 0))
                            {
                                goto Label_02D0;
                            }
                            data.Identifier = 0x9010;
                            data.Attributes = 0;
                            data.UserType = 1;
                            data.DataType = 2;
                            data.DataTag = 0;
                            data.DataBuf = (IntPtr) numRef;
                            data.DataLen = num4;
                            num = base2.GetData(zero, builder.ToString(), ref data, ref requiredDataLen);
                            if (num < 0)
                            {
                                if ((num == -2146646015) || (num == -2147024891))
                                {
                                    goto Label_026E;
                                }
                                goto Label_02D0;
                            }
                            path = Marshal.PtrToStringUni((IntPtr) numRef);
                            if (Directory.Exists(path))
                            {
                                goto Label_02D0;
                            }
                            path = string.Empty;
                        Label_026E:
                            num6++;
                            goto Label_01BC;
                        }
                    }
                    finally
                    {
                        numRef = null;
                    }
                }
            }
            catch (Exception exception)
            {
                if (((exception is SecurityException) || (exception is AuthenticationException)) || (exception is SmtpException))
                {
                    throw;
                }
                throw new SmtpException(SR.GetString("SmtpGetIisPickupDirectoryFailed"));
            }
            finally
            {
                if ((base2 != null) && (zero != IntPtr.Zero))
                {
                    base2.CloseKey(zero);
                }
            }
        Label_02D0:
            if (path == string.Empty)
            {
                throw new SmtpException(SR.GetString("SmtpGetIisPickupDirectoryFailed"));
            }
            return path;
        }
    }
}

