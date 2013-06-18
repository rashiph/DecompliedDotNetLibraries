namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;

    public sealed class Globals
    {
        [ThreadStatic]
        private static TypeReferences _typeRefs;
        internal CultureInfo assemblyCulture;
        internal bool assemblyDelaySign;
        internal AssemblyFlags assemblyFlags = (AssemblyFlags.EnableJITcompileTracking | AssemblyFlags.DisableJITcompileOptimizer);
        internal AssemblyHashAlgorithm assemblyHashAlgorithm = AssemblyHashAlgorithm.SHA1;
        internal string assemblyKeyFileName;
        internal Context assemblyKeyFileNameContext;
        internal string assemblyKeyName;
        internal Context assemblyKeyNameContext;
        internal Version assemblyVersion;
        internal AssemblyVersionCompatibility assemblyVersionCompatibility;
        private static SimpleHashtable BuiltinFunctionTable = null;
        private Stack callContextStack;
        internal object caller;
        [ContextStatic]
        public static VsaEngine contextEngine = null;
        internal VsaEngine engine;
        internal GlobalObject globalObject;
        private SimpleHashtable regExpTable;
        private Stack scopeStack;

        internal Globals(bool fast, VsaEngine engine)
        {
            this.engine = engine;
            this.callContextStack = null;
            this.scopeStack = null;
            this.caller = DBNull.Value;
            this.regExpTable = null;
            if (fast)
            {
                this.globalObject = GlobalObject.commonInstance;
            }
            else
            {
                this.globalObject = new LenientGlobalObject(engine);
            }
        }

        internal static BuiltinFunction BuiltinFunctionFor(object obj, MethodInfo meth)
        {
            if (BuiltinFunctionTable == null)
            {
                BuiltinFunctionTable = new SimpleHashtable(0x40);
            }
            BuiltinFunction function = (BuiltinFunction) BuiltinFunctionTable[meth];
            if (function == null)
            {
                function = new BuiltinFunction(obj, meth);
                lock (BuiltinFunctionTable)
                {
                    BuiltinFunctionTable[meth] = function;
                }
            }
            return function;
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public static ArrayObject ConstructArray(params object[] args)
        {
            return (ArrayObject) ArrayConstructor.ob.Construct(args);
        }

        public static ArrayObject ConstructArrayLiteral(object[] args)
        {
            return ArrayConstructor.ob.ConstructArray(args);
        }

        internal Stack CallContextStack
        {
            get
            {
                if (this.callContextStack == null)
                {
                    this.callContextStack = new Stack();
                }
                return this.callContextStack;
            }
        }

        internal SimpleHashtable RegExpTable
        {
            get
            {
                if (this.regExpTable == null)
                {
                    this.regExpTable = new SimpleHashtable(8);
                }
                return this.regExpTable;
            }
        }

        internal Stack ScopeStack
        {
            get
            {
                if (this.scopeStack == null)
                {
                    this.scopeStack = new Stack();
                    this.scopeStack.Push(this.engine.GetGlobalScope().GetObject());
                }
                return this.scopeStack;
            }
        }

        internal static TypeReferences TypeRefs
        {
            get
            {
                TypeReferences references = _typeRefs;
                if (references == null)
                {
                    references = _typeRefs = Runtime.TypeRefs;
                }
                return references;
            }
            set
            {
                _typeRefs = value;
            }
        }
    }
}

