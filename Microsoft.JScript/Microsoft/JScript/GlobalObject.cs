namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    public class GlobalObject
    {
        internal static readonly GlobalObject commonInstance = new GlobalObject();
        public const double Infinity = double.PositiveInfinity;
        public const double NaN = double.NaN;
        protected ActiveXObjectConstructor originalActiveXObjectField = null;
        protected ArrayConstructor originalArrayField = null;
        protected BooleanConstructor originalBooleanField = null;
        protected DateConstructor originalDateField = null;
        protected EnumeratorConstructor originalEnumeratorField = null;
        protected ErrorConstructor originalErrorField = null;
        protected ErrorConstructor originalEvalErrorField = null;
        protected FunctionConstructor originalFunctionField = null;
        protected NumberConstructor originalNumberField = null;
        protected ObjectConstructor originalObjectField = null;
        protected ObjectPrototype originalObjectPrototypeField = null;
        protected ErrorConstructor originalRangeErrorField = null;
        protected ErrorConstructor originalReferenceErrorField = null;
        protected RegExpConstructor originalRegExpField = null;
        protected StringConstructor originalStringField = null;
        protected ErrorConstructor originalSyntaxErrorField = null;
        protected ErrorConstructor originalTypeErrorField = null;
        protected ErrorConstructor originalURIErrorField = null;
        protected VBArrayConstructor originalVBArrayField = null;
        public static readonly Microsoft.JScript.Empty undefined = null;

        internal GlobalObject()
        {
        }

        private static void AppendInHex(StringBuilder bs, int value)
        {
            bs.Append('%');
            int num = (value >> 4) & 15;
            bs.Append((num >= 10) ? ((char) ((num - 10) + 0x41)) : ((char) (num + 0x30)));
            num = value & 15;
            bs.Append((num >= 10) ? ((char) ((num - 10) + 0x41)) : ((char) (num + 0x30)));
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_CollectGarbage)]
        public static void CollectGarbage()
        {
            GC.Collect();
        }

        private static string Decode(object encodedURI, URISetType flags)
        {
            string str = Microsoft.JScript.Convert.ToString(encodedURI);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (ch != '%')
                {
                    builder.Append(ch);
                }
                else
                {
                    char ch2;
                    int startIndex = i;
                    if ((i + 2) >= str.Length)
                    {
                        throw new JScriptException(JSError.URIDecodeError);
                    }
                    byte num3 = HexValue(str[i + 1], str[i + 2]);
                    i += 2;
                    if ((num3 & 0x80) == 0)
                    {
                        ch2 = (char) num3;
                    }
                    else
                    {
                        int num4 = 1;
                        while (((num3 << num4) & 0x80) != 0)
                        {
                            num4++;
                        }
                        if (((num4 == 1) || (num4 > 4)) || ((i + ((num4 - 1) * 3)) >= str.Length))
                        {
                            throw new JScriptException(JSError.URIDecodeError);
                        }
                        int num5 = num3 & (((int) 0xff) >> (num4 + 1));
                        while (num4 > 1)
                        {
                            if (str[i + 1] != '%')
                            {
                                throw new JScriptException(JSError.URIDecodeError);
                            }
                            num3 = HexValue(str[i + 2], str[i + 3]);
                            i += 3;
                            if ((num3 & 0xc0) != 0x80)
                            {
                                throw new JScriptException(JSError.URIDecodeError);
                            }
                            num5 = (num5 << 6) | (num3 & 0x3f);
                            num4--;
                        }
                        if ((num5 >= 0xd800) && (num5 < 0xe000))
                        {
                            throw new JScriptException(JSError.URIDecodeError);
                        }
                        if (num5 < 0x10000)
                        {
                            ch2 = (char) num5;
                        }
                        else
                        {
                            if (num5 > 0x10ffff)
                            {
                                throw new JScriptException(JSError.URIDecodeError);
                            }
                            builder.Append((char) ((((num5 - 0x10000) >> 10) & 0x3ff) + 0xd800));
                            builder.Append((char) (((num5 - 0x10000) & 0x3ff) + 0xdc00));
                            goto Label_01D4;
                        }
                    }
                    if (InURISet(ch2, flags))
                    {
                        builder.Append(str, startIndex, (i - startIndex) + 1);
                    }
                    else
                    {
                        builder.Append(ch2);
                    }
                Label_01D4:;
                }
            }
            return builder.ToString();
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_decodeURI)]
        public static string decodeURI(object encodedURI)
        {
            return Decode(encodedURI, URISetType.Reserved);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_decodeURIComponent)]
        public static string decodeURIComponent(object encodedURI)
        {
            return Decode(encodedURI, URISetType.None);
        }

        private static string Encode(object uri, URISetType flags)
        {
            string str = Microsoft.JScript.Convert.ToString(uri);
            StringBuilder bs = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (InURISet(ch, flags))
                {
                    bs.Append(ch);
                }
                else
                {
                    int num2 = ch;
                    if ((num2 >= 0) && (num2 <= 0x7f))
                    {
                        AppendInHex(bs, num2);
                    }
                    else if ((num2 >= 0x80) && (num2 <= 0x7ff))
                    {
                        AppendInHex(bs, (num2 >> 6) | 0xc0);
                        AppendInHex(bs, (num2 & 0x3f) | 0x80);
                    }
                    else if ((num2 < 0xd800) || (num2 > 0xdfff))
                    {
                        AppendInHex(bs, (num2 >> 12) | 0xe0);
                        AppendInHex(bs, ((num2 >> 6) & 0x3f) | 0x80);
                        AppendInHex(bs, (num2 & 0x3f) | 0x80);
                    }
                    else
                    {
                        if ((num2 >= 0xdc00) && (num2 <= 0xdfff))
                        {
                            throw new JScriptException(JSError.URIEncodeError);
                        }
                        if (++i >= str.Length)
                        {
                            throw new JScriptException(JSError.URIEncodeError);
                        }
                        int num3 = str[i];
                        if ((num3 < 0xdc00) || (num3 > 0xdfff))
                        {
                            throw new JScriptException(JSError.URIEncodeError);
                        }
                        num2 = (((num2 - 0xd800) << 10) + num3) + 0x2400;
                        AppendInHex(bs, (num2 >> 0x12) | 240);
                        AppendInHex(bs, ((num2 >> 12) & 0x3f) | 0x80);
                        AppendInHex(bs, ((num2 >> 6) & 0x3f) | 0x80);
                        AppendInHex(bs, (num2 & 0x3f) | 0x80);
                    }
                }
            }
            return bs.ToString();
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_encodeURI)]
        public static string encodeURI(object uri)
        {
            return Encode(uri, URISetType.Unescaped | URISetType.Reserved);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_encodeURIComponent)]
        public static string encodeURIComponent(object uriComponent)
        {
            return Encode(uriComponent, URISetType.Unescaped);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_escape), NotRecommended("escape")]
        public static string escape(object @string)
        {
            string str = Microsoft.JScript.Convert.ToString(@string);
            string str2 = "0123456789ABCDEF";
            int length = str.Length;
            StringBuilder builder = new StringBuilder(length * 2);
            int num3 = -1;
            while (++num3 < length)
            {
                char ch = str[num3];
                int num2 = ch;
                if ((((0x41 > num2) || (num2 > 90)) && ((0x61 > num2) || (num2 > 0x7a))) && ((0x30 > num2) || (num2 > 0x39)))
                {
                    switch (ch)
                    {
                        case '@':
                        case '*':
                        case '_':
                        case '+':
                        case '-':
                        case '.':
                        case '/':
                            goto Label_0125;
                    }
                    builder.Append('%');
                    if (num2 < 0x100)
                    {
                        builder.Append(str2[num2 / 0x10]);
                        ch = str2[num2 % 0x10];
                    }
                    else
                    {
                        builder.Append('u');
                        builder.Append(str2[(num2 >> 12) % 0x10]);
                        builder.Append(str2[(num2 >> 8) % 0x10]);
                        builder.Append(str2[(num2 >> 4) % 0x10]);
                        ch = str2[num2 % 0x10];
                    }
                }
            Label_0125:
                builder.Append(ch);
            }
            return builder.ToString();
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_eval)]
        public static object eval(object x)
        {
            throw new JScriptException(JSError.IllegalEval);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_GetObject), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static object GetObject(object moniker, object progId)
        {
            moniker = Microsoft.JScript.Convert.ToPrimitive(moniker, PreferredType.Either);
            if (!(progId is Missing))
            {
                progId = Microsoft.JScript.Convert.ToPrimitive(progId, PreferredType.Either);
            }
            string monikerName = (Microsoft.JScript.Convert.GetTypeCode(moniker) == TypeCode.String) ? moniker.ToString() : null;
            string progID = (Microsoft.JScript.Convert.GetTypeCode(progId) == TypeCode.String) ? progId.ToString() : null;
            if ((monikerName == null) || ((monikerName.Length == 0) && (progID == null)))
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            if ((progID == null) && !(progId is Missing))
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            if ((progID != null) && (progID.Length == 0))
            {
                throw new JScriptException(JSError.InvalidCall);
            }
            if ((progID == null) || (progID.Length == 0))
            {
                return Marshal.BindToMoniker(monikerName);
            }
            if ((monikerName == null) || (monikerName.Length == 0))
            {
                return Marshal.GetActiveObject(progID);
            }
            object obj2 = Activator.CreateInstance(Type.GetTypeFromProgID(progID));
            if (!(obj2 is UCOMIPersistFile))
            {
                throw new JScriptException(JSError.FileNotFound);
            }
            ((UCOMIPersistFile) obj2).Load(monikerName, 0);
            return obj2;
        }

        internal static int HexDigit(char c)
        {
            if ((c >= '0') && (c <= '9'))
            {
                return (c - '0');
            }
            if ((c >= 'A') && (c <= 'F'))
            {
                return (('\n' + c) - 0x41);
            }
            if ((c >= 'a') && (c <= 'f'))
            {
                return (('\n' + c) - 0x61);
            }
            return -1;
        }

        private static byte HexValue(char ch1, char ch2)
        {
            int num;
            int num2;
            if (((num = HexDigit(ch1)) < 0) || ((num2 = HexDigit(ch2)) < 0))
            {
                throw new JScriptException(JSError.URIDecodeError);
            }
            return (byte) ((num << 4) | num2);
        }

        private static bool InURISet(char ch, URISetType flags)
        {
            if ((flags & URISetType.Unescaped) != URISetType.None)
            {
                if ((((ch >= '0') && (ch <= '9')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= 'a') && (ch <= 'z')))
                {
                    return true;
                }
                switch (ch)
                {
                    case '_':
                    case '~':
                    case '\'':
                    case '(':
                    case ')':
                    case '*':
                    case '-':
                    case '.':
                    case '!':
                        return true;
                }
            }
            if ((flags & URISetType.Reserved) != URISetType.None)
            {
                switch (ch)
                {
                    case '#':
                    case '$':
                    case '&':
                    case '+':
                    case ',':
                    case '/':
                    case ':':
                    case ';':
                    case '=':
                    case '?':
                    case '@':
                        return true;
                }
            }
            return false;
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_isFinite)]
        public static bool isFinite(double number)
        {
            return (!double.IsInfinity(number) && !double.IsNaN(number));
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_isNaN)]
        public static bool isNaN(object num)
        {
            double num2 = Microsoft.JScript.Convert.ToNumber(num);
            return !(num2 == num2);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_parseFloat)]
        public static double parseFloat(object @string)
        {
            return Microsoft.JScript.Convert.ToNumber(Microsoft.JScript.Convert.ToString(@string), false, false, Missing.Value);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_parseInt)]
        public static double parseInt(object @string, object radix)
        {
            return Microsoft.JScript.Convert.ToNumber(Microsoft.JScript.Convert.ToString(@string), true, true, radix);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_ScriptEngine)]
        public static string ScriptEngine()
        {
            return "JScript";
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_ScriptEngineBuildVersion)]
        public static int ScriptEngineBuildVersion()
        {
            return 0x766f;
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_ScriptEngineMajorVersion)]
        public static int ScriptEngineMajorVersion()
        {
            return 10;
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_ScriptEngineMinorVersion)]
        public static int ScriptEngineMinorVersion()
        {
            return 0;
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Global_unescape), NotRecommended("unescape")]
        public static string unescape(object @string)
        {
            string str = Microsoft.JScript.Convert.ToString(@string);
            int length = str.Length;
            StringBuilder builder = new StringBuilder(length);
            int num6 = -1;
            while (++num6 < length)
            {
                char ch = str[num6];
                if (ch == '%')
                {
                    int num2;
                    int num3;
                    int num4;
                    int num5;
                    if (((((num6 + 5) < length) && (str[num6 + 1] == 'u')) && (((num2 = HexDigit(str[num6 + 2])) != -1) && ((num3 = HexDigit(str[num6 + 3])) != -1))) && (((num4 = HexDigit(str[num6 + 4])) != -1) && ((num5 = HexDigit(str[num6 + 5])) != -1)))
                    {
                        ch = (char) ((((num2 << 12) + (num3 << 8)) + (num4 << 4)) + num5);
                        num6 += 5;
                    }
                    else if ((((num6 + 2) < length) && ((num2 = HexDigit(str[num6 + 1])) != -1)) && ((num3 = HexDigit(str[num6 + 2])) != -1))
                    {
                        ch = (char) ((num2 << 4) + num3);
                        num6 += 2;
                    }
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static ActiveXObjectConstructor ActiveXObject
        {
            get
            {
                return ActiveXObjectConstructor.ob;
            }
        }

        public static ArrayConstructor Array
        {
            get
            {
                return ArrayConstructor.ob;
            }
        }

        public static Type boolean
        {
            get
            {
                return Typeob.Boolean;
            }
        }

        public static BooleanConstructor Boolean
        {
            get
            {
                return BooleanConstructor.ob;
            }
        }

        public static Type @byte
        {
            get
            {
                return Typeob.Byte;
            }
        }

        public static Type @char
        {
            get
            {
                return Typeob.Char;
            }
        }

        public static DateConstructor Date
        {
            get
            {
                return DateConstructor.ob;
            }
        }

        public static Type @decimal
        {
            get
            {
                return Typeob.Decimal;
            }
        }

        public static Type @double
        {
            get
            {
                return Typeob.Double;
            }
        }

        public static EnumeratorConstructor Enumerator
        {
            get
            {
                return EnumeratorConstructor.ob;
            }
        }

        public static ErrorConstructor Error
        {
            get
            {
                return ErrorConstructor.ob;
            }
        }

        public static ErrorConstructor EvalError
        {
            get
            {
                return ErrorConstructor.evalOb;
            }
        }

        public static Type @float
        {
            get
            {
                return Typeob.Single;
            }
        }

        public static FunctionConstructor Function
        {
            get
            {
                return FunctionConstructor.ob;
            }
        }

        public static Type @int
        {
            get
            {
                return Typeob.Int32;
            }
        }

        public static Type @long
        {
            get
            {
                return Typeob.Int64;
            }
        }

        public static MathObject Math
        {
            get
            {
                if (MathObject.ob == null)
                {
                    MathObject.ob = new MathObject(ObjectPrototype.ob);
                }
                return MathObject.ob;
            }
        }

        public static NumberConstructor Number
        {
            get
            {
                return NumberConstructor.ob;
            }
        }

        public static ObjectConstructor Object
        {
            get
            {
                return ObjectConstructor.ob;
            }
        }

        internal virtual ActiveXObjectConstructor originalActiveXObject
        {
            get
            {
                if (this.originalActiveXObjectField == null)
                {
                    this.originalActiveXObjectField = ActiveXObjectConstructor.ob;
                }
                return this.originalActiveXObjectField;
            }
        }

        internal virtual ArrayConstructor originalArray
        {
            get
            {
                if (this.originalArrayField == null)
                {
                    this.originalArrayField = ArrayConstructor.ob;
                }
                return this.originalArrayField;
            }
        }

        internal virtual BooleanConstructor originalBoolean
        {
            get
            {
                if (this.originalBooleanField == null)
                {
                    this.originalBooleanField = BooleanConstructor.ob;
                }
                return this.originalBooleanField;
            }
        }

        internal virtual DateConstructor originalDate
        {
            get
            {
                if (this.originalDateField == null)
                {
                    this.originalDateField = DateConstructor.ob;
                }
                return this.originalDateField;
            }
        }

        internal virtual EnumeratorConstructor originalEnumerator
        {
            get
            {
                if (this.originalEnumeratorField == null)
                {
                    this.originalEnumeratorField = EnumeratorConstructor.ob;
                }
                return this.originalEnumeratorField;
            }
        }

        internal virtual ErrorConstructor originalError
        {
            get
            {
                if (this.originalErrorField == null)
                {
                    this.originalErrorField = ErrorConstructor.ob;
                }
                return this.originalErrorField;
            }
        }

        internal virtual ErrorConstructor originalEvalError
        {
            get
            {
                if (this.originalEvalErrorField == null)
                {
                    this.originalEvalErrorField = ErrorConstructor.evalOb;
                }
                return this.originalEvalErrorField;
            }
        }

        internal virtual FunctionConstructor originalFunction
        {
            get
            {
                if (this.originalFunctionField == null)
                {
                    this.originalFunctionField = FunctionConstructor.ob;
                }
                return this.originalFunctionField;
            }
        }

        internal virtual NumberConstructor originalNumber
        {
            get
            {
                if (this.originalNumberField == null)
                {
                    this.originalNumberField = NumberConstructor.ob;
                }
                return this.originalNumberField;
            }
        }

        internal virtual ObjectConstructor originalObject
        {
            get
            {
                if (this.originalObjectField == null)
                {
                    this.originalObjectField = ObjectConstructor.ob;
                }
                return this.originalObjectField;
            }
        }

        internal virtual ObjectPrototype originalObjectPrototype
        {
            get
            {
                if (this.originalObjectPrototypeField == null)
                {
                    this.originalObjectPrototypeField = ObjectPrototype.ob;
                }
                return this.originalObjectPrototypeField;
            }
        }

        internal virtual ErrorConstructor originalRangeError
        {
            get
            {
                if (this.originalRangeErrorField == null)
                {
                    this.originalRangeErrorField = ErrorConstructor.rangeOb;
                }
                return this.originalRangeErrorField;
            }
        }

        internal virtual ErrorConstructor originalReferenceError
        {
            get
            {
                if (this.originalReferenceErrorField == null)
                {
                    this.originalReferenceErrorField = ErrorConstructor.referenceOb;
                }
                return this.originalReferenceErrorField;
            }
        }

        internal virtual RegExpConstructor originalRegExp
        {
            get
            {
                if (this.originalRegExpField == null)
                {
                    this.originalRegExpField = RegExpConstructor.ob;
                }
                return this.originalRegExpField;
            }
        }

        internal virtual StringConstructor originalString
        {
            get
            {
                if (this.originalStringField == null)
                {
                    this.originalStringField = StringConstructor.ob;
                }
                return this.originalStringField;
            }
        }

        internal virtual ErrorConstructor originalSyntaxError
        {
            get
            {
                if (this.originalSyntaxErrorField == null)
                {
                    this.originalSyntaxErrorField = ErrorConstructor.syntaxOb;
                }
                return this.originalSyntaxErrorField;
            }
        }

        internal virtual ErrorConstructor originalTypeError
        {
            get
            {
                if (this.originalTypeErrorField == null)
                {
                    this.originalTypeErrorField = ErrorConstructor.typeOb;
                }
                return this.originalTypeErrorField;
            }
        }

        internal virtual ErrorConstructor originalURIError
        {
            get
            {
                if (this.originalURIErrorField == null)
                {
                    this.originalURIErrorField = ErrorConstructor.uriOb;
                }
                return this.originalURIErrorField;
            }
        }

        internal virtual VBArrayConstructor originalVBArray
        {
            get
            {
                if (this.originalVBArrayField == null)
                {
                    this.originalVBArrayField = VBArrayConstructor.ob;
                }
                return this.originalVBArrayField;
            }
        }

        public static ErrorConstructor RangeError
        {
            get
            {
                return ErrorConstructor.rangeOb;
            }
        }

        public static ErrorConstructor ReferenceError
        {
            get
            {
                return ErrorConstructor.referenceOb;
            }
        }

        public static RegExpConstructor RegExp
        {
            get
            {
                return RegExpConstructor.ob;
            }
        }

        public static Type @sbyte
        {
            get
            {
                return Typeob.SByte;
            }
        }

        public static Type @short
        {
            get
            {
                return Typeob.Int16;
            }
        }

        public static StringConstructor String
        {
            get
            {
                return StringConstructor.ob;
            }
        }

        public static ErrorConstructor SyntaxError
        {
            get
            {
                return ErrorConstructor.syntaxOb;
            }
        }

        public static ErrorConstructor TypeError
        {
            get
            {
                return ErrorConstructor.typeOb;
            }
        }

        public static Type @uint
        {
            get
            {
                return Typeob.UInt32;
            }
        }

        public static Type @ulong
        {
            get
            {
                return Typeob.UInt64;
            }
        }

        public static ErrorConstructor URIError
        {
            get
            {
                return ErrorConstructor.uriOb;
            }
        }

        public static Type @ushort
        {
            get
            {
                return Typeob.UInt16;
            }
        }

        public static VBArrayConstructor VBArray
        {
            get
            {
                return VBArrayConstructor.ob;
            }
        }

        public static Type @void
        {
            get
            {
                return Typeob.Void;
            }
        }

        private enum URISetType
        {
            None,
            Reserved,
            Unescaped
        }
    }
}

