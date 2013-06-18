namespace System.Data.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;

    internal sealed class GenericNameHandler
    {
        private Hashtable names;
        private MemberNameValidator validator;

        internal GenericNameHandler(ICollection initialNameSet, CodeDomProvider codeProvider)
        {
            this.validator = new MemberNameValidator(initialNameSet, codeProvider, true);
            this.names = new Hashtable(StringComparer.Ordinal);
        }

        internal string AddNameToList(string originalName)
        {
            if (originalName == null)
            {
                throw new InternalException("Parameter originalName should not be null.");
            }
            string newMemberName = this.validator.GetNewMemberName(originalName);
            this.names.Add(originalName, newMemberName);
            return newMemberName;
        }

        internal string AddParameterNameToList(string originalName, string parameterPrefix)
        {
            if (originalName == null)
            {
                throw new ArgumentNullException("originalName");
            }
            string str = originalName;
            if (!StringUtil.Empty(parameterPrefix) && originalName.StartsWith(parameterPrefix, StringComparison.Ordinal))
            {
                str = originalName.Substring(parameterPrefix.Length);
            }
            string newMemberName = this.validator.GetNewMemberName(str);
            this.names.Add(originalName, newMemberName);
            return newMemberName;
        }

        internal string GetNameFromList(string originalName)
        {
            if (originalName == null)
            {
                throw new InternalException("Parameter originalName should not be null.");
            }
            return (string) this.names[originalName];
        }
    }
}

