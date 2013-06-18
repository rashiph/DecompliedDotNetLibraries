namespace System.Web.UI.Design
{
    using System;
    using System.Web.UI.WebControls;

    [Obsolete("Use of this type is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
    public interface ITemplateEditingFrame : IDisposable
    {
        void Close(bool saveChanges);
        void Open();
        void Resize(int width, int height);
        void Save();
        void UpdateControlName(string newName);

        Style ControlStyle { get; }

        int InitialHeight { get; set; }

        int InitialWidth { get; set; }

        string Name { get; }

        string[] TemplateNames { get; }

        Style[] TemplateStyles { get; }

        TemplateEditingVerb Verb { get; set; }
    }
}

