namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    internal sealed class IdentityContextData : ILogicalThreadAffinative, ISerializable
    {
        private string identity;
        internal const string IdentityContext = "__identitycontext__";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal IdentityContextData(string identity)
        {
            this.identity = identity;
        }

        private IdentityContextData(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name.Equals("identity"))
                {
                    this.identity = (string) enumerator.Value;
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.identity != null)
            {
                info.AddValue("identity", this.identity.ToString());
            }
        }

        internal string Identity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.identity;
            }
        }
    }
}

