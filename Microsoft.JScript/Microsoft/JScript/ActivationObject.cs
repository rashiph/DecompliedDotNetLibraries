namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class ActivationObject : ScriptObject, IActivationObject
    {
        internal bool fast;
        protected ArrayList field_table;
        internal bool isKnownAtCompileTime;
        internal SimpleHashtable name_table;

        internal ActivationObject(ScriptObject parent) : base(parent)
        {
            this.name_table = new SimpleHashtable(0x20);
            this.field_table = new ArrayList();
        }

        internal void AddClassesExcluding(ClassScope excludedClass, string name, ArrayList result)
        {
            ArrayList list = new ArrayList();
            foreach (MemberInfo info in this.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
            {
                if ((info is JSVariableField) && ((JSVariableField) info).IsLiteral)
                {
                    object obj2 = ((JSVariableField) info).value;
                    if (obj2 is ClassScope)
                    {
                        ClassScope other = (ClassScope) obj2;
                        if (((other.name == info.Name) && ((excludedClass == null) || !excludedClass.IsSameOrDerivedFrom(other))) && (other.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length > 0))
                        {
                            list.Add(other);
                        }
                    }
                }
            }
            if (list.Count != 0)
            {
                ClassScope[] array = new ClassScope[list.Count];
                list.CopyTo(array);
                Array.Sort<ClassScope>(array);
                result.AddRange(array);
            }
        }

        internal virtual JSVariableField AddFieldOrUseExistingField(string name, object value, FieldAttributes attributeFlags)
        {
            FieldInfo info = (FieldInfo) this.name_table[name];
            if (info is JSVariableField)
            {
                if (!(value is Microsoft.JScript.Missing))
                {
                    ((JSVariableField) info).value = value;
                }
                return (JSVariableField) info;
            }
            if (value is Microsoft.JScript.Missing)
            {
                value = null;
            }
            return this.AddNewField(name, value, attributeFlags);
        }

        internal virtual JSVariableField AddNewField(string name, object value, FieldAttributes attributeFlags)
        {
            JSVariableField field = this.CreateField(name, attributeFlags, value);
            this.name_table[name] = field;
            this.field_table.Add(field);
            return field;
        }

        protected virtual JSVariableField CreateField(string name, FieldAttributes attributeFlags, object value)
        {
            return new JSGlobalField(this, name, value, attributeFlags | FieldAttributes.Static);
        }

        public virtual object GetDefaultThisObject()
        {
            return ((IActivationObject) base.GetParent()).GetDefaultThisObject();
        }

        public virtual FieldInfo GetField(string name, int lexLevel)
        {
            throw new JScriptException(JSError.InternalError);
        }

        public virtual GlobalScope GetGlobalScope()
        {
            return ((IActivationObject) base.GetParent()).GetGlobalScope();
        }

        public virtual FieldInfo GetLocalField(string name)
        {
            return (FieldInfo) this.name_table[name];
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            FieldInfo info = (FieldInfo) this.name_table[name];
            if (info != null)
            {
                return new MemberInfo[] { info };
            }
            if ((base.parent != null) && ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default))
            {
                return ScriptObject.WrapMembers(base.parent.GetMember(name, bindingAttr), base.parent);
            }
            return new MemberInfo[0];
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            int count = this.field_table.Count;
            MemberInfo[] infoArray = new MemberInfo[count];
            for (int i = 0; i < count; i++)
            {
                infoArray[i] = (MemberInfo) this.field_table[i];
            }
            return infoArray;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object GetMemberValue(string name, int lexlevel)
        {
            if (lexlevel > 0)
            {
                FieldInfo info = (FieldInfo) this.name_table[name];
                if (info != null)
                {
                    return info.GetValue(this);
                }
                if (base.parent != null)
                {
                    return ((IActivationObject) base.parent).GetMemberValue(name, lexlevel - 1);
                }
            }
            return Microsoft.JScript.Missing.Value;
        }

        internal virtual string GetName()
        {
            return null;
        }
    }
}

