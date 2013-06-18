namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Text.RegularExpressions;

    public sealed class AttributeInfo
    {
        private string[] argumentNames;
        private object[] argumentValues;
        private Type attributeType;

        internal AttributeInfo(Type attributeType, string[] argumentNames, object[] argumentValues)
        {
            this.attributeType = attributeType;
            this.argumentNames = (string[]) argumentNames.Clone();
            this.argumentValues = (object[]) argumentValues.Clone();
        }

        public Attribute CreateAttribute()
        {
            if (!this.Creatable)
            {
                throw new InvalidOperationException(SR.GetString("CannotCreateAttribute"));
            }
            List<string> list = new List<string>();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            for (int i = 0; i < this.argumentNames.Length; i++)
            {
                if ((this.argumentNames[i] == null) || (this.argumentNames[i].Length == 0))
                {
                    list3.Add(this.argumentValues[i]);
                }
                else
                {
                    list.Add(this.argumentNames[i]);
                    list2.Add(this.argumentValues[i]);
                }
            }
            Attribute attribute = (Attribute) Activator.CreateInstance(this.attributeType, list3.ToArray());
            for (int j = 0; j < list.Count; j++)
            {
                this.attributeType.GetProperty(list[j]).SetValue(attribute, list2[j], null);
            }
            return attribute;
        }

        public object GetArgumentValueAs(IServiceProvider serviceProvider, int argumentIndex, Type requestedType)
        {
            if ((argumentIndex >= this.ArgumentValues.Count) || (argumentIndex < 0))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentIndex"), "argumentIndex");
            }
            if (requestedType == null)
            {
                throw new ArgumentNullException("requestedType");
            }
            SupportedLanguages supportedLanguage = CompilerHelpers.GetSupportedLanguage(serviceProvider);
            if (requestedType == typeof(string))
            {
                string str = this.ArgumentValues[argumentIndex] as string;
                if (str != null)
                {
                    try
                    {
                        str = Regex.Unescape(str);
                    }
                    catch
                    {
                    }
                }
                if (str != null)
                {
                    if (str.EndsWith("\"", StringComparison.Ordinal))
                    {
                        str = str.Substring(0, str.Length - 1);
                    }
                    if ((supportedLanguage == SupportedLanguages.CSharp) && str.StartsWith("@\"", StringComparison.Ordinal))
                    {
                        return str.Substring(2, str.Length - 2);
                    }
                    if (str.StartsWith("\"", StringComparison.Ordinal))
                    {
                        str = str.Substring(1, str.Length - 1);
                    }
                }
                return str;
            }
            if (requestedType.IsEnum)
            {
                string str2 = "";
                bool flag = true;
                foreach (string str3 in (this.ArgumentValues[argumentIndex] as string).Split(new string[] { (supportedLanguage == SupportedLanguages.CSharp) ? "|" : "Or" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!flag)
                    {
                        str2 = str2 + ",";
                    }
                    int num = str3.LastIndexOf('.');
                    if (num != -1)
                    {
                        str2 = str2 + str3.Substring(num + 1);
                    }
                    else
                    {
                        str2 = str2 + str3;
                    }
                    flag = false;
                }
                return Enum.Parse(requestedType, str2);
            }
            if (requestedType == typeof(bool))
            {
                return Convert.ToBoolean(this.ArgumentValues[argumentIndex], CultureInfo.InvariantCulture);
            }
            if (!(requestedType == typeof(Type)))
            {
                return null;
            }
            string typeName = "";
            if (this.ArgumentValues[argumentIndex] is CodeTypeOfExpression)
            {
                typeName = DesignTimeType.GetTypeNameFromCodeTypeReference((this.ArgumentValues[argumentIndex] as CodeTypeOfExpression).Type, null);
            }
            ITypeProvider service = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).ToString() }));
            }
            Type type = ParseHelpers.ParseTypeName(service, supportedLanguage, typeName);
            if (type != null)
            {
                return type;
            }
            string[] parameters = null;
            string str5 = string.Empty;
            string elemantDecorator = string.Empty;
            if ((!ParseHelpers.ParseTypeName(typeName, (supportedLanguage == SupportedLanguages.CSharp) ? ParseHelpers.ParseTypeNameLanguage.CSharp : ParseHelpers.ParseTypeNameLanguage.VB, out str5, out parameters, out elemantDecorator) || (str5 == null)) || (parameters == null))
            {
                return type;
            }
            string str7 = str5 + "`" + parameters.Length.ToString(CultureInfo.InvariantCulture) + "[";
            foreach (string str8 in parameters)
            {
                if (str8 != parameters[0])
                {
                    str7 = str7 + ",";
                }
                Type type2 = ParseHelpers.ParseTypeName(service, supportedLanguage, str8);
                if (type2 != null)
                {
                    str7 = str7 + "[" + type2.FullName + "]";
                }
                else
                {
                    str7 = str7 + "[" + str8 + "]";
                }
            }
            str7 = str7 + "]";
            return ParseHelpers.ParseTypeName(service, supportedLanguage, str7);
        }

        public ReadOnlyCollection<object> ArgumentValues
        {
            get
            {
                List<object> list = new List<object>(this.argumentValues);
                return list.AsReadOnly();
            }
        }

        public Type AttributeType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.attributeType;
            }
        }

        public bool Creatable
        {
            get
            {
                if (this.attributeType.Assembly == null)
                {
                    return false;
                }
                foreach (object obj2 in this.argumentValues)
                {
                    if (obj2 is Exception)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}

