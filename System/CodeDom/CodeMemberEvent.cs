namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeMemberEvent : CodeTypeMember
    {
        private CodeTypeReferenceCollection implementationTypes;
        private CodeTypeReference privateImplements;
        private CodeTypeReference type;

        public CodeTypeReferenceCollection ImplementationTypes
        {
            get
            {
                if (this.implementationTypes == null)
                {
                    this.implementationTypes = new CodeTypeReferenceCollection();
                }
                return this.implementationTypes;
            }
        }

        public CodeTypeReference PrivateImplementationType
        {
            get
            {
                return this.privateImplements;
            }
            set
            {
                this.privateImplements = value;
            }
        }

        public CodeTypeReference Type
        {
            get
            {
                if (this.type == null)
                {
                    this.type = new CodeTypeReference("");
                }
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

