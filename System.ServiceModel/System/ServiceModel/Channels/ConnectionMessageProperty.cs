namespace System.ServiceModel.Channels
{
    using System;

    internal class ConnectionMessageProperty
    {
        private IConnection connection;

        public ConnectionMessageProperty(IConnection connection)
        {
            this.connection = connection;
        }

        public IConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public static string Name
        {
            get
            {
                return "iconnection";
            }
        }
    }
}

