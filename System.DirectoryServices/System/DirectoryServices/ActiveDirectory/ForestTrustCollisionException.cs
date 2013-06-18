namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ForestTrustCollisionException : ActiveDirectoryOperationException, ISerializable
    {
        private ForestTrustRelationshipCollisionCollection collisions;

        public ForestTrustCollisionException() : base(Res.GetString("ForestTrustCollision"))
        {
            this.collisions = new ForestTrustRelationshipCollisionCollection();
        }

        public ForestTrustCollisionException(string message) : base(message)
        {
            this.collisions = new ForestTrustRelationshipCollisionCollection();
        }

        protected ForestTrustCollisionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.collisions = new ForestTrustRelationshipCollisionCollection();
        }

        public ForestTrustCollisionException(string message, Exception inner) : base(message, inner)
        {
            this.collisions = new ForestTrustRelationshipCollisionCollection();
        }

        public ForestTrustCollisionException(string message, Exception inner, ForestTrustRelationshipCollisionCollection collisions) : base(message, inner)
        {
            this.collisions = new ForestTrustRelationshipCollisionCollection();
            this.collisions = collisions;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public ForestTrustRelationshipCollisionCollection Collisions
        {
            get
            {
                return this.collisions;
            }
        }
    }
}

