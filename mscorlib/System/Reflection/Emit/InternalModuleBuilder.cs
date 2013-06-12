namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;

    internal sealed class InternalModuleBuilder : RuntimeModule
    {
        private InternalModuleBuilder()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is InternalModuleBuilder)
            {
                return (this == obj);
            }
            return obj.Equals(this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

