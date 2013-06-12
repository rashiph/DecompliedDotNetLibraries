namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeMember : CodeObject
    {
        private MemberAttributes attributes = (MemberAttributes.Private | MemberAttributes.Final);
        private CodeCommentStatementCollection comments = new CodeCommentStatementCollection();
        private CodeAttributeDeclarationCollection customAttributes;
        [OptionalField]
        private CodeDirectiveCollection endDirectives;
        private CodeLinePragma linePragma;
        private string name;
        [OptionalField]
        private CodeDirectiveCollection startDirectives;

        public MemberAttributes Attributes
        {
            get
            {
                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        public CodeCommentStatementCollection Comments
        {
            get
            {
                return this.comments;
            }
        }

        public CodeAttributeDeclarationCollection CustomAttributes
        {
            get
            {
                if (this.customAttributes == null)
                {
                    this.customAttributes = new CodeAttributeDeclarationCollection();
                }
                return this.customAttributes;
            }
            set
            {
                this.customAttributes = value;
            }
        }

        public CodeDirectiveCollection EndDirectives
        {
            get
            {
                if (this.endDirectives == null)
                {
                    this.endDirectives = new CodeDirectiveCollection();
                }
                return this.endDirectives;
            }
        }

        public CodeLinePragma LinePragma
        {
            get
            {
                return this.linePragma;
            }
            set
            {
                this.linePragma = value;
            }
        }

        public string Name
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

        public CodeDirectiveCollection StartDirectives
        {
            get
            {
                if (this.startDirectives == null)
                {
                    this.startDirectives = new CodeDirectiveCollection();
                }
                return this.startDirectives;
            }
        }
    }
}

