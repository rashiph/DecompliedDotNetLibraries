namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    internal static class CodeDomUtility
    {
        internal static BooleanSwitch WebFormsCompilation = new BooleanSwitch("WebFormsCompilation", "Outputs information about the WebForms compilation of ASPX templates");

        internal static void AppendCompilerOption(CompilerParameters compilParams, string compilerOptions)
        {
            if (compilParams.CompilerOptions == null)
            {
                compilParams.CompilerOptions = compilerOptions;
            }
            else
            {
                compilParams.CompilerOptions = compilParams.CompilerOptions + " " + compilerOptions;
            }
        }

        internal static CodeTypeReference BuildGlobalCodeTypeReference(string typeName)
        {
            return new CodeTypeReference(typeName, CodeTypeReferenceOptions.GlobalReference);
        }

        internal static CodeTypeReference BuildGlobalCodeTypeReference(Type type)
        {
            return new CodeTypeReference(type, CodeTypeReferenceOptions.GlobalReference);
        }

        private static CodeTypeReferenceExpression BuildGlobalCodeTypeReferenceExpression(string typeName)
        {
            return new CodeTypeReferenceExpression(BuildGlobalCodeTypeReference(typeName));
        }

        private static CodeTypeReferenceExpression BuildGlobalCodeTypeReferenceExpression(Type type)
        {
            return new CodeTypeReferenceExpression(BuildGlobalCodeTypeReference(type));
        }

        internal static CodeCastExpression BuildJSharpCastExpression(Type castType, CodeExpression expression)
        {
            CodeCastExpression expression2 = new CodeCastExpression(castType, expression);
            expression2.UserData.Add("CastIsBoxing", true);
            return expression2;
        }

        internal static CodeExpression BuildPropertyReferenceExpression(CodeExpression objRefExpr, string propName)
        {
            string[] strArray = propName.Split(new char[] { '.' });
            CodeExpression targetObject = objRefExpr;
            foreach (string str in strArray)
            {
                targetObject = new CodePropertyReferenceExpression(targetObject, str);
            }
            return targetObject;
        }

        internal static void CreatePropertySetStatements(CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeExpression target, string targetPropertyName, Type destinationType, CodeExpression value, CodeLinePragma linePragma)
        {
            bool flag = false;
            if (destinationType == null)
            {
                flag = true;
            }
            if (flag)
            {
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression();
                CodeExpressionStatement statement = new CodeExpressionStatement(expression) {
                    LinePragma = linePragma
                };
                expression.Method.TargetObject = new CodeCastExpression(typeof(IAttributeAccessor), target);
                expression.Method.MethodName = "SetAttribute";
                expression.Parameters.Add(new CodePrimitiveExpression(targetPropertyName));
                expression.Parameters.Add(GenerateConvertToString(value));
                statements.Add(statement);
            }
            else if (destinationType.IsValueType)
            {
                CodeAssignStatement statement2 = new CodeAssignStatement(BuildPropertyReferenceExpression(target, targetPropertyName), new CodeCastExpression(destinationType, value)) {
                    LinePragma = linePragma
                };
                statements.Add(statement2);
            }
            else
            {
                CodeExpression expression2;
                if (destinationType == typeof(string))
                {
                    expression2 = GenerateConvertToString(value);
                }
                else
                {
                    expression2 = new CodeCastExpression(destinationType, value);
                }
                CodeAssignStatement statement3 = new CodeAssignStatement(BuildPropertyReferenceExpression(target, targetPropertyName), expression2) {
                    LinePragma = linePragma
                };
                statements.Add(statement3);
            }
        }

        internal static CodeExpression GenerateConvertToString(CodeExpression value)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = BuildGlobalCodeTypeReferenceExpression(typeof(Convert)), MethodName = "ToString" }
            };
            expression.Parameters.Add(value);
            expression.Parameters.Add(new CodePropertyReferenceExpression(BuildGlobalCodeTypeReferenceExpression(typeof(CultureInfo)), "CurrentCulture"));
            return expression;
        }

        internal static CodeExpression GenerateExpressionForValue(PropertyInfo propertyInfo, object value, Type valueType)
        {
            CodeExpression expression = null;
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }
            PropertyDescriptor descriptor = null;
            if (propertyInfo != null)
            {
                descriptor = TypeDescriptor.GetProperties(propertyInfo.ReflectedType)[propertyInfo.Name];
            }
            if ((valueType == typeof(string)) && (value is string))
            {
                bool flag1 = WebFormsCompilation.Enabled;
                return new CodePrimitiveExpression((string) value);
            }
            if (valueType.IsPrimitive)
            {
                bool flag2 = WebFormsCompilation.Enabled;
                return new CodePrimitiveExpression(value);
            }
            if (((propertyInfo == null) && (valueType == typeof(object))) && ((value == null) || value.GetType().IsPrimitive))
            {
                bool flag3 = WebFormsCompilation.Enabled;
                return new CodePrimitiveExpression(value);
            }
            if (valueType.IsArray)
            {
                bool flag4 = WebFormsCompilation.Enabled;
                Array array = (Array) value;
                CodeArrayCreateExpression expression2 = new CodeArrayCreateExpression {
                    CreateType = new CodeTypeReference(valueType.GetElementType())
                };
                if (array != null)
                {
                    foreach (object obj2 in array)
                    {
                        expression2.Initializers.Add(GenerateExpressionForValue(null, obj2, valueType.GetElementType()));
                    }
                }
                return expression2;
            }
            if (valueType == typeof(Type))
            {
                return new CodeTypeOfExpression((Type) value);
            }
            bool enabled = WebFormsCompilation.Enabled;
            TypeConverter converter = null;
            if (descriptor != null)
            {
                converter = descriptor.Converter;
            }
            else
            {
                converter = TypeDescriptor.GetConverter(valueType);
            }
            bool flag = false;
            if (converter != null)
            {
                InstanceDescriptor descriptor2 = null;
                if (converter.CanConvertTo(typeof(InstanceDescriptor)))
                {
                    descriptor2 = (InstanceDescriptor) converter.ConvertTo(value, typeof(InstanceDescriptor));
                }
                if (descriptor2 != null)
                {
                    bool flag6 = WebFormsCompilation.Enabled;
                    if (descriptor2.MemberInfo is FieldInfo)
                    {
                        bool flag7 = WebFormsCompilation.Enabled;
                        CodeFieldReferenceExpression expression3 = new CodeFieldReferenceExpression(BuildGlobalCodeTypeReferenceExpression(descriptor2.MemberInfo.DeclaringType.FullName), descriptor2.MemberInfo.Name);
                        expression = expression3;
                        flag = true;
                    }
                    else if (descriptor2.MemberInfo is PropertyInfo)
                    {
                        bool flag8 = WebFormsCompilation.Enabled;
                        CodePropertyReferenceExpression expression4 = new CodePropertyReferenceExpression(BuildGlobalCodeTypeReferenceExpression(descriptor2.MemberInfo.DeclaringType.FullName), descriptor2.MemberInfo.Name);
                        expression = expression4;
                        flag = true;
                    }
                    else
                    {
                        object[] objArray = new object[descriptor2.Arguments.Count];
                        descriptor2.Arguments.CopyTo(objArray, 0);
                        CodeExpression[] expressionArray = new CodeExpression[objArray.Length];
                        if (descriptor2.MemberInfo is MethodInfo)
                        {
                            ParameterInfo[] parameters = ((MethodInfo) descriptor2.MemberInfo).GetParameters();
                            for (int i = 0; i < objArray.Length; i++)
                            {
                                expressionArray[i] = GenerateExpressionForValue(null, objArray[i], parameters[i].ParameterType);
                            }
                            bool flag9 = WebFormsCompilation.Enabled;
                            CodeMethodInvokeExpression expression5 = new CodeMethodInvokeExpression(BuildGlobalCodeTypeReferenceExpression(descriptor2.MemberInfo.DeclaringType.FullName), descriptor2.MemberInfo.Name, new CodeExpression[0]);
                            foreach (CodeExpression expression6 in expressionArray)
                            {
                                expression5.Parameters.Add(expression6);
                            }
                            expression = new CodeCastExpression(valueType, expression5);
                            flag = true;
                        }
                        else if (descriptor2.MemberInfo is ConstructorInfo)
                        {
                            ParameterInfo[] infoArray2 = ((ConstructorInfo) descriptor2.MemberInfo).GetParameters();
                            for (int j = 0; j < objArray.Length; j++)
                            {
                                expressionArray[j] = GenerateExpressionForValue(null, objArray[j], infoArray2[j].ParameterType);
                            }
                            bool flag10 = WebFormsCompilation.Enabled;
                            CodeObjectCreateExpression expression7 = new CodeObjectCreateExpression(descriptor2.MemberInfo.DeclaringType.FullName, new CodeExpression[0]);
                            foreach (CodeExpression expression8 in expressionArray)
                            {
                                expression7.Parameters.Add(expression8);
                            }
                            expression = expression7;
                            flag = true;
                        }
                    }
                }
            }
            if (flag)
            {
                return expression;
            }
            if (valueType.GetMethod("Parse", new Type[] { typeof(string), typeof(CultureInfo) }) != null)
            {
                string str;
                CodeMethodInvokeExpression expression9 = new CodeMethodInvokeExpression(BuildGlobalCodeTypeReferenceExpression(valueType.FullName), "Parse", new CodeExpression[0]);
                if (converter != null)
                {
                    str = converter.ConvertToInvariantString(value);
                }
                else
                {
                    str = value.ToString();
                }
                expression9.Parameters.Add(new CodePrimitiveExpression(str));
                expression9.Parameters.Add(new CodePropertyReferenceExpression(BuildGlobalCodeTypeReferenceExpression(typeof(CultureInfo)), "InvariantCulture"));
                return expression9;
            }
            if (valueType.GetMethod("Parse", new Type[] { typeof(string) }) == null)
            {
                throw new HttpException(System.Web.SR.GetString("CantGenPropertySet", new object[] { propertyInfo.Name, valueType.FullName }));
            }
            CodeMethodInvokeExpression expression10 = new CodeMethodInvokeExpression(BuildGlobalCodeTypeReferenceExpression(valueType.FullName), "Parse", new CodeExpression[0]);
            expression10.Parameters.Add(new CodePrimitiveExpression(value.ToString()));
            return expression10;
        }

        internal static void PrependCompilerOption(CompilerParameters compilParams, string compilerOptions)
        {
            if (compilParams.CompilerOptions == null)
            {
                compilParams.CompilerOptions = compilerOptions;
            }
            else
            {
                compilParams.CompilerOptions = compilerOptions + " " + compilParams.CompilerOptions;
            }
        }
    }
}

