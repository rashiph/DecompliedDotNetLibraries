namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel.Design;

    internal sealed class DataComponentGenerator
    {
        private static string adapterDesigner = "Microsoft.VSDesigner.DataSource.Design.TableAdapterDesigner";
        private TypedDataSourceCodeGenerator dataSourceGenerator;

        internal DataComponentGenerator(TypedDataSourceCodeGenerator codeGenerator)
        {
            this.dataSourceGenerator = codeGenerator;
        }

        internal CodeTypeDeclaration GenerateDataComponent(DesignTable designTable, bool isFunctionsComponent, bool generateHierarchicalUpdate)
        {
            CodeTypeDeclaration dataComponentClass = CodeGenHelper.Class(designTable.GeneratorDataComponentClassName, true, designTable.DataAccessorModifier);
            dataComponentClass.BaseTypes.Add(CodeGenHelper.GlobalType(designTable.BaseClass));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerCategoryAttribute", CodeGenHelper.Str("code")));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.ToolboxItem", CodeGenHelper.Primitive(true)));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DataObjectAttribute", CodeGenHelper.Primitive(true)));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerAttribute", CodeGenHelper.Str(adapterDesigner + ", Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")));
            dataComponentClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(HelpKeywordAttribute).FullName, CodeGenHelper.Str("vs.data.TableAdapter")));
            if (designTable.WebServiceAttribute)
            {
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration("System.Web.Services.WebService");
                declaration2.Arguments.Add(new CodeAttributeArgument("Namespace", CodeGenHelper.Str(designTable.WebServiceNamespace)));
                declaration2.Arguments.Add(new CodeAttributeArgument("Description", CodeGenHelper.Str(designTable.WebServiceDescription)));
                dataComponentClass.CustomAttributes.Add(declaration2);
            }
            dataComponentClass.Comments.Add(CodeGenHelper.Comment("Represents the connection and commands used to retrieve and save data.", true));
            new DataComponentMethodGenerator(this.dataSourceGenerator, designTable, generateHierarchicalUpdate).AddMethods(dataComponentClass, isFunctionsComponent);
            CodeGenerator.ValidateIdentifiers(dataComponentClass);
            QueryHandler handler = new QueryHandler(this.dataSourceGenerator, designTable);
            if (isFunctionsComponent)
            {
                handler.AddFunctionsToDataComponent(dataComponentClass, true);
                return dataComponentClass;
            }
            handler.AddQueriesToDataComponent(dataComponentClass);
            return dataComponentClass;
        }
    }
}

