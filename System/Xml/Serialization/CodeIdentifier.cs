namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    public class CodeIdentifier
    {
        internal static CodeDomProvider csharp = new CSharpCodeProvider();
        internal const int MaxIdentifierLength = 0x1ff;

        internal static void CheckValidIdentifier(string ident)
        {
            if (!CodeGenerator.IsValidLanguageIndependentIdentifier(ident))
            {
                throw new ArgumentException(Res.GetString("XmlInvalidIdentifier", new object[] { ident }), "ident");
            }
        }

        private static string EscapeKeywords(string identifier, CodeDomProvider codeProvider)
        {
            if ((identifier == null) || (identifier.Length == 0))
            {
                return identifier;
            }
            string str = identifier;
            string[] strArray = identifier.Split(new char[] { '.', ',', '<', '>' });
            StringBuilder sb = new StringBuilder();
            int startIndex = -1;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (startIndex >= 0)
                {
                    sb.Append(str.Substring(startIndex, 1));
                }
                startIndex++;
                startIndex += strArray[i].Length;
                EscapeKeywords(strArray[i].Trim(), codeProvider, sb);
            }
            if (sb.Length != str.Length)
            {
                return sb.ToString();
            }
            return str;
        }

        private static void EscapeKeywords(string identifier, CodeDomProvider codeProvider, StringBuilder sb)
        {
            if ((identifier != null) && (identifier.Length != 0))
            {
                int num = 0;
                while (identifier.EndsWith("[]", StringComparison.Ordinal))
                {
                    num++;
                    identifier = identifier.Substring(0, identifier.Length - 2);
                }
                if (identifier.Length > 0)
                {
                    CheckValidIdentifier(identifier);
                    identifier = codeProvider.CreateEscapedIdentifier(identifier);
                    sb.Append(identifier);
                }
                for (int i = 0; i < num; i++)
                {
                    sb.Append("[]");
                }
            }
        }

        internal static string GetCSharpName(string name)
        {
            return EscapeKeywords(name.Replace('+', '.'), csharp);
        }

        internal static string GetCSharpName(Type t)
        {
            int num = 0;
            while (t.IsArray)
            {
                t = t.GetElementType();
                num++;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("global::");
            string str = t.Namespace;
            if ((str != null) && (str.Length > 0))
            {
                string[] strArray = str.Split(new char[] { '.' });
                for (int j = 0; j < strArray.Length; j++)
                {
                    EscapeKeywords(strArray[j], csharp, sb);
                    sb.Append(".");
                }
            }
            Type[] parameters = (t.IsGenericType || t.ContainsGenericParameters) ? t.GetGenericArguments() : new Type[0];
            GetCSharpName(t, parameters, 0, sb);
            for (int i = 0; i < num; i++)
            {
                sb.Append("[]");
            }
            return sb.ToString();
        }

        private static int GetCSharpName(Type t, Type[] parameters, int index, StringBuilder sb)
        {
            if ((t.DeclaringType != null) && (t.DeclaringType != t))
            {
                index = GetCSharpName(t.DeclaringType, parameters, index, sb);
                sb.Append(".");
            }
            string name = t.Name;
            int length = name.IndexOf('`');
            if (length < 0)
            {
                length = name.IndexOf('!');
            }
            if (length > 0)
            {
                EscapeKeywords(name.Substring(0, length), csharp, sb);
                sb.Append("<");
                int num2 = int.Parse(name.Substring(length + 1), CultureInfo.InvariantCulture) + index;
                while (index < num2)
                {
                    sb.Append(GetCSharpName(parameters[index]));
                    if (index < (num2 - 1))
                    {
                        sb.Append(",");
                    }
                    index++;
                }
                sb.Append(">");
                return index;
            }
            EscapeKeywords(name, csharp, sb);
            return index;
        }

        private static bool IsValid(char c)
        {
            switch (char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                    return true;

                case UnicodeCategory.EnclosingMark:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.OtherNumber:
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.ParagraphSeparator:
                case UnicodeCategory.Control:
                case UnicodeCategory.Format:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.DashPunctuation:
                case UnicodeCategory.OpenPunctuation:
                case UnicodeCategory.ClosePunctuation:
                case UnicodeCategory.InitialQuotePunctuation:
                case UnicodeCategory.FinalQuotePunctuation:
                case UnicodeCategory.OtherPunctuation:
                case UnicodeCategory.MathSymbol:
                case UnicodeCategory.CurrencySymbol:
                case UnicodeCategory.ModifierSymbol:
                case UnicodeCategory.OtherSymbol:
                case UnicodeCategory.OtherNotAssigned:
                    return false;
            }
            return false;
        }

        private static bool IsValidStart(char c)
        {
            if (char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber)
            {
                return false;
            }
            return true;
        }

        public static string MakeCamel(string identifier)
        {
            identifier = MakeValid(identifier);
            if (identifier.Length <= 2)
            {
                return identifier.ToLower(CultureInfo.InvariantCulture);
            }
            if (char.IsUpper(identifier[0]))
            {
                return (char.ToLower(identifier[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) + identifier.Substring(1));
            }
            return identifier;
        }

        public static string MakePascal(string identifier)
        {
            identifier = MakeValid(identifier);
            if (identifier.Length <= 2)
            {
                return identifier.ToUpper(CultureInfo.InvariantCulture);
            }
            if (char.IsLower(identifier[0]))
            {
                return (char.ToUpper(identifier[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) + identifier.Substring(1));
            }
            return identifier;
        }

        public static string MakeValid(string identifier)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; (i < identifier.Length) && (builder.Length < 0x1ff); i++)
            {
                char c = identifier[i];
                if (IsValid(c))
                {
                    if ((builder.Length == 0) && !IsValidStart(c))
                    {
                        builder.Append("Item");
                    }
                    builder.Append(c);
                }
            }
            if (builder.Length == 0)
            {
                return "Item";
            }
            return builder.ToString();
        }

        internal static string MakeValidInternal(string identifier)
        {
            if (identifier.Length > 30)
            {
                return "Item";
            }
            return MakeValid(identifier);
        }
    }
}

