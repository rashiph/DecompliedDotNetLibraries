namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public sealed class TypedDataSetGenerator
    {
        private static Assembly dataAssembly = Assembly.GetAssembly(typeof(SqlDataAdapter));
        private static Assembly entityAssembly;
        private static Assembly[] fixedReferences = new Assembly[] { systemAssembly, dataAssembly, xmlAssembly };
        private static string[] imports = new string[0];
        private static string LINQOverTDSAssemblyName = "System.Data.DataSetExtensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        private static Assembly[] referencedAssemblies = null;
        private static Assembly systemAssembly = Assembly.GetAssembly(typeof(Uri));
        private static Assembly xmlAssembly = Assembly.GetAssembly(typeof(XmlSchemaType));

        private TypedDataSetGenerator()
        {
        }

        private static string CreateExceptionMessage(Exception e)
        {
            string str = (e.Message != null) ? e.Message : string.Empty;
            for (Exception exception = e.InnerException; exception != null; exception = exception.InnerException)
            {
                string message = exception.Message;
                if ((message != null) && (message.Length > 0))
                {
                    str = str + " " + message;
                }
            }
            return str;
        }

        public static string Generate(DataSet dataSet, CodeNamespace codeNamespace, CodeDomProvider codeProvider)
        {
            if (codeProvider == null)
            {
                throw new ArgumentNullException("codeProvider");
            }
            if (dataSet == null)
            {
                throw new ArgumentException(System.Design.SR.GetString("CG_DataSetGeneratorFail_DatasetNull"));
            }
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            dataSet.WriteXmlSchema(writer);
            return Generate(writer.GetStringBuilder().ToString(), null, codeNamespace, codeProvider);
        }

        public static string Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider)
        {
            return Generate(inputFileContent, compileUnit, mainNamespace, codeProvider, GenerateOption.None);
        }

        public static void Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, Hashtable customDBProviders)
        {
            Generate(inputFileContent, compileUnit, mainNamespace, codeProvider, customDBProviders, GenerateOption.None);
        }

        public static void Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, DbProviderFactory specifiedFactory)
        {
            if (specifiedFactory != null)
            {
                ProviderManager.ActiveFactoryContext = specifiedFactory;
            }
            try
            {
                Generate(inputFileContent, compileUnit, mainNamespace, codeProvider);
            }
            finally
            {
                ProviderManager.ActiveFactoryContext = null;
            }
        }

        public static string Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, GenerateOption option)
        {
            return Generate(inputFileContent, compileUnit, mainNamespace, codeProvider, option, null);
        }

        public static void Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, Hashtable customDBProviders, GenerateOption option)
        {
            Generate(inputFileContent, compileUnit, mainNamespace, codeProvider, customDBProviders, option, null);
        }

        public static string Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, GenerateOption option, string dataSetNamespace)
        {
            return Generate(inputFileContent, compileUnit, mainNamespace, codeProvider, option, dataSetNamespace, null);
        }

        public static void Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, Hashtable customDBProviders, GenerateOption option, string dataSetNamespace)
        {
            Generate(inputFileContent, compileUnit, mainNamespace, codeProvider, customDBProviders, option, dataSetNamespace, null);
        }

        public static string Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, GenerateOption option, string dataSetNamespace, string basePath)
        {
            if ((inputFileContent == null) || (inputFileContent.Length == 0))
            {
                throw new ArgumentException(System.Design.SR.GetString("CG_DataSetGeneratorFail_InputFileEmpty"));
            }
            if (mainNamespace == null)
            {
                throw new ArgumentException(System.Design.SR.GetString("CG_DataSetGeneratorFail_CodeNamespaceNull"));
            }
            if (codeProvider == null)
            {
                throw new ArgumentNullException("codeProvider");
            }
            StringReader textReader = new StringReader(inputFileContent);
            DesignDataSource designDS = new DesignDataSource();
            try
            {
                designDS.ReadXmlSchema(textReader, basePath);
            }
            catch (Exception exception)
            {
                throw new Exception(System.Design.SR.GetString("CG_DataSetGeneratorFail_UnableToConvertToDataSet", new object[] { CreateExceptionMessage(exception) }), exception);
            }
            return GenerateInternal(designDS, compileUnit, mainNamespace, codeProvider, option, dataSetNamespace);
        }

        public static void Generate(string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, Hashtable customDBProviders, GenerateOption option, string dataSetNamespace, string basePath)
        {
            if (customDBProviders != null)
            {
                ProviderManager.CustomDBProviders = customDBProviders;
            }
            try
            {
                Generate(inputFileContent, compileUnit, mainNamespace, codeProvider, option, dataSetNamespace, basePath);
            }
            finally
            {
                ProviderManager.CustomDBProviders = null;
            }
        }

        internal static string GenerateInternal(DesignDataSource designDS, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, GenerateOption generateOption, string dataSetNamespace)
        {
            if (StringUtil.Empty(designDS.Name))
            {
                designDS.Name = "DataSet1";
            }
            try
            {
                TypedDataSourceCodeGenerator generator = new TypedDataSourceCodeGenerator {
                    CodeProvider = codeProvider,
                    GenerateSingleNamespace = false
                };
                if (mainNamespace == null)
                {
                    mainNamespace = new CodeNamespace();
                }
                if (compileUnit == null)
                {
                    compileUnit = new CodeCompileUnit();
                    compileUnit.Namespaces.Add(mainNamespace);
                }
                generator.GenerateDataSource(designDS, compileUnit, mainNamespace, dataSetNamespace, generateOption);
                foreach (string str in imports)
                {
                    mainNamespace.Imports.Add(new CodeNamespaceImport(str));
                }
            }
            catch (Exception exception)
            {
                throw new Exception(System.Design.SR.GetString("CG_DataSetGeneratorFail_FailToGenerateCode", new object[] { CreateExceptionMessage(exception) }), exception);
            }
            ArrayList list = new ArrayList(fixedReferences);
            list.AddRange(TypedDataSourceCodeGenerator.GetProviderAssemblies(designDS));
            if ((generateOption & GenerateOption.LinqOverTypedDatasets) == GenerateOption.LinqOverTypedDatasets)
            {
                Assembly entityAssembly = EntityAssembly;
                if (entityAssembly != null)
                {
                    list.Add(entityAssembly);
                }
            }
            referencedAssemblies = (Assembly[]) list.ToArray(typeof(Assembly));
            foreach (Assembly assembly2 in referencedAssemblies)
            {
                compileUnit.ReferencedAssemblies.Add(assembly2.GetName().Name + ".dll");
            }
            return designDS.GeneratorDataSetName;
        }

        public static string GetProviderName(string inputFileContent)
        {
            return GetProviderName(inputFileContent, null);
        }

        public static string GetProviderName(string inputFileContent, string tableName)
        {
            if ((inputFileContent == null) || (inputFileContent.Length == 0))
            {
                throw new ArgumentException(System.Design.SR.GetString("CG_DataSetGeneratorFail_InputFileEmpty"));
            }
            StringReader textReader = new StringReader(inputFileContent);
            DesignDataSource source = new DesignDataSource();
            try
            {
                source.ReadXmlSchema(textReader, null);
            }
            catch (Exception exception)
            {
                throw new Exception(System.Design.SR.GetString("CG_DataSetGeneratorFail_UnableToConvertToDataSet", new object[] { CreateExceptionMessage(exception) }), exception);
            }
            if ((tableName == null) || (tableName.Length == 0))
            {
                if (source.DefaultConnection != null)
                {
                    return source.DefaultConnection.Provider;
                }
            }
            else
            {
                DesignTable table = source.DesignTables[tableName];
                if (table != null)
                {
                    return table.Connection.Provider;
                }
            }
            return null;
        }

        private static Assembly EntityAssembly
        {
            get
            {
                if (entityAssembly == null)
                {
                    try
                    {
                        entityAssembly = Assembly.Load(LINQOverTDSAssemblyName);
                    }
                    catch
                    {
                    }
                }
                return entityAssembly;
            }
        }

        public static ICollection<Assembly> ReferencedAssemblies
        {
            get
            {
                return referencedAssemblies;
            }
        }

        [Flags]
        public enum GenerateOption
        {
            None,
            HierarchicalUpdate,
            LinqOverTypedDatasets
        }
    }
}

