namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal sealed class ObjectLiteral : AST
    {
        internal AST[] keys;
        internal AST[] values;

        internal ObjectLiteral(Context context, ASTList propertyList) : base(context)
        {
            int count = propertyList.count;
            this.keys = new AST[count];
            this.values = new AST[count];
            for (int i = 0; i < count; i++)
            {
                ASTList list = (ASTList) propertyList[i];
                this.keys[i] = list[0];
                this.values[i] = list[1];
            }
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            int index = 0;
            int length = this.values.Length;
            while (index < length)
            {
                this.values[index].CheckIfOKToUseInSuperConstructorCall();
                index++;
            }
        }

        internal override object Evaluate()
        {
            JSObject obj2 = base.Engine.GetOriginalObjectConstructor().ConstructObject();
            int index = 0;
            int length = this.keys.Length;
            while (index < length)
            {
                obj2.SetMemberValue(this.keys[index].Evaluate().ToString(), this.values[index].Evaluate());
                index++;
            }
            return obj2;
        }

        internal override AST PartiallyEvaluate()
        {
            int length = this.keys.Length;
            for (int i = 0; i < length; i++)
            {
                this.keys[i] = this.keys[i].PartiallyEvaluate();
                this.values[i] = this.values[i].PartiallyEvaluate();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            int length = this.keys.Length;
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.getOriginalObjectConstructorMethod);
            il.Emit(OpCodes.Call, CompilerGlobals.constructObjectMethod);
            for (int i = 0; i < length; i++)
            {
                il.Emit(OpCodes.Dup);
                this.keys[i].TranslateToIL(il, Typeob.String);
                this.values[i].TranslateToIL(il, Typeob.Object);
                il.Emit(OpCodes.Call, CompilerGlobals.setMemberValue2Method);
            }
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            int index = 0;
            int length = this.keys.Length;
            while (index < length)
            {
                this.keys[index].TranslateToILInitializer(il);
                this.values[index].TranslateToILInitializer(il);
                index++;
            }
        }
    }
}

