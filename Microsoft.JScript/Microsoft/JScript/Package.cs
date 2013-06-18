namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Package : AST
    {
        private ASTList classList;
        private string name;
        private PackageScope scope;

        internal Package(string name, AST id, ASTList classList, Context context) : base(context)
        {
            this.name = name;
            this.classList = classList;
            this.scope = (PackageScope) base.Globals.ScopeStack.Peek();
            this.scope.owner = this;
            base.Engine.AddPackage(this.scope);
            Lookup lookup = id as Lookup;
            if (lookup != null)
            {
                lookup.EvaluateAsWrappedNamespace(true);
            }
            else
            {
                Member member = id as Member;
                if (member != null)
                {
                    member.EvaluateAsWrappedNamespace(true);
                }
            }
        }

        internal override object Evaluate()
        {
            object obj2;
            base.Globals.ScopeStack.Push(this.scope);
            try
            {
                int num = 0;
                int count = this.classList.count;
                while (num < count)
                {
                    this.classList[num].Evaluate();
                    num++;
                }
                obj2 = new Completion();
            }
            finally
            {
                base.Globals.ScopeStack.Pop();
            }
            return obj2;
        }

        internal override Context GetFirstExecutableContext()
        {
            return null;
        }

        public static void JScriptPackage(string rootName, VsaEngine engine)
        {
            GlobalScope globalScope = ((IActivationObject) engine.ScriptObjectStackTop()).GetGlobalScope();
            if (globalScope.GetLocalField(rootName) == null)
            {
                FieldInfo info = globalScope.AddNewField(rootName, Namespace.GetNamespace(rootName, engine), FieldAttributes.Literal | FieldAttributes.Public);
            }
        }

        internal void MergeWith(Package p)
        {
            int num = 0;
            int count = p.classList.count;
            while (num < count)
            {
                this.classList.Append(p.classList[num]);
                num++;
            }
            this.scope.MergeWith(p.scope);
        }

        internal override AST PartiallyEvaluate()
        {
            AST ast;
            this.scope.AddOwnName();
            base.Globals.ScopeStack.Push(this.scope);
            try
            {
                int num = 0;
                int count = this.classList.count;
                while (num < count)
                {
                    this.classList[num].PartiallyEvaluate();
                    num++;
                }
                ast = this;
            }
            finally
            {
                base.Globals.ScopeStack.Pop();
            }
            return ast;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.Globals.ScopeStack.Push(this.scope);
            int num = 0;
            int count = this.classList.count;
            while (num < count)
            {
                this.classList[num].TranslateToIL(il, Typeob.Void);
                num++;
            }
            base.Globals.ScopeStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            string name = this.name;
            int index = name.IndexOf('.');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }
            il.Emit(OpCodes.Ldstr, name);
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.jScriptPackageMethod);
            base.Globals.ScopeStack.Push(this.scope);
            int num2 = 0;
            int count = this.classList.count;
            while (num2 < count)
            {
                this.classList[num2].TranslateToILInitializer(il);
                num2++;
            }
            base.Globals.ScopeStack.Pop();
        }
    }
}

