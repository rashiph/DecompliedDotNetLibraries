namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class DbConnectionOptions
    {
        private readonly Hashtable _parsetable;
        private PermissionSet _permissionset;
        private readonly string _usersConnectionString;
        private const string ConnectionStringQuoteOdbcValuePattern = "^\\{([^\\}\0]|\\}\\})*\\}$";
        private static readonly Regex ConnectionStringQuoteOdbcValueRegex = new Regex("^\\{([^\\}\0]|\\}\\})*\\}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private const string ConnectionStringQuoteValuePattern = "^[^\"'=;\\s\\p{Cc}]*$";
        private static readonly Regex ConnectionStringQuoteValueRegex = new Regex("^[^\"'=;\\s\\p{Cc}]*$", RegexOptions.Compiled);
        private const string ConnectionStringValidKeyPattern = @"^(?![;\s])[^\p{Cc}]+(?<!\s)$";
        private static readonly Regex ConnectionStringValidKeyRegex = new Regex(@"^(?![;\s])[^\p{Cc}]+(?<!\s)$", RegexOptions.Compiled);
        private const string ConnectionStringValidValuePattern = "^[^\0]*$";
        private static readonly Regex ConnectionStringValidValueRegex = new Regex("^[^\0]*$", RegexOptions.Compiled);
        internal const string DataDirectory = "|datadirectory|";
        internal readonly bool HasPasswordKeyword;
        internal readonly NameValuePair KeyChain;
        internal readonly bool UseOdbcRules;

        protected DbConnectionOptions(DbConnectionOptions connectionOptions)
        {
            this._usersConnectionString = connectionOptions._usersConnectionString;
            this.HasPasswordKeyword = connectionOptions.HasPasswordKeyword;
            this.UseOdbcRules = connectionOptions.UseOdbcRules;
            this._parsetable = connectionOptions._parsetable;
            this.KeyChain = connectionOptions.KeyChain;
        }

        public DbConnectionOptions(string connectionString) : this(connectionString, null, false)
        {
        }

        public DbConnectionOptions(string connectionString, Hashtable synonyms, bool useOdbcRules)
        {
            this.UseOdbcRules = useOdbcRules;
            this._parsetable = new Hashtable();
            this._usersConnectionString = (connectionString != null) ? connectionString : "";
            if (0 < this._usersConnectionString.Length)
            {
                this.KeyChain = ParseInternal(this._parsetable, this._usersConnectionString, true, synonyms, this.UseOdbcRules);
                this.HasPasswordKeyword = this._parsetable.ContainsKey("password") || this._parsetable.ContainsKey("pwd");
            }
        }

        internal static void AppendKeyValuePairBuilder(StringBuilder builder, string keyName, string keyValue, bool useOdbcRules)
        {
            ADP.CheckArgumentNull(builder, "builder");
            ADP.CheckArgumentLength(keyName, "keyName");
            if ((keyName == null) || !ConnectionStringValidKeyRegex.IsMatch(keyName))
            {
                throw ADP.InvalidKeyname(keyName);
            }
            if ((keyValue != null) && !IsValueValidInternal(keyValue))
            {
                throw ADP.InvalidValue(keyName);
            }
            if ((0 < builder.Length) && (';' != builder[builder.Length - 1]))
            {
                builder.Append(";");
            }
            if (useOdbcRules)
            {
                builder.Append(keyName);
            }
            else
            {
                builder.Append(keyName.Replace("=", "=="));
            }
            builder.Append("=");
            if (keyValue != null)
            {
                if (useOdbcRules)
                {
                    if (((0 < keyValue.Length) && ((('{' == keyValue[0]) || (0 <= keyValue.IndexOf(';'))) || (string.Compare("Driver", keyName, StringComparison.OrdinalIgnoreCase) == 0))) && !ConnectionStringQuoteOdbcValueRegex.IsMatch(keyValue))
                    {
                        builder.Append('{').Append(keyValue.Replace("}", "}}")).Append('}');
                    }
                    else
                    {
                        builder.Append(keyValue);
                    }
                }
                else if (ConnectionStringQuoteValueRegex.IsMatch(keyValue))
                {
                    builder.Append(keyValue);
                }
                else if ((-1 != keyValue.IndexOf('"')) && (-1 == keyValue.IndexOf('\'')))
                {
                    builder.Append('\'');
                    builder.Append(keyValue);
                    builder.Append('\'');
                }
                else
                {
                    builder.Append('"');
                    builder.Append(keyValue.Replace("\"", "\"\""));
                    builder.Append('"');
                }
            }
        }

        private static bool CompareInsensitiveInvariant(string strvalue, string strconst)
        {
            return (0 == StringComparer.OrdinalIgnoreCase.Compare(strvalue, strconst));
        }

        public bool ContainsKey(string keyword)
        {
            return this._parsetable.ContainsKey(keyword);
        }

        internal static int ConvertToInt32Internal(string keyname, string stringValue)
        {
            int num;
            try
            {
                num = int.Parse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            catch (FormatException exception2)
            {
                throw ADP.InvalidConnectionOptionValue(keyname, exception2);
            }
            catch (OverflowException exception)
            {
                throw ADP.InvalidConnectionOptionValue(keyname, exception);
            }
            return num;
        }

        public bool ConvertValueToBoolean(string keyName, bool defaultValue)
        {
            object obj2 = this._parsetable[keyName];
            if (obj2 == null)
            {
                return defaultValue;
            }
            return ConvertValueToBooleanInternal(keyName, (string) obj2);
        }

        internal static bool ConvertValueToBooleanInternal(string keyName, string stringValue)
        {
            if (CompareInsensitiveInvariant(stringValue, "true") || CompareInsensitiveInvariant(stringValue, "yes"))
            {
                return true;
            }
            if (!CompareInsensitiveInvariant(stringValue, "false") && !CompareInsensitiveInvariant(stringValue, "no"))
            {
                string strvalue = stringValue.Trim();
                if (CompareInsensitiveInvariant(strvalue, "true") || CompareInsensitiveInvariant(strvalue, "yes"))
                {
                    return true;
                }
                if (!CompareInsensitiveInvariant(strvalue, "false") && !CompareInsensitiveInvariant(strvalue, "no"))
                {
                    throw ADP.InvalidConnectionOptionValue(keyName);
                }
            }
            return false;
        }

        public int ConvertValueToInt32(string keyName, int defaultValue)
        {
            object obj2 = this._parsetable[keyName];
            if (obj2 == null)
            {
                return defaultValue;
            }
            return ConvertToInt32Internal(keyName, (string) obj2);
        }

        public bool ConvertValueToIntegratedSecurity()
        {
            object obj2 = this._parsetable["integrated security"];
            if (obj2 == null)
            {
                return false;
            }
            return this.ConvertValueToIntegratedSecurityInternal((string) obj2);
        }

        internal bool ConvertValueToIntegratedSecurityInternal(string stringValue)
        {
            if ((CompareInsensitiveInvariant(stringValue, "sspi") || CompareInsensitiveInvariant(stringValue, "true")) || CompareInsensitiveInvariant(stringValue, "yes"))
            {
                return true;
            }
            if (!CompareInsensitiveInvariant(stringValue, "false") && !CompareInsensitiveInvariant(stringValue, "no"))
            {
                string strvalue = stringValue.Trim();
                if ((CompareInsensitiveInvariant(strvalue, "sspi") || CompareInsensitiveInvariant(strvalue, "true")) || CompareInsensitiveInvariant(strvalue, "yes"))
                {
                    return true;
                }
                if (!CompareInsensitiveInvariant(strvalue, "false") && !CompareInsensitiveInvariant(strvalue, "no"))
                {
                    throw ADP.InvalidConnectionOptionValue("integrated security");
                }
            }
            return false;
        }

        public string ConvertValueToString(string keyName, string defaultValue)
        {
            string str = (string) this._parsetable[keyName];
            if (str == null)
            {
                return defaultValue;
            }
            return str;
        }

        protected internal virtual PermissionSet CreatePermissionSet()
        {
            return null;
        }

        internal void DemandPermission()
        {
            if (this._permissionset == null)
            {
                this._permissionset = this.CreatePermissionSet();
            }
            this._permissionset.Demand();
        }

        protected internal virtual string Expand()
        {
            return this._usersConnectionString;
        }

        internal string ExpandDataDirectories(ref string filename, ref int position)
        {
            string str = null;
            StringBuilder builder = new StringBuilder(this._usersConnectionString.Length);
            string datadir = null;
            int startIndex = 0;
            bool flag = false;
            for (NameValuePair pair = this.KeyChain; pair != null; pair = pair.Next)
            {
                str = pair.Value;
                if (this.UseOdbcRules)
                {
                    string str2;
                    if (((str2 = pair.Name) == null) || (((str2 != "driver") && (str2 != "pwd")) && (str2 != "uid")))
                    {
                        str = ExpandDataDirectory(pair.Name, str, ref datadir);
                    }
                }
                else
                {
                    switch (pair.Name)
                    {
                        case "provider":
                        case "data provider":
                        case "remote provider":
                        case "extended properties":
                        case "user id":
                        case "password":
                        case "uid":
                        case "pwd":
                            goto Label_0151;
                    }
                    str = ExpandDataDirectory(pair.Name, str, ref datadir);
                }
            Label_0151:
                if (str == null)
                {
                    str = pair.Value;
                }
                if (this.UseOdbcRules || ("file name" != pair.Name))
                {
                    if (str != pair.Value)
                    {
                        flag = true;
                        AppendKeyValuePairBuilder(builder, pair.Name, str, this.UseOdbcRules);
                        builder.Append(';');
                    }
                    else
                    {
                        builder.Append(this._usersConnectionString, startIndex, pair.Length);
                    }
                }
                else
                {
                    flag = true;
                    filename = str;
                    position = builder.Length;
                }
                startIndex += pair.Length;
            }
            if (flag)
            {
                return builder.ToString();
            }
            return null;
        }

        internal static string ExpandDataDirectory(string keyword, string value, ref string datadir)
        {
            string filename = null;
            if ((value != null) && value.StartsWith("|datadirectory|", StringComparison.OrdinalIgnoreCase))
            {
                string baseDirectory = datadir;
                if (baseDirectory == null)
                {
                    object data = AppDomain.CurrentDomain.GetData("DataDirectory");
                    baseDirectory = data as string;
                    if ((data != null) && (baseDirectory == null))
                    {
                        throw ADP.InvalidDataDirectory();
                    }
                    if (ADP.IsEmpty(baseDirectory))
                    {
                        baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    if (baseDirectory == null)
                    {
                        baseDirectory = "";
                    }
                    datadir = baseDirectory;
                }
                int length = "|datadirectory|".Length;
                bool flag2 = (0 < baseDirectory.Length) && (baseDirectory[baseDirectory.Length - 1] == '\\');
                bool flag = (length < value.Length) && (value[length] == '\\');
                if (!flag2 && !flag)
                {
                    filename = baseDirectory + '\\' + value.Substring(length);
                }
                else if (flag2 && flag)
                {
                    filename = baseDirectory + value.Substring(length + 1);
                }
                else
                {
                    filename = baseDirectory + value.Substring(length);
                }
                if (!ADP.GetFullPath(filename).StartsWith(baseDirectory, StringComparison.Ordinal))
                {
                    throw ADP.InvalidConnectionOptionValue(keyword);
                }
            }
            return filename;
        }

        internal string ExpandKeyword(string keyword, string replacementValue)
        {
            bool flag = false;
            int startIndex = 0;
            StringBuilder builder = new StringBuilder(this._usersConnectionString.Length);
            for (NameValuePair pair = this.KeyChain; pair != null; pair = pair.Next)
            {
                if ((pair.Name == keyword) && (pair.Value == this[keyword]))
                {
                    AppendKeyValuePairBuilder(builder, pair.Name, replacementValue, this.UseOdbcRules);
                    builder.Append(';');
                    flag = true;
                }
                else
                {
                    builder.Append(this._usersConnectionString, startIndex, pair.Length);
                }
                startIndex += pair.Length;
            }
            if (!flag)
            {
                AppendKeyValuePairBuilder(builder, keyword, replacementValue, this.UseOdbcRules);
            }
            return builder.ToString();
        }

        private static string GetKeyName(StringBuilder buffer)
        {
            int length = buffer.Length;
            while ((0 < length) && char.IsWhiteSpace(buffer[length - 1]))
            {
                length--;
            }
            return buffer.ToString(0, length).ToLower(CultureInfo.InvariantCulture);
        }

        private static string GetKeyValue(StringBuilder buffer, bool trimWhitespace)
        {
            int length = buffer.Length;
            int startIndex = 0;
            if (trimWhitespace)
            {
                while ((startIndex < length) && char.IsWhiteSpace(buffer[startIndex]))
                {
                    startIndex++;
                }
                while ((0 < length) && char.IsWhiteSpace(buffer[length - 1]))
                {
                    length--;
                }
            }
            return buffer.ToString(startIndex, length - startIndex);
        }

        internal static int GetKeyValuePair(string connectionString, int currentPosition, StringBuilder buffer, bool useOdbcRules, out string keyname, out string keyvalue)
        {
            int index = currentPosition;
            buffer.Length = 0;
            keyname = null;
            keyvalue = null;
            char c = '\0';
            ParserState nothingYet = ParserState.NothingYet;
            int length = connectionString.Length;
            while (currentPosition < length)
            {
                c = connectionString[currentPosition];
                switch (nothingYet)
                {
                    case ParserState.NothingYet:
                        if ((';' != c) && !char.IsWhiteSpace(c))
                        {
                            if (c != '\0')
                            {
                                break;
                            }
                            nothingYet = ParserState.NullTermination;
                        }
                        goto Label_0255;

                    case ParserState.Key:
                        if ('=' != c)
                        {
                            goto Label_00BD;
                        }
                        nothingYet = ParserState.KeyEqual;
                        goto Label_0255;

                    case ParserState.KeyEqual:
                        if (useOdbcRules || ('=' != c))
                        {
                            goto Label_00E9;
                        }
                        nothingYet = ParserState.Key;
                        goto Label_024D;

                    case ParserState.KeyEnd:
                        goto Label_010C;

                    case ParserState.UnquotedValue:
                        if (char.IsWhiteSpace(c) || (!char.IsControl(c) && (';' != c)))
                        {
                            goto Label_024D;
                        }
                        goto Label_0262;

                    case ParserState.DoubleQuoteValue:
                        if ('"' != c)
                        {
                            goto Label_0192;
                        }
                        nothingYet = ParserState.DoubleQuoteValueQuote;
                        goto Label_0255;

                    case ParserState.DoubleQuoteValueQuote:
                        if ('"' != c)
                        {
                            goto Label_01AB;
                        }
                        nothingYet = ParserState.DoubleQuoteValue;
                        goto Label_024D;

                    case ParserState.SingleQuoteValue:
                        if ('\'' != c)
                        {
                            goto Label_01C7;
                        }
                        nothingYet = ParserState.SingleQuoteValueQuote;
                        goto Label_0255;

                    case ParserState.SingleQuoteValueQuote:
                        if ('\'' != c)
                        {
                            goto Label_01DD;
                        }
                        nothingYet = ParserState.SingleQuoteValue;
                        goto Label_024D;

                    case ParserState.BraceQuoteValue:
                        if ('}' != c)
                        {
                            goto Label_01F6;
                        }
                        nothingYet = ParserState.BraceQuoteValueQuote;
                        goto Label_024D;

                    case ParserState.BraceQuoteValueQuote:
                        if ('}' != c)
                        {
                            goto Label_020A;
                        }
                        nothingYet = ParserState.BraceQuoteValue;
                        goto Label_024D;

                    case ParserState.QuotedValueEnd:
                        goto Label_0217;

                    case ParserState.NullTermination:
                        if ((c != '\0') && !char.IsWhiteSpace(c))
                        {
                            throw ADP.ConnectionStringSyntax(currentPosition);
                        }
                        goto Label_0255;

                    default:
                        throw ADP.InternalError(ADP.InternalErrorCode.InvalidParserState1);
                }
                if (char.IsControl(c))
                {
                    throw ADP.ConnectionStringSyntax(index);
                }
                index = currentPosition;
                if ('=' != c)
                {
                    nothingYet = ParserState.Key;
                    goto Label_024D;
                }
                nothingYet = ParserState.KeyEqual;
                goto Label_0255;
            Label_00BD:
                if (char.IsWhiteSpace(c) || !char.IsControl(c))
                {
                    goto Label_024D;
                }
                throw ADP.ConnectionStringSyntax(index);
            Label_00E9:
                keyname = GetKeyName(buffer);
                if (ADP.IsEmpty(keyname))
                {
                    throw ADP.ConnectionStringSyntax(index);
                }
                buffer.Length = 0;
                nothingYet = ParserState.KeyEnd;
            Label_010C:
                if (char.IsWhiteSpace(c))
                {
                    goto Label_0255;
                }
                if (useOdbcRules)
                {
                    if ('{' != c)
                    {
                        goto Label_013F;
                    }
                    nothingYet = ParserState.BraceQuoteValue;
                    goto Label_024D;
                }
                if ('\'' == c)
                {
                    nothingYet = ParserState.SingleQuoteValue;
                    goto Label_0255;
                }
                if ('"' == c)
                {
                    nothingYet = ParserState.DoubleQuoteValue;
                    goto Label_0255;
                }
            Label_013F:
                if ((';' == c) || (c == '\0'))
                {
                    break;
                }
                if (char.IsControl(c))
                {
                    throw ADP.ConnectionStringSyntax(index);
                }
                nothingYet = ParserState.UnquotedValue;
                goto Label_024D;
            Label_0192:
                if (c != '\0')
                {
                    goto Label_024D;
                }
                throw ADP.ConnectionStringSyntax(index);
            Label_01AB:
                keyvalue = GetKeyValue(buffer, false);
                nothingYet = ParserState.QuotedValueEnd;
                goto Label_0217;
            Label_01C7:
                if (c != '\0')
                {
                    goto Label_024D;
                }
                throw ADP.ConnectionStringSyntax(index);
            Label_01DD:
                keyvalue = GetKeyValue(buffer, false);
                nothingYet = ParserState.QuotedValueEnd;
                goto Label_0217;
            Label_01F6:
                if (c != '\0')
                {
                    goto Label_024D;
                }
                throw ADP.ConnectionStringSyntax(index);
            Label_020A:
                keyvalue = GetKeyValue(buffer, false);
                nothingYet = ParserState.QuotedValueEnd;
            Label_0217:
                if (char.IsWhiteSpace(c))
                {
                    goto Label_0255;
                }
                if (';' == c)
                {
                    break;
                }
                if (c == '\0')
                {
                    nothingYet = ParserState.NullTermination;
                    goto Label_0255;
                }
                throw ADP.ConnectionStringSyntax(index);
            Label_024D:
                buffer.Append(c);
            Label_0255:
                currentPosition++;
            }
        Label_0262:
            switch (nothingYet)
            {
                case ParserState.NothingYet:
                case ParserState.KeyEnd:
                case ParserState.NullTermination:
                    break;

                case ParserState.Key:
                case ParserState.DoubleQuoteValue:
                case ParserState.SingleQuoteValue:
                case ParserState.BraceQuoteValue:
                    throw ADP.ConnectionStringSyntax(index);

                case ParserState.KeyEqual:
                    keyname = GetKeyName(buffer);
                    if (ADP.IsEmpty(keyname))
                    {
                        throw ADP.ConnectionStringSyntax(index);
                    }
                    break;

                case ParserState.UnquotedValue:
                {
                    keyvalue = GetKeyValue(buffer, true);
                    char ch2 = keyvalue[keyvalue.Length - 1];
                    if (!useOdbcRules && (('\'' == ch2) || ('"' == ch2)))
                    {
                        throw ADP.ConnectionStringSyntax(index);
                    }
                    break;
                }
                case ParserState.DoubleQuoteValueQuote:
                case ParserState.SingleQuoteValueQuote:
                case ParserState.BraceQuoteValueQuote:
                case ParserState.QuotedValueEnd:
                    keyvalue = GetKeyValue(buffer, false);
                    break;

                default:
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidParserState2);
            }
            if ((';' == c) && (currentPosition < connectionString.Length))
            {
                currentPosition++;
            }
            return currentPosition;
        }

        private static bool IsKeyNameValid(string keyname)
        {
            if (keyname == null)
            {
                return false;
            }
            return ((((0 < keyname.Length) && (';' != keyname[0])) && !char.IsWhiteSpace(keyname[0])) && (-1 == keyname.IndexOf('\0')));
        }

        private static bool IsValueValidInternal(string keyvalue)
        {
            if (keyvalue != null)
            {
                return (-1 == keyvalue.IndexOf('\0'));
            }
            return true;
        }

        private static NameValuePair ParseInternal(Hashtable parsetable, string connectionString, bool buildChain, Hashtable synonyms, bool firstKey)
        {
            StringBuilder buffer = new StringBuilder();
            NameValuePair pair = null;
            NameValuePair pair2 = null;
            int num = 0;
            int length = connectionString.Length;
            while (num < length)
            {
                string str2;
                string str3;
                int currentPosition = num;
                num = GetKeyValuePair(connectionString, currentPosition, buffer, firstKey, out str2, out str3);
                if (ADP.IsEmpty(str2))
                {
                    return pair2;
                }
                string keyname = (synonyms != null) ? ((string) synonyms[str2]) : str2;
                if (!IsKeyNameValid(keyname))
                {
                    throw ADP.KeywordNotSupported(str2);
                }
                if (!firstKey || !parsetable.Contains(keyname))
                {
                    parsetable[keyname] = str3;
                }
                if (pair != null)
                {
                    pair = pair.Next = new NameValuePair(keyname, str3, num - currentPosition);
                }
                else if (buildChain)
                {
                    pair2 = pair = new NameValuePair(keyname, str3, num - currentPosition);
                }
            }
            return pair2;
        }

        internal NameValuePair ReplacePasswordPwd(out string constr, bool fakePassword)
        {
            int startIndex = 0;
            NameValuePair pair4 = null;
            NameValuePair pair3 = null;
            NameValuePair pair2 = null;
            StringBuilder builder = new StringBuilder(this._usersConnectionString.Length);
            for (NameValuePair pair = this.KeyChain; pair != null; pair = pair.Next)
            {
                if (("password" != pair.Name) && ("pwd" != pair.Name))
                {
                    builder.Append(this._usersConnectionString, startIndex, pair.Length);
                    if (fakePassword)
                    {
                        pair2 = new NameValuePair(pair.Name, pair.Value, pair.Length);
                    }
                }
                else if (fakePassword)
                {
                    builder.Append(pair.Name).Append("=*;");
                    pair2 = new NameValuePair(pair.Name, "*", pair.Name.Length + "=*;".Length);
                }
                if (fakePassword)
                {
                    if (pair3 != null)
                    {
                        pair3 = pair3.Next = pair2;
                    }
                    else
                    {
                        pair3 = pair4 = pair2;
                    }
                }
                startIndex += pair.Length;
            }
            constr = builder.ToString();
            return pair4;
        }

        public string UsersConnectionString(bool hidePassword)
        {
            return this.UsersConnectionString(hidePassword, false);
        }

        private string UsersConnectionString(bool hidePassword, bool forceHidePassword)
        {
            string constr = this._usersConnectionString;
            if (this.HasPasswordKeyword && (forceHidePassword || (hidePassword && !this.HasPersistablePassword)))
            {
                this.ReplacePasswordPwd(out constr, false);
            }
            if (constr == null)
            {
                return "";
            }
            return constr;
        }

        internal string UsersConnectionStringForTrace()
        {
            return this.UsersConnectionString(true, true);
        }

        internal static void ValidateKeyValuePair(string keyword, string value)
        {
            if ((keyword == null) || !ConnectionStringValidKeyRegex.IsMatch(keyword))
            {
                throw ADP.InvalidKeyname(keyword);
            }
            if ((value != null) && !ConnectionStringValidValueRegex.IsMatch(value))
            {
                throw ADP.InvalidValue(keyword);
            }
        }

        internal bool HasBlankPassword
        {
            get
            {
                if (this.ConvertValueToIntegratedSecurity())
                {
                    return false;
                }
                if (this._parsetable.ContainsKey("password"))
                {
                    return ADP.IsEmpty((string) this._parsetable["password"]);
                }
                if (this._parsetable.ContainsKey("pwd"))
                {
                    return ADP.IsEmpty((string) this._parsetable["pwd"]);
                }
                return ((this._parsetable.ContainsKey("user id") && !ADP.IsEmpty((string) this._parsetable["user id"])) || (this._parsetable.ContainsKey("uid") && !ADP.IsEmpty((string) this._parsetable["uid"])));
            }
        }

        internal bool HasPersistablePassword
        {
            get
            {
                if (this.HasPasswordKeyword)
                {
                    return this.ConvertValueToBoolean("persist security info", false);
                }
                return true;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (null == this.KeyChain);
            }
        }

        public string this[string keyword]
        {
            get
            {
                return (string) this._parsetable[keyword];
            }
        }

        public ICollection Keys
        {
            get
            {
                return this._parsetable.Keys;
            }
        }

        internal Hashtable Parsetable
        {
            get
            {
                return this._parsetable;
            }
        }

        private enum ParserState
        {
            BraceQuoteValue = 10,
            BraceQuoteValueQuote = 11,
            DoubleQuoteValue = 6,
            DoubleQuoteValueQuote = 7,
            Key = 2,
            KeyEnd = 4,
            KeyEqual = 3,
            NothingYet = 1,
            NullTermination = 13,
            QuotedValueEnd = 12,
            SingleQuoteValue = 8,
            SingleQuoteValueQuote = 9,
            UnquotedValue = 5
        }
    }
}

