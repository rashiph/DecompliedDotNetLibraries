namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    public sealed class TypedArray : IReflect
    {
        internal IReflect elementType;
        internal int rank;

        public TypedArray(IReflect elementType, int rank)
        {
            this.elementType = elementType;
            this.rank = rank;
        }

        public override bool Equals(object obj)
        {
            if (obj is TypedArray)
            {
                return this.ToString().Equals(obj.ToString());
            }
            Type type = obj as Type;
            if (type == null)
            {
                return false;
            }
            if (!type.IsArray)
            {
                return false;
            }
            if (type.GetArrayRank() != this.rank)
            {
                return false;
            }
            return this.elementType.Equals(type.GetElementType());
        }

        public FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return Typeob.Array.GetField(name, bindingAttr);
        }

        public FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return Typeob.Array.GetFields(bindingAttr);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return Typeob.Array.GetMember(name, bindingAttr);
        }

        public MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return Typeob.Array.GetMembers(bindingAttr);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            return Typeob.Array.GetMethod(name, bindingAttr);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return Typeob.Array.GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        public MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return Typeob.Array.GetMethods(bindingAttr);
        }

        public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return Typeob.Array.GetProperties(bindingAttr);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            return Typeob.Array.GetProperty(name, bindingAttr);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return Typeob.Array.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        public object InvokeMember(string name, BindingFlags flags, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo locale, string[] namedParameters)
        {
            if ((flags & BindingFlags.CreateInstance) == BindingFlags.Default)
            {
                return LateBinding.CallValue(this.elementType, args, true, true, null, null, binder, locale, namedParameters);
            }
            return Typeob.Array.InvokeMember(name, flags, binder, target, args, modifiers, locale, namedParameters);
        }

        internal static string ToRankString(int rank)
        {
            switch (rank)
            {
                case 1:
                    return "[]";

                case 2:
                    return "[,]";

                case 3:
                    return "[,,]";
            }
            StringBuilder builder = new StringBuilder(rank + 1);
            builder.Append('[');
            for (int i = 1; i < rank; i++)
            {
                builder.Append(',');
            }
            builder.Append(']');
            return builder.ToString();
        }

        public override string ToString()
        {
            Type elementType = this.elementType as Type;
            if (elementType != null)
            {
                return (elementType.FullName + ToRankString(this.rank));
            }
            ClassScope scope = this.elementType as ClassScope;
            if (scope != null)
            {
                return (scope.GetFullName() + ToRankString(this.rank));
            }
            TypedArray array = this.elementType as TypedArray;
            if (array != null)
            {
                return (array.ToString() + ToRankString(this.rank));
            }
            return (Microsoft.JScript.Convert.ToType(this.elementType).FullName + ToRankString(this.rank));
        }

        internal Type ToType()
        {
            Type elementType = Microsoft.JScript.Convert.ToType(this.elementType);
            return Microsoft.JScript.Convert.ToType(ToRankString(this.rank), elementType);
        }

        public Type UnderlyingSystemType
        {
            get
            {
                return base.GetType();
            }
        }
    }
}

