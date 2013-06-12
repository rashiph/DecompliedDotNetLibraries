namespace System.CodeDom
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeDelegate : CodeTypeDeclaration
    {
        private CodeParameterDeclarationExpressionCollection parameters;
        private CodeTypeReference returnType;

        public CodeTypeDelegate()
        {
            this.parameters = new CodeParameterDeclarationExpressionCollection();
            base.TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
            base.TypeAttributes = base.TypeAttributes;
            base.BaseTypes.Clear();
            base.BaseTypes.Add(new CodeTypeReference("System.Delegate"));
        }

        public CodeTypeDelegate(string name) : this()
        {
            base.Name = name;
        }

        public CodeParameterDeclarationExpressionCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public CodeTypeReference ReturnType
        {
            get
            {
                if (this.returnType == null)
                {
                    this.returnType = new CodeTypeReference("");
                }
                return this.returnType;
            }
            set
            {
                this.returnType = value;
            }
        }
    }
}

