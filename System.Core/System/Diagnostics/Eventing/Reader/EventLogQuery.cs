namespace System.Diagnostics.Eventing.Reader
{
    using System;

    public class EventLogQuery
    {
        private string path;
        private PathType pathType;
        private string query;
        private bool reverseDirection;
        private EventLogSession session;
        private bool tolerateErrors;

        public EventLogQuery(string path, PathType pathType) : this(path, pathType, null)
        {
        }

        public EventLogQuery(string path, PathType pathType, string query)
        {
            this.session = EventLogSession.GlobalSession;
            this.path = path;
            this.pathType = pathType;
            if (query == null)
            {
                if (path == null)
                {
                    throw new ArgumentNullException("path");
                }
            }
            else
            {
                this.query = query;
            }
        }

        internal string Path
        {
            get
            {
                return this.path;
            }
        }

        internal string Query
        {
            get
            {
                return this.query;
            }
        }

        public bool ReverseDirection
        {
            get
            {
                return this.reverseDirection;
            }
            set
            {
                this.reverseDirection = value;
            }
        }

        public EventLogSession Session
        {
            get
            {
                return this.session;
            }
            set
            {
                this.session = value;
            }
        }

        internal PathType ThePathType
        {
            get
            {
                return this.pathType;
            }
        }

        public bool TolerateQueryErrors
        {
            get
            {
                return this.tolerateErrors;
            }
            set
            {
                this.tolerateErrors = value;
            }
        }
    }
}

