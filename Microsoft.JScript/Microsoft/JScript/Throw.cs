namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Throw : AST
    {
        private AST operand;

        internal Throw(Context context, AST operand) : base(context)
        {
            this.operand = operand;
        }

        internal override object Evaluate()
        {
            if (this.operand == null)
            {
                ScriptObject obj2 = base.Engine.ScriptObjectStackTop();
                while (obj2 != null)
                {
                    BlockScope scope = obj2 as BlockScope;
                    if ((scope != null) && scope.catchHanderScope)
                    {
                        throw ((Exception) scope.GetFields(BindingFlags.Public | BindingFlags.Static)[0].GetValue(null));
                    }
                }
            }
            throw JScriptThrow(this.operand.Evaluate());
        }

        internal override bool HasReturn()
        {
            return true;
        }

        public static Exception JScriptThrow(object value)
        {
            if (value is Exception)
            {
                return (Exception) value;
            }
            if ((value is ErrorObject) && (((ErrorObject) value).exception is Exception))
            {
                return (Exception) ((ErrorObject) value).exception;
            }
            return new JScriptException(value, null);
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.operand != null)
            {
                this.operand = this.operand.PartiallyEvaluate();
            }
            else
            {
                BlockScope scope = null;
                for (ScriptObject obj2 = base.Engine.ScriptObjectStackTop(); obj2 != null; obj2 = obj2.GetParent())
                {
                    if (!(obj2 is WithObject))
                    {
                        scope = obj2 as BlockScope;
                        if ((scope == null) || scope.catchHanderScope)
                        {
                            break;
                        }
                    }
                }
                if (scope == null)
                {
                    base.context.HandleError(JSError.BadThrow);
                    this.operand = new ConstantWrapper(null, base.context);
                }
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.context.EmitLineInfo(il);
            if (this.operand == null)
            {
                il.Emit(OpCodes.Rethrow);
            }
            else
            {
                IReflect reflect = this.operand.InferType(null);
                if ((reflect is Type) && Typeob.Exception.IsAssignableFrom((Type) reflect))
                {
                    this.operand.TranslateToIL(il, (Type) reflect);
                }
                else
                {
                    this.operand.TranslateToIL(il, Typeob.Object);
                    il.Emit(OpCodes.Call, CompilerGlobals.jScriptThrowMethod);
                }
                il.Emit(OpCodes.Throw);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (this.operand != null)
            {
                this.operand.TranslateToILInitializer(il);
            }
        }
    }
}

