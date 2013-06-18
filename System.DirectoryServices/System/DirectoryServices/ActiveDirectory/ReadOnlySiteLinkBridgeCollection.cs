namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ReadOnlySiteLinkBridgeCollection : ReadOnlyCollectionBase
    {
        internal ReadOnlySiteLinkBridgeCollection()
        {
        }

        internal int Add(ActiveDirectorySiteLinkBridge bridge)
        {
            return base.InnerList.Add(bridge);
        }

        internal void Clear()
        {
            base.InnerList.Clear();
        }

        public bool Contains(ActiveDirectorySiteLinkBridge bridge)
        {
            if (bridge == null)
            {
                throw new ArgumentNullException("bridge");
            }
            string str = (string) PropertyManager.GetPropertyValue(bridge.context, bridge.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySiteLinkBridge bridge2 = (ActiveDirectorySiteLinkBridge) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(bridge2.context, bridge2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySiteLinkBridge[] bridges, int index)
        {
            base.InnerList.CopyTo(bridges, index);
        }

        public int IndexOf(ActiveDirectorySiteLinkBridge bridge)
        {
            if (bridge == null)
            {
                throw new ArgumentNullException("bridge");
            }
            string str = (string) PropertyManager.GetPropertyValue(bridge.context, bridge.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySiteLinkBridge bridge2 = (ActiveDirectorySiteLinkBridge) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(bridge2.context, bridge2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public ActiveDirectorySiteLinkBridge this[int index]
        {
            get
            {
                return (ActiveDirectorySiteLinkBridge) base.InnerList[index];
            }
        }
    }
}

