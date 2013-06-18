namespace System.Data.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class MemberNameValidator
    {
        private const int additionalTruncationChars = 100;
        private ArrayList bookedMemberNames;
        private CodeDomProvider codeProvider;
        private static Dictionary<string, string[]> invalidEverettIdentifiers = null;
        private static string[] invalidEverettIdentifiersVb = new string[] { "region", "externalsource" };
        private bool languageCaseInsensitive;
        private const int maxGenerationAttempts = 200;
        private bool useSuffix;

        internal MemberNameValidator(ICollection initialNameSet, CodeDomProvider codeProvider, bool languageCaseInsensitive)
        {
            this.codeProvider = codeProvider;
            this.languageCaseInsensitive = languageCaseInsensitive;
            if (initialNameSet != null)
            {
                this.bookedMemberNames = new ArrayList(initialNameSet.Count);
                foreach (string str in initialNameSet)
                {
                    this.AddNameToList(str);
                }
            }
            else
            {
                this.bookedMemberNames = new ArrayList();
            }
        }

        private void AddNameToList(string name)
        {
            if (this.languageCaseInsensitive)
            {
                this.bookedMemberNames.Add(name.ToUpperInvariant());
            }
            else
            {
                this.bookedMemberNames.Add(name);
            }
        }

        internal string GenerateIdName(string name)
        {
            return GenerateIdName(name, this.codeProvider, this.UseSuffix);
        }

        internal static string GenerateIdName(string name, CodeDomProvider codeProvider, bool useSuffix)
        {
            return GenerateIdName(name, codeProvider, useSuffix, 100);
        }

        internal static string GenerateIdName(string name, CodeDomProvider codeProvider, bool useSuffix, int additionalCharsToTruncate)
        {
            if (!useSuffix)
            {
                name = GetBackwardCompatibleIdentifier(name, codeProvider);
            }
            if (codeProvider.IsValidIdentifier(name))
            {
                return name;
            }
            string str = name.Replace(' ', '_');
            if (!codeProvider.IsValidIdentifier(str))
            {
                if (!useSuffix)
                {
                    str = "_" + str;
                }
                for (int i = 0; i < str.Length; i++)
                {
                    UnicodeCategory unicodeCategory = char.GetUnicodeCategory(str[i]);
                    if (((((unicodeCategory != UnicodeCategory.UppercaseLetter) && (UnicodeCategory.LowercaseLetter != unicodeCategory)) && ((UnicodeCategory.TitlecaseLetter != unicodeCategory) && (UnicodeCategory.ModifierLetter != unicodeCategory))) && (((UnicodeCategory.OtherLetter != unicodeCategory) && (UnicodeCategory.NonSpacingMark != unicodeCategory)) && ((UnicodeCategory.SpacingCombiningMark != unicodeCategory) && (UnicodeCategory.DecimalDigitNumber != unicodeCategory)))) && (UnicodeCategory.ConnectorPunctuation != unicodeCategory))
                    {
                        str = str.Replace(str[i], '_');
                    }
                }
            }
            int num2 = 0;
            string str2 = str;
            while (!codeProvider.IsValidIdentifier(str) && (num2 < 200))
            {
                num2++;
                str = "_" + str;
            }
            if (num2 >= 200)
            {
                str = str2;
                while (!codeProvider.IsValidIdentifier(str) && (str.Length > 0))
                {
                    str = str.Remove(str.Length - 1);
                }
                if (str.Length == 0)
                {
                    return str2;
                }
                if (((additionalCharsToTruncate > 0) && (str.Length > additionalCharsToTruncate)) && codeProvider.IsValidIdentifier(str.Remove(str.Length - additionalCharsToTruncate)))
                {
                    str = str.Remove(str.Length - additionalCharsToTruncate);
                }
            }
            return str;
        }

        private static string GetBackwardCompatibleIdentifier(string identifier, CodeDomProvider provider)
        {
            string key = "." + provider.FileExtension;
            if (key.StartsWith("..", StringComparison.Ordinal))
            {
                key = key.Substring(1);
            }
            if (InvalidEverettIdentifiers.ContainsKey(key))
            {
                string[] strArray = InvalidEverettIdentifiers[key];
                if (strArray == null)
                {
                    return identifier;
                }
                bool caseInsensitive = (provider.LanguageOptions & LanguageOptions.CaseInsensitive) > LanguageOptions.None;
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (StringUtil.EqualValue(identifier, strArray[i], caseInsensitive))
                    {
                        return ("_" + identifier);
                    }
                }
            }
            return identifier;
        }

        internal string GetCandidateMemberName(string originalName)
        {
            if (originalName == null)
            {
                throw new InternalException("Member name cannot be null.");
            }
            string name = this.GenerateIdName(originalName);
            string str2 = name;
            int num = 0;
            while (this.ListContains(name))
            {
                num++;
                name = str2 + num.ToString(CultureInfo.CurrentCulture);
                if (!this.codeProvider.IsValidIdentifier(name))
                {
                    throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Unable to generate valid identifier from name: {0}.", new object[] { originalName }));
                }
                if (num > 200)
                {
                    throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Unable to generate unique identifier from name: {0}. Too many attempts.", new object[] { originalName }));
                }
            }
            return name;
        }

        internal string GetNewMemberName(string originalName)
        {
            string candidateMemberName = this.GetCandidateMemberName(originalName);
            this.AddNameToList(candidateMemberName);
            return candidateMemberName;
        }

        private bool ListContains(string name)
        {
            if (this.languageCaseInsensitive)
            {
                return this.bookedMemberNames.Contains(name.ToUpperInvariant());
            }
            return this.bookedMemberNames.Contains(name);
        }

        private static Dictionary<string, string[]> InvalidEverettIdentifiers
        {
            get
            {
                if (invalidEverettIdentifiers == null)
                {
                    invalidEverettIdentifiers = new Dictionary<string, string[]>();
                    invalidEverettIdentifiers.Add(".vb", invalidEverettIdentifiersVb);
                }
                return invalidEverettIdentifiers;
            }
        }

        internal bool UseSuffix
        {
            get
            {
                return this.useSuffix;
            }
            set
            {
                this.useSuffix = value;
            }
        }
    }
}

