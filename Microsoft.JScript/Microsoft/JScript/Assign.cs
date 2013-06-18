namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class Assign : AST
    {
        internal AST lhside;
        internal AST rhside;

        internal Assign(Context context, AST lhside, AST rhside) : base(context)
        {
            this.lhside = lhside;
            this.rhside = rhside;
        }

        internal override object Evaluate()
        {
            object obj3;
            try
            {
                if (this.lhside is Call)
                {
                    ((Call) this.lhside).EvaluateIndices();
                }
                object obj2 = this.rhside.Evaluate();
                this.lhside.SetValue(obj2);
                obj3 = obj2;
            }
            catch (JScriptException exception)
            {
                if (exception.context == null)
                {
                    exception.context = base.context;
                }
                throw exception;
            }
            catch (Exception exception2)
            {
                throw new JScriptException(exception2, base.context);
            }
            return obj3;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return this.rhside.InferType(inference_target);
        }

        internal override AST PartiallyEvaluate()
        {
            AST ast = this.lhside.PartiallyEvaluateAsReference();
            this.lhside = ast;
            this.rhside = this.rhside.PartiallyEvaluate();
            ast.SetPartialValue(this.rhside);
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Type type = Microsoft.JScript.Convert.ToType(this.lhside.InferType(null));
            this.lhside.TranslateToILPreSet(il);
            if (rtype != Typeob.Void)
            {
                Type type2 = Microsoft.JScript.Convert.ToType(this.rhside.InferType(null));
                this.rhside.TranslateToIL(il, type2);
                LocalBuilder local = il.DeclareLocal(type2);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, local);
                Microsoft.JScript.Convert.Emit(this, il, type2, type);
                this.lhside.TranslateToILSet(il);
                il.Emit(OpCodes.Ldloc, local);
                Microsoft.JScript.Convert.Emit(this, il, type2, rtype);
            }
            else
            {
                this.lhside.TranslateToILSet(il, this.rhside);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.lhside.TranslateToILInitializer(il);
            this.rhside.TranslateToILInitializer(il);
        }
    }
}

