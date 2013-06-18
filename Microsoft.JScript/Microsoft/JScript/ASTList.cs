namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class ASTList : AST
    {
        private object[] array;
        internal int count;
        private AST[] list;

        internal ASTList(Context context) : base(context)
        {
            this.count = 0;
            this.list = new AST[0x10];
            this.array = null;
        }

        internal ASTList Append(AST elem)
        {
            int index = this.count++;
            if (this.list.Length == index)
            {
                this.Grow();
            }
            this.list[index] = elem;
            base.context.UpdateWith(elem.context);
            return this;
        }

        internal override object Evaluate()
        {
            return this.EvaluateAsArray();
        }

        internal object[] EvaluateAsArray()
        {
            int count = this.count;
            object[] array = this.array;
            if (array == null)
            {
                this.array = array = new object[count];
            }
            AST[] list = this.list;
            for (int i = 0; i < count; i++)
            {
                array[i] = list[i].Evaluate();
            }
            return array;
        }

        private void Grow()
        {
            AST[] list = this.list;
            int length = list.Length;
            AST[] astArray2 = this.list = new AST[length + 0x10];
            for (int i = 0; i < length; i++)
            {
                astArray2[i] = list[i];
            }
        }

        internal override AST PartiallyEvaluate()
        {
            AST[] list = this.list;
            int index = 0;
            int count = this.count;
            while (index < count)
            {
                list[index] = list[index].PartiallyEvaluate();
                index++;
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Type elementType = rtype.GetElementType();
            int count = this.count;
            ConstantWrapper.TranslateToILInt(il, count);
            il.Emit(OpCodes.Newarr, elementType);
            bool flag = elementType.IsValueType && !elementType.IsPrimitive;
            AST[] list = this.list;
            for (int i = 0; i < count; i++)
            {
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, i);
                list[i].TranslateToIL(il, elementType);
                if (flag)
                {
                    il.Emit(OpCodes.Ldelema, elementType);
                }
                Binding.TranslateToStelem(il, elementType);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            AST[] list = this.list;
            int index = 0;
            int count = this.count;
            while (index < count)
            {
                list[index].TranslateToILInitializer(il);
                index++;
            }
        }

        internal AST this[int i]
        {
            get
            {
                return this.list[i];
            }
            set
            {
                this.list[i] = value;
            }
        }
    }
}

