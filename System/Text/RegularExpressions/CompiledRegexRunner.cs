namespace System.Text.RegularExpressions
{
    using System;

    internal sealed class CompiledRegexRunner : RegexRunner
    {
        private FindFirstCharDelegate findFirstCharMethod;
        private NoParamDelegate goMethod;
        private NoParamDelegate initTrackCountMethod;

        internal CompiledRegexRunner()
        {
        }

        protected override bool FindFirstChar()
        {
            return this.findFirstCharMethod(this);
        }

        protected override void Go()
        {
            this.goMethod(this);
        }

        protected override void InitTrackCount()
        {
            this.initTrackCountMethod(this);
        }

        internal void SetDelegates(NoParamDelegate go, FindFirstCharDelegate firstChar, NoParamDelegate trackCount)
        {
            this.goMethod = go;
            this.findFirstCharMethod = firstChar;
            this.initTrackCountMethod = trackCount;
        }
    }
}

