namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Text;
    using System.Xml.Linq;

    internal static class StoreUtilities
    {
        public static Exception CheckRemainingResultSetForErrors(XName commandName, SqlDataReader reader)
        {
            Exception nextResultSet = null;
            do
            {
                nextResultSet = GetNextResultSet(commandName, reader);
            }
            while ((nextResultSet == null) && reader.NextResult());
            return nextResultSet;
        }

        public static Exception CheckResult(XName commandName, SqlDataReader reader)
        {
            Exception exception = null;
            CommandResult result = (CommandResult) reader.GetInt32(0);
            if (result != CommandResult.Success)
            {
                exception = GetError(commandName, result, reader);
            }
            return exception;
        }

        public static SqlConnection CreateConnection(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static Exception GetError(XName commandName, CommandResult result, SqlDataReader reader)
        {
            Exception exception = null;
            switch (result)
            {
                case CommandResult.InstanceNotFound:
                    return new InstanceNotReadyException(commandName, reader.GetGuid(1));

                case CommandResult.InstanceLockNotAcquired:
                    return new InstanceLockedException(commandName, reader.GetGuid(1), reader.GetGuid(2), ReadLockOwnerMetadata(reader));

                case CommandResult.KeyAlreadyExists:
                    return new InstanceKeyCollisionException(commandName, Guid.Empty, new InstanceKey(reader.GetGuid(1)), Guid.Empty);

                case CommandResult.KeyNotFound:
                    return new InstanceKeyNotReadyException(commandName, new InstanceKey(reader.GetGuid(1)));

                case CommandResult.InstanceAlreadyExists:
                    return new InstanceCollisionException(commandName, reader.GetGuid(1));

                case CommandResult.InstanceLockLost:
                    return new InstanceLockLostException(commandName, reader.GetGuid(1));

                case CommandResult.InstanceCompleted:
                    return new InstanceCompleteException(commandName, reader.GetGuid(1));

                case CommandResult.KeyDisassociated:
                    return new InstanceKeyCompleteException(commandName, new InstanceKey(reader.GetGuid(1)));

                case CommandResult.StaleInstanceVersion:
                    return new InstanceLockLostException(commandName, reader.GetGuid(1));

                case CommandResult.HostLockExpired:
                    return new InstancePersistenceException(System.Activities.DurableInstancing.SR.HostLockExpired);

                case CommandResult.HostLockNotFound:
                    return new InstancePersistenceException(System.Activities.DurableInstancing.SR.HostLockNotFound);

                case CommandResult.CleanupInProgress:
                    return new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.CleanupInProgress);

                case CommandResult.InstanceAlreadyLockedToOwner:
                    return new InstanceAlreadyLockedToOwnerException(commandName, reader.GetGuid(1), reader.GetInt64(2));

                case CommandResult.Success:
                    return exception;
            }
            return new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.UnknownSprocResult(result));
        }

        public static Exception GetNextResultSet(XName commandName, SqlDataReader reader)
        {
            do
            {
                if (reader.Read())
                {
                    do
                    {
                        if (reader.FieldCount != 0)
                        {
                            string name = reader.GetName(0);
                            if (string.Compare("Result", name, StringComparison.Ordinal) == 0)
                            {
                                return CheckResult(commandName, reader);
                            }
                        }
                    }
                    while (reader.Read());
                }
            }
            while (reader.NextResult());
            return null;
        }

        public static bool HasExpired(this TimeoutHelper timeoutHelper)
        {
            return (timeoutHelper.RemainingTime() <= TimeSpan.Zero);
        }

        private static Dictionary<XName, object> ReadLockOwnerMetadata(SqlDataReader reader)
        {
            Dictionary<XName, object> dictionary = new Dictionary<XName, object>();
            InstanceEncodingOption @byte = (InstanceEncodingOption) reader.GetByte(3);
            byte[] bytes = reader.IsDBNull(4) ? null : ((byte[]) reader.GetValue(4));
            byte[] buffer2 = reader.IsDBNull(5) ? null : ((byte[]) reader.GetValue(5));
            IObjectSerializer objectSerializer = ObjectSerializerFactory.GetObjectSerializer(@byte);
            Dictionary<XName, object>[] dictionaryArray = new Dictionary<XName, object>[2];
            if (bytes != null)
            {
                dictionaryArray[0] = (Dictionary<XName, object>) objectSerializer.DeserializeValue(bytes);
            }
            if (buffer2 != null)
            {
                dictionaryArray[1] = objectSerializer.DeserializePropertyBag(buffer2);
            }
            foreach (Dictionary<XName, object> dictionary2 in dictionaryArray)
            {
                if (dictionary2 != null)
                {
                    foreach (KeyValuePair<XName, object> pair in dictionary2)
                    {
                        dictionary.Add(pair.Key, pair.Value);
                    }
                }
            }
            return dictionary;
        }

        public static void TraceSqlCommand(SqlCommand command, bool isStarting)
        {
            if ((isStarting && TD.StartSqlCommandExecuteIsEnabled()) || (!isStarting && TD.EndSqlCommandExecuteIsEnabled()))
            {
                StringBuilder builder = new StringBuilder(0x200);
                bool flag = false;
                foreach (SqlParameter parameter in command.Parameters)
                {
                    string str;
                    if ((parameter.Value == DBNull.Value) || (parameter.Value == null))
                    {
                        str = "Null";
                    }
                    else if (parameter.DbType == DbType.Binary)
                    {
                        str = "Binary";
                    }
                    else
                    {
                        str = parameter.Value.ToString();
                    }
                    if (flag)
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "{0}='{1}'", new object[] { parameter.ParameterName, str });
                        flag = false;
                    }
                    else
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, ", {0}='{1}'", new object[] { parameter.ParameterName, str });
                    }
                    builder.AppendLine(command.CommandText);
                }
                if (isStarting)
                {
                    TD.StartSqlCommandExecute(builder.ToString());
                }
                else
                {
                    TD.EndSqlCommandExecute(builder.ToString());
                }
            }
        }
    }
}

