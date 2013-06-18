namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;

    internal sealed class TypedTableHandler
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        private Hashtable columnHandlers;
        private TypedTableGenerator tableGenerator;
        private DesignTableCollection tables;

        internal TypedTableHandler(TypedDataSourceCodeGenerator codeGenerator, DesignTableCollection tables)
        {
            this.codeGenerator = codeGenerator;
            this.tables = tables;
            this.tableGenerator = new TypedTableGenerator(codeGenerator);
            this.SetColumnHandlers();
        }

        internal void AddPrivateVars(CodeTypeDeclaration dataSourceClass)
        {
            if (this.tables != null)
            {
                foreach (DesignTable table in this.tables)
                {
                    string generatorTableClassName = table.GeneratorTableClassName;
                    string generatorTableVarName = table.GeneratorTableVarName;
                    dataSourceClass.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.Type(generatorTableClassName), generatorTableVarName));
                }
            }
        }

        internal void AddTableClasses(CodeTypeDeclaration dataSourceClass)
        {
            this.tableGenerator.GenerateTables(dataSourceClass);
        }

        internal void AddTableProperties(CodeTypeDeclaration dataSourceClass)
        {
            if (this.tables != null)
            {
                foreach (DesignTable table in this.tables)
                {
                    string generatorTableClassName = table.GeneratorTableClassName;
                    string generatorTablePropName = table.GeneratorTablePropName;
                    string generatorTableVarName = table.GeneratorTableVarName;
                    CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.Type(generatorTableClassName), generatorTablePropName, MemberAttributes.Public | MemberAttributes.Final);
                    property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.Browsable", CodeGenHelper.Primitive(false)));
                    property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerSerializationVisibility", CodeGenHelper.Field(CodeGenHelper.GlobalTypeExpr(typeof(DesignerSerializationVisibility)), "Content")));
                    property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Field(CodeGenHelper.This(), generatorTableVarName)));
                    dataSourceClass.Members.Add(property);
                }
            }
        }

        internal TypedColumnHandler GetColumnHandler(string tableName)
        {
            if (tableName == null)
            {
                return null;
            }
            return (TypedColumnHandler) this.columnHandlers[tableName];
        }

        private void SetColumnHandlers()
        {
            this.columnHandlers = new Hashtable();
            foreach (DesignTable table in this.tables)
            {
                this.columnHandlers.Add(table.Name, new TypedColumnHandler(table, this.codeGenerator));
            }
        }

        internal DesignTableCollection Tables
        {
            get
            {
                return this.tables;
            }
        }
    }
}

