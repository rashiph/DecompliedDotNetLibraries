namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;
    using System.Xml.Linq;

    internal class LoadWorkflowAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        private Dictionary<Guid, IDictionary<XName, InstanceValue>> associatedInstanceKeys;
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[LoadInstance]", new object[] { "[System.Activities.DurableInstancing]" });
        private Dictionary<Guid, IDictionary<XName, InstanceValue>> completedInstanceKeys;
        private Dictionary<XName, InstanceValue> instanceData;
        private Dictionary<XName, InstanceValue> instanceMetadata;
        private IObjectSerializer objectSerializer;

        public LoadWorkflowAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
            this.associatedInstanceKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            this.completedInstanceKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            this.objectSerializer = ObjectSerializerFactory.GetDefaultObjectSerializer();
        }

        protected void GenerateLoadSqlCommand(SqlCommand command, LoadType loadType, Guid keyToLoadBy, Guid instanceId, List<CorrelationKey> keysToAssociate)
        {
            long surrogateLockOwnerId = base.StoreLock.SurrogateLockOwnerId;
            byte[] buffer = null;
            bool flag = (keysToAssociate != null) && (keysToAssociate.Count == 1);
            if (keysToAssociate != null)
            {
                buffer = SerializationUtilities.CreateKeyBinaryBlob(keysToAssociate);
            }
            double totalMilliseconds = base.TimeoutHelper.RemainingTime().TotalMilliseconds;
            SqlParameterCollection parameters = command.Parameters;
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@surrogateLockOwnerId",
                SqlDbType = SqlDbType.BigInt,
                Value = surrogateLockOwnerId
            };
            parameters.Add(parameter);
            SqlParameter parameter2 = new SqlParameter {
                ParameterName = "@operationType",
                SqlDbType = SqlDbType.TinyInt,
                Value = loadType
            };
            parameters.Add(parameter2);
            SqlParameter parameter3 = new SqlParameter {
                ParameterName = "@keyToLoadBy",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = keyToLoadBy
            };
            parameters.Add(parameter3);
            SqlParameter parameter4 = new SqlParameter {
                ParameterName = "@instanceId",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = instanceId
            };
            parameters.Add(parameter4);
            SqlParameter parameter5 = new SqlParameter {
                ParameterName = "@handleInstanceVersion",
                SqlDbType = SqlDbType.BigInt,
                Value = base.InstancePersistenceContext.InstanceVersion
            };
            parameters.Add(parameter5);
            SqlParameter parameter6 = new SqlParameter {
                ParameterName = "@handleIsBoundToLock",
                SqlDbType = SqlDbType.Bit,
                Value = base.InstancePersistenceContext.InstanceView.IsBoundToLock
            };
            parameters.Add(parameter6);
            SqlParameter parameter7 = new SqlParameter {
                ParameterName = "@keysToAssociate",
                SqlDbType = SqlDbType.Xml,
                Value = flag ? DBNull.Value : SerializationUtilities.CreateCorrelationKeyXmlBlob(keysToAssociate)
            };
            parameters.Add(parameter7);
            SqlParameter parameter8 = new SqlParameter {
                ParameterName = "@encodingOption",
                SqlDbType = SqlDbType.TinyInt,
                Value = base.Store.InstanceEncodingOption
            };
            parameters.Add(parameter8);
            SqlParameter parameter9 = new SqlParameter {
                ParameterName = "@concatenatedKeyProperties",
                SqlDbType = SqlDbType.VarBinary,
                Value = buffer ?? DBNull.Value
            };
            parameters.Add(parameter9);
            SqlParameter parameter10 = new SqlParameter {
                ParameterName = "@operationTimeout",
                SqlDbType = SqlDbType.Int,
                Value = (totalMilliseconds < 2147483647.0) ? Convert.ToInt32(totalMilliseconds) : 0x7fffffff
            };
            parameters.Add(parameter10);
            SqlParameter parameter11 = new SqlParameter {
                ParameterName = "@singleKeyId",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = flag ? ((object) keysToAssociate[0].KeyId) : ((object) DBNull.Value)
            };
            parameters.Add(parameter11);
        }

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            LoadWorkflowCommand instancePersistenceCommand = base.InstancePersistenceCommand as LoadWorkflowCommand;
            LoadType loadType = instancePersistenceCommand.AcceptUninitializedInstance ? LoadType.LoadOrCreateByInstance : LoadType.LoadByInstance;
            Guid instanceId = base.InstancePersistenceContext.InstanceView.InstanceId;
            this.GenerateLoadSqlCommand(command, loadType, Guid.Empty, instanceId, null);
        }

        protected override string GetSqlCommandText()
        {
            return commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception nextResultSet = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
            if (nextResultSet == null)
            {
                Guid instanceId = reader.GetGuid(1);
                reader.GetInt64(2);
                byte[] primitiveDataProperties = reader.IsDBNull(3) ? null : ((byte[]) reader.GetValue(3));
                byte[] complexDataProperties = reader.IsDBNull(4) ? null : ((byte[]) reader.GetValue(4));
                byte[] serializedMetadataProperties = reader.IsDBNull(5) ? null : ((byte[]) reader.GetValue(5));
                InstanceEncodingOption @byte = (InstanceEncodingOption) reader.GetByte(6);
                InstanceEncodingOption instanceEncodingOption = (InstanceEncodingOption) reader.GetByte(7);
                long instanceVersion = reader.GetInt64(8);
                bool boolean = reader.GetBoolean(9);
                bool flag2 = reader.GetBoolean(10);
                InstancePersistenceCommand instancePersistenceCommand = base.InstancePersistenceCommand;
                LoadWorkflowByInstanceKeyCommand command = base.InstancePersistenceCommand as LoadWorkflowByInstanceKeyCommand;
                if (!base.InstancePersistenceContext.InstanceView.IsBoundToInstance)
                {
                    base.InstancePersistenceContext.BindInstance(instanceId);
                }
                if (!base.InstancePersistenceContext.InstanceView.IsBoundToInstanceOwner)
                {
                    base.InstancePersistenceContext.BindInstanceOwner(base.StoreLock.LockOwnerId, base.StoreLock.LockOwnerId);
                }
                if (!base.InstancePersistenceContext.InstanceView.IsBoundToLock)
                {
                    ((InstanceLockTracking) base.InstancePersistenceContext.UserContext).TrackStoreLock(instanceId, instanceVersion, base.DependentTransaction);
                    base.InstancePersistenceContext.BindAcquiredLock(instanceVersion);
                }
                this.instanceData = SerializationUtilities.DeserializePropertyBag(primitiveDataProperties, complexDataProperties, @byte);
                this.instanceMetadata = SerializationUtilities.DeserializeMetadataPropertyBag(serializedMetadataProperties, instanceEncodingOption);
                if (!flag2)
                {
                    this.ReadInstanceMetadataChanges(reader, this.instanceMetadata);
                    this.ReadKeyData(reader, this.associatedInstanceKeys, this.completedInstanceKeys);
                }
                else if (command != null)
                {
                    foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair in command.InstanceKeysToAssociate)
                    {
                        this.associatedInstanceKeys.Add(pair.Key, pair.Value);
                    }
                    if (!this.associatedInstanceKeys.ContainsKey(command.LookupInstanceKey))
                    {
                        base.InstancePersistenceContext.AssociatedInstanceKey(command.LookupInstanceKey);
                        this.associatedInstanceKeys.Add(command.LookupInstanceKey, new Dictionary<XName, InstanceValue>());
                    }
                }
                if (command != null)
                {
                    foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair2 in command.InstanceKeysToAssociate)
                    {
                        base.InstancePersistenceContext.AssociatedInstanceKey(pair2.Key);
                        if (pair2.Value != null)
                        {
                            foreach (KeyValuePair<XName, InstanceValue> pair3 in pair2.Value)
                            {
                                base.InstancePersistenceContext.WroteInstanceKeyMetadataValue(pair2.Key, pair3.Key, pair3.Value);
                            }
                        }
                    }
                }
                base.InstancePersistenceContext.LoadedInstance(boolean ? InstanceState.Initialized : InstanceState.Uninitialized, this.instanceData, this.instanceMetadata, this.associatedInstanceKeys, this.completedInstanceKeys);
                return nextResultSet;
            }
            if (nextResultSet is InstanceLockLostException)
            {
                base.InstancePersistenceContext.InstanceHandle.Free();
            }
            return nextResultSet;
        }

        private void ReadInstanceMetadataChanges(SqlDataReader reader, Dictionary<XName, InstanceValue> instanceMetadata)
        {
            if ((StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader) != null) || !reader.IsDBNull(1))
            {
                do
                {
                    InstanceEncodingOption @byte = (InstanceEncodingOption) reader.GetByte(1);
                    byte[] serializedMetadataProperties = (byte[]) reader.GetValue(2);
                    foreach (KeyValuePair<XName, InstanceValue> pair in SerializationUtilities.DeserializeMetadataPropertyBag(serializedMetadataProperties, @byte))
                    {
                        XName key = pair.Key;
                        InstanceValue value2 = pair.Value;
                        if (value2.Value is DeletedMetadataValue)
                        {
                            instanceMetadata.Remove(key);
                        }
                        else
                        {
                            instanceMetadata[key] = value2;
                        }
                    }
                }
                while (reader.Read());
            }
        }

        private void ReadKeyData(SqlDataReader reader, Dictionary<Guid, IDictionary<XName, InstanceValue>> associatedInstanceKeys, Dictionary<Guid, IDictionary<XName, InstanceValue>> completedInstanceKeys)
        {
            if ((StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader) == null) && !reader.IsDBNull(1))
            {
                do
                {
                    Guid guid = reader.GetGuid(1);
                    bool boolean = reader.GetBoolean(2);
                    InstanceEncodingOption @byte = (InstanceEncodingOption) reader.GetByte(3);
                    Dictionary<Guid, IDictionary<XName, InstanceValue>> dictionary = boolean ? associatedInstanceKeys : completedInstanceKeys;
                    if (!reader.IsDBNull(4))
                    {
                        dictionary[guid] = SerializationUtilities.DeserializeKeyMetadata((byte[]) reader.GetValue(4), @byte);
                    }
                    else
                    {
                        dictionary[guid] = new Dictionary<XName, InstanceValue>();
                    }
                }
                while (reader.Read());
            }
        }
    }
}

