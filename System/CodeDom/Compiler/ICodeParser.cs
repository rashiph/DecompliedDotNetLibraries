namespace System.CodeDom.Compiler
{
    using System.CodeDom;
    using System.IO;

    public interface ICodeParser
    {
        CodeCompileUnit Parse(TextReader codeStream);
    }
}

