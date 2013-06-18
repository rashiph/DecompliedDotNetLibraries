namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class WithObject : ScriptObject, IActivationObject
    {
        internal object contained_object;
        internal bool isKnownAtCompileTime;
        private bool isSuperType;

        internal WithObject(ScriptObject parent, object contained_object) : this(parent, contained_object, false)
        {
        }

        internal WithObject(ScriptObject parent, object contained_object, bool isSuperType) : base(parent)
        {
            this.contained_object = contained_object;
            this.isKnownAtCompileTime = ((contained_object is Type) || ((contained_object is ClassScope) && ((ClassScope) contained_object).noExpando)) || ((contained_object is JSObject) && ((JSObject) contained_object).noExpando);
            this.isSuperType = isSuperType;
        }

        public object GetDefaultThisObject()
        {
            return this.contained_object;
        }

        public FieldInfo GetField(string name, int lexLevel)
        {
            if (lexLevel > 0)
            {
                IReflect reflect;
                if (this.contained_object is IReflect)
                {
                    reflect = (IReflect) this.contained_object;
                }
                else
                {
                    reflect = Globals.TypeRefs.ToReferenceContext(this.contained_object.GetType());
                }
                FieldInfo field = reflect.GetField(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (field != null)
                {
                    return new JSWrappedField(field, this.contained_object);
                }
                PropertyInfo property = reflect.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (property != null)
                {
                    return new JSPropertyField(property, this.contained_object);
                }
                if ((base.parent != null) && (lexLevel > 1))
                {
                    field = ((IActivationObject) base.parent).GetField(name, lexLevel - 1);
                    if (field != null)
                    {
                        return new JSWrappedField(field, base.parent);
                    }
                }
            }
            return null;
        }

        public GlobalScope GetGlobalScope()
        {
            return ((IActivationObject) base.GetParent()).GetGlobalScope();
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return this.GetMember(name, bindingAttr, true);
        }

        internal MemberInfo[] GetMember(string name, BindingFlags bindingAttr, bool forceInstanceLookup)
        {
            IReflect reflect;
            Type type = null;
            BindingFlags flags = bindingAttr;
            if ((forceInstanceLookup && this.isSuperType) && ((bindingAttr & BindingFlags.FlattenHierarchy) == BindingFlags.Default))
            {
                flags |= BindingFlags.Instance;
            }
            object obj2 = this.contained_object;
        Label_0020:
            if (obj2 is IReflect)
            {
                reflect = (IReflect) obj2;
                if ((obj2 is Type) && !this.isSuperType)
                {
                    flags &= ~BindingFlags.Instance;
                }
            }
            else
            {
                reflect = type = Globals.TypeRefs.ToReferenceContext(obj2.GetType());
            }
            MemberInfo[] member = reflect.GetMember(name, flags & ~BindingFlags.DeclaredOnly);
            if (member.Length > 0)
            {
                return ScriptObject.WrapMembers(member, obj2);
            }
            if ((obj2 is Type) && !this.isSuperType)
            {
                member = Typeob.Type.GetMember(name, BindingFlags.Public | BindingFlags.Instance);
            }
            if (member.Length > 0)
            {
                return ScriptObject.WrapMembers(member, obj2);
            }
            if ((type != null) && type.IsNestedPublic)
            {
                try
                {
                    new ReflectionPermission(ReflectionPermissionFlag.MemberAccess | ReflectionPermissionFlag.TypeInformation).Assert();
                    FieldInfo field = type.GetField("outer class instance", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        obj2 = field.GetValue(obj2);
                        goto Label_0020;
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            if (member.Length > 0)
            {
                return ScriptObject.WrapMembers(member, obj2);
            }
            return new MemberInfo[0];
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return ((IReflect) this.contained_object).GetMembers(bindingAttr);
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override object GetMemberValue(string name)
        {
            object obj2 = LateBinding.GetMemberValue2(this.contained_object, name);
            if (!(obj2 is Microsoft.JScript.Missing))
            {
                return obj2;
            }
            if (base.parent != null)
            {
                return base.parent.GetMemberValue(name);
            }
            return Microsoft.JScript.Missing.Value;
        }

        [DebuggerHidden, DebuggerStepThrough]
        public object GetMemberValue(string name, int lexlevel)
        {
            if (lexlevel <= 0)
            {
                return Microsoft.JScript.Missing.Value;
            }
            object obj2 = LateBinding.GetMemberValue2(this.contained_object, name);
            if (obj2 != Microsoft.JScript.Missing.Value)
            {
                return obj2;
            }
            return ((IActivationObject) base.parent).GetMemberValue(name, lexlevel - 1);
        }

        FieldInfo IActivationObject.GetLocalField(string name)
        {
            return null;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override void SetMemberValue(string name, object value)
        {
            if (LateBinding.GetMemberValue2(this.contained_object, name) is Microsoft.JScript.Missing)
            {
                base.parent.SetMemberValue(name, value);
            }
            else
            {
                LateBinding.SetMemberValue(this.contained_object, name, value);
            }
        }
    }
}

