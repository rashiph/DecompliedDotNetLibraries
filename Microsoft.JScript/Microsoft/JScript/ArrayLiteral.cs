namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class ArrayLiteral : AST
    {
        internal ASTList elements;

        public ArrayLiteral(Context context, ASTList elements) : base(context)
        {
            this.elements = elements;
        }

        internal bool AssignmentCompatible(IReflect lhir, bool reportError)
        {
            if (((lhir != Typeob.Object) && (lhir != Typeob.Array)) && !(lhir is ArrayObject))
            {
                IReflect elementType;
                if (lhir == Typeob.Array)
                {
                    elementType = Typeob.Object;
                }
                else if (lhir is TypedArray)
                {
                    TypedArray array = (TypedArray) lhir;
                    if (array.rank != 1)
                    {
                        base.context.HandleError(JSError.TypeMismatch, reportError);
                        return false;
                    }
                    elementType = array.elementType;
                }
                else
                {
                    if (!(lhir is Type) || !((Type) lhir).IsArray)
                    {
                        return false;
                    }
                    Type type = (Type) lhir;
                    if (type.GetArrayRank() != 1)
                    {
                        base.context.HandleError(JSError.TypeMismatch, reportError);
                        return false;
                    }
                    elementType = type.GetElementType();
                }
                int num = 0;
                int count = this.elements.count;
                while (num < count)
                {
                    if (!Binding.AssignmentCompatible(elementType, this.elements[num], this.elements[num].InferType(null), reportError))
                    {
                        return false;
                    }
                    num++;
                }
            }
            return true;
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            int num = 0;
            int count = this.elements.count;
            while (num < count)
            {
                this.elements[num].CheckIfOKToUseInSuperConstructorCall();
                num++;
            }
        }

        internal override object Evaluate()
        {
            if (VsaEngine.executeForJSEE)
            {
                throw new JScriptException(JSError.NonSupportedInDebugger);
            }
            int count = this.elements.count;
            object[] args = new object[count];
            for (int i = 0; i < count; i++)
            {
                args[i] = this.elements[i].Evaluate();
            }
            return base.Engine.GetOriginalArrayConstructor().ConstructArray(args);
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.ArrayObject;
        }

        internal bool IsOkToUseInCustomAttribute()
        {
            int count = this.elements.count;
            for (int i = 0; i < count; i++)
            {
                object obj2 = this.elements[i];
                if (!(obj2 is ConstantWrapper))
                {
                    return false;
                }
                if (Microsoft.JScript.CustomAttribute.TypeOfArgument(((ConstantWrapper) obj2).Evaluate()) == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal override AST PartiallyEvaluate()
        {
            int count = this.elements.count;
            for (int i = 0; i < count; i++)
            {
                this.elements[i] = this.elements[i].PartiallyEvaluate();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (rtype == Typeob.Array)
            {
                this.TranslateToILArray(il, Typeob.Object);
            }
            else if (rtype.IsArray && (rtype.GetArrayRank() == 1))
            {
                this.TranslateToILArray(il, rtype.GetElementType());
            }
            else
            {
                int count = this.elements.count;
                MethodInfo meth = null;
                if (base.Engine.Globals.globalObject is LenientGlobalObject)
                {
                    base.EmitILToLoadEngine(il);
                    il.Emit(OpCodes.Call, CompilerGlobals.getOriginalArrayConstructorMethod);
                    meth = CompilerGlobals.constructArrayMethod;
                }
                else
                {
                    meth = CompilerGlobals.fastConstructArrayLiteralMethod;
                }
                ConstantWrapper.TranslateToILInt(il, count);
                il.Emit(OpCodes.Newarr, Typeob.Object);
                for (int i = 0; i < count; i++)
                {
                    il.Emit(OpCodes.Dup);
                    ConstantWrapper.TranslateToILInt(il, i);
                    this.elements[i].TranslateToIL(il, Typeob.Object);
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Call, meth);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.ArrayObject, rtype);
            }
        }

        private void TranslateToILArray(ILGenerator il, Type etype)
        {
            int count = this.elements.count;
            ConstantWrapper.TranslateToILInt(il, count);
            Type.GetTypeCode(etype);
            il.Emit(OpCodes.Newarr, etype);
            for (int i = 0; i < count; i++)
            {
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, i);
                if (etype.IsValueType && !etype.IsPrimitive)
                {
                    il.Emit(OpCodes.Ldelema, etype);
                }
                this.elements[i].TranslateToIL(il, etype);
                Binding.TranslateToStelem(il, etype);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            int num = 0;
            int count = this.elements.count;
            while (num < count)
            {
                this.elements[num].TranslateToILInitializer(il);
                num++;
            }
        }
    }
}

