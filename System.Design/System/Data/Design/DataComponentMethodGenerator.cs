namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Data;
    using System.Data.Common;

    internal sealed class DataComponentMethodGenerator
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        private DesignTable designTable;
        private bool generateHierarchicalUpdate;
        private DbProviderFactory providerFactory;

        internal DataComponentMethodGenerator(TypedDataSourceCodeGenerator codeGenerator, DesignTable designTable, bool generateHierarchicalUpdate)
        {
            this.generateHierarchicalUpdate = generateHierarchicalUpdate;
            this.codeGenerator = codeGenerator;
            this.designTable = designTable;
            if (designTable.Connection != null)
            {
                this.providerFactory = ProviderManager.GetFactory(designTable.Connection.Provider);
            }
        }

        private void AddAdapterMembers(CodeTypeDeclaration dataComponentClass)
        {
            Type type = this.providerFactory.CreateDataAdapter().GetType();
            CodeMemberField field = CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(type), DataComponentNameHandler.AdapterVariableName);
            field.UserData.Add("WithEvents", true);
            dataComponentClass.Members.Add(field);
            CodeMemberProperty property = null;
            if (this.generateHierarchicalUpdate)
            {
                property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(type), DataComponentNameHandler.AdapterPropertyName, MemberAttributes.FamilyOrAssembly | MemberAttributes.Final);
            }
            else
            {
                property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(type), DataComponentNameHandler.AdapterPropertyName, MemberAttributes.Private | MemberAttributes.Final);
            }
            property.GetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdEQ(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.AdapterVariableName), CodeGenHelper.Primitive(null)), new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), DataComponentNameHandler.InitAdapter, new CodeExpression[0])) }));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.AdapterVariableName)));
            dataComponentClass.Members.Add(property);
        }

        private void AddClearBeforeFillMembers(CodeTypeDeclaration dataComponentClass)
        {
            dataComponentClass.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(bool)), DataComponentNameHandler.ClearBeforeFillVariableName));
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(bool)), DataComponentNameHandler.ClearBeforeFillPropertyName, MemberAttributes.Public | MemberAttributes.Final);
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.ClearBeforeFillVariableName)));
            property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.ClearBeforeFillVariableName), CodeGenHelper.Argument("value")));
            dataComponentClass.Members.Add(property);
        }

        private void AddCommandCollectionMembers(CodeTypeDeclaration dataComponentClass, bool isFunctionsDataComponent)
        {
            Type type = null;
            if (isFunctionsDataComponent)
            {
                type = typeof(IDbCommand);
            }
            else
            {
                type = this.providerFactory.CreateCommand().GetType();
            }
            dataComponentClass.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(type, 1), DataComponentNameHandler.SelectCmdCollectionVariableName));
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(type, 1), DataComponentNameHandler.SelectCmdCollectionPropertyName, MemberAttributes.Family | MemberAttributes.Final);
            property.GetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdEQ(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionVariableName), CodeGenHelper.Primitive(null)), new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), DataComponentNameHandler.InitCmdCollection, new CodeExpression[0])) }));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionVariableName)));
            dataComponentClass.Members.Add(property);
        }

        private void AddCommandInitStatements(IList statements, CodeExpression commandExpression, DbSourceCommand command, DbProviderFactory currentFactory, bool isFunctionsDataComponent)
        {
            if (((statements == null) || (commandExpression == null)) || (command == null))
            {
                throw new InternalException("Argument should not be null.");
            }
            Type parameterType = currentFactory.CreateParameter().GetType();
            Type type = currentFactory.CreateCommand().GetType();
            CodeExpression parameterVariable = null;
            statements.Add(CodeGenHelper.Assign(commandExpression, CodeGenHelper.New(CodeGenHelper.GlobalType(type), new CodeExpression[0])));
            if (isFunctionsDataComponent)
            {
                commandExpression = CodeGenHelper.Cast(CodeGenHelper.GlobalType(type), commandExpression);
            }
            if ((((DbSource) command.Parent).Connection == null) || ((this.designTable.Connection != null) && (this.designTable.Connection == ((DbSource) command.Parent).Connection)))
            {
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(commandExpression, "Connection"), CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.DefaultConnectionPropertyName)));
            }
            else
            {
                Type type3 = currentFactory.CreateConnection().GetType();
                IDesignConnection connection = ((DbSource) command.Parent).Connection;
                CodeExpression propertyReference = null;
                if (connection.PropertyReference == null)
                {
                    propertyReference = CodeGenHelper.Str(connection.ConnectionStringObject.ToFullString());
                }
                else
                {
                    propertyReference = connection.PropertyReference;
                }
                statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(commandExpression, "Connection"), CodeGenHelper.New(CodeGenHelper.GlobalType(type3), new CodeExpression[] { propertyReference })));
            }
            statements.Add(QueryGeneratorBase.SetCommandTextStatement(commandExpression, command.CommandText));
            statements.Add(QueryGeneratorBase.SetCommandTypeStatement(commandExpression, command.CommandType));
            if (command.Parameters != null)
            {
                foreach (DesignParameter parameter in command.Parameters)
                {
                    parameterVariable = QueryGeneratorBase.AddNewParameterStatements(parameter, parameterType, currentFactory, statements, parameterVariable);
                    statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(commandExpression, "Parameters"), "Add", new CodeExpression[] { parameterVariable })));
                }
            }
        }

        private void AddConnectionMembers(CodeTypeDeclaration dataComponentClass)
        {
            Type type = this.providerFactory.CreateConnection().GetType();
            MemberAttributes modifier = ((DesignConnection) this.designTable.Connection).Modifier;
            dataComponentClass.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(type), DataComponentNameHandler.DefaultConnectionVariableName));
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(type), DataComponentNameHandler.DefaultConnectionPropertyName, modifier | MemberAttributes.Final);
            property.GetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdEQ(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.DefaultConnectionVariableName), CodeGenHelper.Primitive(null)), new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), DataComponentNameHandler.InitConnection, new CodeExpression[0])) }));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.DefaultConnectionVariableName)));
            property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.DefaultConnectionVariableName), CodeGenHelper.Argument("value")));
            property.SetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "InsertCommand"), CodeGenHelper.Primitive(null)), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "InsertCommand"), "Connection"), CodeGenHelper.Argument("value"))));
            property.SetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "DeleteCommand"), CodeGenHelper.Primitive(null)), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "DeleteCommand"), "Connection"), CodeGenHelper.Argument("value"))));
            property.SetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "UpdateCommand"), CodeGenHelper.Primitive(null)), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName), "UpdateCommand"), "Connection"), CodeGenHelper.Argument("value"))));
            int count = this.designTable.Sources.Count;
            CodeStatement initStmt = CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), "i", CodeGenHelper.Primitive(0));
            CodeExpression testExpression = CodeGenHelper.Less(CodeGenHelper.Variable("i"), CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionPropertyName), "Length"));
            CodeStatement incrementStmt = CodeGenHelper.Assign(CodeGenHelper.Variable("i"), CodeGenHelper.BinOperator(CodeGenHelper.Variable("i"), CodeBinaryOperatorType.Add, CodeGenHelper.Primitive(1)));
            CodeExpression left = CodeGenHelper.Property(CodeGenHelper.Cast(CodeGenHelper.GlobalType(this.providerFactory.CreateCommand().GetType()), CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionPropertyName), CodeGenHelper.Variable("i"))), "Connection");
            CodeStatement statement3 = CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionPropertyName), CodeGenHelper.Variable("i")), CodeGenHelper.Primitive(null)), CodeGenHelper.Assign(left, CodeGenHelper.Argument("value")));
            property.SetStatements.Add(CodeGenHelper.ForLoop(initStmt, testExpression, incrementStmt, new CodeStatement[] { statement3 }));
            dataComponentClass.Members.Add(property);
        }

        private void AddConstructor(CodeTypeDeclaration dataComponentClass)
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Public);
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.ClearBeforeFillPropertyName), CodeGenHelper.Primitive(true)));
            dataComponentClass.Members.Add(constructor);
        }

        private void AddInitAdapter(CodeTypeDeclaration dataComponentClass)
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), DataComponentNameHandler.InitAdapter, MemberAttributes.Private | MemberAttributes.Final);
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.AdapterVariableName), CodeGenHelper.New(CodeGenHelper.GlobalType(this.providerFactory.CreateDataAdapter().GetType()), new CodeExpression[0])));
            if ((this.designTable.Mappings != null) && (this.designTable.Mappings.Count > 0))
            {
                method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataTableMapping)), "tableMapping", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(DataTableMapping)), new CodeExpression[0])));
                method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("tableMapping"), "SourceTable"), CodeGenHelper.Str("Table")));
                method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("tableMapping"), "DataSetTable"), CodeGenHelper.Str(this.designTable.Name)));
                foreach (DataColumnMapping mapping in this.designTable.Mappings)
                {
                    method.Statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable("tableMapping"), "ColumnMappings"), "Add", new CodeExpression[] { CodeGenHelper.Str(mapping.SourceColumn), CodeGenHelper.Str(mapping.DataSetColumn) })));
                }
                method.Statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.AdapterVariableName), "TableMappings"), "Add", new CodeExpression[] { CodeGenHelper.Variable("tableMapping") })));
            }
            this.AddInitAdapterCommands(method);
            dataComponentClass.Members.Add(method);
        }

        private void AddInitAdapterCommands(CodeMemberMethod method)
        {
            if (this.designTable.DeleteCommand != null)
            {
                CodeExpression commandExpression = CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.AdapterVariableName), "DeleteCommand");
                this.AddCommandInitStatements(method.Statements, commandExpression, this.designTable.DeleteCommand, this.providerFactory, false);
            }
            if (this.designTable.InsertCommand != null)
            {
                CodeExpression expression2 = CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.AdapterVariableName), "InsertCommand");
                this.AddCommandInitStatements(method.Statements, expression2, this.designTable.InsertCommand, this.providerFactory, false);
            }
            if (this.designTable.UpdateCommand != null)
            {
                CodeExpression expression3 = CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.AdapterVariableName), "UpdateCommand");
                this.AddCommandInitStatements(method.Statements, expression3, this.designTable.UpdateCommand, this.providerFactory, false);
            }
        }

        private void AddInitCommandCollection(CodeTypeDeclaration dataComponentClass, bool isFunctionsDataComponent)
        {
            int count = this.designTable.Sources.Count;
            if (!isFunctionsDataComponent)
            {
                count++;
            }
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), DataComponentNameHandler.InitCmdCollection, MemberAttributes.Private | MemberAttributes.Final);
            Type type = null;
            if (isFunctionsDataComponent)
            {
                type = typeof(IDbCommand);
            }
            else
            {
                type = this.providerFactory.CreateCommand().GetType();
            }
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionVariableName), CodeGenHelper.NewArray(CodeGenHelper.GlobalType(type), count)));
            if ((!isFunctionsDataComponent && (this.designTable.MainSource != null)) && (this.designTable.MainSource is DbSource))
            {
                DbSourceCommand activeCommand = ((DbSource) this.designTable.MainSource).GetActiveCommand();
                if (activeCommand != null)
                {
                    CodeExpression commandExpression = CodeGenHelper.ArrayIndexer(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionVariableName), CodeGenHelper.Primitive(0));
                    this.AddCommandInitStatements(method.Statements, commandExpression, activeCommand, this.providerFactory, isFunctionsDataComponent);
                }
            }
            if (this.designTable.Sources != null)
            {
                int primitive = 0;
                if (isFunctionsDataComponent)
                {
                    primitive--;
                }
                foreach (Source source2 in this.designTable.Sources)
                {
                    DbSource source3 = source2 as DbSource;
                    primitive++;
                    if (source3 != null)
                    {
                        DbProviderFactory providerFactory = this.providerFactory;
                        if (source3.Connection != null)
                        {
                            providerFactory = ProviderManager.GetFactory(source3.Connection.Provider);
                        }
                        if (providerFactory != null)
                        {
                            DbSourceCommand command = source3.GetActiveCommand();
                            if (command != null)
                            {
                                CodeExpression expression2 = CodeGenHelper.ArrayIndexer(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionVariableName), CodeGenHelper.Primitive(primitive));
                                this.AddCommandInitStatements(method.Statements, expression2, command, providerFactory, isFunctionsDataComponent);
                            }
                        }
                    }
                }
            }
            dataComponentClass.Members.Add(method);
        }

        private void AddInitConnection(CodeTypeDeclaration dataComponentClass)
        {
            IDesignConnection connection = this.designTable.Connection;
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), DataComponentNameHandler.InitConnection, MemberAttributes.Private | MemberAttributes.Final);
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.DefaultConnectionVariableName), CodeGenHelper.New(CodeGenHelper.GlobalType(this.providerFactory.CreateConnection().GetType()), new CodeExpression[0])));
            CodeExpression right = null;
            if (connection.PropertyReference == null)
            {
                right = CodeGenHelper.Str(connection.ConnectionStringObject.ToFullString());
            }
            else
            {
                right = connection.PropertyReference;
            }
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.DefaultConnectionVariableName), "ConnectionString"), right));
            dataComponentClass.Members.Add(method);
        }

        internal void AddMethods(CodeTypeDeclaration dataComponentClass, bool isFunctionsDataComponent)
        {
            if (dataComponentClass == null)
            {
                throw new InternalException("dataComponent CodeTypeDeclaration should not be null.");
            }
            if (isFunctionsDataComponent)
            {
                this.AddCommandCollectionMembers(dataComponentClass, true);
                this.AddInitCommandCollection(dataComponentClass, true);
            }
            else if ((this.designTable.Connection != null) && (this.providerFactory != null))
            {
                this.AddConstructor(dataComponentClass);
                this.AddAdapterMembers(dataComponentClass);
                this.AddInitAdapter(dataComponentClass);
                this.AddConnectionMembers(dataComponentClass);
                this.AddInitConnection(dataComponentClass);
                if (this.generateHierarchicalUpdate)
                {
                    this.AddTransactionMembers(dataComponentClass);
                }
                this.AddCommandCollectionMembers(dataComponentClass, false);
                this.AddInitCommandCollection(dataComponentClass, false);
                this.AddClearBeforeFillMembers(dataComponentClass);
            }
        }

        private void AddTransactionMembers(CodeTypeDeclaration dataComponentClass)
        {
            Type transactionType = this.designTable.PropertyCache.TransactionType;
            if (transactionType != null)
            {
                CodeTypeReference type = CodeGenHelper.GlobalType(transactionType);
                dataComponentClass.Members.Add(CodeGenHelper.FieldDecl(type, DataComponentNameHandler.TransactionVariableName));
                CodeMemberProperty property = CodeGenHelper.PropertyDecl(type, DataComponentNameHandler.TransactionPropertyName, MemberAttributes.Assembly | MemberAttributes.Final);
                property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.TransactionVariableName)));
                property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.TransactionVariableName), CodeGenHelper.Argument("value")));
                CodeStatement initStmt = CodeGenHelper.VariableDecl(CodeGenHelper.Type(typeof(int)), "i", CodeGenHelper.Primitive(0));
                CodeExpression testExpression = CodeGenHelper.Less(CodeGenHelper.Variable("i"), CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionPropertyName), "Length"));
                CodeStatement incrementStmt = CodeGenHelper.Assign(CodeGenHelper.Variable("i"), CodeGenHelper.BinOperator(CodeGenHelper.Variable("i"), CodeBinaryOperatorType.Add, CodeGenHelper.Primitive(1)));
                CodeExpression transaction = CodeGenHelper.Property(CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.SelectCmdCollectionPropertyName), CodeGenHelper.Variable("i")), "Transaction");
                CodeExpression oldTransaction = CodeGenHelper.Variable("oldTransaction");
                CodeExpression newTransaction = CodeGenHelper.Field(CodeGenHelper.This(), DataComponentNameHandler.TransactionVariableName);
                CodeStatement statement3 = this.GenerateSetTransactionStmt(transaction, oldTransaction, newTransaction);
                property.SetStatements.Add(CodeGenHelper.ForLoop(initStmt, testExpression, incrementStmt, new CodeStatement[] { statement3 }));
                CodeExpression exp = CodeGenHelper.Property(CodeGenHelper.This(), DataComponentNameHandler.AdapterPropertyName);
                CodeExpression expression6 = CodeGenHelper.Property(exp, "DeleteCommand");
                transaction = CodeGenHelper.Property(expression6, "Transaction");
                property.SetStatements.Add(CodeGenHelper.If(CodeGenHelper.And(CodeGenHelper.IdNotEQ(exp, CodeGenHelper.Primitive(null)), CodeGenHelper.IdNotEQ(expression6, CodeGenHelper.Primitive(null))), this.GenerateSetTransactionStmt(transaction, oldTransaction, newTransaction)));
                expression6 = CodeGenHelper.Property(exp, "InsertCommand");
                transaction = CodeGenHelper.Property(expression6, "Transaction");
                property.SetStatements.Add(CodeGenHelper.If(CodeGenHelper.And(CodeGenHelper.IdNotEQ(exp, CodeGenHelper.Primitive(null)), CodeGenHelper.IdNotEQ(expression6, CodeGenHelper.Primitive(null))), this.GenerateSetTransactionStmt(transaction, oldTransaction, newTransaction)));
                expression6 = CodeGenHelper.Property(exp, "UpdateCommand");
                transaction = CodeGenHelper.Property(expression6, "Transaction");
                property.SetStatements.Add(CodeGenHelper.If(CodeGenHelper.And(CodeGenHelper.IdNotEQ(exp, CodeGenHelper.Primitive(null)), CodeGenHelper.IdNotEQ(expression6, CodeGenHelper.Primitive(null))), this.GenerateSetTransactionStmt(transaction, oldTransaction, newTransaction)));
                dataComponentClass.Members.Add(property);
            }
        }

        private CodeStatement GenerateSetTransactionStmt(CodeExpression transaction, CodeExpression oldTransaction, CodeExpression newTransaction)
        {
            return CodeGenHelper.Assign(transaction, newTransaction);
        }
    }
}

