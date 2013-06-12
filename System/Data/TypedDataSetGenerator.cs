namespace System.Data
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Obsolete("TypedDataSetGenerator class will be removed in a future release. Please use System.Data.Design.TypedDataSetGenerator in System.Design.dll."), HostProtection(SecurityAction.LinkDemand, SharedState=true, Synchronization=true)]
    public class TypedDataSetGenerator
    {
        private ICodeGenerator codeGen;
        private ArrayList conflictingTables;
        private ArrayList errorList;
        private Hashtable lookupIdentifiers;
        private bool useExtendedNaming;

        private static CodeExpression Argument(string argument)
        {
            return new CodeArgumentReferenceExpression(argument);
        }

        private static CodeStatement Assign(CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }

        private static CodeAttributeDeclaration AttributeDecl(string name)
        {
            return new CodeAttributeDeclaration(name);
        }

        private static CodeAttributeDeclaration AttributeDecl(string name, CodeExpression value)
        {
            return new CodeAttributeDeclaration(name, new CodeAttributeArgument[] { new CodeAttributeArgument(value) });
        }

        private static CodeExpression Base()
        {
            return new CodeBaseReferenceExpression();
        }

        private static CodeBinaryOperatorExpression BinOperator(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right)
        {
            return new CodeBinaryOperatorExpression(left, op, right);
        }

        private static CodeExpression Cast(CodeTypeReference type, CodeExpression expr)
        {
            return new CodeCastExpression(type, expr);
        }

        private static CodeExpression Cast(string type, CodeExpression expr)
        {
            return new CodeCastExpression(type, expr);
        }

        private static CodeCatchClause Catch(System.Type type, string name, CodeStatement catchStmnt)
        {
            CodeCatchClause clause = new CodeCatchClause {
                CatchExceptionType = Type(type),
                LocalName = name
            };
            clause.Statements.Add(catchStmnt);
            return clause;
        }

        private string ChildPropertyName(DataRelation relation)
        {
            string s = (string) relation.ExtendedProperties["typedChildren"];
            if (!isEmpty(s))
            {
                return s;
            }
            string str2 = (string) relation.ChildTable.ExtendedProperties["typedPlural"];
            if (isEmpty(str2))
            {
                str2 = (string) relation.ChildTable.ExtendedProperties["typedName"];
                if (isEmpty(str2))
                {
                    s = "Get" + relation.ChildTable.TableName + "Rows";
                    if (1 < TablesConnectedness(relation.ParentTable, relation.ChildTable))
                    {
                        s = s + "By" + relation.RelationName;
                    }
                    return this.FixIdName(s);
                }
                str2 = str2 + "Rows";
            }
            return ("Get" + str2);
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

        private static CodeMemberMethod CreateOnRowEventMethod(string eventName, string rowClassName)
        {
            CodeMemberMethod method = MethodDecl(typeof(void), "OnRow" + eventName, MemberAttributes.Family | MemberAttributes.Override);
            method.Parameters.Add(ParameterDecl(typeof(DataRowChangeEventArgs), "e"));
            method.Statements.Add(MethodCall(Base(), "OnRow" + eventName, Argument("e")));
            method.Statements.Add(If(IdNotEQ(Event(rowClassName + eventName), Primitive(null)), Stm(DelegateCall(Event(rowClassName + eventName), New(rowClassName + "ChangeEvent", new CodeExpression[] { Cast(rowClassName, Property(Argument("e"), "Row")), Property(Argument("e"), "Action") })))));
            return method;
        }

        private CodeTypeDeclaration CreateTypedDataSet(DataSet dataSet)
        {
            string name = this.FixIdName(dataSet.DataSetName);
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(name);
            declaration.BaseTypes.Add(typeof(DataSet));
            declaration.CustomAttributes.Add(AttributeDecl("System.Serializable"));
            declaration.CustomAttributes.Add(AttributeDecl("System.ComponentModel.DesignerCategoryAttribute", Str("code")));
            declaration.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
            declaration.CustomAttributes.Add(AttributeDecl("System.ComponentModel.ToolboxItem", Primitive(true)));
            declaration.CustomAttributes.Add(AttributeDecl(typeof(XmlSchemaProviderAttribute).FullName, Primitive("GetTypedDataSetSchema")));
            declaration.CustomAttributes.Add(AttributeDecl(typeof(XmlRootAttribute).FullName, Primitive(name)));
            for (int i = 0; i < dataSet.Tables.Count; i++)
            {
                declaration.Members.Add(FieldDecl(this.TableClassName(dataSet.Tables[i]), this.TableFieldName(dataSet.Tables[i])));
            }
            for (int j = 0; j < dataSet.Relations.Count; j++)
            {
                declaration.Members.Add(FieldDecl(typeof(DataRelation), this.RelationFieldName(dataSet.Relations[j])));
            }
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Public
            };
            constructor.Statements.Add(MethodCall(This(), "BeginInit"));
            constructor.Statements.Add(MethodCall(This(), "InitClass"));
            constructor.Statements.Add(VariableDecl(typeof(CollectionChangeEventHandler), "schemaChangedHandler", new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)), This(), "SchemaChanged")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Tables"), "CollectionChanged"), Variable("schemaChangedHandler")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Relations"), "CollectionChanged"), Variable("schemaChangedHandler")));
            constructor.Statements.Add(MethodCall(This(), "EndInit"));
            declaration.Members.Add(constructor);
            constructor = new CodeConstructor {
                Attributes = MemberAttributes.Family
            };
            constructor.Parameters.Add(ParameterDecl(typeof(SerializationInfo), "info"));
            constructor.Parameters.Add(ParameterDecl(typeof(StreamingContext), "context"));
            constructor.BaseConstructorArgs.AddRange(new CodeExpression[] { Argument("info"), Argument("context") });
            constructor.Statements.Add(If(EQ(MethodCall(This(), "IsBinarySerialized", new CodeExpression[] { Argument("info"), Argument("context") }), Primitive(true)), new CodeStatement[] { Stm(MethodCall(This(), "InitVars", Primitive(false))), VariableDecl(typeof(CollectionChangeEventHandler), "schemaChangedHandler1", new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)), This(), "SchemaChanged")), new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Tables"), "CollectionChanged"), Variable("schemaChangedHandler1")), new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Relations"), "CollectionChanged"), Variable("schemaChangedHandler1")), Return() }));
            constructor.Statements.Add(VariableDecl(typeof(string), "strSchema", Cast("System.String", MethodCall(Argument("info"), "GetValue", new CodeExpression[] { Str("XmlSchema"), TypeOf("System.String") }))));
            ArrayList list = new ArrayList();
            list.Add(VariableDecl(typeof(DataSet), "ds", New(typeof(DataSet), new CodeExpression[0])));
            list.Add(Stm(MethodCall(Variable("ds"), "ReadXmlSchema", new CodeExpression[] { New(typeof(XmlTextReader), new CodeExpression[] { New("System.IO.StringReader", new CodeExpression[] { Variable("strSchema") }) }) })));
            for (int k = 0; k < dataSet.Tables.Count; k++)
            {
                list.Add(If(IdNotEQ(Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[k].TableName)), Primitive(null)), Stm(MethodCall(Property(This(), "Tables"), "Add", New(this.TableClassName(dataSet.Tables[k]), new CodeExpression[] { Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[k].TableName)) })))));
            }
            list.Add(Assign(Property(This(), "DataSetName"), Property(Variable("ds"), "DataSetName")));
            list.Add(Assign(Property(This(), "Prefix"), Property(Variable("ds"), "Prefix")));
            list.Add(Assign(Property(This(), "Namespace"), Property(Variable("ds"), "Namespace")));
            list.Add(Assign(Property(This(), "Locale"), Property(Variable("ds"), "Locale")));
            list.Add(Assign(Property(This(), "CaseSensitive"), Property(Variable("ds"), "CaseSensitive")));
            list.Add(Assign(Property(This(), "EnforceConstraints"), Property(Variable("ds"), "EnforceConstraints")));
            list.Add(Stm(MethodCall(This(), "Merge", new CodeExpression[] { Variable("ds"), Primitive(false), Field(TypeExpr(typeof(MissingSchemaAction)), "Add") })));
            list.Add(Stm(MethodCall(This(), "InitVars")));
            CodeStatement[] array = new CodeStatement[list.Count];
            list.CopyTo(array);
            constructor.Statements.Add(If(IdNotEQ(Variable("strSchema"), Primitive(null)), array, new CodeStatement[] { Stm(MethodCall(This(), "BeginInit")), Stm(MethodCall(This(), "InitClass")), Stm(MethodCall(This(), "EndInit")) }));
            constructor.Statements.Add(MethodCall(This(), "GetSerializationData", new CodeExpression[] { Argument("info"), Argument("context") }));
            constructor.Statements.Add(VariableDecl(typeof(CollectionChangeEventHandler), "schemaChangedHandler", new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)), This(), "SchemaChanged")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Tables"), "CollectionChanged"), Variable("schemaChangedHandler")));
            constructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Relations"), "CollectionChanged"), Variable("schemaChangedHandler")));
            declaration.Members.Add(constructor);
            CodeMemberMethod method7 = MethodDecl(typeof(DataSet), "Clone", MemberAttributes.Public | MemberAttributes.Override);
            method7.Statements.Add(VariableDecl(name, "cln", Cast(name, MethodCall(Base(), "Clone", new CodeExpression[0]))));
            method7.Statements.Add(MethodCall(Variable("cln"), "InitVars", new CodeExpression[0]));
            method7.Statements.Add(Return(Variable("cln")));
            declaration.Members.Add(method7);
            CodeMemberMethod method12 = MethodDecl(typeof(void), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final);
            method12.Statements.Add(MethodCall(This(), "InitVars", new CodeExpression[] { Primitive(true) }));
            declaration.Members.Add(method12);
            CodeMemberMethod method = MethodDecl(typeof(void), "InitClass", MemberAttributes.Private);
            CodeMemberMethod method5 = MethodDecl(typeof(void), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final);
            method5.Parameters.Add(ParameterDecl(typeof(bool), "initTable"));
            method.Statements.Add(Assign(Property(This(), "DataSetName"), Str(dataSet.DataSetName)));
            method.Statements.Add(Assign(Property(This(), "Prefix"), Str(dataSet.Prefix)));
            method.Statements.Add(Assign(Property(This(), "Namespace"), Str(dataSet.Namespace)));
            method.Statements.Add(Assign(Property(This(), "Locale"), New(typeof(CultureInfo), new CodeExpression[] { Str(dataSet.Locale.ToString()) })));
            method.Statements.Add(Assign(Property(This(), "CaseSensitive"), Primitive(dataSet.CaseSensitive)));
            method.Statements.Add(Assign(Property(This(), "EnforceConstraints"), Primitive(dataSet.EnforceConstraints)));
            for (int m = 0; m < dataSet.Tables.Count; m++)
            {
                CodeExpression expression2 = Field(This(), this.TableFieldName(dataSet.Tables[m]));
                method.Statements.Add(Assign(expression2, New(this.TableClassName(dataSet.Tables[m]), new CodeExpression[0])));
                method.Statements.Add(MethodCall(Property(This(), "Tables"), "Add", expression2));
                method5.Statements.Add(Assign(expression2, Cast(this.TableClassName(dataSet.Tables[m]), Indexer(Property(This(), "Tables"), Str(dataSet.Tables[m].TableName)))));
                method5.Statements.Add(If(EQ(Variable("initTable"), Primitive(true)), new CodeStatement[] { If(IdNotEQ(expression2, Primitive(null)), Stm(MethodCall(expression2, "InitVars"))) }));
            }
            CodeMemberMethod method11 = MethodDecl(typeof(bool), "ShouldSerializeTables", MemberAttributes.Family | MemberAttributes.Override);
            method11.Statements.Add(Return(Primitive(false)));
            declaration.Members.Add(method11);
            CodeMemberMethod method10 = MethodDecl(typeof(bool), "ShouldSerializeRelations", MemberAttributes.Family | MemberAttributes.Override);
            method10.Statements.Add(Return(Primitive(false)));
            declaration.Members.Add(method10);
            CodeMemberMethod method3 = MethodDecl(typeof(XmlSchemaComplexType), "GetTypedDataSetSchema", MemberAttributes.Public | MemberAttributes.Static);
            method3.Parameters.Add(ParameterDecl(typeof(XmlSchemaSet), "xs"));
            method3.Statements.Add(VariableDecl(name, "ds", New(name, new CodeExpression[0])));
            method3.Statements.Add(MethodCall(Argument("xs"), "Add", new CodeExpression[] { MethodCall(Variable("ds"), "GetSchemaSerializable", new CodeExpression[0]) }));
            method3.Statements.Add(VariableDecl(typeof(XmlSchemaComplexType), "type", New(typeof(XmlSchemaComplexType), new CodeExpression[0])));
            method3.Statements.Add(VariableDecl(typeof(XmlSchemaSequence), "sequence", New(typeof(XmlSchemaSequence), new CodeExpression[0])));
            method3.Statements.Add(VariableDecl(typeof(XmlSchemaAny), "any", New(typeof(XmlSchemaAny), new CodeExpression[0])));
            method3.Statements.Add(Assign(Property(Variable("any"), "Namespace"), Property(Variable("ds"), "Namespace")));
            method3.Statements.Add(MethodCall(Property(Variable("sequence"), "Items"), "Add", new CodeExpression[] { Variable("any") }));
            method3.Statements.Add(Assign(Property(Variable("type"), "Particle"), Variable("sequence")));
            method3.Statements.Add(Return(Variable("type")));
            declaration.Members.Add(method3);
            CodeMemberMethod method2 = MethodDecl(typeof(void), "ReadXmlSerializable", MemberAttributes.Family | MemberAttributes.Override);
            method2.Parameters.Add(ParameterDecl(typeof(XmlReader), "reader"));
            method2.Statements.Add(MethodCall(This(), "Reset", new CodeExpression[0]));
            method2.Statements.Add(VariableDecl(typeof(DataSet), "ds", New(typeof(DataSet), new CodeExpression[0])));
            method2.Statements.Add(MethodCall(Variable("ds"), "ReadXml", new CodeExpression[] { Argument("reader") }));
            for (int n = 0; n < dataSet.Tables.Count; n++)
            {
                method2.Statements.Add(If(IdNotEQ(Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[n].TableName)), Primitive(null)), Stm(MethodCall(Property(This(), "Tables"), "Add", New(this.TableClassName(dataSet.Tables[n]), new CodeExpression[] { Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[n].TableName)) })))));
            }
            method2.Statements.Add(Assign(Property(This(), "DataSetName"), Property(Variable("ds"), "DataSetName")));
            method2.Statements.Add(Assign(Property(This(), "Prefix"), Property(Variable("ds"), "Prefix")));
            method2.Statements.Add(Assign(Property(This(), "Namespace"), Property(Variable("ds"), "Namespace")));
            method2.Statements.Add(Assign(Property(This(), "Locale"), Property(Variable("ds"), "Locale")));
            method2.Statements.Add(Assign(Property(This(), "CaseSensitive"), Property(Variable("ds"), "CaseSensitive")));
            method2.Statements.Add(Assign(Property(This(), "EnforceConstraints"), Property(Variable("ds"), "EnforceConstraints")));
            method2.Statements.Add(MethodCall(This(), "Merge", new CodeExpression[] { Variable("ds"), Primitive(false), Field(TypeExpr(typeof(MissingSchemaAction)), "Add") }));
            method2.Statements.Add(MethodCall(This(), "InitVars"));
            declaration.Members.Add(method2);
            CodeMemberMethod method4 = MethodDecl(typeof(XmlSchema), "GetSchemaSerializable", MemberAttributes.Family | MemberAttributes.Override);
            method4.Statements.Add(VariableDecl(typeof(MemoryStream), "stream", New(typeof(MemoryStream), new CodeExpression[0])));
            method4.Statements.Add(MethodCall(This(), "WriteXmlSchema", New(typeof(XmlTextWriter), new CodeExpression[] { Argument("stream"), Primitive(null) })));
            method4.Statements.Add(Assign(Property(Argument("stream"), "Position"), Primitive(0)));
            method4.Statements.Add(Return(MethodCall(TypeExpr("System.Xml.Schema.XmlSchema"), "Read", new CodeExpression[] { New(typeof(XmlTextReader), new CodeExpression[] { Argument("stream") }), Primitive(null) })));
            declaration.Members.Add(method4);
            CodeExpression left = null;
            foreach (DataTable table2 in dataSet.Tables)
            {
                foreach (Constraint constraint2 in table2.Constraints)
                {
                    if (constraint2 is ForeignKeyConstraint)
                    {
                        ForeignKeyConstraint constraint = (ForeignKeyConstraint) constraint2;
                        CodeArrayCreateExpression expression6 = new CodeArrayCreateExpression(typeof(DataColumn), 0);
                        foreach (DataColumn column3 in constraint.Columns)
                        {
                            expression6.Initializers.Add(Property(Field(This(), this.TableFieldName(column3.Table)), this.TableColumnPropertyName(column3)));
                        }
                        CodeArrayCreateExpression expression5 = new CodeArrayCreateExpression(typeof(DataColumn), 0);
                        foreach (DataColumn column2 in constraint.RelatedColumnsReference)
                        {
                            expression5.Initializers.Add(Property(Field(This(), this.TableFieldName(column2.Table)), this.TableColumnPropertyName(column2)));
                        }
                        if (left == null)
                        {
                            method.Statements.Add(VariableDecl(typeof(ForeignKeyConstraint), "fkc"));
                            left = Variable("fkc");
                        }
                        method.Statements.Add(Assign(left, New(typeof(ForeignKeyConstraint), new CodeExpression[] { Str(constraint.ConstraintName), expression5, expression6 })));
                        method.Statements.Add(MethodCall(Property(Field(This(), this.TableFieldName(table2)), "Constraints"), "Add", left));
                        string field = constraint.AcceptRejectRule.ToString();
                        string str6 = constraint.DeleteRule.ToString();
                        string str5 = constraint.UpdateRule.ToString();
                        method.Statements.Add(Assign(Property(left, "AcceptRejectRule"), Field(TypeExpr(constraint.AcceptRejectRule.GetType()), field)));
                        method.Statements.Add(Assign(Property(left, "DeleteRule"), Field(TypeExpr(constraint.DeleteRule.GetType()), str6)));
                        method.Statements.Add(Assign(Property(left, "UpdateRule"), Field(TypeExpr(constraint.UpdateRule.GetType()), str5)));
                    }
                }
            }
            foreach (DataRelation relation in dataSet.Relations)
            {
                CodeArrayCreateExpression expression4 = new CodeArrayCreateExpression(typeof(DataColumn), 0);
                string str4 = this.TableFieldName(relation.ParentTable);
                foreach (DataColumn column5 in relation.ParentColumnsReference)
                {
                    expression4.Initializers.Add(Property(Field(This(), str4), this.TableColumnPropertyName(column5)));
                }
                CodeArrayCreateExpression expression3 = new CodeArrayCreateExpression(typeof(DataColumn), 0);
                string str3 = this.TableFieldName(relation.ChildTable);
                foreach (DataColumn column4 in relation.ChildColumnsReference)
                {
                    expression3.Initializers.Add(Property(Field(This(), str3), this.TableColumnPropertyName(column4)));
                }
                method.Statements.Add(Assign(Field(This(), this.RelationFieldName(relation)), New(typeof(DataRelation), new CodeExpression[] { Str(relation.RelationName), expression4, expression3, Primitive(false) })));
                if (relation.Nested)
                {
                    method.Statements.Add(Assign(Property(Field(This(), this.RelationFieldName(relation)), "Nested"), Primitive(true)));
                }
                method.Statements.Add(MethodCall(Property(This(), "Relations"), "Add", Field(This(), this.RelationFieldName(relation))));
                method5.Statements.Add(Assign(Field(This(), this.RelationFieldName(relation)), Indexer(Property(This(), "Relations"), Str(relation.RelationName))));
            }
            declaration.Members.Add(method5);
            declaration.Members.Add(method);
            for (int num3 = 0; num3 < dataSet.Tables.Count; num3++)
            {
                string str2 = this.TablePropertyName(dataSet.Tables[num3]);
                CodeMemberProperty property = PropertyDecl(this.TableClassName(dataSet.Tables[num3]), str2, MemberAttributes.Public | MemberAttributes.Final);
                property.CustomAttributes.Add(AttributeDecl("System.ComponentModel.Browsable", Primitive(false)));
                property.CustomAttributes.Add(AttributeDecl("System.ComponentModel.DesignerSerializationVisibilityAttribute", Field(TypeExpr(typeof(DesignerSerializationVisibility)), "Content")));
                property.GetStatements.Add(Return(Field(This(), this.TableFieldName(dataSet.Tables[num3]))));
                declaration.Members.Add(property);
                CodeMemberMethod method9 = MethodDecl(typeof(bool), "ShouldSerialize" + str2, MemberAttributes.Private);
                method9.Statements.Add(Return(Primitive(false)));
                declaration.Members.Add(method9);
            }
            CodeMemberMethod method6 = MethodDecl(typeof(void), "SchemaChanged", MemberAttributes.Private);
            method6.Parameters.Add(ParameterDecl(typeof(object), "sender"));
            method6.Parameters.Add(ParameterDecl(typeof(CollectionChangeEventArgs), "e"));
            method6.Statements.Add(If(EQ(Property(Argument("e"), "Action"), Field(TypeExpr(typeof(CollectionChangeAction)), "Remove")), Stm(MethodCall(This(), "InitVars"))));
            declaration.Members.Add(method6);
            bool flag = false;
            CodeMemberMethod method8 = MethodDecl(typeof(void), "InitExpressions", MemberAttributes.Private);
            foreach (DataTable table in dataSet.Tables)
            {
                for (int num7 = 0; num7 < table.Columns.Count; num7++)
                {
                    DataColumn column = table.Columns[num7];
                    CodeExpression exp = Property(Field(This(), this.TableFieldName(table)), this.TableColumnPropertyName(column));
                    if (column.Expression.Length > 0)
                    {
                        flag = true;
                        method8.Statements.Add(Assign(Property(exp, "Expression"), Str(column.Expression)));
                    }
                }
            }
            if (flag)
            {
                declaration.Members.Add(method8);
                method.Statements.Add(MethodCall(This(), "InitExpressions"));
            }
            return declaration;
        }

        private CodeTypeDeclaration CreateTypedRow(DataTable table)
        {
            string str5 = this.RowClassName(table);
            string str9 = this.TableClassName(table);
            string name = this.TableFieldName(table);
            bool flag = false;
            CodeTypeDeclaration declaration = new CodeTypeDeclaration {
                Name = str5
            };
            string strA = this.RowBaseClassName(table);
            if (string.Compare(strA, "DataRow", StringComparison.Ordinal) == 0)
            {
                declaration.BaseTypes.Add(typeof(DataRow));
            }
            else
            {
                declaration.BaseTypes.Add(strA);
            }
            declaration.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
            declaration.Members.Add(FieldDecl(str9, name));
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Assembly | MemberAttributes.Final
            };
            constructor.Parameters.Add(ParameterDecl(typeof(DataRowBuilder), "rb"));
            constructor.BaseConstructorArgs.Add(Argument("rb"));
            constructor.Statements.Add(Assign(Field(This(), name), Cast(str9, Property(This(), "Table"))));
            declaration.Members.Add(constructor);
            foreach (DataColumn column in table.Columns)
            {
                CodeExpression expression;
                CodeExpression expression2;
                if (column.ColumnMapping == MappingType.Hidden)
                {
                    continue;
                }
                System.Type dataType = column.DataType;
                string str2 = this.RowColumnPropertyName(column);
                string str4 = this.TableColumnPropertyName(column);
                CodeMemberProperty property2 = PropertyDecl(dataType, str2, MemberAttributes.Public | MemberAttributes.Final);
                CodeStatement tryStmnt = Return(Cast(this.GetTypeName(dataType), Indexer(This(), Property(Field(This(), name), str4))));
                if (!column.AllowDBNull)
                {
                    goto Label_06C6;
                }
                string s = (string) column.ExtendedProperties["nullValue"];
                switch (s)
                {
                    case null:
                    case "_throw":
                        tryStmnt = Try(tryStmnt, Catch(typeof(InvalidCastException), "e", Throw(typeof(StrongTypingException), "StrongTyping_CananotAccessDBNull", "e")));
                        goto Label_06C6;

                    default:
                    {
                        expression = null;
                        switch (s)
                        {
                            case "_null":
                                if (column.DataType.IsSubclassOf(typeof(System.ValueType)))
                                {
                                    this.errorList.Add(System.Data.Res.GetString("CodeGen_TypeCantBeNull", new object[] { column.ColumnName, column.DataType.Name }));
                                    continue;
                                }
                                expression2 = Primitive(null);
                                goto Label_0641;

                            case "_empty":
                                if (column.DataType == typeof(string))
                                {
                                    expression2 = Property(TypeExpr(column.DataType), "Empty");
                                }
                                else
                                {
                                    expression2 = Field(TypeExpr(str5), str2 + "_nullValue");
                                    ConstructorInfo info2 = column.DataType.GetConstructor(new System.Type[] { typeof(string) });
                                    if (info2 == null)
                                    {
                                        this.errorList.Add(System.Data.Res.GetString("CodeGen_NoCtor0", new object[] { column.ColumnName, column.DataType.Name }));
                                        continue;
                                    }
                                    info2.Invoke(new object[0]);
                                    expression = New(column.DataType, new CodeExpression[0]);
                                }
                                goto Label_0641;
                        }
                        if (!flag)
                        {
                            table.NewRow();
                            flag = true;
                        }
                        object primitive = column.ConvertXmlToObject(s);
                        if (((((column.DataType == typeof(char)) || (column.DataType == typeof(string))) || ((column.DataType == typeof(decimal)) || (column.DataType == typeof(bool)))) || (((column.DataType == typeof(float)) || (column.DataType == typeof(double))) || ((column.DataType == typeof(sbyte)) || (column.DataType == typeof(byte))))) || ((((column.DataType == typeof(short)) || (column.DataType == typeof(ushort))) || ((column.DataType == typeof(int)) || (column.DataType == typeof(uint)))) || ((column.DataType == typeof(long)) || (column.DataType == typeof(ulong)))))
                        {
                            expression2 = Primitive(primitive);
                        }
                        else
                        {
                            expression2 = Field(TypeExpr(str5), str2 + "_nullValue");
                            if (column.DataType == typeof(byte[]))
                            {
                                expression = MethodCall(TypeExpr(typeof(Convert)), "FromBase64String", Primitive(s));
                            }
                            else if ((column.DataType == typeof(DateTime)) || (column.DataType == typeof(TimeSpan)))
                            {
                                expression = MethodCall(TypeExpr(column.DataType), "Parse", Primitive(primitive.ToString()));
                            }
                            else
                            {
                                ConstructorInfo info = column.DataType.GetConstructor(new System.Type[] { typeof(string) });
                                if (info == null)
                                {
                                    this.errorList.Add(System.Data.Res.GetString("CodeGen_NoCtor1", new object[] { column.ColumnName, column.DataType.Name }));
                                    continue;
                                }
                                info.Invoke(new object[] { s });
                                expression = New(column.DataType, new CodeExpression[] { Primitive(s) });
                            }
                        }
                        break;
                    }
                }
            Label_0641:;
                tryStmnt = If(MethodCall(This(), "Is" + str2 + "Null"), new CodeStatement[] { Return(expression2) }, new CodeStatement[] { tryStmnt });
                if (expression != null)
                {
                    CodeMemberField field = FieldDecl(column.DataType, str2 + "_nullValue");
                    field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                    field.InitExpression = expression;
                    declaration.Members.Add(field);
                }
            Label_06C6:
                property2.GetStatements.Add(tryStmnt);
                property2.SetStatements.Add(Assign(Indexer(This(), Property(Field(This(), name), str4)), Value()));
                declaration.Members.Add(property2);
                if (column.AllowDBNull)
                {
                    CodeMemberMethod method3 = MethodDecl(typeof(bool), "Is" + str2 + "Null", MemberAttributes.Public | MemberAttributes.Final);
                    method3.Statements.Add(Return(MethodCall(This(), "IsNull", Property(Field(This(), name), str4))));
                    declaration.Members.Add(method3);
                    CodeMemberMethod method2 = MethodDecl(typeof(void), "Set" + str2 + "Null", MemberAttributes.Public | MemberAttributes.Final);
                    method2.Statements.Add(Assign(Indexer(This(), Property(Field(This(), name), str4)), Field(TypeExpr(typeof(Convert)), "DBNull")));
                    declaration.Members.Add(method2);
                }
            }
            DataRelationCollection childRelations = table.ChildRelations;
            for (int i = 0; i < childRelations.Count; i++)
            {
                DataRelation relation2 = childRelations[i];
                string type = this.RowConcreteClassName(relation2.ChildTable);
                CodeMemberMethod method = Method(Type(type, 1), this.ChildPropertyName(relation2), MemberAttributes.Public | MemberAttributes.Final);
                method.Statements.Add(Return(Cast(Type(type, 1), MethodCall(This(), "GetChildRows", Indexer(Property(Property(This(), "Table"), "ChildRelations"), Str(relation2.RelationName))))));
                declaration.Members.Add(method);
            }
            DataRelationCollection parentRelations = table.ParentRelations;
            for (int j = 0; j < parentRelations.Count; j++)
            {
                DataRelation relation = parentRelations[j];
                string str6 = this.RowClassName(relation.ParentTable);
                CodeMemberProperty property = PropertyDecl(str6, this.ParentPropertyName(relation), MemberAttributes.Public | MemberAttributes.Final);
                property.GetStatements.Add(Return(Cast(str6, MethodCall(This(), "GetParentRow", Indexer(Property(Property(This(), "Table"), "ParentRelations"), Str(relation.RelationName))))));
                property.SetStatements.Add(MethodCall(This(), "SetParentRow", new CodeExpression[] { Value(), Indexer(Property(Property(This(), "Table"), "ParentRelations"), Str(relation.RelationName)) }));
                declaration.Members.Add(property);
            }
            return declaration;
        }

        private CodeTypeDeclaration CreateTypedRowEvent(DataTable table)
        {
            string str2 = this.RowClassName(table);
            this.TableClassName(table);
            string type = this.RowConcreteClassName(table);
            CodeTypeDeclaration declaration = new CodeTypeDeclaration {
                Name = str2 + "ChangeEvent"
            };
            declaration.BaseTypes.Add(typeof(EventArgs));
            declaration.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
            declaration.Members.Add(FieldDecl(type, "eventRow"));
            declaration.Members.Add(FieldDecl(typeof(DataRowAction), "eventAction"));
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            constructor.Parameters.Add(ParameterDecl(type, "row"));
            constructor.Parameters.Add(ParameterDecl(typeof(DataRowAction), "action"));
            constructor.Statements.Add(Assign(Field(This(), "eventRow"), Argument("row")));
            constructor.Statements.Add(Assign(Field(This(), "eventAction"), Argument("action")));
            declaration.Members.Add(constructor);
            CodeMemberProperty property = PropertyDecl(type, "Row", MemberAttributes.Public | MemberAttributes.Final);
            property.GetStatements.Add(Return(Field(This(), "eventRow")));
            declaration.Members.Add(property);
            property = PropertyDecl(typeof(DataRowAction), "Action", MemberAttributes.Public | MemberAttributes.Final);
            property.GetStatements.Add(Return(Field(This(), "eventAction")));
            declaration.Members.Add(property);
            return declaration;
        }

        private CodeTypeDelegate CreateTypedRowEventHandler(DataTable table)
        {
            CodeTypeDelegate delegate2;
            string str = this.RowClassName(table);
            delegate2 = new CodeTypeDelegate(str + "ChangeEventHandler") {
                TypeAttributes = delegate2.TypeAttributes | TypeAttributes.Public
            };
            delegate2.Parameters.Add(ParameterDecl(typeof(object), "sender"));
            delegate2.Parameters.Add(ParameterDecl(str + "ChangeEvent", "e"));
            return delegate2;
        }

        private CodeTypeDeclaration CreateTypedTable(DataTable table)
        {
            string str = this.RowClassName(table);
            string name = this.TableClassName(table);
            string str2 = this.RowConcreteClassName(table);
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(name);
            declaration.BaseTypes.Add(typeof(DataTable));
            declaration.BaseTypes.Add(typeof(IEnumerable));
            declaration.CustomAttributes.Add(AttributeDecl("System.Serializable"));
            declaration.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
            for (int i = 0; i < table.Columns.Count; i++)
            {
                declaration.Members.Add(FieldDecl(typeof(DataColumn), this.TableColumnFieldName(table.Columns[i])));
            }
            declaration.Members.Add(EventDecl(str + "ChangeEventHandler", str + "Changed"));
            declaration.Members.Add(EventDecl(str + "ChangeEventHandler", str + "Changing"));
            declaration.Members.Add(EventDecl(str + "ChangeEventHandler", str + "Deleted"));
            declaration.Members.Add(EventDecl(str + "ChangeEventHandler", str + "Deleting"));
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Assembly | MemberAttributes.Final
            };
            constructor.BaseConstructorArgs.Add(Str(table.TableName));
            constructor.Statements.Add(MethodCall(This(), "InitClass"));
            declaration.Members.Add(constructor);
            constructor = new CodeConstructor {
                Attributes = MemberAttributes.Family
            };
            constructor.Parameters.Add(ParameterDecl(typeof(SerializationInfo), "info"));
            constructor.Parameters.Add(ParameterDecl(typeof(StreamingContext), "context"));
            constructor.BaseConstructorArgs.AddRange(new CodeExpression[] { Argument("info"), Argument("context") });
            constructor.Statements.Add(MethodCall(This(), "InitVars"));
            declaration.Members.Add(constructor);
            constructor = new CodeConstructor {
                Attributes = MemberAttributes.Assembly | MemberAttributes.Final
            };
            constructor.Parameters.Add(ParameterDecl(typeof(DataTable), "table"));
            constructor.BaseConstructorArgs.Add(Property(Argument("table"), "TableName"));
            constructor.Statements.Add(If(IdNotEQ(Property(Argument("table"), "CaseSensitive"), Property(Property(Argument("table"), "DataSet"), "CaseSensitive")), Assign(Property(This(), "CaseSensitive"), Property(Argument("table"), "CaseSensitive"))));
            constructor.Statements.Add(If(IdNotEQ(MethodCall(Property(Argument("table"), "Locale"), "ToString"), MethodCall(Property(Property(Argument("table"), "DataSet"), "Locale"), "ToString")), Assign(Property(This(), "Locale"), Property(Argument("table"), "Locale"))));
            constructor.Statements.Add(If(IdNotEQ(Property(Argument("table"), "Namespace"), Property(Property(Argument("table"), "DataSet"), "Namespace")), Assign(Property(This(), "Namespace"), Property(Argument("table"), "Namespace"))));
            constructor.Statements.Add(Assign(Property(This(), "Prefix"), Property(Argument("table"), "Prefix")));
            constructor.Statements.Add(Assign(Property(This(), "MinimumCapacity"), Property(Argument("table"), "MinimumCapacity")));
            constructor.Statements.Add(Assign(Property(This(), "DisplayExpression"), Property(Argument("table"), "DisplayExpression")));
            declaration.Members.Add(constructor);
            CodeMemberProperty property2 = PropertyDecl(typeof(int), "Count", MemberAttributes.Public | MemberAttributes.Final);
            property2.CustomAttributes.Add(AttributeDecl("System.ComponentModel.Browsable", Primitive(false)));
            property2.GetStatements.Add(Return(Property(Property(This(), "Rows"), "Count")));
            declaration.Members.Add(property2);
            for (int j = 0; j < table.Columns.Count; j++)
            {
                DataColumn column3 = table.Columns[j];
                CodeMemberProperty property3 = PropertyDecl(typeof(DataColumn), this.TableColumnPropertyName(column3), MemberAttributes.Assembly | MemberAttributes.Final);
                property3.GetStatements.Add(Return(Field(This(), this.TableColumnFieldName(column3))));
                declaration.Members.Add(property3);
            }
            CodeMemberProperty property = PropertyDecl(str2, "Item", MemberAttributes.Public | MemberAttributes.Final);
            property.Parameters.Add(ParameterDecl(typeof(int), "index"));
            property.GetStatements.Add(Return(Cast(str2, Indexer(Property(This(), "Rows"), Argument("index")))));
            declaration.Members.Add(property);
            CodeMemberMethod method8 = MethodDecl(typeof(void), "Add" + str, MemberAttributes.Public | MemberAttributes.Final);
            method8.Parameters.Add(ParameterDecl(str2, "row"));
            method8.Statements.Add(MethodCall(Property(This(), "Rows"), "Add", Argument("row")));
            declaration.Members.Add(method8);
            ArrayList list = new ArrayList();
            for (int k = 0; k < table.Columns.Count; k++)
            {
                if (!table.Columns[k].AutoIncrement)
                {
                    list.Add(table.Columns[k]);
                }
            }
            CodeMemberMethod method2 = MethodDecl(str2, "Add" + str, MemberAttributes.Public | MemberAttributes.Final);
            DataColumn[] array = new DataColumn[list.Count];
            list.CopyTo(array, 0);
            for (int m = 0; m < array.Length; m++)
            {
                System.Type dataType = array[m].DataType;
                DataRelation relation2 = array[m].FindParentRelation();
                if (this.ChildRelationFollowable(relation2))
                {
                    string type = this.RowClassName(relation2.ParentTable);
                    string str10 = this.FixIdName("parent" + type + "By" + relation2.RelationName);
                    method2.Parameters.Add(ParameterDecl(type, str10));
                }
                else
                {
                    method2.Parameters.Add(ParameterDecl(this.GetTypeName(dataType), this.RowColumnPropertyName(array[m])));
                }
            }
            method2.Statements.Add(VariableDecl(str2, "row" + str, Cast(str2, MethodCall(This(), "NewRow"))));
            CodeExpression exp = Variable("row" + str);
            CodeAssignStatement statement = new CodeAssignStatement {
                Left = Property(exp, "ItemArray")
            };
            CodeArrayCreateExpression expression2 = new CodeArrayCreateExpression {
                CreateType = Type(typeof(object))
            };
            array = new DataColumn[table.Columns.Count];
            table.Columns.CopyTo(array, 0);
            for (int n = 0; n < array.Length; n++)
            {
                if (array[n].AutoIncrement)
                {
                    expression2.Initializers.Add(Primitive(null));
                }
                else
                {
                    DataRelation relation = array[n].FindParentRelation();
                    if (this.ChildRelationFollowable(relation))
                    {
                        string argument = this.FixIdName("parent" + this.RowClassName(relation.ParentTable) + "By" + relation.RelationName);
                        expression2.Initializers.Add(Indexer(Argument(argument), Primitive(relation.ParentColumnsReference[0].Ordinal)));
                    }
                    else
                    {
                        expression2.Initializers.Add(Argument(this.RowColumnPropertyName(array[n])));
                    }
                }
            }
            statement.Right = expression2;
            method2.Statements.Add(statement);
            method2.Statements.Add(MethodCall(Property(This(), "Rows"), "Add", exp));
            method2.Statements.Add(Return(exp));
            declaration.Members.Add(method2);
            for (int num = 0; num < table.Constraints.Count; num++)
            {
                if ((table.Constraints[num] is UniqueConstraint) && ((UniqueConstraint) table.Constraints[num]).IsPrimaryKey)
                {
                    DataColumn[] columnsReference = ((UniqueConstraint) table.Constraints[num]).ColumnsReference;
                    string inVarName = "FindBy";
                    bool flag = true;
                    for (int num7 = 0; num7 < columnsReference.Length; num7++)
                    {
                        inVarName = inVarName + this.RowColumnPropertyName(columnsReference[num7]);
                        if (columnsReference[num7].ColumnMapping != MappingType.Hidden)
                        {
                            flag = false;
                        }
                    }
                    if (!flag)
                    {
                        CodeMemberMethod method7 = MethodDecl(str, this.FixIdName(inVarName), MemberAttributes.Public | MemberAttributes.Final);
                        for (int num6 = 0; num6 < columnsReference.Length; num6++)
                        {
                            method7.Parameters.Add(ParameterDecl(this.GetTypeName(columnsReference[num6].DataType), this.RowColumnPropertyName(columnsReference[num6])));
                        }
                        CodeArrayCreateExpression par = new CodeArrayCreateExpression(typeof(object), columnsReference.Length);
                        for (int num11 = 0; num11 < columnsReference.Length; num11++)
                        {
                            par.Initializers.Add(Argument(this.RowColumnPropertyName(columnsReference[num11])));
                        }
                        method7.Statements.Add(Return(Cast(str, MethodCall(Property(This(), "Rows"), "Find", par))));
                        declaration.Members.Add(method7);
                    }
                }
            }
            CodeMemberMethod method6 = MethodDecl(typeof(IEnumerator), "GetEnumerator", MemberAttributes.Public | MemberAttributes.Final);
            method6.ImplementationTypes.Add(Type("System.Collections.IEnumerable"));
            method6.Statements.Add(Return(MethodCall(Property(This(), "Rows"), "GetEnumerator")));
            declaration.Members.Add(method6);
            CodeMemberMethod method3 = MethodDecl(typeof(DataTable), "Clone", MemberAttributes.Public | MemberAttributes.Override);
            method3.Statements.Add(VariableDecl(name, "cln", Cast(name, MethodCall(Base(), "Clone", new CodeExpression[0]))));
            method3.Statements.Add(MethodCall(Variable("cln"), "InitVars", new CodeExpression[0]));
            method3.Statements.Add(Return(Variable("cln")));
            declaration.Members.Add(method3);
            CodeMemberMethod method12 = MethodDecl(typeof(DataTable), "CreateInstance", MemberAttributes.Family | MemberAttributes.Override);
            method12.Statements.Add(Return(New(name, new CodeExpression[0])));
            declaration.Members.Add(method12);
            CodeMemberMethod method = MethodDecl(typeof(void), "InitClass", MemberAttributes.Private);
            CodeMemberMethod method11 = MethodDecl(typeof(void), "InitVars", MemberAttributes.Assembly | MemberAttributes.Final);
            for (int num10 = 0; num10 < table.Columns.Count; num10++)
            {
                DataColumn column2 = table.Columns[num10];
                string field = this.TableColumnFieldName(column2);
                CodeExpression left = Field(This(), field);
                method.Statements.Add(Assign(left, New(typeof(DataColumn), new CodeExpression[] { Str(column2.ColumnName), TypeOf(this.GetTypeName(column2.DataType)), Primitive(null), Field(TypeExpr(typeof(MappingType)), (column2.ColumnMapping == MappingType.SimpleContent) ? "SimpleContent" : ((column2.ColumnMapping == MappingType.Attribute) ? "Attribute" : ((column2.ColumnMapping == MappingType.Hidden) ? "Hidden" : "Element"))) })));
                method.Statements.Add(MethodCall(Property(This(), "Columns"), "Add", Field(This(), field)));
            }
            for (int num5 = 0; num5 < table.Constraints.Count; num5++)
            {
                if (table.Constraints[num5] is UniqueConstraint)
                {
                    UniqueConstraint constraint = (UniqueConstraint) table.Constraints[num5];
                    DataColumn[] columnArray3 = constraint.ColumnsReference;
                    CodeExpression[] initializers = new CodeExpression[columnArray3.Length];
                    for (int num4 = 0; num4 < columnArray3.Length; num4++)
                    {
                        initializers[num4] = Field(This(), this.TableColumnFieldName(columnArray3[num4]));
                    }
                    method.Statements.Add(MethodCall(Property(This(), "Constraints"), "Add", New(typeof(UniqueConstraint), new CodeExpression[] { Str(constraint.ConstraintName), new CodeArrayCreateExpression(typeof(DataColumn), initializers), Primitive(constraint.IsPrimaryKey) })));
                }
            }
            for (int num9 = 0; num9 < table.Columns.Count; num9++)
            {
                DataColumn column = table.Columns[num9];
                string str7 = this.TableColumnFieldName(column);
                CodeExpression expression = Field(This(), str7);
                method11.Statements.Add(Assign(expression, Indexer(Property(This(), "Columns"), Str(column.ColumnName))));
                if (column.AutoIncrement)
                {
                    method.Statements.Add(Assign(Property(expression, "AutoIncrement"), Primitive(true)));
                }
                if (column.AutoIncrementSeed != 0L)
                {
                    method.Statements.Add(Assign(Property(expression, "AutoIncrementSeed"), Primitive(column.AutoIncrementSeed)));
                }
                if (column.AutoIncrementStep != 1L)
                {
                    method.Statements.Add(Assign(Property(expression, "AutoIncrementStep"), Primitive(column.AutoIncrementStep)));
                }
                if (!column.AllowDBNull)
                {
                    method.Statements.Add(Assign(Property(expression, "AllowDBNull"), Primitive(false)));
                }
                if (column.ReadOnly)
                {
                    method.Statements.Add(Assign(Property(expression, "ReadOnly"), Primitive(true)));
                }
                if (column.Unique)
                {
                    method.Statements.Add(Assign(Property(expression, "Unique"), Primitive(true)));
                }
                if (!ADP.IsEmpty(column.Prefix))
                {
                    method.Statements.Add(Assign(Property(expression, "Prefix"), Str(column.Prefix)));
                }
                if (column._columnUri != null)
                {
                    method.Statements.Add(Assign(Property(expression, "Namespace"), Str(column.Namespace)));
                }
                if (column.Caption != column.ColumnName)
                {
                    method.Statements.Add(Assign(Property(expression, "Caption"), Str(column.Caption)));
                }
                if (column.DefaultValue != DBNull.Value)
                {
                    method.Statements.Add(Assign(Property(expression, "DefaultValue"), Primitive(column.DefaultValue)));
                }
                if (column.MaxLength != -1)
                {
                    method.Statements.Add(Assign(Property(expression, "MaxLength"), Primitive(column.MaxLength)));
                }
            }
            if (table.ShouldSerializeCaseSensitive())
            {
                method.Statements.Add(Assign(Property(This(), "CaseSensitive"), Primitive(table.CaseSensitive)));
            }
            if (table.ShouldSerializeLocale())
            {
                method.Statements.Add(Assign(Property(This(), "Locale"), New(typeof(CultureInfo), new CodeExpression[] { Str(table.Locale.ToString()) })));
            }
            if (!ADP.IsEmpty(table.Prefix))
            {
                method.Statements.Add(Assign(Property(This(), "Prefix"), Str(table.Prefix)));
            }
            if (table.tableNamespace != null)
            {
                method.Statements.Add(Assign(Property(This(), "Namespace"), Str(table.Namespace)));
            }
            if (table.MinimumCapacity != 50)
            {
                method.Statements.Add(Assign(Property(This(), "MinimumCapacity"), Primitive(table.MinimumCapacity)));
            }
            if (table.displayExpression != null)
            {
                method.Statements.Add(Assign(Property(This(), "DisplayExpression"), Str(table.DisplayExpressionInternal)));
            }
            declaration.Members.Add(method11);
            declaration.Members.Add(method);
            CodeMemberMethod method10 = MethodDecl(str2, "New" + str, MemberAttributes.Public | MemberAttributes.Final);
            method10.Statements.Add(Return(Cast(str2, MethodCall(This(), "NewRow"))));
            declaration.Members.Add(method10);
            CodeMemberMethod method5 = MethodDecl(typeof(DataRow), "NewRowFromBuilder", MemberAttributes.Family | MemberAttributes.Override);
            method5.Parameters.Add(ParameterDecl(typeof(DataRowBuilder), "builder"));
            method5.Statements.Add(Return(New(str2, new CodeExpression[] { Argument("builder") })));
            declaration.Members.Add(method5);
            CodeMemberMethod method9 = MethodDecl(typeof(System.Type), "GetRowType", MemberAttributes.Family | MemberAttributes.Override);
            method9.Statements.Add(Return(TypeOf(str2)));
            declaration.Members.Add(method9);
            declaration.Members.Add(CreateOnRowEventMethod("Changed", str));
            declaration.Members.Add(CreateOnRowEventMethod("Changing", str));
            declaration.Members.Add(CreateOnRowEventMethod("Deleted", str));
            declaration.Members.Add(CreateOnRowEventMethod("Deleting", str));
            CodeMemberMethod method4 = MethodDecl(typeof(void), "Remove" + str, MemberAttributes.Public | MemberAttributes.Final);
            method4.Parameters.Add(ParameterDecl(str2, "row"));
            method4.Statements.Add(MethodCall(Property(This(), "Rows"), "Remove", Argument("row")));
            declaration.Members.Add(method4);
            return declaration;
        }

        private static CodeExpression DelegateCall(CodeExpression targetObject, CodeExpression par)
        {
            return new CodeDelegateInvokeExpression(targetObject, new CodeExpression[] { This(), par });
        }

        private static CodeBinaryOperatorExpression EQ(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.ValueEquality, right);
        }

        private static CodeExpression Event(string eventName)
        {
            return new CodeEventReferenceExpression(This(), eventName);
        }

        private static CodeMemberEvent EventDecl(string type, string name)
        {
            return new CodeMemberEvent { Name = name, Type = Type(type), Attributes = MemberAttributes.Public | MemberAttributes.Final };
        }

        private static CodeExpression Field(CodeExpression exp, string field)
        {
            return new CodeFieldReferenceExpression(exp, field);
        }

        private static CodeMemberField FieldDecl(string type, string name)
        {
            return new CodeMemberField(type, name);
        }

        private static CodeMemberField FieldDecl(System.Type type, string name)
        {
            return new CodeMemberField(type, name);
        }

        private string FixIdName(string inVarName)
        {
            if (this.lookupIdentifiers == null)
            {
                this.InitLookupIdentifiers();
            }
            string str = (string) this.lookupIdentifiers[inVarName];
            if (str == null)
            {
                str = GenerateIdName(inVarName, this.codeGen);
                while (this.lookupIdentifiers.ContainsValue(str))
                {
                    str = '_' + str;
                }
                this.lookupIdentifiers[inVarName] = str;
                if (!this.codeGen.IsValidIdentifier(str))
                {
                    this.errorList.Add(System.Data.Res.GetString("CodeGen_InvalidIdentifier", new object[] { str }));
                }
            }
            return str;
        }

        public static void Generate(DataSet dataSet, CodeNamespace codeNamespace, ICodeGenerator codeGen)
        {
            new TypedDataSetGenerator().GenerateCode(dataSet, codeNamespace, codeGen);
            CodeGenerator.ValidateIdentifiers(codeNamespace);
        }

        internal CodeTypeDeclaration GenerateCode(DataSet dataSet, CodeNamespace codeNamespace, ICodeGenerator codeGen)
        {
            this.useExtendedNaming = false;
            this.errorList = new ArrayList();
            this.conflictingTables = new ArrayList();
            this.codeGen = codeGen;
            CodeTypeDeclaration declaration = this.CreateTypedDataSet(dataSet);
            foreach (DataTable table2 in dataSet.Tables)
            {
                declaration.Members.Add(this.CreateTypedRowEventHandler(table2));
            }
            foreach (DataTable table in dataSet.Tables)
            {
                declaration.Members.Add(this.CreateTypedTable(table));
                declaration.Members.Add(this.CreateTypedRow(table));
                declaration.Members.Add(this.CreateTypedRowEvent(table));
            }
            if (this.errorList.Count > 0)
            {
                throw new TypedDataSetGeneratorException(this.errorList);
            }
            codeNamespace.Types.Add(declaration);
            return declaration;
        }

        public static string GenerateIdName(string name, ICodeGenerator codeGen)
        {
            if (codeGen.IsValidIdentifier(name))
            {
                return name;
            }
            string str = name.Replace(' ', '_');
            if (!codeGen.IsValidIdentifier(str))
            {
                str = "_" + str;
                for (int i = 1; i < str.Length; i++)
                {
                    UnicodeCategory unicodeCategory = char.GetUnicodeCategory(str[i]);
                    if (((((unicodeCategory != UnicodeCategory.UppercaseLetter) && (UnicodeCategory.LowercaseLetter != unicodeCategory)) && ((UnicodeCategory.TitlecaseLetter != unicodeCategory) && (UnicodeCategory.ModifierLetter != unicodeCategory))) && (((UnicodeCategory.OtherLetter != unicodeCategory) && (UnicodeCategory.LetterNumber != unicodeCategory)) && ((UnicodeCategory.NonSpacingMark != unicodeCategory) && (UnicodeCategory.SpacingCombiningMark != unicodeCategory)))) && ((UnicodeCategory.DecimalDigitNumber != unicodeCategory) && (UnicodeCategory.ConnectorPunctuation != unicodeCategory)))
                    {
                        str = str.Replace(str[i], '_');
                    }
                }
            }
            return str;
        }

        private string GetTypeName(System.Type t)
        {
            return t.FullName;
        }

        private static CodeBinaryOperatorExpression IdNotEQ(CodeExpression left, CodeExpression right)
        {
            return BinOperator(left, CodeBinaryOperatorType.IdentityInequality, right);
        }

        private static CodeStatement If(CodeExpression cond, CodeStatement[] trueStms)
        {
            return new CodeConditionStatement(cond, trueStms);
        }

        private static CodeStatement If(CodeExpression cond, CodeStatement trueStm)
        {
            return If(cond, new CodeStatement[] { trueStm });
        }

        private static CodeStatement If(CodeExpression cond, CodeStatement[] trueStms, CodeStatement[] falseStms)
        {
            return new CodeConditionStatement(cond, trueStms, falseStms);
        }

        private static CodeExpression Indexer(CodeExpression targetObject, CodeExpression indices)
        {
            return new CodeIndexerExpression(targetObject, new CodeExpression[] { indices });
        }

        private void InitLookupIdentifiers()
        {
            this.lookupIdentifiers = new Hashtable();
            foreach (PropertyInfo info in typeof(DataRow).GetProperties())
            {
                this.lookupIdentifiers[info.Name] = '_' + info.Name;
            }
        }

        private static bool isEmpty(string s)
        {
            if (s != null)
            {
                return (s.Length == 0);
            }
            return true;
        }

        private static CodeMemberMethod Method(CodeTypeReference type, string name, MemberAttributes attributes)
        {
            return new CodeMemberMethod { ReturnType = type, Name = name, Attributes = attributes };
        }

        private static CodeExpression MethodCall(CodeExpression targetObject, string methodName)
        {
            return new CodeMethodInvokeExpression(targetObject, methodName, new CodeExpression[0]);
        }

        private static CodeExpression MethodCall(CodeExpression targetObject, string methodName, CodeExpression[] parameters)
        {
            return new CodeMethodInvokeExpression(targetObject, methodName, parameters);
        }

        private static CodeExpression MethodCall(CodeExpression targetObject, string methodName, CodeExpression par)
        {
            return new CodeMethodInvokeExpression(targetObject, methodName, new CodeExpression[] { par });
        }

        private static CodeMemberMethod MethodDecl(string type, string name, MemberAttributes attributes)
        {
            return Method(Type(type), name, attributes);
        }

        private static CodeMemberMethod MethodDecl(System.Type type, string name, MemberAttributes attributes)
        {
            return Method(Type(type), name, attributes);
        }

        private static CodeExpression New(string type, CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(type, parameters);
        }

        private static CodeExpression New(System.Type type, CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(type, parameters);
        }

        private static CodeParameterDeclarationExpression ParameterDecl(string type, string name)
        {
            return new CodeParameterDeclarationExpression(type, name);
        }

        private static CodeParameterDeclarationExpression ParameterDecl(System.Type type, string name)
        {
            return new CodeParameterDeclarationExpression(type, name);
        }

        private string ParentPropertyName(DataRelation relation)
        {
            string s = null;
            s = (string) relation.ExtendedProperties["typedParent"];
            if (isEmpty(s))
            {
                s = this.RowClassName(relation.ParentTable);
                if ((relation.ChildTable == relation.ParentTable) || (relation.ChildColumnsReference.Length != 1))
                {
                    s = s + "Parent";
                }
                if (1 < TablesConnectedness(relation.ParentTable, relation.ChildTable))
                {
                    s = s + "By" + this.FixIdName(relation.RelationName);
                }
            }
            return s;
        }

        private static CodeExpression Primitive(object primitive)
        {
            return new CodePrimitiveExpression(primitive);
        }

        private static CodeExpression Property(CodeExpression exp, string property)
        {
            return new CodePropertyReferenceExpression(exp, property);
        }

        private static CodeMemberProperty PropertyDecl(string type, string name, MemberAttributes attributes)
        {
            return new CodeMemberProperty { Type = Type(type), Name = name, Attributes = attributes };
        }

        private static CodeMemberProperty PropertyDecl(System.Type type, string name, MemberAttributes attributes)
        {
            return new CodeMemberProperty { Type = Type(type), Name = name, Attributes = attributes };
        }

        private string RelationFieldName(DataRelation relation)
        {
            return this.FixIdName("relation" + relation.RelationName);
        }

        private static CodeStatement Return()
        {
            return new CodeMethodReturnStatement();
        }

        private static CodeStatement Return(CodeExpression expr)
        {
            return new CodeMethodReturnStatement(expr);
        }

        private string RowBaseClassName(DataTable table)
        {
            if (!this.useExtendedNaming)
            {
                return "DataRow";
            }
            string s = (string) table.ExtendedProperties["typedBaseClass"];
            if (isEmpty(s))
            {
                s = (string) table.DataSet.ExtendedProperties["typedBaseClass"];
                if (isEmpty(s))
                {
                    s = "DataRow";
                }
            }
            return s;
        }

        private string RowClassName(DataTable table)
        {
            string s = (string) table.ExtendedProperties["typedName"];
            if (isEmpty(s))
            {
                s = this.FixIdName(table.TableName) + "Row";
            }
            return s;
        }

        private string RowColumnPropertyName(DataColumn column)
        {
            string s = (string) column.ExtendedProperties["typedName"];
            if (isEmpty(s))
            {
                s = this.FixIdName(column.ColumnName);
            }
            return s;
        }

        private string RowConcreteClassName(DataTable table)
        {
            if (!this.useExtendedNaming)
            {
                return this.RowClassName(table);
            }
            string s = (string) table.ExtendedProperties["typedConcreteClass"];
            if (isEmpty(s))
            {
                s = this.RowClassName(table);
            }
            return s;
        }

        private static CodeStatement Stm(CodeExpression expr)
        {
            return new CodeExpressionStatement(expr);
        }

        private static CodeExpression Str(string str)
        {
            return Primitive(str);
        }

        private string TableClassName(DataTable table)
        {
            string s = (string) table.ExtendedProperties["typedPlural"];
            if (isEmpty(s))
            {
                s = (string) table.ExtendedProperties["typedName"];
                if (isEmpty(s))
                {
                    if ((table.DataSet.Tables.InternalIndexOf(table.TableName) == -3) && !this.conflictingTables.Contains(table.TableName))
                    {
                        this.conflictingTables.Add(table.TableName);
                        this.errorList.Add(System.Data.Res.GetString("CodeGen_DuplicateTableName", new object[] { table.TableName }));
                    }
                    s = this.FixIdName(table.TableName);
                }
            }
            return (s + "DataTable");
        }

        private string TableColumnFieldName(DataColumn column)
        {
            string strB = this.RowColumnPropertyName(column);
            if (string.Compare("column", strB, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return ("column" + strB);
            }
            return ("columnField" + strB);
        }

        private string TableColumnPropertyName(DataColumn column)
        {
            return (this.RowColumnPropertyName(column) + "Column");
        }

        private string TableFieldName(DataTable table)
        {
            return ("table" + this.TablePropertyName(table));
        }

        private string TablePropertyName(DataTable table)
        {
            string s = (string) table.ExtendedProperties["typedPlural"];
            if (!isEmpty(s))
            {
                return s;
            }
            s = (string) table.ExtendedProperties["typedName"];
            if (isEmpty(s))
            {
                return this.FixIdName(table.TableName);
            }
            return (s + "Table");
        }

        private static int TablesConnectedness(DataTable parentTable, DataTable childTable)
        {
            int num2 = 0;
            DataRelationCollection parentRelations = childTable.ParentRelations;
            for (int i = 0; i < parentRelations.Count; i++)
            {
                if (parentRelations[i].ParentTable == parentTable)
                {
                    num2++;
                }
            }
            return num2;
        }

        private static CodeExpression This()
        {
            return new CodeThisReferenceExpression();
        }

        private static CodeStatement Throw(System.Type exception, string arg, string inner)
        {
            return new CodeThrowExceptionStatement(New(exception, new CodeExpression[] { Str(System.Data.Res.GetString(arg)), Variable(inner) }));
        }

        private static CodeStatement Try(CodeStatement tryStmnt, CodeCatchClause catchClause)
        {
            return new CodeTryCatchFinallyStatement(new CodeStatement[] { tryStmnt }, new CodeCatchClause[] { catchClause });
        }

        private static CodeTypeReference Type(string type)
        {
            return new CodeTypeReference(type);
        }

        private static CodeTypeReference Type(System.Type type)
        {
            return new CodeTypeReference(type);
        }

        private static CodeTypeReference Type(string type, int rank)
        {
            return new CodeTypeReference(type, rank);
        }

        private static CodeTypeReferenceExpression TypeExpr(string type)
        {
            return new CodeTypeReferenceExpression(type);
        }

        private static CodeTypeReferenceExpression TypeExpr(System.Type type)
        {
            return new CodeTypeReferenceExpression(type);
        }

        private static CodeExpression TypeOf(string type)
        {
            return new CodeTypeOfExpression(type);
        }

        private static CodeExpression Value()
        {
            return new CodePropertySetValueReferenceExpression();
        }

        private static CodeExpression Variable(string variable)
        {
            return new CodeVariableReferenceExpression(variable);
        }

        private static CodeStatement VariableDecl(System.Type type, string name)
        {
            return new CodeVariableDeclarationStatement(type, name);
        }

        private static CodeStatement VariableDecl(string type, string name, CodeExpression initExpr)
        {
            return new CodeVariableDeclarationStatement(type, name, initExpr);
        }

        private static CodeStatement VariableDecl(System.Type type, string name, CodeExpression initExpr)
        {
            return new CodeVariableDeclarationStatement(type, name, initExpr);
        }
    }
}

