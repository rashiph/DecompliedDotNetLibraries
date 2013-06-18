namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;

    internal class FunctionGenerator : QueryGeneratorBase
    {
        internal FunctionGenerator(TypedDataSourceCodeGenerator codeGenerator) : base(codeGenerator)
        {
        }

        private void AddCustomAttributesToMethod(CodeMemberMethod dbMethod)
        {
            if (base.methodSource.EnableWebMethods)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("System.Web.Services.WebMethod");
                declaration.Arguments.Add(new CodeAttributeArgument("Description", CodeGenHelper.Str(base.methodSource.WebMethodDescription)));
                dbMethod.CustomAttributes.Add(declaration);
            }
            DataObjectMethodType select = DataObjectMethodType.Select;
            if (base.methodSource.CommandOperation == CommandOperation.Update)
            {
                select = DataObjectMethodType.Update;
            }
            else if (base.methodSource.CommandOperation == CommandOperation.Delete)
            {
                select = DataObjectMethodType.Delete;
            }
            else if (base.methodSource.CommandOperation == CommandOperation.Insert)
            {
                select = DataObjectMethodType.Insert;
            }
            if (select != DataObjectMethodType.Select)
            {
                dbMethod.CustomAttributes.Add(new CodeAttributeDeclaration(CodeGenHelper.GlobalType(typeof(DataObjectMethodAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataObjectMethodType)), select.ToString())), new CodeAttributeArgument(CodeGenHelper.Primitive(false)) }));
            }
        }

        private bool AddExecuteCommandStatements(IList statements)
        {
            CodeStatement[] tryStmnts = new CodeStatement[1];
            CodeStatement[] finallyStmnts = new CodeStatement[1];
            statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(ConnectionState)), base.nameHandler.AddNameToList("previousConnectionState"), CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "Connection"), "State")));
            statements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.BitwiseAnd(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "Connection"), "State"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Open")), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Open")), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "Connection"), "Open"))));
            if (base.methodSource.QueryType == QueryType.Scalar)
            {
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(object)), QueryGeneratorBase.returnVariableName));
                tryStmnts[0] = CodeGenHelper.Assign(CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName), CodeGenHelper.MethodCall(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "ExecuteScalar", new CodeExpression[0]));
            }
            else if ((base.methodSource.DbObjectType == DbObjectType.Function) && (base.GetReturnParameterPosition(base.activeCommand) >= 0))
            {
                tryStmnts[0] = CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "ExecuteNonQuery", new CodeExpression[0]));
            }
            else
            {
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), QueryGeneratorBase.returnVariableName));
                tryStmnts[0] = CodeGenHelper.Assign(CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName), CodeGenHelper.MethodCall(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "ExecuteNonQuery", new CodeExpression[0]));
            }
            finallyStmnts[0] = CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Variable(base.nameHandler.GetNameFromList("previousConnectionState")), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Closed")), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "Connection"), "Close")));
            statements.Add(CodeGenHelper.Try(tryStmnts, new CodeCatchClause[0], finallyStmnts));
            return true;
        }

        private void AddParametersToMethod(CodeMemberMethod dbMethod)
        {
            CodeParameterDeclarationExpression expression = null;
            if (base.activeCommand.Parameters != null)
            {
                DesignConnection connection = (DesignConnection) base.methodSource.Connection;
                if (connection == null)
                {
                    throw new InternalException("Connection for query '" + base.methodSource.Name + "' is null.");
                }
                string parameterPrefix = connection.ParameterPrefix;
                foreach (DesignParameter parameter in base.activeCommand.Parameters)
                {
                    if (parameter.Direction != ParameterDirection.ReturnValue)
                    {
                        Type parameterUrtType = base.GetParameterUrtType(parameter);
                        string name = base.nameHandler.AddParameterNameToList(parameter.ParameterName, parameterPrefix);
                        CodeTypeReference type = null;
                        if (parameter.AllowDbNull && parameterUrtType.IsValueType)
                        {
                            type = CodeGenHelper.NullableType(parameterUrtType);
                        }
                        else
                        {
                            type = CodeGenHelper.Type(parameterUrtType);
                        }
                        expression = CodeGenHelper.ParameterDecl(type, name);
                        expression.Direction = CodeGenHelper.ParameterDirectionToFieldDirection(parameter.Direction);
                        dbMethod.Parameters.Add(expression);
                    }
                }
            }
        }

        private bool AddReturnStatements(IList statements)
        {
            int returnParameterPosition = base.GetReturnParameterPosition(base.activeCommand);
            if (((base.methodSource.DbObjectType == DbObjectType.Function) && (base.methodSource.QueryType != QueryType.Scalar)) && (returnParameterPosition >= 0))
            {
                DesignParameter parameter = base.activeCommand.Parameters[returnParameterPosition];
                Type parameterUrtType = base.GetParameterUrtType(parameter);
                CodeExpression returnParam = CodeGenHelper.Property(CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), "Parameters"), CodeGenHelper.Primitive(returnParameterPosition)), "Value");
                CodeExpression cond = CodeGenHelper.GenerateDbNullCheck(returnParam);
                CodeExpression expr = CodeGenHelper.GenerateNullExpression(parameterUrtType);
                CodeStatement trueStm = null;
                if (expr == null)
                {
                    if (parameter.AllowDbNull && parameterUrtType.IsValueType)
                    {
                        trueStm = CodeGenHelper.Return(CodeGenHelper.New(CodeGenHelper.NullableType(parameterUrtType), new CodeExpression[0]));
                    }
                    else if (parameter.AllowDbNull && !parameterUrtType.IsValueType)
                    {
                        trueStm = CodeGenHelper.Return(CodeGenHelper.Primitive(null));
                    }
                    else
                    {
                        trueStm = CodeGenHelper.Throw(CodeGenHelper.GlobalType(typeof(StrongTypingException)), System.Design.SR.GetString("CG_ParameterIsDBNull", new object[] { base.activeCommand.Parameters[returnParameterPosition].ParameterName }), CodeGenHelper.Primitive(null));
                    }
                }
                else
                {
                    trueStm = CodeGenHelper.Return(expr);
                }
                CodeStatement falseStm = null;
                if (parameter.AllowDbNull && parameterUrtType.IsValueType)
                {
                    falseStm = CodeGenHelper.Return(CodeGenHelper.New(CodeGenHelper.NullableType(parameterUrtType), new CodeExpression[] { CodeGenHelper.Cast(CodeGenHelper.GlobalType(parameterUrtType), returnParam) }));
                }
                else
                {
                    falseStm = CodeGenHelper.Return(CodeGenHelper.GenerateConvertExpression(returnParam, typeof(object), parameterUrtType));
                }
                statements.Add(CodeGenHelper.If(cond, trueStm, falseStm));
            }
            else if (base.methodSource.QueryType == QueryType.Scalar)
            {
                CodeExpression expression5 = CodeGenHelper.GenerateDbNullCheck(CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName));
                CodeStatement statement3 = null;
                CodeStatement statement4 = null;
                if (base.returnType.IsValueType)
                {
                    statement3 = CodeGenHelper.Return(CodeGenHelper.New(CodeGenHelper.NullableType(base.returnType), new CodeExpression[0]));
                    statement4 = CodeGenHelper.Return(CodeGenHelper.New(CodeGenHelper.NullableType(base.returnType), new CodeExpression[] { CodeGenHelper.Cast(CodeGenHelper.GlobalType(base.returnType), CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName)) }));
                }
                else
                {
                    statement3 = CodeGenHelper.Return(CodeGenHelper.Primitive(null));
                    statement4 = CodeGenHelper.Return(CodeGenHelper.Cast(CodeGenHelper.GlobalType(base.returnType), CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName)));
                }
                statements.Add(CodeGenHelper.If(expression5, statement3, statement4));
            }
            else
            {
                statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName)));
            }
            return true;
        }

        private bool AddSetCommandStatements(IList statements)
        {
            Type type = base.ProviderFactory.CreateCommand().GetType();
            CodeExpression expr = CodeGenHelper.ArrayIndexer(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionPropertyName), CodeGenHelper.Primitive(base.CommandIndex));
            if (base.IsFunctionsDataComponent)
            {
                expr = CodeGenHelper.Cast(CodeGenHelper.GlobalType(type), expr);
            }
            statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(type), QueryGeneratorBase.commandVariableName, expr));
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
                    base.AddSetParameterStatements(parameter, nameFromList, CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName), i, statements);
                }
            }
            return true;
        }

        protected bool AddSetReturnParamValuesStatements(IList statements)
        {
            return base.AddSetReturnParamValuesStatements(statements, CodeGenHelper.Variable(QueryGeneratorBase.commandVariableName));
        }

        private bool AddStatementsToMethod(CodeMemberMethod dbMethod)
        {
            if (!this.AddSetCommandStatements(dbMethod.Statements))
            {
                return false;
            }
            if (!this.AddSetParametersStatements(dbMethod.Statements))
            {
                return false;
            }
            if (!this.AddExecuteCommandStatements(dbMethod.Statements))
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
            base.activeCommand = base.methodSource.GetActiveCommand();
            if (base.activeCommand == null)
            {
                return null;
            }
            base.methodAttributes = base.MethodSource.Modifier | MemberAttributes.Overloaded;
            if (base.codeProvider == null)
            {
                base.codeProvider = base.codeGenerator.CodeProvider;
            }
            base.nameHandler = new GenericNameHandler(new string[] { base.MethodName, QueryGeneratorBase.returnVariableName, QueryGeneratorBase.commandVariableName }, base.codeProvider);
            return this.GenerateInternal();
        }

        private CodeMemberMethod GenerateInternal()
        {
            DesignParameter returnParameter = base.GetReturnParameter(base.activeCommand);
            CodeTypeReference type = null;
            if (base.methodSource.QueryType == QueryType.Scalar)
            {
                base.returnType = base.methodSource.ScalarCallRetval;
                if (base.returnType.IsValueType)
                {
                    type = CodeGenHelper.NullableType(base.returnType);
                }
                else
                {
                    type = CodeGenHelper.Type(base.returnType);
                }
            }
            else if ((base.methodSource.DbObjectType == DbObjectType.Function) && (returnParameter != null))
            {
                base.returnType = base.GetParameterUrtType(returnParameter);
                if (returnParameter.AllowDbNull && base.returnType.IsValueType)
                {
                    type = CodeGenHelper.NullableType(base.returnType);
                }
                else
                {
                    type = CodeGenHelper.Type(base.returnType);
                }
            }
            else
            {
                base.returnType = typeof(int);
                type = CodeGenHelper.Type(base.returnType);
            }
            CodeMemberMethod dbMethod = null;
            dbMethod = CodeGenHelper.MethodDecl(type, base.MethodName, base.methodAttributes);
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

