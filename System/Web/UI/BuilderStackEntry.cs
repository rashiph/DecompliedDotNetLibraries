namespace System.Web.UI
{
    using System;

    internal class BuilderStackEntry : SourceLineInfo
    {
        internal ControlBuilder _builder;
        internal string _inputText;
        internal int _repeatCount;
        internal string _tagName;
        internal int _textPos;

        internal BuilderStackEntry(ControlBuilder builder, string tagName, string virtualPath, int line, string inputText, int textPos)
        {
            this._builder = builder;
            this._tagName = tagName;
            base.VirtualPath = virtualPath;
            base.Line = line;
            this._inputText = inputText;
            this._textPos = textPos;
        }
    }
}

