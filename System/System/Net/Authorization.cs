namespace System.Net
{
    using System;

    public class Authorization
    {
        private bool m_Complete;
        private string m_ConnectionGroupId;
        private string m_Message;
        private bool m_MutualAuth;
        private string[] m_ProtectionRealm;

        public Authorization(string token)
        {
            this.m_Message = ValidationHelper.MakeStringNull(token);
            this.m_Complete = true;
        }

        public Authorization(string token, bool finished)
        {
            this.m_Message = ValidationHelper.MakeStringNull(token);
            this.m_Complete = finished;
        }

        public Authorization(string token, bool finished, string connectionGroupId) : this(token, finished, connectionGroupId, false)
        {
        }

        internal Authorization(string token, bool finished, string connectionGroupId, bool mutualAuth)
        {
            this.m_Message = ValidationHelper.MakeStringNull(token);
            this.m_ConnectionGroupId = ValidationHelper.MakeStringNull(connectionGroupId);
            this.m_Complete = finished;
            this.m_MutualAuth = mutualAuth;
        }

        internal void SetComplete(bool complete)
        {
            this.m_Complete = complete;
        }

        public bool Complete
        {
            get
            {
                return this.m_Complete;
            }
        }

        public string ConnectionGroupId
        {
            get
            {
                return this.m_ConnectionGroupId;
            }
        }

        public string Message
        {
            get
            {
                return this.m_Message;
            }
        }

        public bool MutuallyAuthenticated
        {
            get
            {
                return (this.Complete && this.m_MutualAuth);
            }
            set
            {
                this.m_MutualAuth = value;
            }
        }

        public string[] ProtectionRealm
        {
            get
            {
                return this.m_ProtectionRealm;
            }
            set
            {
                string[] strArray = ValidationHelper.MakeEmptyArrayNull(value);
                this.m_ProtectionRealm = strArray;
            }
        }
    }
}

