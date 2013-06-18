namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;

    public abstract class MemberCodeDomSerializer : CodeDomSerializerBase
    {
        protected MemberCodeDomSerializer()
        {
        }

        public abstract void Serialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor, CodeStatementCollection statements);
        public abstract bool ShouldSerialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor);
    }
}

