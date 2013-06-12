namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeParameterDeclarationExpressionCollection : CollectionBase
    {
        public CodeParameterDeclarationExpressionCollection()
        {
        }

        public CodeParameterDeclarationExpressionCollection(CodeParameterDeclarationExpressionCollection value)
        {
            this.AddRange(value);
        }

        public CodeParameterDeclarationExpressionCollection(CodeParameterDeclarationExpression[] value)
        {
            this.AddRange(value);
        }

        public int Add(CodeParameterDeclarationExpression value)
        {
            return base.List.Add(value);
        }

        public void AddRange(CodeParameterDeclarationExpression[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(CodeParameterDeclarationExpressionCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(CodeParameterDeclarationExpression value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CodeParameterDeclarationExpression[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CodeParameterDeclarationExpression value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CodeParameterDeclarationExpression value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(CodeParameterDeclarationExpression value)
        {
            base.List.Remove(value);
        }

        public CodeParameterDeclarationExpression this[int index]
        {
            get
            {
                return (CodeParameterDeclarationExpression) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

