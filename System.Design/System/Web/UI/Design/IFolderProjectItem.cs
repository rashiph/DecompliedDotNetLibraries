namespace System.Web.UI.Design
{
    using System;
    using System.Collections;

    public interface IFolderProjectItem
    {
        IDocumentProjectItem AddDocument(string name, byte[] content);
        IFolderProjectItem AddFolder(string name);

        ICollection Children { get; }
    }
}

