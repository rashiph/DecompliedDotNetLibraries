namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;

    internal sealed class VsaHostObject : VsaItem, IJSVsaGlobalItem, IJSVsaItem
    {
        private bool compiled;
        private bool exposed;
        internal bool exposeMembers;
        private FieldInfo field;
        private object hostObject;
        internal bool isVisible;
        private VsaScriptScope scope;
        private string typeString;

        internal VsaHostObject(VsaEngine engine, string itemName, JSVsaItemType type) : this(engine, itemName, type, null)
        {
        }

        internal VsaHostObject(VsaEngine engine, string itemName, JSVsaItemType type, VsaScriptScope scope) : base(engine, itemName, type, JSVsaItemFlag.None)
        {
            this.hostObject = null;
            this.exposeMembers = false;
            this.isVisible = false;
            this.exposed = false;
            this.compiled = false;
            this.scope = scope;
            this.field = null;
            this.typeString = "System.Object";
        }

        private void AddNamedItemNamespace()
        {
            GlobalScope scope = (GlobalScope) this.Scope.GetObject();
            if (scope.isComponentScope)
            {
                scope = (GlobalScope) scope.GetParent();
            }
            ScriptObject parent = scope.GetParent();
            VsaNamedItemScope scope2 = new VsaNamedItemScope(this.GetObject(), parent, base.engine);
            scope.SetParent(scope2);
            scope2.SetParent(parent);
        }

        internal override void CheckForErrors()
        {
            this.Compile();
        }

        internal override void Close()
        {
            this.Remove();
            base.Close();
            this.hostObject = null;
            this.scope = null;
        }

        internal override void Compile()
        {
            if (!this.compiled && this.isVisible)
            {
                JSVariableField field = ((ActivationObject) this.Scope.GetObject()).AddFieldOrUseExistingField(base.name, null, FieldAttributes.Static | FieldAttributes.Public);
                Type type = base.engine.GetType(this.typeString);
                if (type != null)
                {
                    field.type = new TypeExpression(new ConstantWrapper(type, null));
                }
                this.field = field;
            }
        }

        public object GetObject()
        {
            if (base.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            if (this.hostObject == null)
            {
                if (base.engine.Site == null)
                {
                    throw new JSVsaException(JSVsaError.SiteNotSet);
                }
                this.hostObject = base.engine.Site.GetGlobalInstance(base.name);
            }
            return this.hostObject;
        }

        internal override void Remove()
        {
            base.Remove();
            if (this.exposed)
            {
                if (this.exposeMembers)
                {
                    this.RemoveNamedItemNamespace();
                }
                if (this.isVisible)
                {
                    ((ScriptObject) this.Scope.GetObject()).DeleteMember(base.name);
                }
                this.hostObject = null;
                this.exposed = false;
            }
        }

        private void RemoveNamedItemNamespace()
        {
            ScriptObject obj2 = (ScriptObject) this.Scope.GetObject();
            for (ScriptObject obj3 = obj2.GetParent(); obj3 != null; obj3 = obj3.GetParent())
            {
                if ((obj3 is VsaNamedItemScope) && (((VsaNamedItemScope) obj3).namedItem == this.hostObject))
                {
                    obj2.SetParent(obj3.GetParent());
                    return;
                }
                obj2 = obj3;
            }
        }

        internal void ReRun(GlobalScope scope)
        {
            if (this.field is JSGlobalField)
            {
                ((JSGlobalField) this.field).ILField = scope.GetField(base.name, BindingFlags.Public | BindingFlags.Static);
                this.field.SetValue(scope, this.GetObject());
                this.field = null;
            }
        }

        internal override void Reset()
        {
            base.Reset();
            this.hostObject = null;
            this.exposed = false;
            this.compiled = false;
            this.scope = null;
        }

        internal override void Run()
        {
            if (!this.exposed)
            {
                if (this.isVisible)
                {
                    this.field = ((ActivationObject) this.Scope.GetObject()).AddFieldOrUseExistingField(base.name, this.GetObject(), FieldAttributes.Static | FieldAttributes.Public);
                }
                if (this.exposeMembers)
                {
                    this.AddNamedItemNamespace();
                }
                this.exposed = true;
            }
        }

        public bool ExposeMembers
        {
            get
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.exposeMembers;
            }
            set
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                this.exposeMembers = value;
            }
        }

        internal FieldInfo Field
        {
            get
            {
                JSVariableField field = this.field as JSVariableField;
                if (field != null)
                {
                    return (FieldInfo) field.GetMetaData();
                }
                return this.field;
            }
        }

        private VsaScriptScope Scope
        {
            get
            {
                if (this.scope == null)
                {
                    this.scope = (VsaScriptScope) base.engine.GetGlobalScope();
                }
                return this.scope;
            }
        }

        public string TypeString
        {
            get
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.typeString;
            }
            set
            {
                if (base.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                this.typeString = value;
                base.isDirty = true;
                base.engine.IsDirty = true;
            }
        }
    }
}

