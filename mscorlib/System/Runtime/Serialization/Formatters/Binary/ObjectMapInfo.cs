namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    internal sealed class ObjectMapInfo
    {
        private string[] memberNames;
        private Type[] memberTypes;
        private int numMembers;
        internal int objectId;

        internal ObjectMapInfo(int objectId, int numMembers, string[] memberNames, Type[] memberTypes)
        {
            this.objectId = objectId;
            this.numMembers = numMembers;
            this.memberNames = memberNames;
            this.memberTypes = memberTypes;
        }

        internal bool isCompatible(int numMembers, string[] memberNames, Type[] memberTypes)
        {
            bool flag = true;
            if (this.numMembers == numMembers)
            {
                for (int i = 0; i < numMembers; i++)
                {
                    if (!this.memberNames[i].Equals(memberNames[i]))
                    {
                        return false;
                    }
                    if ((memberTypes != null) && (this.memberTypes[i] != memberTypes[i]))
                    {
                        return false;
                    }
                }
                return flag;
            }
            return false;
        }
    }
}

