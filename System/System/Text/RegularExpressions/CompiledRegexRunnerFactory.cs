namespace System.Text.RegularExpressions
{
    using System;
    using System.Reflection.Emit;
    using System.Security.Permissions;

    internal sealed class CompiledRegexRunnerFactory : RegexRunnerFactory
    {
        private DynamicMethod findFirstCharMethod;
        private DynamicMethod goMethod;
        private DynamicMethod initTrackCountMethod;

        internal CompiledRegexRunnerFactory(DynamicMethod go, DynamicMethod firstChar, DynamicMethod trackCount)
        {
            this.goMethod = go;
            this.findFirstCharMethod = firstChar;
            this.initTrackCountMethod = trackCount;
        }

        protected internal override RegexRunner CreateInstance()
        {
            CompiledRegexRunner runner = new CompiledRegexRunner();
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
            runner.SetDelegates((NoParamDelegate) this.goMethod.CreateDelegate(typeof(NoParamDelegate)), (FindFirstCharDelegate) this.findFirstCharMethod.CreateDelegate(typeof(FindFirstCharDelegate)), (NoParamDelegate) this.initTrackCountMethod.CreateDelegate(typeof(NoParamDelegate)));
            return runner;
        }
    }
}

