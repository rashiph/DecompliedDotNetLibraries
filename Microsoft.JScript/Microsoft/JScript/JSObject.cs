namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices.Expando;

    public class JSObject : ScriptObject, IEnumerable, IExpando, IReflect
    {
        protected ArrayList field_table;
        private bool isASubClass;
        private SimpleHashtable memberCache;
        internal SimpleHashtable name_table;
        internal bool noExpando;
        internal JSObject outer_class_instance;
        private IReflect subClassIR;

        public JSObject() : this(null, false)
        {
            this.noExpando = false;
        }

        internal JSObject(ScriptObject parent) : this(parent, true)
        {
        }

        internal JSObject(ScriptObject parent, bool checkSubType) : base(parent)
        {
            this.memberCache = null;
            this.isASubClass = false;
            this.subClassIR = null;
            if (checkSubType)
            {
                Type type = Globals.TypeRefs.ToReferenceContext(base.GetType());
                if (type != Typeob.JSObject)
                {
                    this.isASubClass = true;
                    this.subClassIR = TypeReflector.GetTypeReflectorFor(type);
                }
            }
            this.noExpando = this.isASubClass;
            this.name_table = null;
            this.field_table = null;
            this.outer_class_instance = null;
        }

        internal JSObject(ScriptObject parent, Type subType) : base(parent)
        {
            this.memberCache = null;
            this.isASubClass = false;
            this.subClassIR = null;
            subType = Globals.TypeRefs.ToReferenceContext(subType);
            if (subType != Typeob.JSObject)
            {
                this.isASubClass = true;
                this.subClassIR = TypeReflector.GetTypeReflectorFor(subType);
            }
            this.noExpando = this.isASubClass;
            this.name_table = null;
            this.field_table = null;
        }

        public FieldInfo AddField(string name)
        {
            if (this.noExpando)
            {
                return null;
            }
            FieldInfo info = (FieldInfo) this.NameTable[name];
            if (info == null)
            {
                info = new JSExpandoField(name);
                this.name_table[name] = info;
                this.field_table.Add(info);
            }
            return info;
        }

        internal override bool DeleteMember(string name)
        {
            FieldInfo info = (FieldInfo) this.NameTable[name];
            if (info != null)
            {
                if (info is JSExpandoField)
                {
                    info.SetValue(this, Microsoft.JScript.Missing.Value);
                    this.name_table.Remove(name);
                    this.field_table.Remove(info);
                    return true;
                }
                if (info is JSPrototypeField)
                {
                    info.SetValue(this, Microsoft.JScript.Missing.Value);
                    return true;
                }
                return false;
            }
            return ((base.parent != null) && LateBinding.DeleteMember(base.parent, name));
        }

        internal virtual string GetClassName()
        {
            return "Object";
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            if (preferred_type == PreferredType.String)
            {
                ScriptFunction memberValue = this.GetMemberValue("toString") as ScriptFunction;
                if (memberValue != null)
                {
                    object ob = memberValue.Call(new object[0], this);
                    if (ob == null)
                    {
                        return ob;
                    }
                    IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(ob);
                    if ((iConvertible != null) && (iConvertible.GetTypeCode() != TypeCode.Object))
                    {
                        return ob;
                    }
                }
                ScriptFunction function2 = this.GetMemberValue("valueOf") as ScriptFunction;
                if (function2 != null)
                {
                    object obj3 = function2.Call(new object[0], this);
                    if (obj3 != null)
                    {
                        IConvertible convertible2 = Microsoft.JScript.Convert.GetIConvertible(obj3);
                        if ((convertible2 == null) || (convertible2.GetTypeCode() == TypeCode.Object))
                        {
                            goto Label_015D;
                        }
                    }
                    return obj3;
                }
            }
            else if (preferred_type == PreferredType.LocaleString)
            {
                ScriptFunction function3 = this.GetMemberValue("toLocaleString") as ScriptFunction;
                if (function3 != null)
                {
                    return function3.Call(new object[0], this);
                }
            }
            else
            {
                if ((preferred_type == PreferredType.Either) && (this is DateObject))
                {
                    return this.GetDefaultValue(PreferredType.String);
                }
                ScriptFunction function4 = this.GetMemberValue("valueOf") as ScriptFunction;
                if (function4 != null)
                {
                    object obj4 = function4.Call(new object[0], this);
                    if (obj4 == null)
                    {
                        return obj4;
                    }
                    IConvertible convertible3 = Microsoft.JScript.Convert.GetIConvertible(obj4);
                    if ((convertible3 != null) && (convertible3.GetTypeCode() != TypeCode.Object))
                    {
                        return obj4;
                    }
                }
                ScriptFunction function5 = this.GetMemberValue("toString") as ScriptFunction;
                if (function5 != null)
                {
                    object obj5 = function5.Call(new object[0], this);
                    if (obj5 == null)
                    {
                        return obj5;
                    }
                    IConvertible convertible4 = Microsoft.JScript.Convert.GetIConvertible(obj5);
                    if ((convertible4 != null) && (convertible4.GetTypeCode() != TypeCode.Object))
                    {
                        return obj5;
                    }
                }
            }
        Label_015D:
            return this;
        }

        private MemberInfo[] GetLocalMember(string name, BindingFlags bindingAttr, bool wrapMembers)
        {
            MemberInfo[] members = null;
            FieldInfo info = (this.name_table == null) ? null : ((FieldInfo) this.name_table[name]);
            if ((info == null) && this.isASubClass)
            {
                if (this.memberCache != null)
                {
                    members = (MemberInfo[]) this.memberCache[name];
                    if (members != null)
                    {
                        return members;
                    }
                }
                bindingAttr &= ~BindingFlags.NonPublic;
                members = this.subClassIR.GetMember(name, bindingAttr);
                if (members.Length == 0)
                {
                    members = this.subClassIR.GetMember(name, (bindingAttr & ~BindingFlags.Instance) | BindingFlags.Static);
                }
                int length = members.Length;
                if (length > 0)
                {
                    int num2 = 0;
                    foreach (MemberInfo info2 in members)
                    {
                        if (IsHiddenMember(info2))
                        {
                            num2++;
                        }
                    }
                    if ((num2 > 0) && (((length != 1) || !(this is ObjectPrototype)) || (name != "ToString")))
                    {
                        MemberInfo[] infoArray2 = new MemberInfo[length - num2];
                        int num3 = 0;
                        foreach (MemberInfo info3 in members)
                        {
                            if (!IsHiddenMember(info3))
                            {
                                infoArray2[num3++] = info3;
                            }
                        }
                        members = infoArray2;
                    }
                }
                if (((members == null) || (members.Length == 0)) && (((bindingAttr & BindingFlags.Public) != BindingFlags.Default) && ((bindingAttr & BindingFlags.Instance) != BindingFlags.Default)))
                {
                    BindingFlags flags = ((bindingAttr & BindingFlags.IgnoreCase) | BindingFlags.Public) | BindingFlags.Instance;
                    if (this is StringObject)
                    {
                        members = TypeReflector.GetTypeReflectorFor(Typeob.String).GetMember(name, flags);
                    }
                    else if (this is NumberObject)
                    {
                        members = TypeReflector.GetTypeReflectorFor(((NumberObject) this).baseType).GetMember(name, flags);
                    }
                    else if (this is BooleanObject)
                    {
                        members = TypeReflector.GetTypeReflectorFor(Typeob.Boolean).GetMember(name, flags);
                    }
                    else if (this is StringConstructor)
                    {
                        members = TypeReflector.GetTypeReflectorFor(Typeob.String).GetMember(name, (flags | BindingFlags.Static) & ~BindingFlags.Instance);
                    }
                    else if (this is BooleanConstructor)
                    {
                        members = TypeReflector.GetTypeReflectorFor(Typeob.Boolean).GetMember(name, (flags | BindingFlags.Static) & ~BindingFlags.Instance);
                    }
                    else if (this is ArrayWrapper)
                    {
                        members = TypeReflector.GetTypeReflectorFor(Typeob.Array).GetMember(name, flags);
                    }
                }
                if ((members != null) && (members.Length > 0))
                {
                    if (wrapMembers)
                    {
                        members = ScriptObject.WrapMembers(members, this);
                    }
                    if (this.memberCache == null)
                    {
                        this.memberCache = new SimpleHashtable(0x20);
                    }
                    this.memberCache[name] = members;
                    return members;
                }
            }
            if (((bindingAttr & BindingFlags.IgnoreCase) != BindingFlags.Default) && ((members == null) || (members.Length == 0)))
            {
                members = null;
                IDictionaryEnumerator enumerator = this.name_table.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (string.Compare(enumerator.Key.ToString(), name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        info = (FieldInfo) enumerator.Value;
                        break;
                    }
                }
            }
            if (info != null)
            {
                return new MemberInfo[] { info };
            }
            if (members == null)
            {
                members = new MemberInfo[0];
            }
            return members;
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return this.GetMember(name, bindingAttr, false);
        }

        private MemberInfo[] GetMember(string name, BindingFlags bindingAttr, bool wrapMembers)
        {
            MemberInfo[] members = this.GetLocalMember(name, bindingAttr, wrapMembers);
            if (members.Length <= 0)
            {
                if (base.parent == null)
                {
                    return new MemberInfo[0];
                }
                if (base.parent is JSObject)
                {
                    members = ((JSObject) base.parent).GetMember(name, bindingAttr, true);
                    wrapMembers = false;
                }
                else
                {
                    members = base.parent.GetMember(name, bindingAttr);
                }
                foreach (MemberInfo info in members)
                {
                    if (info.MemberType == MemberTypes.Field)
                    {
                        FieldInfo info2 = (FieldInfo) info;
                        JSMemberField field = info as JSMemberField;
                        if (field != null)
                        {
                            if (!field.IsStatic)
                            {
                                JSGlobalField field2 = new JSGlobalField(this, name, field.value, FieldAttributes.Public);
                                this.NameTable[name] = field2;
                                this.field_table.Add(field2);
                                info2 = field;
                            }
                        }
                        else
                        {
                            info2 = new JSPrototypeField(base.parent, (FieldInfo) info);
                            if (!this.noExpando)
                            {
                                this.NameTable[name] = info2;
                                this.field_table.Add(info2);
                            }
                        }
                        return new MemberInfo[] { info2 };
                    }
                    if (!this.noExpando && (info.MemberType == MemberTypes.Method))
                    {
                        FieldInfo info3 = new JSPrototypeField(base.parent, new JSGlobalField(this, name, LateBinding.GetMemberValue(base.parent, name, null, members), FieldAttributes.InitOnly | FieldAttributes.Public));
                        this.NameTable[name] = info3;
                        this.field_table.Add(info3);
                        return new MemberInfo[] { info3 };
                    }
                }
                if (wrapMembers)
                {
                    return ScriptObject.WrapMembers(members, base.parent);
                }
            }
            return members;
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            MemberInfoList list = new MemberInfoList();
            SimpleHashtable hashtable = new SimpleHashtable(0x20);
            if (!this.noExpando && (this.field_table != null))
            {
                IEnumerator enumerator = this.field_table.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    FieldInfo current = (FieldInfo) enumerator.Current;
                    list.Add(current);
                    hashtable[current.Name] = current;
                }
            }
            if (this.isASubClass)
            {
                MemberInfo[] members = base.GetType().GetMembers(bindingAttr & ~BindingFlags.NonPublic);
                int index = 0;
                int length = members.Length;
                while (index < length)
                {
                    MemberInfo elem = members[index];
                    if (!elem.DeclaringType.IsAssignableFrom(Typeob.JSObject) && (hashtable[elem.Name] == null))
                    {
                        MethodInfo info3 = elem as MethodInfo;
                        if ((info3 == null) || !info3.IsSpecialName)
                        {
                            list.Add(elem);
                            hashtable[elem.Name] = elem;
                        }
                    }
                    index++;
                }
            }
            if (base.parent != null)
            {
                SimpleHashtable wrappedMemberCache = base.parent.wrappedMemberCache;
                if (wrappedMemberCache == null)
                {
                    wrappedMemberCache = base.parent.wrappedMemberCache = new SimpleHashtable(8);
                }
                MemberInfo[] infoArray2 = ScriptObject.WrapMembers(base.parent.GetMembers(bindingAttr & ~BindingFlags.NonPublic), base.parent, wrappedMemberCache);
                int num3 = 0;
                int num4 = infoArray2.Length;
                while (num3 < num4)
                {
                    MemberInfo info4 = infoArray2[num3];
                    if (hashtable[info4.Name] == null)
                    {
                        list.Add(info4);
                    }
                    num3++;
                }
            }
            return list.ToArray();
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object GetMemberValue(string name)
        {
            FieldInfo field = (FieldInfo) this.NameTable[name];
            if ((field == null) && this.isASubClass)
            {
                field = this.subClassIR.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (field != null)
                {
                    if (field.DeclaringType == Typeob.ScriptObject)
                    {
                        return Microsoft.JScript.Missing.Value;
                    }
                }
                else
                {
                    PropertyInfo property = this.subClassIR.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                    if ((property != null) && !property.DeclaringType.IsAssignableFrom(Typeob.JSObject))
                    {
                        return JSProperty.GetGetMethod(property, false).Invoke(this, BindingFlags.SuppressChangeType, null, null, null);
                    }
                    try
                    {
                        MethodInfo method = this.subClassIR.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
                        if (method != null)
                        {
                            Type declaringType = method.DeclaringType;
                            if (((declaringType != Typeob.JSObject) && (declaringType != Typeob.ScriptObject)) && (declaringType != Typeob.Object))
                            {
                                return new BuiltinFunction(this, method);
                            }
                        }
                    }
                    catch (AmbiguousMatchException)
                    {
                    }
                }
            }
            if (field != null)
            {
                return field.GetValue(this);
            }
            if (base.parent != null)
            {
                return base.parent.GetMemberValue(name);
            }
            return Microsoft.JScript.Missing.Value;
        }

        internal override void GetPropertyEnumerator(ArrayList enums, ArrayList objects)
        {
            if (this.field_table == null)
            {
                this.field_table = new ArrayList();
            }
            enums.Add(new ListEnumerator(this.field_table));
            objects.Add(this);
            if (base.parent != null)
            {
                base.parent.GetPropertyEnumerator(enums, objects);
            }
        }

        internal override object GetValueAtIndex(uint index)
        {
            string name = System.Convert.ToString(index, CultureInfo.InvariantCulture);
            FieldInfo info = (FieldInfo) this.NameTable[name];
            if (info != null)
            {
                return info.GetValue(this);
            }
            object memberValue = null;
            if (base.parent != null)
            {
                memberValue = base.parent.GetMemberValue(name);
            }
            else
            {
                memberValue = Microsoft.JScript.Missing.Value;
            }
            if ((this is StringObject) && (memberValue == Microsoft.JScript.Missing.Value))
            {
                string str2 = ((StringObject) this).value;
                if (index < str2.Length)
                {
                    return str2[(int) index];
                }
            }
            return memberValue;
        }

        private static bool IsHiddenMember(MemberInfo mem)
        {
            Type declaringType = mem.DeclaringType;
            if ((!(declaringType == Typeob.JSObject) && !(declaringType == Typeob.ScriptObject)) && (!(declaringType == Typeob.ArrayWrapper) || !(mem.Name != "length")))
            {
                return false;
            }
            return true;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override void SetMemberValue(string name, object value)
        {
            this.SetMemberValue2(name, value);
        }

        public void SetMemberValue2(string name, object value)
        {
            FieldInfo field = (FieldInfo) this.NameTable[name];
            if ((field == null) && this.isASubClass)
            {
                field = base.GetType().GetField(name);
            }
            if (field == null)
            {
                if (this.noExpando)
                {
                    return;
                }
                field = new JSExpandoField(name);
                this.name_table[name] = field;
                this.field_table.Add(field);
            }
            if (!field.IsInitOnly && !field.IsLiteral)
            {
                field.SetValue(this, value);
            }
        }

        internal override void SetValueAtIndex(uint index, object value)
        {
            this.SetMemberValue(System.Convert.ToString(index, CultureInfo.InvariantCulture), value);
        }

        internal virtual void SwapValues(uint left, uint right)
        {
            string key = System.Convert.ToString(left, CultureInfo.InvariantCulture);
            string str2 = System.Convert.ToString(right, CultureInfo.InvariantCulture);
            FieldInfo info = (FieldInfo) this.NameTable[key];
            FieldInfo info2 = (FieldInfo) this.name_table[str2];
            if (info == null)
            {
                if (info2 != null)
                {
                    this.name_table[key] = info2;
                    this.name_table.Remove(str2);
                }
            }
            else if (info2 == null)
            {
                this.name_table[str2] = info;
                this.name_table.Remove(key);
            }
            else
            {
                this.name_table[key] = info2;
                this.name_table[str2] = info;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ForIn.JScriptGetEnumerator(this);
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

        public override string ToString()
        {
            return Microsoft.JScript.Convert.ToString(this);
        }

        internal SimpleHashtable NameTable
        {
            get
            {
                SimpleHashtable hashtable = this.name_table;
                if (hashtable == null)
                {
                    this.name_table = hashtable = new SimpleHashtable(0x10);
                    this.field_table = new ArrayList();
                }
                return hashtable;
            }
        }
    }
}

