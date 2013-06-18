namespace System.DirectoryServices.Protocols
{
    using System;

    internal class LdapPartialAsyncResult : LdapAsyncResult
    {
        internal LdapConnection con;
        internal Exception exception;
        internal int messageID;
        internal bool partialCallback;
        internal TimeSpan requestTimeout;
        internal SearchResponse response;
        internal ResultsStatus resultStatus;
        internal DateTime startTime;

        public LdapPartialAsyncResult(int messageID, AsyncCallback callbackRoutine, object state, bool partialResults, LdapConnection con, bool partialCallback, TimeSpan requestTimeout) : base(callbackRoutine, state, partialResults)
        {
            this.messageID = -1;
            this.messageID = messageID;
            this.con = con;
            base.partialResults = true;
            this.partialCallback = partialCallback;
            this.requestTimeout = requestTimeout;
            this.startTime = DateTime.Now;
        }
    }
}

