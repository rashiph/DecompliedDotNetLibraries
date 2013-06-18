namespace System.Drawing.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class MetafileEditor : ImageEditor
    {
        protected override string[] GetExtensions()
        {
            return new string[] { "emf", "wmf" };
        }

        protected override string GetFileDialogDescription()
        {
            return System.Drawing.Design.SR.GetString("metafileFileDescription");
        }

        protected override Image LoadFromStream(Stream stream)
        {
            return new Metafile(stream);
        }
    }
}

