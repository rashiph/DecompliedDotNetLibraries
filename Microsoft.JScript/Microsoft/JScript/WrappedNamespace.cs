namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;

    internal sealed class WrappedNamespace : ActivationObject
    {
        internal string name;

        internal WrappedNamespace(string name, VsaEngine engine) : this(name, engine, true)
        {
        }

        internal WrappedNamespace(string name, VsaEngine engine, bool AddReferences) : base(null)
        {
            this.name = name;
            base.engine = engine;
            base.isKnownAtCompileTime = true;
            if ((name.Length > 0) && AddReferences)
            {
                engine.TryToAddImplicitAssemblyReference(name);
            }
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            FieldInfo info = (FieldInfo) base.name_table[name];
            if (info != null)
            {
                return new MemberInfo[] { info };
            }
            FieldAttributes literal = FieldAttributes.Literal;
            string className = ((this.name == null) || (this.name.Length == 0)) ? name : (this.name + "." + name);
            object type = null;
            if ((this.name != null) && (this.name.Length > 0))
            {
                type = base.engine.GetClass(className);
            }
            if (type == null)
            {
                type = base.engine.GetType(className);
                if ((type != null) && !((Type) type).IsPublic)
                {
                    if ((bindingAttr & BindingFlags.NonPublic) == BindingFlags.Default)
                    {
                        type = null;
                    }
                    else
                    {
                        literal |= FieldAttributes.Private;
                    }
                }
            }
            else if ((((ClassScope) type).owner.attributes & TypeAttributes.Public) == TypeAttributes.AnsiClass)
            {
                if ((bindingAttr & BindingFlags.NonPublic) == BindingFlags.Default)
                {
                    type = null;
                }
                else
                {
                    literal |= FieldAttributes.Private;
                }
            }
            if (type == null)
            {
                if ((base.parent != null) && ((bindingAttr & BindingFlags.DeclaredOnly) == BindingFlags.Default))
                {
                    return base.parent.GetMember(name, bindingAttr);
                }
                return new MemberInfo[0];
            }
            JSGlobalField field = (JSGlobalField) this.CreateField(name, literal, type);
            if (base.engine.doFast)
            {
                field.type = new TypeExpression(new ConstantWrapper(Typeob.Type, null));
            }
            base.name_table[name] = field;
            base.field_table.Add(field);
            return new MemberInfo[] { field };
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}

