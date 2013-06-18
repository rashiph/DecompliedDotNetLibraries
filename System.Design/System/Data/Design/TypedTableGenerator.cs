namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Data;
    using System.Reflection;
    using System.Xml.Serialization;

    internal sealed class TypedTableGenerator
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        private static string LINQOverTDSTableBaseClass = "System.Data.TypedTableBase";

        internal TypedTableGenerator(TypedDataSourceCodeGenerator codeGenerator)
        {
            this.codeGenerator = codeGenerator;
        }

        private CodeMemberProperty CountProperty()
        {
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.GlobalType(typeof(int)), "Count", MemberAttributes.Public | MemberAttributes.Final);
            property.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.Browsable", CodeGenHelper.Primitive(false)));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Property(CodeGenHelper.Property(CodeGenHelper.This(), "Rows"), "Count")));
            return property;
        }

        private CodeTypeDeclaration GenerateTable(DesignTable designTable, CodeTypeDeclaration dataSourceClass)
        {
            string generatorTableClassName = designTable.GeneratorTableClassName;
            TypedColumnHandler columnHandler = this.codeGenerator.TableHandler.GetColumnHandler(designTable.Name);
            CodeTypeDeclaration dataTableClass = CodeGenHelper.Class(generatorTableClassName, true, TypeAttributes.Public);
            if ((this.codeGenerator.GenerateOptions & System.Data.Design.TypedDataSetGenerator.GenerateOption.LinqOverTypedDatasets) == System.Data.Design.TypedDataSetGenerator.GenerateOption.LinqOverTypedDatasets)
            {
                dataTableClass.BaseTypes.Add(CodeGenHelper.GlobalGenericType(LINQOverTDSTableBaseClass, CodeGenHelper.Type(designTable.GeneratorRowClassName)));
            }
            else
            {
                dataTableClass.BaseTypes.Add(CodeGenHelper.GlobalType(typeof(DataTable)));
                dataTableClass.BaseTypes.Add(CodeGenHelper.GlobalType(typeof(IEnumerable)));
            }
            dataTableClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.Serializable"));
            dataTableClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(XmlSchemaProviderAttribute).FullName, CodeGenHelper.Primitive("GetTypedTableSchema")));
            dataTableClass.Comments.Add(CodeGenHelper.Comment("Represents the strongly named DataTable class.", true));
            columnHandler.AddPrivateVariables(dataTableClass);
            columnHandler.AddTableColumnProperties(dataTableClass);
            dataTableClass.Members.Add(this.CountProperty());
            if (this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareIndexerProperties))
            {
                dataTableClass.Members.Add(this.IndexProperty(designTable));
            }
            if (this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareEvents) && this.codeGenerator.CodeProvider.Supports(GeneratorSupport.DeclareDelegates))
            {
                this.codeGenerator.RowHandler.AddTypedRowEvents(dataTableClass, designTable.Name);
            }
            new TableMethodGenerator(this.codeGenerator, designTable).AddMethods(dataTableClass);
            return dataTableClass;
        }

        internal void GenerateTables(CodeTypeDeclaration dataSourceClass)
        {
            if (dataSourceClass == null)
            {
                throw new InternalException("DataSource CodeTypeDeclaration should not be null.");
            }
            foreach (DesignTable table in this.codeGenerator.TableHandler.Tables)
            {
                dataSourceClass.Members.Add(this.GenerateTable(table, dataSourceClass));
            }
        }

        private CodeMemberProperty IndexProperty(DesignTable designTable)
        {
            string generatorRowClassName = designTable.GeneratorRowClassName;
            CodeMemberProperty property = CodeGenHelper.PropertyDecl(CodeGenHelper.Type(generatorRowClassName), "Item", MemberAttributes.Public | MemberAttributes.Final);
            property.Parameters.Add(CodeGenHelper.ParameterDecl(CodeGenHelper.GlobalType(typeof(int)), "index"));
            property.GetStatements.Add(CodeGenHelper.Return(CodeGenHelper.Cast(CodeGenHelper.Type(generatorRowClassName), CodeGenHelper.Indexer(CodeGenHelper.Property(CodeGenHelper.This(), "Rows"), CodeGenHelper.Argument("index")))));
            return property;
        }
    }
}

