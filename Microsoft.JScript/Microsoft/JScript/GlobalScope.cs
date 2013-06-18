namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.Expando;

    [ComVisible(true)]
    public class GlobalScope : ActivationObject, IExpando, IReflect
    {
        private ArrayList componentScopes;
        internal bool evilScript;
        internal GlobalObject globalObject;
        private TypeReflector globalObjectTR;
        internal bool isComponentScope;
        private bool recursive;
        internal object thisObject;
        private TypeReflector typeReflector;

        public GlobalScope(GlobalScope parent, VsaEngine engine) : this(parent, engine, parent != null)
        {
        }

        internal GlobalScope(GlobalScope parent, VsaEngine engine, bool isComponentScope) : base(parent)
        {
            this.componentScopes = null;
            this.recursive = false;
            this.isComponentScope = isComponentScope;
            if (parent == null)
            {
                this.globalObject = engine.Globals.globalObject;
                this.globalObjectTR = TypeReflector.GetTypeReflectorFor(Globals.TypeRefs.ToReferenceContext(this.globalObject.GetType()));
                base.fast = !(this.globalObject is LenientGlobalObject);
            }
            else
            {
                this.globalObject = null;
                this.globalObjectTR = null;
                base.fast = parent.fast;
                if (isComponentScope)
                {
                    ((GlobalScope) base.parent).AddComponentScope(this);
                }
            }
            base.engine = engine;
            base.isKnownAtCompileTime = base.fast;
            this.evilScript = true;
            this.thisObject = this;
            this.typeReflector = TypeReflector.GetTypeReflectorFor(Globals.TypeRefs.ToReferenceContext(base.GetType()));
            if (isComponentScope)
            {
                engine.Scopes.Add(this);
            }
        }

        internal void AddComponentScope(GlobalScope component)
        {
            if (this.componentScopes == null)
            {
                this.componentScopes = new ArrayList();
            }
            this.componentScopes.Add(component);
            component.thisObject = this.thisObject;
        }

        public FieldInfo AddField(string name)
        {
            if (base.fast)
            {
                return null;
            }
            if (this.isComponentScope)
            {
                return ((GlobalScope) base.parent).AddField(name);
            }
            FieldInfo info = (FieldInfo) base.name_table[name];
            if (info == null)
            {
                info = new JSExpandoField(name);
                base.name_table[name] = info;
                base.field_table.Add(info);
            }
            return info;
        }

        internal override JSVariableField AddNewField(string name, object value, FieldAttributes attributeFlags)
        {
            if (!this.isComponentScope)
            {
                return base.AddNewField(name, value, attributeFlags);
            }
            return ((GlobalScope) base.parent).AddNewField(name, value, attributeFlags);
        }

        internal override bool DeleteMember(string name)
        {
            if (this.isComponentScope)
            {
                return base.parent.DeleteMember(name);
            }
            FieldInfo info = (FieldInfo) base.name_table[name];
            if ((info != null) && (info is JSExpandoField))
            {
                info.SetValue(this, Microsoft.JScript.Missing.Value);
                base.name_table.Remove(name);
                base.field_table.Remove(info);
                return true;
            }
            return false;
        }

        public override object GetDefaultThisObject()
        {
            return this;
        }

        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            if ((preferred_type != PreferredType.String) && (preferred_type != PreferredType.LocaleString))
            {
                return (double) 1.0 / (double) 0.0;
            }
            return "";
        }

        public override FieldInfo GetField(string name, int lexLevel)
        {
            return base.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        internal JSField[] GetFields()
        {
            int count = base.field_table.Count;
            JSField[] fieldArray = new JSField[count];
            for (int i = 0; i < count; i++)
            {
                fieldArray[i] = (JSField) base.field_table[i];
            }
            return fieldArray;
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return base.GetFields(bindingAttr | BindingFlags.DeclaredOnly);
        }

        public override GlobalScope GetGlobalScope()
        {
            return this;
        }

        public override FieldInfo GetLocalField(string name)
        {
            return base.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return this.GetMember(name, bindingAttr, false);
        }

        private MemberInfo[] GetMember(string name, BindingFlags bindingAttr, bool calledFromParent)
        {
            if (!this.recursive)
            {
                MemberInfo[] members = null;
                if (!this.isComponentScope)
                {
                    MemberInfo[] member = base.GetMember(name, bindingAttr | BindingFlags.DeclaredOnly);
                    if (member.Length > 0)
                    {
                        return member;
                    }
                    if (this.componentScopes != null)
                    {
                        int num = 0;
                        int count = this.componentScopes.Count;
                        while (num < count)
                        {
                            members = ((GlobalScope) this.componentScopes[num]).GetMember(name, bindingAttr | BindingFlags.DeclaredOnly, true);
                            if (members.Length > 0)
                            {
                                return members;
                            }
                            num++;
                        }
                    }
                    if (this.globalObject != null)
                    {
                        members = this.globalObjectTR.GetMember(name, (bindingAttr & ~BindingFlags.NonPublic) | BindingFlags.Static);
                    }
                    if ((members != null) && (members.Length > 0))
                    {
                        return ScriptObject.WrapMembers(members, this.globalObject);
                    }
                }
                else
                {
                    members = this.typeReflector.GetMember(name, (bindingAttr & ~BindingFlags.NonPublic) | BindingFlags.Static);
                    int length = members.Length;
                    if (length > 0)
                    {
                        int num4 = 0;
                        MemberInfo[] infoArray3 = new MemberInfo[length];
                        for (int i = 0; i < length; i++)
                        {
                            MemberInfo info = infoArray3[i] = members[i];
                            if (info.DeclaringType.IsAssignableFrom(Typeob.GlobalScope))
                            {
                                infoArray3[i] = null;
                                num4++;
                            }
                            else if (info is FieldInfo)
                            {
                                FieldInfo info2 = (FieldInfo) info;
                                if (info2.IsStatic && (info2.FieldType == Typeob.Type))
                                {
                                    Type type = (Type) info2.GetValue(null);
                                    if (type != null)
                                    {
                                        infoArray3[i] = type;
                                    }
                                }
                            }
                        }
                        if (num4 == 0)
                        {
                            return members;
                        }
                        if (num4 == length)
                        {
                            return new MemberInfo[0];
                        }
                        MemberInfo[] infoArray4 = new MemberInfo[length - num4];
                        int num6 = 0;
                        foreach (MemberInfo info3 in infoArray3)
                        {
                            if (info3 != null)
                            {
                                infoArray4[num6++] = info3;
                            }
                        }
                        return infoArray4;
                    }
                }
                if (((base.parent != null) && !calledFromParent) && (((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default) || this.isComponentScope))
                {
                    this.recursive = true;
                    try
                    {
                        members = base.parent.GetMember(name, bindingAttr);
                    }
                    finally
                    {
                        this.recursive = false;
                    }
                    if ((members != null) && (members.Length > 0))
                    {
                        return members;
                    }
                }
            }
            return new MemberInfo[0];
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            if (this.recursive)
            {
                return new MemberInfo[0];
            }
            MemberInfoList list = new MemberInfoList();
            if (this.isComponentScope)
            {
                MemberInfo[] members = Globals.TypeRefs.ToReferenceContext(base.GetType()).GetMembers(bindingAttr | BindingFlags.DeclaredOnly);
                if (members != null)
                {
                    foreach (MemberInfo info in members)
                    {
                        list.Add(info);
                    }
                }
            }
            else
            {
                if (this.componentScopes != null)
                {
                    int num = 0;
                    int count = this.componentScopes.Count;
                    while (num < count)
                    {
                        GlobalScope scope = (GlobalScope) this.componentScopes[num];
                        this.recursive = true;
                        MemberInfo[] infoArray2 = null;
                        try
                        {
                            infoArray2 = scope.GetMembers(bindingAttr);
                        }
                        finally
                        {
                            this.recursive = false;
                        }
                        if (infoArray2 != null)
                        {
                            foreach (MemberInfo info2 in infoArray2)
                            {
                                list.Add(info2);
                            }
                        }
                        num++;
                    }
                }
                IEnumerator enumerator = base.field_table.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    FieldInfo current = (FieldInfo) enumerator.Current;
                    list.Add(current);
                }
            }
            if ((base.parent != null) && (this.isComponentScope || ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default)))
            {
                this.recursive = true;
                MemberInfo[] infoArray3 = null;
                try
                {
                    infoArray3 = base.parent.GetMembers(bindingAttr);
                }
                finally
                {
                    this.recursive = false;
                }
                if (infoArray3 != null)
                {
                    foreach (MemberInfo info4 in infoArray3)
                    {
                        list.Add(info4);
                    }
                }
            }
            return list.ToArray();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return base.GetMethods(bindingAttr | BindingFlags.DeclaredOnly);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return base.GetProperties(bindingAttr | BindingFlags.DeclaredOnly);
        }

        internal override void GetPropertyEnumerator(ArrayList enums, ArrayList objects)
        {
            FieldInfo[] fields = this.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (fields.Length > 0)
            {
                enums.Add(fields.GetEnumerator());
                objects.Add(this);
            }
            ScriptObject parent = base.GetParent();
            if (parent != null)
            {
                parent.GetPropertyEnumerator(enums, objects);
            }
        }

        internal void SetFast()
        {
            base.fast = true;
            base.isKnownAtCompileTime = true;
            if (this.globalObject != null)
            {
                this.globalObject = GlobalObject.commonInstance;
                this.globalObjectTR = TypeReflector.GetTypeReflectorFor(Globals.TypeRefs.ToReferenceContext(this.globalObject.GetType()));
            }
        }

        internal override void SetMemberValue(string name, object value)
        {
            MemberInfo[] member = this.GetMember(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (member.Length == 0)
            {
                if (VsaEngine.executeForJSEE)
                {
                    throw new JScriptException(JSError.UndefinedIdentifier, new Context(new DocumentContext("", null), name));
                }
                FieldInfo info = this.AddField(name);
                if (info != null)
                {
                    info.SetValue(this, value);
                }
            }
            else
            {
                MemberInfo info2 = LateBinding.SelectMember(member);
                if (info2 == null)
                {
                    throw new JScriptException(JSError.AssignmentToReadOnly);
                }
                LateBinding.SetMemberValue(this, name, value, info2, member);
            }
        }

        MethodInfo IExpando.AddMethod(string name, Delegate method)
        {
            return null;
        }

        PropertyInfo IExpando.AddProperty(string name)
        {
            return null;
        }

        void IExpando.RemoveMember(MemberInfo m)
        {
            this.DeleteMember(m.Name);
        }
    }
}

