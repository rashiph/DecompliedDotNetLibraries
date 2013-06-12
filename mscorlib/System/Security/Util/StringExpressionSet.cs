namespace System.Security.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [Serializable]
    internal class StringExpressionSet
    {
        protected static readonly char m_alternateDirectorySeparator = '/';
        protected static readonly char m_directorySeparator = '\\';
        protected string m_expressions;
        protected string[] m_expressionsArray;
        protected bool m_ignoreCase;
        protected ArrayList m_list;
        protected static readonly char[] m_separators = new char[] { ';' };
        protected bool m_throwOnRelative;
        protected static readonly char[] m_trimChars = new char[] { ' ' };

        public StringExpressionSet() : this(true, null, false)
        {
        }

        public StringExpressionSet(string str) : this(true, str, false)
        {
        }

        public StringExpressionSet(bool ignoreCase, bool throwOnRelative) : this(ignoreCase, null, throwOnRelative)
        {
        }

        [SecuritySafeCritical]
        public StringExpressionSet(bool ignoreCase, string str, bool throwOnRelative)
        {
            this.m_list = null;
            this.m_ignoreCase = ignoreCase;
            this.m_throwOnRelative = throwOnRelative;
            if (str == null)
            {
                this.m_expressions = null;
            }
            else
            {
                this.AddExpressions(str);
            }
        }

        [SecurityCritical]
        public void AddExpressions(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Length != 0)
            {
                str = this.ProcessWholeString(str);
                if (this.m_expressions == null)
                {
                    this.m_expressions = str;
                }
                else
                {
                    this.m_expressions = this.m_expressions + m_separators[0] + str;
                }
                this.m_expressionsArray = null;
                string[] strArray = this.Split(str);
                if (this.m_list == null)
                {
                    this.m_list = new ArrayList();
                }
                for (int i = 0; i < strArray.Length; i++)
                {
                    if ((strArray[i] != null) && !strArray[i].Equals(""))
                    {
                        string path = this.ProcessSingleString(strArray[i]);
                        int index = path.IndexOf('\0');
                        if (index != -1)
                        {
                            path = path.Substring(0, index);
                        }
                        if ((path != null) && !path.Equals(""))
                        {
                            if (this.m_throwOnRelative)
                            {
                                if (Path.IsRelative(path))
                                {
                                    throw new ArgumentException(Environment.GetResourceString("Argument_AbsolutePathRequired"));
                                }
                                path = CanonicalizePath(path);
                            }
                            this.m_list.Add(path);
                        }
                    }
                }
                this.Reduce();
            }
        }

        [SecurityCritical]
        public void AddExpressions(ArrayList exprArrayList, bool checkForDuplicates)
        {
            this.m_expressionsArray = null;
            this.m_expressions = null;
            if (this.m_list != null)
            {
                this.m_list.AddRange(exprArrayList);
            }
            else
            {
                this.m_list = new ArrayList(exprArrayList);
            }
            if (checkForDuplicates)
            {
                this.Reduce();
            }
        }

        [SecurityCritical]
        public void AddExpressions(string[] str, bool checkForDuplicates, bool needFullPath)
        {
            this.AddExpressions(CreateListFromExpressions(str, needFullPath), checkForDuplicates);
        }

        protected void AddSingleExpressionNoDuplicates(string expression)
        {
            int index = 0;
            this.m_expressionsArray = null;
            this.m_expressions = null;
            if (this.m_list == null)
            {
                this.m_list = new ArrayList();
            }
            while (index < this.m_list.Count)
            {
                if (this.StringSubsetString((string) this.m_list[index], expression, this.m_ignoreCase))
                {
                    this.m_list.RemoveAt(index);
                }
                else
                {
                    if (this.StringSubsetString(expression, (string) this.m_list[index], this.m_ignoreCase))
                    {
                        return;
                    }
                    index++;
                }
            }
            this.m_list.Add(expression);
        }

        [SecurityCritical]
        internal static string CanonicalizePath(string path)
        {
            return CanonicalizePath(path, true);
        }

        [SecurityCritical]
        internal static string CanonicalizePath(string path, bool needFullPath)
        {
            if (path.IndexOf('~') != -1)
            {
                string s = null;
                GetLongPathName(path, JitHelpers.GetStringHandleOnStack(ref s));
                path = (s != null) ? s : path;
            }
            if (path.IndexOf(':', 2) != -1)
            {
                throw new NotSupportedException(Environment.GetResourceString("Argument_PathFormatNotSupported"));
            }
            if (!needFullPath)
            {
                return path;
            }
            string fullPathInternal = Path.GetFullPathInternal(path);
            if (!path.EndsWith(m_directorySeparator + ".", StringComparison.Ordinal))
            {
                return fullPathInternal;
            }
            if (fullPathInternal.EndsWith(m_directorySeparator))
            {
                return (fullPathInternal + ".");
            }
            return (fullPathInternal + m_directorySeparator + ".");
        }

        [SecurityCritical]
        protected void CheckList()
        {
            if ((this.m_list == null) && (this.m_expressions != null))
            {
                this.CreateList();
            }
        }

        public virtual StringExpressionSet Copy()
        {
            StringExpressionSet set = this.CreateNewEmpty();
            if (this.m_list != null)
            {
                set.m_list = new ArrayList(this.m_list);
            }
            set.m_expressions = this.m_expressions;
            set.m_ignoreCase = this.m_ignoreCase;
            set.m_throwOnRelative = this.m_throwOnRelative;
            return set;
        }

        [SecurityCritical]
        protected void CreateList()
        {
            string[] strArray = this.Split(this.m_expressions);
            this.m_list = new ArrayList();
            for (int i = 0; i < strArray.Length; i++)
            {
                if ((strArray[i] != null) && !strArray[i].Equals(""))
                {
                    string path = this.ProcessSingleString(strArray[i]);
                    int index = path.IndexOf('\0');
                    if (index != -1)
                    {
                        path = path.Substring(0, index);
                    }
                    if ((path != null) && !path.Equals(""))
                    {
                        if (this.m_throwOnRelative)
                        {
                            if (Path.IsRelative(path))
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_AbsolutePathRequired"));
                            }
                            path = CanonicalizePath(path);
                        }
                        this.m_list.Add(path);
                    }
                }
            }
        }

        [SecurityCritical]
        internal static ArrayList CreateListFromExpressions(string[] str, bool needFullPath)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            ArrayList list = new ArrayList();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == null)
                {
                    throw new ArgumentNullException("str");
                }
                string str2 = StaticProcessWholeString(str[i]);
                if ((str2 != null) && (str2.Length != 0))
                {
                    string path = StaticProcessSingleString(str2);
                    int index = path.IndexOf('\0');
                    if (index != -1)
                    {
                        path = path.Substring(0, index);
                    }
                    if ((path != null) && (path.Length != 0))
                    {
                        if (Path.IsRelative(path))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_AbsolutePathRequired"));
                        }
                        path = CanonicalizePath(path, needFullPath);
                        list.Add(path);
                    }
                }
            }
            return list;
        }

        protected virtual StringExpressionSet CreateNewEmpty()
        {
            return new StringExpressionSet();
        }

        protected void GenerateString()
        {
            if (this.m_list != null)
            {
                StringBuilder builder = new StringBuilder();
                IEnumerator enumerator = this.m_list.GetEnumerator();
                bool flag = true;
                while (enumerator.MoveNext())
                {
                    if (!flag)
                    {
                        builder.Append(m_separators[0]);
                    }
                    else
                    {
                        flag = false;
                    }
                    string current = (string) enumerator.Current;
                    if (current != null)
                    {
                        int index = current.IndexOf(m_separators[0]);
                        if (index != -1)
                        {
                            builder.Append('"');
                        }
                        builder.Append(current);
                        if (index != -1)
                        {
                            builder.Append('"');
                        }
                    }
                }
                this.m_expressions = builder.ToString();
            }
            else
            {
                this.m_expressions = null;
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void GetLongPathName(string path, StringHandleOnStack retLongPath);
        [SecurityCritical]
        public StringExpressionSet Intersect(StringExpressionSet ses)
        {
            if ((this.IsEmpty() || (ses == null)) || ses.IsEmpty())
            {
                return this.CreateNewEmpty();
            }
            this.CheckList();
            ses.CheckList();
            StringExpressionSet set = this.CreateNewEmpty();
            for (int i = 0; i < this.m_list.Count; i++)
            {
                for (int j = 0; j < ses.m_list.Count; j++)
                {
                    if (this.StringSubsetString((string) this.m_list[i], (string) ses.m_list[j], this.m_ignoreCase))
                    {
                        if (set.m_list == null)
                        {
                            set.m_list = new ArrayList();
                        }
                        set.AddSingleExpressionNoDuplicates((string) this.m_list[i]);
                    }
                    else if (this.StringSubsetString((string) ses.m_list[j], (string) this.m_list[i], this.m_ignoreCase))
                    {
                        if (set.m_list == null)
                        {
                            set.m_list = new ArrayList();
                        }
                        set.AddSingleExpressionNoDuplicates((string) ses.m_list[j]);
                    }
                }
            }
            set.GenerateString();
            return set;
        }

        public bool IsEmpty()
        {
            if (this.m_list == null)
            {
                return (this.m_expressions == null);
            }
            return (this.m_list.Count == 0);
        }

        [SecurityCritical]
        public bool IsSubsetOf(StringExpressionSet ses)
        {
            if (!this.IsEmpty())
            {
                if ((ses == null) || ses.IsEmpty())
                {
                    return false;
                }
                this.CheckList();
                ses.CheckList();
                for (int i = 0; i < this.m_list.Count; i++)
                {
                    if (!this.StringSubsetStringExpression((string) this.m_list[i], ses, this.m_ignoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [SecurityCritical]
        public bool IsSubsetOfPathDiscovery(StringExpressionSet ses)
        {
            if (!this.IsEmpty())
            {
                if ((ses == null) || ses.IsEmpty())
                {
                    return false;
                }
                this.CheckList();
                ses.CheckList();
                for (int i = 0; i < this.m_list.Count; i++)
                {
                    if (!StringSubsetStringExpressionPathDiscovery((string) this.m_list[i], ses, this.m_ignoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected virtual string ProcessSingleString(string str)
        {
            return StaticProcessSingleString(str);
        }

        protected virtual string ProcessWholeString(string str)
        {
            return StaticProcessWholeString(str);
        }

        [SecurityCritical]
        protected void Reduce()
        {
            this.CheckList();
            if (this.m_list != null)
            {
                for (int i = 0; i < (this.m_list.Count - 1); i++)
                {
                    int index = i + 1;
                    while (index < this.m_list.Count)
                    {
                        if (this.StringSubsetString((string) this.m_list[index], (string) this.m_list[i], this.m_ignoreCase))
                        {
                            this.m_list.RemoveAt(index);
                        }
                        else
                        {
                            if (this.StringSubsetString((string) this.m_list[i], (string) this.m_list[index], this.m_ignoreCase))
                            {
                                this.m_list[i] = this.m_list[index];
                                this.m_list.RemoveAt(index);
                                index = i + 1;
                                continue;
                            }
                            index++;
                        }
                    }
                }
            }
        }

        public void SetThrowOnRelative(bool throwOnRelative)
        {
            this.m_throwOnRelative = throwOnRelative;
        }

        protected string[] Split(string expressions)
        {
            if (!this.m_throwOnRelative)
            {
                return expressions.Split(m_separators);
            }
            List<string> list = new List<string>();
            string[] strArray = expressions.Split(new char[] { '"' });
            for (int i = 0; i < strArray.Length; i++)
            {
                if ((i % 2) == 0)
                {
                    string[] strArray2 = strArray[i].Split(new char[] { ';' });
                    for (int j = 0; j < strArray2.Length; j++)
                    {
                        if ((strArray2[j] != null) && !strArray2[j].Equals(""))
                        {
                            list.Add(strArray2[j]);
                        }
                    }
                }
                else
                {
                    list.Add(strArray[i]);
                }
            }
            string[] strArray3 = new string[list.Count];
            IEnumerator enumerator = list.GetEnumerator();
            int num3 = 0;
            while (enumerator.MoveNext())
            {
                strArray3[num3++] = (string) enumerator.Current;
            }
            return strArray3;
        }

        private static string StaticProcessSingleString(string str)
        {
            return str.Trim(m_trimChars);
        }

        private static string StaticProcessWholeString(string str)
        {
            return str.Replace(m_alternateDirectorySeparator, m_directorySeparator);
        }

        protected virtual bool StringSubsetString(string left, string right, bool ignoreCase)
        {
            StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (((right == null) || (left == null)) || (((right.Length == 0) || (left.Length == 0)) || (right.Length > left.Length)))
            {
                return false;
            }
            if (right.Length == left.Length)
            {
                return (string.Compare(right, left, comparisonType) == 0);
            }
            if (((left.Length - right.Length) == 1) && (left[left.Length - 1] == m_directorySeparator))
            {
                return (string.Compare(left, 0, right, 0, right.Length, comparisonType) == 0);
            }
            if (right[right.Length - 1] == m_directorySeparator)
            {
                return (string.Compare(right, 0, left, 0, right.Length, comparisonType) == 0);
            }
            return ((left[right.Length] == m_directorySeparator) && (string.Compare(right, 0, left, 0, right.Length, comparisonType) == 0));
        }

        protected bool StringSubsetStringExpression(string left, StringExpressionSet right, bool ignoreCase)
        {
            for (int i = 0; i < right.m_list.Count; i++)
            {
                if (this.StringSubsetString(left, (string) right.m_list[i], ignoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        protected static bool StringSubsetStringExpressionPathDiscovery(string left, StringExpressionSet right, bool ignoreCase)
        {
            for (int i = 0; i < right.m_list.Count; i++)
            {
                if (StringSubsetStringPathDiscovery(left, (string) right.m_list[i], ignoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        protected static bool StringSubsetStringPathDiscovery(string left, string right, bool ignoreCase)
        {
            string str;
            string str2;
            StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (((right == null) || (left == null)) || ((right.Length == 0) || (left.Length == 0)))
            {
                return false;
            }
            if (right.Length == left.Length)
            {
                return (string.Compare(right, left, comparisonType) == 0);
            }
            if (right.Length < left.Length)
            {
                str = right;
                str2 = left;
            }
            else
            {
                str = left;
                str2 = right;
            }
            if (string.Compare(str, 0, str2, 0, str.Length, comparisonType) != 0)
            {
                return false;
            }
            if (((str.Length != 3) || !str.EndsWith(@":\", StringComparison.Ordinal)) || (((str[0] < 'A') || (str[0] > 'Z')) && ((str[0] < 'a') || (str[0] > 'z'))))
            {
                return (str2[str.Length] == m_directorySeparator);
            }
            return true;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            this.CheckList();
            this.Reduce();
            this.GenerateString();
            return this.m_expressions;
        }

        public string[] ToStringArray()
        {
            if ((this.m_expressionsArray == null) && (this.m_list != null))
            {
                this.m_expressionsArray = (string[]) this.m_list.ToArray(typeof(string));
            }
            return this.m_expressionsArray;
        }

        [SecurityCritical]
        public StringExpressionSet Union(StringExpressionSet ses)
        {
            if ((ses == null) || ses.IsEmpty())
            {
                return this.Copy();
            }
            if (this.IsEmpty())
            {
                return ses.Copy();
            }
            this.CheckList();
            ses.CheckList();
            StringExpressionSet set = (ses.m_list.Count > this.m_list.Count) ? ses : this;
            StringExpressionSet set2 = (ses.m_list.Count <= this.m_list.Count) ? ses : this;
            StringExpressionSet set3 = set.Copy();
            set3.Reduce();
            for (int i = 0; i < set2.m_list.Count; i++)
            {
                set3.AddSingleExpressionNoDuplicates((string) set2.m_list[i]);
            }
            set3.GenerateString();
            return set3;
        }
    }
}

