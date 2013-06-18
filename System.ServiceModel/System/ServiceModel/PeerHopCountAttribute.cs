namespace System.ServiceModel
{
    using System;
    using System.Net.Security;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
    public sealed class PeerHopCountAttribute : MessageHeaderAttribute
    {
        public PeerHopCountAttribute()
        {
            base.Name = "Hops";
            base.Namespace = "http://schemas.microsoft.com/net/2006/05/peer/HopCount";
            base.ProtectionLevel = System.Net.Security.ProtectionLevel.None;
            base.MustUnderstand = false;
        }

        public string Actor
        {
            get
            {
                return base.Actor;
            }
        }

        public bool MustUnderstand
        {
            get
            {
                return base.MustUnderstand;
            }
        }

        public string Name
        {
            get
            {
                return base.Name;
            }
        }

        public string Namespace
        {
            get
            {
                return base.Namespace;
            }
        }

        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return base.ProtectionLevel;
            }
        }

        public bool Relay
        {
            get
            {
                return base.Relay;
            }
        }
    }
}

