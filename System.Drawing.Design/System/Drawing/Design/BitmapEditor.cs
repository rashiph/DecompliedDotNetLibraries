namespace System.Drawing.Design
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class BitmapEditor : ImageEditor
    {
        protected override string[] GetExtensions()
        {
            return new string[] { "bmp", "gif", "jpg", "jpeg", "png", "ico" };
        }

        protected override string GetFileDialogDescription()
        {
            return System.Drawing.Design.SR.GetString("bitmapFileDescription");
        }

        protected override Image LoadFromStream(Stream stream)
        {
            return new Bitmap(stream);
        }
    }
}

