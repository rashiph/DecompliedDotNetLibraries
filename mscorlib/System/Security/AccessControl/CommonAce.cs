namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public sealed class CommonAce : QualifiedAce
    {
        public CommonAce(AceFlags flags, AceQualifier qualifier, int accessMask, SecurityIdentifier sid, bool isCallback, byte[] opaque) : base(TypeFromQualifier(isCallback, qualifier), flags, accessMask, sid, opaque)
        {
        }

        public override void GetBinaryForm(byte[] binaryForm, int offset)
        {
            base.MarshalHeader(binaryForm, offset);
            int index = offset + 4;
            int num2 = 0;
            binaryForm[index] = (byte) base.AccessMask;
            binaryForm[index + 1] = (byte) (base.AccessMask >> 8);
            binaryForm[index + 2] = (byte) (base.AccessMask >> 0x10);
            binaryForm[index + 3] = (byte) (base.AccessMask >> 0x18);
            num2 += 4;
            base.SecurityIdentifier.GetBinaryForm(binaryForm, index + num2);
            num2 += base.SecurityIdentifier.BinaryLength;
            if (base.GetOpaque() != null)
            {
                if (base.OpaqueLength > this.MaxOpaqueLengthInternal)
                {
                    throw new SystemException();
                }
                base.GetOpaque().CopyTo(binaryForm, (int) (index + num2));
            }
        }

        public static int MaxOpaqueLength(bool isCallback)
        {
            return (0xfff7 - SecurityIdentifier.MaxBinaryLength);
        }

        internal static bool ParseBinaryForm(byte[] binaryForm, int offset, out AceQualifier qualifier, out int accessMask, out SecurityIdentifier sid, out bool isCallback, out byte[] opaque)
        {
            int num;
            GenericAce.VerifyHeader(binaryForm, offset);
            if ((binaryForm.Length - offset) < (8 + SecurityIdentifier.MinBinaryLength))
            {
                goto Label_0114;
            }
            AceType type = (AceType) binaryForm[offset];
            switch (type)
            {
                case AceType.AccessAllowed:
                case AceType.AccessDenied:
                case AceType.SystemAudit:
                case AceType.SystemAlarm:
                    isCallback = false;
                    break;

                default:
                    if (((type != AceType.AccessAllowedCallback) && (type != AceType.AccessDeniedCallback)) && ((type != AceType.SystemAuditCallback) && (type != AceType.SystemAlarmCallback)))
                    {
                        goto Label_0114;
                    }
                    isCallback = true;
                    break;
            }
            switch (type)
            {
                case AceType.AccessAllowed:
                case AceType.AccessAllowedCallback:
                    qualifier = AceQualifier.AccessAllowed;
                    break;

                default:
                    switch (type)
                    {
                        case AceType.AccessDenied:
                        case AceType.AccessDeniedCallback:
                            qualifier = AceQualifier.AccessDenied;
                            goto Label_0084;

                        case AceType.SystemAudit:
                        case AceType.SystemAuditCallback:
                            qualifier = AceQualifier.SystemAudit;
                            goto Label_0084;
                    }
                    if ((type != AceType.SystemAlarm) && (type != AceType.SystemAlarmCallback))
                    {
                        goto Label_0114;
                    }
                    qualifier = AceQualifier.SystemAlarm;
                    break;
            }
        Label_0084:
            num = offset + 4;
            int num2 = 0;
            accessMask = ((binaryForm[num] + (binaryForm[num + 1] << 8)) + (binaryForm[num + 2] << 0x10)) + (binaryForm[num + 3] << 0x18);
            num2 += 4;
            sid = new SecurityIdentifier(binaryForm, num + num2);
            opaque = null;
            int num3 = (binaryForm[offset + 3] << 8) + binaryForm[offset + 2];
            if ((num3 % 4) == 0)
            {
                int num4 = ((num3 - 4) - 4) - ((byte) sid.BinaryLength);
                if (num4 > 0)
                {
                    opaque = new byte[num4];
                    for (int i = 0; i < num4; i++)
                    {
                        opaque[i] = binaryForm[((offset + num3) - num4) + i];
                    }
                }
                return true;
            }
        Label_0114:
            qualifier = AceQualifier.AccessAllowed;
            accessMask = 0;
            sid = null;
            isCallback = false;
            opaque = null;
            return false;
        }

        private static AceType TypeFromQualifier(bool isCallback, AceQualifier qualifier)
        {
            switch (qualifier)
            {
                case AceQualifier.AccessAllowed:
                    if (isCallback)
                    {
                        return AceType.AccessAllowedCallback;
                    }
                    return AceType.AccessAllowed;

                case AceQualifier.AccessDenied:
                    if (isCallback)
                    {
                        return AceType.AccessDeniedCallback;
                    }
                    return AceType.AccessDenied;

                case AceQualifier.SystemAudit:
                    if (isCallback)
                    {
                        return AceType.SystemAuditCallback;
                    }
                    return AceType.SystemAudit;

                case AceQualifier.SystemAlarm:
                    if (isCallback)
                    {
                        return AceType.SystemAlarmCallback;
                    }
                    return AceType.SystemAlarm;
            }
            throw new ArgumentOutOfRangeException("qualifier", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
        }

        public override int BinaryLength
        {
            get
            {
                return ((8 + base.SecurityIdentifier.BinaryLength) + base.OpaqueLength);
            }
        }

        internal override int MaxOpaqueLengthInternal
        {
            get
            {
                return MaxOpaqueLength(base.IsCallback);
            }
        }
    }
}

