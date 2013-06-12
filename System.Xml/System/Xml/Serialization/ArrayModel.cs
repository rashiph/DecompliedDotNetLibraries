namespace System.Xml.Serialization
{
    using System;

    internal class ArrayModel : TypeModel
    {
        internal ArrayModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope)
        {
        }

        internal TypeModel Element
        {
            get
            {
                return base.ModelScope.GetTypeModel(TypeScope.GetArrayElementType(base.Type, null));
            }
        }
    }
}

