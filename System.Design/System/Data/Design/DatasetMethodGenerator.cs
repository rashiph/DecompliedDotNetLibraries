namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;

    internal sealed class DatasetMethodGenerator
    {
        private static PropertyDescriptor caseSensitiveProperty = TypeDescriptor.GetProperties(typeof(DataSet))["CaseSensitive"];
        private TypedDataSourceCodeGenerator codeGenerator;
        private DataSet dataSet;
        private DesignDataSource dataSource;
        private CodeMemberMethod initExpressionsMethod;
        private static PropertyDescriptor localeProperty = TypeDescriptor.GetProperties(typeof(DataSet))["Locale"];
        private static PropertyDescriptor namespaceProperty = TypeDescriptor.GetProperties(typeof(DataSet))["Namespace"];

        internal DatasetMethodGenerator(TypedDataSourceCodeGenerator codeGenerator, DesignDataSource dataSource)
        {
            this.codeGenerator = codeGenerator;
            this.dataSource = dataSource;
            this.dataSet = dataSource.DataSet;
        }

        internal void AddMethods(CodeTypeDeclaration dataSourceClass)
        {
            this.AddSchemaSerializationModeMembers(dataSourceClass);
            this.initExpressionsMethod = this.InitExpressionsMethod();
            dataSourceClass.Members.Add(this.PublicConstructor());
            dataSourceClass.Members.Add(this.DeserializingConstructor());
            dataSourceClass.Members.Add(this.InitializeDerivedDataSet());
            dataSourceClass.Members.Add(this.CloneMethod(this.initExpressionsMethod));
            dataSourceClass.Members.Add(this.ShouldSerializeTablesMethod());
            dataSourceClass.Members.Add(this.ShouldSerializeRelationsMethod());
            dataSourceClass.Members.Add(this.ReadXmlSerializableMethod());
            dataSourceClass.Members.Add(this.GetSchemaSerializableMethod());
            dataSourceClass.Members.Add(this.InitVarsParamLess());
            CodeMemberMethod initClassMethod = null;
            CodeMemberMethod initVarsMethod = null;
            this.InitClassAndInitVarsMethods(out initClassMethod, out initVarsMethod);
            dataSourceClass.Members.Add(initVarsMethod);
            dataSourceClass.Members.Add(initClassMethod);
            this.AddShouldSerializeSingleTableMethods(dataSourceClass);
            dataSourceClass.Members.Add(this.SchemaChangedMethod());
            dataSourceClass.Members.Add(this.GetTypedDataSetSchema());
            dataSourceClass.Members.Add(this.TablesProperty());
            dataSourceClass.Members.Add(this.RelationsProperty());
            if (this.initExpressionsMethod != null)
            {
                dataSourceClass.Members.Add(this.initExpressionsMethod);
            }
        }

        private void AddSchemaSerializationModeMembers(CodeTypeDeclaration dataSourceClass)
        {
            CodeMemberField field = CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(SchemaSerializationMode)), "_schemaSerializationMode", CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(SchemaSerializationMode)), this.dataSource.SchemaSerializationMode.ToString()));
            dataSourceClass.Members.Add(field);
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(SchemaSerializationMode)), "SchemaSerializationMode", MemberAttributes.Public | MemberAttributes.Override);
            property.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(BrowsableAttribute).FullName, CodeGenHelper.Primitive(true)));
            property.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(DesignerSerializationVisibilityAttribute).FullName, CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DesignerSerializationVisibility)), "Visible")));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), "_schemaSerializationMode")));
            property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), "_schemaSerializationMode"), CodeGenHelper.Argument("value")));
            dataSourceClass.Members.Add(property);
        }

        private void AddShouldSerializeSingleTableMethods(CodeTypeDeclaration dataSourceClass)
        {
            foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
            {
                string generatorTablePropName = table.GeneratorTablePropName;
                if ((this.codeGenerator.CodeProvider.IsValidIdentifier(generatorTablePropName) && generatorTablePropName.StartsWith("[", StringComparison.Ordinal)) && generatorTablePropName.EndsWith("]", StringComparison.Ordinal))
                {
                    generatorTablePropName = generatorTablePropName.Substring(1, generatorTablePropName.Length - 2);
                }
                string name = MemberNameValidator.GenerateIdName("ShouldSerialize" + generatorTablePropName, this.codeGenerator.CodeProvider, false);
                CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(bool)), name, MemberAttributes.Private);
                method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Primitive(false)));
                dataSourceClass.Members.Add(method);
            }
        }

        private CodeMemberMethod CloneMethod(CodeMemberMethod initExpressionsMethod)
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(DataSet)), "Clone", MemberAttributes.Public | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(this.codeGenerator.DataSourceName), "cln", CodeGenHelper.Cast(CodeGenHelper.Type(this.codeGenerator.DataSourceName), CodeGenHelper.MethodCall(CodeGenHelper.Base(), "Clone", new CodeExpression[0]))));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Variable("cln"), "InitVars", new CodeExpression[0]));
            if (initExpressionsMethod != null)
            {
                method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Variable("cln"), "InitExpressions", new CodeExpression[0]));
            }
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("cln"), "SchemaSerializationMode"), CodeGenHelper.Property(CodeGenHelper.This(), "SchemaSerializationMode")));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable("cln")));
            return method;
        }

        private CodeConstructor DeserializingConstructor()
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Family);
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(SerializationInfo)), "info"));
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(StreamingContext)), "context"));
            constructor.BaseConstructorArgs.AddRange(new CodeExpression[] { CodeGenHelper.Argument("info"), CodeGenHelper.Argument("context"), CodeGenHelper.Primitive(false) });
            List<CodeStatement> list = new List<CodeStatement>();
            list.AddRange(new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitVars", CodeGenHelper.Primitive(false))), CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(CollectionChangeEventHandler)), "schemaChangedHandler1", new CodeDelegateCreateExpression(CodeGenHelper.GlobalType(typeof(CollectionChangeEventHandler)), CodeGenHelper.This(), "SchemaChanged")), new CodeAttachEventStatement(new CodeEventReferenceExpression(CodeGenHelper.Property(CodeGenHelper.This(), "Tables"), "CollectionChanged"), CodeGenHelper.Variable("schemaChangedHandler1")), new CodeAttachEventStatement(new CodeEventReferenceExpression(CodeGenHelper.Property(CodeGenHelper.This(), "Relations"), "CollectionChanged"), CodeGenHelper.Variable("schemaChangedHandler1")) });
            if (this.initExpressionsMethod != null)
            {
                list.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.This(), "DetermineSchemaSerializationMode", new CodeExpression[] { CodeGenHelper.Argument("info"), CodeGenHelper.Argument("context") }), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(SchemaSerializationMode)), "ExcludeSchema")), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitExpressions"))));
            }
            list.Add(CodeGenHelper.Return());
            constructor.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.This(), "IsBinarySerialized", new CodeExpression[] { CodeGenHelper.Argument("info"), CodeGenHelper.Argument("context") }), CodeGenHelper.Primitive(true)), list.ToArray()));
            constructor.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(string)), "strSchema", CodeGenHelper.Cast(CodeGenHelper.GlobalType(typeof(string)), CodeGenHelper.MethodCall(CodeGenHelper.Argument("info"), "GetValue", new CodeExpression[] { CodeGenHelper.Str("XmlSchema"), CodeGenHelper.TypeOf(CodeGenHelper.GlobalType(typeof(string))) }))));
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            list2.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataSet)), "ds", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(DataSet)), new CodeExpression[0])));
            list2.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("ds"), "ReadXmlSchema", new CodeExpression[] { CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlTextReader)), new CodeExpression[] { CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(StringReader)), new CodeExpression[] { CodeGenHelper.Variable("strSchema") }) }) })));
            foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
            {
                list2.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Tables"), CodeGenHelper.Str(table.Name)), CodeGenHelper.Primitive(null)), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Base(), "Tables"), "Add", CodeGenHelper.New(CodeGenHelper.Type(table.GeneratorTableClassName), new CodeExpression[] { CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Tables"), CodeGenHelper.Str(table.Name)) })))));
            }
            list2.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "DataSetName"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "DataSetName")));
            list2.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Prefix"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Prefix")));
            list2.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Namespace"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Namespace")));
            list2.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Locale"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Locale")));
            list2.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "CaseSensitive"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "CaseSensitive")));
            list2.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "EnforceConstraints"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "EnforceConstraints")));
            list2.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "Merge", new CodeExpression[] { CodeGenHelper.Variable("ds"), CodeGenHelper.Primitive(false), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(MissingSchemaAction)), "Add") })));
            list2.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitVars")));
            list3.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "ReadXmlSchema", new CodeExpression[] { CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlTextReader)), new CodeExpression[] { CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(StringReader)), new CodeExpression[] { CodeGenHelper.Variable("strSchema") }) }) })));
            if (this.initExpressionsMethod != null)
            {
                list3.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitExpressions")));
            }
            constructor.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.This(), "DetermineSchemaSerializationMode", new CodeExpression[] { CodeGenHelper.Argument("info"), CodeGenHelper.Argument("context") }), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(SchemaSerializationMode)), "IncludeSchema")), (CodeStatement[]) list2.ToArray(typeof(CodeStatement)), (CodeStatement[]) list3.ToArray(typeof(CodeStatement))));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "GetSerializationData", new CodeExpression[] { CodeGenHelper.Argument("info"), CodeGenHelper.Argument("context") }));
            constructor.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(CollectionChangeEventHandler)), "schemaChangedHandler", new CodeDelegateCreateExpression(CodeGenHelper.GlobalType(typeof(CollectionChangeEventHandler)), CodeGenHelper.This(), "SchemaChanged")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(CodeGenHelper.Property(CodeGenHelper.Base(), "Tables"), "CollectionChanged"), CodeGenHelper.Variable("schemaChangedHandler")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(CodeGenHelper.Property(CodeGenHelper.This(), "Relations"), "CollectionChanged"), CodeGenHelper.Variable("schemaChangedHandler")));
            return constructor;
        }

        internal static void GetSchemaIsInCollection(CodeStatementCollection statements, string dsName, string collectionName)
        {
            CodeStatement[] trueStms = new CodeStatement[] { CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("s1"), "Position"), CodeGenHelper.Primitive(0)), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("s2"), "Position"), CodeGenHelper.Primitive(0)), CodeGenHelper.ForLoop(CodeGenHelper.Stm(new CodeSnippetExpression("")), CodeGenHelper.And(CodeGenHelper.IdNotEQ(CodeGenHelper.Property(CodeGenHelper.Variable("s1"), "Position"), CodeGenHelper.Property(CodeGenHelper.Variable("s1"), "Length")), CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.Variable("s1"), "ReadByte", new CodeExpression[0]), CodeGenHelper.MethodCall(CodeGenHelper.Variable("s2"), "ReadByte", new CodeExpression[0]))), CodeGenHelper.Stm(new CodeSnippetExpression("")), new CodeStatement[] { CodeGenHelper.Stm(new CodeSnippetExpression("")) }), CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Property(CodeGenHelper.Variable("s1"), "Position"), CodeGenHelper.Property(CodeGenHelper.Variable("s1"), "Length")), new CodeStatement[] { CodeGenHelper.Return(CodeGenHelper.Variable("type")) }) };
            CodeStatement[] statementArray2 = new CodeStatement[] { CodeGenHelper.Assign(CodeGenHelper.Variable("schema"), CodeGenHelper.Cast(CodeGenHelper.GlobalType(typeof(XmlSchema)), CodeGenHelper.Property(CodeGenHelper.Variable("schemas"), "Current"))), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("s2"), "SetLength", new CodeExpression[] { CodeGenHelper.Primitive(0) })), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("schema"), "Write", new CodeExpression[] { CodeGenHelper.Variable("s2") })), CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Property(CodeGenHelper.Variable("s1"), "Length"), CodeGenHelper.Property(CodeGenHelper.Variable("s2"), "Length")), trueStms) };
            CodeStatement[] tryStmnts = new CodeStatement[] { CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchema)), "schema", CodeGenHelper.Primitive(null)), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("dsSchema"), "Write", new CodeExpression[] { CodeGenHelper.Variable("s1") })), CodeGenHelper.ForLoop(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(IEnumerator)), "schemas", CodeGenHelper.MethodCall(CodeGenHelper.MethodCall(CodeGenHelper.Variable(collectionName), "Schemas", new CodeExpression[] { CodeGenHelper.Property(CodeGenHelper.Variable("dsSchema"), "TargetNamespace") }), "GetEnumerator", new CodeExpression[0])), CodeGenHelper.MethodCall(CodeGenHelper.Variable("schemas"), "MoveNext", new CodeExpression[0]), CodeGenHelper.Stm(new CodeSnippetExpression("")), statementArray2) };
            CodeStatement[] finallyStmnts = new CodeStatement[] { CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Variable("s1"), CodeGenHelper.Primitive(null)), new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("s1"), "Close", new CodeExpression[0])) }), CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Variable("s2"), CodeGenHelper.Primitive(null)), new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("s2"), "Close", new CodeExpression[0])) }) };
            CodeStatement[] statementArray5 = new CodeStatement[] { CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(MemoryStream)), "s1", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(MemoryStream)), new CodeExpression[0])), CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(MemoryStream)), "s2", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(MemoryStream)), new CodeExpression[0])), CodeGenHelper.Try(tryStmnts, new CodeCatchClause[0], finallyStmnts) };
            statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchema)), "dsSchema", CodeGenHelper.MethodCall(CodeGenHelper.Variable(dsName), "GetSchemaSerializable", new CodeExpression[0])));
            statements.Add(CodeGenHelper.If(CodeGenHelper.MethodCall(CodeGenHelper.Variable(collectionName), "Contains", new CodeExpression[] { CodeGenHelper.Property(CodeGenHelper.Variable("dsSchema"), "TargetNamespace") }), statementArray5));
            statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Argument("xs"), "Add", new CodeExpression[] { CodeGenHelper.Variable("dsSchema") }));
        }

        private CodeMemberMethod GetSchemaSerializableMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(XmlSchema)), "GetSchemaSerializable", MemberAttributes.Family | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(MemoryStream)), "stream", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(MemoryStream)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "WriteXmlSchema", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlTextWriter)), new CodeExpression[] { CodeGenHelper.Argument("stream"), CodeGenHelper.Primitive(null) })));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Argument("stream"), "Position"), CodeGenHelper.Primitive(0)));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.GlobalTypeExpr(typeof(XmlSchema)), "Read", new CodeExpression[] { CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlTextReader)), new CodeExpression[] { CodeGenHelper.Argument("stream") }), CodeGenHelper.Primitive(null) })));
            return method;
        }

        private CodeMemberMethod GetTypedDataSetSchema()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaComplexType)), "GetTypedDataSetSchema", MemberAttributes.Public | MemberAttributes.Static);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaSet)), "xs"));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(this.dataSource.GeneratorDataSetName), "ds", CodeGenHelper.New(CodeGenHelper.Type(this.dataSource.GeneratorDataSetName), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaComplexType)), "type", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaComplexType)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaSequence)), "sequence", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaSequence)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaAny)), "any", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaAny)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any"), "Namespace"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Namespace")));
            method.Statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable("sequence"), "Items"), "Add", new CodeExpression[] { CodeGenHelper.Variable("any") })));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("type"), "Particle"), CodeGenHelper.Variable("sequence")));
            GetSchemaIsInCollection(method.Statements, "ds", "xs");
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable("type")));
            return method;
        }

        private void InitClassAndInitVarsMethods(out CodeMemberMethod initClassMethod, out CodeMemberMethod initVarsMethod)
        {
            initClassMethod = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitClass", MemberAttributes.Private);
            initVarsMethod = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final);
            initVarsMethod.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(bool)), "initTable"));
            initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "DataSetName"), CodeGenHelper.Str(this.dataSet.DataSetName)));
            initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Prefix"), CodeGenHelper.Str(this.dataSet.Prefix)));
            if (namespaceProperty.ShouldSerializeValue(this.dataSet))
            {
                initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Namespace"), CodeGenHelper.Str(this.dataSet.Namespace)));
            }
            if (localeProperty.ShouldSerializeValue(this.dataSet))
            {
                initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Locale"), CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(CultureInfo)), new CodeExpression[] { CodeGenHelper.Str(this.dataSet.Locale.ToString()) })));
            }
            if (caseSensitiveProperty.ShouldSerializeValue(this.dataSet))
            {
                initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "CaseSensitive"), CodeGenHelper.Primitive(this.dataSet.CaseSensitive)));
            }
            initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "EnforceConstraints"), CodeGenHelper.Primitive(this.dataSet.EnforceConstraints)));
            initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "SchemaSerializationMode"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(SchemaSerializationMode)), this.dataSource.SchemaSerializationMode.ToString())));
            foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
            {
                CodeExpression expression = CodeGenHelper.Field(CodeGenHelper.This(), table.GeneratorTableVarName);
                if (this.TableContainsExpressions(table))
                {
                    initClassMethod.Statements.Add(CodeGenHelper.Assign(expression, CodeGenHelper.New(CodeGenHelper.Type(table.GeneratorTableClassName), new CodeExpression[] { CodeGenHelper.Primitive(false) })));
                }
                else
                {
                    initClassMethod.Statements.Add(CodeGenHelper.Assign(expression, CodeGenHelper.New(CodeGenHelper.Type(table.GeneratorTableClassName), new CodeExpression[0])));
                }
                initClassMethod.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Base(), "Tables"), "Add", expression));
                initVarsMethod.Statements.Add(CodeGenHelper.Assign(expression, CodeGenHelper.Cast(CodeGenHelper.Type(table.GeneratorTableClassName), CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Base(), "Tables"), CodeGenHelper.Str(table.Name)))));
                initVarsMethod.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Variable("initTable"), CodeGenHelper.Primitive(true)), new CodeStatement[] { CodeGenHelper.If(CodeGenHelper.IdNotEQ(expression, CodeGenHelper.Primitive(null)), CodeGenHelper.Stm(CodeGenHelper.MethodCall(expression, "InitVars"))) }));
            }
            CodeExpression left = null;
            foreach (DesignTable table2 in this.codeGenerator.TableHandler.Tables)
            {
                DataTable dataTable = table2.DataTable;
                foreach (Constraint constraint in dataTable.Constraints)
                {
                    if (constraint is ForeignKeyConstraint)
                    {
                        ForeignKeyConstraint constraint2 = (ForeignKeyConstraint) constraint;
                        CodeArrayCreateExpression expression3 = new CodeArrayCreateExpression(CodeGenHelper.GlobalType(typeof(DataColumn)), 0);
                        foreach (DataColumn column in constraint2.Columns)
                        {
                            expression3.Initializers.Add(CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), this.codeGenerator.TableHandler.Tables[column.Table.TableName].GeneratorTableVarName), this.codeGenerator.TableHandler.Tables[column.Table.TableName].DesignColumns[column.ColumnName].GeneratorColumnPropNameInTable));
                        }
                        CodeArrayCreateExpression expression4 = new CodeArrayCreateExpression(CodeGenHelper.GlobalType(typeof(DataColumn)), 0);
                        foreach (DataColumn column2 in constraint2.RelatedColumns)
                        {
                            expression4.Initializers.Add(CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), this.codeGenerator.TableHandler.Tables[column2.Table.TableName].GeneratorTableVarName), this.codeGenerator.TableHandler.Tables[column2.Table.TableName].DesignColumns[column2.ColumnName].GeneratorColumnPropNameInTable));
                        }
                        if (left == null)
                        {
                            initClassMethod.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(ForeignKeyConstraint)), "fkc"));
                            left = CodeGenHelper.Variable("fkc");
                        }
                        initClassMethod.Statements.Add(CodeGenHelper.Assign(left, CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(ForeignKeyConstraint)), new CodeExpression[] { CodeGenHelper.Str(constraint2.ConstraintName), expression4, expression3 })));
                        initClassMethod.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), this.codeGenerator.TableHandler.Tables[dataTable.TableName].GeneratorTableVarName), "Constraints"), "Add", left));
                        string field = constraint2.AcceptRejectRule.ToString();
                        string str2 = constraint2.DeleteRule.ToString();
                        string str3 = constraint2.UpdateRule.ToString();
                        initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(left, "AcceptRejectRule"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(constraint2.AcceptRejectRule.GetType()), field)));
                        initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(left, "DeleteRule"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(constraint2.DeleteRule.GetType()), str2)));
                        initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(left, "UpdateRule"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(constraint2.UpdateRule.GetType()), str3)));
                    }
                }
            }
            foreach (DesignRelation relation in this.codeGenerator.RelationHandler.Relations)
            {
                DataRelation dataRelation = relation.DataRelation;
                if (dataRelation != null)
                {
                    CodeArrayCreateExpression expression5 = new CodeArrayCreateExpression(CodeGenHelper.GlobalType(typeof(DataColumn)), 0);
                    string generatorTableVarName = relation.ParentDesignTable.GeneratorTableVarName;
                    foreach (DataColumn column3 in dataRelation.ParentColumns)
                    {
                        expression5.Initializers.Add(CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), generatorTableVarName), this.codeGenerator.TableHandler.Tables[column3.Table.TableName].DesignColumns[column3.ColumnName].GeneratorColumnPropNameInTable));
                    }
                    CodeArrayCreateExpression expression6 = new CodeArrayCreateExpression(CodeGenHelper.GlobalType(typeof(DataColumn)), 0);
                    string str5 = relation.ChildDesignTable.GeneratorTableVarName;
                    foreach (DataColumn column4 in dataRelation.ChildColumns)
                    {
                        expression6.Initializers.Add(CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), str5), this.codeGenerator.TableHandler.Tables[column4.Table.TableName].DesignColumns[column4.ColumnName].GeneratorColumnPropNameInTable));
                    }
                    CodeExpression expression7 = CodeGenHelper.Field(CodeGenHelper.This(), this.codeGenerator.RelationHandler.Relations[dataRelation.RelationName].GeneratorRelationVarName);
                    initClassMethod.Statements.Add(CodeGenHelper.Assign(expression7, CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(DataRelation)), new CodeExpression[] { CodeGenHelper.Str(dataRelation.RelationName), expression5, expression6, CodeGenHelper.Primitive(false) })));
                    if (dataRelation.Nested)
                    {
                        initClassMethod.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression7, "Nested"), CodeGenHelper.Primitive(true)));
                    }
                    ExtendedPropertiesHandler.CodeGenerator = this.codeGenerator;
                    ExtendedPropertiesHandler.AddExtendedProperties(relation, expression7, initClassMethod.Statements, dataRelation.ExtendedProperties);
                    initClassMethod.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), "Relations"), "Add", expression7));
                    initVarsMethod.Statements.Add(CodeGenHelper.Assign(expression7, CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.This(), "Relations"), CodeGenHelper.Str(dataRelation.RelationName))));
                }
            }
            ExtendedPropertiesHandler.CodeGenerator = this.codeGenerator;
            ExtendedPropertiesHandler.AddExtendedProperties(this.dataSource, CodeGenHelper.This(), initClassMethod.Statements, this.dataSet.ExtendedProperties);
        }

        private CodeMemberMethod InitExpressionsMethod()
        {
            bool flag = false;
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitExpressions", MemberAttributes.Private);
            foreach (DesignTable table in this.dataSource.DesignTables)
            {
                foreach (DataColumn column in table.DataTable.Columns)
                {
                    if (column.Expression.Length > 0)
                    {
                        CodeExpression exp = CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), table.GeneratorTablePropName), this.codeGenerator.TableHandler.Tables[column.Table.TableName].DesignColumns[column.ColumnName].GeneratorColumnPropNameInTable);
                        flag = true;
                        method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(exp, "Expression"), CodeGenHelper.Str(column.Expression)));
                    }
                }
            }
            if (flag)
            {
                return method;
            }
            return null;
        }

        private CodeMemberMethod InitializeDerivedDataSet()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitializeDerivedDataSet", MemberAttributes.Family | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "BeginInit"));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitClass"));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "EndInit"));
            return method;
        }

        private CodeMemberMethod InitVarsParamLess()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final);
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitVars", new CodeExpression[] { CodeGenHelper.Primitive(true) }));
            return method;
        }

        private CodeConstructor PublicConstructor()
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Public);
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "BeginInit"));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitClass"));
            constructor.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(CollectionChangeEventHandler)), "schemaChangedHandler", new CodeDelegateCreateExpression(CodeGenHelper.GlobalType(typeof(CollectionChangeEventHandler)), CodeGenHelper.This(), "SchemaChanged")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(CodeGenHelper.Property(CodeGenHelper.Base(), "Tables"), "CollectionChanged"), CodeGenHelper.Variable("schemaChangedHandler")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(CodeGenHelper.Property(CodeGenHelper.Base(), "Relations"), "CollectionChanged"), CodeGenHelper.Variable("schemaChangedHandler")));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "EndInit"));
            if (this.initExpressionsMethod != null)
            {
                constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitExpressions"));
            }
            return constructor;
        }

        private CodeMemberMethod ReadXmlSerializableMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "ReadXmlSerializable", MemberAttributes.Family | MemberAttributes.Override);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(XmlReader)), "reader"));
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            list.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "Reset", new CodeExpression[0])));
            list.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(DataSet)), "ds", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(DataSet)), new CodeExpression[0])));
            list.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Variable("ds"), "ReadXml", new CodeExpression[] { CodeGenHelper.Argument("reader") })));
            foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
            {
                list.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Tables"), CodeGenHelper.Str(table.Name)), CodeGenHelper.Primitive(null)), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Base(), "Tables"), "Add", CodeGenHelper.New(CodeGenHelper.Type(table.GeneratorTableClassName), new CodeExpression[] { CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Tables"), CodeGenHelper.Str(table.Name)) })))));
            }
            list.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "DataSetName"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "DataSetName")));
            list.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Prefix"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Prefix")));
            list.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Namespace"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Namespace")));
            list.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Locale"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Locale")));
            list.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "CaseSensitive"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "CaseSensitive")));
            list.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "EnforceConstraints"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "EnforceConstraints")));
            list.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "Merge", new CodeExpression[] { CodeGenHelper.Variable("ds"), CodeGenHelper.Primitive(false), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(MissingSchemaAction)), "Add") })));
            list.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitVars")));
            list2.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "ReadXml", new CodeExpression[] { CodeGenHelper.Argument("reader") })));
            list2.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitVars")));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.MethodCall(CodeGenHelper.This(), "DetermineSchemaSerializationMode", new CodeExpression[] { CodeGenHelper.Argument("reader") }), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(SchemaSerializationMode)), "IncludeSchema")), (CodeStatement[]) list.ToArray(typeof(CodeStatement)), (CodeStatement[]) list2.ToArray(typeof(CodeStatement))));
            return method;
        }

        private CodeMemberProperty RelationsProperty()
        {
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(DataRelationCollection)), DataSourceNameHandler.RelationsPropertyName, MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final);
            property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerSerializationVisibilityAttribute", CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DesignerSerializationVisibility)), "Hidden")));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Property(CodeGenHelper.Base(), "Relations")));
            return property;
        }

        private CodeMemberMethod SchemaChangedMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "SchemaChanged", MemberAttributes.Private);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(object)), "sender"));
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(CollectionChangeEventArgs)), "e"));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Property(CodeGenHelper.Argument("e"), "Action"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(CollectionChangeAction)), "Remove")), CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitVars"))));
            return method;
        }

        private CodeMemberMethod ShouldSerializeRelationsMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(bool)), "ShouldSerializeRelations", MemberAttributes.Family | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Primitive(false)));
            return method;
        }

        private CodeMemberMethod ShouldSerializeTablesMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(bool)), "ShouldSerializeTables", MemberAttributes.Family | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Primitive(false)));
            return method;
        }

        private bool TableContainsExpressions(DesignTable designTable)
        {
            foreach (DataColumn column in designTable.DataTable.Columns)
            {
                if (column.Expression.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private CodeMemberProperty TablesProperty()
        {
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(DataTableCollection)), DataSourceNameHandler.TablesPropertyName, MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final);
            property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerSerializationVisibilityAttribute", CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DesignerSerializationVisibility)), "Hidden")));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Property(CodeGenHelper.Base(), "Tables")));
            return property;
        }
    }
}

