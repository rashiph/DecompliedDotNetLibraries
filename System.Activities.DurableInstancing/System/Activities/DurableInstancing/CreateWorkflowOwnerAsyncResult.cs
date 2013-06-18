namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Text;
    using System.Transactions;
    using System.Xml.Linq;

    internal sealed class CreateWorkflowOwnerAsyncResult : WorkflowOwnerAsyncResult
    {
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[CreateLockOwner]", new object[] { "[System.Activities.DurableInstancing]" });
        private bool fireActivatableInstancesEvent;
        private bool fireRunnableInstancesEvent;
        private Guid lockOwnerId;

        public CreateWorkflowOwnerAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        private void ExtractWorkflowHostType()
        {
            InstanceValue value2;
            CreateWorkflowOwnerCommand instancePersistenceCommand = (CreateWorkflowOwnerCommand) base.InstancePersistenceCommand;
            if (instancePersistenceCommand.InstanceOwnerMetadata.TryGetValue(WorkflowNamespace.WorkflowHostType, out value2))
            {
                XName name = value2.Value as XName;
                if (name == null)
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.InvalidWorkflowHostTypeValue(WorkflowNamespace.WorkflowHostType)));
                }
                byte[] bytes = Encoding.Unicode.GetBytes(name.ToString());
                base.Store.WorkflowHostType = new Guid(HashHelper.ComputeHash(bytes));
                this.fireRunnableInstancesEvent = true;
            }
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            InstanceValue value2;
            base.GenerateSqlCommand(sqlCommand);
            if (base.StoreLock.IsValid)
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.MultipleLockOwnersNotSupported));
            }
            CreateWorkflowOwnerCommand instancePersistenceCommand = base.InstancePersistenceCommand as CreateWorkflowOwnerCommand;
            SqlParameterCollection parameters = sqlCommand.Parameters;
            double totalSeconds = base.Store.BufferedHostLockRenewalPeriod.TotalSeconds;
            this.lockOwnerId = Guid.NewGuid();
            this.ExtractWorkflowHostType();
            if (instancePersistenceCommand.InstanceOwnerMetadata.TryGetValue(PersistenceMetadataNamespace.ActivationType, out value2))
            {
                if (!PersistenceMetadataNamespace.ActivationTypes.WAS.Equals(value2.Value))
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(System.Activities.DurableInstancing.SR.NonWASActivationNotSupported));
                }
                this.fireActivatableInstancesEvent = true;
            }
            ArraySegment<byte>[] segmentArray = SerializationUtilities.SerializePropertyBag(instancePersistenceCommand.InstanceOwnerMetadata, base.Store.InstanceEncodingOption);
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@lockTimeout",
                SqlDbType = SqlDbType.Int,
                Value = totalSeconds
            };
            parameters.Add(parameter);
            SqlParameter parameter2 = new SqlParameter {
                ParameterName = "@lockOwnerId",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = this.lockOwnerId
            };
            parameters.Add(parameter2);
            SqlParameter parameter3 = new SqlParameter {
                ParameterName = "@workflowHostType",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = (base.Store.WorkflowHostType != Guid.Empty) ? ((object) base.Store.WorkflowHostType) : ((object) DBNull.Value)
            };
            parameters.Add(parameter3);
            SqlParameter parameter4 = new SqlParameter {
                ParameterName = "@enqueueCommand",
                SqlDbType = SqlDbType.Bit,
                Value = base.Store.EnqueueRunCommands
            };
            parameters.Add(parameter4);
            SqlParameter parameter5 = new SqlParameter {
                ParameterName = "@deleteInstanceOnCompletion",
                SqlDbType = SqlDbType.Bit,
                Value = base.Store.InstanceCompletionAction == InstanceCompletionAction.DeleteAll
            };
            parameters.Add(parameter5);
            SqlParameter parameter6 = new SqlParameter {
                ParameterName = "@primitiveLockOwnerData",
                SqlDbType = SqlDbType.VarBinary,
                Size = segmentArray[0].Count,
                Value = segmentArray[0].Array ?? DBNull.Value
            };
            parameters.Add(parameter6);
            SqlParameter parameter7 = new SqlParameter {
                ParameterName = "@complexLockOwnerData",
                SqlDbType = SqlDbType.VarBinary,
                Size = segmentArray[1].Count,
                Value = segmentArray[1].Array ?? DBNull.Value
            };
            parameters.Add(parameter7);
            SqlParameter parameter8 = new SqlParameter {
                ParameterName = "@writeOnlyPrimitiveLockOwnerData",
                SqlDbType = SqlDbType.VarBinary,
                Size = segmentArray[2].Count,
                Value = segmentArray[2].Array ?? DBNull.Value
            };
            parameters.Add(parameter8);
            SqlParameter parameter9 = new SqlParameter {
                ParameterName = "@writeOnlyComplexLockOwnerData",
                SqlDbType = SqlDbType.VarBinary,
                Size = segmentArray[3].Count,
                Value = segmentArray[3].Array ?? DBNull.Value
            };
            parameters.Add(parameter9);
            SqlParameter parameter10 = new SqlParameter {
                ParameterName = "@encodingOption",
                SqlDbType = SqlDbType.TinyInt,
                Value = base.Store.InstanceEncodingOption
            };
            parameters.Add(parameter10);
            SqlParameter parameter11 = new SqlParameter {
                ParameterName = "@machineName",
                SqlDbType = SqlDbType.NVarChar,
                Value = SqlWorkflowInstanceStoreConstants.MachineName
            };
            parameters.Add(parameter11);
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
                base.InstancePersistenceContext.BindInstanceOwner(this.lockOwnerId, this.lockOwnerId);
                long surrogateLockOwnerId = reader.GetInt64(1);
                if (this.fireActivatableInstancesEvent)
                {
                    base.InstancePersistenceContext.BindEvent(InstancePersistenceEvent<HasActivatableWorkflowEvent>.Value);
                }
                else if (this.fireRunnableInstancesEvent)
                {
                    base.InstancePersistenceContext.BindEvent(InstancePersistenceEvent<HasRunnableWorkflowEvent>.Value);
                }
                base.StoreLock.MarkInstanceOwnerCreated(this.lockOwnerId, surrogateLockOwnerId, base.InstancePersistenceContext.InstanceHandle, this.fireRunnableInstancesEvent, this.fireActivatableInstancesEvent);
            }
            return nextResultSet;
        }
    }
}

