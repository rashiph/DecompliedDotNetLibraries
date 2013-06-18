namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Data;

    internal class QueryHandler
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        internal const string dataSetParameterName = "dataSet";
        private bool declarationsOnly;
        private DesignTable designTable;
        private bool languageSupportsNullables;
        internal const string tableParameterName = "dataTable";

        internal QueryHandler(CodeDomProvider codeProvider, DesignTable designTable)
        {
            this.codeGenerator = new TypedDataSourceCodeGenerator();
            this.codeGenerator.CodeProvider = codeProvider;
            this.designTable = designTable;
            this.languageSupportsNullables = this.codeGenerator.CodeProvider.Supports(GeneratorSupport.GenericTypeReference);
        }

        internal QueryHandler(TypedDataSourceCodeGenerator codeGenerator, DesignTable designTable)
        {
            this.codeGenerator = codeGenerator;
            this.designTable = designTable;
            this.languageSupportsNullables = this.codeGenerator.CodeProvider.Supports(GeneratorSupport.GenericTypeReference);
        }

        internal void AddFunctionsToDataComponent(CodeTypeDeclaration classDeclaration, bool isFunctionsDataComponent)
        {
            if (this.designTable == null)
            {
                throw new InternalException("Design Table should not be null.");
            }
            if ((!isFunctionsDataComponent && (this.designTable.MainSource != null)) && ((((DbSource) this.designTable.MainSource).QueryType != QueryType.Rowset) || (((DbSource) this.designTable.MainSource).CommandOperation != CommandOperation.Select)))
            {
                this.AddFunctionToDataComponent(classDeclaration, (DbSource) this.designTable.MainSource, 0, isFunctionsDataComponent);
            }
            if (this.designTable.Sources != null)
            {
                int commandIndex = 1;
                if (isFunctionsDataComponent)
                {
                    commandIndex = 0;
                }
                foreach (Source source in this.designTable.Sources)
                {
                    if ((((DbSource) source).QueryType != QueryType.Rowset) || (((DbSource) source).CommandOperation != CommandOperation.Select))
                    {
                        this.AddFunctionToDataComponent(classDeclaration, (DbSource) source, commandIndex, isFunctionsDataComponent);
                    }
                    commandIndex++;
                }
            }
        }

        private void AddFunctionToDataComponent(CodeTypeDeclaration classDeclaration, DbSource dbSource, int commandIndex, bool isFunctionsDataComponent)
        {
            FunctionGenerator generator;
            if (!this.DeclarationsOnly || (dbSource.Modifier == MemberAttributes.Public))
            {
                generator = new FunctionGenerator(this.codeGenerator) {
                    DeclarationOnly = this.declarationsOnly,
                    MethodSource = dbSource,
                    CommandIndex = commandIndex,
                    DesignTable = this.designTable,
                    IsFunctionsDataComponent = isFunctionsDataComponent
                };
                if (generator.MethodSource.Connection != null)
                {
                    generator.ProviderFactory = ProviderManager.GetFactory(generator.MethodSource.Connection.Provider);
                    goto Label_00A5;
                }
                if (this.designTable.Connection != null)
                {
                    generator.ProviderFactory = ProviderManager.GetFactory(this.designTable.Connection.Provider);
                    goto Label_00A5;
                }
            }
            return;
        Label_00A5:
            generator.MethodName = dbSource.GeneratorSourceName;
            generator.ParameterOption = this.languageSupportsNullables ? ParameterGenerationOption.ClrTypes : ParameterGenerationOption.Objects;
            CodeMemberMethod method = generator.Generate();
            if (method != null)
            {
                classDeclaration.Members.Add(method);
            }
        }

        private void AddMainQueriesToDataComponent(CodeTypeDeclaration classDeclaration)
        {
            if (this.designTable == null)
            {
                throw new InternalException("Design Table should not be null.");
            }
            if (this.designTable.MainSource != null)
            {
                QueryGenerator queryGenerator = new QueryGenerator(this.codeGenerator) {
                    DeclarationOnly = this.declarationsOnly,
                    MethodSource = this.designTable.MainSource as DbSource,
                    CommandIndex = 0,
                    DesignTable = this.designTable
                };
                if (queryGenerator.MethodSource.Connection != null)
                {
                    queryGenerator.ProviderFactory = ProviderManager.GetFactory(queryGenerator.MethodSource.Connection.Provider);
                }
                else if (this.designTable.Connection != null)
                {
                    queryGenerator.ProviderFactory = ProviderManager.GetFactory(this.designTable.Connection.Provider);
                }
                else
                {
                    return;
                }
                if ((queryGenerator.MethodSource.QueryType == QueryType.Rowset) && (queryGenerator.MethodSource.CommandOperation == CommandOperation.Select))
                {
                    if ((queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Fill) || (queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Both))
                    {
                        queryGenerator.MethodName = this.designTable.MainSource.GeneratorSourceName;
                        this.GenerateQueries(classDeclaration, queryGenerator);
                        if (queryGenerator.MethodSource.GeneratePagingMethods)
                        {
                            queryGenerator.MethodName = this.designTable.MainSource.GeneratorSourceNameForPaging;
                            queryGenerator.GeneratePagingMethod = true;
                            this.GenerateQueries(classDeclaration, queryGenerator);
                            queryGenerator.GeneratePagingMethod = false;
                        }
                    }
                    if ((queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Get) || (queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Both))
                    {
                        queryGenerator.GenerateGetMethod = true;
                        queryGenerator.MethodName = this.designTable.MainSource.GeneratorGetMethodName;
                        this.GenerateQueries(classDeclaration, queryGenerator);
                        if (queryGenerator.MethodSource.GeneratePagingMethods)
                        {
                            queryGenerator.MethodName = this.designTable.MainSource.GeneratorGetMethodNameForPaging;
                            queryGenerator.GeneratePagingMethod = true;
                            this.GenerateQueries(classDeclaration, queryGenerator);
                            queryGenerator.GeneratePagingMethod = false;
                        }
                    }
                }
            }
        }

        internal void AddQueriesToDataComponent(CodeTypeDeclaration classDeclaration)
        {
            this.AddMainQueriesToDataComponent(classDeclaration);
            this.AddSecondaryQueriesToDataComponent(classDeclaration);
            this.AddUpdateQueriesToDataComponent(classDeclaration);
            this.AddFunctionsToDataComponent(classDeclaration, false);
        }

        private void AddSecondaryQueriesToDataComponent(CodeTypeDeclaration classDeclaration)
        {
            if (this.designTable == null)
            {
                throw new InternalException("Design Table should not be null.");
            }
            if (this.designTable.Sources != null)
            {
                QueryGenerator queryGenerator = new QueryGenerator(this.codeGenerator) {
                    DeclarationOnly = this.declarationsOnly,
                    DesignTable = this.designTable
                };
                if (this.designTable.Connection != null)
                {
                    queryGenerator.ProviderFactory = ProviderManager.GetFactory(this.designTable.Connection.Provider);
                }
                int num = 1;
                foreach (Source source in this.designTable.Sources)
                {
                    queryGenerator.MethodSource = source as DbSource;
                    queryGenerator.CommandIndex = num++;
                    if ((queryGenerator.MethodSource.QueryType == QueryType.Rowset) && (queryGenerator.MethodSource.CommandOperation == CommandOperation.Select))
                    {
                        if (queryGenerator.MethodSource.Connection != null)
                        {
                            queryGenerator.ProviderFactory = ProviderManager.GetFactory(this.designTable.Connection.Provider);
                        }
                        if ((queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Fill) || (queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Both))
                        {
                            queryGenerator.GenerateGetMethod = false;
                            queryGenerator.MethodName = source.GeneratorSourceName;
                            this.GenerateQueries(classDeclaration, queryGenerator);
                            if (queryGenerator.MethodSource.GeneratePagingMethods)
                            {
                                queryGenerator.MethodName = source.GeneratorSourceNameForPaging;
                                queryGenerator.GeneratePagingMethod = true;
                                this.GenerateQueries(classDeclaration, queryGenerator);
                                queryGenerator.GeneratePagingMethod = false;
                            }
                        }
                        if ((queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Get) || (queryGenerator.MethodSource.GenerateMethods == GenerateMethodTypes.Both))
                        {
                            queryGenerator.GenerateGetMethod = true;
                            queryGenerator.MethodName = source.GeneratorGetMethodName;
                            this.GenerateQueries(classDeclaration, queryGenerator);
                            if (queryGenerator.MethodSource.GeneratePagingMethods)
                            {
                                queryGenerator.MethodName = source.GeneratorGetMethodNameForPaging;
                                queryGenerator.GeneratePagingMethod = true;
                                this.GenerateQueries(classDeclaration, queryGenerator);
                                queryGenerator.GeneratePagingMethod = false;
                            }
                        }
                    }
                }
            }
        }

        private void AddUpdateQueriesToDataComponent(CodeTypeDeclaration classDeclaration)
        {
            this.AddUpdateQueriesToDataComponent(classDeclaration, this.codeGenerator.DataSourceName, this.codeGenerator.CodeProvider);
        }

        internal void AddUpdateQueriesToDataComponent(CodeTypeDeclaration classDeclaration, string dataSourceClassName, CodeDomProvider codeProvider)
        {
            if (this.designTable == null)
            {
                throw new InternalException("Design Table should not be null.");
            }
            if (StringUtil.EmptyOrSpace(dataSourceClassName))
            {
                throw new InternalException("Data source class name should not be empty");
            }
            if (this.designTable.HasAnyUpdateCommand)
            {
                UpdateCommandGenerator generator = new UpdateCommandGenerator(this.codeGenerator) {
                    CodeProvider = codeProvider,
                    DeclarationOnly = this.declarationsOnly,
                    MethodSource = this.designTable.MainSource as DbSource,
                    DesignTable = this.designTable
                };
                if (this.designTable.Connection != null)
                {
                    generator.ProviderFactory = ProviderManager.GetFactory(this.designTable.Connection.Provider);
                }
                else if (!this.declarationsOnly)
                {
                    throw new InternalException("DesignTable.Connection should not be null to generate update query statements.");
                }
                CodeMemberMethod method = null;
                generator.MethodName = DataComponentNameHandler.UpdateMethodName;
                generator.ActiveCommand = generator.MethodSource.UpdateCommand;
                generator.MethodType = MethodTypeEnum.GenericUpdate;
                generator.UpdateParameterTypeReference = CodeGenHelper.GlobalType(typeof(DataTable));
                generator.UpdateParameterName = "dataTable";
                generator.UpdateParameterTypeName = CodeGenHelper.GetTypeName(codeProvider, dataSourceClassName, this.designTable.GeneratorTableClassName);
                if (this.codeGenerator.DataSetNamespace != null)
                {
                    generator.UpdateParameterTypeName = CodeGenHelper.GetTypeName(this.codeGenerator.CodeProvider, this.codeGenerator.DataSetNamespace, generator.UpdateParameterTypeName);
                }
                method = generator.Generate();
                if (method != null)
                {
                    classDeclaration.Members.Add(method);
                }
                generator.UpdateParameterTypeReference = CodeGenHelper.GlobalType(typeof(DataSet));
                generator.UpdateParameterName = "dataSet";
                generator.UpdateParameterTypeName = dataSourceClassName;
                if (this.codeGenerator.DataSetNamespace != null)
                {
                    generator.UpdateParameterTypeName = CodeGenHelper.GetTypeName(this.codeGenerator.CodeProvider, this.codeGenerator.DataSetNamespace, generator.UpdateParameterTypeName);
                }
                method = generator.Generate();
                if (method != null)
                {
                    classDeclaration.Members.Add(method);
                }
                generator.UpdateParameterTypeReference = CodeGenHelper.GlobalType(typeof(DataRow));
                generator.UpdateParameterName = "dataRow";
                generator.UpdateParameterTypeName = null;
                method = generator.Generate();
                if (method != null)
                {
                    classDeclaration.Members.Add(method);
                }
                generator.UpdateParameterTypeReference = CodeGenHelper.GlobalType(typeof(DataRow), 1);
                generator.UpdateParameterName = "dataRows";
                generator.UpdateParameterTypeName = null;
                method = generator.Generate();
                if (method != null)
                {
                    classDeclaration.Members.Add(method);
                }
                if (generator.MethodSource.GenerateShortCommands)
                {
                    generator.MethodType = MethodTypeEnum.ColumnParameters;
                    generator.ActiveCommand = generator.MethodSource.DeleteCommand;
                    if (generator.ActiveCommand != null)
                    {
                        generator.MethodName = DataComponentNameHandler.DeleteMethodName;
                        generator.UpdateCommandName = "DeleteCommand";
                        generator.ParameterOption = this.languageSupportsNullables ? ParameterGenerationOption.ClrTypes : ParameterGenerationOption.Objects;
                        method = generator.Generate();
                        if (method != null)
                        {
                            classDeclaration.Members.Add(method);
                        }
                    }
                    generator.ActiveCommand = generator.MethodSource.InsertCommand;
                    if (generator.ActiveCommand != null)
                    {
                        generator.MethodName = DataComponentNameHandler.InsertMethodName;
                        generator.UpdateCommandName = "InsertCommand";
                        generator.ParameterOption = this.languageSupportsNullables ? ParameterGenerationOption.ClrTypes : ParameterGenerationOption.Objects;
                        method = generator.Generate();
                        if (method != null)
                        {
                            classDeclaration.Members.Add(method);
                        }
                    }
                    generator.ActiveCommand = generator.MethodSource.UpdateCommand;
                    if (generator.ActiveCommand != null)
                    {
                        generator.MethodName = DataComponentNameHandler.UpdateMethodName;
                        generator.UpdateCommandName = "UpdateCommand";
                        generator.ParameterOption = this.languageSupportsNullables ? ParameterGenerationOption.ClrTypes : ParameterGenerationOption.Objects;
                        method = generator.Generate();
                        if (method != null)
                        {
                            classDeclaration.Members.Add(method);
                            method = null;
                            generator.GenerateOverloadWithoutCurrentPKParameters = true;
                            try
                            {
                                method = generator.Generate();
                            }
                            finally
                            {
                                generator.GenerateOverloadWithoutCurrentPKParameters = false;
                            }
                            if (method != null)
                            {
                                classDeclaration.Members.Add(method);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateQueries(CodeTypeDeclaration classDeclaration, QueryGenerator queryGenerator)
        {
            CodeMemberMethod method = null;
            if (queryGenerator.DeclarationOnly)
            {
                if (!queryGenerator.GenerateGetMethod && (queryGenerator.MethodSource.Modifier != MemberAttributes.Public))
                {
                    return;
                }
                if (queryGenerator.GenerateGetMethod && (queryGenerator.MethodSource.GetMethodModifier != MemberAttributes.Public))
                {
                    return;
                }
            }
            queryGenerator.ContainerParameterType = typeof(DataTable);
            queryGenerator.ContainerParameterTypeName = CodeGenHelper.GetTypeName(this.codeGenerator.CodeProvider, this.codeGenerator.DataSourceName, this.designTable.GeneratorTableClassName);
            if (this.codeGenerator.DataSetNamespace != null)
            {
                queryGenerator.ContainerParameterTypeName = CodeGenHelper.GetTypeName(this.codeGenerator.CodeProvider, this.codeGenerator.DataSetNamespace, queryGenerator.ContainerParameterTypeName);
            }
            queryGenerator.ContainerParameterName = "dataTable";
            queryGenerator.ParameterOption = this.languageSupportsNullables ? ParameterGenerationOption.ClrTypes : ParameterGenerationOption.Objects;
            method = queryGenerator.Generate();
            if (method != null)
            {
                classDeclaration.Members.Add(method);
            }
        }

        internal bool DeclarationsOnly
        {
            get
            {
                return this.declarationsOnly;
            }
            set
            {
                this.declarationsOnly = value;
            }
        }
    }
}

