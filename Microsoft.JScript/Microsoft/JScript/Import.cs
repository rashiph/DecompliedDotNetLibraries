namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Import : AST
    {
        private string name;

        internal Import(Context context, AST name) : base(context)
        {
            if (name != null)
            {
                WrappedNamespace ob = name.EvaluateAsWrappedNamespace(true);
                base.Engine.SetEnclosingContext(ob);
                this.name = ob.name;
            }
        }

        internal override object Evaluate()
        {
            return new Completion();
        }

        public static void JScriptImport(string name, VsaEngine engine)
        {
            int index = name.IndexOf('.');
            string str = (index > 0) ? name.Substring(0, index) : name;
            GlobalScope globalScope = ((IActivationObject) engine.ScriptObjectStackTop()).GetGlobalScope();
            if (globalScope.GetLocalField(str) == null)
            {
                FieldInfo info = globalScope.AddNewField(str, Namespace.GetNamespace(str, engine), FieldAttributes.Literal | FieldAttributes.Public);
            }
            engine.SetEnclosingContext(new WrappedNamespace(name, engine, false));
        }

        internal override AST PartiallyEvaluate()
        {
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            il.Emit(OpCodes.Ldstr, this.name);
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptImportMethod);
        }
    }
}

