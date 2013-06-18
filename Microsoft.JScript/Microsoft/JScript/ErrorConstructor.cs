namespace Microsoft.JScript
{
    using System;

    public sealed class ErrorConstructor : ScriptFunction
    {
        internal static readonly ErrorConstructor evalOb = new ErrorConstructor("EvalError", ErrorType.EvalError);
        private GlobalObject globalObject;
        internal static readonly ErrorConstructor ob = new ErrorConstructor();
        private ErrorPrototype originalPrototype;
        internal static readonly ErrorConstructor rangeOb = new ErrorConstructor("RangeError", ErrorType.RangeError);
        internal static readonly ErrorConstructor referenceOb = new ErrorConstructor("ReferenceError", ErrorType.ReferenceError);
        internal static readonly ErrorConstructor syntaxOb = new ErrorConstructor("SyntaxError", ErrorType.SyntaxError);
        private ErrorType type;
        internal static readonly ErrorConstructor typeOb = new ErrorConstructor("TypeError", ErrorType.TypeError);
        internal static readonly ErrorConstructor uriOb = new ErrorConstructor("URIError", ErrorType.URIError);

        internal ErrorConstructor() : base(ErrorPrototype.ob, "Error", 2)
        {
            this.originalPrototype = ErrorPrototype.ob;
            ErrorPrototype.ob._constructor = this;
            base.proto = ErrorPrototype.ob;
            this.type = ErrorType.OtherError;
            this.globalObject = GlobalObject.commonInstance;
        }

        internal ErrorConstructor(string subtypeName, ErrorType type) : base(ob.parent, subtypeName, 2)
        {
            this.originalPrototype = new ErrorPrototype(ob.originalPrototype, subtypeName);
            this.originalPrototype._constructor = this;
            base.proto = this.originalPrototype;
            this.type = type;
            this.globalObject = GlobalObject.commonInstance;
        }

        internal ErrorConstructor(LenientFunctionPrototype parent, LenientErrorPrototype prototypeProp, GlobalObject globalObject) : base(parent, "Error", 2)
        {
            this.originalPrototype = prototypeProp;
            prototypeProp.constructor = this;
            base.proto = prototypeProp;
            this.type = ErrorType.OtherError;
            this.globalObject = globalObject;
            base.noExpando = false;
        }

        internal ErrorConstructor(string subtypeName, ErrorType type, ErrorConstructor error, GlobalObject globalObject) : base(error.parent, subtypeName, 2)
        {
            this.originalPrototype = new LenientErrorPrototype((LenientFunctionPrototype) error.parent, error.originalPrototype, subtypeName);
            base.noExpando = false;
            this.originalPrototype._constructor = this;
            base.proto = this.originalPrototype;
            this.type = type;
            this.globalObject = globalObject;
            base.noExpando = false;
        }

        internal override object Call(object[] args, object thisob)
        {
            return this.Construct(args);
        }

        internal ErrorObject Construct(object e)
        {
            if (!(e is JScriptException) || (this != this.globalObject.originalError))
            {
                switch (this.type)
                {
                    case ErrorType.EvalError:
                        return new EvalErrorObject(this.originalPrototype, e);

                    case ErrorType.RangeError:
                        return new RangeErrorObject(this.originalPrototype, e);

                    case ErrorType.ReferenceError:
                        return new ReferenceErrorObject(this.originalPrototype, e);

                    case ErrorType.SyntaxError:
                        return new SyntaxErrorObject(this.originalPrototype, e);

                    case ErrorType.TypeError:
                        return new TypeErrorObject(this.originalPrototype, e);

                    case ErrorType.URIError:
                        return new URIErrorObject(this.originalPrototype, e);
                }
                return new ErrorObject(this.originalPrototype, e);
            }
            switch (((JScriptException) e).GetErrorType())
            {
                case ErrorType.EvalError:
                    return this.globalObject.originalEvalError.Construct(e);

                case ErrorType.RangeError:
                    return this.globalObject.originalRangeError.Construct(e);

                case ErrorType.ReferenceError:
                    return this.globalObject.originalReferenceError.Construct(e);

                case ErrorType.SyntaxError:
                    return this.globalObject.originalSyntaxError.Construct(e);

                case ErrorType.TypeError:
                    return this.globalObject.originalTypeError.Construct(e);

                case ErrorType.URIError:
                    return this.globalObject.originalURIError.Construct(e);
            }
            return new ErrorObject(this.originalPrototype, e);
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public ErrorObject CreateInstance(params object[] args)
        {
            switch (this.type)
            {
                case ErrorType.EvalError:
                    return new EvalErrorObject(this.originalPrototype, args);

                case ErrorType.RangeError:
                    return new RangeErrorObject(this.originalPrototype, args);

                case ErrorType.ReferenceError:
                    return new ReferenceErrorObject(this.originalPrototype, args);

                case ErrorType.SyntaxError:
                    return new SyntaxErrorObject(this.originalPrototype, args);

                case ErrorType.TypeError:
                    return new TypeErrorObject(this.originalPrototype, args);

                case ErrorType.URIError:
                    return new URIErrorObject(this.originalPrototype, args);
            }
            return new ErrorObject(this.originalPrototype, args);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public object Invoke(params object[] args)
        {
            return this.CreateInstance(args);
        }
    }
}

