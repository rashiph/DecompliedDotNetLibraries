namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;

    public interface ICodeDomDesignerReload
    {
        bool ShouldReloadDesigner(CodeCompileUnit newTree);
    }
}

