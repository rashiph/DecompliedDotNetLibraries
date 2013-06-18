namespace System.ServiceModel.Channels
{
    using System;
    using System.CodeDom;
    using System.ServiceModel;

    internal static class SecurityAttributeGenerationHelper
    {
        public static void CreateOrOverridePropertyDeclaration<V>(CodeAttributeDeclaration attribute, string propertyName, V value)
        {
            CodeExpression expression;
            if (attribute == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attribute");
            }
            if (propertyName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("propertyName");
            }
            if (value is TimeSpan)
            {
                CodeExpression[] parameters = new CodeExpression[1];
                TimeSpan span = (TimeSpan) value;
                parameters[0] = new CodePrimitiveExpression(span.Ticks);
                expression = new CodeObjectCreateExpression(typeof(TimeSpan), parameters);
            }
            else if (value is Enum)
            {
                expression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(V)), value.ToString());
            }
            else
            {
                expression = new CodePrimitiveExpression(value);
            }
            CodeAttributeArgument argument = TryGetAttributeProperty(attribute, propertyName);
            if (argument == null)
            {
                argument = new CodeAttributeArgument(propertyName, expression);
                attribute.Arguments.Add(argument);
            }
            else
            {
                argument.Value = expression;
            }
        }

        public static CodeAttributeDeclaration FindOrCreateAttributeDeclaration<T>(CodeAttributeDeclarationCollection attributes) where T: Attribute
        {
            if (attributes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributes");
            }
            CodeTypeReference attributeType = new CodeTypeReference(typeof(T));
            foreach (CodeAttributeDeclaration declaration in attributes)
            {
                if (declaration.AttributeType.BaseType == attributeType.BaseType)
                {
                    return declaration;
                }
            }
            CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(attributeType);
            attributes.Add(declaration2);
            return declaration2;
        }

        public static CodeAttributeArgument TryGetAttributeProperty(CodeAttributeDeclaration attribute, string propertyName)
        {
            if (attribute == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attribute");
            }
            if (propertyName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("propertyName");
            }
            foreach (CodeAttributeArgument argument in attribute.Arguments)
            {
                if (argument.Name == propertyName)
                {
                    return argument;
                }
            }
            return null;
        }
    }
}

