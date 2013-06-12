namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom.Compiler;

    internal class MemberMapping : AccessorMapping
    {
        private bool checkShouldPersist;
        private SpecifiedAccessor checkSpecified;
        private bool isReturnValue;
        private string name;
        private bool readOnly;
        private int sequenceId = -1;

        private string GetNullableType(TypeDesc td)
        {
            if (td.IsMappedType || (!td.IsValueType && (base.Elements[0].IsSoap || (td.ArrayElementTypeDesc == null))))
            {
                return td.FullName;
            }
            if (td.ArrayElementTypeDesc != null)
            {
                return (this.GetNullableType(td.ArrayElementTypeDesc) + "[]");
            }
            return ("System.Nullable`1[" + td.FullName + "]");
        }

        internal string GetTypeName(CodeDomProvider codeProvider)
        {
            if (base.IsNeedNullable && codeProvider.Supports(GeneratorSupport.GenericTypeReference))
            {
                return this.GetNullableType(base.TypeDesc);
            }
            return base.TypeDesc.FullName;
        }

        internal bool CheckShouldPersist
        {
            get
            {
                return this.checkShouldPersist;
            }
            set
            {
                this.checkShouldPersist = value;
            }
        }

        internal SpecifiedAccessor CheckSpecified
        {
            get
            {
                return this.checkSpecified;
            }
            set
            {
                this.checkSpecified = value;
            }
        }

        internal bool IsReturnValue
        {
            get
            {
                return this.isReturnValue;
            }
            set
            {
                this.isReturnValue = value;
            }
        }

        internal bool IsSequence
        {
            get
            {
                return (this.sequenceId >= 0);
            }
        }

        internal string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }

        internal bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }
            set
            {
                this.readOnly = value;
            }
        }

        internal int SequenceId
        {
            get
            {
                return this.sequenceId;
            }
            set
            {
                this.sequenceId = value;
            }
        }
    }
}

