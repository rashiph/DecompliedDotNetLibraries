namespace System.Data.SqlClient
{
    using System;

    internal class SqlNotification : MarshalByRefObject
    {
        private readonly SqlNotificationInfo _info;
        private readonly string _key;
        private readonly SqlNotificationSource _source;
        private readonly SqlNotificationType _type;

        internal SqlNotification(SqlNotificationInfo info, SqlNotificationSource source, SqlNotificationType type, string key)
        {
            this._info = info;
            this._source = source;
            this._type = type;
            this._key = key;
        }

        internal SqlNotificationInfo Info
        {
            get
            {
                return this._info;
            }
        }

        internal string Key
        {
            get
            {
                return this._key;
            }
        }

        internal SqlNotificationSource Source
        {
            get
            {
                return this._source;
            }
        }

        internal SqlNotificationType Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

