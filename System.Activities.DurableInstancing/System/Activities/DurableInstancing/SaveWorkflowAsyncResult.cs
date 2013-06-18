namespace System.Activities.DurableInstancing
{
    using System;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;

    internal sealed class SaveWorkflowAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        private string commandText;
        private const string createServiceDeploymentStoredProcedureParameters = "@serviceDeploymentHash, @siteName, @relativeServicePath, @relativeApplicationPath,\r\n                @serviceName, @serviceNamespace, @serviceDeploymentId output";
        private Guid serviceDeploymentHash;
        private long serviceDeploymentId;
        private static Dictionary<Guid, long> serviceDeploymentIdsCache = new Dictionary<Guid, long>();
        private static ReaderWriterLockSlim serviceDeploymentIdsCacheLock = new ReaderWriterLockSlim();
        private const string storedProcedureParameters = "@instanceId, @surrogateLockOwnerId, @handleInstanceVersion, @handleIsBoundToLock,\r\n@primitiveDataProperties, @complexDataProperties, @writeOnlyPrimitiveDataProperties, @writeOnlyComplexDataProperties, @metadataProperties,\r\n@metadataIsConsistent, @encodingOption, @timerDurationMilliseconds, @suspensionStateChange, @suspensionReason, @suspensionExceptionName, @keysToAssociate,\r\n@keysToComplete, @keysToFree, @concatenatedKeyProperties, @unlockInstance, @isReadyToRun, @isCompleted, @singleKeyId,\r\n@lastMachineRunOn, @executionStatus, @blockingBookmarks, @workflowHostType, @serviceDeploymentId, @operationTimeout";

        public SaveWorkflowAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
            if (((SaveWorkflowCommand) command).InstanceKeyMetadataChanges.Count > 0)
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.InstanceKeyMetadataChangesNotSupported));
            }
        }

        private static void AddSerializedProperty(ArraySegment<byte> source, SqlParameterCollection parameters, string parameterName)
        {
            int num = (source.Count > 0x1f40) ? source.Count : -1;
            object obj2 = ((num == -1) ? ((bool) GenerateByteArray(source)) : ((bool) source.Array)) ?? DBNull.Value;
            SqlParameter parameter = new SqlParameter {
                ParameterName = parameterName,
                SqlDbType = SqlDbType.VarBinary,
                Size = num,
                Value = obj2
            };
            parameters.Add(parameter);
        }

        private void ExtractServiceDeploymentInformation(SaveWorkflowCommand saveWorkflowCommand, StringBuilder commandTextBuilder, SqlParameterCollection parameters)
        {
            InstanceValue value2;
            string localName = null;
            string namespaceName = null;
            string str3 = null;
            string str4 = null;
            string str5 = null;
            if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(PersistenceMetadataNamespace.ActivationType, out value2))
            {
                if (!PersistenceMetadataNamespace.ActivationTypes.WAS.Equals(value2.Value))
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.NonWASActivationNotSupported));
                }
                if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.Service, out value2))
                {
                    localName = ((XName) value2.Value).LocalName;
                    namespaceName = ((XName) value2.Value).Namespace.NamespaceName;
                }
                if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.SiteName, out value2))
                {
                    str3 = (string) value2.Value;
                }
                if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.RelativeApplicationPath, out value2))
                {
                    str4 = (string) value2.Value;
                }
                if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.RelativeServicePath, out value2))
                {
                    str5 = (string) value2.Value;
                }
                object[] args = new object[] { localName ?? string.Empty, namespaceName ?? string.Empty, str3 ?? string.Empty, str4 ?? string.Empty, str5 ?? string.Empty };
                byte[] bytes = Encoding.Unicode.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}#{1}#{2}#{3}#{4}", args));
                this.serviceDeploymentHash = new Guid(HashHelper.ComputeHash(bytes));
                this.GetServiceDeploymentId();
            }
            if ((this.serviceDeploymentHash != Guid.Empty) && (this.serviceDeploymentId == 0L))
            {
                commandTextBuilder.AppendLine("declare @serviceDeploymentId bigint");
                commandTextBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "exec {0}.[CreateServiceDeployment] {1} ;", new object[] { "[System.Activities.DurableInstancing]", "@serviceDeploymentHash, @siteName, @relativeServicePath, @relativeApplicationPath,\r\n                @serviceName, @serviceNamespace, @serviceDeploymentId output" }));
                SqlParameter parameter = new SqlParameter {
                    ParameterName = "@serviceDeploymentHash",
                    SqlDbType = SqlDbType.UniqueIdentifier,
                    Value = this.serviceDeploymentHash
                };
                parameters.Add(parameter);
                SqlParameter parameter2 = new SqlParameter {
                    ParameterName = "@serviceName",
                    Size = -1,
                    SqlDbType = SqlDbType.NVarChar,
                    Value = localName ?? DBNull.Value
                };
                parameters.Add(parameter2);
                SqlParameter parameter3 = new SqlParameter {
                    ParameterName = "@serviceNamespace",
                    Size = -1,
                    SqlDbType = SqlDbType.NVarChar,
                    Value = namespaceName ?? DBNull.Value
                };
                parameters.Add(parameter3);
                SqlParameter parameter4 = new SqlParameter {
                    ParameterName = "@siteName",
                    Size = -1,
                    SqlDbType = SqlDbType.NVarChar,
                    Value = str3 ?? DBNull.Value
                };
                parameters.Add(parameter4);
                SqlParameter parameter5 = new SqlParameter {
                    ParameterName = "@relativeServicePath",
                    Size = -1,
                    SqlDbType = SqlDbType.NVarChar,
                    Value = str5 ?? DBNull.Value
                };
                parameters.Add(parameter5);
                SqlParameter parameter6 = new SqlParameter {
                    ParameterName = "@relativeApplicationPath",
                    Size = -1,
                    SqlDbType = SqlDbType.NVarChar,
                    Value = str4 ?? DBNull.Value
                };
                parameters.Add(parameter6);
            }
            else
            {
                SqlParameter parameter7 = new SqlParameter {
                    ParameterName = "@serviceDeploymentId",
                    SqlDbType = SqlDbType.BigInt,
                    Value = (this.serviceDeploymentId != 0L) ? ((object) this.serviceDeploymentId) : ((object) DBNull.Value)
                };
                parameters.Add(parameter7);
            }
        }

        private static byte[] GenerateByteArray(ArraySegment<byte> source)
        {
            if (source.Array != null)
            {
                byte[] dst = new byte[source.Count];
                Buffer.BlockCopy(source.Array, 0, dst, 0, source.Count);
                return dst;
            }
            return null;
        }

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            string str;
            string str2;
            SaveWorkflowCommand instancePersistenceCommand = base.InstancePersistenceCommand as SaveWorkflowCommand;
            StringBuilder commandTextBuilder = new StringBuilder(0x200);
            double totalMilliseconds = base.TimeoutHelper.RemainingTime().TotalMilliseconds;
            SqlParameterCollection parameters = command.Parameters;
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@instanceId",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = base.InstancePersistenceContext.InstanceView.InstanceId
            };
            parameters.Add(parameter);
            SqlParameter parameter2 = new SqlParameter {
                ParameterName = "@surrogateLockOwnerId",
                SqlDbType = SqlDbType.BigInt,
                Value = base.StoreLock.SurrogateLockOwnerId
            };
            parameters.Add(parameter2);
            SqlParameter parameter3 = new SqlParameter {
                ParameterName = "@handleInstanceVersion",
                SqlDbType = SqlDbType.BigInt,
                Value = base.InstancePersistenceContext.InstanceVersion
            };
            parameters.Add(parameter3);
            SqlParameter parameter4 = new SqlParameter {
                ParameterName = "@handleIsBoundToLock",
                SqlDbType = SqlDbType.Bit,
                Value = base.InstancePersistenceContext.InstanceView.IsBoundToLock
            };
            parameters.Add(parameter4);
            SqlParameter parameter5 = new SqlParameter {
                ParameterName = "@timerDurationMilliseconds",
                SqlDbType = SqlDbType.BigInt,
                Value = GetPendingTimerExpiration(instancePersistenceCommand) ?? DBNull.Value
            };
            parameters.Add(parameter5);
            SqlParameter parameter6 = new SqlParameter {
                ParameterName = "@unlockInstance",
                SqlDbType = SqlDbType.Bit,
                Value = instancePersistenceCommand.UnlockInstance
            };
            parameters.Add(parameter6);
            SqlParameter parameter7 = new SqlParameter {
                ParameterName = "@suspensionStateChange",
                SqlDbType = SqlDbType.TinyInt,
                Value = GetSuspensionReason(instancePersistenceCommand, out str, out str2)
            };
            parameters.Add(parameter7);
            SqlParameter parameter8 = new SqlParameter {
                ParameterName = "@suspensionReason",
                SqlDbType = SqlDbType.NVarChar,
                Value = str ?? DBNull.Value
            };
            parameters.Add(parameter8);
            SqlParameter parameter9 = new SqlParameter {
                ParameterName = "@suspensionExceptionName",
                SqlDbType = SqlDbType.NVarChar,
                Size = 450,
                Value = str2 ?? DBNull.Value
            };
            parameters.Add(parameter9);
            SqlParameter parameter10 = new SqlParameter {
                ParameterName = "@isCompleted",
                SqlDbType = SqlDbType.Bit,
                Value = instancePersistenceCommand.CompleteInstance
            };
            parameters.Add(parameter10);
            SqlParameter parameter11 = new SqlParameter {
                ParameterName = "@isReadyToRun",
                SqlDbType = SqlDbType.Bit,
                Value = IsReadyToRun(instancePersistenceCommand)
            };
            parameters.Add(parameter11);
            SqlParameter parameter12 = new SqlParameter {
                ParameterName = "@workflowHostType",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = GetWorkflowHostType(instancePersistenceCommand) ?? DBNull.Value
            };
            parameters.Add(parameter12);
            SqlParameter parameter13 = new SqlParameter {
                ParameterName = "@operationTimeout",
                SqlDbType = SqlDbType.Int,
                Value = (totalMilliseconds < 2147483647.0) ? Convert.ToInt32(totalMilliseconds) : 0x7fffffff
            };
            parameters.Add(parameter13);
            commandTextBuilder.AppendLine("set nocount on\r\n\t                                        set transaction isolation level read committed\t\t\r\n\t                                        set xact_abort on\r\n                                            begin transaction");
            this.ExtractServiceDeploymentInformation(instancePersistenceCommand, commandTextBuilder, parameters);
            commandTextBuilder.AppendLine("declare @result int");
            commandTextBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "exec @result = {0}.[SaveInstance] {1} ;", new object[] { "[System.Activities.DurableInstancing]", "@instanceId, @surrogateLockOwnerId, @handleInstanceVersion, @handleIsBoundToLock,\r\n@primitiveDataProperties, @complexDataProperties, @writeOnlyPrimitiveDataProperties, @writeOnlyComplexDataProperties, @metadataProperties,\r\n@metadataIsConsistent, @encodingOption, @timerDurationMilliseconds, @suspensionStateChange, @suspensionReason, @suspensionExceptionName, @keysToAssociate,\r\n@keysToComplete, @keysToFree, @concatenatedKeyProperties, @unlockInstance, @isReadyToRun, @isCompleted, @singleKeyId,\r\n@lastMachineRunOn, @executionStatus, @blockingBookmarks, @workflowHostType, @serviceDeploymentId, @operationTimeout" }));
            commandTextBuilder.AppendLine("if (@result = 0)");
            commandTextBuilder.AppendLine("begin");
            this.SerializeAssociatedData(parameters, instancePersistenceCommand, commandTextBuilder);
            commandTextBuilder.AppendLine("commit transaction");
            commandTextBuilder.AppendLine("end");
            commandTextBuilder.AppendLine("else");
            commandTextBuilder.AppendLine("rollback transaction");
            this.commandText = commandTextBuilder.ToString();
        }

        private static string GetBlockingBookmarks(SaveWorkflowCommand saveWorkflowCommand)
        {
            string str = null;
            InstanceValue value2;
            if (!saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.BinaryBlockingBookmarksPropertyName, out value2))
            {
                return str;
            }
            StringBuilder builder = new StringBuilder(0x200);
            IEnumerable<BookmarkInfo> enumerable = value2.Value as IEnumerable<BookmarkInfo>;
            foreach (BookmarkInfo info in enumerable)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "[{0}: {1}]{2}", new object[] { info.BookmarkName, info.OwnerDisplayName, Environment.NewLine });
            }
            return builder.ToString();
        }

        private static string GetExecutionStatus(SaveWorkflowCommand saveWorkflowCommand)
        {
            string str = null;
            InstanceValue value2;
            if (saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.StatusPropertyName, out value2))
            {
                str = (string) value2.Value;
            }
            return str;
        }

        private static long? GetPendingTimerExpiration(SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceValue value2;
            if (saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.PendingTimerExpirationPropertyName, out value2))
            {
                DateTime time2 = (DateTime) value2.Value;
                TimeSpan span = (TimeSpan) (time2.ToUniversalTime() - DateTime.UtcNow);
                return new long?((long) span.TotalMilliseconds);
            }
            return null;
        }

        private void GetServiceDeploymentId()
        {
            try
            {
                serviceDeploymentIdsCacheLock.EnterReadLock();
                serviceDeploymentIdsCache.TryGetValue(this.serviceDeploymentHash, out this.serviceDeploymentId);
            }
            finally
            {
                serviceDeploymentIdsCacheLock.ExitReadLock();
            }
        }

        protected override string GetSqlCommandText()
        {
            return this.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.Text;
        }

        private static SuspensionStateChange GetSuspensionReason(SaveWorkflowCommand saveWorkflowCommand, out string suspensionReason, out string suspensionExceptionName)
        {
            InstanceValue value2;
            IDictionary<XName, InstanceValue> instanceMetadataChanges = saveWorkflowCommand.InstanceMetadataChanges;
            SuspensionStateChange noChange = SuspensionStateChange.NoChange;
            suspensionReason = null;
            suspensionExceptionName = null;
            if (!instanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.SuspendReason, out value2))
            {
                return noChange;
            }
            if (!value2.IsDeletedValue)
            {
                noChange = SuspensionStateChange.SuspendInstance;
                suspensionReason = (string) value2.Value;
                if (instanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.SuspendException, out value2) && !value2.IsDeletedValue)
                {
                    suspensionExceptionName = ((Exception) value2.Value).GetType().ToString();
                }
                return noChange;
            }
            return SuspensionStateChange.UnsuspendInstance;
        }

        private static Guid? GetWorkflowHostType(SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceValue value2;
            if (!saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowNamespace.WorkflowHostType, out value2))
            {
                return null;
            }
            XName name = value2.Value as XName;
            if (name == null)
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.InvalidWorkflowHostTypeValue(WorkflowNamespace.WorkflowHostType)));
            }
            return new Guid(HashHelper.ComputeHash(Encoding.Unicode.GetBytes(((XName) value2.Value).ToString())));
        }

        private static bool IsReadyToRun(SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceValue value2;
            return (saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.StatusPropertyName, out value2) && (((string) value2.Value) == "Executing"));
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception nextResultSet = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
            if (nextResultSet == null)
            {
                SaveWorkflowCommand instancePersistenceCommand = base.InstancePersistenceCommand as SaveWorkflowCommand;
                InstanceLockTracking userContext = (InstanceLockTracking) base.InstancePersistenceContext.UserContext;
                if ((this.serviceDeploymentHash != Guid.Empty) && (this.serviceDeploymentId == 0L))
                {
                    this.serviceDeploymentId = reader.GetInt64(1);
                    this.PutServiceDeploymentId();
                    nextResultSet = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
                }
                if (nextResultSet == null)
                {
                    if (!base.InstancePersistenceContext.InstanceView.IsBoundToLock)
                    {
                        long instanceVersion = reader.GetInt64(1);
                        userContext.TrackStoreLock(base.InstancePersistenceContext.InstanceView.InstanceId, instanceVersion, base.DependentTransaction);
                        base.InstancePersistenceContext.BindAcquiredLock(instanceVersion);
                    }
                    if (instancePersistenceCommand.InstanceData.Count > 0)
                    {
                        base.InstancePersistenceContext.PersistedInstance(instancePersistenceCommand.InstanceData);
                    }
                    UpdateKeyData(base.InstancePersistenceContext, instancePersistenceCommand);
                    foreach (KeyValuePair<XName, InstanceValue> pair in instancePersistenceCommand.InstanceMetadataChanges)
                    {
                        base.InstancePersistenceContext.WroteInstanceMetadataValue(pair.Key, pair.Value);
                    }
                    if (instancePersistenceCommand.CompleteInstance)
                    {
                        base.InstancePersistenceContext.CompletedInstance();
                    }
                    if (instancePersistenceCommand.UnlockInstance || instancePersistenceCommand.CompleteInstance)
                    {
                        userContext.TrackStoreUnlock(base.DependentTransaction);
                        base.InstancePersistenceContext.InstanceHandle.Free();
                    }
                    return nextResultSet;
                }
                if (nextResultSet is InstanceLockLostException)
                {
                    base.InstancePersistenceContext.InstanceHandle.Free();
                }
            }
            return nextResultSet;
        }

        private void PutServiceDeploymentId()
        {
            try
            {
                serviceDeploymentIdsCacheLock.EnterWriteLock();
                serviceDeploymentIdsCache[this.serviceDeploymentHash] = this.serviceDeploymentId;
            }
            finally
            {
                serviceDeploymentIdsCacheLock.ExitWriteLock();
            }
        }

        private void SerializeAssociatedData(SqlParameterCollection parameters, SaveWorkflowCommand saveWorkflowCommand, StringBuilder commandTextBuilder)
        {
            if (saveWorkflowCommand.CompleteInstance && (base.Store.InstanceCompletionAction == InstanceCompletionAction.DeleteAll))
            {
                SqlParameter parameter = new SqlParameter {
                    ParameterName = "@keysToAssociate",
                    SqlDbType = SqlDbType.Xml,
                    Value = DBNull.Value
                };
                parameters.Add(parameter);
                SqlParameter parameter2 = new SqlParameter {
                    ParameterName = "@singleKeyId",
                    SqlDbType = SqlDbType.UniqueIdentifier,
                    Value = DBNull.Value
                };
                parameters.Add(parameter2);
                SqlParameter parameter3 = new SqlParameter {
                    ParameterName = "@keysToComplete",
                    SqlDbType = SqlDbType.Xml,
                    Value = DBNull.Value
                };
                parameters.Add(parameter3);
                SqlParameter parameter4 = new SqlParameter {
                    ParameterName = "@keysToFree",
                    SqlDbType = SqlDbType.Xml,
                    Value = DBNull.Value
                };
                parameters.Add(parameter4);
                SqlParameter parameter5 = new SqlParameter {
                    ParameterName = "@concatenatedKeyProperties",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = DBNull.Value
                };
                parameters.Add(parameter5);
                SqlParameter parameter6 = new SqlParameter {
                    ParameterName = "@primitiveDataProperties",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = DBNull.Value
                };
                parameters.Add(parameter6);
                SqlParameter parameter7 = new SqlParameter {
                    ParameterName = "@complexDataProperties",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = DBNull.Value
                };
                parameters.Add(parameter7);
                SqlParameter parameter8 = new SqlParameter {
                    ParameterName = "@writeOnlyPrimitiveDataProperties",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = DBNull.Value
                };
                parameters.Add(parameter8);
                SqlParameter parameter9 = new SqlParameter {
                    ParameterName = "@writeOnlyComplexDataProperties",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = DBNull.Value
                };
                parameters.Add(parameter9);
                SqlParameter parameter10 = new SqlParameter {
                    ParameterName = "@metadataProperties",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = DBNull.Value
                };
                parameters.Add(parameter10);
                SqlParameter parameter11 = new SqlParameter {
                    ParameterName = "@metadataIsConsistent",
                    SqlDbType = SqlDbType.Bit,
                    Value = DBNull.Value
                };
                parameters.Add(parameter11);
                SqlParameter parameter12 = new SqlParameter {
                    ParameterName = "@encodingOption",
                    SqlDbType = SqlDbType.TinyInt,
                    Value = DBNull.Value
                };
                parameters.Add(parameter12);
                SqlParameter parameter13 = new SqlParameter {
                    ParameterName = "@lastMachineRunOn",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = DBNull.Value
                };
                parameters.Add(parameter13);
                SqlParameter parameter14 = new SqlParameter {
                    ParameterName = "@executionStatus",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = DBNull.Value
                };
                parameters.Add(parameter14);
                SqlParameter parameter15 = new SqlParameter {
                    ParameterName = "@blockingBookmarks",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = DBNull.Value
                };
                parameters.Add(parameter15);
            }
            else
            {
                List<CorrelationKey> correlationKeys = CorrelationKey.BuildKeyList(saveWorkflowCommand.InstanceKeysToAssociate, base.Store.InstanceEncodingOption);
                List<CorrelationKey> list2 = CorrelationKey.BuildKeyList(saveWorkflowCommand.InstanceKeysToComplete);
                List<CorrelationKey> list3 = CorrelationKey.BuildKeyList(saveWorkflowCommand.InstanceKeysToFree);
                ArraySegment<byte>[] segmentArray = SerializationUtilities.SerializePropertyBag(saveWorkflowCommand.InstanceData, base.Store.InstanceEncodingOption);
                ArraySegment<byte> segment = SerializationUtilities.SerializeMetadataPropertyBag(saveWorkflowCommand, base.InstancePersistenceContext, base.Store.InstanceEncodingOption);
                byte[] buffer = SerializationUtilities.CreateKeyBinaryBlob(correlationKeys);
                bool flag = base.InstancePersistenceContext.InstanceView.InstanceMetadataConsistency == InstanceValueConsistency.None;
                bool flag2 = (correlationKeys != null) && (correlationKeys.Count == 1);
                SqlParameter parameter16 = new SqlParameter {
                    ParameterName = "@keysToAssociate",
                    SqlDbType = SqlDbType.Xml,
                    Value = flag2 ? DBNull.Value : SerializationUtilities.CreateCorrelationKeyXmlBlob(correlationKeys)
                };
                parameters.Add(parameter16);
                SqlParameter parameter17 = new SqlParameter {
                    ParameterName = "@singleKeyId",
                    SqlDbType = SqlDbType.UniqueIdentifier,
                    Value = flag2 ? ((object) correlationKeys[0].KeyId) : ((object) DBNull.Value)
                };
                parameters.Add(parameter17);
                SqlParameter parameter18 = new SqlParameter {
                    ParameterName = "@keysToComplete",
                    SqlDbType = SqlDbType.Xml,
                    Value = SerializationUtilities.CreateCorrelationKeyXmlBlob(list2)
                };
                parameters.Add(parameter18);
                SqlParameter parameter19 = new SqlParameter {
                    ParameterName = "@keysToFree",
                    SqlDbType = SqlDbType.Xml,
                    Value = SerializationUtilities.CreateCorrelationKeyXmlBlob(list3)
                };
                parameters.Add(parameter19);
                SqlParameter parameter20 = new SqlParameter {
                    ParameterName = "@concatenatedKeyProperties",
                    SqlDbType = SqlDbType.VarBinary,
                    Size = -1,
                    Value = buffer ?? DBNull.Value
                };
                parameters.Add(parameter20);
                SqlParameter parameter21 = new SqlParameter {
                    ParameterName = "@metadataIsConsistent",
                    SqlDbType = SqlDbType.Bit,
                    Value = flag
                };
                parameters.Add(parameter21);
                SqlParameter parameter22 = new SqlParameter {
                    ParameterName = "@encodingOption",
                    SqlDbType = SqlDbType.TinyInt,
                    Value = base.Store.InstanceEncodingOption
                };
                parameters.Add(parameter22);
                SqlParameter parameter23 = new SqlParameter {
                    ParameterName = "@lastMachineRunOn",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 450,
                    Value = SqlWorkflowInstanceStoreConstants.MachineName
                };
                parameters.Add(parameter23);
                SqlParameter parameter24 = new SqlParameter {
                    ParameterName = "@executionStatus",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 450,
                    Value = GetExecutionStatus(saveWorkflowCommand) ?? DBNull.Value
                };
                parameters.Add(parameter24);
                SqlParameter parameter25 = new SqlParameter {
                    ParameterName = "@blockingBookmarks",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = -1,
                    Value = GetBlockingBookmarks(saveWorkflowCommand) ?? DBNull.Value
                };
                parameters.Add(parameter25);
                ArraySegment<byte>[] segmentArray2 = new ArraySegment<byte>[] { segmentArray[0], segmentArray[1], segmentArray[2], segmentArray[3], segment };
                string[] strArray = new string[] { "@primitiveDataProperties", "@complexDataProperties", "@writeOnlyPrimitiveDataProperties", "writeOnlyComplexDataProperties", "@metadataProperties" };
                for (int i = 0; i < 5; i++)
                {
                    AddSerializedProperty(segmentArray2[i], parameters, strArray[i]);
                }
                this.SerializePromotedProperties(parameters, commandTextBuilder, saveWorkflowCommand);
            }
        }

        private void SerializePromotedProperties(SqlParameterCollection parameters, StringBuilder commandTextBuilder, SaveWorkflowCommand saveWorkflowCommand)
        {
            int num = 0;
            foreach (KeyValuePair<string, Tuple<List<XName>, List<XName>>> pair in base.Store.Promotions)
            {
                StringBuilder builder = new StringBuilder(0x200);
                int num2 = 1;
                bool flag = false;
                string str = string.Format(CultureInfo.InvariantCulture, "@promotionName{0}", new object[] { num });
                string str2 = string.Format(CultureInfo.InvariantCulture, "@instanceId{0}", new object[] { num });
                builder.Append(string.Format(CultureInfo.InvariantCulture, "exec {0}.[InsertPromotedProperties] ", new object[] { "[System.Activities.DurableInstancing]" }));
                builder.Append("@promotionName=");
                builder.Append(str);
                builder.Append(",");
                builder.Append("@instanceId=");
                builder.Append(str2);
                foreach (XName name in pair.Value.Item1)
                {
                    InstanceValue value2;
                    if (saveWorkflowCommand.InstanceData.TryGetValue(name, out value2))
                    {
                        if (!SerializationUtilities.IsPropertyTypeSqlVariantCompatible(value2))
                        {
                            throw FxTrace.Exception.AsError(new InstancePersistenceException(System.Activities.DurableInstancing.SR.CannotPromoteAsSqlVariant(value2.Value.GetType().ToString(), name.ToString())));
                        }
                        string str3 = string.Format(CultureInfo.InvariantCulture, "@value{0}=", new object[] { num2 });
                        string str4 = string.Format(CultureInfo.InvariantCulture, "@value{0}_promotion{1}", new object[] { num2, num });
                        SqlParameter parameter = new SqlParameter {
                            SqlDbType = SqlDbType.Variant,
                            ParameterName = str4,
                            Value = value2.Value ?? DBNull.Value
                        };
                        parameters.Add(parameter);
                        builder.Append(", ");
                        builder.Append(str3);
                        builder.Append(str4);
                        flag = true;
                    }
                    num2++;
                }
                num2 = 0x21;
                foreach (XName name2 in pair.Value.Item2)
                {
                    InstanceValue value3;
                    IObjectSerializer objectSerializer = ObjectSerializerFactory.GetObjectSerializer(base.Store.InstanceEncodingOption);
                    if (saveWorkflowCommand.InstanceData.TryGetValue(name2, out value3))
                    {
                        string str5 = string.Format(CultureInfo.InvariantCulture, "@value{0}=", new object[] { num2 });
                        string parameterName = string.Format(CultureInfo.InvariantCulture, "@value{0}_promotion{1}", new object[] { num2, num });
                        AddSerializedProperty(objectSerializer.SerializeValue(value3.Value), parameters, parameterName);
                        builder.Append(", ");
                        builder.Append(str5);
                        builder.Append(parameterName);
                        flag = true;
                    }
                    num2++;
                }
                if (flag)
                {
                    SqlParameter parameter2 = new SqlParameter {
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 400,
                        ParameterName = str,
                        Value = pair.Key
                    };
                    parameters.Add(parameter2);
                    SqlParameter parameter3 = new SqlParameter {
                        SqlDbType = SqlDbType.UniqueIdentifier,
                        ParameterName = str2,
                        Value = base.InstancePersistenceContext.InstanceView.InstanceId
                    };
                    parameters.Add(parameter3);
                    builder.Append(";");
                    commandTextBuilder.AppendLine(builder.ToString());
                    num++;
                }
            }
        }

        private static void UpdateKeyData(InstancePersistenceContext context, SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceView instanceView = context.InstanceView;
            foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair in saveWorkflowCommand.InstanceKeysToAssociate)
            {
                if (!instanceView.InstanceKeys.ContainsKey(pair.Key))
                {
                    context.AssociatedInstanceKey(pair.Key);
                    if (pair.Value != null)
                    {
                        foreach (KeyValuePair<XName, InstanceValue> pair2 in pair.Value)
                        {
                            context.WroteInstanceKeyMetadataValue(pair.Key, pair2.Key, pair2.Value);
                        }
                    }
                }
            }
            foreach (Guid guid in saveWorkflowCommand.InstanceKeysToComplete)
            {
                InstanceKeyView view2;
                if (instanceView.InstanceKeys.TryGetValue(guid, out view2) && (view2.InstanceKeyState != InstanceKeyState.Completed))
                {
                    context.CompletedInstanceKey(guid);
                }
            }
            foreach (Guid guid2 in saveWorkflowCommand.InstanceKeysToFree)
            {
                InstanceKeyView view3;
                if (instanceView.InstanceKeys.TryGetValue(guid2, out view3))
                {
                    context.UnassociatedInstanceKey(guid2);
                }
            }
            foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair3 in saveWorkflowCommand.InstanceKeyMetadataChanges)
            {
                if (pair3.Value != null)
                {
                    foreach (KeyValuePair<XName, InstanceValue> pair4 in pair3.Value)
                    {
                        context.WroteInstanceKeyMetadataValue(pair3.Key, pair4.Key, pair4.Value);
                    }
                }
            }
            if (saveWorkflowCommand.CompleteInstance)
            {
                foreach (KeyValuePair<Guid, InstanceKeyView> pair5 in instanceView.InstanceKeys)
                {
                    if ((pair5.Value != null) && (pair5.Value.InstanceKeyState == InstanceKeyState.Associated))
                    {
                        context.CompletedInstanceKey(pair5.Key);
                    }
                }
            }
        }
    }
}

