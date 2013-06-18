namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal sealed class FunctionScope : ActivationObject
    {
        internal bool closuresMightEscape;
        private ArrayList fields_for_nested_functions;
        internal bool isMethod;
        internal bool isStatic;
        internal bool mustSaveStackLocals;
        internal ArrayList nested_functions;
        internal FunctionObject owner;
        internal SimpleHashtable ProvidesOuterScopeLocals;
        internal JSLocalField returnVar;

        internal FunctionScope(ScriptObject parent) : this(parent, false)
        {
        }

        internal FunctionScope(ScriptObject parent, bool isMethod) : base(parent)
        {
            base.isKnownAtCompileTime = true;
            this.isMethod = isMethod;
            this.mustSaveStackLocals = false;
            if ((parent != null) && (parent is ActivationObject))
            {
                base.fast = ((ActivationObject) parent).fast;
            }
            else
            {
                base.fast = false;
            }
            this.returnVar = null;
            this.owner = null;
            this.isStatic = false;
            this.nested_functions = null;
            this.fields_for_nested_functions = null;
            if (parent is FunctionScope)
            {
                this.ProvidesOuterScopeLocals = new SimpleHashtable(0x10);
            }
            else
            {
                this.ProvidesOuterScopeLocals = null;
            }
            this.closuresMightEscape = false;
        }

        internal JSVariableField AddNewField(string name, FieldAttributes attributeFlags, FunctionObject func)
        {
            if (this.nested_functions == null)
            {
                this.nested_functions = new ArrayList();
                this.fields_for_nested_functions = new ArrayList();
            }
            this.nested_functions.Add(func);
            JSVariableField field = this.AddNewField(name, func, attributeFlags);
            this.fields_for_nested_functions.Add(field);
            return field;
        }

        internal void AddOuterScopeField(string name, JSLocalField field)
        {
            base.name_table[name] = field;
            base.field_table.Add(field);
        }

        internal void AddReturnValueField()
        {
            if (base.name_table["return value"] == null)
            {
                this.returnVar = new JSLocalField("return value", this, base.field_table.Count, Microsoft.JScript.Missing.Value);
                base.name_table["return value"] = this.returnVar;
                base.field_table.Add(this.returnVar);
            }
        }

        internal void CloseNestedFunctions(StackFrame sf)
        {
            if (this.nested_functions != null)
            {
                IEnumerator enumerator = this.nested_functions.GetEnumerator();
                IEnumerator enumerator2 = this.fields_for_nested_functions.GetEnumerator();
                while (enumerator.MoveNext() && enumerator2.MoveNext())
                {
                    FieldInfo current = (FieldInfo) enumerator2.Current;
                    FunctionObject func = (FunctionObject) enumerator.Current;
                    func.enclosing_scope = sf;
                    current.SetValue(sf, new Closure(func));
                }
            }
        }

        protected override JSVariableField CreateField(string name, FieldAttributes attributeFlags, object value)
        {
            if ((attributeFlags & FieldAttributes.Static) != FieldAttributes.PrivateScope)
            {
                return new JSGlobalField(this, name, value, attributeFlags);
            }
            return new JSLocalField(name, this, base.field_table.Count, value);
        }

        internal JSLocalField[] GetLocalFields()
        {
            int count = base.field_table.Count;
            JSLocalField[] fieldArray = new JSLocalField[base.field_table.Count];
            for (int i = 0; i < count; i++)
            {
                fieldArray[i] = (JSLocalField) base.field_table[i];
            }
            return fieldArray;
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            FieldInfo info = (FieldInfo) base.name_table[name];
            if (info != null)
            {
                return new MemberInfo[] { info };
            }
            bool flag = false;
            ScriptObject parent = base.parent;
            while (parent is FunctionScope)
            {
                FunctionScope scope = (FunctionScope) parent;
                flag = scope.isMethod && !scope.isStatic;
                JSLocalField field = (JSLocalField) scope.name_table[name];
                if (field == null)
                {
                    parent = parent.GetParent();
                }
                else
                {
                    if (field.IsLiteral && !(field.value is FunctionObject))
                    {
                        return new MemberInfo[] { field };
                    }
                    JSLocalField field2 = new JSLocalField(field.Name, this, base.field_table.Count, Microsoft.JScript.Missing.Value) {
                        outerField = field,
                        debugOn = field.debugOn
                    };
                    if ((!field2.debugOn && this.owner.funcContext.document.debugOn) && scope.owner.funcContext.document.debugOn)
                    {
                        field2.debugOn = Array.IndexOf<string>(scope.owner.formal_parameters, field.Name) >= 0;
                    }
                    field2.isDefined = field.isDefined;
                    field2.debuggerName = "outer." + field2.Name;
                    if (field.IsLiteral)
                    {
                        field2.attributeFlags |= FieldAttributes.Literal;
                        field2.value = field.value;
                    }
                    this.AddOuterScopeField(name, field2);
                    if (this.ProvidesOuterScopeLocals[parent] == null)
                    {
                        this.ProvidesOuterScopeLocals[parent] = parent;
                    }
                    ((FunctionScope) parent).mustSaveStackLocals = true;
                    return new MemberInfo[] { field2 };
                }
            }
            if ((parent is ClassScope) && flag)
            {
                MemberInfo[] member = parent.GetMember(name, bindingAttr & ~BindingFlags.DeclaredOnly);
                int length = member.Length;
                bool flag2 = false;
                for (int i = 0; i < length; i++)
                {
                    MethodInfo info3;
                    PropertyInfo info4;
                    MemberInfo info2 = member[i];
                    MemberTypes memberType = info2.MemberType;
                    if (memberType != MemberTypes.Field)
                    {
                        if (memberType == MemberTypes.Method)
                        {
                            goto Label_029E;
                        }
                        if (memberType == MemberTypes.Property)
                        {
                            goto Label_02C7;
                        }
                    }
                    else
                    {
                        info = (FieldInfo) info2;
                        if (info.IsLiteral)
                        {
                            JSMemberField field3 = info as JSMemberField;
                            if (((field3 != null) && (field3.value is ClassScope)) && !((ClassScope) field3.value).owner.IsStatic)
                            {
                                flag2 = true;
                            }
                        }
                        if (!info.IsStatic && !info.IsLiteral)
                        {
                            member[i] = new JSClosureField(info);
                            flag2 = true;
                        }
                    }
                    continue;
                Label_029E:
                    info3 = (MethodInfo) info2;
                    if (!info3.IsStatic)
                    {
                        member[i] = new JSClosureMethod(info3);
                        flag2 = true;
                    }
                    continue;
                Label_02C7:
                    info4 = (PropertyInfo) info2;
                    MethodInfo getMethod = JSProperty.GetGetMethod(info4, (bindingAttr & BindingFlags.NonPublic) != BindingFlags.Default);
                    MethodInfo setMethod = JSProperty.GetSetMethod(info4, (bindingAttr & BindingFlags.NonPublic) != BindingFlags.Default);
                    bool flag3 = false;
                    if ((getMethod != null) && !getMethod.IsStatic)
                    {
                        flag3 = true;
                        getMethod = new JSClosureMethod(getMethod);
                    }
                    if ((setMethod != null) && !setMethod.IsStatic)
                    {
                        flag3 = true;
                        setMethod = new JSClosureMethod(setMethod);
                    }
                    if (flag3)
                    {
                        member[i] = new JSClosureProperty(info4, getMethod, setMethod);
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    this.GiveOuterFunctionsTheBadNews();
                }
                if (length > 0)
                {
                    return member;
                }
            }
            if ((bindingAttr & BindingFlags.DeclaredOnly) != BindingFlags.Default)
            {
                return new MemberInfo[0];
            }
            return parent.GetMember(name, bindingAttr);
        }

        internal override string GetName()
        {
            string name = null;
            if (base.parent != null)
            {
                name = ((ActivationObject) base.parent).GetName();
            }
            if (name != null)
            {
                return (name + "." + this.owner.name);
            }
            return this.owner.name;
        }

        internal int GetNextSlotNumber()
        {
            return base.field_table.Count;
        }

        internal JSLocalField GetOuterLocalField(string name)
        {
            FieldInfo info = (FieldInfo) base.name_table[name];
            if (((info != null) && (info is JSLocalField)) && (((JSLocalField) info).outerField != null))
            {
                return (JSLocalField) info;
            }
            return null;
        }

        private void GiveOuterFunctionsTheBadNews()
        {
            FunctionScope parent = (FunctionScope) base.parent;
            parent.mustSaveStackLocals = true;
            while (!parent.isMethod)
            {
                parent = (FunctionScope) parent.GetParent();
                parent.mustSaveStackLocals = true;
            }
        }

        internal void HandleUnitializedVariables()
        {
            int num = 0;
            int count = base.field_table.Count;
            while (num < count)
            {
                JSLocalField field = (JSLocalField) base.field_table[num];
                if (field.isUsedBeforeDefinition)
                {
                    field.SetInferredType(Typeob.Object, null);
                }
                num++;
            }
        }

        internal override void SetMemberValue(string name, object value)
        {
            FieldInfo info = (FieldInfo) base.name_table[name];
            if (info != null)
            {
                info.SetValue(this, value);
            }
            else
            {
                base.parent.SetMemberValue(name, value);
            }
        }

        internal void SetMemberValue(string name, object value, StackFrame sf)
        {
            FieldInfo info = (FieldInfo) base.name_table[name];
            if (info != null)
            {
                info.SetValue(sf, value);
            }
            else
            {
                base.parent.SetMemberValue(name, value);
            }
        }

        internal BitArray DefinedFlags
        {
            get
            {
                int count = base.field_table.Count;
                BitArray array = new BitArray(count);
                for (int i = 0; i < count; i++)
                {
                    JSLocalField field = (JSLocalField) base.field_table[i];
                    if (field.isDefined)
                    {
                        array[i] = true;
                    }
                }
                return array;
            }
            set
            {
                int count = value.Count;
                for (int i = 0; i < count; i++)
                {
                    JSLocalField field = (JSLocalField) base.field_table[i];
                    field.isDefined = value[i];
                }
            }
        }
    }
}

