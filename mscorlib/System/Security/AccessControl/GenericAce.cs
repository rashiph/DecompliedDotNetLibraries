namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public abstract class GenericAce
    {
        private System.Security.AccessControl.AceFlags _flags;
        internal ushort _indexInAcl;
        private readonly System.Security.AccessControl.AceType _type;
        internal const int HeaderLength = 4;

        internal GenericAce(System.Security.AccessControl.AceType type, System.Security.AccessControl.AceFlags flags)
        {
            this._type = type;
            this._flags = flags;
        }

        internal static System.Security.AccessControl.AceFlags AceFlagsFromAuditFlags(System.Security.AccessControl.AuditFlags auditFlags)
        {
            System.Security.AccessControl.AceFlags none = System.Security.AccessControl.AceFlags.None;
            if ((auditFlags & System.Security.AccessControl.AuditFlags.Success) != System.Security.AccessControl.AuditFlags.None)
            {
                none = (System.Security.AccessControl.AceFlags) ((byte) (none | (System.Security.AccessControl.AceFlags.None | System.Security.AccessControl.AceFlags.SuccessfulAccess)));
            }
            if ((auditFlags & System.Security.AccessControl.AuditFlags.Failure) != System.Security.AccessControl.AuditFlags.None)
            {
                none = (System.Security.AccessControl.AceFlags) ((byte) (none | System.Security.AccessControl.AceFlags.FailedAccess));
            }
            if (none == System.Security.AccessControl.AceFlags.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "auditFlags");
            }
            return none;
        }

        internal static System.Security.AccessControl.AceFlags AceFlagsFromInheritanceFlags(System.Security.AccessControl.InheritanceFlags inheritanceFlags, System.Security.AccessControl.PropagationFlags propagationFlags)
        {
            System.Security.AccessControl.AceFlags none = System.Security.AccessControl.AceFlags.None;
            if ((inheritanceFlags & System.Security.AccessControl.InheritanceFlags.ContainerInherit) != System.Security.AccessControl.InheritanceFlags.None)
            {
                none = (System.Security.AccessControl.AceFlags) ((byte) (none | System.Security.AccessControl.AceFlags.ContainerInherit));
            }
            if ((inheritanceFlags & System.Security.AccessControl.InheritanceFlags.ObjectInherit) != System.Security.AccessControl.InheritanceFlags.None)
            {
                none = (System.Security.AccessControl.AceFlags) ((byte) (none | (System.Security.AccessControl.AceFlags.None | System.Security.AccessControl.AceFlags.ObjectInherit)));
            }
            if (none != System.Security.AccessControl.AceFlags.None)
            {
                if ((propagationFlags & System.Security.AccessControl.PropagationFlags.NoPropagateInherit) != System.Security.AccessControl.PropagationFlags.None)
                {
                    none = (System.Security.AccessControl.AceFlags) ((byte) (none | (System.Security.AccessControl.AceFlags.None | System.Security.AccessControl.AceFlags.NoPropagateInherit)));
                }
                if ((propagationFlags & System.Security.AccessControl.PropagationFlags.InheritOnly) != System.Security.AccessControl.PropagationFlags.None)
                {
                    none = (System.Security.AccessControl.AceFlags) ((byte) (none | System.Security.AccessControl.AceFlags.InheritOnly));
                }
            }
            return none;
        }

        public GenericAce Copy()
        {
            byte[] binaryForm = new byte[this.BinaryLength];
            this.GetBinaryForm(binaryForm, 0);
            return CreateFromBinaryForm(binaryForm, 0);
        }

        public static GenericAce CreateFromBinaryForm(byte[] binaryForm, int offset)
        {
            GenericAce ace;
            VerifyHeader(binaryForm, offset);
            System.Security.AccessControl.AceType type = (System.Security.AccessControl.AceType) binaryForm[offset];
            switch (type)
            {
                case System.Security.AccessControl.AceType.AccessAllowed:
                case System.Security.AccessControl.AceType.AccessDenied:
                case System.Security.AccessControl.AceType.SystemAudit:
                case System.Security.AccessControl.AceType.SystemAlarm:
                case System.Security.AccessControl.AceType.AccessAllowedCallback:
                case System.Security.AccessControl.AceType.AccessDeniedCallback:
                case System.Security.AccessControl.AceType.SystemAuditCallback:
                case System.Security.AccessControl.AceType.SystemAlarmCallback:
                {
                    AceQualifier qualifier;
                    int num;
                    SecurityIdentifier identifier;
                    bool flag;
                    byte[] buffer;
                    if (!CommonAce.ParseBinaryForm(binaryForm, offset, out qualifier, out num, out identifier, out flag, out buffer))
                    {
                        goto Label_01A8;
                    }
                    System.Security.AccessControl.AceFlags flags = (System.Security.AccessControl.AceFlags) binaryForm[offset + 1];
                    ace = new CommonAce(flags, qualifier, num, identifier, flag, buffer);
                    break;
                }
                case System.Security.AccessControl.AceType.AccessAllowedObject:
                case System.Security.AccessControl.AceType.AccessDeniedObject:
                case System.Security.AccessControl.AceType.SystemAuditObject:
                case System.Security.AccessControl.AceType.SystemAlarmObject:
                case System.Security.AccessControl.AceType.AccessAllowedCallbackObject:
                case System.Security.AccessControl.AceType.AccessDeniedCallbackObject:
                case System.Security.AccessControl.AceType.SystemAuditCallbackObject:
                case System.Security.AccessControl.AceType.SystemAlarmCallbackObject:
                {
                    AceQualifier qualifier2;
                    int num2;
                    SecurityIdentifier identifier2;
                    ObjectAceFlags flags2;
                    Guid guid;
                    Guid guid2;
                    bool flag2;
                    byte[] buffer2;
                    if (!ObjectAce.ParseBinaryForm(binaryForm, offset, out qualifier2, out num2, out identifier2, out flags2, out guid, out guid2, out flag2, out buffer2))
                    {
                        goto Label_01A8;
                    }
                    System.Security.AccessControl.AceFlags aceFlags = (System.Security.AccessControl.AceFlags) binaryForm[offset + 1];
                    ace = new ObjectAce(aceFlags, qualifier2, num2, identifier2, flags2, guid, guid2, flag2, buffer2);
                    break;
                }
                case System.Security.AccessControl.AceType.AccessAllowedCompound:
                {
                    int num3;
                    CompoundAceType type2;
                    SecurityIdentifier identifier3;
                    if (!CompoundAce.ParseBinaryForm(binaryForm, offset, out num3, out type2, out identifier3))
                    {
                        goto Label_01A8;
                    }
                    System.Security.AccessControl.AceFlags flags4 = (System.Security.AccessControl.AceFlags) binaryForm[offset + 1];
                    ace = new CompoundAce(flags4, num3, type2, identifier3);
                    break;
                }
                default:
                {
                    System.Security.AccessControl.AceFlags flags5 = (System.Security.AccessControl.AceFlags) binaryForm[offset + 1];
                    byte[] opaque = null;
                    int num4 = binaryForm[offset + 2] + (binaryForm[offset + 3] << 8);
                    if ((num4 % 4) != 0)
                    {
                        goto Label_01A8;
                    }
                    int num5 = num4 - 4;
                    if (num5 > 0)
                    {
                        opaque = new byte[num5];
                        for (int i = 0; i < num5; i++)
                        {
                            opaque[i] = binaryForm[((offset + num4) - num5) + i];
                        }
                    }
                    ace = new CustomAce(type, flags5, opaque);
                    break;
                }
            }
            if (((ace is ObjectAce) || ((binaryForm[offset + 2] + (binaryForm[offset + 3] << 8)) == ace.BinaryLength)) && ((!(ace is ObjectAce) || ((binaryForm[offset + 2] + (binaryForm[offset + 3] << 8)) == ace.BinaryLength)) || (((binaryForm[offset + 2] + (binaryForm[offset + 3] << 8)) - 0x20) == ace.BinaryLength)))
            {
                return ace;
            }
        Label_01A8:
            throw new ArgumentException(Environment.GetResourceString("ArgumentException_InvalidAceBinaryForm"), "binaryForm");
        }

        public sealed override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            GenericAce ace = o as GenericAce;
            if (ace == null)
            {
                return false;
            }
            if ((this.AceType != ace.AceType) || (this.AceFlags != ace.AceFlags))
            {
                return false;
            }
            int binaryLength = this.BinaryLength;
            int num2 = ace.BinaryLength;
            if (binaryLength != num2)
            {
                return false;
            }
            byte[] binaryForm = new byte[binaryLength];
            byte[] buffer2 = new byte[num2];
            this.GetBinaryForm(binaryForm, 0);
            ace.GetBinaryForm(buffer2, 0);
            for (int i = 0; i < binaryForm.Length; i++)
            {
                if (binaryForm[i] != buffer2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public abstract void GetBinaryForm(byte[] binaryForm, int offset);
        public sealed override int GetHashCode()
        {
            int binaryLength = this.BinaryLength;
            byte[] binaryForm = new byte[binaryLength];
            this.GetBinaryForm(binaryForm, 0);
            int num2 = 0;
            for (int i = 0; i < binaryLength; i += 4)
            {
                int num4 = ((binaryForm[i] + (binaryForm[i + 1] << 8)) + (binaryForm[i + 2] << 0x10)) + (binaryForm[i + 3] << 0x18);
                num2 ^= num4;
            }
            return num2;
        }

        internal void MarshalHeader(byte[] binaryForm, int offset)
        {
            int binaryLength = this.BinaryLength;
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((binaryForm.Length - offset) < this.BinaryLength)
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
            if (binaryLength > 0xffff)
            {
                throw new SystemException();
            }
            binaryForm[offset] = (byte) this.AceType;
            binaryForm[offset + 1] = (byte) this.AceFlags;
            binaryForm[offset + 2] = (byte) binaryLength;
            binaryForm[offset + 3] = (byte) (binaryLength >> 8);
        }

        public static bool operator ==(GenericAce left, GenericAce right)
        {
            object obj2 = left;
            object obj3 = right;
            return (((obj2 == null) && (obj3 == null)) || (((obj2 != null) && (obj3 != null)) && left.Equals(right)));
        }

        public static bool operator !=(GenericAce left, GenericAce right)
        {
            return !(left == right);
        }

        internal static void VerifyHeader(byte[] binaryForm, int offset)
        {
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((binaryForm.Length - offset) < 4)
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
            if (((binaryForm[offset + 3] << 8) + binaryForm[offset + 2]) > (binaryForm.Length - offset))
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
        }

        public System.Security.AccessControl.AceFlags AceFlags
        {
            get
            {
                return this._flags;
            }
            set
            {
                this._flags = value;
            }
        }

        public System.Security.AccessControl.AceType AceType
        {
            get
            {
                return this._type;
            }
        }

        public System.Security.AccessControl.AuditFlags AuditFlags
        {
            get
            {
                System.Security.AccessControl.AuditFlags none = System.Security.AccessControl.AuditFlags.None;
                if (((byte) (this.AceFlags & (System.Security.AccessControl.AceFlags.None | System.Security.AccessControl.AceFlags.SuccessfulAccess))) != 0)
                {
                    none |= System.Security.AccessControl.AuditFlags.Success;
                }
                if (((byte) (this.AceFlags & System.Security.AccessControl.AceFlags.FailedAccess)) != 0)
                {
                    none |= System.Security.AccessControl.AuditFlags.Failure;
                }
                return none;
            }
        }

        public abstract int BinaryLength { get; }

        public System.Security.AccessControl.InheritanceFlags InheritanceFlags
        {
            get
            {
                System.Security.AccessControl.InheritanceFlags none = System.Security.AccessControl.InheritanceFlags.None;
                if (((byte) (this.AceFlags & System.Security.AccessControl.AceFlags.ContainerInherit)) != 0)
                {
                    none |= System.Security.AccessControl.InheritanceFlags.ContainerInherit;
                }
                if (((byte) (this.AceFlags & (System.Security.AccessControl.AceFlags.None | System.Security.AccessControl.AceFlags.ObjectInherit))) != 0)
                {
                    none |= System.Security.AccessControl.InheritanceFlags.ObjectInherit;
                }
                return none;
            }
        }

        public bool IsInherited
        {
            get
            {
                return (((byte) (this.AceFlags & System.Security.AccessControl.AceFlags.Inherited)) != 0);
            }
        }

        public System.Security.AccessControl.PropagationFlags PropagationFlags
        {
            get
            {
                System.Security.AccessControl.PropagationFlags none = System.Security.AccessControl.PropagationFlags.None;
                if (((byte) (this.AceFlags & System.Security.AccessControl.AceFlags.InheritOnly)) != 0)
                {
                    none |= System.Security.AccessControl.PropagationFlags.InheritOnly;
                }
                if (((byte) (this.AceFlags & (System.Security.AccessControl.AceFlags.None | System.Security.AccessControl.AceFlags.NoPropagateInherit))) != 0)
                {
                    none |= System.Security.AccessControl.PropagationFlags.NoPropagateInherit;
                }
                return none;
            }
        }
    }
}

