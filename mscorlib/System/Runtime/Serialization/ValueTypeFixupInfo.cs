namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection;

    internal class ValueTypeFixupInfo
    {
        private long m_containerID;
        private FieldInfo m_parentField;
        private int[] m_parentIndex;

        public ValueTypeFixupInfo(long containerID, FieldInfo member, int[] parentIndex)
        {
            if ((member == null) && (parentIndex == null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustSupplyParent"));
            }
            if ((containerID == 0L) && (member == null))
            {
                this.m_containerID = containerID;
                this.m_parentField = member;
                this.m_parentIndex = parentIndex;
            }
            if (member != null)
            {
                if (parentIndex != null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MemberAndArray"));
                }
                if (member.FieldType.IsValueType && (containerID == 0L))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MustSupplyContainer"));
                }
            }
            this.m_containerID = containerID;
            this.m_parentField = member;
            this.m_parentIndex = parentIndex;
        }

        public long ContainerID
        {
            get
            {
                return this.m_containerID;
            }
        }

        public FieldInfo ParentField
        {
            get
            {
                return this.m_parentField;
            }
        }

        public int[] ParentIndex
        {
            get
            {
                return this.m_parentIndex;
            }
        }
    }
}

