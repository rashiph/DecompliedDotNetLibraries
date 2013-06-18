namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Text;

    internal class PropertyReferenceSerializer
    {
        private const string appConfigPrefix = "AppConfig";
        private const string applicationSettingsPrefix = "ApplicationSettings";

        private PropertyReferenceSerializer()
        {
        }

        internal static CodePropertyReferenceExpression Deserialize(string expressionString)
        {
            string[] expressionParts = expressionString.Split(new char[] { '.' });
            if ((expressionParts != null) && (expressionParts.Length > 0))
            {
                if (StringUtil.EqualValue(expressionParts[0], "ApplicationSettings"))
                {
                    return DeserializeApplicationSettingsExpression(expressionParts);
                }
                if (StringUtil.EqualValue(expressionParts[0], "AppConfig"))
                {
                    return DeserializeAppConfigExpression(expressionParts);
                }
            }
            UTF8Encoding encoding = new UTF8Encoding();
            MemoryStream serializationStream = new MemoryStream(encoding.GetBytes(expressionString));
            IFormatter formatter = new SoapFormatter();
            return (CodePropertyReferenceExpression) formatter.Deserialize(serializationStream);
        }

        private static CodePropertyReferenceExpression DeserializeAppConfigExpression(string[] expressionParts)
        {
            int index = expressionParts.Length - 1;
            CodePropertyReferenceExpression expression = new CodePropertyReferenceExpression {
                PropertyName = expressionParts[index]
            };
            index--;
            CodeIndexerExpression expression2 = new CodeIndexerExpression();
            expression.TargetObject = expression2;
            expression2.Indices.Add(new CodePrimitiveExpression(expressionParts[index]));
            index--;
            CodePropertyReferenceExpression expression3 = new CodePropertyReferenceExpression();
            expression2.TargetObject = expression3;
            expression3.PropertyName = expressionParts[index];
            index--;
            CodeTypeReferenceExpression expression4 = new CodeTypeReferenceExpression();
            expression3.TargetObject = expression4;
            expression4.Type.Options = (CodeTypeReferenceOptions) Enum.Parse(typeof(CodeTypeReferenceOptions), expressionParts[index]);
            index--;
            expression4.Type.BaseType = expressionParts[index];
            index--;
            while (index > 0)
            {
                expression4.Type.BaseType = expressionParts[index] + "." + expression4.Type.BaseType;
                index--;
            }
            return expression;
        }

        private static CodePropertyReferenceExpression DeserializeApplicationSettingsExpression(string[] expressionParts)
        {
            int index = expressionParts.Length - 1;
            CodePropertyReferenceExpression expression = new CodePropertyReferenceExpression {
                PropertyName = expressionParts[index]
            };
            index--;
            CodePropertyReferenceExpression expression2 = new CodePropertyReferenceExpression();
            expression.TargetObject = expression2;
            expression2.PropertyName = expressionParts[index];
            index--;
            CodeTypeReferenceExpression expression3 = new CodeTypeReferenceExpression();
            expression2.TargetObject = expression3;
            expression3.Type.Options = (CodeTypeReferenceOptions) Enum.Parse(typeof(CodeTypeReferenceOptions), expressionParts[index]);
            index--;
            expression3.Type.BaseType = expressionParts[index];
            index--;
            while (index > 0)
            {
                expression3.Type.BaseType = expressionParts[index] + "." + expression3.Type.BaseType;
                index--;
            }
            return expression;
        }

        private static bool IsWellKnownAppConfigExpression(CodePropertyReferenceExpression expression)
        {
            if ((expression.UserData != null) && (expression.UserData.Count > 0))
            {
                return false;
            }
            if (!(expression.TargetObject is CodeIndexerExpression))
            {
                return false;
            }
            CodeIndexerExpression targetObject = (CodeIndexerExpression) expression.TargetObject;
            if ((targetObject.UserData != null) && (targetObject.UserData.Count > 0))
            {
                return false;
            }
            if (((targetObject.Indices == null) || (targetObject.Indices.Count != 1)) || !(targetObject.Indices[0] is CodePrimitiveExpression))
            {
                return false;
            }
            if (!(((CodePrimitiveExpression) targetObject.Indices[0]).Value is string))
            {
                return false;
            }
            if (!(targetObject.TargetObject is CodePropertyReferenceExpression))
            {
                return false;
            }
            CodePropertyReferenceExpression expression3 = (CodePropertyReferenceExpression) targetObject.TargetObject;
            if ((expression3.UserData != null) && (expression3.UserData.Count > 0))
            {
                return false;
            }
            if (!(expression3.TargetObject is CodeTypeReferenceExpression))
            {
                return false;
            }
            CodeTypeReferenceExpression expression4 = (CodeTypeReferenceExpression) expression3.TargetObject;
            if ((expression4.UserData != null) && (expression4.UserData.Count > 0))
            {
                return false;
            }
            CodeTypeReference type = expression4.Type;
            if ((type.UserData != null) && (type.UserData.Count > 0))
            {
                return false;
            }
            if ((type.TypeArguments != null) && (type.TypeArguments.Count > 0))
            {
                return false;
            }
            return ((type.ArrayElementType == null) && (type.ArrayRank <= 0));
        }

        private static bool IsWellKnownApplicationSettingsExpression(CodePropertyReferenceExpression expression)
        {
            if ((expression.UserData != null) && (expression.UserData.Count > 0))
            {
                return false;
            }
            if (!(expression.TargetObject is CodePropertyReferenceExpression))
            {
                return false;
            }
            CodePropertyReferenceExpression targetObject = (CodePropertyReferenceExpression) expression.TargetObject;
            if ((targetObject.UserData != null) && (targetObject.UserData.Count > 0))
            {
                return false;
            }
            if (!(targetObject.TargetObject is CodeTypeReferenceExpression))
            {
                return false;
            }
            CodeTypeReferenceExpression expression3 = (CodeTypeReferenceExpression) targetObject.TargetObject;
            if ((expression3.UserData != null) && (expression3.UserData.Count > 0))
            {
                return false;
            }
            CodeTypeReference type = expression3.Type;
            if ((type.UserData != null) && (type.UserData.Count > 0))
            {
                return false;
            }
            if ((type.TypeArguments != null) && (type.TypeArguments.Count > 0))
            {
                return false;
            }
            return ((type.ArrayElementType == null) && (type.ArrayRank <= 0));
        }

        internal static string Serialize(CodePropertyReferenceExpression expression)
        {
            if (IsWellKnownApplicationSettingsExpression(expression))
            {
                return SerializeApplicationSettingsExpression(expression);
            }
            if (IsWellKnownAppConfigExpression(expression))
            {
                return SerializeAppConfigExpression(expression);
            }
            return SerializeWithSoapFormatter(expression);
        }

        private static string SerializeAppConfigExpression(CodePropertyReferenceExpression expression)
        {
            string propertyName = expression.PropertyName;
            CodeIndexerExpression targetObject = (CodeIndexerExpression) expression.TargetObject;
            string str2 = ((CodePrimitiveExpression) targetObject.Indices[0]).Value as string;
            propertyName = str2 + "." + propertyName;
            CodePropertyReferenceExpression expression3 = (CodePropertyReferenceExpression) targetObject.TargetObject;
            propertyName = expression3.PropertyName + "." + propertyName;
            CodeTypeReferenceExpression expression4 = (CodeTypeReferenceExpression) expression3.TargetObject;
            propertyName = expression4.Type.Options.ToString() + "." + propertyName;
            propertyName = expression4.Type.BaseType + "." + propertyName;
            return ("AppConfig." + propertyName);
        }

        private static string SerializeApplicationSettingsExpression(CodePropertyReferenceExpression expression)
        {
            string propertyName = expression.PropertyName;
            CodePropertyReferenceExpression targetObject = (CodePropertyReferenceExpression) expression.TargetObject;
            propertyName = targetObject.PropertyName + "." + propertyName;
            CodeTypeReferenceExpression expression3 = (CodeTypeReferenceExpression) targetObject.TargetObject;
            propertyName = expression3.Type.Options.ToString() + "." + propertyName;
            propertyName = expression3.Type.BaseType + "." + propertyName;
            return ("ApplicationSettings." + propertyName);
        }

        private static string SerializeWithSoapFormatter(CodePropertyReferenceExpression expression)
        {
            MemoryStream serializationStream = new MemoryStream();
            IFormatter formatter = new SoapFormatter();
            formatter.Serialize(serializationStream, expression);
            if (serializationStream.Length > 0x7fffffffL)
            {
                throw new InternalException("Serialized property expression is too long.");
            }
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] buffer = new byte[serializationStream.Length];
            serializationStream.Position = 0L;
            serializationStream.Read(buffer, 0, (int) serializationStream.Length);
            return encoding.GetString(buffer);
        }
    }
}

