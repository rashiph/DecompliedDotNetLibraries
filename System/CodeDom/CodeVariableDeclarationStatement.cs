namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeVariableDeclarationStatement : CodeStatement
    {
        private CodeExpression initExpression;
        private string name;
        private CodeTypeReference type;

        public CodeVariableDeclarationStatement()
        {
        }

        public CodeVariableDeclarationStatement(CodeTypeReference type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        public CodeVariableDeclarationStatement(string type, string name)
        {
            this.Type = new CodeTypeReference(type);
            this.Name = name;
        }

        public CodeVariableDeclarationStatement(System.Type type, string name)
        {
            this.Type = new CodeTypeReference(type);
            this.Name = name;
        }

        public CodeVariableDeclarationStatement(CodeTypeReference type, string name, CodeExpression initExpression)
        {
            this.Type = type;
            this.Name = name;
            this.InitExpression = initExpression;
        }

        public CodeVariableDeclarationStatement(string type, string name, CodeExpression initExpression)
        {
            this.Type = new CodeTypeReference(type);
            this.Name = name;
            this.InitExpression = initExpression;
        }

        public CodeVariableDeclarationStatement(System.Type type, string name, CodeExpression initExpression)
        {
            this.Type = new CodeTypeReference(type);
            this.Name = name;
            this.InitExpression = initExpression;
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

