namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public class JScriptException : ApplicationException, IVsaFullErrorInfo, IJSVsaError
    {
        private int code;
        [NonSerialized]
        internal Context context;
        internal static readonly string ContextStringDelimiter = ";;";
        internal bool isError;
        internal object value;

        public JScriptException() : this(JSError.NoError)
        {
        }

        public JScriptException(JSError errorNumber) : this(errorNumber, null)
        {
        }

        public JScriptException(string m) : this(m, null)
        {
        }

        internal JScriptException(JSError errorNumber, Context context)
        {
            this.value = Missing.Value;
            this.context = context;
            this.code = base.HResult = (int) (0x800a0000L + ((long) errorNumber));
        }

        internal JScriptException(Exception e, Context context) : this(null, e, context)
        {
        }

        internal JScriptException(object value, Context context)
        {
            this.value = value;
            this.context = context;
            this.code = base.HResult = -2146823266;
        }

        protected JScriptException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.code = base.HResult = info.GetInt32("Code");
            this.value = Missing.Value;
            this.isError = info.GetBoolean("IsError");
        }

        public JScriptException(string m, Exception e) : this(m, e, null)
        {
        }

        internal JScriptException(string m, Exception e, Context context) : base(m, e)
        {
            this.value = e;
            this.context = context;
            if (e is StackOverflowException)
            {
                this.code = base.HResult = -2146828260;
                this.value = Missing.Value;
            }
            else if (e is OutOfMemoryException)
            {
                this.code = base.HResult = -2146828281;
                this.value = Missing.Value;
            }
            else if (e is ExternalException)
            {
                this.code = base.HResult = ((ExternalException) e).ErrorCode;
                if (((base.HResult & 0xffff0000L) == 0x800a0000L) && Enum.IsDefined(typeof(JSError), base.HResult & 0xffff))
                {
                    this.value = Missing.Value;
                }
            }
            else
            {
                int hRForException = Marshal.GetHRForException(e);
                if (((hRForException & 0xffff0000L) == 0x800a0000L) && Enum.IsDefined(typeof(JSError), hRForException & 0xffff))
                {
                    this.code = base.HResult = hRForException;
                    this.value = Missing.Value;
                }
                else
                {
                    this.code = base.HResult = -2146823266;
                }
            }
        }

        internal ErrorType GetErrorType()
        {
            int hResult = base.HResult;
            if ((hResult & 0xffff0000L) == 0x800a0000L)
            {
                switch ((((JSError) hResult) & ((JSError) 0xffff)))
                {
                    case JSError.SyntaxError:
                        return ErrorType.SyntaxError;

                    case JSError.NoColon:
                        return ErrorType.SyntaxError;

                    case JSError.NoSemicolon:
                        return ErrorType.SyntaxError;

                    case JSError.NoLeftParen:
                        return ErrorType.SyntaxError;

                    case JSError.NoRightParen:
                        return ErrorType.SyntaxError;

                    case JSError.NoRightBracket:
                        return ErrorType.SyntaxError;

                    case JSError.NoLeftCurly:
                        return ErrorType.SyntaxError;

                    case JSError.NoRightCurly:
                        return ErrorType.SyntaxError;

                    case JSError.NoIdentifier:
                        return ErrorType.SyntaxError;

                    case JSError.NoEqual:
                        return ErrorType.SyntaxError;

                    case JSError.IllegalChar:
                        return ErrorType.SyntaxError;

                    case JSError.UnterminatedString:
                        return ErrorType.SyntaxError;

                    case JSError.NoCommentEnd:
                        return ErrorType.SyntaxError;

                    case JSError.BadReturn:
                        return ErrorType.SyntaxError;

                    case JSError.BadBreak:
                        return ErrorType.SyntaxError;

                    case JSError.BadContinue:
                        return ErrorType.SyntaxError;

                    case JSError.BadHexDigit:
                        return ErrorType.SyntaxError;

                    case JSError.NoWhile:
                        return ErrorType.SyntaxError;

                    case JSError.BadLabel:
                        return ErrorType.SyntaxError;

                    case JSError.NoLabel:
                        return ErrorType.SyntaxError;

                    case JSError.DupDefault:
                        return ErrorType.SyntaxError;

                    case JSError.NoMemberIdentifier:
                        return ErrorType.SyntaxError;

                    case JSError.NoCcEnd:
                        return ErrorType.SyntaxError;

                    case JSError.CcOff:
                        return ErrorType.SyntaxError;

                    case JSError.NotConst:
                        return ErrorType.SyntaxError;

                    case JSError.NoAt:
                        return ErrorType.SyntaxError;

                    case JSError.NoCatch:
                        return ErrorType.SyntaxError;

                    case JSError.InvalidElse:
                        return ErrorType.SyntaxError;

                    case JSError.NotCollection:
                        return ErrorType.TypeError;

                    case JSError.OLENoPropOrMethod:
                        return ErrorType.TypeError;

                    case JSError.InvalidCall:
                        return ErrorType.TypeError;

                    case JSError.TypeMismatch:
                        return ErrorType.TypeError;

                    case JSError.NoComma:
                        return ErrorType.SyntaxError;

                    case JSError.BadSwitch:
                        return ErrorType.SyntaxError;

                    case JSError.CcInvalidEnd:
                        return ErrorType.SyntaxError;

                    case JSError.CcInvalidElse:
                        return ErrorType.SyntaxError;

                    case JSError.CcInvalidElif:
                        return ErrorType.SyntaxError;

                    case JSError.ErrEOF:
                        return ErrorType.SyntaxError;

                    case JSError.ClassNotAllowed:
                        return ErrorType.SyntaxError;

                    case JSError.NeedCompileTimeConstant:
                        return ErrorType.ReferenceError;

                    case JSError.NeedType:
                        return ErrorType.TypeError;

                    case JSError.NotInsideClass:
                        return ErrorType.SyntaxError;

                    case JSError.InvalidPositionDirective:
                        return ErrorType.SyntaxError;

                    case JSError.MustBeEOL:
                        return ErrorType.SyntaxError;

                    case JSError.WrongDirective:
                        return ErrorType.SyntaxError;

                    case JSError.CannotNestPositionDirective:
                        return ErrorType.SyntaxError;

                    case JSError.CircularDefinition:
                        return ErrorType.SyntaxError;

                    case JSError.NotAccessible:
                        return ErrorType.ReferenceError;

                    case JSError.NeedInterface:
                        return ErrorType.TypeError;

                    case JSError.UnreachableCatch:
                        return ErrorType.SyntaxError;

                    case JSError.TypeCannotBeExtended:
                        return ErrorType.ReferenceError;

                    case JSError.UndeclaredVariable:
                        return ErrorType.ReferenceError;

                    case JSError.KeywordUsedAsIdentifier:
                        return ErrorType.SyntaxError;

                    case JSError.InvalidCustomAttribute:
                        return ErrorType.TypeError;

                    case JSError.InvalidCustomAttributeArgument:
                        return ErrorType.TypeError;

                    case JSError.InvalidCustomAttributeClassOrCtor:
                        return ErrorType.TypeError;

                    case JSError.NoSuchMember:
                        return ErrorType.ReferenceError;

                    case JSError.ItemNotAllowedOnExpandoClass:
                        return ErrorType.SyntaxError;

                    case JSError.NotIndexable:
                        return ErrorType.TypeError;

                    case JSError.StaticMissingInStaticInit:
                        return ErrorType.SyntaxError;

                    case JSError.MissingConstructForAttributes:
                        return ErrorType.SyntaxError;

                    case JSError.OnlyClassesAllowed:
                        return ErrorType.SyntaxError;

                    case JSError.PackageExpected:
                        return ErrorType.SyntaxError;

                    case JSError.DifferentReturnTypeFromBase:
                        return ErrorType.TypeError;

                    case JSError.ClashWithProperty:
                        return ErrorType.SyntaxError;

                    case JSError.CannotReturnValueFromVoidFunction:
                        return ErrorType.TypeError;

                    case JSError.AmbiguousMatch:
                        return ErrorType.ReferenceError;

                    case JSError.AmbiguousConstructorCall:
                        return ErrorType.ReferenceError;

                    case JSError.SuperClassConstructorNotAccessible:
                        return ErrorType.ReferenceError;

                    case JSError.NoCommaOrTypeDefinitionError:
                        return ErrorType.SyntaxError;

                    case JSError.AbstractWithBody:
                        return ErrorType.SyntaxError;

                    case JSError.NoRightParenOrComma:
                        return ErrorType.SyntaxError;

                    case JSError.NoRightBracketOrComma:
                        return ErrorType.SyntaxError;

                    case JSError.ExpressionExpected:
                        return ErrorType.SyntaxError;

                    case JSError.UnexpectedSemicolon:
                        return ErrorType.SyntaxError;

                    case JSError.TooManyTokensSkipped:
                        return ErrorType.SyntaxError;

                    case JSError.BadVariableDeclaration:
                        return ErrorType.SyntaxError;

                    case JSError.BadFunctionDeclaration:
                        return ErrorType.SyntaxError;

                    case JSError.BadPropertyDeclaration:
                        return ErrorType.SyntaxError;

                    case JSError.DoesNotHaveAnAddress:
                        return ErrorType.ReferenceError;

                    case JSError.TooFewParameters:
                        return ErrorType.TypeError;

                    case JSError.ImpossibleConversion:
                        return ErrorType.TypeError;

                    case JSError.NeedInstance:
                        return ErrorType.ReferenceError;

                    case JSError.InvalidBaseTypeForEnum:
                        return ErrorType.TypeError;

                    case JSError.CannotInstantiateAbstractClass:
                        return ErrorType.TypeError;

                    case JSError.ShouldBeAbstract:
                        return ErrorType.SyntaxError;

                    case JSError.BadModifierInInterface:
                        return ErrorType.SyntaxError;

                    case JSError.VarIllegalInInterface:
                        return ErrorType.SyntaxError;

                    case JSError.InterfaceIllegalInInterface:
                        return ErrorType.SyntaxError;

                    case JSError.NoVarInEnum:
                        return ErrorType.SyntaxError;

                    case JSError.EnumNotAllowed:
                        return ErrorType.SyntaxError;

                    case JSError.PackageInWrongContext:
                        return ErrorType.SyntaxError;

                    case JSError.ConstructorMayNotHaveReturnType:
                        return ErrorType.SyntaxError;

                    case JSError.OnlyClassesAndPackagesAllowed:
                        return ErrorType.SyntaxError;

                    case JSError.InvalidDebugDirective:
                        return ErrorType.SyntaxError;

                    case JSError.NestedInstanceTypeCannotBeExtendedByStatic:
                        return ErrorType.ReferenceError;

                    case JSError.PropertyLevelAttributesMustBeOnGetter:
                        return ErrorType.ReferenceError;

                    case JSError.ParamListNotLast:
                        return ErrorType.SyntaxError;

                    case JSError.InstanceNotAccessibleFromStatic:
                        return ErrorType.ReferenceError;

                    case JSError.StaticRequiresTypeName:
                        return ErrorType.ReferenceError;

                    case JSError.NonStaticWithTypeName:
                        return ErrorType.ReferenceError;

                    case JSError.NoSuchStaticMember:
                        return ErrorType.ReferenceError;

                    case JSError.ExpectedAssembly:
                        return ErrorType.SyntaxError;

                    case JSError.AssemblyAttributesMustBeGlobal:
                        return ErrorType.SyntaxError;

                    case JSError.DuplicateMethod:
                        return ErrorType.TypeError;

                    case JSError.NotAnExpandoFunction:
                        return ErrorType.ReferenceError;

                    case JSError.CcInvalidInDebugger:
                        return ErrorType.SyntaxError;

                    case JSError.TypeNameTooLong:
                        return ErrorType.SyntaxError;

                    case JSError.MemberInitializerCannotContainFuncExpr:
                        return ErrorType.SyntaxError;

                    case JSError.CantAssignThis:
                        return ErrorType.ReferenceError;

                    case JSError.NumberExpected:
                        return ErrorType.TypeError;

                    case JSError.FunctionExpected:
                        return ErrorType.TypeError;

                    case JSError.StringExpected:
                        return ErrorType.TypeError;

                    case JSError.DateExpected:
                        return ErrorType.TypeError;

                    case JSError.ObjectExpected:
                        return ErrorType.TypeError;

                    case JSError.IllegalAssignment:
                        return ErrorType.ReferenceError;

                    case JSError.UndefinedIdentifier:
                        return ErrorType.ReferenceError;

                    case JSError.BooleanExpected:
                        return ErrorType.TypeError;

                    case JSError.VBArrayExpected:
                        return ErrorType.TypeError;

                    case JSError.EnumeratorExpected:
                        return ErrorType.TypeError;

                    case JSError.RegExpExpected:
                        return ErrorType.TypeError;

                    case JSError.RegExpSyntax:
                        return ErrorType.SyntaxError;

                    case JSError.InvalidPrototype:
                        return ErrorType.TypeError;

                    case JSError.URIEncodeError:
                        return ErrorType.URIError;

                    case JSError.URIDecodeError:
                        return ErrorType.URIError;

                    case JSError.FractionOutOfRange:
                        return ErrorType.RangeError;

                    case JSError.PrecisionOutOfRange:
                        return ErrorType.RangeError;

                    case JSError.ArrayLengthConstructIncorrect:
                        return ErrorType.RangeError;

                    case JSError.ArrayLengthAssignIncorrect:
                        return ErrorType.RangeError;

                    case JSError.NeedArrayObject:
                        return ErrorType.TypeError;

                    case JSError.NoConstructor:
                        return ErrorType.TypeError;

                    case JSError.IllegalEval:
                        return ErrorType.EvalError;

                    case JSError.MustProvideNameForNamedParameter:
                        return ErrorType.ReferenceError;

                    case JSError.DuplicateNamedParameter:
                        return ErrorType.ReferenceError;

                    case JSError.MissingNameParameter:
                        return ErrorType.ReferenceError;

                    case JSError.MoreNamedParametersThanArguments:
                        return ErrorType.ReferenceError;

                    case JSError.AssignmentToReadOnly:
                        return ErrorType.ReferenceError;

                    case JSError.WriteOnlyProperty:
                        return ErrorType.ReferenceError;

                    case JSError.IncorrectNumberOfIndices:
                        return ErrorType.ReferenceError;
                }
            }
            return ErrorType.OtherError;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("IsError", this.isError);
            info.AddValue("Code", this.code);
        }

        internal static string Localize(string key, CultureInfo culture)
        {
            return Localize(key, null, culture);
        }

        internal static string Localize(string key, string context, CultureInfo culture)
        {
            try
            {
                string str = new ResourceManager("Microsoft.JScript", typeof(JScriptException).Module.Assembly).GetString(key, culture);
                if (str == null)
                {
                    return key;
                }
                int index = str.IndexOf(ContextStringDelimiter);
                if (index == -1)
                {
                    return str;
                }
                if (context == null)
                {
                    return str.Substring(0, index);
                }
                return string.Format(culture, str.Substring(index + 2), new object[] { context });
            }
            catch (MissingManifestResourceException)
            {
            }
            return key;
        }

        public int Column
        {
            get
            {
                if (this.context != null)
                {
                    return ((this.context.StartColumn + this.context.document.startCol) + 1);
                }
                return 0;
            }
        }

        public string Description
        {
            get
            {
                return this.Message;
            }
        }

        public int EndColumn
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.context != null)
                {
                    return ((this.context.EndColumn + this.context.document.startCol) + 1);
                }
                return 0;
            }
        }

        public int EndLine
        {
            get
            {
                if (this.context != null)
                {
                    return ((this.context.EndLine + this.context.document.startLine) - this.context.document.lastLineInSource);
                }
                return 0;
            }
        }

        public int ErrorNumber
        {
            get
            {
                return base.HResult;
            }
        }

        public int Line
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.context != null)
                {
                    return ((this.context.StartLine + this.context.document.startLine) - this.context.document.lastLineInSource);
                }
                return 0;
            }
        }

        public string LineText
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.context != null)
                {
                    return this.context.source_string;
                }
                return "";
            }
        }

        public override string Message
        {
            get
            {
                if (this.value is Exception)
                {
                    Exception exception = (Exception) this.value;
                    string message = exception.Message;
                    if ((message != null) && (message.Length > 0))
                    {
                        return message;
                    }
                    return exception.ToString();
                }
                string key = (base.HResult & 0xffff).ToString(CultureInfo.InvariantCulture);
                CultureInfo culture = null;
                if ((this.context != null) && (this.context.document != null))
                {
                    VsaEngine engine = this.context.document.engine;
                    if (engine != null)
                    {
                        culture = engine.ErrorCultureInfo;
                    }
                }
                if (this.value is ErrorObject)
                {
                    string str3 = ((ErrorObject) this.value).Message;
                    if ((str3 != null) && (str3.Length > 0))
                    {
                        return str3;
                    }
                    return (Localize("No description available", culture) + ": " + key);
                }
                if (!(this.value is string))
                {
                    if (this.context != null)
                    {
                        switch ((((JSError) base.HResult) & ((JSError) 0xffff)))
                        {
                            case JSError.DuplicateName:
                            case JSError.NotAccessible:
                            case JSError.UndeclaredVariable:
                            case JSError.VariableLeftUninitialized:
                            case JSError.KeywordUsedAsIdentifier:
                            case JSError.NotMeantToBeCalledDirectly:
                            case JSError.AmbiguousBindingBecauseOfWith:
                            case JSError.AmbiguousBindingBecauseOfEval:
                            case JSError.NotDeletable:
                            case JSError.VariableMightBeUnitialized:
                            case JSError.NeedInstance:
                            case JSError.InstanceNotAccessibleFromStatic:
                            case JSError.StaticRequiresTypeName:
                            case JSError.NonStaticWithTypeName:
                            case JSError.ObjectExpected:
                            case JSError.UndefinedIdentifier:
                            case JSError.AssignmentToReadOnly:
                                goto Label_0312;
                        }
                    }
                    int num2 = base.HResult & 0xffff;
                    return Localize(num2.ToString(CultureInfo.InvariantCulture), culture);
                }
                switch ((((JSError) base.HResult) & ((JSError) 0xffff)))
                {
                    case JSError.Deprecated:
                    case JSError.MustImplementMethod:
                    case JSError.TypeMismatch:
                    case JSError.DuplicateName:
                    case JSError.TypeCannotBeExtended:
                    case JSError.NoSuchMember:
                    case JSError.HidesParentMember:
                    case JSError.HidesAbstractInBase:
                    case JSError.NotIndexable:
                    case JSError.InvalidCustomAttributeTarget:
                    case JSError.NoSuchType:
                    case JSError.DifferentReturnTypeFromBase:
                    case JSError.CannotBeAbstract:
                    case JSError.NoSuchStaticMember:
                    case JSError.ImplicitlyReferencedAssemblyNotFound:
                    case JSError.IncompatibleAssemblyReference:
                    case JSError.InvalidAssemblyKeyFile:
                    case JSError.TypeNameTooLong:
                    case JSError.InvalidResource:
                        return Localize(key, (string) this.value, culture);

                    default:
                        return (string) this.value;
                }
            Label_0312:
                return Localize(key, this.context.GetCode(), culture);
            }
        }

        string IJSVsaError.Description
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                return this.Description;
            }
        }

        int IJSVsaError.Number
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                return this.Number;
            }
        }

        public int Number
        {
            get
            {
                return this.ErrorNumber;
            }
        }

        public int Severity
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                int hResult = base.HResult;
                if (((hResult & 0xffff0000L) == 0x800a0000L) && !this.isError)
                {
                    switch ((((JSError) hResult) & ((JSError) 0xffff)))
                    {
                        case JSError.UndeclaredVariable:
                            return 3;

                        case JSError.VariableLeftUninitialized:
                            return 3;

                        case JSError.KeywordUsedAsIdentifier:
                            return 2;

                        case JSError.NotMeantToBeCalledDirectly:
                            return 1;

                        case JSError.GetAndSetAreInconsistent:
                            return 1;

                        case JSError.Deprecated:
                            return 2;

                        case JSError.DuplicateName:
                            return 1;

                        case JSError.DupVisibility:
                            return 1;

                        case JSError.IncompatibleVisibility:
                            return 1;

                        case JSError.TooManyParameters:
                            return 1;

                        case JSError.AmbiguousBindingBecauseOfWith:
                            return 4;

                        case JSError.AmbiguousBindingBecauseOfEval:
                            return 4;

                        case JSError.BaseClassIsExpandoAlready:
                            return 1;

                        case JSError.UselessExpression:
                            return 1;

                        case JSError.HidesParentMember:
                            return 1;

                        case JSError.NewNotSpecifiedInMethodDeclaration:
                            return 1;

                        case JSError.DifferentReturnTypeFromBase:
                            return 1;

                        case JSError.NotDeletable:
                            return 1;

                        case JSError.ArrayMayBeCopied:
                            return 1;

                        case JSError.ShouldBeAbstract:
                            return 1;

                        case JSError.BadOctalLiteral:
                            return 1;

                        case JSError.OctalLiteralsAreDeprecated:
                            return 2;

                        case JSError.VariableMightBeUnitialized:
                            return 3;

                        case JSError.BadWayToLeaveFinally:
                            return 3;

                        case JSError.TooFewParameters:
                            return 1;

                        case JSError.UselessAssignment:
                            return 1;

                        case JSError.SuspectAssignment:
                            return 1;

                        case JSError.SuspectSemicolon:
                            return 1;

                        case JSError.SuspectLoopCondition:
                            return 1;

                        case JSError.StringConcatIsSlow:
                            return 3;

                        case JSError.PossibleBadConversion:
                            return 1;

                        case JSError.PossibleBadConversionFromString:
                            return 4;

                        case JSError.IncompatibleAssemblyReference:
                            return 1;

                        case JSError.AssignmentToReadOnly:
                            return 1;
                    }
                }
                return 0;
            }
        }

        public IJSVsaItem SourceItem
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.context == null)
                {
                    throw new NoContextException();
                }
                return this.context.document.sourceItem;
            }
        }

        public string SourceMoniker
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.context != null)
                {
                    return this.context.document.documentName;
                }
                return "no source";
            }
        }

        public override string StackTrace
        {
            get
            {
                if (this.context == null)
                {
                    return (this.Message + Environment.NewLine + base.StackTrace);
                }
                StringBuilder builder = new StringBuilder();
                Context context = this.context;
                string documentName = context.document.documentName;
                if ((documentName != null) && (documentName.Length > 0))
                {
                    builder.Append(documentName + ": ");
                }
                CultureInfo culture = null;
                if ((this.context != null) && (this.context.document != null))
                {
                    VsaEngine engine = this.context.document.engine;
                    if (engine != null)
                    {
                        culture = engine.ErrorCultureInfo;
                    }
                }
                builder.Append(Localize("Line", culture));
                builder.Append(' ');
                builder.Append(context.StartLine);
                builder.Append(" - ");
                builder.Append(Localize("Error", culture));
                builder.Append(": ");
                builder.Append(this.Message);
                builder.Append(Environment.NewLine);
                if (context.document.engine != null)
                {
                    Stack callContextStack = context.document.engine.Globals.CallContextStack;
                    int i = 0;
                    int num2 = callContextStack.Size();
                    while (i < num2)
                    {
                        CallContext context2 = (CallContext) callContextStack.Peek(i);
                        builder.Append("    ");
                        builder.Append(Localize("at call to", culture));
                        builder.Append(context2.FunctionName());
                        builder.Append(' ');
                        builder.Append(Localize("in line", culture));
                        builder.Append(": ");
                        builder.Append(context2.sourceContext.EndLine);
                        i++;
                    }
                }
                return builder.ToString();
            }
        }

        public int StartColumn
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                return this.Column;
            }
        }
    }
}

