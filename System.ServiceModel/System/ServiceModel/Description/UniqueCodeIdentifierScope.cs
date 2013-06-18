namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    internal class UniqueCodeIdentifierScope
    {
        private const int MaxIdentifierLength = 0x1ff;
        private SortedList<string, string> names;

        protected virtual void AddIdentifier(string identifier)
        {
            if (this.names == null)
            {
                this.names = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            this.names.Add(identifier, identifier);
        }

        public void AddReserved(string identifier)
        {
            this.AddIdentifier(identifier);
        }

        public string AddUnique(string name, string defaultName)
        {
            string str = MakeValid(name, defaultName);
            string identifier = str;
            int num = 1;
            while (!this.IsUnique(identifier))
            {
                identifier = str + num++.ToString(CultureInfo.InvariantCulture);
            }
            this.AddIdentifier(identifier);
            return identifier;
        }

        public virtual bool IsUnique(string identifier)
        {
            if (this.names != null)
            {
                return !this.names.ContainsKey(identifier);
            }
            return true;
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
            }
            return false;
        }

        private static bool IsValidStart(char c)
        {
            return (char.GetUnicodeCategory(c) != UnicodeCategory.DecimalDigitNumber);
        }

        public static string MakeValid(string identifier, string defaultIdentifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return defaultIdentifier;
            }
            if ((identifier.Length <= 0x1ff) && CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
            {
                return identifier;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; (i < identifier.Length) && (builder.Length < 0x1ff); i++)
            {
                char c = identifier[i];
                if (IsValid(c))
                {
                    if ((builder.Length == 0) && !IsValidStart(c))
                    {
                        builder.Append('_');
                    }
                    builder.Append(c);
                }
            }
            if (builder.Length == 0)
            {
                return defaultIdentifier;
            }
            return builder.ToString();
        }
    }
}

