namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class RegExpLiteral : AST
    {
        private static int counter;
        private bool global;
        private bool ignoreCase;
        private bool multiline;
        private JSGlobalField regExpVar;
        private string source;

        internal RegExpLiteral(string source, string flags, Context context) : base(context)
        {
            this.source = source;
            this.ignoreCase = this.global = this.multiline = false;
            if (flags != null)
            {
                for (int i = 0; i < flags.Length; i++)
                {
                    switch (flags[i])
                    {
                        case 'g':
                            if (this.global)
                            {
                                throw new JScriptException(JSError.RegExpSyntax);
                            }
                            goto Label_0087;

                        case 'i':
                            if (this.ignoreCase)
                            {
                                throw new JScriptException(JSError.RegExpSyntax);
                            }
                            break;

                        case 'm':
                        {
                            if (this.multiline)
                            {
                                throw new JScriptException(JSError.RegExpSyntax);
                            }
                            this.multiline = true;
                            continue;
                        }
                        default:
                            throw new JScriptException(JSError.RegExpSyntax);
                    }
                    this.ignoreCase = true;
                    continue;
                Label_0087:
                    this.global = true;
                }
            }
        }

        internal override object Evaluate()
        {
            if (VsaEngine.executeForJSEE)
            {
                throw new JScriptException(JSError.NonSupportedInDebugger);
            }
            RegExpObject obj2 = (RegExpObject) base.Globals.RegExpTable[this];
            if (obj2 == null)
            {
                obj2 = (RegExpObject) base.Engine.GetOriginalRegExpConstructor().Construct(this.source, this.ignoreCase, this.global, this.multiline);
                base.Globals.RegExpTable[this] = obj2;
            }
            return obj2;
        }

        internal override IReflect InferType(JSField inferenceTarget)
        {
            return Typeob.RegExpObject;
        }

        internal override AST PartiallyEvaluate()
        {
            string name = "regexp " + counter++.ToString(CultureInfo.InvariantCulture);
            GlobalScope scope = (GlobalScope) base.Engine.GetGlobalScope().GetObject();
            JSGlobalField field = (JSGlobalField) scope.AddNewField(name, null, FieldAttributes.Assembly);
            field.type = new TypeExpression(new ConstantWrapper(Typeob.RegExpObject, base.context));
            this.regExpVar = field;
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            il.Emit(OpCodes.Ldsfld, (FieldInfo) this.regExpVar.GetMetaData());
            Microsoft.JScript.Convert.Emit(this, il, Typeob.RegExpObject, rtype);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            ScriptObject parent = base.Engine.ScriptObjectStackTop();
            while ((parent != null) && ((parent is WithObject) || (parent is BlockScope)))
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                base.EmitILToLoadEngine(il);
                il.Emit(OpCodes.Pop);
            }
            il.Emit(OpCodes.Ldsfld, (FieldInfo) this.regExpVar.GetMetaData());
            Label label = il.DefineLabel();
            il.Emit(OpCodes.Brtrue_S, label);
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.getOriginalRegExpConstructorMethod);
            il.Emit(OpCodes.Ldstr, this.source);
            if (this.ignoreCase)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            if (this.global)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            if (this.multiline)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            il.Emit(OpCodes.Call, CompilerGlobals.regExpConstructMethod);
            il.Emit(OpCodes.Castclass, Typeob.RegExpObject);
            il.Emit(OpCodes.Stsfld, (FieldInfo) this.regExpVar.GetMetaData());
            il.MarkLabel(label);
        }
    }
}

