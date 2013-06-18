namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    public class ForestTrustRelationshipCollision
    {
        private DomainCollisionOptions domainFlag;
        private string record;
        private TopLevelNameCollisionOptions tlnFlag;
        private ForestTrustCollisionType type;

        internal ForestTrustRelationshipCollision(ForestTrustCollisionType collisionType, TopLevelNameCollisionOptions TLNFlag, DomainCollisionOptions domainFlag, string record)
        {
            this.type = collisionType;
            this.tlnFlag = TLNFlag;
            this.domainFlag = domainFlag;
            this.record = record;
        }

        public string CollisionRecord
        {
            get
            {
                return this.record;
            }
        }

        public ForestTrustCollisionType CollisionType
        {
            get
            {
                return this.type;
            }
        }

        public DomainCollisionOptions DomainCollisionOption
        {
            get
            {
                return this.domainFlag;
            }
        }

        public TopLevelNameCollisionOptions TopLevelNameCollisionOption
        {
            get
            {
                return this.tlnFlag;
            }
        }
    }
}

