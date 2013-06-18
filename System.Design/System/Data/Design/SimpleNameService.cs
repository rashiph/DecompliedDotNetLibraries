namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Design;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal class SimpleNameService : INameService
    {
        private bool caseSensitive = true;
        internal const int DEFAULT_MAX_TRIALS = 0x186a0;
        private static SimpleNameService defaultInstance;
        private const int MAX_LENGTH = 0x400;
        private int maxNumberOfTrials = 0x186a0;
        private static readonly string regexAlphaCharacter = @"[\p{L}\p{Nl}]";
        private static readonly string regexIdentifier = (regexIdentifierStart + regexIdentifierCharacter + "*");
        private static readonly string regexIdentifierCharacter = @"[\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Cf}]";
        private static readonly string regexIdentifierStart = ("(" + regexAlphaCharacter + "|(" + regexUnderscoreCharacter + regexIdentifierCharacter + "))");
        private static readonly string regexUnderscoreCharacter = @"\p{Pc}";

        internal SimpleNameService()
        {
        }

        public string CreateUniqueName(INamedObjectCollection container, string proposed)
        {
            if (!this.NameExist(container, proposed))
            {
                this.ValidateName(proposed);
                return proposed;
            }
            return this.CreateUniqueName(container, proposed, 1);
        }

        public string CreateUniqueName(INamedObjectCollection container, Type type)
        {
            return this.CreateUniqueName(container, type.Name, 1);
        }

        public string CreateUniqueName(INamedObjectCollection container, string proposedNameRoot, int startSuffix)
        {
            return this.CreateUniqueNameOnCollection(container, proposedNameRoot, startSuffix);
        }

        public string CreateUniqueNameOnCollection(ICollection container, string proposedNameRoot, int startSuffix)
        {
            int num = startSuffix;
            if (num < 0)
            {
                num = 0;
            }
            this.ValidateName(proposedNameRoot);
            string nameTobeChecked = proposedNameRoot + num.ToString(CultureInfo.CurrentCulture);
            while (this.NameExist(container, nameTobeChecked))
            {
                num++;
                if (num >= this.maxNumberOfTrials)
                {
                    throw new InternalException("Failed to create unique name after many attempts", 1, true);
                }
                nameTobeChecked = proposedNameRoot + num.ToString(CultureInfo.CurrentCulture);
            }
            this.ValidateName(nameTobeChecked);
            return nameTobeChecked;
        }

        private bool NameExist(ICollection container, string nameTobeChecked)
        {
            return this.NameExist(container, null, nameTobeChecked);
        }

        private bool NameExist(ICollection container, INamedObject objTobeChecked, string nameTobeChecked)
        {
            if (StringUtil.Empty(nameTobeChecked) && (objTobeChecked != null))
            {
                nameTobeChecked = objTobeChecked.Name;
            }
            foreach (INamedObject obj2 in container)
            {
                if ((obj2 != objTobeChecked) && StringUtil.EqualValue(obj2.Name, nameTobeChecked, !this.caseSensitive))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void ValidateName(string name)
        {
            if (StringUtil.EmptyOrSpace(name))
            {
                throw new NameValidationException(System.Design.SR.GetString("CM_NameNotEmptyExcption"));
            }
            if (name.Length > 0x400)
            {
                throw new NameValidationException(System.Design.SR.GetString("CM_NameTooLongExcption"));
            }
            if (!Regex.Match(name, regexIdentifier).Success)
            {
                throw new NameValidationException(System.Design.SR.GetString("CM_NameInvalid", new object[] { name }));
            }
        }

        public void ValidateUniqueName(INamedObjectCollection container, string proposedName)
        {
            this.ValidateUniqueName(container, null, proposedName);
        }

        public void ValidateUniqueName(INamedObjectCollection container, INamedObject namedObject, string proposedName)
        {
            this.ValidateName(proposedName);
            if (this.NameExist(container, namedObject, proposedName))
            {
                throw new NameValidationException(System.Design.SR.GetString("CM_NameExist", new object[] { proposedName }));
            }
        }

        internal static SimpleNameService DefaultInstance
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new SimpleNameService();
                }
                return defaultInstance;
            }
        }
    }
}

