namespace System.Security.Util
{
    using System;
    using System.Collections;
    using System.Globalization;

    [Serializable]
    internal class SiteString
    {
        protected ArrayList m_separatedSite;
        protected static char[] m_separators = new char[] { '.' };
        protected string m_site;

        protected internal SiteString()
        {
        }

        public SiteString(string site)
        {
            this.m_separatedSite = CreateSeparatedSite(site);
            this.m_site = site;
        }

        private SiteString(string site, ArrayList separatedSite)
        {
            this.m_separatedSite = separatedSite;
            this.m_site = site;
        }

        private static bool AllLegalCharacters(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (!IsLegalDNSChar(c) && !IsNetbiosSplChar(c))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual SiteString Copy()
        {
            return new SiteString(this.m_site, this.m_separatedSite);
        }

        private static ArrayList CreateSeparatedSite(string site)
        {
            if ((site == null) || (site.Length == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSite"));
            }
            ArrayList list = new ArrayList();
            int index = -1;
            int num2 = -1;
            index = site.IndexOf('[');
            if (index == 0)
            {
                num2 = site.IndexOf(']', index + 1);
            }
            if (num2 != -1)
            {
                string str = site.Substring(index + 1, (num2 - index) - 1);
                list.Add(str);
                return list;
            }
            string[] strArray = site.Split(m_separators);
            for (int i = strArray.Length - 1; i > -1; i--)
            {
                if (strArray[i] == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSite"));
                }
                if (strArray[i].Equals(""))
                {
                    if (i != (strArray.Length - 1))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSite"));
                    }
                }
                else if (strArray[i].Equals("*"))
                {
                    if (i != 0)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSite"));
                    }
                    list.Add(strArray[i]);
                }
                else
                {
                    if (!AllLegalCharacters(strArray[i]))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSite"));
                    }
                    list.Add(strArray[i]);
                }
            }
            return list;
        }

        public override bool Equals(object o)
        {
            return (((o != null) && (o is SiteString)) && this.Equals((SiteString) o, true));
        }

        internal bool Equals(SiteString ss, bool ignoreCase)
        {
            if (this.m_site == null)
            {
                return (ss.m_site == null);
            }
            if (ss.m_site == null)
            {
                return false;
            }
            return (this.IsSubsetOf(ss, ignoreCase) && ss.IsSubsetOf(this, ignoreCase));
        }

        public override int GetHashCode()
        {
            return CultureInfo.InvariantCulture.TextInfo.GetCaseInsensitiveHashCode(this.m_site);
        }

        public virtual SiteString Intersect(SiteString operand)
        {
            if (operand != null)
            {
                if (this.IsSubsetOf(operand))
                {
                    return this.Copy();
                }
                if (operand.IsSubsetOf(this))
                {
                    return operand.Copy();
                }
            }
            return null;
        }

        private static bool IsLegalDNSChar(char c)
        {
            if ((((c < 'a') || (c > 'z')) && ((c < 'A') || (c > 'Z'))) && (((c < '0') || (c > '9')) && (c != '-')))
            {
                return false;
            }
            return true;
        }

        private static bool IsNetbiosSplChar(char c)
        {
            switch (c)
            {
                case '!':
                case '#':
                case '$':
                case '%':
                case '&':
                case '\'':
                case '(':
                case ')':
                case '-':
                case '.':
                case '@':
                case '^':
                case '_':
                case '{':
                case '}':
                case '~':
                    return true;
            }
            return false;
        }

        public virtual bool IsSubsetOf(SiteString operand)
        {
            return this.IsSubsetOf(operand, true);
        }

        public virtual bool IsSubsetOf(SiteString operand, bool ignoreCase)
        {
            StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (operand == null)
            {
                return false;
            }
            if ((this.m_separatedSite.Count != operand.m_separatedSite.Count) || (this.m_separatedSite.Count != 0))
            {
                if (this.m_separatedSite.Count < (operand.m_separatedSite.Count - 1))
                {
                    return false;
                }
                if (((this.m_separatedSite.Count > operand.m_separatedSite.Count) && (operand.m_separatedSite.Count > 0)) && !operand.m_separatedSite[operand.m_separatedSite.Count - 1].Equals("*"))
                {
                    return false;
                }
                if (string.Compare(this.m_site, operand.m_site, comparisonType) == 0)
                {
                    return true;
                }
                for (int i = 0; i < (operand.m_separatedSite.Count - 1); i++)
                {
                    if (string.Compare((string) this.m_separatedSite[i], (string) operand.m_separatedSite[i], comparisonType) != 0)
                    {
                        return false;
                    }
                }
                if (this.m_separatedSite.Count < operand.m_separatedSite.Count)
                {
                    return operand.m_separatedSite[operand.m_separatedSite.Count - 1].Equals("*");
                }
                if (this.m_separatedSite.Count != operand.m_separatedSite.Count)
                {
                    return true;
                }
                if (string.Compare((string) this.m_separatedSite[this.m_separatedSite.Count - 1], (string) operand.m_separatedSite[this.m_separatedSite.Count - 1], comparisonType) != 0)
                {
                    return operand.m_separatedSite[operand.m_separatedSite.Count - 1].Equals("*");
                }
            }
            return true;
        }

        public override string ToString()
        {
            return this.m_site;
        }

        public virtual SiteString Union(SiteString operand)
        {
            if (operand == null)
            {
                return this;
            }
            if (this.IsSubsetOf(operand))
            {
                return operand.Copy();
            }
            if (operand.IsSubsetOf(this))
            {
                return this.Copy();
            }
            return null;
        }
    }
}

