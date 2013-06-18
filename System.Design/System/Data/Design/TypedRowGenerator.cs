namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Data;
    using System.Reflection;

    internal sealed class TypedRowGenerator
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        private MethodInfo convertXmlToObject;

        internal TypedRowGenerator(TypedDataSourceCodeGenerator codeGenerator)
        {
            this.codeGenerator = codeGenerator;
            this.convertXmlToObject = typeof(DataColumn).GetMethod("ConvertXmlToObject", BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new Type[] { typeof(string) }, null);
        }

        private CodeTypeDeclaration CreateTypedRowEventArg(DesignTable designTable)
        {
            if (designTable == null)
            {
                throw new InternalException("DesignTable should not be null.");
            }
            DataTable dataTable = designTable.DataTable;
            string generatorRowClassName = designTable.GeneratorRowClassName;
            string generatorTableClassName = designTable.GeneratorTableClassName;
            string type = designTable.GeneratorRowClassName;
            CodeTypeDeclaration declaration = CodeGenHelper.Class(designTable.GeneratorRowEvArgName, false, TypeAttributes.Public);
            declaration.BaseTypes.Add(CodeGenHelper.GlobalType(typeof(EventArgs)));
            declaration.Comments.Add(CodeGenHelper.Comment("Row event argument class", true));
            declaration.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.Type(type), "eventRow"));
            declaration.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(DataRowAction)), "eventAction"));
            declaration.Members.Add(this.EventArgConstructor(type));
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.Type(type), "Row", MemberAttributes.Public | MemberAttributes.Final);
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), "eventRow")));
            declaration.Members.Add(property);
            property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(DataRowAction)), "Action", MemberAttributes.Public | MemberAttributes.Final);
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), "eventAction")));
            declaration.Members.Add(property);
            return declaration;
        }

        private CodeConstructor EventArgConstructor(string rowConcreteClassName)
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Public | MemberAttributes.Final);
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(rowConcreteClassName), "row"));
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRowAction)), "action"));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), "eventRow"), CodeGenHelper.Argument("row")));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), "eventAction"), CodeGenHelper.Argument("action")));
            return constructor;
        }

        private CodeTypeDeclaration GenerateRow(DesignTable table)
        {
            if (table == null)
            {
                throw new InternalException("DesignTable should not be null.");
            }
            string generatorRowClassName = table.GeneratorRowClassName;
            string generatorTableClassName = table.GeneratorTableClassName;
            string generatorTableVarName = table.GeneratorTableVarName;
            TypedColumnHandler columnHandler = this.codeGenerator.TableHandler.GetColumnHandler(table.Name);
            CodeTypeDeclaration rowClass = CodeGenHelper.Class(generatorRowClassName, true, TypeAttributes.Public);
            rowClass.BaseTypes.Add(CodeGenHelper.GlobalType(typeof(DataRow)));
            rowClass.Comments.Add(CodeGenHelper.Comment("Represents strongly named DataRow class.", true));
            rowClass.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.Type(generatorTableClassName), generatorTableVarName));
            rowClass.Members.Add(this.RowConstructor(generatorTableClassName, generatorTableVarName));
            columnHandler.AddRowColumnProperties(rowClass);
            columnHandler.AddRowGetRelatedRowsMethods(rowClass);
            return rowClass;
        }

        internal void GenerateRows(CodeTypeDeclaration dataSourceClass)
        {
            foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
            {
                dataSourceClass.Members.Add(this.GenerateRow(table));
            }
        }

        internal void GenerateTypedRowEventArgs(CodeTypeDeclaration dataSourceClass)
        {
            if (this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareEvents) && this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareDelegates))
            {
                foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
                {
                    dataSourceClass.Members.Add(this.CreateTypedRowEventArg(table));
                }
            }
        }

        private CodeTypeDelegate GenerateTypedRowEventHandler(DesignTable table)
        {
            CodeTypeDelegate delegate2;
            if (table == null)
            {
                throw new InternalException("DesignTable should not be null.");
            }
            string generatorRowClassName = table.GeneratorRowClassName;
            delegate2 = new CodeTypeDelegate(table.GeneratorRowEvHandlerName) {
                TypeAttributes = delegate2.TypeAttributes | TypeAttributes.Public
            };
            delegate2.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(object)), "sender"));
            delegate2.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.Type(table.GeneratorRowEvArgName), "e"));
            delegate2.CustomAttributes.Add(CodeGenHelper.GeneratedCodeAttributeDecl());
            return delegate2;
        }

        internal void GenerateTypedRowEventHandlers(CodeTypeDeclaration dataSourceClass)
        {
            if (this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareEvents) && this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareDelegates))
            {
                foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
                {
                    dataSourceClass.Members.Add(this.GenerateTypedRowEventHandler(table));
                }
            }
        }

        private CodeConstructor RowConstructor(string tableClassName, string tableFieldName)
        {
            CodeConstructor constructor = CodeGenHelper.Constructor(MemberAttributes.Assembly | MemberAttributes.Final);
            constructor.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(DataRowBuilder)), "rb"));
            constructor.BaseConstructorArgs.Add(CodeGenHelper.Argument("rb"));
            constructor.Statements.Add(CodeGenHelper.Assign(CodeGenHelper.Field(CodeGenHelper.This(), tableFieldName), CodeGenHelper.Cast(CodeGenHelper.Type(tableClassName), CodeGenHelper.Property(CodeGenHelper.This(), "Table"))));
            return constructor;
        }

        internal MethodInfo ConvertXmlToObject
        {
            get
            {
                return this.convertXmlToObject;
            }
        }
    }
}

