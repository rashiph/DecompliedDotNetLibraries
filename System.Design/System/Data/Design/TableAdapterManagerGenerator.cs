namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;

    internal sealed class TableAdapterManagerGenerator
    {
        private const string adapterDesigner = "Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerDesigner";
        private TypedDataSourceCodeGenerator dataSourceGenerator;
        private const string helpKeyword = "vs.data.TableAdapterManager";

        internal TableAdapterManagerGenerator(TypedDataSourceCodeGenerator codeGenerator)
        {
            this.dataSourceGenerator = codeGenerator;
        }

        internal CodeTypeDeclaration GenerateAdapterManager(DesignDataSource dataSource, CodeTypeDeclaration dataSourceClass)
        {
            TypeAttributes @public = TypeAttributes.Public;
            foreach (DesignTable table in dataSource.DesignTables)
            {
                if ((table.DataAccessorModifier & TypeAttributes.Public) != TypeAttributes.Public)
                {
                    @public = table.DataAccessorModifier;
                }
            }
            CodeTypeDeclaration dataComponentClass = CodeGenHelper.Class("TableAdapterManager", true, @public);
            dataComponentClass.Comments.Add(CodeGenHelper.Comment("TableAdapterManager is used to coordinate TableAdapters in the dataset to enable Hierarchical Update scenarios", true));
            dataComponentClass.BaseTypes.Add(CodeGenHelper.GlobalType(typeof(Component)));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerCategoryAttribute", CodeGenHelper.Str("code")));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.ToolboxItem", CodeGenHelper.Primitive(true)));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerAttribute", CodeGenHelper.Str("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(HelpKeywordAttribute).FullName, CodeGenHelper.Str("vs.data.TableAdapterManager")));
            new TableAdapterManagerMethodGenerator(this.dataSourceGenerator, dataSource, dataSourceClass).AddEverything(dataComponentClass);
            try
            {
                CodeGenerator.ValidateIdentifiers(dataComponentClass);
            }
            catch (Exception)
            {
            }
            return dataComponentClass;
        }
    }
}

