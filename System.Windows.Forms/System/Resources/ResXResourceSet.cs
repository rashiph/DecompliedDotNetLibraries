namespace System.Resources
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ResXResourceSet : ResourceSet
    {
        public ResXResourceSet(Stream stream)
        {
            base.Reader = new ResXResourceReader(stream);
            base.Table = new Hashtable();
            this.ReadResources();
        }

        public ResXResourceSet(string fileName)
        {
            base.Reader = new ResXResourceReader(fileName);
            base.Table = new Hashtable();
            this.ReadResources();
        }

        public override Type GetDefaultReader()
        {
            return typeof(ResXResourceReader);
        }

        public override Type GetDefaultWriter()
        {
            return typeof(ResXResourceWriter);
        }
    }
}

