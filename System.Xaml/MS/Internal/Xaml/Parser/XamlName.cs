namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal abstract class XamlName
    {
        protected string _namespace;
        protected string _prefix;
        public const char Dot = '.';
        public const char PlusSign = '+';
        public const char UnderScore = '_';

        protected XamlName() : this(string.Empty)
        {
        }

        public XamlName(string name)
        {
            this.Name = name;
        }

        public XamlName(string prefix, string name)
        {
            this.Name = name;
            this._prefix = prefix ?? string.Empty;
        }

        public static bool ContainsDot(string name)
        {
            return name.Contains(".");
        }

        public static bool IsValidNameChar(char ch)
        {
            if (!IsValidNameStartChar(ch) && !char.IsDigit(ch))
            {
                UnicodeCategory unicodeCategory = char.GetUnicodeCategory(ch);
                if ((unicodeCategory != UnicodeCategory.NonSpacingMark) && (unicodeCategory != UnicodeCategory.SpacingCombiningMark))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidNameStartChar(char ch)
        {
            if (!char.IsLetter(ch))
            {
                return (ch == '_');
            }
            return true;
        }

        public static bool IsValidQualifiedNameChar(char ch)
        {
            if (ch != '.')
            {
                return IsValidNameChar(ch);
            }
            return true;
        }

        public static bool IsValidQualifiedNameCharPlus(char ch)
        {
            if (!IsValidQualifiedNameChar(ch))
            {
                return (ch == '+');
            }
            return true;
        }

        public static bool IsValidXamlName(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            if (!IsValidNameStartChar(name[0]))
            {
                return false;
            }
            for (int i = 1; i < name.Length; i++)
            {
                if (!IsValidNameChar(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public string Name { get; protected set; }

        public string Namespace
        {
            get
            {
                return this._namespace;
            }
        }

        public string Prefix
        {
            get
            {
                return this._prefix;
            }
        }

        public abstract string ScopedName { get; }
    }
}

