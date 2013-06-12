namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeMemberField : CodeTypeMember
    {
        private CodeExpression initExpression;
        private CodeTypeReference type;

        public CodeMemberField()
        {
        }

        public CodeMemberField(CodeTypeReference type, string name)
        {
            this.Type = type;
            base.Name = name;
        }

        public CodeMemberField(string type, string name)
        {
            this.Type = new CodeTypeReference(type);
            base.Name = name;
        }

        public CodeMemberField(System.Type type, string name)
        {
            this.Type = new CodeTypeReference(type);
            base.Name = name;
        }

        public CodeExpression InitExpression
        {
            get
            {
                return this.initExpression;
            }
            set
            {
                this.initExpression = value;
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

