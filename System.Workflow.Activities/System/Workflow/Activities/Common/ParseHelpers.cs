namespace System.Workflow.Activities.Common
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Compiler;

    internal static class ParseHelpers
    {
        private static readonly ArrayList CSKeywords = new ArrayList(new string[] { 
            "int", "string", "bool", "object", "void", "float", "double", "char", "Date", "long", "byte", "short", "Single", "double", "decimal", "uint", 
            "ulong", "sbyte", "ushort"
         });
        private const string CultureTag = "culture";
        private static readonly string[] DotNetKeywords = new string[] { 
            "System.Int32", "System.String", "System.Boolean", "System.Object", "System.Void", "System.Single", "System.Double", "System.Char", "System.DateTime", "System.Int64", "System.Byte", "System.Int16", "System.Single", "System.Double", "System.Decimal", "System.UInt32", 
            "System.UInt64", "System.SByte", "System.UInt16"
         };
        private static readonly Version emptyVersion = new Version(0, 0, 0, 0);
        private const string PublicKeyTokenTag = "publickeytoken";
        private static readonly ArrayList VBKeywords = new ArrayList(new string[] { 
            "Integer", "String", "Boolean", "Object", "Void", "Single", "Double", "Char", "DateTime", "Long", "Byte", "Short", "Single", "Double", "Decimal", "UInteger", 
            "ULong", "SByte", "UShort"
         });
        private const string VersionTag = "version";

        internal static bool AssemblyNameEquals(AssemblyName thisName, AssemblyName thatName)
        {
            if ((thisName.Name == null) || (thatName.Name == null))
            {
                return false;
            }
            if (!thatName.Name.Equals(thisName.Name))
            {
                return false;
            }
            Version version = thatName.Version;
            if (((version != null) && (version != emptyVersion)) && (version != thisName.Version))
            {
                return false;
            }
            CultureInfo cultureInfo = thatName.CultureInfo;
            if ((cultureInfo != null) && !cultureInfo.Equals(CultureInfo.InvariantCulture))
            {
                CultureInfo parent = thisName.CultureInfo;
                if (parent == null)
                {
                    return false;
                }
                while (!cultureInfo.Equals(parent))
                {
                    parent = parent.Parent;
                    if (parent.Equals(CultureInfo.InvariantCulture))
                    {
                        return false;
                    }
                }
            }
            byte[] publicKeyToken = thatName.GetPublicKeyToken();
            if ((publicKeyToken != null) && (publicKeyToken.Length != 0))
            {
                byte[] buffer2 = thisName.GetPublicKeyToken();
                if (buffer2 == null)
                {
                    return false;
                }
                if (publicKeyToken.Length != buffer2.Length)
                {
                    return false;
                }
                for (int i = 0; i < publicKeyToken.Length; i++)
                {
                    if (publicKeyToken[i] != buffer2[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool AssemblyNameEquals(AssemblyName thisName, string thatName)
        {
            if ((thisName == null) || string.IsNullOrEmpty(thisName.Name))
            {
                return false;
            }
            if (string.IsNullOrEmpty(thatName))
            {
                return false;
            }
            string[] strArray = thatName.Split(new char[] { ',' });
            if (strArray.Length == 0)
            {
                return false;
            }
            if (!strArray[0].Trim().Equals(thisName.Name))
            {
                return false;
            }
            if (strArray.Length != 1)
            {
                Version version = null;
                CultureInfo info = null;
                byte[] buffer = null;
                for (int i = 1; i < strArray.Length; i++)
                {
                    string str3;
                    int index = strArray[i].IndexOf('=');
                    if (index != -1)
                    {
                        string str4;
                        string str2 = strArray[i].Substring(0, index).Trim().ToLowerInvariant();
                        str3 = strArray[i].Substring(index + 1).Trim().ToLowerInvariant();
                        if (!string.IsNullOrEmpty(str3) && ((str4 = str2) != null))
                        {
                            if (!(str4 == "version"))
                            {
                                if (str4 == "culture")
                                {
                                    goto Label_00FC;
                                }
                                if (str4 == "publickeytoken")
                                {
                                    goto Label_0115;
                                }
                            }
                            else
                            {
                                version = new Version(str3);
                            }
                        }
                    }
                    continue;
                Label_00FC:
                    if (!string.Equals(str3, "neutral", StringComparison.OrdinalIgnoreCase))
                    {
                        info = new CultureInfo(str3);
                    }
                    continue;
                Label_0115:
                    if (!string.Equals(str3, "null", StringComparison.OrdinalIgnoreCase))
                    {
                        buffer = new byte[str3.Length / 2];
                        for (int j = 0; j < buffer.Length; j++)
                        {
                            buffer[j] = byte.Parse(str3.Substring(j * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                    }
                }
                if (((version != null) && (version != emptyVersion)) && (version != thisName.Version))
                {
                    return false;
                }
                if ((info != null) && !info.Equals(CultureInfo.InvariantCulture))
                {
                    CultureInfo cultureInfo = thisName.CultureInfo;
                    if (cultureInfo == null)
                    {
                        return false;
                    }
                    while (!info.Equals(cultureInfo))
                    {
                        cultureInfo = cultureInfo.Parent;
                        if (cultureInfo.Equals(CultureInfo.InvariantCulture))
                        {
                            return false;
                        }
                    }
                }
                if ((buffer != null) && (buffer.Length != 0))
                {
                    byte[] publicKeyToken = thisName.GetPublicKeyToken();
                    if (publicKeyToken == null)
                    {
                        return false;
                    }
                    if (buffer.Length != publicKeyToken.Length)
                    {
                        return false;
                    }
                    for (int k = 0; k < buffer.Length; k++)
                    {
                        if (buffer[k] != publicKeyToken[k])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal static string FormatType(string type, System.Workflow.Activities.Common.SupportedLanguages language)
        {
            string str = string.Empty;
            string[] parameters = null;
            string typeName = string.Empty;
            string elemantDecorator = string.Empty;
            if (ParseTypeName(type, ParseTypeNameLanguage.NetFramework, out typeName, out parameters, out elemantDecorator))
            {
                if (elemantDecorator.Length > 0)
                {
                    if (language == System.Workflow.Activities.Common.SupportedLanguages.VB)
                    {
                        elemantDecorator = elemantDecorator.Replace('[', '(').Replace(']', ')');
                    }
                    return (FormatType(typeName, language) + elemantDecorator);
                }
                if ((parameters != null) && (parameters.Length > 0))
                {
                    str = FormatType(typeName, language);
                    if (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp)
                    {
                        str = str + '<';
                    }
                    else
                    {
                        str = str + '(';
                    }
                    bool flag = true;
                    foreach (string str4 in parameters)
                    {
                        if (!flag)
                        {
                            str = str + ", ";
                        }
                        else
                        {
                            if (language == System.Workflow.Activities.Common.SupportedLanguages.VB)
                            {
                                str = str + "Of ";
                            }
                            flag = false;
                        }
                        str = str + FormatType(str4, language);
                    }
                    if (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp)
                    {
                        return (str + '>');
                    }
                    return (str + ')');
                }
                str = typeName.Replace('+', '.');
                int index = str.IndexOf('`');
                if (index != -1)
                {
                    str = str.Substring(0, index);
                }
                index = str.IndexOf(',');
                if (index != -1)
                {
                    str = str.Substring(0, index);
                }
            }
            return str;
        }

        internal static string FormatType(Type type, System.Workflow.Activities.Common.SupportedLanguages language)
        {
            string fullName = string.Empty;
            if (type.IsArray)
            {
                fullName = FormatType(type.GetElementType(), language);
                if (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp)
                {
                    fullName = fullName + '[';
                }
                else
                {
                    fullName = fullName + '(';
                }
                fullName = fullName + new string(',', type.GetArrayRank() - 1);
                if (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp)
                {
                    return (fullName + ']');
                }
                return (fullName + ')');
            }
            fullName = type.FullName;
            int index = fullName.IndexOf('`');
            if (index != -1)
            {
                fullName = fullName.Substring(0, index);
            }
            fullName = fullName.Replace('+', '.');
            if (!type.ContainsGenericParameters && !type.IsGenericType)
            {
                return fullName;
            }
            Type[] genericArguments = type.GetGenericArguments();
            if (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp)
            {
                fullName = fullName + '<';
            }
            else
            {
                fullName = fullName + '(';
            }
            bool flag = true;
            foreach (Type type2 in genericArguments)
            {
                if (!flag)
                {
                    fullName = fullName + ", ";
                }
                else
                {
                    if (language == System.Workflow.Activities.Common.SupportedLanguages.VB)
                    {
                        fullName = fullName + "Of ";
                    }
                    flag = false;
                }
                fullName = fullName + FormatType(type2, language);
            }
            if (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp)
            {
                return (fullName + '>');
            }
            return (fullName + ')');
        }

        internal static Type ParseTypeName(ITypeProvider typeProvider, System.Workflow.Activities.Common.SupportedLanguages language, string typeName)
        {
            Type type = null;
            type = typeProvider.GetType(typeName, false);
            if (type == null)
            {
                string str = string.Empty;
                string elemantDecorator = string.Empty;
                string[] parameters = null;
                if (ParseTypeName(typeName, (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp) ? ParseTypeNameLanguage.CSharp : ParseTypeNameLanguage.VB, out str, out parameters, out elemantDecorator))
                {
                    type = typeProvider.GetType(str + elemantDecorator, false);
                }
            }
            return type;
        }

        internal static bool ParseTypeName(string inputTypeName, ParseTypeNameLanguage parseTypeNameLanguage, out string typeName, out string[] parameters, out string elemantDecorator)
        {
            typeName = string.Empty;
            parameters = null;
            elemantDecorator = string.Empty;
            if (parseTypeNameLanguage == ParseTypeNameLanguage.VB)
            {
                inputTypeName = inputTypeName.Replace('(', '[').Replace(')', ']');
            }
            else if (parseTypeNameLanguage == ParseTypeNameLanguage.CSharp)
            {
                inputTypeName = inputTypeName.Replace('<', '[').Replace('>', ']');
            }
            int length = inputTypeName.LastIndexOfAny(new char[] { ']', '&', '*' });
            if (length == -1)
            {
                typeName = inputTypeName;
            }
            else if (inputTypeName[length] == ']')
            {
                int num2 = length;
                int num3 = 1;
                while ((num2 > 0) && (num3 > 0))
                {
                    num2--;
                    if (inputTypeName[num2] == ']')
                    {
                        num3++;
                    }
                    else if (inputTypeName[num2] == '[')
                    {
                        num3--;
                    }
                }
                if (num3 != 0)
                {
                    return false;
                }
                typeName = inputTypeName.Substring(0, num2) + inputTypeName.Substring(length + 1);
                string str = inputTypeName.Substring(num2 + 1, (length - num2) - 1).Trim();
                if ((str == string.Empty) || (str.TrimStart(new char[0])[0] == ','))
                {
                    elemantDecorator = "[" + str + "]";
                }
                else
                {
                    int num4 = 0;
                    char[] chArray = str.ToCharArray();
                    for (int i = 0; i < chArray.Length; i++)
                    {
                        if (chArray[i] == '[')
                        {
                            num4++;
                        }
                        else if (chArray[i] == ']')
                        {
                            num4--;
                        }
                        else if ((chArray[i] == ',') && (num4 == 0))
                        {
                            chArray[i] = '$';
                        }
                    }
                    parameters = new string(chArray).Split(new char[] { '$' });
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        parameters[j] = parameters[j].Trim();
                        if (parameters[j][0] == '[')
                        {
                            parameters[j] = parameters[j].Substring(1, parameters[j].Length - 2);
                        }
                        if ((parseTypeNameLanguage == ParseTypeNameLanguage.VB) && parameters[j].StartsWith("Of ", StringComparison.OrdinalIgnoreCase))
                        {
                            parameters[j] = parameters[j].Substring(3).TrimStart(new char[0]);
                        }
                    }
                }
            }
            else
            {
                typeName = inputTypeName.Substring(0, length) + inputTypeName.Substring(length + 1);
                elemantDecorator = inputTypeName.Substring(length, 1);
            }
            if ((parseTypeNameLanguage == ParseTypeNameLanguage.CSharp) && CSKeywords.Contains(typeName))
            {
                typeName = DotNetKeywords[CSKeywords.IndexOf(typeName)];
            }
            else if ((parseTypeNameLanguage == ParseTypeNameLanguage.VB) && VBKeywords.Contains(typeName))
            {
                typeName = DotNetKeywords[VBKeywords.IndexOf(typeName)];
            }
            return true;
        }

        internal enum ParseTypeNameLanguage
        {
            VB,
            CSharp,
            NetFramework
        }
    }
}

