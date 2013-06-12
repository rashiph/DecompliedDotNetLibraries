namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class MemberRelationshipService
    {
        private Dictionary<RelationshipEntry, RelationshipEntry> _relationships = new Dictionary<RelationshipEntry, RelationshipEntry>();

        protected MemberRelationshipService()
        {
        }

        protected virtual MemberRelationship GetRelationship(MemberRelationship source)
        {
            RelationshipEntry entry;
            if (((this._relationships != null) && this._relationships.TryGetValue(new RelationshipEntry(source), out entry)) && entry.Owner.IsAlive)
            {
                return new MemberRelationship(entry.Owner.Target, entry.Member);
            }
            return MemberRelationship.Empty;
        }

        protected virtual void SetRelationship(MemberRelationship source, MemberRelationship relationship)
        {
            if (!relationship.IsEmpty && !this.SupportsRelationship(source, relationship))
            {
                string componentName = TypeDescriptor.GetComponentName(source.Owner);
                string str2 = TypeDescriptor.GetComponentName(relationship.Owner);
                if (componentName == null)
                {
                    componentName = source.Owner.ToString();
                }
                if (str2 == null)
                {
                    str2 = relationship.Owner.ToString();
                }
                throw new ArgumentException(SR.GetString("MemberRelationshipService_RelationshipNotSupported", new object[] { componentName, source.Member.Name, str2, relationship.Member.Name }));
            }
            if (this._relationships == null)
            {
                this._relationships = new Dictionary<RelationshipEntry, RelationshipEntry>();
            }
            this._relationships[new RelationshipEntry(source)] = new RelationshipEntry(relationship);
        }

        public abstract bool SupportsRelationship(MemberRelationship source, MemberRelationship relationship);

        public MemberRelationship this[MemberRelationship source]
        {
            get
            {
                if (source.Owner == null)
                {
                    throw new ArgumentNullException("Owner");
                }
                if (source.Member == null)
                {
                    throw new ArgumentNullException("Member");
                }
                return this.GetRelationship(source);
            }
            set
            {
                if (source.Owner == null)
                {
                    throw new ArgumentNullException("Owner");
                }
                if (source.Member == null)
                {
                    throw new ArgumentNullException("Member");
                }
                this.SetRelationship(source, value);
            }
        }

        public MemberRelationship this[object sourceOwner, MemberDescriptor sourceMember]
        {
            get
            {
                if (sourceOwner == null)
                {
                    throw new ArgumentNullException("sourceOwner");
                }
                if (sourceMember == null)
                {
                    throw new ArgumentNullException("sourceMember");
                }
                return this.GetRelationship(new MemberRelationship(sourceOwner, sourceMember));
            }
            set
            {
                if (sourceOwner == null)
                {
                    throw new ArgumentNullException("sourceOwner");
                }
                if (sourceMember == null)
                {
                    throw new ArgumentNullException("sourceMember");
                }
                this.SetRelationship(new MemberRelationship(sourceOwner, sourceMember), value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RelationshipEntry
        {
            internal WeakReference Owner;
            internal MemberDescriptor Member;
            private int hashCode;
            internal RelationshipEntry(MemberRelationship rel)
            {
                this.Owner = new WeakReference(rel.Owner);
                this.Member = rel.Member;
                this.hashCode = (rel.Owner == null) ? 0 : rel.Owner.GetHashCode();
            }

            public override bool Equals(object o)
            {
                if (o is MemberRelationshipService.RelationshipEntry)
                {
                    MemberRelationshipService.RelationshipEntry entry = (MemberRelationshipService.RelationshipEntry) o;
                    return (this == entry);
                }
                return false;
            }

            public static bool operator ==(MemberRelationshipService.RelationshipEntry re1, MemberRelationshipService.RelationshipEntry re2)
            {
                object obj2 = re1.Owner.IsAlive ? re1.Owner.Target : null;
                object obj3 = re2.Owner.IsAlive ? re2.Owner.Target : null;
                return ((obj2 == obj3) && re1.Member.Equals(re2.Member));
            }

            public static bool operator !=(MemberRelationshipService.RelationshipEntry re1, MemberRelationshipService.RelationshipEntry re2)
            {
                return !(re1 == re2);
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }
        }
    }
}

