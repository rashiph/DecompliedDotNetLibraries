namespace System.Text.RegularExpressions
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;

    internal abstract class RegexCompiler
    {
        internal int _anchors;
        internal int _backpos;
        internal Label _backtrack;
        internal RegexBoyerMoore _bmPrefix;
        internal static MethodInfo _captureM;
        internal static MethodInfo _charInSetM;
        internal static MethodInfo _chartolowerM;
        internal RegexCode _code;
        internal int _codepos;
        internal int[] _codes;
        internal static MethodInfo _crawlposM;
        internal static MethodInfo _ensurestorageM;
        internal RegexPrefix _fcPrefix;
        internal static MethodInfo _getcharM;
        internal static MethodInfo _getCurrentCulture;
        internal static MethodInfo _getInvariantCulture;
        internal int[] _goto;
        internal ILGenerator _ilg;
        internal static MethodInfo _isboundaryM;
        internal static MethodInfo _isECMABoundaryM;
        internal static MethodInfo _ismatchedM;
        internal Label[] _labels;
        internal static MethodInfo _matchindexM;
        internal static MethodInfo _matchlengthM;
        internal int _notecount;
        internal BacktrackNote[] _notes;
        internal RegexOptions _options;
        internal int _regexopcode;
        internal static FieldInfo _stackF;
        internal static FieldInfo _stackposF;
        internal LocalBuilder _stackposV;
        internal LocalBuilder _stackV;
        internal string[] _strings;
        internal LocalBuilder _temp2V;
        internal LocalBuilder _temp3V;
        internal LocalBuilder _tempV;
        internal static FieldInfo _textbegF;
        internal LocalBuilder _textbegV;
        internal static FieldInfo _textendF;
        internal LocalBuilder _textendV;
        internal static FieldInfo _textF;
        internal static FieldInfo _textposF;
        internal LocalBuilder _textposV;
        internal static FieldInfo _textstartF;
        internal LocalBuilder _textstartV;
        internal LocalBuilder _textV;
        internal int _trackcount;
        internal static FieldInfo _trackcountF;
        internal static FieldInfo _trackF;
        internal static FieldInfo _trackposF;
        internal LocalBuilder _trackposV;
        internal LocalBuilder _trackV;
        internal static MethodInfo _transferM;
        internal static MethodInfo _uncaptureM;
        internal int[] _uniquenote;
        internal const int branchcountback2 = 7;
        internal const int branchmarkback2 = 5;
        internal const int capback = 3;
        internal const int capback2 = 4;
        internal const int forejumpback = 9;
        internal const int lazybranchcountback2 = 8;
        internal const int lazybranchmarkback2 = 6;
        internal const int stackpop = 0;
        internal const int stackpop2 = 1;
        internal const int stackpop3 = 2;
        internal const int uniquecount = 10;

        static RegexCompiler()
        {
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
            try
            {
                _textbegF = RegexRunnerField("runtextbeg");
                _textendF = RegexRunnerField("runtextend");
                _textstartF = RegexRunnerField("runtextstart");
                _textposF = RegexRunnerField("runtextpos");
                _textF = RegexRunnerField("runtext");
                _trackposF = RegexRunnerField("runtrackpos");
                _trackF = RegexRunnerField("runtrack");
                _stackposF = RegexRunnerField("runstackpos");
                _stackF = RegexRunnerField("runstack");
                _trackcountF = RegexRunnerField("runtrackcount");
                _ensurestorageM = RegexRunnerMethod("EnsureStorage");
                _captureM = RegexRunnerMethod("Capture");
                _transferM = RegexRunnerMethod("TransferCapture");
                _uncaptureM = RegexRunnerMethod("Uncapture");
                _ismatchedM = RegexRunnerMethod("IsMatched");
                _matchlengthM = RegexRunnerMethod("MatchLength");
                _matchindexM = RegexRunnerMethod("MatchIndex");
                _isboundaryM = RegexRunnerMethod("IsBoundary");
                _charInSetM = RegexRunnerMethod("CharInClass");
                _isECMABoundaryM = RegexRunnerMethod("IsECMABoundary");
                _crawlposM = RegexRunnerMethod("Crawlpos");
                _chartolowerM = typeof(char).GetMethod("ToLower", new Type[] { typeof(char), typeof(CultureInfo) });
                _getcharM = typeof(string).GetMethod("get_Chars", new Type[] { typeof(int) });
                _getCurrentCulture = typeof(CultureInfo).GetMethod("get_CurrentCulture");
                _getInvariantCulture = typeof(CultureInfo).GetMethod("get_InvariantCulture");
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        protected RegexCompiler()
        {
        }

        internal void Add()
        {
            this._ilg.Emit(OpCodes.Add);
        }

        internal void Add(bool negate)
        {
            if (negate)
            {
                this._ilg.Emit(OpCodes.Sub);
            }
            else
            {
                this._ilg.Emit(OpCodes.Add);
            }
        }

        internal int AddBacktrackNote(int flags, Label l, int codepos)
        {
            if ((this._notes == null) || (this._notecount >= this._notes.Length))
            {
                BacktrackNote[] destinationArray = new BacktrackNote[(this._notes == null) ? 0x10 : (this._notes.Length * 2)];
                if (this._notes != null)
                {
                    Array.Copy(this._notes, 0, destinationArray, 0, this._notecount);
                }
                this._notes = destinationArray;
            }
            this._notes[this._notecount] = new BacktrackNote(flags, l, codepos);
            return this._notecount++;
        }

        internal int AddGoto(int destpos)
        {
            if (this._goto[destpos] == -1)
            {
                this._goto[destpos] = this.AddBacktrackNote(0, this._labels[destpos], destpos);
            }
            return this._goto[destpos];
        }

        internal int AddTrack()
        {
            return this.AddTrack(0x80);
        }

        internal int AddTrack(int flags)
        {
            return this.AddBacktrackNote(flags, this.DefineLabel(), this._codepos);
        }

        internal int AddUniqueTrack(int i)
        {
            return this.AddUniqueTrack(i, 0x80);
        }

        internal int AddUniqueTrack(int i, int flags)
        {
            if (this._uniquenote[i] == -1)
            {
                this._uniquenote[i] = this.AddTrack(flags);
            }
            return this._uniquenote[i];
        }

        internal void Advance()
        {
            this._ilg.Emit(OpCodes.Br, this.AdvanceLabel());
        }

        internal Label AdvanceLabel()
        {
            return this._labels[this.NextCodepos()];
        }

        internal void Back()
        {
            this._ilg.Emit(OpCodes.Br, this._backtrack);
        }

        internal void Beq(Label l)
        {
            this._ilg.Emit(OpCodes.Beq_S, l);
        }

        internal void BeqFar(Label l)
        {
            this._ilg.Emit(OpCodes.Beq, l);
        }

        internal void Bge(Label l)
        {
            this._ilg.Emit(OpCodes.Bge_S, l);
        }

        internal void BgeFar(Label l)
        {
            this._ilg.Emit(OpCodes.Bge, l);
        }

        internal void Bgt(Label l)
        {
            this._ilg.Emit(OpCodes.Bgt_S, l);
        }

        internal void BgtFar(Label l)
        {
            this._ilg.Emit(OpCodes.Bgt, l);
        }

        internal void Bgtun(Label l)
        {
            this._ilg.Emit(OpCodes.Bgt_Un_S, l);
        }

        internal void Ble(Label l)
        {
            this._ilg.Emit(OpCodes.Ble_S, l);
        }

        internal void BleFar(Label l)
        {
            this._ilg.Emit(OpCodes.Ble, l);
        }

        internal void Blt(Label l)
        {
            this._ilg.Emit(OpCodes.Blt_S, l);
        }

        internal void BltFar(Label l)
        {
            this._ilg.Emit(OpCodes.Blt, l);
        }

        internal void Bne(Label l)
        {
            this._ilg.Emit(OpCodes.Bne_Un_S, l);
        }

        internal void BneFar(Label l)
        {
            this._ilg.Emit(OpCodes.Bne_Un, l);
        }

        internal void Br(Label l)
        {
            this._ilg.Emit(OpCodes.Br_S, l);
        }

        internal void Brfalse(Label l)
        {
            this._ilg.Emit(OpCodes.Brfalse_S, l);
        }

        internal void BrfalseFar(Label l)
        {
            this._ilg.Emit(OpCodes.Brfalse, l);
        }

        internal void BrFar(Label l)
        {
            this._ilg.Emit(OpCodes.Br, l);
        }

        internal void BrtrueFar(Label l)
        {
            this._ilg.Emit(OpCodes.Brtrue, l);
        }

        internal void Call(MethodInfo mt)
        {
            this._ilg.Emit(OpCodes.Call, mt);
        }

        internal void CallToLower()
        {
            if ((this._options & RegexOptions.CultureInvariant) != RegexOptions.None)
            {
                this.Call(_getInvariantCulture);
            }
            else
            {
                this.Call(_getCurrentCulture);
            }
            this.Call(_chartolowerM);
        }

        internal void Callvirt(MethodInfo mt)
        {
            this._ilg.Emit(OpCodes.Callvirt, mt);
        }

        internal int Code()
        {
            return (this._regexopcode & 0x3f);
        }

        internal static RegexRunnerFactory Compile(RegexCode code, RegexOptions options)
        {
            RegexRunnerFactory factory;
            RegexLWCGCompiler compiler = new RegexLWCGCompiler();
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
            try
            {
                factory = compiler.FactoryInstanceFromCode(code, options);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return factory;
        }

        internal static void CompileToAssembly(RegexCompilationInfo[] regexes, AssemblyName an, CustomAttributeBuilder[] attribs, string resourceFile)
        {
            RegexTypeCompiler compiler = new RegexTypeCompiler(an, attribs, resourceFile);
            for (int i = 0; i < regexes.Length; i++)
            {
                string name;
                if (regexes[i] == null)
                {
                    throw new ArgumentNullException("regexes", SR.GetString("ArgumentNull_ArrayWithNullElements"));
                }
                string pattern = regexes[i].Pattern;
                RegexOptions op = regexes[i].Options;
                if (regexes[i].Namespace.Length == 0)
                {
                    name = regexes[i].Name;
                }
                else
                {
                    name = regexes[i].Namespace + "." + regexes[i].Name;
                }
                RegexTree t = RegexParser.Parse(pattern, op);
                RegexCode code = RegexWriter.Write(t);
                new ReflectionPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    Type factory = compiler.FactoryTypeFromCode(code, op, name);
                    compiler.GenerateRegexType(pattern, op, name, regexes[i].IsPublic, code, t, factory);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            compiler.Save();
        }

        internal LocalBuilder DeclareInt()
        {
            return this._ilg.DeclareLocal(typeof(int));
        }

        internal LocalBuilder DeclareIntArray()
        {
            return this._ilg.DeclareLocal(typeof(int[]));
        }

        internal LocalBuilder DeclareString()
        {
            return this._ilg.DeclareLocal(typeof(string));
        }

        internal Label DefineLabel()
        {
            return this._ilg.DefineLabel();
        }

        internal void DoPush()
        {
            this._ilg.Emit(OpCodes.Stelem_I4);
        }

        internal void DoReplace()
        {
            this._ilg.Emit(OpCodes.Stelem_I4);
        }

        internal void Dup()
        {
            this._ilg.Emit(OpCodes.Dup);
        }

        internal void GenerateBacktrackSection()
        {
            for (int i = 0; i < this._notecount; i++)
            {
                BacktrackNote note = this._notes[i];
                if (note._flags != 0)
                {
                    this._ilg.MarkLabel(note._label);
                    this._codepos = note._codepos;
                    this._backpos = i;
                    this._regexopcode = this._codes[note._codepos] | note._flags;
                    this.GenerateOneCode();
                }
            }
        }

        internal void GenerateFindFirstChar()
        {
            this._textposV = this.DeclareInt();
            this._textV = this.DeclareString();
            this._tempV = this.DeclareInt();
            this._temp2V = this.DeclareInt();
            if ((this._anchors & 0x35) != 0)
            {
                if (!this._code._rightToLeft)
                {
                    if ((this._anchors & 1) != 0)
                    {
                        Label l = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textbegF);
                        this.Ble(l);
                        this.Ldthis();
                        this.Ldthisfld(_textendF);
                        this.Stfld(_textposF);
                        this.Ldc(0);
                        this.Ret();
                        this.MarkLabel(l);
                    }
                    if ((this._anchors & 4) != 0)
                    {
                        Label label2 = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textstartF);
                        this.Ble(label2);
                        this.Ldthis();
                        this.Ldthisfld(_textendF);
                        this.Stfld(_textposF);
                        this.Ldc(0);
                        this.Ret();
                        this.MarkLabel(label2);
                    }
                    if ((this._anchors & 0x10) != 0)
                    {
                        Label label3 = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textendF);
                        this.Ldc(1);
                        this.Sub();
                        this.Bge(label3);
                        this.Ldthis();
                        this.Ldthisfld(_textendF);
                        this.Ldc(1);
                        this.Sub();
                        this.Stfld(_textposF);
                        this.MarkLabel(label3);
                    }
                    if ((this._anchors & 0x20) != 0)
                    {
                        Label label4 = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textendF);
                        this.Bge(label4);
                        this.Ldthis();
                        this.Ldthisfld(_textendF);
                        this.Stfld(_textposF);
                        this.MarkLabel(label4);
                    }
                }
                else
                {
                    if ((this._anchors & 0x20) != 0)
                    {
                        Label label5 = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textendF);
                        this.Bge(label5);
                        this.Ldthis();
                        this.Ldthisfld(_textbegF);
                        this.Stfld(_textposF);
                        this.Ldc(0);
                        this.Ret();
                        this.MarkLabel(label5);
                    }
                    if ((this._anchors & 0x10) != 0)
                    {
                        Label label6 = this.DefineLabel();
                        Label label7 = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textendF);
                        this.Ldc(1);
                        this.Sub();
                        this.Blt(label6);
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textendF);
                        this.Beq(label7);
                        this.Ldthisfld(_textF);
                        this.Ldthisfld(_textposF);
                        this.Callvirt(_getcharM);
                        this.Ldc(10);
                        this.Beq(label7);
                        this.MarkLabel(label6);
                        this.Ldthis();
                        this.Ldthisfld(_textbegF);
                        this.Stfld(_textposF);
                        this.Ldc(0);
                        this.Ret();
                        this.MarkLabel(label7);
                    }
                    if ((this._anchors & 4) != 0)
                    {
                        Label label8 = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textstartF);
                        this.Bge(label8);
                        this.Ldthis();
                        this.Ldthisfld(_textbegF);
                        this.Stfld(_textposF);
                        this.Ldc(0);
                        this.Ret();
                        this.MarkLabel(label8);
                    }
                    if ((this._anchors & 1) != 0)
                    {
                        Label label9 = this.DefineLabel();
                        this.Ldthisfld(_textposF);
                        this.Ldthisfld(_textbegF);
                        this.Ble(label9);
                        this.Ldthis();
                        this.Ldthisfld(_textbegF);
                        this.Stfld(_textposF);
                        this.MarkLabel(label9);
                    }
                }
                this.Ldc(1);
                this.Ret();
            }
            else if ((this._bmPrefix != null) && (this._bmPrefix._negativeUnicode == null))
            {
                int num2;
                int length;
                int num4;
                LocalBuilder lt = this._tempV;
                LocalBuilder builder2 = this._tempV;
                LocalBuilder builder3 = this._temp2V;
                Label label10 = this.DefineLabel();
                Label label11 = this.DefineLabel();
                Label label12 = this.DefineLabel();
                Label label13 = this.DefineLabel();
                this.DefineLabel();
                Label label14 = this.DefineLabel();
                if (!this._code._rightToLeft)
                {
                    length = -1;
                    num4 = this._bmPrefix._pattern.Length - 1;
                }
                else
                {
                    length = this._bmPrefix._pattern.Length;
                    num4 = 0;
                }
                int i = this._bmPrefix._pattern[num4];
                this.Mvfldloc(_textF, this._textV);
                if (!this._code._rightToLeft)
                {
                    this.Ldthisfld(_textendF);
                }
                else
                {
                    this.Ldthisfld(_textbegF);
                }
                this.Stloc(builder3);
                this.Ldthisfld(_textposF);
                if (!this._code._rightToLeft)
                {
                    this.Ldc(this._bmPrefix._pattern.Length - 1);
                    this.Add();
                }
                else
                {
                    this.Ldc(this._bmPrefix._pattern.Length);
                    this.Sub();
                }
                this.Stloc(this._textposV);
                this.Br(label13);
                this.MarkLabel(label10);
                if (!this._code._rightToLeft)
                {
                    this.Ldc(this._bmPrefix._pattern.Length);
                }
                else
                {
                    this.Ldc(-this._bmPrefix._pattern.Length);
                }
                this.MarkLabel(label11);
                this.Ldloc(this._textposV);
                this.Add();
                this.Stloc(this._textposV);
                this.MarkLabel(label13);
                this.Ldloc(this._textposV);
                this.Ldloc(builder3);
                if (!this._code._rightToLeft)
                {
                    this.BgeFar(label12);
                }
                else
                {
                    this.BltFar(label12);
                }
                this.Rightchar();
                if (this._bmPrefix._caseInsensitive)
                {
                    this.CallToLower();
                }
                this.Dup();
                this.Stloc(lt);
                this.Ldc(i);
                this.BeqFar(label14);
                this.Ldloc(lt);
                this.Ldc(this._bmPrefix._lowASCII);
                this.Sub();
                this.Dup();
                this.Stloc(lt);
                this.Ldc(this._bmPrefix._highASCII - this._bmPrefix._lowASCII);
                this.Bgtun(label10);
                Label[] labels = new Label[(this._bmPrefix._highASCII - this._bmPrefix._lowASCII) + 1];
                for (num2 = this._bmPrefix._lowASCII; num2 <= this._bmPrefix._highASCII; num2++)
                {
                    if (this._bmPrefix._negativeASCII[num2] == length)
                    {
                        labels[num2 - this._bmPrefix._lowASCII] = label10;
                    }
                    else
                    {
                        labels[num2 - this._bmPrefix._lowASCII] = this.DefineLabel();
                    }
                }
                this.Ldloc(lt);
                this._ilg.Emit(OpCodes.Switch, labels);
                for (num2 = this._bmPrefix._lowASCII; num2 <= this._bmPrefix._highASCII; num2++)
                {
                    if (this._bmPrefix._negativeASCII[num2] != length)
                    {
                        this.MarkLabel(labels[num2 - this._bmPrefix._lowASCII]);
                        this.Ldc(this._bmPrefix._negativeASCII[num2]);
                        this.BrFar(label11);
                    }
                }
                this.MarkLabel(label14);
                this.Ldloc(this._textposV);
                this.Stloc(builder2);
                for (num2 = this._bmPrefix._pattern.Length - 2; num2 >= 0; num2--)
                {
                    int num5;
                    Label label15 = this.DefineLabel();
                    if (!this._code._rightToLeft)
                    {
                        num5 = num2;
                    }
                    else
                    {
                        num5 = (this._bmPrefix._pattern.Length - 1) - num2;
                    }
                    this.Ldloc(this._textV);
                    this.Ldloc(builder2);
                    this.Ldc(1);
                    this.Sub(this._code._rightToLeft);
                    this.Dup();
                    this.Stloc(builder2);
                    this.Callvirt(_getcharM);
                    if (this._bmPrefix._caseInsensitive)
                    {
                        this.CallToLower();
                    }
                    this.Ldc(this._bmPrefix._pattern[num5]);
                    this.Beq(label15);
                    this.Ldc(this._bmPrefix._positive[num5]);
                    this.BrFar(label11);
                    this.MarkLabel(label15);
                }
                this.Ldthis();
                this.Ldloc(builder2);
                if (this._code._rightToLeft)
                {
                    this.Ldc(1);
                    this.Add();
                }
                this.Stfld(_textposF);
                this.Ldc(1);
                this.Ret();
                this.MarkLabel(label12);
                this.Ldthis();
                if (!this._code._rightToLeft)
                {
                    this.Ldthisfld(_textendF);
                }
                else
                {
                    this.Ldthisfld(_textbegF);
                }
                this.Stfld(_textposF);
                this.Ldc(0);
                this.Ret();
            }
            else if (this._fcPrefix == null)
            {
                this.Ldc(1);
                this.Ret();
            }
            else
            {
                LocalBuilder builder4 = this._temp2V;
                Label label16 = this.DefineLabel();
                Label label17 = this.DefineLabel();
                Label label18 = this.DefineLabel();
                Label label19 = this.DefineLabel();
                Label label20 = this.DefineLabel();
                this.Mvfldloc(_textposF, this._textposV);
                this.Mvfldloc(_textF, this._textV);
                if (!this._code._rightToLeft)
                {
                    this.Ldthisfld(_textendF);
                    this.Ldloc(this._textposV);
                }
                else
                {
                    this.Ldloc(this._textposV);
                    this.Ldthisfld(_textbegF);
                }
                this.Sub();
                this.Stloc(builder4);
                this.Ldloc(builder4);
                this.Ldc(0);
                this.BleFar(label19);
                this.MarkLabel(label16);
                this.Ldloc(builder4);
                this.Ldc(1);
                this.Sub();
                this.Stloc(builder4);
                if (this._code._rightToLeft)
                {
                    this.Leftcharnext();
                }
                else
                {
                    this.Rightcharnext();
                }
                if (this._fcPrefix.CaseInsensitive)
                {
                    this.CallToLower();
                }
                if (!RegexCharClass.IsSingleton(this._fcPrefix.Prefix))
                {
                    this.Ldstr(this._fcPrefix.Prefix);
                    this.Call(_charInSetM);
                    this.BrtrueFar(label17);
                }
                else
                {
                    this.Ldc(RegexCharClass.SingletonChar(this._fcPrefix.Prefix));
                    this.Beq(label17);
                }
                this.MarkLabel(label20);
                this.Ldloc(builder4);
                this.Ldc(0);
                if (!RegexCharClass.IsSingleton(this._fcPrefix.Prefix))
                {
                    this.BgtFar(label16);
                }
                else
                {
                    this.Bgt(label16);
                }
                this.Ldc(0);
                this.BrFar(label18);
                this.MarkLabel(label17);
                this.Ldloc(this._textposV);
                this.Ldc(1);
                this.Sub(this._code._rightToLeft);
                this.Stloc(this._textposV);
                this.Ldc(1);
                this.MarkLabel(label18);
                this.Mvlocfld(this._textposV, _textposF);
                this.Ret();
                this.MarkLabel(label19);
                this.Ldc(0);
                this.Ret();
            }
        }

        internal void GenerateForwardSection()
        {
            int num;
            this._labels = new Label[this._codes.Length];
            this._goto = new int[this._codes.Length];
            for (num = 0; num < this._codes.Length; num += RegexCode.OpcodeSize(this._codes[num]))
            {
                this._goto[num] = -1;
                this._labels[num] = this._ilg.DefineLabel();
            }
            this._uniquenote = new int[10];
            for (int i = 0; i < 10; i++)
            {
                this._uniquenote[i] = -1;
            }
            this.Mvfldloc(_textF, this._textV);
            this.Mvfldloc(_textstartF, this._textstartV);
            this.Mvfldloc(_textbegF, this._textbegV);
            this.Mvfldloc(_textendF, this._textendV);
            this.Mvfldloc(_textposF, this._textposV);
            this.Mvfldloc(_trackF, this._trackV);
            this.Mvfldloc(_trackposF, this._trackposV);
            this.Mvfldloc(_stackF, this._stackV);
            this.Mvfldloc(_stackposF, this._stackposV);
            this._backpos = -1;
            for (num = 0; num < this._codes.Length; num += RegexCode.OpcodeSize(this._codes[num]))
            {
                this.MarkLabel(this._labels[num]);
                this._codepos = num;
                this._regexopcode = this._codes[num];
                this.GenerateOneCode();
            }
        }

        internal void GenerateGo()
        {
            this._textposV = this.DeclareInt();
            this._textV = this.DeclareString();
            this._trackposV = this.DeclareInt();
            this._trackV = this.DeclareIntArray();
            this._stackposV = this.DeclareInt();
            this._stackV = this.DeclareIntArray();
            this._tempV = this.DeclareInt();
            this._temp2V = this.DeclareInt();
            this._temp3V = this.DeclareInt();
            this._textbegV = this.DeclareInt();
            this._textendV = this.DeclareInt();
            this._textstartV = this.DeclareInt();
            this._labels = null;
            this._notes = null;
            this._notecount = 0;
            this._backtrack = this.DefineLabel();
            this.GenerateForwardSection();
            this.GenerateMiddleSection();
            this.GenerateBacktrackSection();
        }

        internal void GenerateInitTrackCount()
        {
            this.Ldthis();
            this.Ldc(this._trackcount);
            this.Stfld(_trackcountF);
            this.Ret();
        }

        internal void GenerateMiddleSection()
        {
            this.DefineLabel();
            this.MarkLabel(this._backtrack);
            this.Mvlocfld(this._trackposV, _trackposF);
            this.Mvlocfld(this._stackposV, _stackposF);
            this.Ldthis();
            this.Callvirt(_ensurestorageM);
            this.Mvfldloc(_trackposF, this._trackposV);
            this.Mvfldloc(_stackposF, this._stackposV);
            this.Mvfldloc(_trackF, this._trackV);
            this.Mvfldloc(_stackF, this._stackV);
            this.PopTrack();
            Label[] labels = new Label[this._notecount];
            for (int i = 0; i < this._notecount; i++)
            {
                labels[i] = this._notes[i]._label;
            }
            this._ilg.Emit(OpCodes.Switch, labels);
        }

        internal void GenerateOneCode()
        {
            switch (this._regexopcode)
            {
                case 0:
                case 1:
                case 2:
                case 0x40:
                case 0x41:
                case 0x42:
                case 0x200:
                case 0x201:
                case 0x202:
                case 0x240:
                case 0x241:
                case 0x242:
                {
                    LocalBuilder lt = this._tempV;
                    Label l = this.DefineLabel();
                    int i = this.Operand(1);
                    if (i != 0)
                    {
                        this.Ldc(i);
                        if (!this.IsRtl())
                        {
                            this.Ldloc(this._textendV);
                            this.Ldloc(this._textposV);
                        }
                        else
                        {
                            this.Ldloc(this._textposV);
                            this.Ldloc(this._textbegV);
                        }
                        this.Sub();
                        this.BgtFar(this._backtrack);
                        this.Ldloc(this._textposV);
                        this.Ldc(i);
                        this.Add(this.IsRtl());
                        this.Stloc(this._textposV);
                        this.Ldc(i);
                        this.Stloc(lt);
                        this.MarkLabel(l);
                        this.Ldloc(this._textV);
                        this.Ldloc(this._textposV);
                        this.Ldloc(lt);
                        if (this.IsRtl())
                        {
                            this.Ldc(1);
                            this.Sub();
                            this.Dup();
                            this.Stloc(lt);
                            this.Add();
                        }
                        else
                        {
                            this.Dup();
                            this.Ldc(1);
                            this.Sub();
                            this.Stloc(lt);
                            this.Sub();
                        }
                        this.Callvirt(_getcharM);
                        if (this.IsCi())
                        {
                            this.CallToLower();
                        }
                        if (this.Code() == 2)
                        {
                            this.Ldstr(this._strings[this.Operand(0)]);
                            this.Call(_charInSetM);
                            this.BrfalseFar(this._backtrack);
                        }
                        else
                        {
                            this.Ldc(this.Operand(0));
                            if (this.Code() == 0)
                            {
                                this.BneFar(this._backtrack);
                            }
                            else
                            {
                                this.BeqFar(this._backtrack);
                            }
                        }
                        this.Ldloc(lt);
                        this.Ldc(0);
                        if (this.Code() == 2)
                        {
                            this.BgtFar(l);
                            return;
                        }
                        this.Bgt(l);
                    }
                    return;
                }
                case 3:
                case 4:
                case 5:
                case 0x43:
                case 0x44:
                case 0x45:
                case 0x203:
                case 0x204:
                case 0x205:
                case 0x243:
                case 580:
                case 0x245:
                {
                    LocalBuilder builder12 = this._tempV;
                    LocalBuilder builder13 = this._temp2V;
                    Label label18 = this.DefineLabel();
                    Label label19 = this.DefineLabel();
                    int num4 = this.Operand(1);
                    if (num4 != 0)
                    {
                        if (!this.IsRtl())
                        {
                            this.Ldloc(this._textendV);
                            this.Ldloc(this._textposV);
                        }
                        else
                        {
                            this.Ldloc(this._textposV);
                            this.Ldloc(this._textbegV);
                        }
                        this.Sub();
                        if (num4 != 0x7fffffff)
                        {
                            Label label20 = this.DefineLabel();
                            this.Dup();
                            this.Ldc(num4);
                            this.Blt(label20);
                            this.Pop();
                            this.Ldc(num4);
                            this.MarkLabel(label20);
                        }
                        this.Dup();
                        this.Stloc(builder13);
                        this.Ldc(1);
                        this.Add();
                        this.Stloc(builder12);
                        this.MarkLabel(label18);
                        this.Ldloc(builder12);
                        this.Ldc(1);
                        this.Sub();
                        this.Dup();
                        this.Stloc(builder12);
                        this.Ldc(0);
                        if (this.Code() == 5)
                        {
                            this.BleFar(label19);
                        }
                        else
                        {
                            this.Ble(label19);
                        }
                        if (this.IsRtl())
                        {
                            this.Leftcharnext();
                        }
                        else
                        {
                            this.Rightcharnext();
                        }
                        if (this.IsCi())
                        {
                            this.CallToLower();
                        }
                        if (this.Code() == 5)
                        {
                            this.Ldstr(this._strings[this.Operand(0)]);
                            this.Call(_charInSetM);
                            this.BrtrueFar(label18);
                        }
                        else
                        {
                            this.Ldc(this.Operand(0));
                            if (this.Code() == 3)
                            {
                                this.Beq(label18);
                            }
                            else
                            {
                                this.Bne(label18);
                            }
                        }
                        this.Ldloc(this._textposV);
                        this.Ldc(1);
                        this.Sub(this.IsRtl());
                        this.Stloc(this._textposV);
                        this.MarkLabel(label19);
                        this.Ldloc(builder13);
                        this.Ldloc(builder12);
                        this.Ble(this.AdvanceLabel());
                        this.ReadyPushTrack();
                        this.Ldloc(builder13);
                        this.Ldloc(builder12);
                        this.Sub();
                        this.Ldc(1);
                        this.Sub();
                        this.DoPush();
                        this.ReadyPushTrack();
                        this.Ldloc(this._textposV);
                        this.Ldc(1);
                        this.Sub(this.IsRtl());
                        this.DoPush();
                        this.Track();
                    }
                    return;
                }
                case 6:
                case 7:
                case 8:
                case 70:
                case 0x47:
                case 0x48:
                case 0x206:
                case 0x207:
                case 520:
                case 0x246:
                case 0x247:
                case 0x248:
                {
                    LocalBuilder builder14 = this._tempV;
                    int num5 = this.Operand(1);
                    if (num5 != 0)
                    {
                        if (!this.IsRtl())
                        {
                            this.Ldloc(this._textendV);
                            this.Ldloc(this._textposV);
                        }
                        else
                        {
                            this.Ldloc(this._textposV);
                            this.Ldloc(this._textbegV);
                        }
                        this.Sub();
                        if (num5 != 0x7fffffff)
                        {
                            Label label21 = this.DefineLabel();
                            this.Dup();
                            this.Ldc(num5);
                            this.Blt(label21);
                            this.Pop();
                            this.Ldc(num5);
                            this.MarkLabel(label21);
                        }
                        this.Dup();
                        this.Stloc(builder14);
                        this.Ldc(0);
                        this.Ble(this.AdvanceLabel());
                        this.ReadyPushTrack();
                        this.Ldloc(builder14);
                        this.Ldc(1);
                        this.Sub();
                        this.DoPush();
                        this.PushTrack(this._textposV);
                        this.Track();
                    }
                    return;
                }
                case 9:
                case 10:
                case 11:
                case 0x49:
                case 0x4a:
                case 0x4b:
                case 0x209:
                case 0x20a:
                case 0x20b:
                case 0x249:
                case 0x24a:
                case 0x24b:
                    this.Ldloc(this._textposV);
                    if (!this.IsRtl())
                    {
                        this.Ldloc(this._textendV);
                        this.BgeFar(this._backtrack);
                        this.Rightcharnext();
                    }
                    else
                    {
                        this.Ldloc(this._textbegV);
                        this.BleFar(this._backtrack);
                        this.Leftcharnext();
                    }
                    if (this.IsCi())
                    {
                        this.CallToLower();
                    }
                    if (this.Code() == 11)
                    {
                        this.Ldstr(this._strings[this.Operand(0)]);
                        this.Call(_charInSetM);
                        this.BrfalseFar(this._backtrack);
                        return;
                    }
                    this.Ldc(this.Operand(0));
                    if (this.Code() == 9)
                    {
                        this.BneFar(this._backtrack);
                        return;
                    }
                    this.BeqFar(this._backtrack);
                    return;

                case 12:
                case 0x20c:
                {
                    string str = this._strings[this.Operand(0)];
                    this.Ldc(str.Length);
                    this.Ldloc(this._textendV);
                    this.Ldloc(this._textposV);
                    this.Sub();
                    this.BgtFar(this._backtrack);
                    for (int j = 0; j < str.Length; j++)
                    {
                        this.Ldloc(this._textV);
                        this.Ldloc(this._textposV);
                        if (j != 0)
                        {
                            this.Ldc(j);
                            this.Add();
                        }
                        this.Callvirt(_getcharM);
                        if (this.IsCi())
                        {
                            this.CallToLower();
                        }
                        this.Ldc(str[j]);
                        this.BneFar(this._backtrack);
                    }
                    this.Ldloc(this._textposV);
                    this.Ldc(str.Length);
                    this.Add();
                    this.Stloc(this._textposV);
                    return;
                }
                case 13:
                case 0x4d:
                case 0x20d:
                case 0x24d:
                {
                    LocalBuilder builder9 = this._tempV;
                    LocalBuilder builder10 = this._temp2V;
                    Label label16 = this.DefineLabel();
                    this.Ldthis();
                    this.Ldc(this.Operand(0));
                    this.Callvirt(_ismatchedM);
                    if ((this._options & RegexOptions.ECMAScript) != RegexOptions.None)
                    {
                        this.Brfalse(this.AdvanceLabel());
                    }
                    else
                    {
                        this.BrfalseFar(this._backtrack);
                    }
                    this.Ldthis();
                    this.Ldc(this.Operand(0));
                    this.Callvirt(_matchlengthM);
                    this.Dup();
                    this.Stloc(builder9);
                    if (!this.IsRtl())
                    {
                        this.Ldloc(this._textendV);
                        this.Ldloc(this._textposV);
                    }
                    else
                    {
                        this.Ldloc(this._textposV);
                        this.Ldloc(this._textbegV);
                    }
                    this.Sub();
                    this.BgtFar(this._backtrack);
                    this.Ldthis();
                    this.Ldc(this.Operand(0));
                    this.Callvirt(_matchindexM);
                    if (!this.IsRtl())
                    {
                        this.Ldloc(builder9);
                        this.Add(this.IsRtl());
                    }
                    this.Stloc(builder10);
                    this.Ldloc(this._textposV);
                    this.Ldloc(builder9);
                    this.Add(this.IsRtl());
                    this.Stloc(this._textposV);
                    this.MarkLabel(label16);
                    this.Ldloc(builder9);
                    this.Ldc(0);
                    this.Ble(this.AdvanceLabel());
                    this.Ldloc(this._textV);
                    this.Ldloc(builder10);
                    this.Ldloc(builder9);
                    if (this.IsRtl())
                    {
                        this.Ldc(1);
                        this.Sub();
                        this.Dup();
                        this.Stloc(builder9);
                    }
                    this.Sub(this.IsRtl());
                    this.Callvirt(_getcharM);
                    if (this.IsCi())
                    {
                        this.CallToLower();
                    }
                    this.Ldloc(this._textV);
                    this.Ldloc(this._textposV);
                    this.Ldloc(builder9);
                    if (!this.IsRtl())
                    {
                        this.Dup();
                        this.Ldc(1);
                        this.Sub();
                        this.Stloc(builder9);
                    }
                    this.Sub(this.IsRtl());
                    this.Callvirt(_getcharM);
                    if (this.IsCi())
                    {
                        this.CallToLower();
                    }
                    this.Beq(label16);
                    this.Back();
                    return;
                }
                case 14:
                {
                    Label label14 = this._labels[this.NextCodepos()];
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textbegV);
                    this.Ble(label14);
                    this.Leftchar();
                    this.Ldc(10);
                    this.BneFar(this._backtrack);
                    return;
                }
                case 15:
                {
                    Label label15 = this._labels[this.NextCodepos()];
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textendV);
                    this.Bge(label15);
                    this.Rightchar();
                    this.Ldc(10);
                    this.BneFar(this._backtrack);
                    return;
                }
                case 0x10:
                case 0x11:
                    this.Ldthis();
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textbegV);
                    this.Ldloc(this._textendV);
                    this.Callvirt(_isboundaryM);
                    if (this.Code() != 0x10)
                    {
                        this.BrtrueFar(this._backtrack);
                        return;
                    }
                    this.BrfalseFar(this._backtrack);
                    return;

                case 0x12:
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textbegV);
                    this.BgtFar(this._backtrack);
                    return;

                case 0x13:
                    this.Ldloc(this._textposV);
                    this.Ldthisfld(_textstartF);
                    this.BneFar(this._backtrack);
                    return;

                case 20:
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textendV);
                    this.Ldc(1);
                    this.Sub();
                    this.BltFar(this._backtrack);
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textendV);
                    this.Bge(this._labels[this.NextCodepos()]);
                    this.Rightchar();
                    this.Ldc(10);
                    this.BneFar(this._backtrack);
                    return;

                case 0x15:
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textendV);
                    this.BltFar(this._backtrack);
                    return;

                case 0x16:
                    this.Back();
                    return;

                case 0x17:
                    this.PushTrack(this._textposV);
                    this.Track();
                    return;

                case 0x18:
                {
                    LocalBuilder builder = this._tempV;
                    Label label = this.DefineLabel();
                    this.PopStack();
                    this.Dup();
                    this.Stloc(builder);
                    this.PushTrack(builder);
                    this.Ldloc(this._textposV);
                    this.Beq(label);
                    this.PushTrack(this._textposV);
                    this.PushStack(this._textposV);
                    this.Track();
                    this.Goto(this.Operand(0));
                    this.MarkLabel(label);
                    this.TrackUnique2(5);
                    return;
                }
                case 0x19:
                {
                    LocalBuilder builder2 = this._tempV;
                    Label label2 = this.DefineLabel();
                    Label label3 = this.DefineLabel();
                    Label label4 = this.DefineLabel();
                    this.PopStack();
                    this.Dup();
                    this.Stloc(builder2);
                    this.Ldloc(builder2);
                    this.Ldc(-1);
                    this.Beq(label3);
                    this.PushTrack(builder2);
                    this.Br(label4);
                    this.MarkLabel(label3);
                    this.PushTrack(this._textposV);
                    this.MarkLabel(label4);
                    this.Ldloc(this._textposV);
                    this.Beq(label2);
                    this.PushTrack(this._textposV);
                    this.Track();
                    this.Br(this.AdvanceLabel());
                    this.MarkLabel(label2);
                    this.ReadyPushStack();
                    this.Ldloc(builder2);
                    this.DoPush();
                    this.TrackUnique2(6);
                    return;
                }
                case 0x1a:
                    this.ReadyPushStack();
                    this.Ldc(-1);
                    this.DoPush();
                    this.ReadyPushStack();
                    this.Ldc(this.Operand(0));
                    this.DoPush();
                    this.TrackUnique(1);
                    return;

                case 0x1b:
                    this.PushStack(this._textposV);
                    this.ReadyPushStack();
                    this.Ldc(this.Operand(0));
                    this.DoPush();
                    this.TrackUnique(1);
                    return;

                case 0x1c:
                {
                    LocalBuilder builder3 = this._tempV;
                    LocalBuilder builder4 = this._temp2V;
                    Label label5 = this.DefineLabel();
                    Label label6 = this.DefineLabel();
                    this.PopStack();
                    this.Stloc(builder3);
                    this.PopStack();
                    this.Dup();
                    this.Stloc(builder4);
                    this.PushTrack(builder4);
                    this.Ldloc(this._textposV);
                    this.Bne(label5);
                    this.Ldloc(builder3);
                    this.Ldc(0);
                    this.Bge(label6);
                    this.MarkLabel(label5);
                    this.Ldloc(builder3);
                    this.Ldc(this.Operand(1));
                    this.Bge(label6);
                    this.PushStack(this._textposV);
                    this.ReadyPushStack();
                    this.Ldloc(builder3);
                    this.Ldc(1);
                    this.Add();
                    this.DoPush();
                    this.Track();
                    this.Goto(this.Operand(0));
                    this.MarkLabel(label6);
                    this.PushTrack(builder3);
                    this.TrackUnique2(7);
                    return;
                }
                case 0x1d:
                {
                    LocalBuilder builder6 = this._tempV;
                    LocalBuilder builder7 = this._temp2V;
                    Label label8 = this.DefineLabel();
                    this.DefineLabel();
                    Label label1 = this._labels[this.NextCodepos()];
                    this.PopStack();
                    this.Stloc(builder6);
                    this.PopStack();
                    this.Stloc(builder7);
                    this.Ldloc(builder6);
                    this.Ldc(0);
                    this.Bge(label8);
                    this.PushTrack(builder7);
                    this.PushStack(this._textposV);
                    this.ReadyPushStack();
                    this.Ldloc(builder6);
                    this.Ldc(1);
                    this.Add();
                    this.DoPush();
                    this.TrackUnique2(8);
                    this.Goto(this.Operand(0));
                    this.MarkLabel(label8);
                    this.PushTrack(builder7);
                    this.PushTrack(builder6);
                    this.PushTrack(this._textposV);
                    this.Track();
                    return;
                }
                case 30:
                    this.ReadyPushStack();
                    this.Ldc(-1);
                    this.DoPush();
                    this.TrackUnique(0);
                    return;

                case 0x1f:
                    this.PushStack(this._textposV);
                    this.TrackUnique(0);
                    return;

                case 0x20:
                    if (this.Operand(1) != -1)
                    {
                        this.Ldthis();
                        this.Ldc(this.Operand(1));
                        this.Callvirt(_ismatchedM);
                        this.BrfalseFar(this._backtrack);
                    }
                    this.PopStack();
                    this.Stloc(this._tempV);
                    if (this.Operand(1) != -1)
                    {
                        this.Ldthis();
                        this.Ldc(this.Operand(0));
                        this.Ldc(this.Operand(1));
                        this.Ldloc(this._tempV);
                        this.Ldloc(this._textposV);
                        this.Callvirt(_transferM);
                    }
                    else
                    {
                        this.Ldthis();
                        this.Ldc(this.Operand(0));
                        this.Ldloc(this._tempV);
                        this.Ldloc(this._textposV);
                        this.Callvirt(_captureM);
                    }
                    this.PushTrack(this._tempV);
                    if ((this.Operand(0) != -1) && (this.Operand(1) != -1))
                    {
                        this.TrackUnique(4);
                        return;
                    }
                    this.TrackUnique(3);
                    return;

                case 0x21:
                    this.ReadyPushTrack();
                    this.PopStack();
                    this.Dup();
                    this.Stloc(this._textposV);
                    this.DoPush();
                    this.Track();
                    return;

                case 0x22:
                    this.ReadyPushStack();
                    this.Ldthisfld(_trackF);
                    this.Ldlen();
                    this.Ldloc(this._trackposV);
                    this.Sub();
                    this.DoPush();
                    this.ReadyPushStack();
                    this.Ldthis();
                    this.Callvirt(_crawlposM);
                    this.DoPush();
                    this.TrackUnique(1);
                    return;

                case 0x23:
                {
                    Label label10 = this.DefineLabel();
                    Label label11 = this.DefineLabel();
                    this.PopStack();
                    this.Ldthisfld(_trackF);
                    this.Ldlen();
                    this.PopStack();
                    this.Sub();
                    this.Stloc(this._trackposV);
                    this.Dup();
                    this.Ldthis();
                    this.Callvirt(_crawlposM);
                    this.Beq(label11);
                    this.MarkLabel(label10);
                    this.Ldthis();
                    this.Callvirt(_uncaptureM);
                    this.Dup();
                    this.Ldthis();
                    this.Callvirt(_crawlposM);
                    this.Bne(label10);
                    this.MarkLabel(label11);
                    this.Pop();
                    this.Back();
                    return;
                }
                case 0x24:
                    this.PopStack();
                    this.Stloc(this._tempV);
                    this.Ldthisfld(_trackF);
                    this.Ldlen();
                    this.PopStack();
                    this.Sub();
                    this.Stloc(this._trackposV);
                    this.PushTrack(this._tempV);
                    this.TrackUnique(9);
                    return;

                case 0x25:
                    this.Ldthis();
                    this.Ldc(this.Operand(0));
                    this.Callvirt(_ismatchedM);
                    this.BrfalseFar(this._backtrack);
                    return;

                case 0x26:
                    this.Goto(this.Operand(0));
                    return;

                case 40:
                    this.Mvlocfld(this._textposV, _textposF);
                    this.Ret();
                    return;

                case 0x29:
                case 0x2a:
                    this.Ldthis();
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textbegV);
                    this.Ldloc(this._textendV);
                    this.Callvirt(_isECMABoundaryM);
                    if (this.Code() != 0x29)
                    {
                        this.BrtrueFar(this._backtrack);
                        return;
                    }
                    this.BrfalseFar(this._backtrack);
                    return;

                case 0x4c:
                case 0x24c:
                {
                    string str2 = this._strings[this.Operand(0)];
                    this.Ldc(str2.Length);
                    this.Ldloc(this._textposV);
                    this.Ldloc(this._textbegV);
                    this.Sub();
                    this.BgtFar(this._backtrack);
                    int length = str2.Length;
                    while (length > 0)
                    {
                        length--;
                        this.Ldloc(this._textV);
                        this.Ldloc(this._textposV);
                        this.Ldc(str2.Length - length);
                        this.Sub();
                        this.Callvirt(_getcharM);
                        if (this.IsCi())
                        {
                            this.CallToLower();
                        }
                        this.Ldc(str2[length]);
                        this.BneFar(this._backtrack);
                    }
                    this.Ldloc(this._textposV);
                    this.Ldc(str2.Length);
                    this.Sub();
                    this.Stloc(this._textposV);
                    return;
                }
                case 0x83:
                case 0x84:
                case 0x85:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0x283:
                case 0x284:
                case 0x285:
                case 0x2c3:
                case 0x2c4:
                case 0x2c5:
                    this.PopTrack();
                    this.Stloc(this._textposV);
                    this.PopTrack();
                    this.Stloc(this._tempV);
                    this.Ldloc(this._tempV);
                    this.Ldc(0);
                    this.BleFar(this.AdvanceLabel());
                    this.ReadyPushTrack();
                    this.Ldloc(this._tempV);
                    this.Ldc(1);
                    this.Sub();
                    this.DoPush();
                    this.ReadyPushTrack();
                    this.Ldloc(this._textposV);
                    this.Ldc(1);
                    this.Sub(this.IsRtl());
                    this.DoPush();
                    this.Trackagain();
                    this.Advance();
                    return;

                case 0x86:
                case 0x87:
                case 0x88:
                case 0xc6:
                case 0xc7:
                case 200:
                case 0x286:
                case 0x287:
                case 0x288:
                case 710:
                case 0x2c7:
                case 0x2c8:
                    this.PopTrack();
                    this.Stloc(this._textposV);
                    this.PopTrack();
                    this.Stloc(this._temp2V);
                    if (!this.IsRtl())
                    {
                        this.Rightcharnext();
                    }
                    else
                    {
                        this.Leftcharnext();
                    }
                    if (this.IsCi())
                    {
                        this.CallToLower();
                    }
                    if (this.Code() == 8)
                    {
                        this.Ldstr(this._strings[this.Operand(0)]);
                        this.Call(_charInSetM);
                        this.BrfalseFar(this._backtrack);
                    }
                    else
                    {
                        this.Ldc(this.Operand(0));
                        if (this.Code() == 6)
                        {
                            this.BneFar(this._backtrack);
                        }
                        else
                        {
                            this.BeqFar(this._backtrack);
                        }
                    }
                    this.Ldloc(this._temp2V);
                    this.Ldc(0);
                    this.BleFar(this.AdvanceLabel());
                    this.ReadyPushTrack();
                    this.Ldloc(this._temp2V);
                    this.Ldc(1);
                    this.Sub();
                    this.DoPush();
                    this.PushTrack(this._textposV);
                    this.Trackagain();
                    this.Advance();
                    return;

                case 0x97:
                    this.PopTrack();
                    this.Stloc(this._textposV);
                    this.Goto(this.Operand(0));
                    return;

                case 0x98:
                    this.PopTrack();
                    this.Stloc(this._textposV);
                    this.PopStack();
                    this.Pop();
                    this.TrackUnique2(5);
                    this.Advance();
                    return;

                case 0x99:
                    this.PopTrack();
                    this.Stloc(this._textposV);
                    this.PushStack(this._textposV);
                    this.TrackUnique2(6);
                    this.Goto(this.Operand(0));
                    return;

                case 0x9a:
                case 0x9b:
                    this.PopDiscardStack(2);
                    this.Back();
                    return;

                case 0x9c:
                {
                    LocalBuilder builder5 = this._tempV;
                    Label label7 = this.DefineLabel();
                    this.PopStack();
                    this.Ldc(1);
                    this.Sub();
                    this.Dup();
                    this.Stloc(builder5);
                    this.Ldc(0);
                    this.Blt(label7);
                    this.PopStack();
                    this.Stloc(this._textposV);
                    this.PushTrack(builder5);
                    this.TrackUnique2(7);
                    this.Advance();
                    this.MarkLabel(label7);
                    this.ReadyReplaceStack(0);
                    this.PopTrack();
                    this.DoReplace();
                    this.PushStack(builder5);
                    this.Back();
                    return;
                }
                case 0x9d:
                {
                    Label label9 = this.DefineLabel();
                    LocalBuilder builder8 = this._tempV;
                    this.PopTrack();
                    this.Stloc(this._textposV);
                    this.PopTrack();
                    this.Dup();
                    this.Stloc(builder8);
                    this.Ldc(this.Operand(1));
                    this.Bge(label9);
                    this.Ldloc(this._textposV);
                    this.TopTrack();
                    this.Beq(label9);
                    this.PushStack(this._textposV);
                    this.ReadyPushStack();
                    this.Ldloc(builder8);
                    this.Ldc(1);
                    this.Add();
                    this.DoPush();
                    this.TrackUnique2(8);
                    this.Goto(this.Operand(0));
                    this.MarkLabel(label9);
                    this.ReadyPushStack();
                    this.PopTrack();
                    this.DoPush();
                    this.PushStack(builder8);
                    this.Back();
                    return;
                }
                case 0x9e:
                case 0x9f:
                    this.PopDiscardStack();
                    this.Back();
                    return;

                case 160:
                    this.ReadyPushStack();
                    this.PopTrack();
                    this.DoPush();
                    this.Ldthis();
                    this.Callvirt(_uncaptureM);
                    if ((this.Operand(0) != -1) && (this.Operand(1) != -1))
                    {
                        this.Ldthis();
                        this.Callvirt(_uncaptureM);
                    }
                    this.Back();
                    return;

                case 0xa1:
                    this.ReadyPushStack();
                    this.PopTrack();
                    this.DoPush();
                    this.Back();
                    return;

                case 0xa2:
                    this.PopDiscardStack(2);
                    this.Back();
                    return;

                case 0xa4:
                {
                    Label label12 = this.DefineLabel();
                    Label label13 = this.DefineLabel();
                    this.PopTrack();
                    this.Dup();
                    this.Ldthis();
                    this.Callvirt(_crawlposM);
                    this.Beq(label13);
                    this.MarkLabel(label12);
                    this.Ldthis();
                    this.Callvirt(_uncaptureM);
                    this.Dup();
                    this.Ldthis();
                    this.Callvirt(_crawlposM);
                    this.Bne(label12);
                    this.MarkLabel(label13);
                    this.Pop();
                    this.Back();
                    return;
                }
                case 280:
                    this.ReadyPushStack();
                    this.PopTrack();
                    this.DoPush();
                    this.Back();
                    return;

                case 0x119:
                    this.ReadyReplaceStack(0);
                    this.PopTrack();
                    this.DoReplace();
                    this.Back();
                    return;

                case 0x11c:
                    this.PopTrack();
                    this.Stloc(this._tempV);
                    this.ReadyPushStack();
                    this.PopTrack();
                    this.DoPush();
                    this.PushStack(this._tempV);
                    this.Back();
                    return;

                case 0x11d:
                    this.ReadyReplaceStack(1);
                    this.PopTrack();
                    this.DoReplace();
                    this.ReadyReplaceStack(0);
                    this.TopStack();
                    this.Ldc(1);
                    this.Sub();
                    this.DoReplace();
                    this.Back();
                    return;
            }
            throw new NotImplementedException(SR.GetString("UnimplementedState"));
        }

        internal void Goto(int i)
        {
            if (i < this._codepos)
            {
                Label l = this.DefineLabel();
                this.Ldloc(this._trackposV);
                this.Ldc(this._trackcount * 4);
                this.Ble(l);
                this.Ldloc(this._stackposV);
                this.Ldc(this._trackcount * 3);
                this.BgtFar(this._labels[i]);
                this.MarkLabel(l);
                this.ReadyPushTrack();
                this.Ldc(this.AddGoto(i));
                this.DoPush();
                this.BrFar(this._backtrack);
            }
            else
            {
                this.BrFar(this._labels[i]);
            }
        }

        internal bool IsCi()
        {
            return ((this._regexopcode & 0x200) != 0);
        }

        internal bool IsRtl()
        {
            return ((this._regexopcode & 0x40) != 0);
        }

        internal void Ldc(int i)
        {
            if ((i <= 0x7f) && (i >= -128))
            {
                this._ilg.Emit(OpCodes.Ldc_I4_S, (byte) i);
            }
            else
            {
                this._ilg.Emit(OpCodes.Ldc_I4, i);
            }
        }

        internal void Ldlen()
        {
            this._ilg.Emit(OpCodes.Ldlen);
        }

        internal void Ldloc(LocalBuilder lt)
        {
            this._ilg.Emit(OpCodes.Ldloc_S, lt);
        }

        internal void Ldstr(string str)
        {
            this._ilg.Emit(OpCodes.Ldstr, str);
        }

        internal void Ldthis()
        {
            this._ilg.Emit(OpCodes.Ldarg_0);
        }

        internal void Ldthisfld(FieldInfo ft)
        {
            this.Ldthis();
            this._ilg.Emit(OpCodes.Ldfld, ft);
        }

        internal void Leftchar()
        {
            this.Ldloc(this._textV);
            this.Ldloc(this._textposV);
            this.Ldc(1);
            this.Sub();
            this.Callvirt(_getcharM);
        }

        internal void Leftcharnext()
        {
            this.Ldloc(this._textV);
            this.Ldloc(this._textposV);
            this.Ldc(1);
            this.Sub();
            this.Dup();
            this.Stloc(this._textposV);
            this.Callvirt(_getcharM);
        }

        internal void MarkLabel(Label l)
        {
            this._ilg.MarkLabel(l);
        }

        internal void Mvfldloc(FieldInfo ft, LocalBuilder lt)
        {
            this.Ldthisfld(ft);
            this.Stloc(lt);
        }

        internal void Mvlocfld(LocalBuilder lt, FieldInfo ft)
        {
            this.Ldthis();
            this.Ldloc(lt);
            this.Stfld(ft);
        }

        internal void Newobj(ConstructorInfo ct)
        {
            this._ilg.Emit(OpCodes.Newobj, ct);
        }

        internal int NextCodepos()
        {
            return (this._codepos + RegexCode.OpcodeSize(this._codes[this._codepos]));
        }

        internal int Operand(int i)
        {
            return this._codes[(this._codepos + i) + 1];
        }

        internal void Pop()
        {
            this._ilg.Emit(OpCodes.Pop);
        }

        internal void PopDiscardStack()
        {
            this.PopDiscardStack(1);
        }

        internal void PopDiscardStack(int i)
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackposV);
            this.Ldc(i);
            this._ilg.Emit(OpCodes.Add);
            this._ilg.Emit(OpCodes.Stloc_S, this._stackposV);
        }

        internal void PopStack()
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackV);
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackposV);
            this._ilg.Emit(OpCodes.Dup);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Add);
            this._ilg.Emit(OpCodes.Stloc_S, this._stackposV);
            this._ilg.Emit(OpCodes.Ldelem_I4);
        }

        internal void PopTrack()
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._trackV);
            this._ilg.Emit(OpCodes.Ldloc_S, this._trackposV);
            this._ilg.Emit(OpCodes.Dup);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Add);
            this._ilg.Emit(OpCodes.Stloc_S, this._trackposV);
            this._ilg.Emit(OpCodes.Ldelem_I4);
        }

        internal void PushStack(LocalBuilder lt)
        {
            this.ReadyPushStack();
            this._ilg.Emit(OpCodes.Ldloc_S, lt);
            this.DoPush();
        }

        internal void PushTrack(LocalBuilder lt)
        {
            this.ReadyPushTrack();
            this.Ldloc(lt);
            this.DoPush();
        }

        internal void ReadyPushStack()
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackV);
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackposV);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Sub);
            this._ilg.Emit(OpCodes.Dup);
            this._ilg.Emit(OpCodes.Stloc_S, this._stackposV);
        }

        internal void ReadyPushTrack()
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._trackV);
            this._ilg.Emit(OpCodes.Ldloc_S, this._trackposV);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Sub);
            this._ilg.Emit(OpCodes.Dup);
            this._ilg.Emit(OpCodes.Stloc_S, this._trackposV);
        }

        internal void ReadyReplaceStack(int i)
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackV);
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackposV);
            if (i != 0)
            {
                this.Ldc(i);
                this._ilg.Emit(OpCodes.Add);
            }
        }

        private static FieldInfo RegexRunnerField(string fieldname)
        {
            return typeof(RegexRunner).GetField(fieldname, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        private static MethodInfo RegexRunnerMethod(string methname)
        {
            return typeof(RegexRunner).GetMethod(methname, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        internal void Ret()
        {
            this._ilg.Emit(OpCodes.Ret);
        }

        internal void Rightchar()
        {
            this.Ldloc(this._textV);
            this.Ldloc(this._textposV);
            this.Callvirt(_getcharM);
        }

        internal void Rightcharnext()
        {
            this.Ldloc(this._textV);
            this.Ldloc(this._textposV);
            this.Dup();
            this.Ldc(1);
            this.Add();
            this.Stloc(this._textposV);
            this.Callvirt(_getcharM);
        }

        internal void Stfld(FieldInfo ft)
        {
            this._ilg.Emit(OpCodes.Stfld, ft);
        }

        internal void Stloc(LocalBuilder lt)
        {
            this._ilg.Emit(OpCodes.Stloc_S, lt);
        }

        internal void Sub()
        {
            this._ilg.Emit(OpCodes.Sub);
        }

        internal void Sub(bool negate)
        {
            if (negate)
            {
                this._ilg.Emit(OpCodes.Add);
            }
            else
            {
                this._ilg.Emit(OpCodes.Sub);
            }
        }

        internal void TopStack()
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackV);
            this._ilg.Emit(OpCodes.Ldloc_S, this._stackposV);
            this._ilg.Emit(OpCodes.Ldelem_I4);
        }

        internal void TopTrack()
        {
            this._ilg.Emit(OpCodes.Ldloc_S, this._trackV);
            this._ilg.Emit(OpCodes.Ldloc_S, this._trackposV);
            this._ilg.Emit(OpCodes.Ldelem_I4);
        }

        internal void Track()
        {
            this.ReadyPushTrack();
            this.Ldc(this.AddTrack());
            this.DoPush();
        }

        internal void Trackagain()
        {
            this.ReadyPushTrack();
            this.Ldc(this._backpos);
            this.DoPush();
        }

        internal void TrackUnique(int i)
        {
            this.ReadyPushTrack();
            this.Ldc(this.AddUniqueTrack(i));
            this.DoPush();
        }

        internal void TrackUnique2(int i)
        {
            this.ReadyPushTrack();
            this.Ldc(this.AddUniqueTrack(i, 0x100));
            this.DoPush();
        }

        internal sealed class BacktrackNote
        {
            internal int _codepos;
            internal int _flags;
            internal Label _label;

            internal BacktrackNote(int flags, Label label, int codepos)
            {
                this._codepos = codepos;
                this._flags = flags;
                this._label = label;
            }
        }
    }
}

