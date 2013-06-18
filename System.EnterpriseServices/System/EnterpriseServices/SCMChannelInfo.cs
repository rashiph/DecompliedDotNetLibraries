namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.Remoting;

    [Serializable]
    internal class SCMChannelInfo : IChannelInfo
    {
        public virtual object[] ChannelData
        {
            get
            {
                return new object[0];
            }
            set
            {
            }
        }
    }
}

