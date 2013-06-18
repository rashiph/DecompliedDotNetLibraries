namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class CodeGenHelper
    {
        private CodeGenHelper()
        {
        }

        internal static CodeBinaryOperatorExpression And(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.BooleanAnd, right);
        }

        internal static CodeExpression Argument(string argument)
        {
            return new CodeArgumentReferenceExpression(argument);
        }

        internal static CodeExpression ArrayIndexer(CodeExpression targetObject, CodeExpression indices)
        {
            return new CodeArrayIndexerExpression(targetObject, new CodeExpression[] { indices });
        }

        internal static CodeStatement Assign(CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }

        internal static CodeAttributeDeclaration AttributeDecl(string name)
        {
            return new CodeAttributeDeclaration(GlobalType(name));
        }

        internal static CodeAttributeDeclaration AttributeDecl(string name, CodeExpression value)
        {
            return new CodeAttributeDeclaration(GlobalType(name), new CodeAttributeArgument[] { new CodeAttributeArgument(value) });
        }

        internal static CodeAttributeDeclaration AttributeDecl(string name, CodeExpression value1, CodeExpression value2)
        {
            return new CodeAttributeDeclaration(GlobalType(name), new CodeAttributeArgument[] { new CodeAttributeArgument(value1), new CodeAttributeArgument(value2) });
        }

        internal static CodeExpression Base()
        {
            return new CodeBaseReferenceExpression();
        }

        internal static CodeBinaryOperatorExpression BinOperator(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right)
        {
            return new CodeBinaryOperatorExpression(left, op, right);
        }

        internal static CodeBinaryOperatorExpression BitwiseAnd(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.BitwiseAnd, right);
        }

        internal static CodeExpression Cast(CodeTypeReference type, CodeExpression expr)
        {
            return new CodeCastExpression(type, expr);
        }

        internal static CodeCatchClause Catch(CodeTypeReference type, string name, CodeStatement catchStmnt)
        {
            CodeCatchClause clause = new CodeCatchClause {
                CatchExceptionType = type,
                LocalName = name
            };
            if (catchStmnt != null)
            {
                clause.Statements.Add(catchStmnt);
            }
            return clause;
        }

        internal static CodeTypeDeclaration Class(string name, bool isPartial, TypeAttributes typeAttributes)
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(name) {
                IsPartial = isPartial,
                TypeAttributes = typeAttributes
            };
            if (!declaration.IsPartial)
            {
                declaration.CustomAttributes.Add(GeneratedCodeAttributeDecl());
            }
            return declaration;
        }

        internal static CodeCommentStatement Comment(string comment, bool docSummary)
        {
            if (docSummary)
            {
                return new CodeCommentStatement("<summary>\r\n" + comment + "\r\n</summary>", docSummary);
            }
            return new CodeCommentStatement(comment);
        }

        internal static CodeConstructor Constructor(MemberAttributes attributes)
        {
            CodeConstructor constructor = new CodeConstructor {
                Attributes = attributes
            };
            constructor.CustomAttributes.Add(AttributeDecl(typeof(DebuggerNonUserCodeAttribute).FullName));
            constructor.CustomAttributes.Add(GeneratedCodeAttributeDecl());
            return constructor;
        }

        internal static CodeExpression DelegateCall(CodeExpression targetObject, CodeExpression par)
        {
            return new CodeDelegateInvokeExpression(targetObject, new CodeExpression[] { This(), par });
        }

        internal static CodeBinaryOperatorExpression EQ(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.ValueEquality, right);
        }

        internal static CodeExpression Event(string eventName)
        {
            return new CodeEventReferenceExpression(This(), eventName);
        }

        internal static CodeMemberEvent EventDecl(string type, string name)
        {
            CodeMemberEvent event2 = new CodeMemberEvent {
                Name = name,
                Type = Type(type),
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            event2.CustomAttributes.Add(GeneratedCodeAttributeDecl());
            return event2;
        }

        internal static CodeExpression Field(CodeExpression exp, string field)
        {
            return new CodeFieldReferenceExpression(exp, field);
        }

        internal static CodeMemberField FieldDecl(CodeTypeReference type, string name)
        {
            return new CodeMemberField(type, name);
        }

        internal static CodeMemberField FieldDecl(CodeTypeReference type, string name, CodeExpression initExpr)
        {
            return new CodeMemberField(type, name) { InitExpression = initExpr };
        }

        internal static CodeStatement ForLoop(CodeStatement initStmt, CodeExpression testExpression, CodeStatement incrementStmt, CodeStatement[] statements)
        {
            return new CodeIterationStatement(initStmt, testExpression, incrementStmt, statements);
        }

        internal static CodeExpression GenerateConvertExpression(CodeExpression sourceExpression, System.Type sourceType, System.Type targetType)
        {
            if (sourceType == targetType)
            {
                return sourceExpression;
            }
            if (IsSqlType(sourceType))
            {
                if (IsSqlType(targetType))
                {
                    throw new InternalException("Cannot perform the conversion between 2 SqlTypes.");
                }
                PropertyInfo property = sourceType.GetProperty("Value");
                if (property == null)
                {
                    throw new InternalException("Type does not expose a 'Value' property.");
                }
                System.Type propertyType = property.PropertyType;
                CodeExpression expression = new CodePropertyReferenceExpression(sourceExpression, "Value");
                return GenerateUrtConvertExpression(expression, propertyType, targetType);
            }
            if (IsSqlType(targetType))
            {
                System.Type targetUrtType = targetType.GetProperty("Value").PropertyType;
                CodeExpression expression2 = GenerateUrtConvertExpression(sourceExpression, sourceType, targetUrtType);
                return new CodeObjectCreateExpression(targetType, new CodeExpression[] { expression2 });
            }
            return GenerateUrtConvertExpression(sourceExpression, sourceType, targetType);
        }

        internal static CodeExpression GenerateDbNullCheck(CodeExpression returnParam)
        {
            return Or(IdEQ(returnParam, Primitive(null)), IdEQ(MethodCall(returnParam, "GetType"), TypeOf(GlobalType(typeof(DBNull)))));
        }

        internal static CodeAttributeDeclaration GeneratedCodeAttributeDecl()
        {
            return AttributeDecl(typeof(GeneratedCodeAttribute).FullName, Str(typeof(System.Data.Design.TypedDataSetGenerator).FullName), Str("4.0.0.0"));
        }

        internal static CodeExpression GenerateNullExpression(System.Type returnType)
        {
            if (IsSqlType(returnType))
            {
                return Field(GlobalTypeExpr(returnType), "Null");
            }
            if (returnType == typeof(object))
            {
                return Field(GlobalTypeExpr(typeof(DBNull)), "Value");
            }
            if (!returnType.IsValueType)
            {
                return Primitive(null);
            }
            return null;
        }

        private static CodeExpression GenerateUrtConvertExpression(CodeExpression sourceExpression, System.Type sourceUrtType, System.Type targetUrtType)
        {
            if (sourceUrtType == targetUrtType)
            {
                return sourceExpression;
            }
            if (sourceUrtType == typeof(object))
            {
                return Cast(GlobalType(targetUrtType), sourceExpression);
            }
            if (ConversionHelper.CanConvert(sourceUrtType, targetUrtType))
            {
                return new CodeMethodInvokeExpression(GlobalTypeExpr("System.Convert"), ConversionHelper.GetConversionMethodName(sourceUrtType, targetUrtType), new CodeExpression[] { sourceExpression });
            }
            return new CodeCastExpression(GlobalType(targetUrtType), new CodeMethodInvokeExpression(GlobalTypeExpr("System.Convert"), "ChangeType", new CodeExpression[] { sourceExpression, TypeOf(GlobalType(targetUrtType)) }));
        }

        internal static DSGeneratorProblem GenerateValueExprAndFieldInit(DesignColumn designColumn, object valueObj, object value, string className, string fieldName, out CodeExpression valueExpr, out CodeExpression fieldInit)
        {
            DataColumn dataColumn = designColumn.DataColumn;
            valueExpr = null;
            fieldInit = null;
            if (((((dataColumn.DataType == typeof(char)) || (dataColumn.DataType == typeof(string))) || ((dataColumn.DataType == typeof(decimal)) || (dataColumn.DataType == typeof(bool)))) || (((dataColumn.DataType == typeof(float)) || (dataColumn.DataType == typeof(double))) || ((dataColumn.DataType == typeof(sbyte)) || (dataColumn.DataType == typeof(byte))))) || ((((dataColumn.DataType == typeof(short)) || (dataColumn.DataType == typeof(ushort))) || ((dataColumn.DataType == typeof(int)) || (dataColumn.DataType == typeof(uint)))) || ((dataColumn.DataType == typeof(long)) || (dataColumn.DataType == typeof(ulong)))))
            {
                valueExpr = Primitive(valueObj);
            }
            else
            {
                valueExpr = Field(TypeExpr(Type(className)), fieldName);
                if (dataColumn.DataType == typeof(byte[]))
                {
                    fieldInit = MethodCall(GlobalTypeExpr(typeof(Convert)), "FromBase64String", Primitive(value));
                }
                else if (dataColumn.DataType == typeof(DateTime))
                {
                    DateTime time = (DateTime) valueObj;
                    fieldInit = MethodCall(GlobalTypeExpr(dataColumn.DataType), "Parse", Primitive(time.ToString("s", DateTimeFormatInfo.InvariantInfo)));
                }
                else if (dataColumn.DataType == typeof(TimeSpan))
                {
                    fieldInit = MethodCall(GlobalTypeExpr(dataColumn.DataType), "Parse", Primitive(valueObj.ToString()));
                }
                else
                {
                    ConstructorInfo constructor = dataColumn.DataType.GetConstructor(new System.Type[] { typeof(string) });
                    if (constructor == null)
                    {
                        return new DSGeneratorProblem(System.Design.SR.GetString("CG_NoCtor1", new object[] { dataColumn.ColumnName, dataColumn.DataType.Name }), ProblemSeverity.NonFatalError, designColumn);
                    }
                    constructor.Invoke(new object[] { value });
                    fieldInit = New(GlobalType(dataColumn.DataType), new CodeExpression[] { Primitive(value) });
                }
            }
            return null;
        }

        internal static string GetLanguageExtension(CodeDomProvider codeProvider)
        {
            if (codeProvider == null)
            {
                return string.Empty;
            }
            string str = "." + codeProvider.FileExtension;
            if (str.StartsWith("..", StringComparison.Ordinal))
            {
                str = str.Substring(1);
            }
            return str;
        }

        internal static string GetTypeName(CodeDomProvider codeProvider, string string1, string string2)
        {
            string str2 = codeProvider.GetTypeOutput(Type(typeof(Activator))).Replace("System", "").Replace("Activator", "");
            return (string1 + str2 + string2);
        }

        internal static CodeTypeReference GlobalGenericType(string fullTypeName, CodeTypeReference itemType)
        {
            return new CodeTypeReference(fullTypeName, new CodeTypeReference[] { itemType }) { Options = CodeTypeReferenceOptions.GlobalReference };
        }

        internal static CodeTypeReference GlobalGenericType(string fullTypeName, System.Type itemType)
        {
            return GlobalGenericType(fullTypeName, GlobalType(itemType));
        }

        internal static CodeTypeReference GlobalType(string type)
        {
            return new CodeTypeReference(type, CodeTypeReferenceOptions.GlobalReference);
        }

        internal static CodeTypeReference GlobalType(System.Type type)
        {
            return new CodeTypeReference(type.ToString(), CodeTypeReferenceOptions.GlobalReference);
        }

        internal static CodeTypeReference GlobalType(System.Type type, int rank)
        {
            return new CodeTypeReference(GlobalType(type), rank);
        }

        internal static CodeTypeReferenceExpression GlobalTypeExpr(string type)
        {
            return new CodeTypeReferenceExpression(GlobalType(type));
        }

        internal static CodeTypeReferenceExpression GlobalTypeExpr(System.Type type)
        {
            return new CodeTypeReferenceExpression(GlobalType(type));
        }

        internal static CodeBinaryOperatorExpression IdEQ(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.IdentityEquality, right);
        }

        internal static CodeBinaryOperatorExpression IdIsNotNull(CodeExpression id)
        {
            return IdNotEQ(id, Primitive(null));
        }

        internal static CodeBinaryOperatorExpression IdIsNull(CodeExpression id)
        {
            return IdEQ(id, Primitive(null));
        }

        internal static CodeBinaryOperatorExpression IdNotEQ(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.IdentityInequality, right);
        }

        internal static CodeStatement If(CodeExpression cond, CodeStatement[] trueStms)
        {
            return new CodeConditionStatement(cond, trueStms);
        }

        internal static CodeStatement If(CodeExpression cond, CodeStatement trueStm)
        {
            return If(cond, new CodeStatement[] { trueStm });
        }

        internal static CodeStatement If(CodeExpression cond, CodeStatement[] trueStms, CodeStatement[] falseStms)
        {
            return new CodeConditionStatement(cond, trueStms, falseStms);
        }

        internal static CodeStatement If(CodeExpression cond, CodeStatement trueStm, CodeStatement falseStm)
        {
            return new CodeConditionStatement(cond, new CodeStatement[] { trueStm }, new CodeStatement[] { falseStm });
        }

        internal static CodeExpression Indexer(CodeExpression targetObject, CodeExpression indices)
        {
            return new CodeIndexerExpression(targetObject, new CodeExpression[] { indices });
        }

        internal static bool IsGeneratingJSharpCode(CodeDomProvider codeProvider)
        {
            return StringUtil.EqualValue(GetLanguageExtension(codeProvider), ".jsl");
        }

        private static bool IsSqlType(System.Type type)
        {
            if ((((!(type == typeof(SqlBinary)) && !(type == typeof(SqlBoolean))) && (!(type == typeof(SqlByte)) && !(type == typeof(SqlDateTime)))) && ((!(type == typeof(SqlDecimal)) && !(type == typeof(SqlDouble))) && (!(type == typeof(SqlGuid)) && !(type == typeof(SqlInt16))))) && ((!(type == typeof(SqlInt32)) && !(type == typeof(SqlInt64))) && ((!(type == typeof(SqlMoney)) && !(type == typeof(SqlSingle))) && !(type == typeof(SqlString)))))
            {
                return false;
            }
            return true;
        }

        internal static CodeBinaryOperatorExpression Less(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.LessThan, right);
        }

        internal static CodeExpression MethodCall(CodeExpression targetObject, string methodName)
        {
            return new CodeMethodInvokeExpression(targetObject, methodName, new CodeExpression[0]);
        }

        internal static CodeExpression MethodCall(CodeExpression targetObject, string methodName, CodeExpression[] parameters)
        {
            return new CodeMethodInvokeExpression(targetObject, methodName, parameters);
        }

        internal static CodeExpression MethodCall(CodeExpression targetObject, string methodName, CodeExpression par)
        {
            return new CodeMethodInvokeExpression(targetObject, methodName, new CodeExpression[] { par });
        }

        internal static CodeStatement MethodCallStm(CodeExpression targetObject, string methodName)
        {
            return Stm(MethodCall(targetObject, methodName));
        }

        internal static CodeStatement MethodCallStm(CodeExpression targetObject, string methodName, CodeExpression[] parameters)
        {
            return Stm(MethodCall(targetObject, methodName, parameters));
        }

        internal static CodeStatement MethodCallStm(CodeExpression targetObject, string methodName, CodeExpression par)
        {
            return Stm(MethodCall(targetObject, methodName, par));
        }

        internal static CodeMemberMethod MethodDecl(CodeTypeReference type, string name, MemberAttributes attributes)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                ReturnType = type,
                Name = name,
                Attributes = attributes
            };
            method.CustomAttributes.Add(AttributeDecl(typeof(DebuggerNonUserCodeAttribute).FullName));
            method.CustomAttributes.Add(GeneratedCodeAttributeDecl());
            return method;
        }

        internal static CodeExpression New(CodeTypeReference type, CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(type, parameters);
        }

        internal static CodeExpression NewArray(CodeTypeReference type, int size)
        {
            return new CodeArrayCreateExpression(type, size);
        }

        internal static CodeExpression NewArray(CodeTypeReference type, params CodeExpression[] initializers)
        {
            return new CodeArrayCreateExpression(type, initializers);
        }

        internal static CodeBinaryOperatorExpression NotEQ(CodeExpression left, CodeExpression right)
        {
            return EQ(EQ(left, right), Primitive(false));
        }

        internal static CodeTypeReference NullableType(System.Type type)
        {
            CodeTypeReference reference = new CodeTypeReference(typeof(Nullable)) {
                Options = CodeTypeReferenceOptions.GlobalReference
            };
            reference.TypeArguments.Add(GlobalType(type));
            return reference;
        }

        internal static CodeBinaryOperatorExpression Or(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.BooleanOr, right);
        }

        internal static CodeParameterDeclarationExpression ParameterDecl(CodeTypeReference type, string name)
        {
            return new CodeParameterDeclarationExpression(type, name);
        }

        internal static FieldDirection ParameterDirectionToFieldDirection(ParameterDirection paramDirection)
        {
            switch (paramDirection)
            {
                case ParameterDirection.Input:
                    return FieldDirection.In;

                case ParameterDirection.Output:
                    return FieldDirection.Out;

                case ParameterDirection.InputOutput:
                    return FieldDirection.Ref;

                case ParameterDirection.ReturnValue:
                    throw new InternalException("Can't map from ParameterDirection.ReturnValue to FieldDirection.");
            }
            throw new InternalException("Unknown ParameterDirection.");
        }

        internal static CodeExpression Primitive(object primitive)
        {
            return new CodePrimitiveExpression(primitive);
        }

        internal static CodeExpression Property(CodeExpression exp, string property)
        {
            return new CodePropertyReferenceExpression(exp, property);
        }

        internal static CodeMemberProperty PropertyDecl(CodeTypeReference type, string name, MemberAttributes attributes)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Type = type,
                Name = name,
                Attributes = attributes
            };
            property.CustomAttributes.Add(AttributeDecl(typeof(DebuggerNonUserCodeAttribute).FullName));
            property.CustomAttributes.Add(GeneratedCodeAttributeDecl());
            return property;
        }

        internal static CodeExpression ReferenceEquals(CodeExpression left, CodeExpression right)
        {
            return MethodCall(GlobalTypeExpr(typeof(object)), "ReferenceEquals", new CodeExpression[] { left, right });
        }

        internal static CodeExpression ReferenceNotEquals(CodeExpression left, CodeExpression right)
        {
            return EQ(ReferenceEquals(left, right), Primitive(false));
        }

        internal static CodeStatement Return()
        {
            return new CodeMethodReturnStatement();
        }

        internal static CodeStatement Return(CodeExpression expr)
        {
            return new CodeMethodReturnStatement(expr);
        }

        internal static CodeStatement Stm(CodeExpression expr)
        {
            return new CodeExpressionStatement(expr);
        }

        internal static CodeExpression Str(string str)
        {
            return Primitive(str);
        }

        internal static bool SupportsMultipleNamespaces(CodeDomProvider codeProvider)
        {
            string name = MemberNameValidator.GenerateIdName("TestNs1", codeProvider, false);
            string str2 = MemberNameValidator.GenerateIdName("TestNs2", codeProvider, false);
            CodeNamespace namespace2 = new CodeNamespace(name);
            CodeNamespace namespace3 = new CodeNamespace(str2);
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(namespace2);
            compileUnit.Namespaces.Add(namespace3);
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            codeProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
            string str3 = writer.GetStringBuilder().ToString();
            return (str3.Contains(name) && str3.Contains(str2));
        }

        internal static CodeExpression This()
        {
            return new CodeThisReferenceExpression();
        }

        internal static CodeExpression ThisField(string field)
        {
            return new CodeFieldReferenceExpression(This(), field);
        }

        internal static CodeExpression ThisProperty(string property)
        {
            return new CodePropertyReferenceExpression(This(), property);
        }

        internal static CodeStatement Throw(CodeTypeReference exception, string arg)
        {
            return new CodeThrowExceptionStatement(New(exception, new CodeExpression[] { Str(arg) }));
        }

        internal static CodeStatement Throw(CodeTypeReference exception, string arg, CodeExpression inner)
        {
            return new CodeThrowExceptionStatement(New(exception, new CodeExpression[] { Str(arg), inner }));
        }

        internal static CodeStatement Throw(CodeTypeReference exception, string arg, string inner)
        {
            return new CodeThrowExceptionStatement(New(exception, new CodeExpression[] { Str(arg), Variable(inner) }));
        }

        internal static CodeStatement Try(CodeStatement tryStmnt, CodeCatchClause catchClause)
        {
            return new CodeTryCatchFinallyStatement(new CodeStatement[] { tryStmnt }, new CodeCatchClause[] { catchClause });
        }

        internal static CodeStatement Try(CodeStatement[] tryStmnts, CodeCatchClause[] catchClauses, CodeStatement[] finallyStmnts)
        {
            return new CodeTryCatchFinallyStatement(tryStmnts, catchClauses, finallyStmnts);
        }

        internal static CodeTypeReference Type(string type)
        {
            return new CodeTypeReference(type);
        }

        internal static CodeTypeReference Type(System.Type type)
        {
            return new CodeTypeReference(type);
        }

        internal static CodeTypeReference Type(string type, int rank)
        {
            return new CodeTypeReference(type, rank);
        }

        internal static CodeTypeReferenceExpression TypeExpr(CodeTypeReference type)
        {
            return new CodeTypeReferenceExpression(type);
        }

        internal static CodeExpression TypeOf(CodeTypeReference type)
        {
            return new CodeTypeOfExpression(type);
        }

        internal static CodeExpression Value()
        {
            return new CodePropertySetValueReferenceExpression();
        }

        internal static CodeExpression Variable(string variable)
        {
            return new CodeVariableReferenceExpression(variable);
        }

        internal static CodeStatement VariableDecl(CodeTypeReference type, string name)
        {
            return new CodeVariableDeclarationStatement(type, name);
        }

        internal static CodeStatement VariableDecl(CodeTypeReference type, string name, CodeExpression initExpr)
        {
            return new CodeVariableDeclarationStatement(type, name, initExpr);
        }
    }
}

