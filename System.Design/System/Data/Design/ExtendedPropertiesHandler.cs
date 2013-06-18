namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Design;
    using System.Globalization;

    internal sealed class ExtendedPropertiesHandler
    {
        private static TypedDataSourceCodeGenerator codeGenerator;
        private static DataSourceComponent targetObject;

        private ExtendedPropertiesHandler()
        {
        }

        internal static void AddExtendedProperties(DataSourceComponent targetObj, CodeExpression addTarget, IList statementCollection, Hashtable extendedProperties)
        {
            if (extendedProperties != null)
            {
                if (addTarget == null)
                {
                    throw new InternalException("ExtendedPropertiesHandler.AddExtendedProperties: addTarget cannot be null");
                }
                if (statementCollection == null)
                {
                    throw new InternalException("ExtendedPropertiesHandler.AddExtendedProperties: statementCollection cannot be null");
                }
                if (codeGenerator == null)
                {
                    throw new InternalException("ExtendedPropertiesHandler.AddExtendedProperties: codeGenerator cannot be null");
                }
                if (targetObj == null)
                {
                    throw new InternalException("ExtendedPropertiesHandler.AddExtendedProperties: targetObject cannot be null");
                }
                targetObject = targetObj;
                if (codeGenerator.GenerateExtendedProperties)
                {
                    GenerateProperties(addTarget, statementCollection, extendedProperties);
                }
                else
                {
                    SortedList list = new SortedList(new Comparer(CultureInfo.InvariantCulture));
                    foreach (string str in targetObject.NamingPropertyNames)
                    {
                        string str2 = extendedProperties[str] as string;
                        if (!StringUtil.Empty(str2))
                        {
                            list.Add(str, str2);
                        }
                    }
                    GenerateProperties(addTarget, statementCollection, list);
                }
            }
        }

        private static void GenerateProperties(CodeExpression addTarget, IList statementCollection, ICollection extendedProperties)
        {
            if (extendedProperties != null)
            {
                IDictionaryEnumerator enumerator = (IDictionaryEnumerator) extendedProperties.GetEnumerator();
                if (enumerator != null)
                {
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        string key = enumerator.Key as string;
                        string primitive = enumerator.Value as string;
                        if ((key == null) || (primitive == null))
                        {
                            codeGenerator.ProblemList.Add(new DSGeneratorProblem(System.Design.SR.GetString("CG_UnableToReadExtProperties"), ProblemSeverity.NonFatalError, targetObject));
                        }
                        else
                        {
                            statementCollection.Add(CodeGenHelper.Stm(CodeGenHelper.MethodCall(CodeGenHelper.Property(addTarget, "ExtendedProperties"), "Add", new CodeExpression[] { CodeGenHelper.Primitive(key), CodeGenHelper.Primitive(primitive) })));
                        }
                    }
                }
            }
        }

        internal static TypedDataSourceCodeGenerator CodeGenerator
        {
            set
            {
                codeGenerator = value;
            }
        }
    }
}

