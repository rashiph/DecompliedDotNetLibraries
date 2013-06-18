namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class TrustRelationshipInformationCollection : ReadOnlyCollectionBase
    {
        internal TrustRelationshipInformationCollection()
        {
        }

        internal TrustRelationshipInformationCollection(DirectoryContext context, string source, ArrayList trusts)
        {
            for (int i = 0; i < trusts.Count; i++)
            {
                TrustObject obj2 = (TrustObject) trusts[i];
                if ((obj2.TrustType != TrustType.Forest) && (obj2.TrustType != (TrustType.Unknown | TrustType.ParentChild)))
                {
                    TrustRelationshipInformation info = new TrustRelationshipInformation(context, source, obj2);
                    this.Add(info);
                }
            }
        }

        internal int Add(TrustRelationshipInformation info)
        {
            return base.InnerList.Add(info);
        }

        public bool Contains(TrustRelationshipInformation information)
        {
            if (information == null)
            {
                throw new ArgumentNullException("information");
            }
            return base.InnerList.Contains(information);
        }

        public void CopyTo(TrustRelationshipInformation[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(TrustRelationshipInformation information)
        {
            if (information == null)
            {
                throw new ArgumentNullException("information");
            }
            return base.InnerList.IndexOf(information);
        }

        public TrustRelationshipInformation this[int index]
        {
            get
            {
                return (TrustRelationshipInformation) base.InnerList[index];
            }
        }
    }
}

