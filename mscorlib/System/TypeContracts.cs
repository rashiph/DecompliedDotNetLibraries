namespace System
{
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal abstract class TypeContracts : Type
    {
        protected TypeContracts()
        {
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return Contract.Result<FieldInfo[]>();
        }

        public override Type[] GetInterfaces()
        {
            return Contract.Result<Type[]>();
        }

        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            return Contract.Result<Type>();
        }
    }
}

