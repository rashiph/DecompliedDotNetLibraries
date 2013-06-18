namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSMemberField : JSVariableField
    {
        private object expandoValue;
        internal JSMemberField nextOverload;

        internal JSMemberField(ClassScope obj, string name, object value, FieldAttributes attributeFlags) : base(name, obj, attributeFlags)
        {
            base.value = value;
            this.nextOverload = null;
        }

        internal JSMemberField AddOverload(FunctionObject func, FieldAttributes attributeFlags)
        {
            JSMemberField nextOverload = this;
            while (nextOverload.nextOverload != null)
            {
                nextOverload = nextOverload.nextOverload;
            }
            JSMemberField field2 = nextOverload.nextOverload = new JSMemberField((ClassScope) base.obj, this.Name, func, attributeFlags);
            field2.type = base.type;
            return field2;
        }

        internal void AddOverloadedMembers(MemberInfoList mems, ClassScope scope, BindingFlags attrs)
        {
            for (JSMemberField field = this; field != null; field = field.nextOverload)
            {
                MethodInfo asMethod = field.GetAsMethod(scope);
                if (asMethod.IsStatic)
                {
                    if ((attrs & BindingFlags.Static) != BindingFlags.Default)
                    {
                        goto Label_0020;
                    }
                    continue;
                }
                if ((attrs & BindingFlags.Instance) == BindingFlags.Default)
                {
                    continue;
                }
            Label_0020:
                if (asMethod.IsPublic)
                {
                    if ((attrs & BindingFlags.Public) != BindingFlags.Default)
                    {
                        goto Label_0036;
                    }
                    continue;
                }
                if ((attrs & BindingFlags.NonPublic) == BindingFlags.Default)
                {
                    continue;
                }
            Label_0036:
                mems.Add(asMethod);
            }
            if (((attrs & BindingFlags.DeclaredOnly) == BindingFlags.Default) || ((attrs & BindingFlags.FlattenHierarchy) != BindingFlags.Default))
            {
                foreach (MemberInfo info2 in scope.GetSuperType().GetMember(this.Name, attrs & ~BindingFlags.DeclaredOnly))
                {
                    if (info2.MemberType == MemberTypes.Method)
                    {
                        mems.Add(info2);
                    }
                }
            }
        }

        internal void CheckOverloadsForDuplicates()
        {
            for (JSMemberField field = this; field != null; field = field.nextOverload)
            {
                FunctionObject obj2 = field.value as FunctionObject;
                if (obj2 == null)
                {
                    return;
                }
                for (JSMemberField field2 = field.nextOverload; field2 != null; field2 = field2.nextOverload)
                {
                    FunctionObject obj3 = (FunctionObject) field2.value;
                    if ((obj3.implementedIface == obj2.implementedIface) && Class.ParametersMatch(obj3.parameter_declarations, obj2.parameter_declarations))
                    {
                        obj2.funcContext.HandleError(JSError.DuplicateMethod);
                        obj3.funcContext.HandleError(JSError.DuplicateMethod);
                        break;
                    }
                }
            }
        }

        internal ConstructorInfo[] GetAsConstructors(object proto)
        {
            JSMemberField nextOverload = this;
            int num = 0;
            while (nextOverload != null)
            {
                nextOverload = nextOverload.nextOverload;
                num++;
            }
            ConstructorInfo[] infoArray = new ConstructorInfo[num];
            nextOverload = this;
            num = 0;
            while (nextOverload != null)
            {
                FunctionObject cons = (FunctionObject) nextOverload.value;
                cons.isConstructor = true;
                cons.proto = proto;
                infoArray[num++] = new JSConstructor(cons);
                nextOverload = nextOverload.nextOverload;
            }
            return infoArray;
        }

        internal override object GetMetaData()
        {
            if (base.metaData == null)
            {
                ((ClassScope) base.obj).GetTypeBuilderOrEnumBuilder();
            }
            return base.metaData;
        }

        public override object GetValue(object obj)
        {
            if (obj is StackFrame)
            {
                return this.GetValue(((StackFrame) obj).closureInstance, (StackFrame) obj);
            }
            if (obj is ScriptObject)
            {
                return this.GetValue(obj, (ScriptObject) obj);
            }
            return this.GetValue(obj, null);
        }

        private object GetValue(object obj, ScriptObject scope)
        {
            if (base.IsStatic || base.IsLiteral)
            {
                return base.value;
            }
            if (base.obj != obj)
            {
                JSObject obj2 = obj as JSObject;
                if (obj2 != null)
                {
                    FieldInfo field = obj2.GetField(this.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (field != null)
                    {
                        return field.GetValue(obj);
                    }
                    if (obj2.outer_class_instance != null)
                    {
                        return this.GetValue(obj2.outer_class_instance, null);
                    }
                }
                throw new TargetException();
            }
            if (base.IsPublic || ((scope != null) && this.IsAccessibleFrom(scope)))
            {
                return base.value;
            }
            if (((JSObject) base.obj).noExpando)
            {
                throw new JScriptException(JSError.NotAccessible, new Context(new DocumentContext("", null), this.Name));
            }
            return this.expandoValue;
        }

        internal bool IsAccessibleFrom(ScriptObject scope)
        {
            while ((scope != null) && !(scope is ClassScope))
            {
                scope = scope.GetParent();
            }
            ClassScope other = null;
            if (base.obj is ClassScope)
            {
                other = (ClassScope) base.obj;
            }
            else
            {
                other = (ClassScope) base.obj.GetParent();
            }
            if (base.IsPrivate)
            {
                if (scope == null)
                {
                    return false;
                }
                if (scope != other)
                {
                    return ((ClassScope) scope).IsNestedIn(other, base.IsStatic);
                }
                return true;
            }
            if (base.IsFamily)
            {
                if (scope == null)
                {
                    return false;
                }
                if (!((ClassScope) scope).IsSameOrDerivedFrom(other))
                {
                    return ((ClassScope) scope).IsNestedIn(other, base.IsStatic);
                }
                return true;
            }
            if ((base.IsFamilyOrAssembly && (scope != null)) && (((ClassScope) scope).IsSameOrDerivedFrom(other) || ((ClassScope) scope).IsNestedIn(other, base.IsStatic)))
            {
                return true;
            }
            if (scope == null)
            {
                return (other.GetPackage() == null);
            }
            return (other.GetPackage() == ((ClassScope) scope).GetPackage());
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo locale)
        {
            if (obj is StackFrame)
            {
                this.SetValue(((StackFrame) obj).closureInstance, value, invokeAttr, binder, locale, (StackFrame) obj);
            }
            else if (obj is ScriptObject)
            {
                this.SetValue(obj, value, invokeAttr, binder, locale, (ScriptObject) obj);
            }
            else
            {
                this.SetValue(obj, value, invokeAttr, binder, locale, null);
            }
        }

        private void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo locale, ScriptObject scope)
        {
            if (base.IsStatic || base.IsLiteral)
            {
                if ((base.IsLiteral || base.IsInitOnly) && !(base.value is Microsoft.JScript.Missing))
                {
                    throw new JScriptException(JSError.AssignmentToReadOnly);
                }
            }
            else
            {
                if (base.obj != obj)
                {
                    if (obj is JSObject)
                    {
                        FieldInfo field = ((JSObject) obj).GetField(this.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (field != null)
                        {
                            field.SetValue(obj, value, invokeAttr, binder, locale);
                            return;
                        }
                    }
                    throw new TargetException();
                }
                if (!base.IsPublic && ((scope == null) || !this.IsAccessibleFrom(scope)))
                {
                    if (((JSObject) base.obj).noExpando)
                    {
                        throw new JScriptException(JSError.NotAccessible, new Context(new DocumentContext("", null), this.Name));
                    }
                    this.expandoValue = value;
                    return;
                }
            }
            if (base.type != null)
            {
                base.value = Microsoft.JScript.Convert.Coerce(value, base.type);
            }
            else
            {
                base.value = value;
            }
        }

        public override FieldAttributes Attributes
        {
            get
            {
                if ((base.attributeFlags & FieldAttributes.Literal) != FieldAttributes.PrivateScope)
                {
                    if ((base.value is FunctionObject) && !((FunctionObject) base.value).isStatic)
                    {
                        return base.attributeFlags;
                    }
                    if (!(base.value is JSProperty))
                    {
                        return base.attributeFlags;
                    }
                    JSProperty property = (JSProperty) base.value;
                    if ((property.getter != null) && !property.getter.IsStatic)
                    {
                        return base.attributeFlags;
                    }
                    if ((property.setter == null) || property.setter.IsStatic)
                    {
                        return (base.attributeFlags | FieldAttributes.Static);
                    }
                }
                return base.attributeFlags;
            }
        }
    }
}

