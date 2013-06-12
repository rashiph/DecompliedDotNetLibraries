namespace System.Security.AccessControl
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public abstract class CommonAcl : GenericAcl
    {
        private System.Security.AccessControl.RawAcl _acl;
        private readonly bool _isCanonical;
        private readonly bool _isContainer;
        private bool _isDirty;
        private readonly bool _isDS;
        private static PM[] AFtoPM = new PM[0x10];
        private static AF[] PMtoAF;

        static CommonAcl()
        {
            for (int i = 0; i < AFtoPM.Length; i++)
            {
                AFtoPM[i] = PM.GO;
            }
            AFtoPM[0] = PM.F;
            AFtoPM[4] = PM.F | PM.CO | PM.GO;
            AFtoPM[5] = PM.F | PM.CO;
            AFtoPM[6] = PM.CO | PM.GO;
            AFtoPM[7] = PM.CO;
            AFtoPM[8] = PM.F | PM.CF | PM.GF;
            AFtoPM[9] = PM.F | PM.CF;
            AFtoPM[10] = PM.CF | PM.GF;
            AFtoPM[11] = PM.CF;
            AFtoPM[12] = PM.F | PM.CF | PM.CO | PM.GF | PM.GO;
            AFtoPM[13] = PM.F | PM.CF | PM.CO;
            AFtoPM[14] = PM.CF | PM.CO | PM.GF | PM.GO;
            AFtoPM[15] = PM.CF | PM.CO;
            PMtoAF = new AF[0x20];
            for (int j = 0; j < PMtoAF.Length; j++)
            {
                PMtoAF[j] = AF.Invalid;
            }
            PMtoAF[0x10] = 0;
            PMtoAF[0x15] = AF.OI;
            PMtoAF[20] = AF.OI | AF.Invalid;
            PMtoAF[5] = AF.OI | AF.IO;
            PMtoAF[4] = AF.OI | AF.IO | AF.Invalid;
            PMtoAF[0x1a] = AF.CI;
            PMtoAF[0x18] = AF.CI | AF.Invalid;
            PMtoAF[10] = AF.CI | AF.IO;
            PMtoAF[8] = AF.CI | AF.IO | AF.Invalid;
            PMtoAF[0x1f] = AF.CI | AF.OI;
            PMtoAF[0x1c] = AF.CI | AF.OI | AF.Invalid;
            PMtoAF[15] = AF.CI | AF.OI | AF.IO;
            PMtoAF[12] = AF.CI | AF.OI | AF.IO | AF.Invalid;
        }

        internal CommonAcl(bool isContainer, bool isDS, byte revision, int capacity)
        {
            this._isContainer = isContainer;
            this._isDS = isDS;
            this._acl = new System.Security.AccessControl.RawAcl(revision, capacity);
            this._isCanonical = true;
        }

        internal CommonAcl(bool isContainer, bool isDS, System.Security.AccessControl.RawAcl rawAcl, bool trusted, bool isDacl)
        {
            if (rawAcl == null)
            {
                throw new ArgumentNullException("rawAcl");
            }
            this._isContainer = isContainer;
            this._isDS = isDS;
            if (trusted)
            {
                this._acl = rawAcl;
                this.RemoveMeaninglessAcesAndFlags(isDacl);
            }
            else
            {
                this._acl = new System.Security.AccessControl.RawAcl(rawAcl.Revision, rawAcl.Count);
                for (int i = 0; i < rawAcl.Count; i++)
                {
                    GenericAce ace = rawAcl[i].Copy();
                    if (this.InspectAce(ref ace, isDacl))
                    {
                        this._acl.InsertAce(this._acl.Count, ace);
                    }
                }
            }
            if (this.CanonicalCheck(isDacl))
            {
                this.Canonicalize(true, isDacl);
                this._isCanonical = true;
            }
            else
            {
                this._isCanonical = false;
            }
        }

        private bool AccessMasksAreMergeable(QualifiedAce ace, QualifiedAce newAce)
        {
            if (this.ObjectTypesMatch(ace, newAce))
            {
                return true;
            }
            ObjectAceFlags flags = (ace is ObjectAce) ? ((ObjectAce) ace).ObjectAceFlags : ObjectAceFlags.None;
            return ((((ace.AccessMask & newAce.AccessMask) & ObjectAce.AccessMaskWithObjectType) == (newAce.AccessMask & ObjectAce.AccessMaskWithObjectType)) && ((flags & ObjectAceFlags.ObjectAceTypePresent) == ObjectAceFlags.None));
        }

        private bool AceFlagsAreMergeable(QualifiedAce ace, QualifiedAce newAce)
        {
            if (this.InheritedObjectTypesMatch(ace, newAce))
            {
                return true;
            }
            ObjectAceFlags flags = (ace is ObjectAce) ? ((ObjectAce) ace).ObjectAceFlags : ObjectAceFlags.None;
            return ((flags & ObjectAceFlags.InheritedObjectAceTypePresent) == ObjectAceFlags.None);
        }

        private static AceFlags AceFlagsFromAF(AF af, bool isDS)
        {
            AceFlags none = AceFlags.None;
            if ((af & AF.CI) != 0)
            {
                none = (AceFlags) ((byte) (none | AceFlags.ContainerInherit));
            }
            if (!isDS && ((af & AF.OI) != 0))
            {
                none = (AceFlags) ((byte) (none | (AceFlags.None | AceFlags.ObjectInherit)));
            }
            if ((af & AF.IO) != 0)
            {
                none = (AceFlags) ((byte) (none | AceFlags.InheritOnly));
            }
            if ((af & AF.Invalid) != 0)
            {
                none = (AceFlags) ((byte) (none | (AceFlags.None | AceFlags.NoPropagateInherit)));
            }
            return none;
        }

        internal void AddQualifiedAce(SecurityIdentifier sid, AceQualifier qualifier, int accessMask, AceFlags flags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            GenericAce ace;
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            this.ThrowIfNotCanonical();
            bool flag = false;
            if ((qualifier == AceQualifier.SystemAudit) && (((byte) (flags & AceFlags.AuditFlags)) == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "flags");
            }
            if (accessMask == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "accessMask");
            }
            if (!this.IsDS || (objectFlags == ObjectAceFlags.None))
            {
                ace = new CommonAce(flags, qualifier, accessMask, sid, false, null);
            }
            else
            {
                ace = new ObjectAce(flags, qualifier, accessMask, sid, objectFlags, objectType, inheritedObjectType, false, null);
            }
            if (this.InspectAce(ref ace, this is DiscretionaryAcl))
            {
                for (int i = 0; i < this.Count; i++)
                {
                    QualifiedAce ace2 = this._acl[i] as QualifiedAce;
                    if ((ace2 != null) && this.MergeAces(ref ace2, ace as QualifiedAce))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    this._acl.InsertAce(this._acl.Count, ace);
                    this._isDirty = true;
                }
                this.OnAclModificationTried();
            }
        }

        private static AF AFFromAceFlags(AceFlags aceFlags, bool isDS)
        {
            AF af = 0;
            if (((byte) (aceFlags & AceFlags.ContainerInherit)) != 0)
            {
                af |= AF.CI;
            }
            if (!isDS && (((byte) (aceFlags & (AceFlags.None | AceFlags.ObjectInherit))) != 0))
            {
                af |= AF.OI;
            }
            if (((byte) (aceFlags & AceFlags.InheritOnly)) != 0)
            {
                af |= AF.IO;
            }
            if (((byte) (aceFlags & (AceFlags.None | AceFlags.NoPropagateInherit))) != 0)
            {
                af |= AF.Invalid;
            }
            return af;
        }

        private bool CanonicalCheck(bool isDacl)
        {
            if (isDacl)
            {
                int num = 0;
                for (int i = 0; i < this._acl.Count; i++)
                {
                    int num3 = 3;
                    GenericAce ace = this._acl[i];
                    if (((byte) (ace.AceFlags & AceFlags.Inherited)) != 0)
                    {
                        num3 = 2;
                    }
                    else
                    {
                        QualifiedAce ace2 = ace as QualifiedAce;
                        if (ace2 != null)
                        {
                            if (ace2.AceQualifier == AceQualifier.AccessAllowed)
                            {
                                num3 = 1;
                                goto Label_0059;
                            }
                            if (ace2.AceQualifier == AceQualifier.AccessDenied)
                            {
                                num3 = 0;
                                goto Label_0059;
                            }
                        }
                        return false;
                    }
                Label_0059:
                    if (num3 != 3)
                    {
                        if (num3 > num)
                        {
                            num = num3;
                        }
                        else if (num3 < num)
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                int num4 = 0;
                for (int j = 0; j < this._acl.Count; j++)
                {
                    int num6 = 2;
                    GenericAce ace3 = this._acl[j];
                    if (ace3 != null)
                    {
                        if (((byte) (ace3.AceFlags & AceFlags.Inherited)) != 0)
                        {
                            num6 = 1;
                        }
                        else
                        {
                            QualifiedAce ace4 = ace3 as QualifiedAce;
                            if (ace4 == null)
                            {
                                return false;
                            }
                            if ((ace4.AceQualifier != AceQualifier.SystemAudit) && (ace4.AceQualifier != AceQualifier.SystemAlarm))
                            {
                                return false;
                            }
                            num6 = 0;
                        }
                        if (num6 > num4)
                        {
                            num4 = num6;
                        }
                        else if (num6 < num4)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void Canonicalize(bool compact, bool isDacl)
        {
            for (ushort i = 0; i < this._acl.Count; i = (ushort) (i + 1))
            {
                this._acl[i]._indexInAcl = i;
            }
            this.QuickSort(0, this._acl.Count - 1, isDacl);
            if (compact)
            {
                for (int j = 0; j < (this.Count - 1); j++)
                {
                    QualifiedAce ace = this._acl[j] as QualifiedAce;
                    if (ace != null)
                    {
                        QualifiedAce newAce = this._acl[j + 1] as QualifiedAce;
                        if ((newAce != null) && this.MergeAces(ref ace, newAce))
                        {
                            this._acl.RemoveAce(j + 1);
                        }
                    }
                }
            }
        }

        private void CanonicalizeIfNecessary()
        {
            if (this._isDirty)
            {
                this.Canonicalize(false, this is DiscretionaryAcl);
                this._isDirty = false;
            }
        }

        internal void CheckAccessType(AccessControlType accessType)
        {
            if ((accessType != AccessControlType.Allow) && (accessType != AccessControlType.Deny))
            {
                throw new ArgumentOutOfRangeException("accessType", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
        }

        internal void CheckFlags(InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            if (this.IsContainer)
            {
                if ((inheritanceFlags == InheritanceFlags.None) && (propagationFlags != PropagationFlags.None))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAnyFlag"), "propagationFlags");
                }
            }
            else
            {
                if (inheritanceFlags != InheritanceFlags.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAnyFlag"), "inheritanceFlags");
                }
                if (propagationFlags != PropagationFlags.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAnyFlag"), "propagationFlags");
                }
            }
        }

        private static ComparisonResult CompareAces(GenericAce ace1, GenericAce ace2, bool isDacl)
        {
            int num = isDacl ? DaclAcePriority(ace1) : SaclAcePriority(ace1);
            int num2 = isDacl ? DaclAcePriority(ace2) : SaclAcePriority(ace2);
            if (num < num2)
            {
                return ComparisonResult.LessThan;
            }
            if (num > num2)
            {
                return ComparisonResult.GreaterThan;
            }
            KnownAce ace = ace1 as KnownAce;
            KnownAce ace3 = ace2 as KnownAce;
            if ((ace != null) && (ace3 != null))
            {
                int num3 = ace.SecurityIdentifier.CompareTo(ace3.SecurityIdentifier);
                if (num3 < 0)
                {
                    return ComparisonResult.LessThan;
                }
                if (num3 > 0)
                {
                    return ComparisonResult.GreaterThan;
                }
            }
            return ComparisonResult.EqualTo;
        }

        private static int DaclAcePriority(GenericAce ace)
        {
            AceType aceType = ace.AceType;
            if (((byte) (ace.AceFlags & AceFlags.Inherited)) != 0)
            {
                return (0x1fffe + ace._indexInAcl);
            }
            switch (aceType)
            {
                case AceType.AccessDenied:
                case AceType.AccessDeniedCallback:
                    return 0;

                case AceType.AccessDeniedObject:
                case AceType.AccessDeniedCallbackObject:
                    return 1;

                case AceType.AccessAllowed:
                case AceType.AccessAllowedCallback:
                    return 2;

                case AceType.AccessAllowedObject:
                case AceType.AccessAllowedCallbackObject:
                    return 3;
            }
            return (0xffff + ace._indexInAcl);
        }

        private bool GetAccessMaskForRemoval(QualifiedAce ace, ObjectAceFlags objectFlags, Guid objectType, ref int accessMask)
        {
            if (((ace.AccessMask & accessMask) & ObjectAce.AccessMaskWithObjectType) != 0)
            {
                if (ace is ObjectAce)
                {
                    ObjectAce ace2 = ace as ObjectAce;
                    if (((objectFlags & ObjectAceFlags.ObjectAceTypePresent) != ObjectAceFlags.None) && ((ace2.ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent) == ObjectAceFlags.None))
                    {
                        return false;
                    }
                    if (!(((objectFlags & ObjectAceFlags.ObjectAceTypePresent) == ObjectAceFlags.None) || ace2.ObjectTypesMatch(objectFlags, objectType)))
                    {
                        accessMask &= ~ObjectAce.AccessMaskWithObjectType;
                    }
                }
                else if ((objectFlags & ObjectAceFlags.ObjectAceTypePresent) != ObjectAceFlags.None)
                {
                    return false;
                }
            }
            return true;
        }

        public sealed override void GetBinaryForm(byte[] binaryForm, int offset)
        {
            this.CanonicalizeIfNecessary();
            this._acl.GetBinaryForm(binaryForm, offset);
        }

        private bool GetInheritanceFlagsForRemoval(QualifiedAce ace, ObjectAceFlags objectFlags, Guid inheritedObjectType, ref AceFlags aceFlags)
        {
            if ((((byte) (ace.AceFlags & AceFlags.ContainerInherit)) != 0) && (((byte) (aceFlags & AceFlags.ContainerInherit)) != 0))
            {
                if (ace is ObjectAce)
                {
                    ObjectAce ace2 = ace as ObjectAce;
                    if (((objectFlags & ObjectAceFlags.InheritedObjectAceTypePresent) != ObjectAceFlags.None) && ((ace2.ObjectAceFlags & ObjectAceFlags.InheritedObjectAceTypePresent) == ObjectAceFlags.None))
                    {
                        return false;
                    }
                    if (!(((objectFlags & ObjectAceFlags.InheritedObjectAceTypePresent) == ObjectAceFlags.None) || ace2.InheritedObjectTypesMatch(objectFlags, inheritedObjectType)))
                    {
                        aceFlags = (AceFlags) ((byte) (((int) aceFlags) & 240));
                    }
                }
                else if ((objectFlags & ObjectAceFlags.InheritedObjectAceTypePresent) != ObjectAceFlags.None)
                {
                    return false;
                }
            }
            return true;
        }

        private void GetObjectTypesForSplit(ObjectAce originalAce, int accessMask, AceFlags aceFlags, out ObjectAceFlags objectFlags, out Guid objectType, out Guid inheritedObjectType)
        {
            objectFlags = ObjectAceFlags.None;
            objectType = Guid.Empty;
            inheritedObjectType = Guid.Empty;
            if ((accessMask & ObjectAce.AccessMaskWithObjectType) != 0)
            {
                objectType = originalAce.ObjectAceType;
                objectFlags |= originalAce.ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent;
            }
            if (((byte) (aceFlags & AceFlags.ContainerInherit)) != 0)
            {
                inheritedObjectType = originalAce.InheritedObjectAceType;
                objectFlags |= originalAce.ObjectAceFlags & ObjectAceFlags.InheritedObjectAceTypePresent;
            }
        }

        private bool InheritedObjectTypesMatch(QualifiedAce ace, QualifiedAce newAce)
        {
            Guid guid = (ace is ObjectAce) ? ((ObjectAce) ace).InheritedObjectAceType : Guid.Empty;
            Guid g = (newAce is ObjectAce) ? ((ObjectAce) newAce).InheritedObjectAceType : Guid.Empty;
            return guid.Equals(g);
        }

        private bool InspectAce(ref GenericAce ace, bool isDacl)
        {
            KnownAce ace2 = ace as KnownAce;
            if ((ace2 != null) && (ace2.AccessMask == 0))
            {
                return false;
            }
            if (!this.IsContainer)
            {
                if (((byte) (ace.AceFlags & AceFlags.InheritOnly)) != 0)
                {
                    return false;
                }
                if (((byte) (ace.AceFlags & (AceFlags.ContainerInherit | AceFlags.InheritOnly | AceFlags.NoPropagateInherit | AceFlags.ObjectInherit))) != 0)
                {
                    ace.AceFlags = (AceFlags) ((byte) (((int) ace.AceFlags) & 240));
                }
            }
            else
            {
                if (((((byte) (ace.AceFlags & AceFlags.InheritOnly)) != 0) && (((byte) (ace.AceFlags & AceFlags.ContainerInherit)) == 0)) && (((byte) (ace.AceFlags & (AceFlags.None | AceFlags.ObjectInherit))) == 0))
                {
                    return false;
                }
                if (((((byte) (ace.AceFlags & (AceFlags.None | AceFlags.NoPropagateInherit))) != 0) && (((byte) (ace.AceFlags & AceFlags.ContainerInherit)) == 0)) && (((byte) (ace.AceFlags & (AceFlags.None | AceFlags.ObjectInherit))) == 0))
                {
                    ace.AceFlags = (AceFlags) ((byte) (((int) ace.AceFlags) & 0xfb));
                }
            }
            QualifiedAce ace3 = ace2 as QualifiedAce;
            if (isDacl)
            {
                ace.AceFlags = (AceFlags) ((byte) (((int) ace.AceFlags) & 0x3f));
                if (((ace3 != null) && (ace3.AceQualifier != AceQualifier.AccessAllowed)) && (ace3.AceQualifier != AceQualifier.AccessDenied))
                {
                    return false;
                }
            }
            else
            {
                if (((byte) (ace.AceFlags & AceFlags.AuditFlags)) == 0)
                {
                    return false;
                }
                if ((ace3 != null) && (ace3.AceQualifier != AceQualifier.SystemAudit))
                {
                    return false;
                }
            }
            return true;
        }

        private bool MergeAces(ref QualifiedAce ace, QualifiedAce newAce)
        {
            if (((byte) (ace.AceFlags & AceFlags.Inherited)) == 0)
            {
                if (((byte) (newAce.AceFlags & AceFlags.Inherited)) != 0)
                {
                    return false;
                }
                if (ace.AceQualifier != newAce.AceQualifier)
                {
                    return false;
                }
                if (ace.SecurityIdentifier != newAce.SecurityIdentifier)
                {
                    return false;
                }
                if (ace.AceFlags == newAce.AceFlags)
                {
                    if (!(ace is ObjectAce) && !(newAce is ObjectAce))
                    {
                        ace.AccessMask |= newAce.AccessMask;
                        return true;
                    }
                    if (this.InheritedObjectTypesMatch(ace, newAce) && this.AccessMasksAreMergeable(ace, newAce))
                    {
                        ace.AccessMask |= newAce.AccessMask;
                        return true;
                    }
                }
                if ((((byte) (ace.AceFlags & (AceFlags.ContainerInherit | AceFlags.InheritOnly | AceFlags.NoPropagateInherit | AceFlags.ObjectInherit))) == ((byte) (newAce.AceFlags & (AceFlags.ContainerInherit | AceFlags.InheritOnly | AceFlags.NoPropagateInherit | AceFlags.ObjectInherit)))) && (ace.AccessMask == newAce.AccessMask))
                {
                    if (!(ace is ObjectAce) && !(newAce is ObjectAce))
                    {
                        ace.AceFlags = (AceFlags) ((byte) (ace.AceFlags | ((byte) (newAce.AceFlags & AceFlags.AuditFlags))));
                        return true;
                    }
                    if (this.InheritedObjectTypesMatch(ace, newAce) && this.ObjectTypesMatch(ace, newAce))
                    {
                        ace.AceFlags = (AceFlags) ((byte) (ace.AceFlags | ((byte) (newAce.AceFlags & AceFlags.AuditFlags))));
                        return true;
                    }
                }
                if ((((byte) (ace.AceFlags & AceFlags.AuditFlags)) == ((byte) (newAce.AceFlags & AceFlags.AuditFlags))) && (ace.AccessMask == newAce.AccessMask))
                {
                    AceFlags flags;
                    if ((ace is ObjectAce) || (newAce is ObjectAce))
                    {
                        if ((this.ObjectTypesMatch(ace, newAce) && this.AceFlagsAreMergeable(ace, newAce)) && MergeInheritanceBits(ace.AceFlags, newAce.AceFlags, this.IsDS, out flags))
                        {
                            ace.AceFlags = (AceFlags) ((byte) (flags | ((byte) (ace.AceFlags & AceFlags.AuditFlags))));
                            return true;
                        }
                    }
                    else if (MergeInheritanceBits(ace.AceFlags, newAce.AceFlags, this.IsDS, out flags))
                    {
                        ace.AceFlags = (AceFlags) ((byte) (flags | ((byte) (ace.AceFlags & AceFlags.AuditFlags))));
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool MergeInheritanceBits(AceFlags left, AceFlags right, bool isDS, out AceFlags result)
        {
            result = AceFlags.None;
            AF af = AFFromAceFlags(left, isDS);
            AF af2 = AFFromAceFlags(right, isDS);
            PM pm = AFtoPM[(int) af];
            PM pm2 = AFtoPM[(int) af2];
            if ((pm == PM.GO) || (pm2 == PM.GO))
            {
                return false;
            }
            PM pm3 = pm | pm2;
            AF af3 = PMtoAF[(int) pm3];
            if (af3 == AF.Invalid)
            {
                return false;
            }
            result = AceFlagsFromAF(af3, isDS);
            return true;
        }

        private bool ObjectTypesMatch(QualifiedAce ace, QualifiedAce newAce)
        {
            Guid guid = (ace is ObjectAce) ? ((ObjectAce) ace).ObjectAceType : Guid.Empty;
            Guid g = (newAce is ObjectAce) ? ((ObjectAce) newAce).ObjectAceType : Guid.Empty;
            return guid.Equals(g);
        }

        internal virtual void OnAclModificationTried()
        {
        }

        public void Purge(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            this.ThrowIfNotCanonical();
            for (int i = this.Count - 1; i >= 0; i--)
            {
                KnownAce ace = this._acl[i] as KnownAce;
                if (((ace != null) && (((byte) (ace.AceFlags & AceFlags.Inherited)) == 0)) && (ace.SecurityIdentifier == sid))
                {
                    this._acl.RemoveAce(i);
                }
            }
            this.OnAclModificationTried();
        }

        private void QuickSort(int left, int right, bool isDacl)
        {
            if (left < right)
            {
                int num = left;
                int num2 = right;
                GenericAce ace = this._acl[left];
                int num3 = left;
                while (left < right)
                {
                    while ((CompareAces(this._acl[right], ace, isDacl) != ComparisonResult.LessThan) && (left < right))
                    {
                        right--;
                    }
                    if (left != right)
                    {
                        this._acl[left] = this._acl[right];
                        left++;
                    }
                    while ((ComparisonResult.GreaterThan != CompareAces(this._acl[left], ace, isDacl)) && (left < right))
                    {
                        left++;
                    }
                    if (left != right)
                    {
                        this._acl[right] = this._acl[left];
                        right--;
                    }
                }
                this._acl[left] = ace;
                num3 = left;
                left = num;
                right = num2;
                if (left < num3)
                {
                    this.QuickSort(left, num3 - 1, isDacl);
                }
                if (right > num3)
                {
                    this.QuickSort(num3 + 1, right, isDacl);
                }
            }
        }

        private static bool RemoveInheritanceBits(AceFlags existing, AceFlags remove, bool isDS, out AceFlags result, out bool total)
        {
            result = AceFlags.None;
            total = false;
            AF af = AFFromAceFlags(existing, isDS);
            AF af2 = AFFromAceFlags(remove, isDS);
            PM pm = AFtoPM[(int) af];
            PM pm2 = AFtoPM[(int) af2];
            if ((pm == PM.GO) || (pm2 == PM.GO))
            {
                return false;
            }
            PM pm3 = pm & ~pm2;
            if (pm3 == 0)
            {
                total = true;
                return true;
            }
            AF af3 = PMtoAF[(int) pm3];
            if (af3 == AF.Invalid)
            {
                return false;
            }
            result = AceFlagsFromAF(af3, isDS);
            return true;
        }

        public void RemoveInheritedAces()
        {
            this.ThrowIfNotCanonical();
            for (int i = this._acl.Count - 1; i >= 0; i--)
            {
                GenericAce ace = this._acl[i];
                if (((byte) (ace.AceFlags & AceFlags.Inherited)) != 0)
                {
                    this._acl.RemoveAce(i);
                }
            }
            this.OnAclModificationTried();
        }

        private void RemoveMeaninglessAcesAndFlags(bool isDacl)
        {
            for (int i = this._acl.Count - 1; i >= 0; i--)
            {
                GenericAce ace = this._acl[i];
                if (!this.InspectAce(ref ace, isDacl))
                {
                    this._acl.RemoveAce(i);
                }
            }
        }

        internal bool RemoveQualifiedAces(SecurityIdentifier sid, AceQualifier qualifier, int accessMask, AceFlags flags, bool saclSemantics, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (accessMask == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "accessMask");
            }
            if ((qualifier == AceQualifier.SystemAudit) && (((byte) (flags & AceFlags.AuditFlags)) == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "flags");
            }
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            this.ThrowIfNotCanonical();
            bool flag = true;
            bool flag2 = true;
            int num = accessMask;
            AceFlags flags2 = flags;
            byte[] binaryForm = new byte[this.BinaryLength];
            this.GetBinaryForm(binaryForm, 0);
        Label_0075:
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    QualifiedAce ace = this._acl[i] as QualifiedAce;
                    if (((ace == null) || (((byte) (ace.AceFlags & AceFlags.Inherited)) != 0)) || ((ace.AceQualifier != qualifier) || (ace.SecurityIdentifier != sid)))
                    {
                        continue;
                    }
                    if (this.IsDS)
                    {
                        accessMask = num;
                        bool flag3 = !this.GetAccessMaskForRemoval(ace, objectFlags, objectType, ref accessMask);
                        if ((ace.AccessMask & accessMask) == 0)
                        {
                            continue;
                        }
                        flags = flags2;
                        bool flag4 = !this.GetInheritanceFlagsForRemoval(ace, objectFlags, inheritedObjectType, ref flags);
                        if (((((((byte) (ace.AceFlags & AceFlags.ContainerInherit)) == 0) && (((byte) (flags & AceFlags.ContainerInherit)) != 0)) && (((byte) (flags & AceFlags.InheritOnly)) != 0)) || (((((byte) (flags & AceFlags.ContainerInherit)) == 0) && (((byte) (ace.AceFlags & AceFlags.ContainerInherit)) != 0)) && (((byte) (ace.AceFlags & AceFlags.InheritOnly)) != 0))) || (((((byte) (flags2 & AceFlags.ContainerInherit)) != 0) && (((byte) (flags2 & AceFlags.InheritOnly)) != 0)) && (((byte) (flags & AceFlags.ContainerInherit)) == 0)))
                        {
                            continue;
                        }
                        if (!flag3 && !flag4)
                        {
                            goto Label_0184;
                        }
                        flag2 = false;
                        goto Label_0483;
                    }
                    if ((ace.AccessMask & accessMask) == 0)
                    {
                        continue;
                    }
                Label_0184:
                    if (!saclSemantics || (((byte) (((byte) (ace.AceFlags & flags)) & 0xc0)) != 0))
                    {
                        AceFlags none = AceFlags.None;
                        int num3 = 0;
                        ObjectAceFlags flags4 = ObjectAceFlags.None;
                        Guid empty = Guid.Empty;
                        Guid guid2 = Guid.Empty;
                        AceFlags aceFlags = AceFlags.None;
                        int num4 = 0;
                        ObjectAceFlags flags6 = ObjectAceFlags.None;
                        Guid guid3 = Guid.Empty;
                        Guid guid4 = Guid.Empty;
                        AceFlags existing = AceFlags.None;
                        int num5 = 0;
                        ObjectAceFlags flags8 = ObjectAceFlags.None;
                        Guid guid5 = Guid.Empty;
                        Guid guid6 = Guid.Empty;
                        AceFlags result = AceFlags.None;
                        bool total = false;
                        none = ace.AceFlags;
                        num3 = ace.AccessMask & ~accessMask;
                        if (ace is ObjectAce)
                        {
                            this.GetObjectTypesForSplit(ace as ObjectAce, num3, none, out flags4, out empty, out guid2);
                        }
                        if (saclSemantics)
                        {
                            aceFlags = (AceFlags) ((byte) (ace.AceFlags & ~((byte) (flags & AceFlags.AuditFlags))));
                            num4 = ace.AccessMask & accessMask;
                            if (ace is ObjectAce)
                            {
                                this.GetObjectTypesForSplit(ace as ObjectAce, num4, aceFlags, out flags6, out guid3, out guid4);
                            }
                        }
                        existing = (AceFlags) ((byte) (((byte) (ace.AceFlags & (AceFlags.ContainerInherit | AceFlags.InheritOnly | AceFlags.NoPropagateInherit | AceFlags.ObjectInherit))) | ((byte) (((byte) (flags & ace.AceFlags)) & 0xc0))));
                        num5 = ace.AccessMask & accessMask;
                        if (!saclSemantics || (((byte) (existing & AceFlags.AuditFlags)) != 0))
                        {
                            if (!RemoveInheritanceBits(existing, flags, this.IsDS, out result, out total))
                            {
                                flag2 = false;
                                goto Label_0483;
                            }
                            if (!total)
                            {
                                result = (AceFlags) ((byte) (result | ((byte) (existing & AceFlags.AuditFlags))));
                                if (ace is ObjectAce)
                                {
                                    this.GetObjectTypesForSplit(ace as ObjectAce, num5, result, out flags8, out guid5, out guid6);
                                }
                            }
                        }
                        if (!flag)
                        {
                            QualifiedAce ace2;
                            if (num3 != 0)
                            {
                                if (((ace is ObjectAce) && ((((ObjectAce) ace).ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent) != ObjectAceFlags.None)) && ((flags4 & ObjectAceFlags.ObjectAceTypePresent) == ObjectAceFlags.None))
                                {
                                    this._acl.RemoveAce(i);
                                    ObjectAce ace3 = new ObjectAce(none, qualifier, num3, ace.SecurityIdentifier, flags4, empty, guid2, false, null);
                                    this._acl.InsertAce(i, ace3);
                                }
                                else
                                {
                                    ace.AceFlags = none;
                                    ace.AccessMask = num3;
                                    if (ace is ObjectAce)
                                    {
                                        ObjectAce ace4 = ace as ObjectAce;
                                        ace4.ObjectAceFlags = flags4;
                                        ace4.ObjectAceType = empty;
                                        ace4.InheritedObjectAceType = guid2;
                                    }
                                }
                            }
                            else
                            {
                                this._acl.RemoveAce(i);
                                i--;
                            }
                            if (saclSemantics && (((byte) (aceFlags & AceFlags.AuditFlags)) != 0))
                            {
                                if (ace is CommonAce)
                                {
                                    ace2 = new CommonAce(aceFlags, qualifier, num4, ace.SecurityIdentifier, false, null);
                                }
                                else
                                {
                                    ace2 = new ObjectAce(aceFlags, qualifier, num4, ace.SecurityIdentifier, flags6, guid3, guid4, false, null);
                                }
                                i++;
                                this._acl.InsertAce(i, ace2);
                            }
                            if (!total)
                            {
                                if (ace is CommonAce)
                                {
                                    ace2 = new CommonAce(result, qualifier, num5, ace.SecurityIdentifier, false, null);
                                }
                                else
                                {
                                    ace2 = new ObjectAce(result, qualifier, num5, ace.SecurityIdentifier, flags8, guid5, guid6, false, null);
                                }
                                i++;
                                this._acl.InsertAce(i, ace2);
                            }
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                this._acl.SetBinaryForm(binaryForm, 0);
                return false;
            }
        Label_0483:
            if (flag && flag2)
            {
                flag = false;
                goto Label_0075;
            }
            this.OnAclModificationTried();
            return flag2;
        }

        internal void RemoveQualifiedAcesSpecific(SecurityIdentifier sid, AceQualifier qualifier, int accessMask, AceFlags flags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (accessMask == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "accessMask");
            }
            if ((qualifier == AceQualifier.SystemAudit) && (((byte) (flags & AceFlags.AuditFlags)) == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "flags");
            }
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            this.ThrowIfNotCanonical();
            for (int i = 0; i < this.Count; i++)
            {
                QualifiedAce ace = this._acl[i] as QualifiedAce;
                if ((((ace == null) || (((byte) (ace.AceFlags & AceFlags.Inherited)) != 0)) || ((ace.AceQualifier != qualifier) || (ace.SecurityIdentifier != sid))) || ((ace.AceFlags != flags) || (ace.AccessMask != accessMask)))
                {
                    continue;
                }
                if (this.IsDS)
                {
                    if ((ace is ObjectAce) && (objectFlags != ObjectAceFlags.None))
                    {
                        ObjectAce ace2 = ace as ObjectAce;
                        if (ace2.ObjectTypesMatch(objectFlags, objectType) && ace2.InheritedObjectTypesMatch(objectFlags, inheritedObjectType))
                        {
                            goto Label_00F2;
                        }
                        continue;
                    }
                    if ((ace is ObjectAce) || (objectFlags != ObjectAceFlags.None))
                    {
                        continue;
                    }
                }
            Label_00F2:
                this._acl.RemoveAce(i);
                i--;
            }
            this.OnAclModificationTried();
        }

        private static int SaclAcePriority(GenericAce ace)
        {
            AceType aceType = ace.AceType;
            if (((byte) (ace.AceFlags & AceFlags.Inherited)) != 0)
            {
                return (0x1fffe + ace._indexInAcl);
            }
            switch (aceType)
            {
                case AceType.SystemAudit:
                case AceType.SystemAlarm:
                case AceType.SystemAuditCallback:
                case AceType.SystemAlarmCallback:
                    return 0;

                case AceType.SystemAuditObject:
                case AceType.SystemAlarmObject:
                case AceType.SystemAuditCallbackObject:
                case AceType.SystemAlarmCallbackObject:
                    return 1;
            }
            return (0xffff + ace._indexInAcl);
        }

        internal void SetQualifiedAce(SecurityIdentifier sid, AceQualifier qualifier, int accessMask, AceFlags flags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            GenericAce ace;
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            if ((qualifier == AceQualifier.SystemAudit) && (((byte) (flags & AceFlags.AuditFlags)) == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "flags");
            }
            if (accessMask == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "accessMask");
            }
            this.ThrowIfNotCanonical();
            if (!this.IsDS || (objectFlags == ObjectAceFlags.None))
            {
                ace = new CommonAce(flags, qualifier, accessMask, sid, false, null);
            }
            else
            {
                ace = new ObjectAce(flags, qualifier, accessMask, sid, objectFlags, objectType, inheritedObjectType, false, null);
            }
            if (this.InspectAce(ref ace, this is DiscretionaryAcl))
            {
                for (int i = 0; i < this.Count; i++)
                {
                    QualifiedAce ace2 = this._acl[i] as QualifiedAce;
                    if (((ace2 != null) && (((byte) (ace2.AceFlags & AceFlags.Inherited)) == 0)) && ((ace2.AceQualifier == qualifier) && !(ace2.SecurityIdentifier != sid)))
                    {
                        this._acl.RemoveAce(i);
                        i--;
                    }
                }
                this._acl.InsertAce(this._acl.Count, ace);
                this._isDirty = true;
                this.OnAclModificationTried();
            }
        }

        private void ThrowIfNotCanonical()
        {
            if (!this._isCanonical)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModificationOfNonCanonicalAcl"));
            }
        }

        public sealed override int BinaryLength
        {
            get
            {
                this.CanonicalizeIfNecessary();
                return this._acl.BinaryLength;
            }
        }

        public sealed override int Count
        {
            get
            {
                this.CanonicalizeIfNecessary();
                return this._acl.Count;
            }
        }

        public bool IsCanonical
        {
            get
            {
                return this._isCanonical;
            }
        }

        public bool IsContainer
        {
            get
            {
                return this._isContainer;
            }
        }

        public bool IsDS
        {
            get
            {
                return this._isDS;
            }
        }

        public sealed override GenericAce this[int index]
        {
            get
            {
                this.CanonicalizeIfNecessary();
                return this._acl[index].Copy();
            }
            set
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SetMethod"));
            }
        }

        internal System.Security.AccessControl.RawAcl RawAcl
        {
            get
            {
                return this._acl;
            }
        }

        public sealed override byte Revision
        {
            get
            {
                return this._acl.Revision;
            }
        }

        [Flags]
        private enum AF
        {
            CI = 8,
            Invalid = 1,
            IO = 2,
            NP = 1,
            OI = 4
        }

        private enum ComparisonResult
        {
            LessThan,
            EqualTo,
            GreaterThan
        }

        [Flags]
        private enum PM
        {
            CF = 8,
            CO = 4,
            F = 0x10,
            GF = 2,
            GO = 1,
            Invalid = 1
        }
    }
}

