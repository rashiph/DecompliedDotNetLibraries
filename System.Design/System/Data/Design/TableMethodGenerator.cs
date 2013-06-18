namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml.Schema;

    internal sealed class TableMethodGenerator
    {
        private static PropertyDescriptor caseSensitiveProperty = TypeDescriptor.GetProperties(typeof(DataTable))["CaseSensitive"];
        private TypedDataSourceCodeGenerator codeGenerator;
        private static PropertyDescriptor columnNamespaceProperty = TypeDescriptor.GetProperties(typeof(DataColumn))["Namespace"];
        private static string columnValuesArrayName = "columnValuesArray";
        private static PropertyDescriptor dateTimeModeProperty = TypeDescriptor.GetProperties(typeof(DataColumn))["DateTimeMode"];
        private DesignTable designTable;
        private CodeMemberMethod initExpressionsMethod;
        private static PropertyDescriptor localeProperty = TypeDescriptor.GetProperties(typeof(DataTable))["Locale"];
        private static PropertyDescriptor namespaceProperty = TypeDescriptor.GetProperties(typeof(DataTable))["Namespace"];
        private string rowClassName;
        private string rowConcreteClassName;
        private string tableClassName;

        internal TableMethodGenerator(TypedDataSourceCodeGenerator codeGenerator, DesignTable designTable)
        {
            this.codeGenerator = codeGenerator;
            this.designTable = designTable;
        }

        private void AddFindByMethods(CodeTypeDeclaration dataTableClass)
        {
            DataTable dataTable = this.designTable.DataTable;
            for (int i = 0; i < dataTable.Constraints.Count; i++)
            {
                if ((dataTable.Constraints[i] is UniqueConstraint) && ((UniqueConstraint) dataTable.Constraints[i]).IsPrimaryKey)
                {
                    DataColumn[] columns = ((UniqueConstraint) dataTable.Constraints[i]).Columns;
                    string inVarName = "FindBy";
                    bool flag = true;
                    for (int j = 0; j < columns.Length; j++)
                    {
                        inVarName = inVarName + this.codeGenerator.TableHandler.Tables[columns[j].Table.TableName].DesignColumns[columns[j].ColumnName].GeneratorColumnPropNameInRow;
                        if (columns[j].ColumnMapping != MappingType.Hidden)
                        {
                            flag = false;
                        }
                    }
                    if (!flag)
                    {
                        CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.Type(this.rowClassName), NameHandler.FixIdName(inVarName), MemberAttributes.Public | MemberAttributes.Final);
                        for (int k = 0; k < columns.Length; k++)
                        {
                            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(columns[k].DataType), this.codeGenerator.TableHandler.Tables[columns[k].Table.TableName].DesignColumns[columns[k].ColumnName].GeneratorColumnPropNameInRow));
                        }
                        CodeArrayCreateExpression par = new CodeArrayCreateExpression(typeof(object), columns.Length);
                        for (int m = 0; m < columns.Length; m++)
                        {
                            par.Initializers.Add(CodeGenHelper.Argument(this.codeGenerator.TableHandler.Tables[columns[m].Table.TableName].DesignColumns[columns[m].ColumnName].GeneratorColumnPropNameInRow));
                        }
                        method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Cast(CodeGenHelper.Type(this.rowClassName), CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), "Rows"), "Find", par))));
                        dataTableClass.Members.Add(method);
                    }
                }
            }
        }

        internal void AddMethods(CodeTypeDeclaration dataTableClass)
        {
            if (dataTableClass == null)
            {
                throw new InternalException("Table CodeTypeDeclaration should not be null.");
            }
            this.rowClassName = this.designTable.GeneratorRowClassName;
            this.rowConcreteClassName = this.designTable.GeneratorRowClassName;
            this.tableClassName = this.designTable.GeneratorTableClassName;
            this.initExpressionsMethod = this.InitExpressionsMethod();
            if (this.initExpressionsMethod != null)
            {
                dataTableClass.Members.Add(this.ArgumentLessConstructorInitExpressions());
                dataTableClass.Members.Add(this.ConstructorWithBoolArgument());
            }
            else
            {
                dataTableClass.Members.Add(this.ArgumentLessConstructorNoInitExpressions());
            }
            dataTableClass.Members.Add(this.ConstructorWithArguments());
            dataTableClass.Members.Add(this.DeserializingConstructor());
            dataTableClass.Members.Add(this.AddTypedRowMethod());
            this.AddTypedRowByColumnsMethods(dataTableClass);
            this.AddFindByMethods(dataTableClass);
            if ((this.codeGenerator.GenerateOptions & System.Data.Design.TypedDataSetGenerator.GenerateOption.LinqOverTypedDatasets) != System.Data.Design.TypedDataSetGenerator.GenerateOption.LinqOverTypedDatasets)
            {
                dataTableClass.Members.Add(this.GetEnumeratorMethod());
            }
            dataTableClass.Members.Add(this.CloneMethod());
            dataTableClass.Members.Add(this.CreateInstanceMethod());
            CodeMemberMethod tableInitClass = null;
            CodeMemberMethod tableInitVars = null;
            this.InitClassAndInitVarsMethods(dataTableClass, out tableInitClass, out tableInitVars);
            dataTableClass.Members.Add(tableInitVars);
            dataTableClass.Members.Add(tableInitClass);
            dataTableClass.Members.Add(this.NewTypedRowMethod());
            dataTableClass.Members.Add(this.NewRowFromBuilderMethod());
            dataTableClass.Members.Add(this.GetRowTypeMethod());
            if (this.initExpressionsMethod != null)
            {
                dataTableClass.Members.Add(this.initExpressionsMethod);
            }
            if (this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareEvents) && this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareDelegates))
            {
                this.AddOnRowEventMethods(dataTableClass);
            }
            dataTableClass.Members.Add(this.RemoveRowMethod());
            dataTableClass.Members.Add(this.GetTypedTableSchema());
        }

        private void AddOnRowEventMethods(CodeTypeDeclaration dataTableClass)
        {
            dataTableClass.Members.Add(this.CreateOnRowEventMethod("Changed", this.designTable.GeneratorRowChangedName));
            dataTableClass.Members.Add(this.CreateOnRowEventMethod("Changing", this.designTable.GeneratorRowChangingName));
            dataTableClass.Members.Add(this.CreateOnRowEventMethod("Deleted", this.designTable.GeneratorRowDeletedName));
            dataTableClass.Members.Add(this.CreateOnRowEventMethod("Deleting", this.designTable.GeneratorRowDeletingName));
        }

        private void AddTypedRowByColumnsMethods(CodeTypeDeclaration dataTableClass)
        {
            DataTable dataTable = this.designTable.DataTable;
            ArrayList list = new ArrayList();
            bool flag = false;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (!dataTable.Columns[i].AutoIncrement)
                {
                    list.Add(dataTable.Columns[i]);
                }
            }
            string name = NameHandler.FixIdName("Add" + this.rowClassName);
            GenericNameHandler handler = new GenericNameHandler(new string[] { name, columnValuesArrayName }, this.codeGenerator.CodeProvider);
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.Type(this.rowConcreteClassName), name, MemberAttributes.Public | MemberAttributes.Final);
            CodeMemberMethod method2 = CodeGenHelper.MethodDecl(CodeGenHelper.Type(this.rowConcreteClassName), name, MemberAttributes.Public | MemberAttributes.Final);
            DataColumn[] array = new DataColumn[list.Count];
            list.CopyTo(array, 0);
            for (int j = 0; j < array.Length; j++)
            {
                Type dataType = array[j].DataType;
                DataRelation relation = this.FindParentRelation(array[j]);
                if (this.ChildRelationFollowable(relation))
                {
                    string generatorRowClassName = this.codeGenerator.TableHandler.Tables[relation.ParentTable.TableName].GeneratorRowClassName;
                    string originalName = NameHandler.FixIdName("parent" + generatorRowClassName + "By" + relation.RelationName);
                    method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(generatorRowClassName), handler.AddNameToList(originalName)));
                    method2.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(generatorRowClassName), handler.GetNameFromList(originalName)));
                }
                else
                {
                    method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(dataType), handler.AddNameToList(this.codeGenerator.TableHandler.Tables[array[j].Table.TableName].DesignColumns[array[j].ColumnName].GeneratorColumnPropNameInRow)));
                    if (StringUtil.Empty(array[j].Expression))
                    {
                        method2.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(dataType), handler.GetNameFromList(this.codeGenerator.TableHandler.Tables[array[j].Table.TableName].DesignColumns[array[j].ColumnName].GeneratorColumnPropNameInRow)));
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            CodeStatement statement = CodeGenHelper.VariableDecl(CodeGenHelper.Type(this.rowConcreteClassName), NameHandler.FixIdName("row" + this.rowClassName), CodeGenHelper.Cast(CodeGenHelper.Type(this.rowConcreteClassName), CodeGenHelper.MethodCall(CodeGenHelper.This(), "NewRow")));
            method.Statements.Add(statement);
            method2.Statements.Add(statement);
            CodeExpression exp = CodeGenHelper.Variable(NameHandler.FixIdName("row" + this.rowClassName));
            CodeAssignStatement statement2 = new CodeAssignStatement {
                Left = CodeGenHelper.Property(exp, "ItemArray")
            };
            CodeArrayCreateExpression initExpr = new CodeArrayCreateExpression {
                CreateType = CodeGenHelper.GlobalType(typeof(object))
            };
            CodeArrayCreateExpression expression3 = new CodeArrayCreateExpression {
                CreateType = CodeGenHelper.GlobalType(typeof(object))
            };
            array = new DataColumn[dataTable.Columns.Count];
            dataTable.Columns.CopyTo(array, 0);
            for (int k = 0; k < array.Length; k++)
            {
                if (array[k].AutoIncrement)
                {
                    initExpr.Initializers.Add(CodeGenHelper.Primitive(null));
                    expression3.Initializers.Add(CodeGenHelper.Primitive(null));
                }
                else
                {
                    DataRelation relation2 = this.FindParentRelation(array[k]);
                    if (this.ChildRelationFollowable(relation2))
                    {
                        initExpr.Initializers.Add(CodeGenHelper.Primitive(null));
                        expression3.Initializers.Add(CodeGenHelper.Primitive(null));
                    }
                    else
                    {
                        initExpr.Initializers.Add(CodeGenHelper.Argument(handler.GetNameFromList(this.codeGenerator.TableHandler.Tables[array[k].Table.TableName].DesignColumns[array[k].ColumnName].GeneratorColumnPropNameInRow)));
                        if (StringUtil.Empty(array[k].Expression))
                        {
                            expression3.Initializers.Add(CodeGenHelper.Argument(handler.GetNameFromList(this.codeGenerator.TableHandler.Tables[array[k].Table.TableName].DesignColumns[array[k].ColumnName].GeneratorColumnPropNameInRow)));
                        }
                        else
                        {
                            expression3.Initializers.Add(CodeGenHelper.Primitive(null));
                        }
                    }
                }
            }
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(object), 1), columnValuesArrayName, initExpr));
            method2.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(object), 1), columnValuesArrayName, expression3));
            for (int m = 0; m < array.Length; m++)
            {
                if (!array[m].AutoIncrement)
                {
                    DataRelation relation3 = this.FindParentRelation(array[m]);
                    if (this.ChildRelationFollowable(relation3))
                    {
                        string str5 = NameHandler.FixIdName("parent" + this.codeGenerator.TableHandler.Tables[relation3.ParentTable.TableName].GeneratorRowClassName + "By" + relation3.RelationName);
                        CodeStatement statement3 = CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Argument(handler.GetNameFromList(str5)), CodeGenHelper.Primitive(null)), CodeGenHelper.Assign(CodeGenHelper.Indexer(CodeGenHelper.Variable(columnValuesArrayName), CodeGenHelper.Primitive(m)), CodeGenHelper.Indexer(CodeGenHelper.Argument(handler.GetNameFromList(str5)), CodeGenHelper.Primitive(relation3.ParentColumns[0].Ordinal))));
                        method.Statements.Add(statement3);
                        method2.Statements.Add(statement3);
                    }
                }
            }
            statement2.Right = CodeGenHelper.Variable(columnValuesArrayName);
            method.Statements.Add(statement2);
            method2.Statements.Add(statement2);
            CodeExpression expression4 = CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), "Rows"), "Add", exp);
            method.Statements.Add(expression4);
            method2.Statements.Add(expression4);
            method.Statements.Add(CodeGenHelper.Return(exp));
            method2.Statements.Add(CodeGenHelper.Return(exp));
            dataTableClass.Members.Add(method);
            if (flag)
            {
                dataTableClass.Members.Add(method2);
            }
        }

        private CodeMemberMethod AddTypedRowMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), NameHandler.FixIdName("Add" + this.rowClassName), MemberAttributes.Public | MemberAttributes.Final);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(this.rowConcreteClassName), "row"));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), "Rows"), "Add", CodeGenHelper.Argument("row")));
            return method;
        }

        private CodeConstructor ArgumentLessConstructorInitExpressions()
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Public | MemberAttributes.Final);
            constructor.ChainedConstructorArgs.Add(CodeGenHelper.Primitive(false));
            return constructor;
        }

        private CodeConstructor ArgumentLessConstructorNoInitExpressions()
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Public | MemberAttributes.Final);
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "TableName"), CodeGenHelper.Str(this.designTable.Name)));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "BeginInit"));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitClass"));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "EndInit"));
            return constructor;
        }

        private bool ChildRelationFollowable(DataRelation relation)
        {
            if (relation == null)
            {
                return false;
            }
            if ((relation.ChildTable == relation.ParentTable) && (relation.ChildTable.Columns.Count == 1))
            {
                return false;
            }
            return true;
        }

        private CodeMemberMethod CloneMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(DataTable)), "Clone", MemberAttributes.Public | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(this.tableClassName), "cln", CodeGenHelper.Cast(CodeGenHelper.Type(this.tableClassName), CodeGenHelper.MethodCall(CodeGenHelper.Base(), "Clone", new CodeExpression[0]))));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Variable("cln"), "InitVars", new CodeExpression[0]));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable("cln")));
            return method;
        }

        private CodeConstructor ConstructorWithArguments()
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Assembly | MemberAttributes.Final);
            constructor.Attributes = MemberAttributes.Assembly | MemberAttributes.Final;
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataTable)), "table"));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "TableName"), CodeGenHelper.Property(CodeGenHelper.Argument("table"), "TableName")));
            constructor.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Property(CodeGenHelper.Argument("table"), "CaseSensitive"), CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Argument("table"), "DataSet"), "CaseSensitive")), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "CaseSensitive"), CodeGenHelper.Property(CodeGenHelper.Argument("table"), "CaseSensitive"))));
            constructor.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Argument("table"), "Locale"), "ToString"), CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Argument("table"), "DataSet"), "Locale"), "ToString")), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Locale"), CodeGenHelper.Property(CodeGenHelper.Argument("table"), "Locale"))));
            constructor.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Property(CodeGenHelper.Argument("table"), "Namespace"), CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.Argument("table"), "DataSet"), "Namespace")), CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Namespace"), CodeGenHelper.Property(CodeGenHelper.Argument("table"), "Namespace"))));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Prefix"), CodeGenHelper.Property(CodeGenHelper.Argument("table"), "Prefix")));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "MinimumCapacity"), CodeGenHelper.Property(CodeGenHelper.Argument("table"), "MinimumCapacity")));
            return constructor;
        }

        private CodeConstructor ConstructorWithBoolArgument()
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Assembly | MemberAttributes.Final);
            constructor.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(bool)), "initExpressions"));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "TableName"), CodeGenHelper.Str(this.designTable.Name)));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "BeginInit"));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitClass"));
            constructor.Statements.Add(CodeGenHelper.If(CodeGenHelper.EQ(CodeGenHelper.Argument("initExpressions"), CodeGenHelper.Primitive(true)), new CodeStatement[] { CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitExpressions")) }));
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "EndInit"));
            return constructor;
        }

        private CodeMemberMethod CreateInstanceMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(DataTable)), "CreateInstance", MemberAttributes.Family | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.New(CodeGenHelper.Type(this.tableClassName), new CodeExpression[0])));
            return method;
        }

        private CodeMemberMethod CreateOnRowEventMethod(string eventName, string typedEventName)
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "OnRow" + eventName, MemberAttributes.Family | MemberAttributes.Override);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRowChangeEventArgs)), "e"));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Base(), "OnRow" + eventName, CodeGenHelper.Argument("e")));
            method.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdNotEQ(CodeGenHelper.Event(typedEventName), CodeGenHelper.Primitive(null)), CodeGenHelper.Stm(CodeGenHelper.DelegateCall(CodeGenHelper.Event(typedEventName), CodeGenHelper.New(CodeGenHelper.Type(this.designTable.GeneratorRowEvArgName), new CodeExpression[] { CodeGenHelper.Cast(CodeGenHelper.Type(this.rowClassName), CodeGenHelper.Property(CodeGenHelper.Argument("e"), "Row")), CodeGenHelper.Property(CodeGenHelper.Argument("e"), "Action") })))));
            return method;
        }

        private CodeConstructor DeserializingConstructor()
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Family);
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(SerializationInfo)), "info"));
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(StreamingContext)), "context"));
            constructor.BaseConstructorArgs.AddRange(new CodeExpression[] { CodeGenHelper.Argument("info"), CodeGenHelper.Argument("context") });
            constructor.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "InitVars"));
            return constructor;
        }

        private DataRelation FindParentRelation(DataColumn column)
        {
            DataRelation[] array = new DataRelation[column.Table.ParentRelations.Count];
            column.Table.ParentRelations.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                DataRelation relation = array[i];
                if ((relation.ChildColumns.Length == 1) && (relation.ChildColumns[0] == column))
                {
                    return relation;
                }
            }
            return null;
        }

        private CodeMemberMethod GetEnumeratorMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(IEnumerator)), "GetEnumerator", MemberAttributes.Public);
            method.ImplementationTypes.Add(CodeGenHelper.GlobalType(typeof(IEnumerable)));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), "Rows"), "GetEnumerator")));
            return method;
        }

        private CodeMemberMethod GetRowTypeMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(Type)), "GetRowType", MemberAttributes.Family | MemberAttributes.Override);
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.TypeOf(CodeGenHelper.Type(this.rowConcreteClassName))));
            return method;
        }

        private CodeMemberMethod GetTypedTableSchema()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaComplexType)), "GetTypedTableSchema", MemberAttributes.Public | MemberAttributes.Static);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaSet)), "xs"));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaComplexType)), "type", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaComplexType)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaSequence)), "sequence", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaSequence)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.Type(this.codeGenerator.DataSourceName), "ds", CodeGenHelper.New(CodeGenHelper.Type(this.codeGenerator.DataSourceName), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaAny)), "any1", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaAny)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any1"), "Namespace"), CodeGenHelper.Str("http://www.w3.org/2001/XMLSchema")));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any1"), "MinOccurs"), CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(decimal)), new CodeExpression[] { CodeGenHelper.Primitive(0) })));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any1"), "MaxOccurs"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(decimal)), "MaxValue")));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any1"), "ProcessContents"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(XmlSchemaContentProcessing)), "Lax")));
            method.Statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable("sequence"), "Items"), "Add", new CodeExpression[] { CodeGenHelper.Variable("any1") })));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaAny)), "any2", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaAny)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any2"), "Namespace"), CodeGenHelper.Str("urn:schemas-microsoft-com:xml-diffgram-v1")));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any2"), "MinOccurs"), CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(decimal)), new CodeExpression[] { CodeGenHelper.Primitive(1) })));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("any2"), "ProcessContents"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(XmlSchemaContentProcessing)), "Lax")));
            method.Statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable("sequence"), "Items"), "Add", new CodeExpression[] { CodeGenHelper.Variable("any2") })));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaAttribute)), "attribute1", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaAttribute)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("attribute1"), "Name"), CodeGenHelper.Primitive("namespace")));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("attribute1"), "FixedValue"), CodeGenHelper.Property(CodeGenHelper.Variable("ds"), "Namespace")));
            method.Statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable("type"), "Attributes"), "Add", new CodeExpression[] { CodeGenHelper.Variable("attribute1") })));
            method.Statements.Add(CodeGenHelper.VariableDecl(CodeGenHelper.GlobalType(typeof(XmlSchemaAttribute)), "attribute2", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(XmlSchemaAttribute)), new CodeExpression[0])));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("attribute2"), "Name"), CodeGenHelper.Primitive("tableTypeName")));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("attribute2"), "FixedValue"), CodeGenHelper.Str(this.designTable.GeneratorTableClassName)));
            method.Statements.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Variable("type"), "Attributes"), "Add", new CodeExpression[] { CodeGenHelper.Variable("attribute2") })));
            method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.Variable("type"), "Particle"), CodeGenHelper.Variable("sequence")));
            DatasetMethodGenerator.GetSchemaIsInCollection(method.Statements, "ds", "xs");
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Variable("type")));
            return method;
        }

        private void InitClassAndInitVarsMethods(CodeTypeDeclaration tableClass, out CodeMemberMethod tableInitClass, out CodeMemberMethod tableInitVars)
        {
            DataTable dataTable = this.designTable.DataTable;
            tableInitClass = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitClass", MemberAttributes.Private);
            tableInitVars = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final);
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn column = dataTable.Columns[i];
                string generatorColumnVarNameInTable = this.codeGenerator.TableHandler.Tables[dataTable.TableName].DesignColumns[column.ColumnName].GeneratorColumnVarNameInTable;
                CodeExpression left = CodeGenHelper.Field(CodeGenHelper.This(), generatorColumnVarNameInTable);
                string str2 = "Element";
                if (column.ColumnMapping == MappingType.SimpleContent)
                {
                    str2 = "SimpleContent";
                }
                else if (column.ColumnMapping == MappingType.Attribute)
                {
                    str2 = "Attribute";
                }
                else if (column.ColumnMapping == MappingType.Hidden)
                {
                    str2 = "Hidden";
                }
                tableInitClass.Statements.Add(CodeGenHelper.Assign(left, CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(DataColumn)), new CodeExpression[] { CodeGenHelper.Str(column.ColumnName), CodeGenHelper.TypeOf(CodeGenHelper.GlobalType(column.DataType)), CodeGenHelper.Primitive(null), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(MappingType)), str2) })));
                ExtendedPropertiesHandler.CodeGenerator = this.codeGenerator;
                ExtendedPropertiesHandler.AddExtendedProperties(this.designTable.DesignColumns[column.ColumnName], left, tableInitClass.Statements, column.ExtendedProperties);
                tableInitClass.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.Base(), "Columns"), "Add", CodeGenHelper.Field(CodeGenHelper.This(), generatorColumnVarNameInTable)));
            }
            for (int j = 0; j < dataTable.Constraints.Count; j++)
            {
                if (dataTable.Constraints[j] is UniqueConstraint)
                {
                    UniqueConstraint constraint = (UniqueConstraint) dataTable.Constraints[j];
                    DataColumn[] columns = constraint.Columns;
                    CodeExpression[] initializers = new CodeExpression[columns.Length];
                    for (int m = 0; m < columns.Length; m++)
                    {
                        initializers[m] = CodeGenHelper.Field(CodeGenHelper.This(), this.codeGenerator.TableHandler.Tables[columns[m].Table.TableName].DesignColumns[columns[m].ColumnName].GeneratorColumnVarNameInTable);
                    }
                    tableInitClass.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), "Constraints"), "Add", CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(UniqueConstraint)), new CodeExpression[] { CodeGenHelper.Str(constraint.ConstraintName), new CodeArrayCreateExpression(CodeGenHelper.GlobalType(typeof(DataColumn)), initializers), CodeGenHelper.Primitive(constraint.IsPrimaryKey) })));
                }
            }
            for (int k = 0; k < dataTable.Columns.Count; k++)
            {
                DataColumn component = dataTable.Columns[k];
                string str3 = this.codeGenerator.TableHandler.Tables[dataTable.TableName].DesignColumns[component.ColumnName].GeneratorColumnVarNameInTable;
                CodeExpression expression2 = CodeGenHelper.Field(CodeGenHelper.This(), str3);
                tableInitVars.Statements.Add(CodeGenHelper.Assign(expression2, CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Base(), "Columns"), CodeGenHelper.Str(component.ColumnName))));
                if (component.AutoIncrement)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "AutoIncrement"), CodeGenHelper.Primitive(true)));
                }
                if (component.AutoIncrementSeed != 0L)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "AutoIncrementSeed"), CodeGenHelper.Primitive(component.AutoIncrementSeed)));
                }
                if (component.AutoIncrementStep != 1L)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "AutoIncrementStep"), CodeGenHelper.Primitive(component.AutoIncrementStep)));
                }
                if (!component.AllowDBNull)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "AllowDBNull"), CodeGenHelper.Primitive(false)));
                }
                if (component.ReadOnly)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "ReadOnly"), CodeGenHelper.Primitive(true)));
                }
                if (component.Unique)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "Unique"), CodeGenHelper.Primitive(true)));
                }
                if (!StringUtil.Empty(component.Prefix))
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "Prefix"), CodeGenHelper.Str(component.Prefix)));
                }
                if (columnNamespaceProperty.ShouldSerializeValue(component))
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "Namespace"), CodeGenHelper.Str(component.Namespace)));
                }
                if (component.Caption != component.ColumnName)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "Caption"), CodeGenHelper.Str(component.Caption)));
                }
                if (component.DefaultValue != DBNull.Value)
                {
                    CodeExpression valueExpr = null;
                    CodeExpression fieldInit = null;
                    DesignColumn designColumn = this.codeGenerator.TableHandler.Tables[dataTable.TableName].DesignColumns[component.ColumnName];
                    DSGeneratorProblem problem = CodeGenHelper.GenerateValueExprAndFieldInit(designColumn, component.DefaultValue, component.DefaultValue, this.designTable.GeneratorTableClassName, str3 + "_defaultValue", out valueExpr, out fieldInit);
                    if (problem != null)
                    {
                        this.codeGenerator.ProblemList.Add(problem);
                    }
                    else
                    {
                        if (fieldInit != null)
                        {
                            CodeMemberField field = CodeGenHelper.FieldDecl(CodeGenHelper.Type(component.DataType.FullName), str3 + "_defaultValue");
                            field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                            field.InitExpression = fieldInit;
                            tableClass.Members.Add(field);
                        }
                        CodeCastExpression right = new CodeCastExpression(component.DataType, valueExpr);
                        right.UserData.Add("CastIsBoxing", true);
                        tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "DefaultValue"), right));
                    }
                }
                if (component.MaxLength != -1)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "MaxLength"), CodeGenHelper.Primitive(component.MaxLength)));
                }
                if (component.DateTimeMode != DataSetDateTime.UnspecifiedLocal)
                {
                    tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(expression2, "DateTimeMode"), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DataSetDateTime)), component.DateTimeMode.ToString())));
                }
            }
            if (caseSensitiveProperty.ShouldSerializeValue(dataTable))
            {
                tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "CaseSensitive"), CodeGenHelper.Primitive(dataTable.CaseSensitive)));
            }
            if ((dataTable.Locale != null) && localeProperty.ShouldSerializeValue(dataTable))
            {
                tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Locale"), CodeGenHelper.New(CodeGenHelper.GlobalType(typeof(CultureInfo)), new CodeExpression[] { CodeGenHelper.Str(dataTable.Locale.ToString()) })));
            }
            if (!StringUtil.Empty(dataTable.Prefix))
            {
                tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Prefix"), CodeGenHelper.Str(dataTable.Prefix)));
            }
            if (namespaceProperty.ShouldSerializeValue(dataTable))
            {
                tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "Namespace"), CodeGenHelper.Str(dataTable.Namespace)));
            }
            if (dataTable.MinimumCapacity != 50)
            {
                tableInitClass.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(CodeGenHelper.This(), "MinimumCapacity"), CodeGenHelper.Primitive(dataTable.MinimumCapacity)));
            }
            ExtendedPropertiesHandler.CodeGenerator = this.codeGenerator;
            ExtendedPropertiesHandler.AddExtendedProperties(this.designTable, CodeGenHelper.This(), tableInitClass.Statements, dataTable.ExtendedProperties);
        }

        private CodeMemberMethod InitExpressionsMethod()
        {
            bool flag = false;
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), "InitExpressions", MemberAttributes.Private);
            foreach (DataColumn column in this.designTable.DataTable.Columns)
            {
                if (column.Expression.Length > 0)
                {
                    CodeExpression exp = CodeGenHelper.Property(CodeGenHelper.This(), this.codeGenerator.TableHandler.Tables[column.Table.TableName].DesignColumns[column.ColumnName].GeneratorColumnPropNameInTable);
                    flag = true;
                    method.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Property(exp, "Expression"), CodeGenHelper.Str(column.Expression)));
                }
            }
            if (flag)
            {
                return method;
            }
            return null;
        }

        private CodeMemberMethod NewRowFromBuilderMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(DataRow)), "NewRowFromBuilder", MemberAttributes.Family | MemberAttributes.Override);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRowBuilder)), "builder"));
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.New(CodeGenHelper.Type(this.rowConcreteClassName), new CodeExpression[] { CodeGenHelper.Argument("builder") })));
            return method;
        }

        private CodeMemberMethod NewTypedRowMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.Type(this.rowConcreteClassName), NameHandler.FixIdName("New" + this.rowClassName), MemberAttributes.Public | MemberAttributes.Final);
            method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.Cast(CodeGenHelper.Type(this.rowConcreteClassName), CodeGenHelper.MethodCall(CodeGenHelper.This(), "NewRow"))));
            return method;
        }

        private CodeMemberMethod RemoveRowMethod()
        {
            CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), NameHandler.FixIdName("Remove" + this.rowClassName), MemberAttributes.Public | MemberAttributes.Final);
            method.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(this.rowConcreteClassName), "row"));
            method.Statements.Add(CodeGenHelper.MethodCall(CodeGenHelper.Property(CodeGenHelper.This(), "Rows"), "Remove", CodeGenHelper.Argument("row")));
            return method;
        }
    }
}

