namespace System.Web.Security
{
    using System;

    [Serializable]
    internal class AnonymousIdData
    {
        internal string AnonymousId;
        internal DateTime ExpireDate;

        internal AnonymousIdData(string id, DateTime dt)
        {
            this.ExpireDate = dt;
            this.AnonymousId = (dt > DateTime.UtcNow) ? id : null;
        }
    }
}

