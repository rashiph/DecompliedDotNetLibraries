namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class Regex : ISerializable
    {
        internal static int cacheSize = 15;
        protected internal Hashtable capnames;
        protected internal Hashtable caps;
        protected internal int capsize;
        protected internal string[] capslist;
        internal RegexCode code;
        protected internal RegexRunnerFactory factory;
        internal static LinkedList<CachedCodeEntry> livecode = new LinkedList<CachedCodeEntry>();
        internal const int MaxOptionShift = 10;
        protected internal string pattern;
        internal bool refsInitialized;
        internal SharedReference replref;
        protected internal RegexOptions roptions;
        internal ExclusiveReference runnerref;

        protected Regex()
        {
        }

        public Regex(string pattern) : this(pattern, RegexOptions.None, false)
        {
        }

        protected Regex(SerializationInfo info, StreamingContext context) : this(info.GetString("pattern"), (RegexOptions) info.GetInt32("options"))
        {
        }

        public Regex(string pattern, RegexOptions options) : this(pattern, options, false)
        {
        }

        private Regex(string pattern, RegexOptions options, bool useCache)
        {
            CachedCodeEntry cachedAndUpdate = null;
            string str = null;
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }
            if ((options < RegexOptions.None) || ((((int) options) >> 10) != 0))
            {
                throw new ArgumentOutOfRangeException("options");
            }
            if (((options & RegexOptions.ECMAScript) != RegexOptions.None) && ((options & ~(RegexOptions.CultureInvariant | RegexOptions.ECMAScript | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase)) != RegexOptions.None))
            {
                throw new ArgumentOutOfRangeException("options");
            }
            if ((options & RegexOptions.CultureInvariant) != RegexOptions.None)
            {
                str = CultureInfo.InvariantCulture.ToString();
            }
            else
            {
                str = CultureInfo.CurrentCulture.ToString();
            }
            string[] strArray = new string[] { ((int) options).ToString(NumberFormatInfo.InvariantInfo), ":", str, ":", pattern };
            string key = string.Concat(strArray);
            cachedAndUpdate = LookupCachedAndUpdate(key);
            this.pattern = pattern;
            this.roptions = options;
            if (cachedAndUpdate == null)
            {
                RegexTree t = RegexParser.Parse(pattern, this.roptions);
                this.capnames = t._capnames;
                this.capslist = t._capslist;
                this.code = RegexWriter.Write(t);
                this.caps = this.code._caps;
                this.capsize = this.code._capsize;
                this.InitializeReferences();
                t = null;
                if (useCache)
                {
                    cachedAndUpdate = this.CacheCode(key);
                }
            }
            else
            {
                this.caps = cachedAndUpdate._caps;
                this.capnames = cachedAndUpdate._capnames;
                this.capslist = cachedAndUpdate._capslist;
                this.capsize = cachedAndUpdate._capsize;
                this.code = cachedAndUpdate._code;
                this.factory = cachedAndUpdate._factory;
                this.runnerref = cachedAndUpdate._runnerref;
                this.replref = cachedAndUpdate._replref;
                this.refsInitialized = true;
            }
            if (this.UseOptionC() && (this.factory == null))
            {
                this.factory = this.Compile(this.code, this.roptions);
                if (useCache && (cachedAndUpdate != null))
                {
                    cachedAndUpdate.AddCompiled(this.factory);
                }
                this.code = null;
            }
        }

        private CachedCodeEntry CacheCode(string key)
        {
            CachedCodeEntry entry = null;
            lock (livecode)
            {
                for (LinkedListNode<CachedCodeEntry> node = livecode.First; node != null; node = node.Next)
                {
                    if (node.Value._key == key)
                    {
                        livecode.Remove(node);
                        livecode.AddFirst(node);
                        return node.Value;
                    }
                }
                if (cacheSize != 0)
                {
                    entry = new CachedCodeEntry(key, this.capnames, this.capslist, this.code, this.caps, this.capsize, this.runnerref, this.replref);
                    livecode.AddFirst(entry);
                    if (livecode.Count > cacheSize)
                    {
                        livecode.RemoveLast();
                    }
                }
            }
            return entry;
        }

        [MethodImpl(MethodImplOptions.NoInlining), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        internal RegexRunnerFactory Compile(RegexCode code, RegexOptions roptions)
        {
            return RegexCompiler.Compile(code, roptions);
        }

        [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public static void CompileToAssembly(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname)
        {
            CompileToAssemblyInternal(regexinfos, assemblyname, null, null);
        }

        [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public static void CompileToAssembly(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname, CustomAttributeBuilder[] attributes)
        {
            CompileToAssemblyInternal(regexinfos, assemblyname, attributes, null);
        }

        [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public static void CompileToAssembly(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname, CustomAttributeBuilder[] attributes, string resourceFile)
        {
            CompileToAssemblyInternal(regexinfos, assemblyname, attributes, resourceFile);
        }

        private static void CompileToAssemblyInternal(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname, CustomAttributeBuilder[] attributes, string resourceFile)
        {
            if (assemblyname == null)
            {
                throw new ArgumentNullException("assemblyname");
            }
            if (regexinfos == null)
            {
                throw new ArgumentNullException("regexinfos");
            }
            RegexCompiler.CompileToAssembly(regexinfos, assemblyname, attributes, resourceFile);
        }

        public static string Escape(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return RegexParser.Escape(str);
        }

        public string[] GetGroupNames()
        {
            string[] strArray;
            if (this.capslist == null)
            {
                int capsize = this.capsize;
                strArray = new string[capsize];
                for (int i = 0; i < capsize; i++)
                {
                    strArray[i] = Convert.ToString(i, CultureInfo.InvariantCulture);
                }
                return strArray;
            }
            strArray = new string[this.capslist.Length];
            Array.Copy(this.capslist, 0, strArray, 0, this.capslist.Length);
            return strArray;
        }

        public int[] GetGroupNumbers()
        {
            int[] numArray;
            if (this.caps == null)
            {
                int capsize = this.capsize;
                numArray = new int[capsize];
                for (int i = 0; i < capsize; i++)
                {
                    numArray[i] = i;
                }
                return numArray;
            }
            numArray = new int[this.caps.Count];
            IDictionaryEnumerator enumerator = this.caps.GetEnumerator();
            while (enumerator.MoveNext())
            {
                numArray[(int) enumerator.Value] = (int) enumerator.Key;
            }
            return numArray;
        }

        public string GroupNameFromNumber(int i)
        {
            if (this.capslist == null)
            {
                if ((i >= 0) && (i < this.capsize))
                {
                    return i.ToString(CultureInfo.InvariantCulture);
                }
                return string.Empty;
            }
            if (this.caps != null)
            {
                object obj2 = this.caps[i];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                i = (int) obj2;
            }
            if ((i >= 0) && (i < this.capslist.Length))
            {
                return this.capslist[i];
            }
            return string.Empty;
        }

        public int GroupNumberFromName(string name)
        {
            int num = -1;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (this.capnames != null)
            {
                object obj2 = this.capnames[name];
                if (obj2 == null)
                {
                    return -1;
                }
                return (int) obj2;
            }
            num = 0;
            for (int i = 0; i < name.Length; i++)
            {
                char ch = name[i];
                if ((ch > '9') || (ch < '0'))
                {
                    return -1;
                }
                num *= 10;
                num += ch - '0';
            }
            if ((num >= 0) && (num < this.capsize))
            {
                return num;
            }
            return -1;
        }

        protected void InitializeReferences()
        {
            if (this.refsInitialized)
            {
                throw new NotSupportedException(SR.GetString("OnlyAllowedOnce"));
            }
            this.refsInitialized = true;
            this.runnerref = new ExclusiveReference();
            this.replref = new SharedReference();
        }

        public bool IsMatch(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return (null == this.Run(true, -1, input, 0, input.Length, this.UseOptionR() ? input.Length : 0));
        }

        public bool IsMatch(string input, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return (null == this.Run(true, -1, input, 0, input.Length, startat));
        }

        public static bool IsMatch(string input, string pattern)
        {
            return new Regex(pattern, RegexOptions.None, true).IsMatch(input);
        }

        public static bool IsMatch(string input, string pattern, RegexOptions options)
        {
            return new Regex(pattern, options, true).IsMatch(input);
        }

        private static CachedCodeEntry LookupCachedAndUpdate(string key)
        {
            lock (livecode)
            {
                for (LinkedListNode<CachedCodeEntry> node = livecode.First; node != null; node = node.Next)
                {
                    if (node.Value._key == key)
                    {
                        livecode.Remove(node);
                        livecode.AddFirst(node);
                        return node.Value;
                    }
                }
            }
            return null;
        }

        public System.Text.RegularExpressions.Match Match(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Run(false, -1, input, 0, input.Length, this.UseOptionR() ? input.Length : 0);
        }

        public System.Text.RegularExpressions.Match Match(string input, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Run(false, -1, input, 0, input.Length, startat);
        }

        public static System.Text.RegularExpressions.Match Match(string input, string pattern)
        {
            return new Regex(pattern, RegexOptions.None, true).Match(input);
        }

        public System.Text.RegularExpressions.Match Match(string input, int beginning, int length)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Run(false, -1, input, beginning, length, this.UseOptionR() ? (beginning + length) : beginning);
        }

        public static System.Text.RegularExpressions.Match Match(string input, string pattern, RegexOptions options)
        {
            return new Regex(pattern, options, true).Match(input);
        }

        public MatchCollection Matches(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return new MatchCollection(this, input, 0, input.Length, this.UseOptionR() ? input.Length : 0);
        }

        public MatchCollection Matches(string input, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return new MatchCollection(this, input, 0, input.Length, startat);
        }

        public static MatchCollection Matches(string input, string pattern)
        {
            return new Regex(pattern, RegexOptions.None, true).Matches(input);
        }

        public static MatchCollection Matches(string input, string pattern, RegexOptions options)
        {
            return new Regex(pattern, options, true).Matches(input);
        }

        public string Replace(string input, string replacement)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Replace(input, replacement, -1, this.UseOptionR() ? input.Length : 0);
        }

        public string Replace(string input, MatchEvaluator evaluator)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Replace(input, evaluator, -1, this.UseOptionR() ? input.Length : 0);
        }

        public string Replace(string input, string replacement, int count)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Replace(input, replacement, count, this.UseOptionR() ? input.Length : 0);
        }

        public static string Replace(string input, string pattern, string replacement)
        {
            return new Regex(pattern, RegexOptions.None, true).Replace(input, replacement);
        }

        public static string Replace(string input, string pattern, MatchEvaluator evaluator)
        {
            return new Regex(pattern, RegexOptions.None, true).Replace(input, evaluator);
        }

        public string Replace(string input, MatchEvaluator evaluator, int count)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Replace(input, evaluator, count, this.UseOptionR() ? input.Length : 0);
        }

        public string Replace(string input, string replacement, int count, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (replacement == null)
            {
                throw new ArgumentNullException("replacement");
            }
            RegexReplacement replacement2 = (RegexReplacement) this.replref.Get();
            if ((replacement2 == null) || !replacement2.Pattern.Equals(replacement))
            {
                replacement2 = RegexParser.ParseReplacement(replacement, this.caps, this.capsize, this.capnames, this.roptions);
                this.replref.Cache(replacement2);
            }
            return replacement2.Replace(this, input, count, startat);
        }

        public static string Replace(string input, string pattern, string replacement, RegexOptions options)
        {
            return new Regex(pattern, options, true).Replace(input, replacement);
        }

        public static string Replace(string input, string pattern, MatchEvaluator evaluator, RegexOptions options)
        {
            return new Regex(pattern, options, true).Replace(input, evaluator);
        }

        public string Replace(string input, MatchEvaluator evaluator, int count, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return RegexReplacement.Replace(evaluator, this, input, count, startat);
        }

        internal System.Text.RegularExpressions.Match Run(bool quick, int prevlen, string input, int beginning, int length, int startat)
        {
            RegexRunner runner = null;
            if ((startat < 0) || (startat > input.Length))
            {
                throw new ArgumentOutOfRangeException("start", SR.GetString("BeginIndexNotNegative"));
            }
            if ((length < 0) || (length > input.Length))
            {
                throw new ArgumentOutOfRangeException("length", SR.GetString("LengthNotNegative"));
            }
            runner = (RegexRunner) this.runnerref.Get();
            if (runner == null)
            {
                if (this.factory != null)
                {
                    runner = this.factory.CreateInstance();
                }
                else
                {
                    runner = new RegexInterpreter(this.code, this.UseOptionInvariant() ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
                }
            }
            System.Text.RegularExpressions.Match match = runner.Scan(this, input, beginning, beginning + length, startat, prevlen, quick);
            this.runnerref.Release(runner);
            return match;
        }

        public string[] Split(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return this.Split(input, 0, this.UseOptionR() ? input.Length : 0);
        }

        public string[] Split(string input, int count)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return RegexReplacement.Split(this, input, count, this.UseOptionR() ? input.Length : 0);
        }

        public static string[] Split(string input, string pattern)
        {
            return new Regex(pattern, RegexOptions.None, true).Split(input);
        }

        public string[] Split(string input, int count, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return RegexReplacement.Split(this, input, count, startat);
        }

        public static string[] Split(string input, string pattern, RegexOptions options)
        {
            return new Regex(pattern, options, true).Split(input);
        }

        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            si.AddValue("pattern", this.ToString());
            si.AddValue("options", this.Options);
        }

        public override string ToString()
        {
            return this.pattern;
        }

        public static string Unescape(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return RegexParser.Unescape(str);
        }

        protected bool UseOptionC()
        {
            return ((this.roptions & RegexOptions.Compiled) != RegexOptions.None);
        }

        internal bool UseOptionInvariant()
        {
            return ((this.roptions & RegexOptions.CultureInvariant) != RegexOptions.None);
        }

        protected bool UseOptionR()
        {
            return ((this.roptions & RegexOptions.RightToLeft) != RegexOptions.None);
        }

        public static int CacheSize
        {
            get
            {
                return cacheSize;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                cacheSize = value;
                if (livecode.Count > cacheSize)
                {
                    lock (livecode)
                    {
                        while (livecode.Count > cacheSize)
                        {
                            livecode.RemoveLast();
                        }
                    }
                }
            }
        }

        public RegexOptions Options
        {
            get
            {
                return this.roptions;
            }
        }

        public bool RightToLeft
        {
            get
            {
                return this.UseOptionR();
            }
        }
    }
}

