namespace Microsoft.JScript
{
    using System;
    using System.Text.RegularExpressions;

    public sealed class RegExpConstructor : ScriptFunction
    {
        internal ArrayPrototype arrayPrototype;
        internal object inputString;
        private string lastInput;
        private Match lastRegexMatch;
        internal static readonly RegExpConstructor ob = new RegExpConstructor();
        private RegExpPrototype originalPrototype;
        private Regex regex;

        internal RegExpConstructor() : base(FunctionPrototype.ob, "RegExp", 2)
        {
            this.originalPrototype = RegExpPrototype.ob;
            RegExpPrototype._constructor = this;
            base.proto = RegExpPrototype.ob;
            this.arrayPrototype = ArrayPrototype.ob;
            this.regex = null;
            this.lastRegexMatch = null;
            this.inputString = "";
            this.lastInput = null;
        }

        internal RegExpConstructor(LenientFunctionPrototype parent, LenientRegExpPrototype prototypeProp, LenientArrayPrototype arrayPrototype) : base(parent, "RegExp", 2)
        {
            this.originalPrototype = prototypeProp;
            prototypeProp.constructor = this;
            base.proto = prototypeProp;
            this.arrayPrototype = arrayPrototype;
            this.regex = null;
            this.lastRegexMatch = null;
            this.inputString = "";
            this.lastInput = null;
            base.noExpando = false;
        }

        internal override object Call(object[] args, object thisob)
        {
            return this.Invoke(args);
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        public object Construct(string pattern, bool ignoreCase, bool global, bool multiline)
        {
            return new RegExpObject(this.originalPrototype, pattern, ignoreCase, global, multiline, this);
        }

        private RegExpObject ConstructNew(object[] args)
        {
            string source = ((args.Length > 0) && (args[0] != null)) ? Microsoft.JScript.Convert.ToString(args[0]) : "";
            if ((args.Length > 0) && (args[0] is Regex))
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            bool ignoreCase = false;
            bool global = false;
            bool multiline = false;
            if ((args.Length >= 2) && (args[1] != null))
            {
                string str2 = Microsoft.JScript.Convert.ToString(args[1]);
                for (int i = 0; i < str2.Length; i++)
                {
                    switch (str2[i])
                    {
                        case 'g':
                        {
                            global = true;
                            continue;
                        }
                        case 'i':
                        {
                            ignoreCase = true;
                            continue;
                        }
                        case 'm':
                        {
                            multiline = true;
                            continue;
                        }
                    }
                    throw new JScriptException(JSError.RegExpSyntax);
                }
            }
            return new RegExpObject(this.originalPrototype, source, ignoreCase, global, multiline, this);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public RegExpObject CreateInstance(params object[] args)
        {
            RegExpObject obj2;
            if (((args == null) || (args.Length <= 0)) || ((obj2 = args[0] as RegExpObject) == null))
            {
                return this.ConstructNew(args);
            }
            if ((args.Length > 1) && (args[1] != null))
            {
                throw new JScriptException(JSError.RegExpSyntax);
            }
            return new RegExpObject(this.originalPrototype, obj2.source, obj2.ignoreCase, obj2.global, obj2.multiline, this);
        }

        private object GetIndex()
        {
            return ((this.lastRegexMatch == null) ? -1 : this.lastRegexMatch.Index);
        }

        private object GetInput()
        {
            return this.inputString;
        }

        private object GetLastIndex()
        {
            return ((this.lastRegexMatch == null) ? -1 : ((this.lastRegexMatch.Length == 0) ? (this.lastRegexMatch.Index + 1) : (this.lastRegexMatch.Index + this.lastRegexMatch.Length)));
        }

        private object GetLastMatch()
        {
            if (this.lastRegexMatch != null)
            {
                return this.lastRegexMatch.ToString();
            }
            return "";
        }

        private object GetLastParen()
        {
            if ((this.regex == null) || (this.lastRegexMatch == null))
            {
                return "";
            }
            string[] groupNames = this.regex.GetGroupNames();
            if (groupNames.Length <= 1)
            {
                return "";
            }
            int num = this.regex.GroupNumberFromName(groupNames[groupNames.Length - 1]);
            Group group = this.lastRegexMatch.Groups[num];
            if (!group.Success)
            {
                return "";
            }
            return group.ToString();
        }

        private object GetLeftContext()
        {
            if ((this.lastRegexMatch != null) && (this.lastInput != null))
            {
                return this.lastInput.Substring(0, this.lastRegexMatch.Index);
            }
            return "";
        }

        internal override object GetMemberValue(string name)
        {
            if ((name.Length == 2) && (name[0] == '$'))
            {
                char ch = name[1];
                switch (ch)
                {
                    case '&':
                        return this.GetLastMatch();

                    case '\'':
                        return this.GetRightContext();

                    case '+':
                        return this.GetLastParen();

                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (this.lastRegexMatch != null)
                        {
                            Group group = this.lastRegexMatch.Groups[ch.ToString()];
                            if (!group.Success)
                            {
                                return "";
                            }
                            return group.ToString();
                        }
                        return "";

                    case '_':
                        return this.GetInput();

                    case '`':
                        return this.GetLeftContext();
                }
            }
            return base.GetMemberValue(name);
        }

        private object GetRightContext()
        {
            if ((this.lastRegexMatch != null) && (this.lastInput != null))
            {
                return this.lastInput.Substring(this.lastRegexMatch.Index + this.lastRegexMatch.Length);
            }
            return "";
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public RegExpObject Invoke(params object[] args)
        {
            RegExpObject obj2;
            if (((args == null) || (args.Length <= 0)) || ((obj2 = args[0] as RegExpObject) == null))
            {
                return this.ConstructNew(args);
            }
            if ((args.Length > 1) && (args[1] != null))
            {
                throw new JScriptException(JSError.RegExpSyntax);
            }
            return obj2;
        }

        private void SetInput(object value)
        {
            this.inputString = value;
        }

        internal override void SetMemberValue(string name, object value)
        {
            if (base.noExpando)
            {
                throw new JScriptException(JSError.AssignmentToReadOnly);
            }
            if ((name.Length == 2) && (name[0] == '$'))
            {
                switch (name[1])
                {
                    case '&':
                    case '\'':
                    case '+':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '`':
                        return;

                    case '_':
                        this.SetInput(value);
                        return;
                }
            }
            base.SetMemberValue(name, value);
        }

        internal int UpdateConstructor(Regex regex, Match match, string input)
        {
            if (!base.noExpando)
            {
                this.regex = regex;
                this.lastRegexMatch = match;
                this.inputString = input;
                this.lastInput = input;
            }
            if (match.Length != 0)
            {
                return (match.Index + match.Length);
            }
            return (match.Index + 1);
        }

        public object index
        {
            get
            {
                return this.GetIndex();
            }
        }

        public object input
        {
            get
            {
                return this.GetInput();
            }
            set
            {
                if (base.noExpando)
                {
                    throw new JScriptException(JSError.AssignmentToReadOnly);
                }
                this.SetInput(value);
            }
        }

        public object lastIndex
        {
            get
            {
                return this.GetLastIndex();
            }
        }

        public object lastMatch
        {
            get
            {
                return this.GetLastMatch();
            }
        }

        public object lastParen
        {
            get
            {
                return this.GetLastParen();
            }
        }

        public object leftContext
        {
            get
            {
                return this.GetLeftContext();
            }
        }

        public object rightContext
        {
            get
            {
                return this.GetRightContext();
            }
        }
    }
}

