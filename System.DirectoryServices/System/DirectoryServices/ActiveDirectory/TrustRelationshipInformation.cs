namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    public class TrustRelationshipInformation
    {
        internal DirectoryContext context;
        internal System.DirectoryServices.ActiveDirectory.TrustDirection direction;
        internal string source;
        internal string target;
        internal System.DirectoryServices.ActiveDirectory.TrustType type;

        internal TrustRelationshipInformation()
        {
        }

        internal TrustRelationshipInformation(DirectoryContext context, string source, TrustObject obj)
        {
            this.context = context;
            this.source = source;
            this.target = (obj.DnsDomainName == null) ? obj.NetbiosDomainName : obj.DnsDomainName;
            if (((obj.Flags & 2) != 0) && ((obj.Flags & 0x20) != 0))
            {
                this.direction = System.DirectoryServices.ActiveDirectory.TrustDirection.Bidirectional;
            }
            else if ((obj.Flags & 2) != 0)
            {
                this.direction = System.DirectoryServices.ActiveDirectory.TrustDirection.Outbound;
            }
            else if ((obj.Flags & 0x20) != 0)
            {
                this.direction = System.DirectoryServices.ActiveDirectory.TrustDirection.Inbound;
            }
            this.type = obj.TrustType;
        }

        public string SourceName
        {
            get
            {
                return this.source;
            }
        }

        public string TargetName
        {
            get
            {
                return this.target;
            }
        }

        public System.DirectoryServices.ActiveDirectory.TrustDirection TrustDirection
        {
            get
            {
                return this.direction;
            }
        }

        public System.DirectoryServices.ActiveDirectory.TrustType TrustType
        {
            get
            {
                return this.type;
            }
        }
    }
}

