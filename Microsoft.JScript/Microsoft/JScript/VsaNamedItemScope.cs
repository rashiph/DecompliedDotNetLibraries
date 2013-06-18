namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Diagnostics;
    using System.Reflection;

    internal sealed class VsaNamedItemScope : ScriptObject, IActivationObject
    {
        internal object namedItem;
        private SimpleHashtable namedItemWrappedMemberCache;
        private bool recursive;
        private IReflect reflectObj;

        internal VsaNamedItemScope(object hostObject, ScriptObject parent, VsaEngine engine) : base(parent)
        {
            this.namedItem = hostObject;
            this.reflectObj = hostObject as IReflect;
            if (this.reflectObj == null)
            {
                this.reflectObj = Globals.TypeRefs.ToReferenceContext(hostObject.GetType());
            }
            this.recursive = false;
            base.engine = engine;
        }

        private static MemberInfo[] GetAndWrapMember(IReflect reflect, object namedItem, string name, BindingFlags bindingAttr)
        {
            PropertyInfo property = reflect.GetProperty(name, bindingAttr);
            if (property != null)
            {
                MethodInfo getMethod = JSProperty.GetGetMethod(property, false);
                MethodInfo setMethod = JSProperty.GetSetMethod(property, false);
                if (((getMethod != null) && !getMethod.IsStatic) || ((setMethod != null) && !setMethod.IsStatic))
                {
                    MethodInfo method = reflect.GetMethod(name, bindingAttr);
                    if ((method != null) && !method.IsStatic)
                    {
                        return new MemberInfo[] { new JSWrappedPropertyAndMethod(property, method, namedItem) };
                    }
                }
            }
            MemberInfo[] member = reflect.GetMember(name, bindingAttr);
            if ((member != null) && (member.Length > 0))
            {
                return ScriptObject.WrapMembers(member, namedItem);
            }
            return null;
        }

        public object GetDefaultThisObject()
        {
            return ((IActivationObject) base.GetParent()).GetDefaultThisObject();
        }

        public FieldInfo GetField(string name, int lexLevel)
        {
            throw new JScriptException(JSError.InternalError);
        }

        public GlobalScope GetGlobalScope()
        {
            return ((IActivationObject) base.GetParent()).GetGlobalScope();
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            MemberInfo[] infoArray = null;
            if (!this.recursive && (this.reflectObj != null))
            {
                this.recursive = true;
                try
                {
                    ISite2 site;
                    if (!this.reflectObj.GetType().IsCOMObject || ((site = base.engine.Site as ISite2) == null))
                    {
                        infoArray = ScriptObject.WrapMembers(this.reflectObj.GetMember(name, bindingAttr), this.namedItem);
                    }
                    else
                    {
                        infoArray = GetAndWrapMember(this.reflectObj, this.namedItem, name, bindingAttr);
                        if (infoArray == null)
                        {
                            object[] parentChain = site.GetParentChain(this.reflectObj);
                            if (parentChain != null)
                            {
                                int length = parentChain.Length;
                                for (int i = 0; i < length; i++)
                                {
                                    IReflect reflect = parentChain[i] as IReflect;
                                    if ((reflect != null) && ((infoArray = GetAndWrapMember(reflect, reflect, name, bindingAttr)) != null))
                                    {
                                        goto Label_00C3;
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.recursive = false;
                }
            }
        Label_00C3:
            if (infoArray != null)
            {
                return infoArray;
            }
            return new MemberInfo[0];
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            MemberInfo[] members = null;
            if (!this.recursive)
            {
                this.recursive = true;
                try
                {
                    members = this.reflectObj.GetMembers(bindingAttr);
                    if (members == null)
                    {
                        return members;
                    }
                    if (members.Length > 0)
                    {
                        SimpleHashtable namedItemWrappedMemberCache = this.namedItemWrappedMemberCache;
                        if (namedItemWrappedMemberCache == null)
                        {
                            namedItemWrappedMemberCache = this.namedItemWrappedMemberCache = new SimpleHashtable(0x10);
                        }
                        return ScriptObject.WrapMembers(members, this.namedItem, namedItemWrappedMemberCache);
                    }
                    members = null;
                }
                finally
                {
                    this.recursive = false;
                }
            }
            return members;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override object GetMemberValue(string name)
        {
            object memberValue = Microsoft.JScript.Missing.Value;
            if (!this.recursive)
            {
                this.recursive = true;
                try
                {
                    FieldInfo field = this.reflectObj.GetField(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (field == null)
                    {
                        PropertyInfo property = this.reflectObj.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (property != null)
                        {
                            memberValue = JSProperty.GetValue(property, this.namedItem, null);
                        }
                    }
                    else
                    {
                        memberValue = field.GetValue(this.namedItem);
                    }
                    if ((memberValue is Microsoft.JScript.Missing) && (base.parent != null))
                    {
                        memberValue = base.parent.GetMemberValue(name);
                    }
                }
                finally
                {
                    this.recursive = false;
                }
            }
            return memberValue;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object GetMemberValue(string name, int lexlevel)
        {
            if (lexlevel <= 0)
            {
                return Microsoft.JScript.Missing.Value;
            }
            object memberValue = LateBinding.GetMemberValue(this.namedItem, name);
            if (memberValue is Microsoft.JScript.Missing)
            {
                return ((IActivationObject) base.parent).GetMemberValue(name, lexlevel - 1);
            }
            return memberValue;
        }

        FieldInfo IActivationObject.GetLocalField(string name)
        {
            return null;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override void SetMemberValue(string name, object value)
        {
            bool flag = false;
            if (!this.recursive)
            {
                this.recursive = true;
                try
                {
                    FieldInfo field = this.reflectObj.GetField(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (field == null)
                    {
                        PropertyInfo property = this.reflectObj.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (property != null)
                        {
                            JSProperty.SetValue(property, this.namedItem, value, null);
                            flag = true;
                        }
                    }
                    else
                    {
                        field.SetValue(this.namedItem, value);
                        flag = true;
                    }
                    if (!flag && (base.parent != null))
                    {
                        base.parent.SetMemberValue(name, value);
                    }
                }
                finally
                {
                    this.recursive = false;
                }
            }
        }
    }
}

