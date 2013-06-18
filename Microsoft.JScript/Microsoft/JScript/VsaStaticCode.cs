namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.CodeDom;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class VsaStaticCode : VsaItem, IJSVsaCodeItem, IJSVsaItem
    {
        private ScriptBlock block;
        internal Context codeContext;
        private Type compiledClass;

        internal VsaStaticCode(VsaEngine engine, string itemName, JSVsaItemFlag flag) : base(engine, itemName, JSVsaItemType.Code, flag)
        {
            this.compiledClass = null;
            this.codeContext = new Context(new DocumentContext(this), "");
        }

        public void AddEventSource(string eventSourceName, string eventSourceType)
        {
            if (base.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            throw new NotSupportedException();
        }

        public void AppendSourceText(string SourceCode)
        {
            if (base.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            if ((SourceCode != null) && (SourceCode.Length != 0))
            {
                this.codeContext.SetSourceContext(this.codeContext.document, this.codeContext.source_string + SourceCode);
                this.compiledClass = null;
                base.isDirty = true;
                base.engine.IsDirty = true;
            }
        }

        internal override void CheckForErrors()
        {
            if (this.compiledClass == null)
            {
                new JSParser(this.codeContext).Parse();
            }
        }

        internal override void Close()
        {
            base.Close();
            this.codeContext = null;
            this.compiledClass = null;
        }

        internal override Type GetCompiledType()
        {
            TypeBuilder compiledClass = this.compiledClass as TypeBuilder;
            if (compiledClass != null)
            {
                this.compiledClass = compiledClass.CreateType();
            }
            return this.compiledClass;
        }

        internal void Parse()
        {
            if ((this.block == null) && (this.compiledClass == null))
            {
                GlobalScope item = (GlobalScope) base.engine.GetGlobalScope().GetObject();
                item.evilScript = !item.fast || (base.engine.GetStaticCodeBlockCount() > 1);
                base.engine.Globals.ScopeStack.Push(item);
                try
                {
                    JSParser parser = new JSParser(this.codeContext);
                    this.block = parser.Parse();
                    if (parser.HasAborted)
                    {
                        this.block = null;
                    }
                }
                finally
                {
                    base.engine.Globals.ScopeStack.Pop();
                }
            }
        }

        internal void PartiallyEvaluate()
        {
            if ((this.block != null) && (this.compiledClass == null))
            {
                GlobalScope item = (GlobalScope) base.engine.GetGlobalScope().GetObject();
                base.engine.Globals.ScopeStack.Push(item);
                try
                {
                    this.block.PartiallyEvaluate();
                    if (base.engine.HasErrors && !base.engine.alwaysGenerateIL)
                    {
                        throw new EndOfFile();
                    }
                }
                finally
                {
                    base.engine.Globals.ScopeStack.Pop();
                }
            }
        }

        internal void ProcessAssemblyAttributeLists()
        {
            if (this.block != null)
            {
                this.block.ProcessAssemblyAttributeLists();
            }
        }

        internal override void Remove()
        {
            if (base.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            base.Remove();
        }

        public void RemoveEventSource(string eventSourceName)
        {
            if (base.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            throw new NotSupportedException();
        }

        internal override void Reset()
        {
            this.compiledClass = null;
            this.block = null;
            this.codeContext = new Context(new DocumentContext(this), this.codeContext.source_string);
        }

        internal override void Run()
        {
            if (this.compiledClass != null)
            {
                GlobalScope item = (GlobalScope) Activator.CreateInstance(this.GetCompiledType(), new object[] { base.engine.GetGlobalScope().GetObject() });
                base.engine.Globals.ScopeStack.Push(item);
                try
                {
                    MethodInfo method = this.compiledClass.GetMethod("Global Code");
                    try
                    {
                        method.Invoke(item, null);
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                }
                finally
                {
                    base.engine.Globals.ScopeStack.Pop();
                }
            }
        }

        public override void SetOption(string name, object value)
        {
            if (base.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            if (string.Compare(name, "codebase", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new JSVsaException(JSVsaError.OptionNotSupported);
            }
            base.codebase = (string) value;
            this.codeContext.document.documentName = base.codebase;
            base.isDirty = true;
            base.engine.IsDirty = true;
        }

        internal void TranslateToIL()
        {
            if ((this.block != null) && (this.compiledClass == null))
            {
                GlobalScope item = (GlobalScope) base.engine.GetGlobalScope().GetObject();
                base.engine.Globals.ScopeStack.Push(item);
                try
                {
                    this.compiledClass = this.block.TranslateToILClass(base.engine.CompilerGlobals, false);
                }
                finally
                {
                    base.engine.Globals.ScopeStack.Pop();
                }
            }
        }

        public CodeObject CodeDOM
        {
            get
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                throw new JSVsaException(JSVsaError.CodeDOMNotAvailable);
            }
        }

        public override string Name
        {
            set
            {
                base.Name = value;
                if (base.codebase == null)
                {
                    string rootMoniker = base.engine.RootMoniker;
                    this.codeContext.document.documentName = rootMoniker + (rootMoniker.EndsWith("/", StringComparison.Ordinal) ? "" : "/") + base.name;
                }
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
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.codeContext.source_string;
            }
            set
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                this.codeContext.SetSourceContext(this.codeContext.document, (value == null) ? "" : value);
                this.compiledClass = null;
                base.isDirty = true;
                base.engine.IsDirty = true;
            }
        }
    }
}

