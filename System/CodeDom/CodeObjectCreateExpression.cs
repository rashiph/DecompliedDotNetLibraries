namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeObjectCreateExpression : CodeExpression
    {
        private CodeTypeReference createType;
        private CodeExpressionCollection parameters;

        public CodeObjectCreateExpression()
        {
            this.parameters = new CodeExpressionCollection();
        }

        public CodeObjectCreateExpression(CodeTypeReference createType, params CodeExpression[] parameters)
        {
            this.parameters = new CodeExpressionCollection();
            this.CreateType = createType;
            this.Parameters.AddRange(parameters);
        }

        public CodeObjectCreateExpression(string createType, params CodeExpression[] parameters)
        {
            this.parameters = new CodeExpressionCollection();
            this.CreateType = new CodeTypeReference(createType);
            this.Parameters.AddRange(parameters);
        }

        public CodeObjectCreateExpression(Type createType, params CodeExpression[] parameters)
        {
            this.parameters = new CodeExpressionCollection();
            this.CreateType = new CodeTypeReference(createType);
            this.Parameters.AddRange(parameters);
        }

        public CodeTypeReference CreateType
        {
            get
            {
                if (this.createType == null)
                {
                    this.createType = new CodeTypeReference("");
                }
                return this.createType;
            }
            set
            {
                this.createType = value;
            }
        }

        public CodeExpressionCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }
    }
}

