namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Globalization;

    internal class QueryGenerator : QueryGeneratorBase
    {
        internal QueryGenerator(TypedDataSourceCodeGenerator codeGenerator) : base(codeGenerator)
        {
        }

        private bool AddClearStatements(IList statements)
        {
            if (!base.getMethod)
            {
                CodeStatement trueStm = null;
                if (base.containerParamType == typeof(DataTable))
                {
                    trueStm = CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Argument(base.ContainerParameterName), "Clear", new CodeExpression[0]));
                }
                else
                {
                    if (base.containerParamType != typeof(DataSet))
                    {
                        throw new InternalException("Unknown containerParameterType.");
                    }
                    trueStm = CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Argument(base.ContainerParameterName), base.DesignTable.GeneratorTablePropName), "Clear", new CodeExpression[0]));
                }
                statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.ClearBeforeFillPropertyName), CodeGenHelper.Primitive(true)), trueStm));
            }
            return true;
        }

        private void AddCustomAttributesToMethod(CodeMemberMethod dbMethod)
        {
            bool primitive = false;
            DataObjectMethodType fill = DataObjectMethodType.Fill;
            if (base.methodSource.EnableWebMethods && base.getMethod)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("System.Web.Services.WebMethod");
                declaration.Arguments.Add(new CodeAttributeArgument("Description", CodeGenHelper.Str(base.methodSource.WebMethodDescription)));
                dbMethod.CustomAttributes.Add(declaration);
            }
            if (!base.GeneratePagingMethod && (base.getMethod || (base.ContainerParameterType == typeof(DataTable))))
            {
                if (base.MethodSource == base.DesignTable.MainSource)
                {
                    primitive = true;
                }
                if (base.getMethod)
                {
                    fill = DataObjectMethodType.Select;
                }
                else
                {
                    fill = DataObjectMethodType.Fill;
                }
                dbMethod.CustomAttributes.Add(new CodeAttributeDeclaration(CodeGenHelper.GlobalType(typeof(DataObjectMethodAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataObjectMethodType)), fill.ToString())), new CodeAttributeArgument(CodeGenHelper.Primitive(primitive)) }));
            }
        }

        private bool AddExecuteCommandStatements(IList statements)
        {
            if (base.getMethod)
            {
                CodeExpression[] expressionArray = new CodeExpression[0];
                if ((base.designTable != null) && base.designTable.HasAnyExpressionColumn)
                {
                    expressionArray = new CodeExpression[] { CodeGenHelper.Primitive(true) };
                }
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(base.ContainerParameterTypeName), base.ContainerParameterName, CodeGenHelper.New(CodeGenHelper.Type(base.ContainerParameterTypeName), expressionArray)));
            }
            CodeExpression[] parameters = new CodeExpression[] { CodeGenHelper.Variable(base.ContainerParameterName) };
            if (!base.getMethod)
            {
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), QueryGeneratorBase.returnVariableName, CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "Fill", parameters)));
            }
            else
            {
                statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "Fill", parameters)));
            }
            return true;
        }

        private bool AddExecuteCommandStatementsForPaging(IList statements)
        {
            if (base.containerParamType == typeof(DataTable))
            {
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(base.codeGenerator.DataSourceName), base.nameHandler.AddNameToList("dataSet"), CodeGenHelper.New(CodeGenHelper.Type(base.codeGenerator.DataSourceName), new CodeExpression[0])));
            }
            CodeExpression[] parameters = new CodeExpression[4];
            if (base.containerParamType == typeof(DataTable))
            {
                parameters[0] = CodeGenHelper.Variable(base.nameHandler.GetNameFromList("dataSet"));
            }
            else
            {
                parameters[0] = CodeGenHelper.Argument(base.ContainerParameterName);
            }
            parameters[1] = CodeGenHelper.Argument(base.nameHandler.GetNameFromList(QueryGeneratorBase.startRecordParameterName));
            parameters[2] = CodeGenHelper.Argument(base.nameHandler.GetNameFromList(QueryGeneratorBase.maxRecordsParameterName));
            parameters[3] = CodeGenHelper.Str("Table");
            if (!base.getMethod)
            {
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), QueryGeneratorBase.returnVariableName, CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "Fill", parameters)));
            }
            else
            {
                statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "Fill", parameters)));
            }
            if ((base.containerParamType == typeof(DataTable)) && !base.getMethod)
            {
                CodeStatement initStmt = CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), "i", CodeGenHelper.Primitive(0));
                CodeExpression testExpression = CodeGenHelper.Less(CodeGenHelper.Variable("i"), CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Variable(base.nameHandler.GetNameFromList("dataSet")), base.DesignTable.GeneratorName), "Rows"), "Count"));
                CodeStatement incrementStmt = CodeGenHelper.Assign(CodeGenHelper.Variable("i"), CodeGenHelper.BinOperator(CodeGenHelper.Variable("i"), CodeBinaryOperatorType.Add, CodeGenHelper.Primitive(1)));
                CodeStatement statement3 = CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Argument(base.ContainerParameterName), "ImportRow", new CodeExpression[] { CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Variable(base.nameHandler.GetNameFromList("dataSet")), base.DesignTable.GeneratorName), "Rows"), CodeGenHelper.Variable("i")) }));
                statements.Add(CodeGenHelper.ForLoop(initStmt, testExpression, incrementStmt, new CodeStatement[] { statement3 }));
            }
            return true;
        }

        private void AddParametersToMethod(CodeMemberMethod dbMethod)
        {
            CodeParameterDeclarationExpression expression = null;
            if (!base.getMethod)
            {
                string name = base.nameHandler.AddNameToList(base.ContainerParameterName);
                expression = CodeGenHelper.ParameterDecl(CodeGenHelper.Type(base.ContainerParameterTypeName), name);
                dbMethod.Parameters.Add(expression);
            }
            if (base.GeneratePagingMethod)
            {
                string str2 = base.nameHandler.AddNameToList(QueryGeneratorBase.startRecordParameterName);
                expression = CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(int)), str2);
                dbMethod.Parameters.Add(expression);
                string str3 = base.nameHandler.AddNameToList(QueryGeneratorBase.maxRecordsParameterName);
                expression = CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(int)), str3);
                dbMethod.Parameters.Add(expression);
            }
            if (base.activeCommand.Parameters != null)
            {
                DesignConnection connection = (DesignConnection) base.methodSource.Connection;
                if (connection == null)
                {
                    throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Connection for query {0} is null.", new object[] { base.methodSource.Name }));
                }
                string parameterPrefix = connection.ParameterPrefix;
                foreach (DesignParameter parameter in base.activeCommand.Parameters)
                {
                    if (parameter.Direction != ParameterDirection.ReturnValue)
                    {
                        Type parameterUrtType = base.GetParameterUrtType(parameter);
                        string str5 = base.nameHandler.AddParameterNameToList(parameter.ParameterName, parameterPrefix);
                        CodeTypeReference type = null;
                        if (parameter.AllowDbNull && parameterUrtType.IsValueType)
                        {
                            type = CodeGenHelper.NullableType(parameterUrtType);
                        }
                        else
                        {
                            type = CodeGenHelper.Type(parameterUrtType);
                        }
                        expression = CodeGenHelper.ParameterDecl(type, str5);
                        expression.Direction = CodeGenHelper.ParameterDirectionToFieldDirection(parameter.Direction);
                        dbMethod.Parameters.Add(expression);
                    }
                }
            }
        }

        private bool AddReturnStatements(IList statements)
        {
            if (base.getMethod)
            {
                if (base.GeneratePagingMethod)
                {
                    statements.Add(CodeGenHelper.Return(CodeGenHelper.Property(CodeGenHelper.Variable(base.nameHandler.GetNameFromList("dataSet")), base.DesignTable.GeneratorName)));
                }
                else
                {
                    statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(base.ContainerParameterName)));
                }
            }
            else
            {
                statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName)));
            }
            return true;
        }

        private bool AddSetCommandStatements(IList statements)
        {
            base.ProviderFactory.CreateCommand().GetType();
            statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "SelectCommand"), CodeGenHelper.ArrayIndexer(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionPropertyName), CodeGenHelper.Primitive(base.CommandIndex))));
            return true;
        }

        private bool AddSetParametersStatements(IList statements)
        {
            int count = 0;
            if (base.activeCommand.Parameters != null)
            {
                count = base.activeCommand.Parameters.Count;
            }
            for (int i = 0; i < count; i++)
            {
                DesignParameter parameter = base.activeCommand.Parameters[i];
                if (parameter == null)
                {
                    throw new DataSourceGeneratorException("Parameter type is not DesignParameter.");
                }
                if ((parameter.Direction == ParameterDirection.Input) || (parameter.Direction == ParameterDirection.InputOutput))
                {
                    string nameFromList = base.nameHandler.GetNameFromList(parameter.ParameterName);
                    CodeExpression cmdExpression = CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "SelectCommand");
                    base.AddSetParameterStatements(parameter, nameFromList, cmdExpression, i, statements);
                }
            }
            return true;
        }

        protected bool AddSetReturnParamValuesStatements(IList statements)
        {
            CodeExpression commandExpression = CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "SelectCommand");
            return base.AddSetReturnParamValuesStatements(statements, commandExpression);
        }

        private bool AddStatementsToMethod(CodeMemberMethod dbMethod)
        {
            bool flag = true;
            if (!this.AddSetCommandStatements(dbMethod.Statements))
            {
                return false;
            }
            if (!this.AddSetParametersStatements(dbMethod.Statements))
            {
                return false;
            }
            if (!this.AddClearStatements(dbMethod.Statements))
            {
                return false;
            }
            if (base.GeneratePagingMethod)
            {
                flag = this.AddExecuteCommandStatementsForPaging(dbMethod.Statements);
            }
            else
            {
                flag = this.AddExecuteCommandStatements(dbMethod.Statements);
            }
            if (!flag)
            {
                return false;
            }
            if (!this.AddSetReturnParamValuesStatements(dbMethod.Statements))
            {
                return false;
            }
            if (!this.AddReturnStatements(dbMethod.Statements))
            {
                return false;
            }
            return true;
        }

        internal override CodeMemberMethod Generate()
        {
            if (base.methodSource == null)
            {
                throw new InternalException("MethodSource should not be null.");
            }
            if (StringUtil.Empty(base.ContainerParameterName))
            {
                throw new InternalException("ContainerParameterName should not be empty.");
            }
            if (base.methodSource.SelectCommand == null)
            {
                base.codeGenerator.ProblemList.Add(new DSGeneratorProblem(System.Design.SR.GetString("CG_MainSelectCommandNotSet", new object[] { base.DesignTable.Name }), ProblemSeverity.NonFatalError, base.methodSource));
                return null;
            }
            base.activeCommand = base.methodSource.SelectCommand;
            base.methodAttributes = MemberAttributes.Overloaded;
            if (base.getMethod)
            {
                base.methodAttributes |= base.MethodSource.GetMethodModifier;
            }
            else
            {
                base.methodAttributes |= base.MethodSource.Modifier;
            }
            if (base.codeProvider == null)
            {
                base.codeProvider = base.codeGenerator.CodeProvider;
            }
            base.nameHandler = new GenericNameHandler(new string[] { base.MethodName, QueryGeneratorBase.returnVariableName }, base.codeProvider);
            return this.GenerateInternal();
        }

        private CodeMemberMethod GenerateInternal()
        {
            base.returnType = typeof(int);
            CodeMemberMethod dbMethod = null;
            if (base.getMethod)
            {
                dbMethod = CodeGenHelper.MethodDecl(CodeGenHelper.Type(base.ContainerParameterTypeName), base.MethodName, base.methodAttributes);
            }
            else
            {
                dbMethod = CodeGenHelper.MethodDecl(CodeGenHelper.Type(base.returnType), base.MethodName, base.methodAttributes);
            }
            dbMethod.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(HelpKeywordAttribute).FullName, CodeGenHelper.Str("vs.data.TableAdapter")));
            this.AddParametersToMethod(dbMethod);
            if (base.declarationOnly)
            {
                base.AddThrowsClauseIfNeeded(dbMethod);
                return dbMethod;
            }
            this.AddCustomAttributesToMethod(dbMethod);
            if (this.AddStatementsToMethod(dbMethod))
            {
                return dbMethod;
            }
            return null;
        }
    }
}

