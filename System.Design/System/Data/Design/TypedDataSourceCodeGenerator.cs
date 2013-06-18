namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Reflection;
    using System.Xml.Serialization;

    internal sealed class TypedDataSourceCodeGenerator
    {
        private CodeDomProvider codeProvider;
        private string dataSetNamespace;
        private DesignDataSource designDataSource;
        private bool generateExtendedProperties;
        private System.Data.Design.TypedDataSetGenerator.GenerateOption generateOption;
        private bool generateSingleNamespace;
        private ArrayList problemList = new ArrayList();
        private System.Data.Design.RelationHandler relationHandler;
        private TypedRowHandler rowHandler;
        private TypedTableHandler tableHandler;
        private IDictionary userData;

        internal TypedDataSourceCodeGenerator()
        {
        }

        private void AddUserData(CodeCompileUnit compileUnit)
        {
            if (this.UserData != null)
            {
                foreach (object obj2 in this.UserData.Keys)
                {
                    compileUnit.UserData.Add(obj2, this.userData[obj2]);
                }
            }
        }

        private string CreateAdaptersNamespace(string generatorDataSetName)
        {
            if (generatorDataSetName.StartsWith("[", StringComparison.Ordinal) && generatorDataSetName.EndsWith("]", StringComparison.Ordinal))
            {
                generatorDataSetName = generatorDataSetName.Substring(1, generatorDataSetName.Length - 2);
            }
            return MemberNameValidator.GenerateIdName(generatorDataSetName + "TableAdapters", this.CodeProvider, false);
        }

        private CodeTypeDeclaration CreateDataSourceDeclaration(DesignDataSource dtDataSource)
        {
            if (dtDataSource.Name == null)
            {
                throw new DataSourceGeneratorException("DataSource name cannot be null.");
            }
            new NameHandler(this.codeProvider).GenerateMemberNames(dtDataSource, this.problemList);
            CodeTypeDeclaration dataSourceClass = CodeGenHelper.Class(dtDataSource.GeneratorDataSetName, true, dtDataSource.Modifier);
            dataSourceClass.BaseTypes.Add(CodeGenHelper.GlobalType(typeof(DataSet)));
            dataSourceClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.Serializable"));
            dataSourceClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.DesignerCategoryAttribute", CodeGenHelper.Str("code")));
            dataSourceClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl("System.ComponentModel.ToolboxItem", CodeGenHelper.Primitive(true)));
            dataSourceClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(XmlSchemaProviderAttribute).FullName, CodeGenHelper.Primitive("GetTypedDataSetSchema")));
            dataSourceClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(XmlRootAttribute).FullName, CodeGenHelper.Primitive(dtDataSource.GeneratorDataSetName)));
            dataSourceClass.CustomAttributes.Add(CodeGenHelper.AttributeDecl(typeof(HelpKeywordAttribute).FullName, CodeGenHelper.Str("vs.data.DataSet")));
            dataSourceClass.Comments.Add(CodeGenHelper.Comment("Represents a strongly typed in-memory cache of data.", true));
            this.tableHandler = new TypedTableHandler(this, dtDataSource.DesignTables);
            this.relationHandler = new System.Data.Design.RelationHandler(this, dtDataSource.DesignRelations);
            this.rowHandler = new TypedRowHandler(this, dtDataSource.DesignTables);
            DatasetMethodGenerator generator = new DatasetMethodGenerator(this, dtDataSource);
            this.tableHandler.AddPrivateVars(dataSourceClass);
            this.tableHandler.AddTableProperties(dataSourceClass);
            this.relationHandler.AddPrivateVars(dataSourceClass);
            generator.AddMethods(dataSourceClass);
            this.rowHandler.AddTypedRowEventHandlers(dataSourceClass);
            this.tableHandler.AddTableClasses(dataSourceClass);
            this.rowHandler.AddTypedRows(dataSourceClass);
            this.rowHandler.AddTypedRowEventArgs(dataSourceClass);
            return dataSourceClass;
        }

        internal void GenerateDataSource(DesignDataSource dtDataSource, CodeCompileUnit codeCompileUnit, CodeNamespace mainNamespace, string dataSetNamespace, System.Data.Design.TypedDataSetGenerator.GenerateOption generateOption)
        {
            this.designDataSource = dtDataSource;
            this.generateOption = generateOption;
            this.dataSetNamespace = dataSetNamespace;
            bool generateHierarchicalUpdate = ((generateOption & System.Data.Design.TypedDataSetGenerator.GenerateOption.HierarchicalUpdate) == System.Data.Design.TypedDataSetGenerator.GenerateOption.HierarchicalUpdate) && dtDataSource.EnableTableAdapterManager;
            this.AddUserData(codeCompileUnit);
            CodeTypeDeclaration declaration = this.CreateDataSourceDeclaration(dtDataSource);
            mainNamespace.Types.Add(declaration);
            bool flag2 = CodeGenHelper.SupportsMultipleNamespaces(this.codeProvider);
            CodeNamespace namespace2 = null;
            if (!this.GenerateSingleNamespace && flag2)
            {
                string name = this.CreateAdaptersNamespace(dtDataSource.GeneratorDataSetName);
                if (!StringUtil.Empty(mainNamespace.Name))
                {
                    name = mainNamespace.Name + "." + name;
                }
                namespace2 = new CodeNamespace(name);
            }
            DataComponentGenerator generator = new DataComponentGenerator(this);
            bool flag3 = false;
            foreach (DesignTable table in dtDataSource.DesignTables)
            {
                if (table.TableType == TableType.RadTable)
                {
                    flag3 = true;
                    table.PropertyCache = new DesignTable.CodeGenPropertyCache(table);
                    CodeTypeDeclaration declaration2 = generator.GenerateDataComponent(table, false, generateHierarchicalUpdate);
                    if (this.GenerateSingleNamespace)
                    {
                        mainNamespace.Types.Add(declaration2);
                    }
                    else if (flag2)
                    {
                        namespace2.Types.Add(declaration2);
                    }
                    else
                    {
                        declaration2.Name = declaration.Name + declaration2.Name;
                        mainNamespace.Types.Add(declaration2);
                    }
                }
            }
            generateHierarchicalUpdate = generateHierarchicalUpdate && flag3;
            if ((dtDataSource.Sources != null) && (dtDataSource.Sources.Count > 0))
            {
                DesignTable designTable = new DesignTable {
                    TableType = TableType.RadTable,
                    MainSource = null,
                    GeneratorDataComponentClassName = dtDataSource.GeneratorFunctionsComponentClassName
                };
                foreach (Source source in dtDataSource.Sources)
                {
                    designTable.Sources.Add(source);
                }
                CodeTypeDeclaration declaration3 = generator.GenerateDataComponent(designTable, true, generateHierarchicalUpdate);
                if (this.GenerateSingleNamespace)
                {
                    mainNamespace.Types.Add(declaration3);
                }
                else if (flag2)
                {
                    namespace2.Types.Add(declaration3);
                }
                else
                {
                    declaration3.Name = declaration.Name + declaration3.Name;
                    mainNamespace.Types.Add(declaration3);
                }
            }
            if ((namespace2 != null) && (namespace2.Types.Count > 0))
            {
                codeCompileUnit.Namespaces.Add(namespace2);
            }
            if (generateHierarchicalUpdate)
            {
                CodeTypeDeclaration declaration4 = new TableAdapterManagerGenerator(this).GenerateAdapterManager(this.designDataSource, declaration);
                if (this.GenerateSingleNamespace)
                {
                    mainNamespace.Types.Add(declaration4);
                }
                else if (flag2)
                {
                    namespace2.Types.Add(declaration4);
                }
                else
                {
                    declaration4.Name = declaration.Name + declaration4.Name;
                    mainNamespace.Types.Add(declaration4);
                }
            }
        }

        internal static ArrayList GetProviderAssemblies(DesignDataSource designDS)
        {
            ArrayList list = new ArrayList();
            foreach (IDesignConnection connection in designDS.DesignConnections)
            {
                IDbConnection connection2 = connection.CreateEmptyDbConnection();
                if (connection2 != null)
                {
                    Assembly item = connection2.GetType().Assembly;
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        internal CodeDomProvider CodeProvider
        {
            get
            {
                return this.codeProvider;
            }
            set
            {
                this.codeProvider = value;
            }
        }

        internal string DataSetNamespace
        {
            get
            {
                return this.dataSetNamespace;
            }
        }

        internal string DataSourceName
        {
            get
            {
                return this.designDataSource.GeneratorDataSetName;
            }
        }

        internal bool GenerateExtendedProperties
        {
            get
            {
                return this.generateExtendedProperties;
            }
        }

        internal System.Data.Design.TypedDataSetGenerator.GenerateOption GenerateOptions
        {
            get
            {
                return this.generateOption;
            }
        }

        internal bool GenerateSingleNamespace
        {
            get
            {
                return this.generateSingleNamespace;
            }
            set
            {
                this.generateSingleNamespace = value;
            }
        }

        internal ArrayList ProblemList
        {
            get
            {
                return this.problemList;
            }
        }

        internal System.Data.Design.RelationHandler RelationHandler
        {
            get
            {
                return this.relationHandler;
            }
        }

        internal TypedRowHandler RowHandler
        {
            get
            {
                return this.rowHandler;
            }
        }

        internal TypedTableHandler TableHandler
        {
            get
            {
                return this.tableHandler;
            }
        }

        internal IDictionary UserData
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }
    }
}

