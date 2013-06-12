namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeArrayCreateExpression : CodeExpression
    {
        private CodeTypeReference createType;
        private CodeExpressionCollection initializers;
        private int size;
        private CodeExpression sizeExpression;

        public CodeArrayCreateExpression()
        {
            this.initializers = new CodeExpressionCollection();
        }

        public CodeArrayCreateExpression(CodeTypeReference createType, params CodeExpression[] initializers)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = createType;
            this.initializers.AddRange(initializers);
        }

        public CodeArrayCreateExpression(CodeTypeReference createType, CodeExpression size)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = createType;
            this.sizeExpression = size;
        }

        public CodeArrayCreateExpression(CodeTypeReference createType, int size)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = createType;
            this.size = size;
        }

        public CodeArrayCreateExpression(string createType, params CodeExpression[] initializers)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = new CodeTypeReference(createType);
            this.initializers.AddRange(initializers);
        }

        public CodeArrayCreateExpression(string createType, CodeExpression size)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = new CodeTypeReference(createType);
            this.sizeExpression = size;
        }

        public CodeArrayCreateExpression(string createType, int size)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = new CodeTypeReference(createType);
            this.size = size;
        }

        public CodeArrayCreateExpression(Type createType, params CodeExpression[] initializers)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = new CodeTypeReference(createType);
            this.initializers.AddRange(initializers);
        }

        public CodeArrayCreateExpression(Type createType, CodeExpression size)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = new CodeTypeReference(createType);
            this.sizeExpression = size;
        }

        public CodeArrayCreateExpression(Type createType, int size)
        {
            this.initializers = new CodeExpressionCollection();
            this.createType = new CodeTypeReference(createType);
            this.size = size;
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

        public CodeExpressionCollection Initializers
        {
            get
            {
                return this.initializers;
            }
        }

        public int Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }

        public CodeExpression SizeExpression
        {
            get
            {
                return this.sizeExpression;
            }
            set
            {
                this.sizeExpression = value;
            }
        }
    }
}

