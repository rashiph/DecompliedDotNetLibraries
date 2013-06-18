namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Resources;

    internal sealed class CodeDomSerializationProvider : IDesignerSerializationProvider
    {
        private object GetCodeDomSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (currentSerializer != null)
            {
                return null;
            }
            if (objectType == null)
            {
                return PrimitiveCodeDomSerializer.Default;
            }
            if (typeof(IComponent).IsAssignableFrom(objectType))
            {
                return ComponentCodeDomSerializer.Default;
            }
            if (typeof(Enum).IsAssignableFrom(objectType))
            {
                return EnumCodeDomSerializer.Default;
            }
            if ((objectType.IsPrimitive || objectType.IsEnum) || (objectType == typeof(string)))
            {
                return PrimitiveCodeDomSerializer.Default;
            }
            if (typeof(ICollection).IsAssignableFrom(objectType))
            {
                return CollectionCodeDomSerializer.Default;
            }
            if (typeof(IContainer).IsAssignableFrom(objectType))
            {
                return ContainerCodeDomSerializer.Default;
            }
            if (typeof(ResourceManager).IsAssignableFrom(objectType))
            {
                return ResourceCodeDomSerializer.Default;
            }
            return CodeDomSerializer.Default;
        }

        private object GetMemberCodeDomSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (currentSerializer == null)
            {
                if (typeof(PropertyDescriptor).IsAssignableFrom(objectType))
                {
                    return PropertyMemberCodeDomSerializer.Default;
                }
                if (typeof(EventDescriptor).IsAssignableFrom(objectType))
                {
                    return EventMemberCodeDomSerializer.Default;
                }
            }
            return null;
        }

        private object GetTypeCodeDomSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (currentSerializer != null)
            {
                return null;
            }
            if (typeof(IComponent).IsAssignableFrom(objectType))
            {
                return ComponentTypeCodeDomSerializer.Default;
            }
            return TypeCodeDomSerializer.Default;
        }

        object IDesignerSerializationProvider.GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (serializerType == typeof(CodeDomSerializer))
            {
                return this.GetCodeDomSerializer(manager, currentSerializer, objectType, serializerType);
            }
            if (serializerType == typeof(MemberCodeDomSerializer))
            {
                return this.GetMemberCodeDomSerializer(manager, currentSerializer, objectType, serializerType);
            }
            if (serializerType == typeof(TypeCodeDomSerializer))
            {
                return this.GetTypeCodeDomSerializer(manager, currentSerializer, objectType, serializerType);
            }
            return null;
        }
    }
}

