namespace System.Text.RegularExpressions
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    internal class RegexLWCGCompiler : RegexCompiler
    {
        private static Type[] _paramTypes = new Type[] { typeof(RegexRunner) };
        private static int _regexCount = 0;

        internal RegexLWCGCompiler()
        {
        }

        internal DynamicMethod DefineDynamicMethod(string methname, Type returntype, Type hostType)
        {
            MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.Public;
            CallingConventions standard = CallingConventions.Standard;
            DynamicMethod method = new DynamicMethod(methname, attributes, standard, returntype, _paramTypes, hostType, false);
            base._ilg = method.GetILGenerator();
            return method;
        }

        internal RegexRunnerFactory FactoryInstanceFromCode(RegexCode code, RegexOptions options)
        {
            base._code = code;
            base._codes = code._codes;
            base._strings = code._strings;
            base._fcPrefix = code._fcPrefix;
            base._bmPrefix = code._bmPrefix;
            base._anchors = code._anchors;
            base._trackcount = code._trackcount;
            base._options = options;
            string str = Interlocked.Increment(ref _regexCount).ToString(CultureInfo.InvariantCulture);
            DynamicMethod go = this.DefineDynamicMethod("Go" + str, null, typeof(CompiledRegexRunner));
            base.GenerateGo();
            DynamicMethod firstChar = this.DefineDynamicMethod("FindFirstChar" + str, typeof(bool), typeof(CompiledRegexRunner));
            base.GenerateFindFirstChar();
            DynamicMethod trackCount = this.DefineDynamicMethod("InitTrackCount" + str, null, typeof(CompiledRegexRunner));
            base.GenerateInitTrackCount();
            return new CompiledRegexRunnerFactory(go, firstChar, trackCount);
        }
    }
}

