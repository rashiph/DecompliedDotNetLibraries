namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.CodeDom;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;

    internal class VsaScriptCode : VsaItem, IVsaScriptCodeItem, IJSVsaCodeItem, IJSVsaItem, IDebugVsaScriptCodeItem
    {
        private ScriptBlock binaryCode;
        private Context codeContext;
        private Type compiledBlock;
        private bool compileToIL;
        internal bool executed;
        private bool optimize;
        private VsaScriptScope scope;

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal VsaScriptCode(VsaEngine engine, string itemName, JSVsaItemType type, IVsaScriptScope scope) : base(engine, itemName, type, JSVsaItemFlag.None)
        {
            this.binaryCode = null;
            this.executed = false;
            this.scope = (VsaScriptScope) scope;
            this.codeContext = new Context(new DocumentContext(this), null);
            this.compiledBlock = null;
            this.compileToIL = true;
            this.optimize = true;
        }

        public void AddEventSource(string EventSourceName, string EventSourceType)
        {
        }

        public void AppendSourceText(string SourceCode)
        {
            if ((SourceCode != null) && (SourceCode.Length != 0))
            {
                this.codeContext.SetSourceContext(this.codeContext.document, this.codeContext.source_string + SourceCode);
                this.executed = false;
                this.binaryCode = null;
            }
        }

        internal override void Close()
        {
            base.Close();
            this.binaryCode = null;
            this.scope = null;
            this.codeContext = null;
            this.compiledBlock = null;
        }

        internal override void Compile()
        {
            if (this.binaryCode == null)
            {
                JSParser parser = new JSParser(this.codeContext);
                if (base.ItemType == ((JSVsaItemType) 0x16))
                {
                    this.binaryCode = parser.ParseExpressionItem();
                }
                else
                {
                    this.binaryCode = parser.Parse();
                }
                if (this.optimize && !parser.HasAborted)
                {
                    this.binaryCode.ProcessAssemblyAttributeLists();
                    this.binaryCode.PartiallyEvaluate();
                }
                if (base.engine.HasErrors && !base.engine.alwaysGenerateIL)
                {
                    throw new EndOfFile();
                }
                if (this.compileToIL)
                {
                    this.compiledBlock = this.binaryCode.TranslateToILClass(base.engine.CompilerGlobals).CreateType();
                }
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual object Evaluate()
        {
            return this.Execute();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual object Execute()
        {
            object obj2;
            if (!base.engine.IsRunning)
            {
                throw new JSVsaException(JSVsaError.EngineNotRunning);
            }
            base.engine.Globals.ScopeStack.Push((ScriptObject) this.scope.GetObject());
            try
            {
                this.Compile();
                obj2 = this.RunCode();
            }
            finally
            {
                base.engine.Globals.ScopeStack.Pop();
            }
            return obj2;
        }

        internal override Type GetCompiledType()
        {
            return this.compiledBlock;
        }

        public override object GetOption(string name)
        {
            if (string.Compare(name, "il", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.compileToIL;
            }
            if (string.Compare(name, "optimize", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.optimize;
            }
            return base.GetOption(name);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual bool ParseNamedBreakPoint(string input, out string functionName, out int nargs, out string arguments, out string returnType, out ulong offset)
        {
            functionName = "";
            nargs = 0;
            arguments = "";
            returnType = "";
            offset = 0L;
            string[] strArray = new JSParser(this.codeContext).ParseNamedBreakpoint(out nargs);
            if ((strArray == null) || (strArray.Length != 4))
            {
                return false;
            }
            if (strArray[0] != null)
            {
                functionName = strArray[0];
            }
            if (strArray[1] != null)
            {
                arguments = strArray[1];
            }
            if (strArray[2] != null)
            {
                returnType = strArray[2];
            }
            if (strArray[3] != null)
            {
                offset = ((IConvertible) Microsoft.JScript.Convert.LiteralToNumber(strArray[3])).ToUInt64(null);
            }
            return true;
        }

        public void RemoveEventSource(string EventSourceName)
        {
        }

        internal override void Reset()
        {
            this.binaryCode = null;
            this.compiledBlock = null;
            this.executed = false;
            this.codeContext = new Context(new DocumentContext(this), this.codeContext.source_string);
        }

        internal override void Run()
        {
            if (!this.executed)
            {
                this.RunCode();
            }
        }

        private object RunCode()
        {
            if (this.binaryCode == null)
            {
                return null;
            }
            object obj2 = null;
            if (null != this.compiledBlock)
            {
                GlobalScope scope = (GlobalScope) Activator.CreateInstance(this.compiledBlock, new object[] { this.scope.GetObject() });
                this.scope.ReRun(scope);
                MethodInfo method = this.compiledBlock.GetMethod("Global Code");
                try
                {
                    System.Runtime.Remoting.Messaging.CallContext.SetData("JScript:" + this.compiledBlock.Assembly.FullName, base.engine);
                    obj2 = method.Invoke(scope, null);
                    goto Label_00A6;
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            }
            obj2 = this.binaryCode.Evaluate();
        Label_00A6:
            this.executed = true;
            return obj2;
        }

        public override void SetOption(string name, object value)
        {
            if (string.Compare(name, "il", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.compileToIL = (bool) value;
                if (this.compileToIL)
                {
                    this.optimize = true;
                }
            }
            else if (string.Compare(name, "optimize", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.optimize = (bool) value;
                if (!this.optimize)
                {
                    this.compileToIL = false;
                }
            }
            else if (string.Compare(name, "codebase", StringComparison.OrdinalIgnoreCase) == 0)
            {
                base.codebase = (string) value;
                this.codeContext.document.documentName = base.codebase;
            }
            else
            {
                base.SetOption(name, value);
            }
        }

        public CodeObject CodeDOM
        {
            get
            {
                throw new JSVsaException(JSVsaError.CodeDOMNotAvailable);
            }
        }

        public override string Name
        {
            set
            {
                base.name = value;
                if (base.codebase == null)
                {
                    string rootMoniker = base.engine.RootMoniker;
                    this.codeContext.document.documentName = rootMoniker + (rootMoniker.EndsWith("/", StringComparison.Ordinal) ? "" : "/") + base.name;
                }
            }
        }

        public IVsaScriptScope Scope
        {
            get
            {
                return this.scope;
            }
        }

        public object SourceContext
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string SourceText
        {
            get
            {
                return this.codeContext.source_string;
            }
            set
            {
                this.codeContext.SetSourceContext(this.codeContext.document, (value == null) ? "" : value);
                this.executed = false;
                this.binaryCode = null;
            }
        }

        public int StartColumn
        {
            get
            {
                return this.codeContext.document.startCol;
            }
            set
            {
                this.codeContext.document.startCol = value;
            }
        }

        public int StartLine
        {
            get
            {
                return this.codeContext.document.startLine;
            }
            set
            {
                this.codeContext.document.startLine = value;
            }
        }
    }
}

