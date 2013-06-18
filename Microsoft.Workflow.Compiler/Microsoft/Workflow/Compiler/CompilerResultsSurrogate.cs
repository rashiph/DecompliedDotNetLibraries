namespace Microsoft.Workflow.Compiler
{
    using System;
    using System.CodeDom;
    using System.Runtime.Serialization;
    using System.Security;

    internal class CompilerResultsSurrogate : ISerializationSurrogate
    {
        [SecurityCritical]
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SerializableMemberAttributes", new SerializableMemberAttributes((MemberAttributes) obj));
        }

        [SecurityCritical]
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            SerializableMemberAttributes attributes = (SerializableMemberAttributes) info.GetValue("SerializableMemberAttributes", typeof(SerializableMemberAttributes));
            return attributes.ToMemberAttributes();
        }
    }
}

