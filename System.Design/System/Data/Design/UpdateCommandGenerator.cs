namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal class UpdateCommandGenerator : QueryGeneratorBase
    {
        private bool generateOverloadWithoutCurrentPKParameters;

        internal UpdateCommandGenerator(TypedDataSourceCodeGenerator codeGenerator) : base(codeGenerator)
        {
        }

        private bool AddCallOverloadUpdateStm(CodeMemberMethod dbMethod)
        {
            int count = 0;
            if (base.activeCommand.Parameters != null)
            {
                count = base.activeCommand.Parameters.Count;
            }
            if (count <= 0)
            {
                return false;
            }
            List<CodeExpression> list = new List<CodeExpression>();
            bool flag = false;
            for (int i = 0; i < count; i++)
            {
                DesignParameter currentVersionParameter = base.activeCommand.Parameters[i];
                if (currentVersionParameter == null)
                {
                    throw new DataSourceGeneratorException("Parameter type is not DesignParameter.");
                }
                if (((currentVersionParameter.Direction == ParameterDirection.Input) || (currentVersionParameter.Direction == ParameterDirection.InputOutput)) && !currentVersionParameter.SourceColumnNullMapping)
                {
                    if ((currentVersionParameter.SourceVersion == DataRowVersion.Current) && this.IsPrimaryColumn(currentVersionParameter.SourceColumn))
                    {
                        DesignParameter originalVersionParameter = this.GetOriginalVersionParameter(currentVersionParameter);
                        if (originalVersionParameter != null)
                        {
                            flag = true;
                            currentVersionParameter = originalVersionParameter;
                        }
                    }
                    if (currentVersionParameter != null)
                    {
                        string nameFromList = base.nameHandler.GetNameFromList(currentVersionParameter.ParameterName);
                        list.Add(CodeGenHelper.Argument(nameFromList));
                    }
                }
            }
            if (!flag)
            {
                return false;
            }
            CodeStatement statement = CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.This(), "Update", list.ToArray()));
            dbMethod.Statements.Add(statement);
            return true;
        }

        private void AddCustomAttributesToMethod(CodeMemberMethod dbMethod)
        {
            DataObjectMethodType update = DataObjectMethodType.Update;
            if (base.methodSource.EnableWebMethods)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("System.Web.Services.WebMethod");
                declaration.Arguments.Add(new CodeAttributeArgument("Description", CodeGenHelper.Str(base.methodSource.WebMethodDescription)));
                dbMethod.CustomAttributes.Add(declaration);
            }
            if (base.MethodType != MethodTypeEnum.GenericUpdate)
            {
                if (base.activeCommand == base.methodSource.DeleteCommand)
                {
                    update = DataObjectMethodType.Delete;
                }
                else if (base.activeCommand == base.methodSource.InsertCommand)
                {
                    update = DataObjectMethodType.Insert;
                }
                else if (base.activeCommand == base.methodSource.UpdateCommand)
                {
                    update = DataObjectMethodType.Update;
                }
                dbMethod.CustomAttributes.Add(new CodeAttributeDeclaration(CodeGenHelper.GlobalType(typeof(DataObjectMethodAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataObjectMethodType)), update.ToString())), new CodeAttributeArgument(CodeGenHelper.Primitive(true)) }));
            }
        }

        private bool AddExecuteCommandStatements(IList statements)
        {
            if (base.MethodType == MethodTypeEnum.ColumnParameters)
            {
                CodeStatement[] tryStmnts = new CodeStatement[1];
                CodeStatement[] finallyStmnts = new CodeStatement[1];
                statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(ConnectionState)), base.nameHandler.AddNameToList("previousConnectionState"), CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), base.UpdateCommandName), "Connection"), "State")));
                statements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.BitwiseAnd(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), base.UpdateCommandName), "Connection"), "State"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Open")), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Open")), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), base.UpdateCommandName), "Connection"), "Open"))));
                tryStmnts[0] = CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), QueryGeneratorBase.returnVariableName, CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), base.UpdateCommandName), "ExecuteNonQuery", new CodeExpression[0]));
                finallyStmnts[0] = CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Variable(base.nameHandler.GetNameFromList("previousConnectionState")), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Closed")), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), base.UpdateCommandName), "Connection"), "Close")));
                statements.Add(CodeGenHelper.Try(tryStmnts, new CodeCatchClause[0], finallyStmnts));
            }
            else if (StringUtil.EqualValue(base.UpdateParameterTypeReference.BaseType, typeof(DataRow).FullName) && (base.UpdateParameterTypeReference.ArrayRank == 0))
            {
                statements.Add(CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "Update", new CodeExpression[] { CodeGenHelper.NewArray(base.UpdateParameterTypeReference, new CodeExpression[] { CodeGenHelper.Argument(base.UpdateParameterName) }) })));
            }
            else if (StringUtil.EqualValue(base.UpdateParameterTypeReference.BaseType, typeof(DataSet).FullName))
            {
                statements.Add(CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "Update", new CodeExpression[] { CodeGenHelper.Argument(base.UpdateParameterName), CodeGenHelper.Str(base.DesignTable.Name) })));
            }
            else
            {
                statements.Add(CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "Update", new CodeExpression[] { CodeGenHelper.Argument(base.UpdateParameterName) })));
            }
            return true;
        }

        private void AddParametersToMethod(CodeMemberMethod dbMethod)
        {
            DesignConnection connection = (DesignConnection) base.methodSource.Connection;
            if (connection == null)
            {
                throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Connection for query {0} is null.", new object[] { base.methodSource.Name }));
            }
            string parameterPrefix = connection.ParameterPrefix;
            if (base.MethodType == MethodTypeEnum.ColumnParameters)
            {
                if (base.activeCommand.Parameters != null)
                {
                    CodeParameterDeclarationExpression expression = null;
                    foreach (DesignParameter parameter in base.activeCommand.Parameters)
                    {
                        if (((parameter.Direction != ParameterDirection.ReturnValue) && !parameter.SourceColumnNullMapping) && ((!this.GenerateOverloadWithoutCurrentPKParameters || (parameter.SourceVersion != DataRowVersion.Current)) || (!this.IsPrimaryColumn(parameter.SourceColumn) || (this.GetOriginalVersionParameter(parameter) == null))))
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
            else
            {
                CodeParameterDeclarationExpression expression2 = null;
                string str3 = base.nameHandler.AddParameterNameToList(base.UpdateParameterName, parameterPrefix);
                if (base.UpdateParameterTypeName != null)
                {
                    expression2 = CodeGenHelper.ParameterDecl(CodeGenHelper.Type(base.UpdateParameterTypeName), str3);
                }
                else
                {
                    expression2 = CodeGenHelper.ParameterDecl(base.UpdateParameterTypeReference, str3);
                }
                dbMethod.Parameters.Add(expression2);
            }
        }

        private bool AddReturnStatements(IList statements)
        {
            CodeTryCatchFinallyStatement statement = (CodeTryCatchFinallyStatement) statements[statements.Count - 1];
            statement.TryStatements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(QueryGeneratorBase.returnVariableName)));
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
                DesignParameter originalParameter = base.activeCommand.Parameters[i];
                if (originalParameter == null)
                {
                    throw new DataSourceGeneratorException("Parameter type is not DesignParameter.");
                }
                if (((originalParameter.Direction == ParameterDirection.Input) || (originalParameter.Direction == ParameterDirection.InputOutput)) && !originalParameter.SourceColumnNullMapping)
                {
                    string nameFromList = base.nameHandler.GetNameFromList(originalParameter.ParameterName);
                    CodeExpression cmdExpression = CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), base.UpdateCommandName);
                    DesignParameter isNullParameter = null;
                    int isNullParameterIndex = 0;
                    if (originalParameter.SourceVersion == DataRowVersion.Original)
                    {
                        isNullParameter = this.FindCorrespondingIsNullParameter(originalParameter, out isNullParameterIndex);
                    }
                    base.AddSetParameterStatements(originalParameter, nameFromList, isNullParameter, cmdExpression, i, isNullParameterIndex, statements);
                }
            }
            return true;
        }

        protected bool AddSetReturnParamValuesStatements(IList statements)
        {
            CodeExpression commandExpression = CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), base.UpdateCommandName);
            CodeTryCatchFinallyStatement statement = (CodeTryCatchFinallyStatement) statements[statements.Count - 1];
            return base.AddSetReturnParamValuesStatements(statement.TryStatements, commandExpression);
        }

        private bool AddStatementsToMethod(CodeMemberMethod dbMethod)
        {
            if (this.GenerateOverloadWithoutCurrentPKParameters)
            {
                return this.AddCallOverloadUpdateStm(dbMethod);
            }
            if ((base.MethodType == MethodTypeEnum.ColumnParameters) && !this.AddSetParametersStatements(dbMethod.Statements))
            {
                return false;
            }
            if (!this.AddExecuteCommandStatements(dbMethod.Statements))
            {
                return false;
            }
            if (base.MethodType == MethodTypeEnum.ColumnParameters)
            {
                if (!this.AddSetReturnParamValuesStatements(dbMethod.Statements))
                {
                    return false;
                }
                if (!this.AddReturnStatements(dbMethod.Statements))
                {
                    return false;
                }
            }
            return true;
        }

        private DesignParameter FindCorrespondingIsNullParameter(DesignParameter originalParameter, out int isNullParameterIndex)
        {
            if (((originalParameter == null) || (originalParameter.SourceVersion != DataRowVersion.Original)) || originalParameter.SourceColumnNullMapping)
            {
                throw new InternalException("'originalParameter' is not valid.");
            }
            isNullParameterIndex = 0;
            for (int i = 0; i < base.activeCommand.Parameters.Count; i++)
            {
                DesignParameter parameter = base.activeCommand.Parameters[i];
                if (parameter == null)
                {
                    throw new DataSourceGeneratorException("Parameter type is not DesignParameter.");
                }
                if ((((parameter.Direction != ParameterDirection.Input) && (parameter.Direction != ParameterDirection.InputOutput)) || (parameter.SourceColumnNullMapping && (parameter.SourceVersion == DataRowVersion.Original))) && StringUtil.EqualValue(originalParameter.SourceColumn, parameter.SourceColumn))
                {
                    isNullParameterIndex = i;
                    return parameter;
                }
            }
            return null;
        }

        internal override CodeMemberMethod Generate()
        {
            if (base.methodSource == null)
            {
                throw new InternalException("MethodSource should not be null.");
            }
            if ((base.MethodType == MethodTypeEnum.ColumnParameters) && (base.activeCommand == null))
            {
                throw new InternalException("ActiveCommand should not be null.");
            }
            base.methodAttributes = base.MethodSource.Modifier | MemberAttributes.Overloaded;
            base.returnType = typeof(int);
            CodeDomProvider codeProvider = (base.codeProvider != null) ? base.codeGenerator.CodeProvider : base.CodeProvider;
            base.nameHandler = new GenericNameHandler(new string[] { base.MethodName, QueryGeneratorBase.commandVariableName, QueryGeneratorBase.returnVariableName }, codeProvider);
            return this.GenerateInternal();
        }

        private CodeMemberMethod GenerateInternal()
        {
            CodeMemberMethod dbMethod = null;
            dbMethod = CodeGenHelper.MethodDecl(CodeGenHelper.Type(base.returnType), base.MethodName, base.methodAttributes);
            dbMethod.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(HelpKeywordAttribute).FullName, CodeGenHelper.Str("vs.data.TableAdapter")));
            this.AddParametersToMethod(dbMethod);
            if (base.declarationOnly)
            {
                return dbMethod;
            }
            this.AddCustomAttributesToMethod(dbMethod);
            if (this.AddStatementsToMethod(dbMethod))
            {
                return dbMethod;
            }
            return null;
        }

        private DesignParameter GetOriginalVersionParameter(DesignParameter currentVersionParameter)
        {
            if ((currentVersionParameter == null) || (currentVersionParameter.SourceVersion != DataRowVersion.Current))
            {
                throw new InternalException("Invalid argutment currentVersionParameter");
            }
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
                if (((parameter.Direction == ParameterDirection.Input) || (parameter.Direction == ParameterDirection.InputOutput)) && ((!parameter.SourceColumnNullMapping && (parameter.SourceVersion == DataRowVersion.Original)) && StringUtil.EqualValue(parameter.SourceColumn, currentVersionParameter.SourceColumn)))
                {
                    return parameter;
                }
            }
            return null;
        }

        private bool IsPrimaryColumn(string columnName)
        {
            DataColumn[] primaryKeyColumns = base.DesignTable.PrimaryKeyColumns;
            if ((primaryKeyColumns != null) && (primaryKeyColumns.Length != 0))
            {
                foreach (DataColumn column in primaryKeyColumns)
                {
                    if (StringUtil.EqualValue(column.ColumnName, columnName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool GenerateOverloadWithoutCurrentPKParameters
        {
            get
            {
                return this.generateOverloadWithoutCurrentPKParameters;
            }
            set
            {
                this.generateOverloadWithoutCurrentPKParameters = value;
            }
        }
    }
}

