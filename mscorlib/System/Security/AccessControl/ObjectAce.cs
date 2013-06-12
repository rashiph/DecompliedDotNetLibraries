namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public sealed class ObjectAce : QualifiedAce
    {
        private Guid _inheritedObjectAceType;
        private Guid _objectAceType;
        private System.Security.AccessControl.ObjectAceFlags _objectFlags;
        internal static readonly int AccessMaskWithObjectType = 0x13b;
        private const int GuidLength = 0x10;
        private const int ObjectFlagsLength = 4;

        public ObjectAce(AceFlags aceFlags, AceQualifier qualifier, int accessMask, SecurityIdentifier sid, System.Security.AccessControl.ObjectAceFlags flags, Guid type, Guid inheritedType, bool isCallback, byte[] opaque) : base(TypeFromQualifier(isCallback, qualifier), aceFlags, accessMask, sid, opaque)
        {
            this._objectFlags = flags;
            this._objectAceType = type;
            this._inheritedObjectAceType = inheritedType;
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
            binaryForm[index + num2] = (byte) this.ObjectAceFlags;
            binaryForm[(index + num2) + 1] = (byte) (((int) this.ObjectAceFlags) >> 8);
            binaryForm[(index + num2) + 2] = (byte) (((int) this.ObjectAceFlags) >> 0x10);
            binaryForm[(index + num2) + 3] = (byte) (((int) this.ObjectAceFlags) >> 0x18);
            num2 += 4;
            if ((this.ObjectAceFlags & System.Security.AccessControl.ObjectAceFlags.ObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None)
            {
                this.ObjectAceType.ToByteArray().CopyTo(binaryForm, (int) (index + num2));
                num2 += 0x10;
            }
            if ((this.ObjectAceFlags & System.Security.AccessControl.ObjectAceFlags.InheritedObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None)
            {
                this.InheritedObjectAceType.ToByteArray().CopyTo(binaryForm, (int) (index + num2));
                num2 += 0x10;
            }
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

        internal bool InheritedObjectTypesMatch(System.Security.AccessControl.ObjectAceFlags objectFlags, Guid inheritedObjectType)
        {
            if ((this.ObjectAceFlags & System.Security.AccessControl.ObjectAceFlags.InheritedObjectAceTypePresent) != (objectFlags & System.Security.AccessControl.ObjectAceFlags.InheritedObjectAceTypePresent))
            {
                return false;
            }
            if (((this.ObjectAceFlags & System.Security.AccessControl.ObjectAceFlags.InheritedObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None) && !this.InheritedObjectAceType.Equals(inheritedObjectType))
            {
                return false;
            }
            return true;
        }

        public static int MaxOpaqueLength(bool isCallback)
        {
            return (0xffd3 - SecurityIdentifier.MaxBinaryLength);
        }

        internal bool ObjectTypesMatch(System.Security.AccessControl.ObjectAceFlags objectFlags, Guid objectType)
        {
            if ((this.ObjectAceFlags & System.Security.AccessControl.ObjectAceFlags.ObjectAceTypePresent) != (objectFlags & System.Security.AccessControl.ObjectAceFlags.ObjectAceTypePresent))
            {
                return false;
            }
            if (((this.ObjectAceFlags & System.Security.AccessControl.ObjectAceFlags.ObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None) && !this.ObjectAceType.Equals(objectType))
            {
                return false;
            }
            return true;
        }

        internal static bool ParseBinaryForm(byte[] binaryForm, int offset, out AceQualifier qualifier, out int accessMask, out SecurityIdentifier sid, out System.Security.AccessControl.ObjectAceFlags objectFlags, out Guid objectAceType, out Guid inheritedObjectAceType, out bool isCallback, out byte[] opaque)
        {
            int num;
            byte[] b = new byte[0x10];
            GenericAce.VerifyHeader(binaryForm, offset);
            if ((binaryForm.Length - offset) < (12 + SecurityIdentifier.MinBinaryLength))
            {
                goto Label_0209;
            }
            AceType type = (AceType) binaryForm[offset];
            switch (type)
            {
                case AceType.AccessAllowedObject:
                case AceType.AccessDeniedObject:
                case AceType.SystemAuditObject:
                case AceType.SystemAlarmObject:
                    isCallback = false;
                    break;

                default:
                    if (((type != AceType.AccessAllowedCallbackObject) && (type != AceType.AccessDeniedCallbackObject)) && ((type != AceType.SystemAuditCallbackObject) && (type != AceType.SystemAlarmCallbackObject)))
                    {
                        goto Label_0209;
                    }
                    isCallback = true;
                    break;
            }
            switch (type)
            {
                case AceType.AccessAllowedObject:
                case AceType.AccessAllowedCallbackObject:
                    qualifier = AceQualifier.AccessAllowed;
                    break;

                default:
                    switch (type)
                    {
                        case AceType.AccessDeniedObject:
                        case AceType.AccessDeniedCallbackObject:
                            qualifier = AceQualifier.AccessDenied;
                            goto Label_008F;

                        case AceType.SystemAuditObject:
                        case AceType.SystemAuditCallbackObject:
                            qualifier = AceQualifier.SystemAudit;
                            goto Label_008F;
                    }
                    if ((type != AceType.SystemAlarmObject) && (type != AceType.SystemAlarmCallbackObject))
                    {
                        goto Label_0209;
                    }
                    qualifier = AceQualifier.SystemAlarm;
                    break;
            }
        Label_008F:
            num = offset + 4;
            int num2 = 0;
            accessMask = ((binaryForm[num] + (binaryForm[num + 1] << 8)) + (binaryForm[num + 2] << 0x10)) + (binaryForm[num + 3] << 0x18);
            num2 += 4;
            objectFlags = (System.Security.AccessControl.ObjectAceFlags) (((binaryForm[num + num2] + (binaryForm[(num + num2) + 1] << 8)) + (binaryForm[(num + num2) + 2] << 0x10)) + (binaryForm[(num + num2) + 3] << 0x18));
            num2 += 4;
            if ((objectFlags & System.Security.AccessControl.ObjectAceFlags.ObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None)
            {
                for (int i = 0; i < 0x10; i++)
                {
                    b[i] = binaryForm[(num + num2) + i];
                }
                num2 += 0x10;
            }
            else
            {
                for (int j = 0; j < 0x10; j++)
                {
                    b[j] = 0;
                }
            }
            objectAceType = new Guid(b);
            if ((objectFlags & System.Security.AccessControl.ObjectAceFlags.InheritedObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None)
            {
                for (int k = 0; k < 0x10; k++)
                {
                    b[k] = binaryForm[(num + num2) + k];
                }
                num2 += 0x10;
            }
            else
            {
                for (int m = 0; m < 0x10; m++)
                {
                    b[m] = 0;
                }
            }
            inheritedObjectAceType = new Guid(b);
            sid = new SecurityIdentifier(binaryForm, num + num2);
            opaque = null;
            int num7 = (binaryForm[offset + 3] << 8) + binaryForm[offset + 2];
            if ((num7 % 4) == 0)
            {
                int num8 = (((num7 - 4) - 4) - 4) - ((byte) sid.BinaryLength);
                if ((objectFlags & System.Security.AccessControl.ObjectAceFlags.ObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None)
                {
                    num8 -= 0x10;
                }
                if ((objectFlags & System.Security.AccessControl.ObjectAceFlags.InheritedObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None)
                {
                    num8 -= 0x10;
                }
                if (num8 > 0)
                {
                    opaque = new byte[num8];
                    for (int n = 0; n < num8; n++)
                    {
                        opaque[n] = binaryForm[((offset + num7) - num8) + n];
                    }
                }
                return true;
            }
        Label_0209:
            qualifier = AceQualifier.AccessAllowed;
            accessMask = 0;
            sid = null;
            objectFlags = System.Security.AccessControl.ObjectAceFlags.None;
            objectAceType = Guid.NewGuid();
            inheritedObjectAceType = Guid.NewGuid();
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
                        return AceType.AccessAllowedCallbackObject;
                    }
                    return AceType.AccessAllowedObject;

                case AceQualifier.AccessDenied:
                    if (isCallback)
                    {
                        return AceType.AccessDeniedCallbackObject;
                    }
                    return AceType.AccessDeniedObject;

                case AceQualifier.SystemAudit:
                    if (isCallback)
                    {
                        return AceType.SystemAuditCallbackObject;
                    }
                    return AceType.SystemAuditObject;

                case AceQualifier.SystemAlarm:
                    if (isCallback)
                    {
                        return AceType.SystemAlarmCallbackObject;
                    }
                    return AceType.SystemAlarmObject;
            }
            throw new ArgumentOutOfRangeException("qualifier", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
        }

        public override int BinaryLength
        {
            get
            {
                int num = (((this._objectFlags & System.Security.AccessControl.ObjectAceFlags.ObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None) ? 0x10 : 0) + (((this._objectFlags & System.Security.AccessControl.ObjectAceFlags.InheritedObjectAceTypePresent) != System.Security.AccessControl.ObjectAceFlags.None) ? 0x10 : 0);
                return (((12 + num) + base.SecurityIdentifier.BinaryLength) + base.OpaqueLength);
            }
        }

        public Guid InheritedObjectAceType
        {
            get
            {
                return this._inheritedObjectAceType;
            }
            set
            {
                this._inheritedObjectAceType = value;
            }
        }

        internal override int MaxOpaqueLengthInternal
        {
            get
            {
                return MaxOpaqueLength(base.IsCallback);
            }
        }

        public System.Security.AccessControl.ObjectAceFlags ObjectAceFlags
        {
            get
            {
                return this._objectFlags;
            }
            set
            {
                this._objectFlags = value;
            }
        }

        public Guid ObjectAceType
        {
            get
            {
                return this._objectAceType;
            }
            set
            {
                this._objectAceType = value;
            }
        }
    }
}

