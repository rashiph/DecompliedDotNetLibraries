namespace System.Web.UI.Design
{
    using System;
    using System.IO;

    public interface IDocumentProjectItem
    {
        Stream GetContents();
        void Open();
    }
}

