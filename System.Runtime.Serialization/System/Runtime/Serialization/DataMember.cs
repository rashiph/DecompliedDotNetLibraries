namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security;

    internal class DataMember
    {
        [SecurityCritical]
        private CriticalHelper helper;

        [SecuritySafeCritical]
        internal DataMember()
        {
            this.helper = new CriticalHelper();
        }

        [SecuritySafeCritical]
        internal DataMember(System.Reflection.MemberInfo memberInfo)
        {
            this.helper = new CriticalHelper(memberInfo);
        }

        [SecuritySafeCritical]
        internal DataMember(string name)
        {
            this.helper = new CriticalHelper(name);
        }

        [SecuritySafeCritical]
        internal DataMember(DataContract memberTypeContract, string name, bool isNullable, bool isRequired, bool emitDefaultValue, int order)
        {
            this.helper = new CriticalHelper(memberTypeContract, name, isNullable, isRequired, emitDefaultValue, order);
        }

        internal DataMember BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            DataContract memberTypeContract = this.MemberTypeContract.BindGenericParameters(paramContracts, boundContracts);
            return new DataMember(memberTypeContract, this.Name, !memberTypeContract.IsValueType, this.IsRequired, this.EmitDefaultValue, this.Order);
        }

        internal bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (this == other)
            {
                return true;
            }
            DataMember member = other as DataMember;
            if (member == null)
            {
                return false;
            }
            bool flag = (this.MemberTypeContract != null) && !this.MemberTypeContract.IsValueType;
            bool flag2 = (member.MemberTypeContract != null) && !member.MemberTypeContract.IsValueType;
            return ((((this.Name == member.Name) && ((this.IsNullable || flag) == (member.IsNullable || flag2))) && ((this.IsRequired == member.IsRequired) && (this.EmitDefaultValue == member.EmitDefaultValue))) && this.MemberTypeContract.Equals(member.MemberTypeContract, checkedContracts));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal bool RequiresMemberAccessForGet()
        {
            System.Reflection.MemberInfo memberInfo = this.MemberInfo;
            FieldInfo field = memberInfo as FieldInfo;
            if (field != null)
            {
                return DataContract.FieldRequiresMemberAccess(field);
            }
            PropertyInfo info3 = (PropertyInfo) memberInfo;
            MethodInfo getMethod = info3.GetGetMethod(true);
            if (getMethod == null)
            {
                return false;
            }
            if (!DataContract.MethodRequiresMemberAccess(getMethod))
            {
                return !DataContract.IsTypeVisible(info3.PropertyType);
            }
            return true;
        }

        internal bool RequiresMemberAccessForSet()
        {
            System.Reflection.MemberInfo memberInfo = this.MemberInfo;
            FieldInfo field = memberInfo as FieldInfo;
            if (field != null)
            {
                return DataContract.FieldRequiresMemberAccess(field);
            }
            PropertyInfo info3 = (PropertyInfo) memberInfo;
            MethodInfo setMethod = info3.GetSetMethod(true);
            if (setMethod == null)
            {
                return false;
            }
            if (!DataContract.MethodRequiresMemberAccess(setMethod))
            {
                return !DataContract.IsTypeVisible(info3.PropertyType);
            }
            return true;
        }

        internal DataMember ConflictingMember
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ConflictingMember;
            }
            [SecurityCritical]
            set
            {
                this.helper.ConflictingMember = value;
            }
        }

        internal bool EmitDefaultValue
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.EmitDefaultValue;
            }
            [SecurityCritical]
            set
            {
                this.helper.EmitDefaultValue = value;
            }
        }

        internal bool HasConflictingNameAndType
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.HasConflictingNameAndType;
            }
            [SecurityCritical]
            set
            {
                this.helper.HasConflictingNameAndType = value;
            }
        }

        internal bool IsGetOnlyCollection
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsGetOnlyCollection;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsGetOnlyCollection = value;
            }
        }

        internal bool IsNullable
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsNullable;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsNullable = value;
            }
        }

        internal bool IsRequired
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsRequired;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsRequired = value;
            }
        }

        internal System.Reflection.MemberInfo MemberInfo
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.MemberInfo;
            }
        }

        internal Type MemberType
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.MemberType;
            }
        }

        internal DataContract MemberTypeContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.MemberTypeContract;
            }
            [SecurityCritical]
            set
            {
                this.helper.MemberTypeContract = value;
            }
        }

        internal string Name
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.Name;
            }
            [SecurityCritical]
            set
            {
                this.helper.Name = value;
            }
        }

        internal int Order
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.Order;
            }
            [SecurityCritical]
            set
            {
                this.helper.Order = value;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class CriticalHelper
        {
            private DataMember conflictingMember;
            private bool emitDefaultValue;
            private bool hasConflictingNameAndType;
            private bool isGetOnlyCollection;
            private bool isNullable;
            private bool isRequired;
            private System.Reflection.MemberInfo memberInfo;
            private DataContract memberTypeContract;
            private string name;
            private int order;

            internal CriticalHelper()
            {
                this.emitDefaultValue = true;
            }

            internal CriticalHelper(System.Reflection.MemberInfo memberInfo)
            {
                this.emitDefaultValue = true;
                this.memberInfo = memberInfo;
            }

            internal CriticalHelper(string name)
            {
                this.Name = name;
            }

            internal CriticalHelper(DataContract memberTypeContract, string name, bool isNullable, bool isRequired, bool emitDefaultValue, int order)
            {
                this.MemberTypeContract = memberTypeContract;
                this.Name = name;
                this.IsNullable = isNullable;
                this.IsRequired = isRequired;
                this.EmitDefaultValue = emitDefaultValue;
                this.Order = order;
            }

            internal DataMember ConflictingMember
            {
                get
                {
                    return this.conflictingMember;
                }
                set
                {
                    this.conflictingMember = value;
                }
            }

            internal bool EmitDefaultValue
            {
                get
                {
                    return this.emitDefaultValue;
                }
                set
                {
                    this.emitDefaultValue = value;
                }
            }

            internal bool HasConflictingNameAndType
            {
                get
                {
                    return this.hasConflictingNameAndType;
                }
                set
                {
                    this.hasConflictingNameAndType = value;
                }
            }

            internal bool IsGetOnlyCollection
            {
                get
                {
                    return this.isGetOnlyCollection;
                }
                set
                {
                    this.isGetOnlyCollection = value;
                }
            }

            internal bool IsNullable
            {
                get
                {
                    return this.isNullable;
                }
                set
                {
                    this.isNullable = value;
                }
            }

            internal bool IsRequired
            {
                get
                {
                    return this.isRequired;
                }
                set
                {
                    this.isRequired = value;
                }
            }

            internal System.Reflection.MemberInfo MemberInfo
            {
                get
                {
                    return this.memberInfo;
                }
            }

            internal Type MemberType
            {
                get
                {
                    FieldInfo memberInfo = this.MemberInfo as FieldInfo;
                    if (memberInfo != null)
                    {
                        return memberInfo.FieldType;
                    }
                    return ((PropertyInfo) this.MemberInfo).PropertyType;
                }
            }

            internal DataContract MemberTypeContract
            {
                get
                {
                    if ((this.memberTypeContract == null) && (this.MemberInfo != null))
                    {
                        if (this.IsGetOnlyCollection)
                        {
                            this.memberTypeContract = DataContract.GetGetOnlyCollectionDataContract(DataContract.GetId(this.MemberType.TypeHandle), this.MemberType.TypeHandle, this.MemberType, SerializationMode.SharedContract);
                        }
                        else
                        {
                            this.memberTypeContract = DataContract.GetDataContract(this.MemberType);
                        }
                    }
                    return this.memberTypeContract;
                }
                set
                {
                    this.memberTypeContract = value;
                }
            }

            internal string Name
            {
                get
                {
                    return this.name;
                }
                set
                {
                    this.name = value;
                }
            }

            internal int Order
            {
                get
                {
                    return this.order;
                }
                set
                {
                    this.order = value;
                }
            }
        }
    }
}

