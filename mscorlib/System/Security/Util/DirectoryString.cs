namespace System.Security.Util
{
    using System;
    using System.Collections;

    [Serializable]
    internal class DirectoryString : SiteString
    {
        private bool m_checkForIllegalChars;
        protected static char[] m_illegalDirectoryCharacters = new char[] { '\\', ':', '*', '?', '"', '<', '>', '|' };
        private static char[] m_separators = new char[] { '/' };

        public DirectoryString()
        {
            base.m_site = "";
            base.m_separatedSite = new ArrayList();
        }

        public DirectoryString(string directory, bool checkForIllegalChars)
        {
            base.m_site = directory;
            this.m_checkForIllegalChars = checkForIllegalChars;
            base.m_separatedSite = this.CreateSeparatedString(directory);
        }

        private ArrayList CreateSeparatedString(string directory)
        {
            if ((directory == null) || (directory.Length == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDirectoryOnUrl"));
            }
            ArrayList list = new ArrayList();
            string[] strArray = directory.Split(m_separators);
            for (int i = 0; i < strArray.Length; i++)
            {
                if ((strArray[i] != null) && !strArray[i].Equals(""))
                {
                    if (strArray[i].Equals("*"))
                    {
                        if (i != (strArray.Length - 1))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDirectoryOnUrl"));
                        }
                        list.Add(strArray[i]);
                    }
                    else
                    {
                        if (this.m_checkForIllegalChars && (strArray[i].IndexOfAny(m_illegalDirectoryCharacters) != -1))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDirectoryOnUrl"));
                        }
                        list.Add(strArray[i]);
                    }
                }
            }
            return list;
        }

        public virtual bool IsSubsetOf(DirectoryString operand)
        {
            return this.IsSubsetOf(operand, true);
        }

        public virtual bool IsSubsetOf(DirectoryString operand, bool ignoreCase)
        {
            if (operand == null)
            {
                return false;
            }
            if (operand.m_separatedSite.Count == 0)
            {
                return ((base.m_separatedSite.Count == 0) || ((base.m_separatedSite.Count > 0) && (string.Compare((string) base.m_separatedSite[0], "*", StringComparison.Ordinal) == 0)));
            }
            if (base.m_separatedSite.Count == 0)
            {
                return (string.Compare((string) operand.m_separatedSite[0], "*", StringComparison.Ordinal) == 0);
            }
            return base.IsSubsetOf(operand, ignoreCase);
        }
    }
}

