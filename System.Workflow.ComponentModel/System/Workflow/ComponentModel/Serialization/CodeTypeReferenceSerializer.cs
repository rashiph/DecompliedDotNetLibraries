namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class CodeTypeReferenceSerializer : WorkflowMarkupSerializer
    {
        internal const string QualifiedName = "QualifiedName";

        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return (value is CodeTypeReference);
        }

        private static string ConvertTypeReferenceToString(CodeTypeReference reference)
        {
            StringBuilder builder;
            if (reference.ArrayElementType != null)
            {
                builder = new StringBuilder(ConvertTypeReferenceToString(reference.ArrayElementType));
                if (reference.ArrayRank > 0)
                {
                    builder.Append("[");
                    builder.Append(',', reference.ArrayRank - 1);
                    builder.Append("]");
                }
            }
            else
            {
                builder = new StringBuilder(reference.BaseType);
                if ((reference.TypeArguments != null) && (reference.TypeArguments.Count > 0))
                {
                    string str = "[";
                    foreach (CodeTypeReference reference2 in reference.TypeArguments)
                    {
                        builder.Append(str);
                        builder.Append(ConvertTypeReferenceToString(reference2));
                        str = ", ";
                    }
                    builder.Append("]");
                }
            }
            return builder.ToString();
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            CodeTypeReference reference;
            if (!propertyType.IsAssignableFrom(typeof(CodeTypeReference)))
            {
                return null;
            }
            if (string.IsNullOrEmpty(value) || base.IsValidCompactAttributeFormat(value))
            {
                return null;
            }
            try
            {
                Type type = serializationManager.GetType(value);
                if (type != null)
                {
                    reference = new CodeTypeReference(type);
                    reference.UserData["QualifiedName"] = type.AssemblyQualifiedName;
                    return reference;
                }
            }
            catch (Exception)
            {
            }
            reference = new CodeTypeReference(value);
            reference.UserData["QualifiedName"] = value;
            return reference;
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CodeTypeReference reference = value as CodeTypeReference;
            if (reference == null)
            {
                return string.Empty;
            }
            string typeName = ConvertTypeReferenceToString(reference);
            Type type = serializationManager.GetType(typeName);
            if (type == null)
            {
                type = Type.GetType(typeName, false);
                if (type == null)
                {
                    return typeName;
                }
            }
            string assemblyName = null;
            TypeProvider service = serializationManager.GetService(typeof(ITypeProvider)) as TypeProvider;
            if (service != null)
            {
                assemblyName = service.GetAssemblyName(type);
            }
            if (string.IsNullOrEmpty(assemblyName))
            {
                return type.AssemblyQualifiedName;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", new object[] { type.FullName, assemblyName });
        }
    }
}

