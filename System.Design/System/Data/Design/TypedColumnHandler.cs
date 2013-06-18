namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Data;
    using System.Design;
    using System.Reflection;

    internal sealed class TypedColumnHandler
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        private DesignColumnCollection columns;
        private DesignTable designTable;
        private DataTable table;

        internal TypedColumnHandler(DesignTable designTable, TypedDataSourceCodeGenerator codeGenerator)
        {
            this.codeGenerator = codeGenerator;
            this.table = designTable.DataTable;
            this.designTable = designTable;
            this.columns = designTable.DesignColumns;
        }

        internal void AddPrivateVariables(CodeTypeDeclaration dataTableClass)
        {
            if (dataTableClass == null)
            {
                throw new InternalException("Table CodeTypeDeclaration should not be null.");
            }
            if (this.columns != null)
            {
                foreach (DesignColumn column in this.columns)
                {
                    dataTableClass.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(DataColumn)), column.GeneratorColumnVarNameInTable));
                }
            }
        }

        internal void AddRowColumnProperties(CodeTypeDeclaration rowClass)
        {
            bool flag = false;
            string generatorRowClassName = this.codeGenerator.TableHandler.Tables[this.table.TableName].GeneratorRowClassName;
            string generatorTableVarName = this.codeGenerator.TableHandler.Tables[this.table.TableName].GeneratorTableVarName;
            foreach (DesignColumn column in this.columns)
            {
                CodeExpression expression;
                CodeExpression expression2;
                DataColumn dataColumn = column.DataColumn;
                Type dataType = dataColumn.DataType;
                string generatorColumnPropNameInRow = column.GeneratorColumnPropNameInRow;
                string generatorColumnPropNameInTable = column.GeneratorColumnPropNameInTable;
                GenericNameHandler handler = new GenericNameHandler(new string[] { generatorColumnPropNameInRow }, this.codeGenerator.CodeProvider);
                CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.Type(dataType), generatorColumnPropNameInRow, MemberAttributes.Public | MemberAttributes.Final);
                CodeStatement tryStmnt = CodeGenHelper.Return(CodeGenHelper.Cast(CodeGenHelper.GlobalType(dataType), CodeGenHelper.Indexer(CodeGenHelper.This(), CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), generatorTableVarName), generatorColumnPropNameInTable))));
                if (!dataColumn.AllowDBNull)
                {
                    goto Label_0440;
                }
                string str5 = (string) dataColumn.ExtendedProperties["nullValue"];
                switch (str5)
                {
                    case null:
                    case "_throw":
                        tryStmnt = CodeGenHelper.Try(tryStmnt, CodeGenHelper.Catch(CodeGenHelper.GlobalType(typeof(InvalidCastException)), handler.AddNameToList("e"), CodeGenHelper.Throw(CodeGenHelper.GlobalType(typeof(StrongTypingException)), System.Design.SR.GetString("CG_ColumnIsDBNull", new object[] { dataColumn.ColumnName, this.table.TableName }), handler.GetNameFromList("e"))));
                        goto Label_0440;

                    default:
                    {
                        expression = null;
                        switch (str5)
                        {
                            case "_null":
                                if (dataColumn.DataType.IsSubclassOf(typeof(System.ValueType)))
                                {
                                    this.codeGenerator.ProblemList.Add(new DSGeneratorProblem(System.Design.SR.GetString("CG_TypeCantBeNull", new object[] { dataColumn.ColumnName, dataColumn.DataType.Name }), ProblemSeverity.NonFatalError, column));
                                    continue;
                                }
                                expression2 = CodeGenHelper.Primitive(null);
                                goto Label_03AE;

                            case "_empty":
                                if (dataColumn.DataType == typeof(string))
                                {
                                    expression2 = CodeGenHelper.Property(CodeGenHelper.TypeExpr(CodeGenHelper.GlobalType(dataColumn.DataType)), "Empty");
                                }
                                else
                                {
                                    expression2 = CodeGenHelper.Field(CodeGenHelper.TypeExpr(CodeGenHelper.Type(generatorRowClassName)), generatorColumnPropNameInRow + "_nullValue");
                                    ConstructorInfo constructor = dataColumn.DataType.GetConstructor(new Type[] { typeof(string) });
                                    if (constructor == null)
                                    {
                                        this.codeGenerator.ProblemList.Add(new DSGeneratorProblem(System.Design.SR.GetString("CG_NoCtor0", new object[] { dataColumn.ColumnName, dataColumn.DataType.Name }), ProblemSeverity.NonFatalError, column));
                                        continue;
                                    }
                                    constructor.Invoke(new object[0]);
                                    expression = CodeGenHelper.New(CodeGenHelper.Type(dataColumn.DataType), new CodeExpression[0]);
                                }
                                goto Label_03AE;
                        }
                        if (!flag)
                        {
                            this.table.NewRow();
                            flag = true;
                        }
                        object valueObj = this.codeGenerator.RowHandler.RowGenerator.ConvertXmlToObject.Invoke(dataColumn, new object[] { str5 });
                        DSGeneratorProblem problem = CodeGenHelper.GenerateValueExprAndFieldInit(column, valueObj, str5, generatorRowClassName, generatorColumnPropNameInRow + "_nullValue", out expression2, out expression);
                        if (problem != null)
                        {
                            this.codeGenerator.ProblemList.Add(problem);
                            continue;
                        }
                        break;
                    }
                }
            Label_03AE:;
                tryStmnt = CodeGenHelper.If(CodeGenHelper.MethodCall(CodeGenHelper.This(), "Is" + generatorColumnPropNameInRow + "Null"), new CodeStatement[] { CodeGenHelper.Return(expression2) }, new CodeStatement[] { tryStmnt });
                if (expression != null)
                {
                    CodeMemberField field = CodeGenHelper.FieldDecl(CodeGenHelper.Type(dataColumn.DataType.FullName), generatorColumnPropNameInRow + "_nullValue");
                    field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                    field.InitExpression = expression;
                    rowClass.Members.Add(field);
                }
            Label_0440:
                property.GetStatements.Add(tryStmnt);
                property.SetStatements.Add(CodeGenHelper.Assign(CodeGenHelper.Indexer(CodeGenHelper.This(), CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), generatorTableVarName), generatorColumnPropNameInTable)), CodeGenHelper.Value()));
                rowClass.Members.Add(property);
                if (dataColumn.AllowDBNull)
                {
                    string name = MemberNameValidator.GenerateIdName("Is" + generatorColumnPropNameInRow + "Null", this.codeGenerator.CodeProvider, false);
                    CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(bool)), name, MemberAttributes.Public | MemberAttributes.Final);
                    method.Statements.Add(CodeGenHelper.Return(CodeGenHelper.MethodCall(CodeGenHelper.This(), "IsNull", CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), generatorTableVarName), generatorColumnPropNameInTable))));
                    rowClass.Members.Add(method);
                    name = MemberNameValidator.GenerateIdName("Set" + generatorColumnPropNameInRow + "Null", this.codeGenerator.CodeProvider, false);
                    CodeMemberMethod method2 = CodeGenHelper.MethodDecl(CodeGenHelper.GlobalType(typeof(void)), name, MemberAttributes.Public | MemberAttributes.Final);
                    method2.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Indexer(CodeGenHelper.This(), CodeGenHelper.Property(CodeGenHelper.Field(CodeGenHelper.This(), generatorTableVarName), generatorColumnPropNameInTable)), CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(Convert)), "DBNull")));
                    rowClass.Members.Add(method2);
                }
            }
        }

        internal void AddRowGetRelatedRowsMethods(CodeTypeDeclaration rowClass)
        {
            DataRelationCollection childRelations = this.table.ChildRelations;
            for (int i = 0; i < childRelations.Count; i++)
            {
                DataRelation relation = childRelations[i];
                string generatorRowClassName = this.codeGenerator.TableHandler.Tables[relation.ChildTable.TableName].GeneratorRowClassName;
                CodeMemberMethod method = CodeGenHelper.MethodDecl(CodeGenHelper.Type(generatorRowClassName, 1), this.codeGenerator.RelationHandler.Relations[relation.RelationName].GeneratorChildPropName, MemberAttributes.Public | MemberAttributes.Final);
                method.Statements.Add(CodeGenHelper.If(CodeGenHelper.IdEQ(CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), "Table"), "ChildRelations"), CodeGenHelper.Str(relation.RelationName)), CodeGenHelper.Primitive(null)), CodeGenHelper.Return(new CodeArrayCreateExpression(generatorRowClassName, 0)), CodeGenHelper.Return(CodeGenHelper.Cast(CodeGenHelper.Type(generatorRowClassName, 1), CodeGenHelper.MethodCall(CodeGenHelper.Base(), "GetChildRows", CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), "Table"), "ChildRelations"), CodeGenHelper.Str(relation.RelationName)))))));
                rowClass.Members.Add(method);
            }
            DataRelationCollection parentRelations = this.table.ParentRelations;
            for (int j = 0; j < parentRelations.Count; j++)
            {
                DataRelation relation2 = parentRelations[j];
                string type = this.codeGenerator.TableHandler.Tables[relation2.ParentTable.TableName].GeneratorRowClassName;
                CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.Type(type), this.codeGenerator.RelationHandler.Relations[relation2.RelationName].GeneratorParentPropName, MemberAttributes.Public | MemberAttributes.Final);
                property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Cast(CodeGenHelper.Type(type), CodeGenHelper.MethodCall(CodeGenHelper.This(), "GetParentRow", CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), "Table"), "ParentRelations"), CodeGenHelper.Str(relation2.RelationName))))));
                property.SetStatements.Add(CodeGenHelper.MethodCall(CodeGenHelper.This(), "SetParentRow", new CodeExpression[] { CodeGenHelper.Value(), CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), "Table"), "ParentRelations"), CodeGenHelper.Str(relation2.RelationName)) }));
                rowClass.Members.Add(property);
            }
        }

        internal void AddTableColumnProperties(CodeTypeDeclaration dataTableClass)
        {
            if (this.columns != null)
            {
                foreach (DesignColumn column in this.columns)
                {
                    CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(DataColumn)), column.GeneratorColumnPropNameInTable, MemberAttributes.Public | MemberAttributes.Final);
                    property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), column.GeneratorColumnVarNameInTable)));
                    dataTableClass.Members.Add(property);
                }
            }
        }
    }
}

