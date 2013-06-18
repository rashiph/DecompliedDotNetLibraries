namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    public class StringPrototype : StringObject
    {
        internal static StringConstructor _constructor;
        internal static readonly StringPrototype ob = new StringPrototype(FunctionPrototype.ob, ObjectPrototype.ob);

        internal StringPrototype(FunctionPrototype funcprot, ObjectPrototype parent) : base(parent, "")
        {
            base.noExpando = true;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_anchor)]
        public static string anchor(object thisob, object anchorName)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            string str2 = Microsoft.JScript.Convert.ToString(anchorName);
            return ("<A NAME=\"" + str2 + "\">" + str + "</A>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_big)]
        public static string big(object thisob)
        {
            return ("<BIG>" + Microsoft.JScript.Convert.ToString(thisob) + "</BIG>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_blink)]
        public static string blink(object thisob)
        {
            return ("<BLINK>" + Microsoft.JScript.Convert.ToString(thisob) + "</BLINK>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_bold)]
        public static string bold(object thisob)
        {
            return ("<B>" + Microsoft.JScript.Convert.ToString(thisob) + "</B>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_charAt)]
        public static string charAt(object thisob, double pos)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            double num = Microsoft.JScript.Convert.ToInteger(pos);
            if ((num >= 0.0) && (num < str.Length))
            {
                return str.Substring((int) num, 1);
            }
            return "";
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_charCodeAt)]
        public static object charCodeAt(object thisob, double pos)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            double num = Microsoft.JScript.Convert.ToInteger(pos);
            if ((num >= 0.0) && (num < str.Length))
            {
                return (int) str[(int) num];
            }
            return (double) 1.0 / (double) 0.0;
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_concat)]
        public static string concat(object thisob, params object[] args)
        {
            StringBuilder builder = new StringBuilder(Microsoft.JScript.Convert.ToString(thisob));
            for (int i = 0; i < args.Length; i++)
            {
                builder.Append(Microsoft.JScript.Convert.ToString(args[i]));
            }
            return builder.ToString();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_fixed)]
        public static string @fixed(object thisob)
        {
            return ("<TT>" + Microsoft.JScript.Convert.ToString(thisob) + "</TT>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_fontcolor)]
        public static string fontcolor(object thisob, object colorName)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            string str2 = Microsoft.JScript.Convert.ToString(thisob);
            return ("<FONT COLOR=\"" + str2 + "\">" + str + "</FONT>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_fontsize)]
        public static string fontsize(object thisob, object fontSize)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            string str2 = Microsoft.JScript.Convert.ToString(fontSize);
            return ("<FONT SIZE=\"" + str2 + "\">" + str + "</FONT>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_indexOf)]
        public static int indexOf(object thisob, object searchString, double position)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            string str2 = Microsoft.JScript.Convert.ToString(searchString);
            double num = Microsoft.JScript.Convert.ToInteger(position);
            int length = str.Length;
            if (num < 0.0)
            {
                num = 0.0;
            }
            if (num < length)
            {
                return str.IndexOf(str2, (int) num);
            }
            if (str2.Length != 0)
            {
                return -1;
            }
            return 0;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_italics)]
        public static string italics(object thisob)
        {
            return ("<I>" + Microsoft.JScript.Convert.ToString(thisob) + "</I>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_lastIndexOf)]
        public static int lastIndexOf(object thisob, object searchString, double position)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            string str2 = Microsoft.JScript.Convert.ToString(searchString);
            int length = str.Length;
            int num2 = ((position != position) || (position > length)) ? length : ((int) position);
            if (num2 < 0)
            {
                num2 = 0;
            }
            if (num2 >= length)
            {
                num2 = length;
            }
            int num3 = str2.Length;
            if (num3 == 0)
            {
                return num2;
            }
            int startIndex = (num2 - 1) + num3;
            if (startIndex >= length)
            {
                startIndex = length - 1;
            }
            if (startIndex < 0)
            {
                return -1;
            }
            return str.LastIndexOf(str2, startIndex);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_link)]
        public static string link(object thisob, object linkRef)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            string str2 = Microsoft.JScript.Convert.ToString(linkRef);
            return ("<A HREF=\"" + str2 + "\">" + str + "</A>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_localeCompare)]
        public static int localeCompare(object thisob, object thatob)
        {
            return string.Compare(Microsoft.JScript.Convert.ToString(thisob), Microsoft.JScript.Convert.ToString(thatob), StringComparison.CurrentCulture);
        }

        [JSFunction(JSFunctionAttributeEnum.HasEngine | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_match)]
        public static object match(object thisob, VsaEngine engine, object regExp)
        {
            Match match;
            string input = Microsoft.JScript.Convert.ToString(thisob);
            RegExpObject obj2 = ToRegExpObject(regExp, engine);
            if (!obj2.globalInt)
            {
                match = obj2.regex.Match(input);
                if (!match.Success)
                {
                    obj2.lastIndexInt = 0;
                    return DBNull.Value;
                }
                if (obj2.regExpConst != null)
                {
                    obj2.lastIndexInt = obj2.regExpConst.UpdateConstructor(obj2.regex, match, input);
                    return new RegExpMatch(obj2.regExpConst.arrayPrototype, obj2.regex, match, input);
                }
                return new RegExpMatch(engine.Globals.globalObject.originalRegExp.arrayPrototype, obj2.regex, match, input);
            }
            MatchCollection matches = obj2.regex.Matches(input);
            if (matches.Count == 0)
            {
                obj2.lastIndexInt = 0;
                return DBNull.Value;
            }
            match = matches[matches.Count - 1];
            obj2.lastIndexInt = obj2.regExpConst.UpdateConstructor(obj2.regex, match, input);
            return new RegExpMatch(obj2.regExpConst.arrayPrototype, obj2.regex, matches, input);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_replace)]
        public static string replace(object thisob, object regExp, object replacement)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            RegExpObject regExpObject = regExp as RegExpObject;
            if (regExpObject != null)
            {
                return ReplaceWithRegExp(str, regExpObject, replacement);
            }
            Regex regex = regExp as Regex;
            if (regex != null)
            {
                return ReplaceWithRegExp(str, new RegExpObject(regex), replacement);
            }
            return ReplaceWithString(str, Microsoft.JScript.Convert.ToString(regExp), Microsoft.JScript.Convert.ToString(replacement));
        }

        private static string ReplaceWithRegExp(string thisob, RegExpObject regExpObject, object replacement)
        {
            RegExpReplace replace = (replacement is ScriptFunction) ? ((RegExpReplace) new ReplaceUsingFunction(regExpObject.regex, (ScriptFunction) replacement, thisob)) : ((RegExpReplace) new Microsoft.JScript.ReplaceWithString(Microsoft.JScript.Convert.ToString(replacement)));
            MatchEvaluator evaluator = new MatchEvaluator(replace.Evaluate);
            string str = regExpObject.globalInt ? regExpObject.regex.Replace(thisob, evaluator) : regExpObject.regex.Replace(thisob, evaluator, 1);
            regExpObject.lastIndexInt = (replace.lastMatch == null) ? 0 : regExpObject.regExpConst.UpdateConstructor(regExpObject.regex, replace.lastMatch, thisob);
            return str;
        }

        private static string ReplaceWithString(string thisob, string searchString, string replaceString)
        {
            int index = thisob.IndexOf(searchString);
            if (index < 0)
            {
                return thisob;
            }
            StringBuilder builder = new StringBuilder(thisob.Substring(0, index));
            builder.Append(replaceString);
            builder.Append(thisob.Substring(index + searchString.Length));
            return builder.ToString();
        }

        [JSFunction(JSFunctionAttributeEnum.HasEngine | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_search)]
        public static int search(object thisob, VsaEngine engine, object regExp)
        {
            string input = Microsoft.JScript.Convert.ToString(thisob);
            RegExpObject obj2 = ToRegExpObject(regExp, engine);
            Match match = obj2.regex.Match(input);
            if (!match.Success)
            {
                obj2.lastIndexInt = 0;
                return -1;
            }
            obj2.lastIndexInt = obj2.regExpConst.UpdateConstructor(obj2.regex, match, input);
            return match.Index;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_slice)]
        public static string slice(object thisob, double start, object end)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            int length = str.Length;
            double num2 = Microsoft.JScript.Convert.ToInteger(start);
            double num3 = ((end == null) || (end is Missing)) ? ((double) length) : Microsoft.JScript.Convert.ToInteger(end);
            if (num2 < 0.0)
            {
                num2 = length + num2;
                if (num2 < 0.0)
                {
                    num2 = 0.0;
                }
            }
            else if (num2 > length)
            {
                num2 = length;
            }
            if (num3 < 0.0)
            {
                num3 = length + num3;
                if (num3 < 0.0)
                {
                    num3 = 0.0;
                }
            }
            else if (num3 > length)
            {
                num3 = length;
            }
            int num4 = (int) (num3 - num2);
            if (num4 <= 0)
            {
                return "";
            }
            return str.Substring((int) num2, num4);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_small)]
        public static string small(object thisob)
        {
            return ("<SMALL>" + Microsoft.JScript.Convert.ToString(thisob) + "</SMALL>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasEngine | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_split)]
        public static ArrayObject split(object thisob, VsaEngine engine, object separator, object limit)
        {
            string str = Microsoft.JScript.Convert.ToString(thisob);
            uint maxValue = uint.MaxValue;
            if (((limit != null) && !(limit is Missing)) && (limit != DBNull.Value))
            {
                double num2 = Microsoft.JScript.Convert.ToInteger(limit);
                if ((num2 >= 0.0) && (num2 < 4294967295))
                {
                    maxValue = (uint) num2;
                }
            }
            if (maxValue == 0)
            {
                return engine.GetOriginalArrayConstructor().Construct();
            }
            if ((separator == null) || (separator is Missing))
            {
                ArrayObject obj2 = engine.GetOriginalArrayConstructor().Construct();
                obj2.SetValueAtIndex(0, thisob);
                return obj2;
            }
            RegExpObject regExpObject = separator as RegExpObject;
            if (regExpObject != null)
            {
                return SplitWithRegExp(str, engine, regExpObject, maxValue);
            }
            Regex regex = separator as Regex;
            if (regex != null)
            {
                return SplitWithRegExp(str, engine, new RegExpObject(regex), maxValue);
            }
            return SplitWithString(str, engine, Microsoft.JScript.Convert.ToString(separator), maxValue);
        }

        private static ArrayObject SplitWithRegExp(string thisob, VsaEngine engine, RegExpObject regExpObject, uint limit)
        {
            Match match2;
            ArrayObject obj2 = engine.GetOriginalArrayConstructor().Construct();
            Match match = regExpObject.regex.Match(thisob);
            if (!match.Success)
            {
                obj2.SetValueAtIndex(0, thisob);
                regExpObject.lastIndexInt = 0;
                return obj2;
            }
            int startIndex = 0;
            uint index = 0;
            do
            {
                int length = match.Index - startIndex;
                if (length > 0)
                {
                    obj2.SetValueAtIndex(index++, thisob.Substring(startIndex, length));
                    if ((limit > 0) && (index >= limit))
                    {
                        regExpObject.lastIndexInt = regExpObject.regExpConst.UpdateConstructor(regExpObject.regex, match, thisob);
                        return obj2;
                    }
                }
                startIndex = match.Index + match.Length;
                match2 = match;
                match = match.NextMatch();
            }
            while (match.Success);
            if (startIndex < thisob.Length)
            {
                obj2.SetValueAtIndex(index, thisob.Substring(startIndex));
            }
            regExpObject.lastIndexInt = regExpObject.regExpConst.UpdateConstructor(regExpObject.regex, match2, thisob);
            return obj2;
        }

        private static ArrayObject SplitWithString(string thisob, VsaEngine engine, string separator, uint limit)
        {
            int num4;
            ArrayObject obj2 = engine.GetOriginalArrayConstructor().Construct();
            if (separator.Length == 0)
            {
                if (limit > thisob.Length)
                {
                    limit = (uint) thisob.Length;
                }
                for (int i = 0; i < limit; i++)
                {
                    obj2.SetValueAtIndex((uint) i, thisob[i].ToString());
                }
                return obj2;
            }
            int startIndex = 0;
            uint index = 0;
            while ((num4 = thisob.IndexOf(separator, startIndex)) >= 0)
            {
                obj2.SetValueAtIndex(index++, thisob.Substring(startIndex, num4 - startIndex));
                if (index >= limit)
                {
                    return obj2;
                }
                startIndex = num4 + separator.Length;
            }
            if (index == 0)
            {
                obj2.SetValueAtIndex(0, thisob);
                return obj2;
            }
            obj2.SetValueAtIndex(index, thisob.Substring(startIndex));
            return obj2;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_strike)]
        public static string strike(object thisob)
        {
            return ("<STRIKE>" + Microsoft.JScript.Convert.ToString(thisob) + "</STRIKE>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_sub)]
        public static string sub(object thisob)
        {
            return ("<SUB>" + Microsoft.JScript.Convert.ToString(thisob) + "</SUB>");
        }

        [NotRecommended("substr"), JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_substr)]
        public static string substr(object thisob, double start, object count)
        {
            string str = thisob as string;
            if (str == null)
            {
                str = Microsoft.JScript.Convert.ToString(thisob);
            }
            int length = str.Length;
            double val = Microsoft.JScript.Convert.ToInteger(start);
            if (val < 0.0)
            {
                val += length;
            }
            if (val < 0.0)
            {
                val = 0.0;
            }
            else if (val > length)
            {
                val = length;
            }
            int num3 = (count is int) ? ((int) count) : (((count == null) || (count is Missing)) ? (length - ((int) Runtime.DoubleToInt64(val))) : ((int) Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToInteger(count))));
            if ((val + num3) > length)
            {
                num3 = length - ((int) val);
            }
            if (num3 <= 0)
            {
                return "";
            }
            return str.Substring((int) val, num3);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_substring)]
        public static string substring(object thisob, double start, object end)
        {
            string str = thisob as string;
            if (str == null)
            {
                str = Microsoft.JScript.Convert.ToString(thisob);
            }
            int length = str.Length;
            double num2 = Microsoft.JScript.Convert.ToInteger(start);
            if (num2 < 0.0)
            {
                num2 = 0.0;
            }
            else if (num2 > length)
            {
                num2 = length;
            }
            double num3 = ((end == null) || (end is Missing)) ? ((double) length) : Microsoft.JScript.Convert.ToInteger(end);
            if (num3 < 0.0)
            {
                num3 = 0.0;
            }
            else if (num3 > length)
            {
                num3 = length;
            }
            if (num2 > num3)
            {
                double num4 = num2;
                num2 = num3;
                num3 = num4;
            }
            return str.Substring((int) num2, (int) (num3 - num2));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_sup)]
        public static string sup(object thisob)
        {
            return ("<SUP>" + Microsoft.JScript.Convert.ToString(thisob) + "</SUP>");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toLocaleLowerCase)]
        public static string toLocaleLowerCase(object thisob)
        {
            return Microsoft.JScript.Convert.ToString(thisob).ToLower(CultureInfo.CurrentUICulture);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toLocaleUpperCase)]
        public static string toLocaleUpperCase(object thisob)
        {
            return Microsoft.JScript.Convert.ToString(thisob).ToUpperInvariant();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toLowerCase)]
        public static string toLowerCase(object thisob)
        {
            return Microsoft.JScript.Convert.ToString(thisob).ToLowerInvariant();
        }

        private static RegExpObject ToRegExpObject(object regExp, VsaEngine engine)
        {
            if ((regExp == null) || (regExp is Missing))
            {
                return (RegExpObject) engine.GetOriginalRegExpConstructor().Construct("", false, false, false);
            }
            RegExpObject obj2 = regExp as RegExpObject;
            if (obj2 != null)
            {
                return obj2;
            }
            Regex regex = regExp as Regex;
            if (regex != null)
            {
                return new RegExpObject(regex);
            }
            return (RegExpObject) engine.GetOriginalRegExpConstructor().Construct(Microsoft.JScript.Convert.ToString(regExp), false, false, false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toString)]
        public static string toString(object thisob)
        {
            StringObject obj2 = thisob as StringObject;
            if (obj2 != null)
            {
                return obj2.value;
            }
            ConcatString str = thisob as ConcatString;
            if (str != null)
            {
                return str.ToString();
            }
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(thisob);
            if (Microsoft.JScript.Convert.GetTypeCode(thisob, iConvertible) != TypeCode.String)
            {
                throw new JScriptException(JSError.StringExpected);
            }
            return iConvertible.ToString(null);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_toUpperCase)]
        public static string toUpperCase(object thisob)
        {
            return Microsoft.JScript.Convert.ToString(thisob).ToUpperInvariant();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.String_valueOf)]
        public static object valueOf(object thisob)
        {
            return toString(thisob);
        }

        public static StringConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

