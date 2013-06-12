namespace System.Security.Util
{
    using System;
    using System.Collections;

    [Serializable]
    internal class LocalSiteString : SiteString
    {
        private static char[] m_separators = new char[] { '/' };

        public LocalSiteString(string site)
        {
            base.m_site = site.Replace('|', ':');
            if ((base.m_site.Length > 2) && (base.m_site.IndexOf(':') != -1))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDirectoryOnUrl"));
            }
            base.m_separatedSite = this.CreateSeparatedString(base.m_site);
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
                if ((strArray[i] == null) || strArray[i].Equals(""))
                {
                    if ((i >= 2) || (directory[i] != '/'))
                    {
                        if (i != (strArray.Length - 1))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDirectoryOnUrl"));
                        }
                    }
                    else
                    {
                        list.Add("//");
                    }
                }
                else if (strArray[i].Equals("*"))
                {
                    if (i != (strArray.Length - 1))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDirectoryOnUrl"));
                    }
                    list.Add(strArray[i]);
                }
                else
                {
                    list.Add(strArray[i]);
                }
            }
            return list;
        }

        public virtual bool IsSubsetOf(LocalSiteString operand)
        {
            return this.IsSubsetOf(operand, true);
        }

        public virtual bool IsSubsetOf(LocalSiteString operand, bool ignoreCase)
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

