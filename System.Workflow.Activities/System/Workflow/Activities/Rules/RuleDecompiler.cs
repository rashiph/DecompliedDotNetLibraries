namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class RuleDecompiler
    {
        private static Dictionary<string, string> knownOperatorMap = InitializeKnownOperatorMap();
        private static Dictionary<string, string> knownTypeMap = InitializeKnownTypeMap();
        private static Dictionary<Type, ComputePrecedence> precedenceMap = InitializePrecedenceMap();

        private static void AppendCharacter(StringBuilder decompilation, char charValue, char quoteCharacter)
        {
            if (charValue == quoteCharacter)
            {
                decompilation.Append(@"\");
                decompilation.Append(quoteCharacter);
            }
            else if (charValue == '\\')
            {
                decompilation.Append(@"\\");
            }
            else if (((charValue >= ' ') && (charValue < '\x007f')) || (char.IsLetterOrDigit(charValue) || char.IsPunctuation(charValue)))
            {
                decompilation.Append(charValue);
            }
            else
            {
                string str = null;
                switch (charValue)
                {
                    case '\0':
                        str = @"\0";
                        break;

                    case '\a':
                        str = @"\a";
                        break;

                    case '\b':
                        str = @"\b";
                        break;

                    case '\t':
                        str = @"\t";
                        break;

                    case '\n':
                        str = @"\n";
                        break;

                    case '\v':
                        str = @"\v";
                        break;

                    case '\f':
                        str = @"\f";
                        break;

                    case '\r':
                        str = @"\r";
                        break;
                }
                if (str != null)
                {
                    decompilation.Append(str);
                }
                else
                {
                    decompilation.Append(@"\u");
                    ushort num = charValue;
                    for (int i = 12; i >= 0; i -= 4)
                    {
                        int num3 = ((int) 15) << i;
                        byte num4 = (byte) ((num & num3) >> i);
                        decompilation.Append("0123456789ABCDEF"[num4]);
                    }
                }
            }
        }

        private static void DecompileCharacterLiteral(StringBuilder decompilation, char charValue)
        {
            decompilation.Append("'");
            AppendCharacter(decompilation, charValue, '\'');
            decompilation.Append("'");
        }

        private static void DecompileFloatingPointLiteral(StringBuilder decompilation, object value, char suffix)
        {
            string str = Convert.ToString(value, CultureInfo.InvariantCulture);
            decompilation.Append(str);
            if (suffix == 'd')
            {
                bool flag = str.IndexOf('.') >= 0;
                bool flag2 = str.IndexOfAny(new char[] { 'e', 'E' }) >= 0;
                if (!flag && !flag2)
                {
                    decompilation.Append(".0");
                }
            }
            else
            {
                decompilation.Append(suffix);
            }
        }

        internal static string DecompileMethod(MethodInfo method)
        {
            string str;
            if (method == null)
            {
                return string.Empty;
            }
            StringBuilder decompilation = new StringBuilder();
            DecompileType_Helper(decompilation, method.DeclaringType);
            decompilation.Append('.');
            if (knownOperatorMap.TryGetValue(method.Name, out str))
            {
                decompilation.Append(str);
            }
            else
            {
                decompilation.Append(method.Name);
            }
            decompilation.Append('(');
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                DecompileType_Helper(decompilation, parameters[i].ParameterType);
                if (i != (parameters.Length - 1))
                {
                    decompilation.Append(", ");
                }
            }
            decompilation.Append(')');
            return decompilation.ToString();
        }

        internal static void DecompileObjectLiteral(StringBuilder decompilation, object primitiveValue)
        {
            if (primitiveValue == null)
            {
                decompilation.Append("null");
            }
            else
            {
                Type type = primitiveValue.GetType();
                if (type == typeof(string))
                {
                    DecompileStringLiteral(decompilation, (string) primitiveValue);
                }
                else if (type == typeof(char))
                {
                    DecompileCharacterLiteral(decompilation, (char) primitiveValue);
                }
                else if (type == typeof(long))
                {
                    DecompileSuffixedIntegerLiteral(decompilation, primitiveValue, "L");
                }
                else if (type == typeof(uint))
                {
                    DecompileSuffixedIntegerLiteral(decompilation, primitiveValue, "U");
                }
                else if (type == typeof(ulong))
                {
                    DecompileSuffixedIntegerLiteral(decompilation, primitiveValue, "UL");
                }
                else if (type == typeof(float))
                {
                    DecompileFloatingPointLiteral(decompilation, primitiveValue, 'f');
                }
                else if (type == typeof(double))
                {
                    DecompileFloatingPointLiteral(decompilation, primitiveValue, 'd');
                }
                else if (type == typeof(decimal))
                {
                    DecompileFloatingPointLiteral(decompilation, primitiveValue, 'm');
                }
                else
                {
                    decompilation.Append(primitiveValue.ToString());
                }
            }
        }

        private static void DecompileStringLiteral(StringBuilder decompilation, string strValue)
        {
            decompilation.Append("\"");
            for (int i = 0; i < strValue.Length; i++)
            {
                char c = strValue[i];
                if ((char.IsHighSurrogate(c) && ((i + 1) < strValue.Length)) && char.IsLowSurrogate(strValue[i + 1]))
                {
                    decompilation.Append(c);
                    i++;
                    decompilation.Append(strValue[i]);
                }
                else
                {
                    AppendCharacter(decompilation, c, '"');
                }
            }
            decompilation.Append("\"");
        }

        private static void DecompileSuffixedIntegerLiteral(StringBuilder decompilation, object value, string suffix)
        {
            decompilation.Append(value.ToString());
            decompilation.Append(suffix);
        }

        internal static string DecompileType(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }
            StringBuilder decompilation = new StringBuilder();
            DecompileType_Helper(decompilation, type);
            return decompilation.ToString();
        }

        internal static void DecompileType(StringBuilder decompilation, CodeTypeReference typeRef)
        {
            string str = UnmangleTypeName(typeRef.BaseType);
            decompilation.Append(str);
            if ((typeRef.TypeArguments != null) && (typeRef.TypeArguments.Count > 0))
            {
                decompilation.Append("<");
                bool flag = true;
                foreach (CodeTypeReference reference in typeRef.TypeArguments)
                {
                    if (!flag)
                    {
                        decompilation.Append(", ");
                    }
                    flag = false;
                    DecompileType(decompilation, reference);
                }
                decompilation.Append(">");
            }
            if (typeRef.ArrayRank > 0)
            {
                do
                {
                    decompilation.Append("[");
                    for (int i = 1; i < typeRef.ArrayRank; i++)
                    {
                        decompilation.Append(",");
                    }
                    decompilation.Append("]");
                    typeRef = typeRef.ArrayElementType;
                }
                while (typeRef.ArrayRank > 0);
            }
        }

        private static void DecompileType_Helper(StringBuilder decompilation, Type type)
        {
            if (type.HasElementType)
            {
                DecompileType_Helper(decompilation, type.GetElementType());
                if (type.IsArray)
                {
                    decompilation.Append("[");
                    decompilation.Append(',', type.GetArrayRank() - 1);
                    decompilation.Append("]");
                }
                else if (type.IsByRef)
                {
                    decompilation.Append('&');
                }
                else if (type.IsPointer)
                {
                    decompilation.Append('*');
                }
            }
            else
            {
                string fullName = type.FullName;
                if (fullName == null)
                {
                    fullName = type.Name;
                }
                fullName = UnmangleTypeName(fullName);
                decompilation.Append(fullName);
                if (type.IsGenericType)
                {
                    decompilation.Append("<");
                    Type[] genericArguments = type.GetGenericArguments();
                    DecompileType_Helper(decompilation, genericArguments[0]);
                    for (int i = 1; i < genericArguments.Length; i++)
                    {
                        decompilation.Append(", ");
                        DecompileType_Helper(decompilation, genericArguments[i]);
                    }
                    decompilation.Append(">");
                }
            }
        }

        private static Operation GetBinaryPrecedence(CodeExpression expression)
        {
            CodeBinaryOperatorExpression expression2 = (CodeBinaryOperatorExpression) expression;
            switch (expression2.Operator)
            {
                case CodeBinaryOperatorType.Add:
                case CodeBinaryOperatorType.Subtract:
                    return Operation.Additive;

                case CodeBinaryOperatorType.Multiply:
                case CodeBinaryOperatorType.Divide:
                case CodeBinaryOperatorType.Modulus:
                    return Operation.Multiplicative;

                case CodeBinaryOperatorType.IdentityInequality:
                case CodeBinaryOperatorType.IdentityEquality:
                case CodeBinaryOperatorType.ValueEquality:
                    return Operation.Equality;

                case CodeBinaryOperatorType.BitwiseOr:
                    return Operation.BitwiseOr;

                case CodeBinaryOperatorType.BitwiseAnd:
                    return Operation.BitwiseAnd;

                case CodeBinaryOperatorType.BooleanOr:
                    return Operation.LogicalOr;

                case CodeBinaryOperatorType.BooleanAnd:
                    return Operation.LogicalAnd;

                case CodeBinaryOperatorType.LessThan:
                case CodeBinaryOperatorType.LessThanOrEqual:
                case CodeBinaryOperatorType.GreaterThan:
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    return Operation.Comparitive;
            }
            NotSupportedException exception = new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, new object[] { expression2.Operator.ToString() }));
            exception.Data["ErrorObject"] = expression2;
            throw exception;
        }

        private static Operation GetCastPrecedence(CodeExpression expression)
        {
            return Operation.Unary;
        }

        private static Operation GetPostfixPrecedence(CodeExpression expression)
        {
            return Operation.Postfix;
        }

        private static Operation GetPrecedence(CodeExpression expression)
        {
            ComputePrecedence precedence;
            Operation noParentheses = Operation.NoParentheses;
            if (precedenceMap.TryGetValue(expression.GetType(), out precedence))
            {
                noParentheses = precedence(expression);
            }
            return noParentheses;
        }

        private static Dictionary<string, string> InitializeKnownOperatorMap()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(0x1b);
            dictionary.Add("op_UnaryPlus", "operator +");
            dictionary.Add("op_UnaryNegation", "operator -");
            dictionary.Add("op_OnesComplement", "operator ~");
            dictionary.Add("op_LogicalNot", "operator !");
            dictionary.Add("op_Increment", "operator ++");
            dictionary.Add("op_Decrement", "operator --");
            dictionary.Add("op_True", "operator true");
            dictionary.Add("op_False", "operator false");
            dictionary.Add("op_Implicit", "implicit operator");
            dictionary.Add("op_Explicit", "explicit operator");
            dictionary.Add("op_Equality", "operator ==");
            dictionary.Add("op_Inequality", "operator !=");
            dictionary.Add("op_GreaterThan", "operator >");
            dictionary.Add("op_GreaterThanOrEqual", "operator >=");
            dictionary.Add("op_LessThan", "operator <");
            dictionary.Add("op_LessThanOrEqual", "operator <=");
            dictionary.Add("op_Addition", "operator +");
            dictionary.Add("op_Subtraction", "operator -");
            dictionary.Add("op_Multiply", "operator *");
            dictionary.Add("op_Division", "operator /");
            dictionary.Add("op_IntegerDivision", @"operator \");
            dictionary.Add("op_Modulus", "operator %");
            dictionary.Add("op_LeftShift", "operator <<");
            dictionary.Add("op_RightShift", "operator >>");
            dictionary.Add("op_BitwiseAnd", "operator &");
            dictionary.Add("op_BitwiseOr", "operator |");
            dictionary.Add("op_ExclusiveOr", "operator ^");
            return dictionary;
        }

        private static Dictionary<string, string> InitializeKnownTypeMap()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("System.Char", "char");
            dictionary.Add("System.Byte", "byte");
            dictionary.Add("System.SByte", "sbyte");
            dictionary.Add("System.Int16", "short");
            dictionary.Add("System.UInt16", "ushort");
            dictionary.Add("System.Int32", "int");
            dictionary.Add("System.UInt32", "uint");
            dictionary.Add("System.Int64", "long");
            dictionary.Add("System.UInt64", "ulong");
            dictionary.Add("System.Single", "float");
            dictionary.Add("System.Double", "double");
            dictionary.Add("System.Decimal", "decimal");
            dictionary.Add("System.Boolean", "bool");
            dictionary.Add("System.String", "string");
            dictionary.Add("System.Object", "object");
            dictionary.Add("System.Void", "void");
            return dictionary;
        }

        private static Dictionary<Type, ComputePrecedence> InitializePrecedenceMap()
        {
            Dictionary<Type, ComputePrecedence> dictionary = new Dictionary<Type, ComputePrecedence>(7);
            dictionary.Add(typeof(CodeBinaryOperatorExpression), new ComputePrecedence(RuleDecompiler.GetBinaryPrecedence));
            dictionary.Add(typeof(CodeCastExpression), new ComputePrecedence(RuleDecompiler.GetCastPrecedence));
            dictionary.Add(typeof(CodeFieldReferenceExpression), new ComputePrecedence(RuleDecompiler.GetPostfixPrecedence));
            dictionary.Add(typeof(CodePropertyReferenceExpression), new ComputePrecedence(RuleDecompiler.GetPostfixPrecedence));
            dictionary.Add(typeof(CodeMethodInvokeExpression), new ComputePrecedence(RuleDecompiler.GetPostfixPrecedence));
            dictionary.Add(typeof(CodeObjectCreateExpression), new ComputePrecedence(RuleDecompiler.GetPostfixPrecedence));
            dictionary.Add(typeof(CodeArrayCreateExpression), new ComputePrecedence(RuleDecompiler.GetPostfixPrecedence));
            return dictionary;
        }

        internal static bool MustParenthesize(CodeExpression childExpr, CodeExpression parentExpr)
        {
            if (parentExpr == null)
            {
                return false;
            }
            Operation precedence = GetPrecedence(childExpr);
            Operation operation2 = GetPrecedence(parentExpr);
            if (operation2 == precedence)
            {
                CodeBinaryOperatorExpression expression = parentExpr as CodeBinaryOperatorExpression;
                return ((expression != null) && (childExpr == expression.Right));
            }
            return (operation2 > precedence);
        }

        private static string TryReplaceKnownTypes(string typeName)
        {
            string str = null;
            if (!knownTypeMap.TryGetValue(typeName, out str))
            {
                str = typeName;
            }
            return str;
        }

        private static string UnmangleTypeName(string typeName)
        {
            int index = typeName.IndexOf('`');
            if (index > 0)
            {
                typeName = typeName.Substring(0, index);
            }
            typeName = typeName.Replace('+', '.');
            typeName = TryReplaceKnownTypes(typeName);
            return typeName;
        }

        private delegate RuleDecompiler.Operation ComputePrecedence(CodeExpression expresssion);

        private enum Operation
        {
            RootExpression,
            LogicalOr,
            LogicalAnd,
            BitwiseOr,
            BitwiseAnd,
            Equality,
            Comparitive,
            Additive,
            Multiplicative,
            Unary,
            Postfix,
            NoParentheses
        }
    }
}

