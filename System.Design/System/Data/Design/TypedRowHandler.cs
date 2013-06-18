namespace System.Data.Design
{
    using System;
    using System.CodeDom;

    internal sealed class TypedRowHandler
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        private TypedRowGenerator rowGenerator;
        private DesignTableCollection tables;

        internal TypedRowHandler(TypedDataSourceCodeGenerator codeGenerator, DesignTableCollection tables)
        {
            this.codeGenerator = codeGenerator;
            this.tables = tables;
            this.rowGenerator = new TypedRowGenerator(codeGenerator);
        }

        internal void AddTypedRowEventArgs(CodeTypeDeclaration dataSourceClass)
        {
            this.rowGenerator.GenerateTypedRowEventArgs(dataSourceClass);
        }

        internal void AddTypedRowEventHandlers(CodeTypeDeclaration dataSourceClass)
        {
            this.rowGenerator.GenerateTypedRowEventHandlers(dataSourceClass);
        }

        internal void AddTypedRowEvents(CodeTypeDeclaration dataTableClass, string tableName)
        {
            DesignTable table = this.codeGenerator.TableHandler.Tables[tableName];
            string generatorRowClassName = table.GeneratorRowClassName;
            string generatorRowEvHandlerName = table.GeneratorRowEvHandlerName;
            dataTableClass.Members.Add(CodeGenHelper.EventDecl(generatorRowEvHandlerName, table.GeneratorRowChangingName));
            dataTableClass.Members.Add(CodeGenHelper.EventDecl(generatorRowEvHandlerName, table.GeneratorRowChangedName));
            dataTableClass.Members.Add(CodeGenHelper.EventDecl(generatorRowEvHandlerName, table.GeneratorRowDeletingName));
            dataTableClass.Members.Add(CodeGenHelper.EventDecl(generatorRowEvHandlerName, table.GeneratorRowDeletedName));
        }

        internal void AddTypedRows(CodeTypeDeclaration dataSourceClass)
        {
            this.rowGenerator.GenerateRows(dataSourceClass);
        }

        internal TypedRowGenerator RowGenerator
        {
            get
            {
                return this.rowGenerator;
            }
        }
    }
}

