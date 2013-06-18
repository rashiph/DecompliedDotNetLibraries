namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Design;
    using System.Diagnostics;
    using System.Reflection;

    internal sealed class TableAdapterManagerMethodGenerator
    {
        private const string adapterPropertyEditor = "Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor";
        private TypedDataSourceCodeGenerator codeGenerator;
        private DesignDataSource dataSource;
        private CodeTypeDeclaration dataSourceType;
        private TableAdapterManagerNameHandler nameHandler;

        internal TableAdapterManagerMethodGenerator(TypedDataSourceCodeGenerator codeGenerator, DesignDataSource dataSource, CodeTypeDeclaration dataSourceType)
        {
            this.codeGenerator = codeGenerator;
            this.dataSource = dataSource;
            this.dataSourceType = dataSourceType;
            this.nameHandler = new TableAdapterManagerNameHandler(codeGenerator.CodeProvider);
        }

        private void AddAdapterMembers(CodeTypeDeclaration dataComponentClass)
        {
            foreach (DesignTable table in this.dataSource.DesignTables)
            {
                if (this.CanAddTableAdapter(table))
                {
                    table.PropertyCache.TAMAdapterPropName = this.nameHandler.GetTableAdapterPropName(table.GeneratorDataComponentClassName);
                    table.PropertyCache.TAMAdapterVarName = this.nameHandler.GetTableAdapterVarName(table.PropertyCache.TAMAdapterPropName);
                    string tAMAdapterVarName = table.PropertyCache.TAMAdapterVarName;
                    CodeMemberField field = CodeGenHelper.FieldDecl(CodeGenHelper.Type(table.GeneratorDataComponentClassName), tAMAdapterVarName);
                    dataComponentClass.Members.Add(field);
                    CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.Type(table.GeneratorDataComponentClassName), table.PropertyCache.TAMAdapterPropName, MemberAttributes.Public | MemberAttributes.Final);
                    property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.EditorAttribute", CodeGenHelper.Str("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), CodeGenHelper.Str("System.Drawing.Design.UITypeEditor")));
                    property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.ThisField(tAMAdapterVarName)));
                    property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.ThisField(tAMAdapterVarName), CodeGenHelper.Argument("value")));
                    dataComponentClass.Members.Add(property);
                }
            }
        }

        private void AddConnectionMembers(CodeTypeDeclaration dataComponentClass)
        {
            string name = "_connection";
            CodeMemberField field = CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(IDbConnection)), name);
            dataComponentClass.Members.Add(field);
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(IDbConnection)), "Connection", MemberAttributes.Public | MemberAttributes.Final);
            property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.Browsable", CodeGenHelper.Primitive(false)));
            property.GetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNotNull(CodeGenHelper.ThisField(name)), CodeGenHelper.Return(CodeGenHelper.ThisField(name))));
            foreach (DesignTable table in this.dataSource.DesignTables)
            {
                if (this.CanAddTableAdapter(table))
                {
                    string tAMAdapterVarName = table.PropertyCache.TAMAdapterVarName;
                    property.GetStatements.Add(CodeGenHelper.If(CodeGenHelper.And(CodeGenHelper.IdIsNotNull(CodeGenHelper.ThisField(tAMAdapterVarName)), CodeGenHelper.IdIsNotNull(CodeGenHelper.Property(CodeGenHelper.ThisField(tAMAdapterVarName), "Connection"))), CodeGenHelper.Return(CodeGenHelper.Property(CodeGenHelper.ThisField(tAMAdapterVarName), "Connection"))));
                }
            }
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Primitive(null)));
            property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.ThisField(name), CodeGenHelper.Argument("value")));
            dataComponentClass.Members.Add(property);
        }

        internal void AddEverything(CodeTypeDeclaration dataComponentClass)
        {
            if (dataComponentClass == null)
            {
                throw new InternalException("dataComponent CodeTypeDeclaration should not be null.");
            }
            this.AddUpdateOrderMembers(dataComponentClass);
            this.AddAdapterMembers(dataComponentClass);
            this.AddVariableAndProperty(dataComponentClass, MemberAttributes.Public | MemberAttributes.Final, CodeGenHelper.GlobalType(typeof(bool)), "BackupDataSetBeforeUpdate", "_backupDataSetBeforeUpdate", false);
            this.AddConnectionMembers(dataComponentClass);
            this.AddTableAdapterCountMembers(dataComponentClass);
            this.AddUpdateAll(dataComponentClass);
            this.AddSortSelfRefRows(dataComponentClass);
            this.AddSelfRefComparer(dataComponentClass);
            this.AddMatchTableAdapterConnection(dataComponentClass);
        }

        private void AddMatchTableAdapterConnection(CodeTypeDeclaration dataComponentClass)
        {
            string name = "inputConnection";
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(bool)), "MatchTableAdapterConnection", MemberAttributes.Family);
            CodeParameterDeclarationExpression expression = CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(IDbConnection)), name);
            method.Parameters.Add(expression);
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNotNull(CodeGenHelper.ThisField("_connection")), CodeGenHelper.Return(CodeGenHelper.Primitive(true))));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.Or(CodeGenHelper.IdIsNull(CodeGenHelper.ThisProperty("Connection")), CodeGenHelper.IdIsNull(CodeGenHelper.Argument(name))), CodeGenHelper.Return(CodeGenHelper.Primitive(true))));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.MethodCall(CodeGenHelper.GlobalTypeExpr(typeof(string)), "Equals", new CodeExpression[] { CodeGenHelper.Property(CodeGenHelper.ThisProperty("Connection"), "ConnectionString"), CodeGenHelper.Property(CodeGenHelper.Argument(name), "ConnectionString"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(StringComparison)), "Ordinal") }), CodeGenHelper.Return(CodeGenHelper.Primitive(true))));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Primitive(false)));
            dataComponentClass.Members.Add(method);
        }

        private void AddRealUpdatedRowsMethod(CodeTypeDeclaration dataComponentClass, string updatedRowsStr, string allAddedRowsStr)
        {
            string name = "realUpdatedRows";
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(DataRow), 1), "GetRealUpdatedRows", MemberAttributes.Private);
            CodeTypeReference type = CodeGenHelper.GlobalGenericType("System.Collections.Generic.List", typeof(DataRow));
            CodeParameterDeclarationExpression expression = CodeGenHelper.ParameterDecl(type, allAddedRowsStr);
            CodeParameterDeclarationExpression expression2 = CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRow), 1), updatedRowsStr);
            method.Comments.Add(CodeGenHelper.Comment("Remove inserted rows that become updated rows after calling TableAdapter.Update(inserted rows) first", true));
            method.Parameters.AddRange(new CodeParameterDeclarationExpression[] { expression2, expression });
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.Or(CodeGenHelper.IdIsNull(CodeGenHelper.Argument(updatedRowsStr)), CodeGenHelper.Less(CodeGenHelper.Property(CodeGenHelper.Argument(updatedRowsStr), "Length"), CodeGenHelper.Primitive(1))), CodeGenHelper.Return(CodeGenHelper.Variable(updatedRowsStr))));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.Or(CodeGenHelper.IdIsNull(CodeGenHelper.Argument(allAddedRowsStr)), CodeGenHelper.Less(CodeGenHelper.Property(CodeGenHelper.Argument(allAddedRowsStr), "Count"), CodeGenHelper.Primitive(1))), CodeGenHelper.Return(CodeGenHelper.Variable(updatedRowsStr))));
            method.Statements.Add(CodeGenHelper.VariableDecl(type, name, CodeGenHelper.New(type, new CodeExpression[0])));
            string str2 = "row";
            CodeStatement[] forStms = new CodeStatement[] { CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str2, CodeGenHelper.Indexer(CodeGenHelper.Variable(updatedRowsStr), CodeGenHelper.Variable("i"))), CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.Argument(allAddedRowsStr), "Contains", CodeGenHelper.Variable(str2)), CodeGenHelper.Primitive(false)), CodeGenHelper.MethodCallStm(CodeGenHelper.Variable(name), "Add", CodeGenHelper.Variable(str2))) };
            method.Statements.Add(this.GetForLoopItoCount(CodeGenHelper.Property(CodeGenHelper.Argument(updatedRowsStr), "Length"), forStms));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.Variable(name), "ToArray")));
            dataComponentClass.Members.Add(method);
        }

        private void AddSelfRefComparer(CodeTypeDeclaration dataComponentClass)
        {
            string name = "_relation";
            string str2 = "_childFirst";
            CodeTypeDeclaration declaration = CodeGenHelper.Class("SelfReferenceComparer", false, TypeAttributes.NestedPrivate);
            CodeTypeReference reference = CodeGenHelper.GlobalGenericType("System.Collections.Generic.IComparer", typeof(DataRow));
            declaration.BaseTypes.Add(CodeGenHelper.GlobalType(typeof(object)));
            declaration.BaseTypes.Add(reference);
            declaration.Comments.Add(CodeGenHelper.Comment("Used to sort self-referenced table's rows", true));
            dataComponentClass.Members.Add(declaration);
            declaration.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(DataRelation)), name));
            declaration.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(int)), str2));
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Assembly);
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRelation)), "relation"));
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(bool)), "childFirst"));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.ThisField(name), CodeGenHelper.Argument("relation")));
            constructor.Statements.Add(CodeGenHelper.If(CodeGenHelper.Argument("childFirst"), CodeGenHelper.Assign(CodeGenHelper.ThisField(str2), CodeGenHelper.Primitive(-1)), CodeGenHelper.Assign(CodeGenHelper.ThisField(str2), CodeGenHelper.Primitive(1))));
            declaration.Members.Add(constructor);
            string str3 = "row";
            string str4 = "distance";
            string str5 = "root";
            string str6 = "parent";
            string str7 = "GetRoot";
            string str8 = "traversedRows";
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str7, MemberAttributes.Private);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str3));
            CodeParameterDeclarationExpression expression = CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(int)), str4);
            expression.Direction = FieldDirection.Out;
            method.Parameters.Add(expression);
            method.Statements.Add(CodeGenHelper.MethodCallStm(CodeGenHelper.GlobalTypeExpr(typeof(Debug)), "Assert", CodeGenHelper.IdIsNotNull(CodeGenHelper.Argument(str3))));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str5, CodeGenHelper.Argument(str3)));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Argument(str4), CodeGenHelper.Primitive(0)));
            method.Statements.Add(new CodeSnippetStatement());
            CodeTypeReference type = new CodeTypeReference("System.Collections.Generic.IDictionary", new CodeTypeReference[] { CodeGenHelper.GlobalType(typeof(DataRow)), CodeGenHelper.GlobalType(typeof(DataRow)) }) {
                Options = CodeTypeReferenceOptions.GlobalReference
            };
            CodeTypeReference reference3 = new CodeTypeReference("System.Collections.Generic.Dictionary", new CodeTypeReference[] { CodeGenHelper.GlobalType(typeof(DataRow)), CodeGenHelper.GlobalType(typeof(DataRow)) }) {
                Options = CodeTypeReferenceOptions.GlobalReference
            };
            method.Statements.Add(CodeGenHelper.VariableDecl(type, str8, CodeGenHelper.New(reference3, new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Indexer(CodeGenHelper.Variable(str8), CodeGenHelper.Argument(str3)), CodeGenHelper.Argument(str3)));
            method.Statements.Add(new CodeSnippetStatement());
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str6, CodeGenHelper.MethodCall(CodeGenHelper.Argument(str3), "GetParentRow", new CodeExpression[] { CodeGenHelper.ThisField(name), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), "Default") })));
            CodeIterationStatement statement = new CodeIterationStatement {
                TestExpression = CodeGenHelper.And(CodeGenHelper.IdIsNotNull(CodeGenHelper.Variable(str6)), CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str8), "ContainsKey", CodeGenHelper.Variable(str6)), CodeGenHelper.Primitive(false))),
                InitStatement = new CodeSnippetStatement(),
                IncrementStatement = new CodeSnippetStatement()
            };
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Argument(str4), CodeGenHelper.BinOperator(CodeGenHelper.Argument(str4), CodeBinaryOperatorType.Add, CodeGenHelper.Primitive(1))));
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Variable(str5), CodeGenHelper.Variable(str6)));
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Indexer(CodeGenHelper.Variable(str8), CodeGenHelper.Variable(str6)), CodeGenHelper.Variable(str6)));
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Variable(str6), CodeGenHelper.MethodCall(CodeGenHelper.Variable(str6), "GetParentRow", new CodeExpression[] { CodeGenHelper.ThisField(name), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), "Default") })));
            method.Statements.Add(statement);
            method.Statements.Add(new CodeSnippetStatement());
            CodeConditionStatement statement2 = new CodeConditionStatement(CodeGenHelper.EQ(CodeGenHelper.Argument(str4), CodeGenHelper.Primitive(0)), new CodeStatement[0]);
            statement2.TrueStatements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str8), "Clear"));
            statement2.TrueStatements.Add(CodeGenHelper.Assign(CodeGenHelper.Indexer(CodeGenHelper.Variable(str8), CodeGenHelper.Argument(str3)), CodeGenHelper.Argument(str3)));
            statement2.TrueStatements.Add(CodeGenHelper.Assign(CodeGenHelper.Variable(str6), CodeGenHelper.MethodCall(CodeGenHelper.Argument(str3), "GetParentRow", new CodeExpression[] { CodeGenHelper.ThisField(name), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), "Original") })));
            statement = new CodeIterationStatement {
                TestExpression = CodeGenHelper.And(CodeGenHelper.IdIsNotNull(CodeGenHelper.Variable(str6)), CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str8), "ContainsKey", CodeGenHelper.Variable(str6)), CodeGenHelper.Primitive(false))),
                InitStatement = new CodeSnippetStatement(),
                IncrementStatement = new CodeSnippetStatement()
            };
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Argument(str4), CodeGenHelper.BinOperator(CodeGenHelper.Argument(str4), CodeBinaryOperatorType.Add, CodeGenHelper.Primitive(1))));
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Variable(str5), CodeGenHelper.Variable(str6)));
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Indexer(CodeGenHelper.Variable(str8), CodeGenHelper.Variable(str6)), CodeGenHelper.Variable(str6)));
            statement.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Variable(str6), CodeGenHelper.MethodCall(CodeGenHelper.Variable(str6), "GetParentRow", new CodeExpression[] { CodeGenHelper.ThisField(name), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataRowVersion)), "Original") })));
            statement2.TrueStatements.Add(statement);
            method.Statements.Add(statement2);
            method.Statements.Add(new CodeSnippetStatement());
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(str5)));
            declaration.Members.Add(method);
            string str9 = "row1";
            string str10 = "row2";
            string str11 = "root1";
            string str12 = "root2";
            string str13 = "distance1";
            string str14 = "distance2";
            CodeMemberMethod method2 = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(int)), "Compare", MemberAttributes.Public | MemberAttributes.Final);
            method2.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str9));
            method2.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str10));
            method2.ImplementationTypes.Add(reference);
            declaration.Members.Add(method2);
            method2.Statements.Add(CodeGenHelper.If(CodeGenHelper.ReferenceEquals(CodeGenHelper.Argument(str9), CodeGenHelper.Argument(str10)), CodeGenHelper.Return(CodeGenHelper.Primitive(0))));
            method2.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNull(CodeGenHelper.Argument(str9)), CodeGenHelper.Return(CodeGenHelper.Primitive(-1))));
            method2.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNull(CodeGenHelper.Argument(str10)), CodeGenHelper.Return(CodeGenHelper.Primitive(1))));
            method2.Statements.Add(new CodeSnippetStatement());
            method2.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), str13, CodeGenHelper.Primitive(0)));
            method2.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str11, CodeGenHelper.MethodCall(CodeGenHelper.This(), "GetRoot", new CodeExpression[] { CodeGenHelper.Argument(str9), new CodeDirectionExpression(FieldDirection.Out, CodeGenHelper.Variable(str13)) })));
            method2.Statements.Add(new CodeSnippetStatement());
            method2.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), str14, CodeGenHelper.Primitive(0)));
            method2.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow)), str12, CodeGenHelper.MethodCall(CodeGenHelper.This(), "GetRoot", new CodeExpression[] { CodeGenHelper.Argument(str10), new CodeDirectionExpression(FieldDirection.Out, CodeGenHelper.Variable(str14)) })));
            method2.Statements.Add(new CodeSnippetStatement());
            CodeBinaryOperatorExpression expr = CodeGenHelper.BinOperator(CodeGenHelper.ThisField(str2), CodeBinaryOperatorType.Multiply, CodeGenHelper.MethodCall(CodeGenHelper.Variable(str13), "CompareTo", CodeGenHelper.Variable(str14)));
            CodeStatement statement3 = CodeGenHelper.MethodCallStm(CodeGenHelper.GlobalTypeExpr(typeof(Debug)), "Assert", CodeGenHelper.And(CodeGenHelper.IdIsNotNull(CodeGenHelper.Field(CodeGenHelper.Variable(str11), "Table")), CodeGenHelper.IdIsNotNull(CodeGenHelper.Field(CodeGenHelper.Variable(str12), "Table"))));
            CodeConditionStatement statement4 = new CodeConditionStatement(CodeGenHelper.Less(CodeGenHelper.MethodCall(CodeGenHelper.Field(CodeGenHelper.Field(CodeGenHelper.Variable(str11), "Table"), "Rows"), "IndexOf", CodeGenHelper.Variable(str11)), CodeGenHelper.MethodCall(CodeGenHelper.Field(CodeGenHelper.Field(CodeGenHelper.Variable(str12), "Table"), "Rows"), "IndexOf", CodeGenHelper.Variable(str12))), new CodeStatement[] { CodeGenHelper.Return(CodeGenHelper.Primitive(-1)) }, new CodeStatement[] { CodeGenHelper.Return(CodeGenHelper.Primitive(1)) });
            method2.Statements.Add(CodeGenHelper.If(CodeGenHelper.ReferenceEquals(CodeGenHelper.Variable(str11), CodeGenHelper.Variable(str12)), new CodeStatement[] { CodeGenHelper.Return(expr) }, new CodeStatement[] { statement3, statement4 }));
        }

        private void AddSortSelfRefRows(CodeTypeDeclaration dataComponentClass)
        {
            string name = "rows";
            string str2 = "relation";
            string str3 = "childFirst";
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "SortSelfReferenceRows", MemberAttributes.Family);
            method.Parameters.AddRange(new CodeParameterDeclarationExpression[] { CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRow), 1), name), CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRelation)), str2), CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(bool)), str3) });
            CodeMethodReferenceExpression expression = new CodeMethodReferenceExpression(CodeGenHelper.GlobalTypeExpr("System.Array"), "Sort", new CodeTypeReference[] { CodeGenHelper.GlobalType(typeof(DataRow)) });
            CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression(expression, new CodeExpression[] { CodeGenHelper.Argument(name), CodeGenHelper.New(CodeGenHelper.Type("SelfReferenceComparer"), new CodeExpression[] { CodeGenHelper.Argument(str2), CodeGenHelper.Argument(str3) }) });
            method.Statements.Add(CodeGenHelper.Stm(expr));
            dataComponentClass.Members.Add(method);
        }

        private void AddTableAdapterCountMembers(CodeTypeDeclaration dataComponentClass)
        {
            string variable = "count";
            CodeExpression left = CodeGenHelper.Variable(variable);
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(int)), "TableAdapterInstanceCount", MemberAttributes.Public | MemberAttributes.Final);
            property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.Browsable", CodeGenHelper.Primitive(false)));
            property.GetStatements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), variable, CodeGenHelper.Primitive(0)));
            foreach (DesignTable table in this.dataSource.DesignTables)
            {
                if (this.CanAddTableAdapter(table))
                {
                    string tAMAdapterVarName = table.PropertyCache.TAMAdapterVarName;
                    property.GetStatements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNotNull(CodeGenHelper.ThisField(tAMAdapterVarName)), CodeGenHelper.Assign(left, CodeGenHelper.BinOperator(left, CodeBinaryOperatorType.Add, CodeGenHelper.Primitive(1)))));
                }
            }
            property.GetStatements.Add(CodeGenHelper.Return(left));
            dataComponentClass.Members.Add(property);
        }

        private void AddUpdateAll(CodeTypeDeclaration dataComponentClass)
        {
            string name = "dataSet";
            string str2 = "backupDataSet";
            string deletedRowsStr = "deletedRows";
            string addedRowsStr = "addedRows";
            string updatedRowsStr = "updatedRows";
            string variable = "result";
            string str7 = "workConnection";
            string str8 = "workTransaction";
            string str9 = "workConnOpened";
            string str10 = "allChangedRows";
            string str11 = "allAddedRows";
            string str12 = "adaptersWithAcceptChangesDuringUpdate";
            string str13 = "revertConnections";
            CodeExpression left = CodeGenHelper.Variable(variable);
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(int)), "UpdateAll", MemberAttributes.Public);
            string str14 = this.dataSourceType.Name;
            if (this.codeGenerator.DataSetNamespace != null)
            {
                str14 = CodeGenHelper.GetTypeName(this.codeGenerator.CodeProvider, this.codeGenerator.DataSetNamespace, str14);
            }
            CodeParameterDeclarationExpression expression2 = CodeGenHelper.ParameterDecl(CodeGenHelper.Type(str14), name);
            method.Parameters.Add(expression2);
            method.Comments.Add(CodeGenHelper.Comment("Update all changes to the dataset.", true));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNull(CodeGenHelper.Argument(name)), CodeGenHelper.Throw(CodeGenHelper.GlobalType(typeof(ArgumentNullException)), name)));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.Argument(name), "HasChanges"), CodeGenHelper.Primitive(false)), CodeGenHelper.Return(CodeGenHelper.Primitive(0))));
            foreach (DesignTable table in this.dataSource.DesignTables)
            {
                if (this.CanAddTableAdapter(table))
                {
                    string tAMAdapterVarName = table.PropertyCache.TAMAdapterVarName;
                    method.Statements.Add(CodeGenHelper.If(CodeGenHelper.And(CodeGenHelper.IdIsNotNull(CodeGenHelper.ThisField(tAMAdapterVarName)), CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.This(), "MatchTableAdapterConnection", CodeGenHelper.Property(CodeGenHelper.ThisField(tAMAdapterVarName), "Connection")), CodeGenHelper.Primitive(false))), new CodeStatement[] { new CodeThrowExceptionStatement(CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(ArgumentException)), new CodeExpression[] { CodeGenHelper.Str(System.Design.SR.GetString("CG_TableAdapterManagerNeedsSameConnString")) })) }));
                }
            }
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(IDbConnection)), str7, CodeGenHelper.ThisProperty("Connection")));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNull(CodeGenHelper.Variable(str7)), CodeGenHelper.Throw(CodeGenHelper.GlobalType(typeof(ApplicationException)), System.Design.SR.GetString("CG_TableAdapterManagerHasNoConnection"))));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(bool)), str9, CodeGenHelper.Primitive(false)));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.BitwiseAnd(CodeGenHelper.Property(CodeGenHelper.Variable(str7), "State"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Broken")), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Broken")), CodeGenHelper.MethodCallStm(CodeGenHelper.Variable(str7), "Close")));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Property(CodeGenHelper.Variable(str7), "State"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(ConnectionState)), "Closed")), new CodeStatement[] { CodeGenHelper.MethodCallStm(CodeGenHelper.Variable(str7), "Open"), CodeGenHelper.Assign(CodeGenHelper.Variable(str9), CodeGenHelper.Primitive(true)) }));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(IDbTransaction)), str8, CodeGenHelper.MethodCall(CodeGenHelper.Variable(str7), "BeginTransaction")));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdIsNull(CodeGenHelper.Variable(str8)), CodeGenHelper.Throw(CodeGenHelper.GlobalType(typeof(ApplicationException)), System.Design.SR.GetString("CG_TableAdapterManagerNotSupportTransaction"))));
            CodeTypeReference type = CodeGenHelper.GlobalGenericType("System.Collections.Generic.List", typeof(DataRow));
            method.Statements.Add(CodeGenHelper.VariableDecl(type, str10, CodeGenHelper.New(type, new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(type, str11, CodeGenHelper.New(type, new CodeExpression[0])));
            type = CodeGenHelper.GlobalGenericType("System.Collections.Generic.List", typeof(DataAdapter));
            method.Statements.Add(CodeGenHelper.VariableDecl(type, str12, CodeGenHelper.New(type, new CodeExpression[0])));
            CodeTypeReference reference3 = new CodeTypeReference("System.Collections.Generic.Dictionary", new CodeTypeReference[] { CodeGenHelper.GlobalType(typeof(object)), CodeGenHelper.GlobalType(typeof(IDbConnection)) }) {
                Options = CodeTypeReferenceOptions.GlobalReference
            };
            method.Statements.Add(CodeGenHelper.VariableDecl(reference3, str13, CodeGenHelper.New(reference3, new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(typeof(int)), variable, CodeGenHelper.Primitive(0)));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataSet)), str2, CodeGenHelper.Primitive(null)));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.ThisProperty("BackupDataSetBeforeUpdate"), new CodeStatement[] { CodeGenHelper.Assign(CodeGenHelper.Variable(str2), CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(DataSet)), new CodeExpression[0])), CodeGenHelper.MethodCallStm(CodeGenHelper.Variable(str2), "Merge", CodeGenHelper.Argument(name)) }));
            List<CodeStatement> list = new List<CodeStatement> {
                new CodeCommentStatement("---- Prepare for update -----------\r\n")
            };
            foreach (DesignTable table2 in this.dataSource.DesignTables)
            {
                if (this.CanAddTableAdapter(table2))
                {
                    string field = table2.PropertyCache.TAMAdapterVarName;
                    CodeStatement statement = null;
                    if (table2.PropertyCache.TransactionType != null)
                    {
                        statement = CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.ThisField(field), "Transaction"), CodeGenHelper.Cast(CodeGenHelper.GlobalType(table2.PropertyCache.TransactionType), CodeGenHelper.Variable(str8)));
                    }
                    else
                    {
                        statement = new CodeCommentStatement("Note: The TableAdapter does not have the Transaction property.");
                    }
                    CodeStatement statement2 = null;
                    if ((table2.PropertyCache.AdapterType != null) && typeof(DataAdapter).IsAssignableFrom(table2.PropertyCache.AdapterType))
                    {
                        statement2 = CodeGenHelper.If(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.ThisField(field), "Adapter"), "AcceptChangesDuringUpdate"), new CodeStatement[] { CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.ThisField(field), "Adapter"), "AcceptChangesDuringUpdate"), CodeGenHelper.Primitive(false)), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str12), "Add", CodeGenHelper.Property(CodeGenHelper.ThisField(field), "Adapter"))) });
                    }
                    else
                    {
                        statement2 = new CodeCommentStatement("Note: Adapter is not a DataAdapter, so AcceptChangesDuringUpdate cannot be set to false.");
                    }
                    list.Add(CodeGenHelper.If(CodeGenHelper.IdIsNotNull(CodeGenHelper.ThisField(field)), new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str13), "Add", new CodeExpression[] { CodeGenHelper.ThisField(field), CodeGenHelper.Property(CodeGenHelper.ThisField(field), "Connection") })), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.ThisField(field), "Connection"), CodeGenHelper.Cast(CodeGenHelper.GlobalType(table2.PropertyCache.ConnectionType), CodeGenHelper.Variable(str7))), statement, statement2 }));
                }
            }
            DataTable[] updateOrder = TableAdapterManagerHelper.GetUpdateOrder(this.dataSource.DataSet);
            this.AddUpdateUpdatedMethod(dataComponentClass, updateOrder, expression2, name, variable, updatedRowsStr, str10, str11);
            this.AddUpdateInsertedMethod(dataComponentClass, updateOrder, expression2, name, variable, addedRowsStr, str11);
            this.AddUpdateDeletedMethod(dataComponentClass, updateOrder, expression2, name, variable, deletedRowsStr, str10);
            this.AddRealUpdatedRowsMethod(dataComponentClass, updatedRowsStr, str11);
            list.Add(new CodeCommentStatement("\r\n---- Perform updates -----------\r\n"));
            CodeStatement statement3 = CodeGenHelper.Assign(left, CodeGenHelper.BinOperator(left, CodeBinaryOperatorType.Add, CodeGenHelper.MethodCall(CodeGenHelper.This(), "UpdateInsertedRows", new CodeExpression[] { CodeGenHelper.Argument(name), CodeGenHelper.Variable(str11) })));
            CodeStatement statement4 = CodeGenHelper.Assign(left, CodeGenHelper.BinOperator(left, CodeBinaryOperatorType.Add, CodeGenHelper.MethodCall(CodeGenHelper.This(), "UpdateUpdatedRows", new CodeExpression[] { CodeGenHelper.Argument(name), CodeGenHelper.Variable(str10), CodeGenHelper.Variable(str11) })));
            list.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.ThisProperty("UpdateOrder"), CodeGenHelper.Field(CodeGenHelper.TypeExpr(CodeGenHelper.Type("UpdateOrderOption")), "UpdateInsertDelete")), new CodeStatement[] { statement4, statement3 }, new CodeStatement[] { statement3, statement4 }));
            list.Add(CodeGenHelper.Assign(left, CodeGenHelper.BinOperator(left, CodeBinaryOperatorType.Add, CodeGenHelper.MethodCall(CodeGenHelper.This(), "UpdateDeletedRows", new CodeExpression[] { CodeGenHelper.Argument(name), CodeGenHelper.Variable(str10) }))));
            list.Add(new CodeCommentStatement("\r\n---- Commit updates -----------\r\n"));
            list.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str8), "Commit")));
            list.Add(this.HandleForEachRowInList(str11, new string[] { "AcceptChanges" }));
            list.Add(this.HandleForEachRowInList(str10, new string[] { "AcceptChanges" }));
            CodeCatchClause clause = new CodeCatchClause();
            clause.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str8), "Rollback"));
            clause.Statements.Add(new CodeCommentStatement("---- Restore the dataset -----------"));
            clause.Statements.Add(CodeGenHelper.If(CodeGenHelper.ThisProperty("BackupDataSetBeforeUpdate"), new CodeStatement[] { CodeGenHelper.MethodCallStm(CodeGenHelper.GlobalTypeExpr(typeof(Debug)), "Assert", CodeGenHelper.IdIsNotNull(CodeGenHelper.Variable(str2))), CodeGenHelper.MethodCallStm(CodeGenHelper.Argument(name), "Clear"), CodeGenHelper.MethodCallStm(CodeGenHelper.Argument(name), "Merge", CodeGenHelper.Variable(str2)) }, new CodeStatement[] { this.HandleForEachRowInList(str11, new string[] { "AcceptChanges", "SetAdded" }) }));
            clause.CatchExceptionType = CodeGenHelper.GlobalType(typeof(Exception));
            clause.LocalName = "ex";
            clause.Statements.Add(new CodeThrowExceptionStatement(CodeGenHelper.Variable("ex")));
            List<CodeStatement> list2 = new List<CodeStatement> {
                CodeGenHelper.If(CodeGenHelper.Variable(str9), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(str7), "Close")))
            };
            foreach (DesignTable table3 in this.dataSource.DesignTables)
            {
                if (this.CanAddTableAdapter(table3))
                {
                    string str17 = table3.PropertyCache.TAMAdapterVarName;
                    CodeStatement statement5 = null;
                    if (table3.PropertyCache.TransactionType != null)
                    {
                        statement5 = CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.ThisField(str17), "Transaction"), CodeGenHelper.Primitive(null));
                    }
                    else
                    {
                        statement5 = new CodeCommentStatement("Note: No Transaction property of the TableAdapter");
                    }
                    list2.Add(CodeGenHelper.If(CodeGenHelper.IdIsNotNull(CodeGenHelper.ThisField(str17)), new CodeStatement[] { CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.ThisField(str17), "Connection"), CodeGenHelper.Cast(CodeGenHelper.GlobalType(table3.PropertyCache.ConnectionType), CodeGenHelper.Indexer(CodeGenHelper.Variable(str13), CodeGenHelper.ThisField(str17)))), statement5 }));
                }
            }
            list2.Add(this.RestoreAdaptersWithACDU(str12));
            method.Statements.Add(CodeGenHelper.Try(list.ToArray(), new CodeCatchClause[] { clause }, list2.ToArray()));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(variable)));
            dataComponentClass.Members.Add(method);
        }

        private CodeStatement AddUpdateAllTAUpdate(DesignTable table, string dataSetStr, string resultStr, string updateRowsStr, string allUpdateRowsStr, string rowState, string allAddedRowsStr)
        {
            string tAMAdapterVarName = table.PropertyCache.TAMAdapterVarName;
            CodeStatement[] collection = new CodeStatement[] { CodeGenHelper.Assign(CodeGenHelper.Variable(resultStr), CodeGenHelper.BinOperator(CodeGenHelper.Variable(resultStr), CodeBinaryOperatorType.Add, CodeGenHelper.MethodCall(CodeGenHelper.ThisField(tAMAdapterVarName), "Update", CodeGenHelper.Variable(updateRowsStr)))), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(allUpdateRowsStr), "AddRange", CodeGenHelper.Variable(updateRowsStr))) };
            DataRelation[] selfRefRelations = TableAdapterManagerHelper.GetSelfRefRelations(table.DataTable);
            if ((selfRefRelations != null) && (selfRefRelations.Length > 0))
            {
                bool primitive = StringUtil.EqualValue("Deleted", rowState, true);
                List<CodeStatement> list = new List<CodeStatement>(collection.Length + selfRefRelations.Length);
                for (int i = 0; i < selfRefRelations.Length; i++)
                {
                    if (i > 0)
                    {
                        list.Add(new CodeCommentStatement("Note: More than one self-referenced relation found.  The generated code may not work correctly."));
                    }
                    list.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "SortSelfReferenceRows", new CodeExpression[] { CodeGenHelper.Variable(updateRowsStr), CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Argument(dataSetStr), "Relations"), CodeGenHelper.Str(selfRefRelations[i].RelationName)), CodeGenHelper.Primitive(primitive) })));
                }
                list.AddRange(collection);
                collection = list.ToArray();
            }
            List<CodeStatement> list2 = new List<CodeStatement>(3) {
                CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow), 1), updateRowsStr, CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Argument(dataSetStr), table.GeneratorTablePropName), "Select", new CodeExpression[] { CodeGenHelper.Primitive(null), CodeGenHelper.Primitive(null), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataViewRowState)), rowState) }))
            };
            if (StringUtil.NotEmptyAfterTrim(allAddedRowsStr))
            {
                list2.Add(CodeGenHelper.Assign(CodeGenHelper.Argument(updateRowsStr), CodeGenHelper.MethodCall(CodeGenHelper.This(), "GetRealUpdatedRows", new CodeExpression[] { CodeGenHelper.Argument(updateRowsStr), CodeGenHelper.Argument(allAddedRowsStr) })));
            }
            list2.Add(CodeGenHelper.If(CodeGenHelper.And(CodeGenHelper.IdNotEQ(CodeGenHelper.Variable(updateRowsStr), CodeGenHelper.Primitive(null)), CodeGenHelper.Less(CodeGenHelper.Primitive(0), CodeGenHelper.Property(CodeGenHelper.Variable(updateRowsStr), "Length"))), collection));
            return CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.ThisField(tAMAdapterVarName), CodeGenHelper.Primitive(null)), list2.ToArray());
        }

        private void AddUpdateDeletedMethod(CodeTypeDeclaration dataComponentClass, DataTable[] orderedTables, CodeParameterDeclarationExpression dataSetPara, string dataSetStr, string resultStr, string deletedRowsStr, string allChangedRowsStr)
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(int)), "UpdateDeletedRows", MemberAttributes.Private);
            CodeParameterDeclarationExpression expression = CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalGenericType("System.Collections.Generic.List", typeof(DataRow)), allChangedRowsStr);
            method.Parameters.AddRange(new CodeParameterDeclarationExpression[] { dataSetPara, expression });
            method.Comments.Add(CodeGenHelper.Comment("Delete rows in bottom-up order.", true));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(typeof(int)), resultStr, CodeGenHelper.Primitive(0)));
            for (int i = orderedTables.Length - 1; i >= 0; i--)
            {
                DesignTable table = this.dataSource.DesignTables[orderedTables[i]];
                if (this.CanAddTableAdapter(table))
                {
                    method.Statements.Add(this.AddUpdateAllTAUpdate(table, dataSetStr, resultStr, deletedRowsStr, allChangedRowsStr, "Deleted", null));
                }
            }
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(resultStr)));
            dataComponentClass.Members.Add(method);
        }

        private void AddUpdateInsertedMethod(CodeTypeDeclaration dataComponentClass, DataTable[] orderedTables, CodeParameterDeclarationExpression dataSetPara, string dataSetStr, string resultStr, string addedRowsStr, string allAddedRowsStr)
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(int)), "UpdateInsertedRows", MemberAttributes.Private);
            CodeParameterDeclarationExpression expression = CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalGenericType("System.Collections.Generic.List", typeof(DataRow)), allAddedRowsStr);
            method.Parameters.AddRange(new CodeParameterDeclarationExpression[] { dataSetPara, expression });
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(typeof(int)), resultStr, CodeGenHelper.Primitive(0)));
            method.Comments.Add(CodeGenHelper.Comment("Insert rows in top-down order.", true));
            for (int i = 0; i < orderedTables.Length; i++)
            {
                DesignTable table = this.dataSource.DesignTables[orderedTables[i]];
                if (this.CanAddTableAdapter(table))
                {
                    method.Statements.Add(this.AddUpdateAllTAUpdate(table, dataSetStr, resultStr, addedRowsStr, allAddedRowsStr, "Added", null));
                }
            }
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(resultStr)));
            dataComponentClass.Members.Add(method);
        }

        private void AddUpdateOrderMembers(CodeTypeDeclaration dataComponentClass)
        {
            CodeTypeDeclaration declaration = CodeGenHelper.Class("UpdateOrderOption", false, TypeAttributes.NestedPublic);
            declaration.IsEnum = true;
            declaration.Comments.Add(CodeGenHelper.Comment("Update Order Option", true));
            CodeMemberField field = CodeGenHelper.FieldDecl(CodeGenHelper.Type(typeof(int)), "InsertUpdateDelete", CodeGenHelper.Primitive(0));
            declaration.Members.Add(field);
            CodeMemberField field2 = CodeGenHelper.FieldDecl(CodeGenHelper.Type(typeof(int)), "UpdateInsertDelete", CodeGenHelper.Primitive(1));
            declaration.Members.Add(field2);
            dataComponentClass.Members.Add(declaration);
            this.AddVariableAndProperty(dataComponentClass, MemberAttributes.Public | MemberAttributes.Final, CodeGenHelper.Type("UpdateOrderOption"), "UpdateOrder", "_updateOrder", false);
        }

        private void AddUpdateUpdatedMethod(CodeTypeDeclaration dataComponentClass, DataTable[] orderedTables, CodeParameterDeclarationExpression dataSetPara, string dataSetStr, string resultStr, string updatedRowsStr, string allChangedRowsStr, string allAddedRowsStr)
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(int)), "UpdateUpdatedRows", MemberAttributes.Private);
            CodeTypeReference type = CodeGenHelper.GlobalGenericType("System.Collections.Generic.List", typeof(DataRow));
            CodeParameterDeclarationExpression expression = CodeGenHelper.ParameterDecl(type, allChangedRowsStr);
            CodeParameterDeclarationExpression expression2 = CodeGenHelper.ParameterDecl(type, allAddedRowsStr);
            method.Parameters.AddRange(new CodeParameterDeclarationExpression[] { dataSetPara, expression, expression2 });
            method.Comments.Add(CodeGenHelper.Comment("Update rows in top-down order.", true));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(typeof(int)), resultStr, CodeGenHelper.Primitive(0)));
            for (int i = 0; i < orderedTables.Length; i++)
            {
                DesignTable table = this.dataSource.DesignTables[orderedTables[i]];
                if (this.CanAddTableAdapter(table))
                {
                    method.Statements.Add(this.AddUpdateAllTAUpdate(table, dataSetStr, resultStr, updatedRowsStr, allChangedRowsStr, "ModifiedCurrent", allAddedRowsStr));
                }
            }
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable(resultStr)));
            dataComponentClass.Members.Add(method);
        }

        private void AddVariableAndProperty(CodeTypeDeclaration codeType, MemberAttributes memberAttributes, CodeTypeReference propertyType, string propertyName, string variableName, bool getOnly)
        {
            codeType.Members.Add(CodeGenHelper.FieldDecl(propertyType, variableName));
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(propertyType, propertyName, memberAttributes);
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.ThisField(variableName)));
            if (!getOnly)
            {
                property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.ThisField(variableName), CodeGenHelper.Argument("value")));
            }
            codeType.Members.Add(property);
        }

        private bool CanAddTableAdapter(DesignTable table)
        {
            if ((table != null) && table.HasAnyUpdateCommand)
            {
                switch ((((DesignConnection) table.Connection).Modifier & MemberAttributes.AccessMask))
                {
                    case MemberAttributes.FamilyOrAssembly:
                    case MemberAttributes.Assembly:
                    case MemberAttributes.Public:
                    case MemberAttributes.FamilyAndAssembly:
                        return true;
                }
            }
            return false;
        }

        private CodeStatement GetForLoopItoCount(CodeExpression countExp, CodeStatement[] forStms)
        {
            return this.GetForLoopItoCount("i", countExp, forStms);
        }

        private CodeStatement GetForLoopItoCount(string iStr, CodeExpression countExp, CodeStatement[] forStms)
        {
            CodeStatement initStmt = CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(int)), iStr, CodeGenHelper.Primitive(0));
            CodeStatement incrementStmt = CodeGenHelper.Assign(CodeGenHelper.Variable(iStr), CodeGenHelper.BinOperator(CodeGenHelper.Variable(iStr), CodeBinaryOperatorType.Add, CodeGenHelper.Primitive(1)));
            CodeExpression testExpression = CodeGenHelper.Less(CodeGenHelper.Variable(iStr), countExp);
            return CodeGenHelper.ForLoop(initStmt, testExpression, incrementStmt, forStms);
        }

        private CodeStatement HandleForEachRowInList(string listStr, string[] methods)
        {
            CodeStatement[] forStms = new CodeStatement[methods.Length + 1];
            forStms[0] = CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow)), "row", CodeGenHelper.Indexer(CodeGenHelper.Variable("rows"), CodeGenHelper.Variable("i")));
            for (int i = 0; i < methods.Length; i++)
            {
                forStms[i + 1] = CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("row"), methods[i]));
            }
            return CodeGenHelper.If(CodeGenHelper.Less(CodeGenHelper.Primitive(0), CodeGenHelper.Property(CodeGenHelper.Variable(listStr), "Count")), new CodeStatement[] { CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataRow), 1), "rows", this.NewArray(CodeGenHelper.GlobalType(typeof(DataRow), 1), CodeGenHelper.Property(CodeGenHelper.Variable(listStr), "Count"))), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(listStr), "CopyTo", CodeGenHelper.Variable("rows"))), this.GetForLoopItoCount(CodeGenHelper.Property(CodeGenHelper.Variable("rows"), "Length"), forStms) });
        }

        private CodeExpression NewArray(CodeTypeReference type, CodeExpression size)
        {
            return new CodeArrayCreateExpression(type, size);
        }

        private CodeStatement RestoreAdaptersWithACDU(string listStr)
        {
            CodeStatement[] forStms = new CodeStatement[] { CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataAdapter)), "adapter", CodeGenHelper.Indexer(CodeGenHelper.Variable("adapters"), CodeGenHelper.Variable("i"))), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("adapter"), "AcceptChangesDuringUpdate"), CodeGenHelper.Primitive(true)) };
            return CodeGenHelper.If(CodeGenHelper.Less(CodeGenHelper.Primitive(0), CodeGenHelper.Property(CodeGenHelper.Variable(listStr), "Count")), new CodeStatement[] { CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataAdapter), 1), "adapters", this.NewArray(CodeGenHelper.GlobalType(typeof(DataAdapter), 1), CodeGenHelper.Property(CodeGenHelper.Variable(listStr), "Count"))), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable(listStr), "CopyTo", CodeGenHelper.Variable("adapters"))), this.GetForLoopItoCount(CodeGenHelper.Property(CodeGenHelper.Variable("adapters"), "Length"), forStms) });
        }
    }
}

