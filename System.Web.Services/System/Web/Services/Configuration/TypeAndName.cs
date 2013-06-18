namespace System.Web.Services.Configuration
{
    using System;

    internal class TypeAndName
    {
        public readonly string name;
        public readonly Type type;

        public TypeAndName(string name)
        {
            this.type = Type.GetType(name, true, true);
            this.name = name;
        }

        public TypeAndName(Type type)
        {
            this.type = type;
        }

        public override bool Equals(object comparand)
        {
            return this.type.Equals(((TypeAndName) comparand).type);
        }

        public override int GetHashCode()
        {
            return this.type.GetHashCode();
        }
    }
}

