namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ForestTrustRelationshipCollisionCollection : ReadOnlyCollectionBase
    {
        internal ForestTrustRelationshipCollisionCollection()
        {
        }

        internal int Add(ForestTrustRelationshipCollision collision)
        {
            return base.InnerList.Add(collision);
        }

        public bool Contains(ForestTrustRelationshipCollision collision)
        {
            if (collision == null)
            {
                throw new ArgumentNullException("collision");
            }
            return base.InnerList.Contains(collision);
        }

        public void CopyTo(ForestTrustRelationshipCollision[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(ForestTrustRelationshipCollision collision)
        {
            if (collision == null)
            {
                throw new ArgumentNullException("collision");
            }
            return base.InnerList.IndexOf(collision);
        }

        public ForestTrustRelationshipCollision this[int index]
        {
            get
            {
                return (ForestTrustRelationshipCollision) base.InnerList[index];
            }
        }
    }
}

