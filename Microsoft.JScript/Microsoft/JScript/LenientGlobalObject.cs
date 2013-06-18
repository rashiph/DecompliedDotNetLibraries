namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;

    public sealed class LenientGlobalObject : GlobalObject
    {
        private object ActiveXObjectField;
        private object ArrayField;
        private LenientArrayPrototype arrayPrototypeField;
        public object boolean;
        private object BooleanField;
        public object @byte;
        public object @char;
        private object DateField;
        public object @decimal;
        public object decodeURI;
        public object decodeURIComponent;
        public object @double;
        public object encodeURI;
        public object encodeURIComponent;
        private VsaEngine engine;
        private object EnumeratorField;
        private object ErrorField;
        [NotRecommended("escape")]
        public object escape;
        public object eval;
        private object EvalErrorField;
        public object @float;
        private object FunctionField;
        private LenientFunctionPrototype functionPrototypeField;
        public object GetObject;
        public object Infinity;
        public object @int;
        public object isFinite;
        public object isNaN;
        public object @long;
        private object MathField;
        public object NaN;
        private object NumberField;
        private object ObjectField;
        private LenientObjectPrototype objectPrototypeField;
        public object parseFloat;
        public object parseInt;
        private object RangeErrorField;
        private object ReferenceErrorField;
        private object RegExpField;
        public object @sbyte;
        public object ScriptEngine;
        public object ScriptEngineBuildVersion;
        public object ScriptEngineMajorVersion;
        public object ScriptEngineMinorVersion;
        public object @short;
        private object StringField;
        private object SyntaxErrorField;
        private object TypeErrorField;
        public object @uint;
        public object @ulong;
        public object undefined;
        [NotRecommended("unescape")]
        public object unescape;
        private object URIErrorField;
        public object @ushort;
        private object VBArrayField;
        public object @void;

        internal LenientGlobalObject(VsaEngine engine)
        {
            this.engine = engine;
            this.Infinity = (double) 1.0 / (double) 0.0;
            this.NaN = (double) 1.0 / (double) 0.0;
            this.undefined = null;
            this.ActiveXObjectField = Missing.Value;
            this.ArrayField = Missing.Value;
            this.BooleanField = Missing.Value;
            this.DateField = Missing.Value;
            this.EnumeratorField = Missing.Value;
            this.ErrorField = Missing.Value;
            this.EvalErrorField = Missing.Value;
            this.FunctionField = Missing.Value;
            this.MathField = Missing.Value;
            this.NumberField = Missing.Value;
            this.ObjectField = Missing.Value;
            this.RangeErrorField = Missing.Value;
            this.ReferenceErrorField = Missing.Value;
            this.RegExpField = Missing.Value;
            this.StringField = Missing.Value;
            this.SyntaxErrorField = Missing.Value;
            this.TypeErrorField = Missing.Value;
            this.VBArrayField = Missing.Value;
            this.URIErrorField = Missing.Value;
            Type type = typeof(GlobalObject);
            LenientFunctionPrototype functionPrototype = this.functionPrototype;
            this.decodeURI = new BuiltinFunction("decodeURI", this, type.GetMethod("decodeURI"), functionPrototype);
            this.decodeURIComponent = new BuiltinFunction("decodeURIComponent", this, type.GetMethod("decodeURIComponent"), functionPrototype);
            this.encodeURI = new BuiltinFunction("encodeURI", this, type.GetMethod("encodeURI"), functionPrototype);
            this.encodeURIComponent = new BuiltinFunction("encodeURIComponent", this, type.GetMethod("encodeURIComponent"), functionPrototype);
            this.escape = new BuiltinFunction("escape", this, type.GetMethod("escape"), functionPrototype);
            this.eval = new BuiltinFunction("eval", this, type.GetMethod("eval"), functionPrototype);
            this.isNaN = new BuiltinFunction("isNaN", this, type.GetMethod("isNaN"), functionPrototype);
            this.isFinite = new BuiltinFunction("isFinite", this, type.GetMethod("isFinite"), functionPrototype);
            this.parseInt = new BuiltinFunction("parseInt", this, type.GetMethod("parseInt"), functionPrototype);
            this.GetObject = new BuiltinFunction("GetObject", this, type.GetMethod("GetObject"), functionPrototype);
            this.parseFloat = new BuiltinFunction("parseFloat", this, type.GetMethod("parseFloat"), functionPrototype);
            this.ScriptEngine = new BuiltinFunction("ScriptEngine", this, type.GetMethod("ScriptEngine"), functionPrototype);
            this.ScriptEngineBuildVersion = new BuiltinFunction("ScriptEngineBuildVersion", this, type.GetMethod("ScriptEngineBuildVersion"), functionPrototype);
            this.ScriptEngineMajorVersion = new BuiltinFunction("ScriptEngineMajorVersion", this, type.GetMethod("ScriptEngineMajorVersion"), functionPrototype);
            this.ScriptEngineMinorVersion = new BuiltinFunction("ScriptEngineMinorVersion", this, type.GetMethod("ScriptEngineMinorVersion"), functionPrototype);
            this.unescape = new BuiltinFunction("unescape", this, type.GetMethod("unescape"), functionPrototype);
            this.boolean = Typeob.Boolean;
            this.@byte = Typeob.Byte;
            this.@char = Typeob.Char;
            this.@decimal = Typeob.Decimal;
            this.@double = Typeob.Double;
            this.@float = Typeob.Single;
            this.@int = Typeob.Int32;
            this.@long = Typeob.Int64;
            this.@sbyte = Typeob.SByte;
            this.@short = Typeob.Int16;
            this.@void = Typeob.Void;
            this.@uint = Typeob.UInt32;
            this.@ulong = Typeob.UInt64;
            this.@ushort = Typeob.UInt16;
        }

        public object ActiveXObject
        {
            get
            {
                if (this.ActiveXObjectField is Missing)
                {
                    this.ActiveXObjectField = this.originalActiveXObject;
                }
                return this.ActiveXObjectField;
            }
            set
            {
                this.ActiveXObjectField = value;
            }
        }

        public object Array
        {
            get
            {
                if (this.ArrayField is Missing)
                {
                    this.ArrayField = this.originalArray;
                }
                return this.ArrayField;
            }
            set
            {
                this.ArrayField = value;
            }
        }

        private LenientArrayPrototype arrayPrototype
        {
            get
            {
                if (this.arrayPrototypeField == null)
                {
                    this.arrayPrototypeField = new LenientArrayPrototype(this.functionPrototype, this.objectPrototype);
                }
                return this.arrayPrototypeField;
            }
        }

        public object Boolean
        {
            get
            {
                if (this.BooleanField is Missing)
                {
                    this.BooleanField = this.originalBoolean;
                }
                return this.BooleanField;
            }
            set
            {
                this.BooleanField = value;
            }
        }

        public object Date
        {
            get
            {
                if (this.DateField is Missing)
                {
                    this.DateField = this.originalDate;
                }
                return this.DateField;
            }
            set
            {
                this.DateField = value;
            }
        }

        public object Enumerator
        {
            get
            {
                if (this.EnumeratorField is Missing)
                {
                    this.EnumeratorField = this.originalEnumerator;
                }
                return this.EnumeratorField;
            }
            set
            {
                this.EnumeratorField = value;
            }
        }

        public object Error
        {
            get
            {
                if (this.ErrorField is Missing)
                {
                    this.ErrorField = this.originalError;
                }
                return this.ErrorField;
            }
            set
            {
                this.ErrorField = value;
            }
        }

        public object EvalError
        {
            get
            {
                if (this.EvalErrorField is Missing)
                {
                    this.EvalErrorField = this.originalEvalError;
                }
                return this.EvalErrorField;
            }
            set
            {
                this.EvalErrorField = value;
            }
        }

        public object Function
        {
            get
            {
                if (this.FunctionField is Missing)
                {
                    this.FunctionField = this.originalFunction;
                }
                return this.FunctionField;
            }
            set
            {
                this.FunctionField = value;
            }
        }

        private LenientFunctionPrototype functionPrototype
        {
            get
            {
                if (this.functionPrototypeField == null)
                {
                    LenientObjectPrototype objectPrototype = this.objectPrototype;
                }
                return this.functionPrototypeField;
            }
        }

        public object Math
        {
            get
            {
                if (this.MathField is Missing)
                {
                    this.MathField = new LenientMathObject(this.objectPrototype, this.functionPrototype);
                }
                return this.MathField;
            }
            set
            {
                this.MathField = value;
            }
        }

        public object Number
        {
            get
            {
                if (this.NumberField is Missing)
                {
                    this.NumberField = this.originalNumber;
                }
                return this.NumberField;
            }
            set
            {
                this.NumberField = value;
            }
        }

        public object Object
        {
            get
            {
                if (this.ObjectField is Missing)
                {
                    this.ObjectField = this.originalObject;
                }
                return this.ObjectField;
            }
            set
            {
                this.ObjectField = value;
            }
        }

        private LenientObjectPrototype objectPrototype
        {
            get
            {
                if (this.objectPrototypeField == null)
                {
                    LenientObjectPrototype parent = this.objectPrototypeField = new LenientObjectPrototype(this.engine);
                    LenientFunctionPrototype funcprot = this.functionPrototypeField = new LenientFunctionPrototype(parent);
                    parent.Initialize(funcprot);
                    JSObject obj2 = new JSObject(parent, false);
                    obj2.AddField("constructor").SetValue(obj2, funcprot);
                    funcprot.proto = obj2;
                }
                return this.objectPrototypeField;
            }
        }

        internal override ActiveXObjectConstructor originalActiveXObject
        {
            get
            {
                if (base.originalActiveXObjectField == null)
                {
                    base.originalActiveXObjectField = new ActiveXObjectConstructor(this.functionPrototype);
                }
                return base.originalActiveXObjectField;
            }
        }

        internal override ArrayConstructor originalArray
        {
            get
            {
                if (base.originalArrayField == null)
                {
                    base.originalArrayField = new ArrayConstructor(this.functionPrototype, this.arrayPrototype);
                }
                return base.originalArrayField;
            }
        }

        internal override BooleanConstructor originalBoolean
        {
            get
            {
                if (base.originalBooleanField == null)
                {
                    base.originalBooleanField = new BooleanConstructor(this.functionPrototype, new LenientBooleanPrototype(this.functionPrototype, this.objectPrototype));
                }
                return base.originalBooleanField;
            }
        }

        internal override DateConstructor originalDate
        {
            get
            {
                if (base.originalDateField == null)
                {
                    base.originalDateField = new LenientDateConstructor(this.functionPrototype, new LenientDatePrototype(this.functionPrototype, this.objectPrototype));
                }
                return base.originalDateField;
            }
        }

        internal override EnumeratorConstructor originalEnumerator
        {
            get
            {
                if (base.originalEnumeratorField == null)
                {
                    base.originalEnumeratorField = new EnumeratorConstructor(this.functionPrototype, new LenientEnumeratorPrototype(this.functionPrototype, this.objectPrototype));
                }
                return base.originalEnumeratorField;
            }
        }

        internal override ErrorConstructor originalError
        {
            get
            {
                if (base.originalErrorField == null)
                {
                    base.originalErrorField = new ErrorConstructor(this.functionPrototype, new LenientErrorPrototype(this.functionPrototype, this.objectPrototype, "Error"), this);
                }
                return base.originalErrorField;
            }
        }

        internal override ErrorConstructor originalEvalError
        {
            get
            {
                if (base.originalEvalErrorField == null)
                {
                    base.originalEvalErrorField = new ErrorConstructor("EvalError", ErrorType.EvalError, this.originalError, this);
                }
                return base.originalEvalErrorField;
            }
        }

        internal override FunctionConstructor originalFunction
        {
            get
            {
                if (base.originalFunctionField == null)
                {
                    base.originalFunctionField = new FunctionConstructor(this.functionPrototype);
                }
                return base.originalFunctionField;
            }
        }

        internal override NumberConstructor originalNumber
        {
            get
            {
                if (base.originalNumberField == null)
                {
                    base.originalNumberField = new NumberConstructor(this.functionPrototype, new LenientNumberPrototype(this.functionPrototype, this.objectPrototype));
                }
                return base.originalNumberField;
            }
        }

        internal override ObjectConstructor originalObject
        {
            get
            {
                if (base.originalObjectField == null)
                {
                    base.originalObjectField = new ObjectConstructor(this.functionPrototype, this.objectPrototype);
                }
                return base.originalObjectField;
            }
        }

        internal override ObjectPrototype originalObjectPrototype
        {
            get
            {
                if (base.originalObjectPrototypeField == null)
                {
                    base.originalObjectPrototypeField = ObjectPrototype.ob;
                }
                return base.originalObjectPrototypeField;
            }
        }

        internal override ErrorConstructor originalRangeError
        {
            get
            {
                if (base.originalRangeErrorField == null)
                {
                    base.originalRangeErrorField = new ErrorConstructor("RangeError", ErrorType.RangeError, this.originalError, this);
                }
                return base.originalRangeErrorField;
            }
        }

        internal override ErrorConstructor originalReferenceError
        {
            get
            {
                if (base.originalReferenceErrorField == null)
                {
                    base.originalReferenceErrorField = new ErrorConstructor("ReferenceError", ErrorType.ReferenceError, this.originalError, this);
                }
                return base.originalReferenceErrorField;
            }
        }

        internal override RegExpConstructor originalRegExp
        {
            get
            {
                if (base.originalRegExpField == null)
                {
                    base.originalRegExpField = new RegExpConstructor(this.functionPrototype, new LenientRegExpPrototype(this.functionPrototype, this.objectPrototype), this.arrayPrototype);
                }
                return base.originalRegExpField;
            }
        }

        internal override StringConstructor originalString
        {
            get
            {
                if (base.originalStringField == null)
                {
                    base.originalStringField = new LenientStringConstructor(this.functionPrototype, new LenientStringPrototype(this.functionPrototype, this.objectPrototype));
                }
                return base.originalStringField;
            }
        }

        internal override ErrorConstructor originalSyntaxError
        {
            get
            {
                if (base.originalSyntaxErrorField == null)
                {
                    base.originalSyntaxErrorField = new ErrorConstructor("SyntaxError", ErrorType.SyntaxError, this.originalError, this);
                }
                return base.originalSyntaxErrorField;
            }
        }

        internal override ErrorConstructor originalTypeError
        {
            get
            {
                if (base.originalTypeErrorField == null)
                {
                    base.originalTypeErrorField = new ErrorConstructor("TypeError", ErrorType.TypeError, this.originalError, this);
                }
                return base.originalTypeErrorField;
            }
        }

        internal override ErrorConstructor originalURIError
        {
            get
            {
                if (base.originalURIErrorField == null)
                {
                    base.originalURIErrorField = new ErrorConstructor("URIError", ErrorType.URIError, this.originalError, this);
                }
                return base.originalURIErrorField;
            }
        }

        internal override VBArrayConstructor originalVBArray
        {
            get
            {
                if (base.originalVBArrayField == null)
                {
                    base.originalVBArrayField = new VBArrayConstructor(this.functionPrototype, new LenientVBArrayPrototype(this.functionPrototype, this.objectPrototype));
                }
                return base.originalVBArrayField;
            }
        }

        public object RangeError
        {
            get
            {
                if (this.RangeErrorField is Missing)
                {
                    this.RangeErrorField = this.originalRangeError;
                }
                return this.RangeErrorField;
            }
            set
            {
                this.RangeErrorField = value;
            }
        }

        public object ReferenceError
        {
            get
            {
                if (this.ReferenceErrorField is Missing)
                {
                    this.ReferenceErrorField = this.originalReferenceError;
                }
                return this.ReferenceErrorField;
            }
            set
            {
                this.ReferenceErrorField = value;
            }
        }

        public object RegExp
        {
            get
            {
                if (this.RegExpField is Missing)
                {
                    this.RegExpField = this.originalRegExp;
                }
                return this.RegExpField;
            }
            set
            {
                this.RegExpField = value;
            }
        }

        public object String
        {
            get
            {
                if (this.StringField is Missing)
                {
                    this.StringField = this.originalString;
                }
                return this.StringField;
            }
            set
            {
                this.StringField = value;
            }
        }

        public object SyntaxError
        {
            get
            {
                if (this.SyntaxErrorField is Missing)
                {
                    this.SyntaxErrorField = this.originalSyntaxError;
                }
                return this.SyntaxErrorField;
            }
            set
            {
                this.SyntaxErrorField = value;
            }
        }

        public object TypeError
        {
            get
            {
                if (this.TypeErrorField is Missing)
                {
                    this.TypeErrorField = this.originalTypeError;
                }
                return this.TypeErrorField;
            }
            set
            {
                this.TypeErrorField = value;
            }
        }

        public object URIError
        {
            get
            {
                if (this.URIErrorField is Missing)
                {
                    this.URIErrorField = this.originalURIError;
                }
                return this.URIErrorField;
            }
            set
            {
                this.URIErrorField = value;
            }
        }

        public object VBArray
        {
            get
            {
                if (this.VBArrayField is Missing)
                {
                    this.VBArrayField = this.originalVBArray;
                }
                return this.VBArrayField;
            }
            set
            {
                this.VBArrayField = value;
            }
        }
    }
}

