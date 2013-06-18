namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Workflow.Runtime;
    using System.Xml;

    public sealed class SqlTrackingQuery
    {
        private string _connectionString;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SqlTrackingQuery()
        {
        }

        public SqlTrackingQuery(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            this._connectionString = connectionString;
        }

        private void BuildArtifactParameters(SqlCommand cmd, IList<TrackingDataItemValue> artifacts)
        {
            if ((artifacts != null) && (artifacts.Count != 0))
            {
                StringBuilder output = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(output);
                try
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("TrackingDataItems");
                    foreach (TrackingDataItemValue value2 in artifacts)
                    {
                        writer.WriteStartElement("TrackingDataItem");
                        writer.WriteElementString("QualifiedName", value2.QualifiedName);
                        writer.WriteElementString("FieldName", value2.FieldName);
                        if (value2.DataValue != null)
                        {
                            writer.WriteElementString("DataValue", value2.DataValue);
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                finally
                {
                    writer.Flush();
                    writer.Close();
                }
                SqlParameter parameter = new SqlParameter {
                    ParameterName = "@TrackingDataItems",
                    SqlDbType = SqlDbType.NText,
                    Value = output.ToString()
                };
                cmd.Parameters.Add(parameter);
            }
        }

        private SqlCommand BuildCommand(Guid workflowInstanceId)
        {
            SqlCommand command = new SqlCommand("[dbo].[GetWorkflows]") {
                CommandType = CommandType.StoredProcedure
            };
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@WorkflowInstanceId",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = workflowInstanceId
            };
            command.Parameters.Add(parameter);
            return command;
        }

        private SqlCommand BuildCommand(SqlTrackingQueryOptions opt)
        {
            SqlCommand cmd = new SqlCommand("[dbo].[GetWorkflows]") {
                CommandType = CommandType.StoredProcedure
            };
            SqlParameter parameter = new SqlParameter();
            if (opt.WorkflowStatus.HasValue)
            {
                parameter.ParameterName = "@WorkflowStatusId";
                parameter.SqlDbType = SqlDbType.TinyInt;
                parameter.Value = opt.WorkflowStatus.Value;
                cmd.Parameters.Add(parameter);
                if ((DateTime.MinValue != opt.StatusMinDateTime) || (DateTime.MaxValue != opt.StatusMaxDateTime))
                {
                    parameter = new SqlParameter {
                        ParameterName = "@StatusMinDateTime",
                        SqlDbType = SqlDbType.DateTime
                    };
                    if (opt.StatusMinDateTime < SqlDateTime.MinValue.Value)
                    {
                        parameter.Value = SqlDateTime.MinValue.Value;
                    }
                    else
                    {
                        parameter.Value = opt.StatusMinDateTime;
                    }
                    cmd.Parameters.Add(parameter);
                    parameter = new SqlParameter {
                        ParameterName = "@StatusMaxDateTime",
                        SqlDbType = SqlDbType.DateTime
                    };
                    if (opt.StatusMaxDateTime > SqlDateTime.MaxValue.Value)
                    {
                        parameter.Value = SqlDateTime.MaxValue.Value;
                    }
                    else
                    {
                        parameter.Value = opt.StatusMaxDateTime;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
            if (null != opt.WorkflowType)
            {
                parameter = new SqlParameter("@TypeFullName", opt.WorkflowType.FullName) {
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 0x80
                };
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter("@AssemblyFullName", opt.WorkflowType.Assembly.FullName) {
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 0x80
                };
                cmd.Parameters.Add(parameter);
            }
            if ((opt.TrackingDataItems != null) && (opt.TrackingDataItems.Count > 0))
            {
                this.BuildArtifactParameters(cmd, opt.TrackingDataItems);
            }
            return cmd;
        }

        private SqlTrackingWorkflowInstance BuildInstance(SqlDataReader reader)
        {
            return BuildInstance(reader, this._connectionString);
        }

        internal static SqlTrackingWorkflowInstance BuildInstance(SqlDataReader reader, string connectionString)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.IsClosed)
            {
                throw new ArgumentException(ExecutionStringManager.InvalidSqlDataReader, "reader");
            }
            SqlTrackingWorkflowInstance instance = new SqlTrackingWorkflowInstance(connectionString) {
                WorkflowInstanceId = reader.GetGuid(1),
                WorkflowInstanceInternalId = reader.GetInt64(2),
                Initialized = reader.GetDateTime(3)
            };
            if (DBNull.Value == reader[4])
            {
                instance.InvokingWorkflowInstanceId = Guid.Empty;
            }
            else
            {
                instance.InvokingWorkflowInstanceId = reader.GetGuid(4);
            }
            instance.Status = (WorkflowStatus) reader.GetInt32(5);
            if (!reader.IsDBNull(6))
            {
                instance.WorkflowType = Type.GetType(reader.GetString(6) + ", " + reader.GetString(7), true, false);
            }
            return instance;
        }

        private SqlConnection GetConnection()
        {
            if (this._connectionString == null)
            {
                throw new InvalidOperationException(ExecutionStringManager.MissingConnectionString);
            }
            SqlConnection connection = new SqlConnection(this._connectionString);
            connection.Open();
            return connection;
        }

        public IList<SqlTrackingWorkflowInstance> GetWorkflows(SqlTrackingQueryOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (options.TrackingDataItems != null)
            {
                foreach (TrackingDataItemValue value2 in options.TrackingDataItems)
                {
                    if (value2.QualifiedName == null)
                    {
                        throw new ArgumentNullException("options.TrackingDataItems.QualifiedName");
                    }
                    if (value2.FieldName == null)
                    {
                        throw new ArgumentNullException("options.TrackingDataItems.FieldName");
                    }
                }
            }
            SqlCommand command = this.BuildCommand(options);
            SqlDataReader reader = null;
            List<SqlTrackingWorkflowInstance> list = new List<SqlTrackingWorkflowInstance>();
            try
            {
                command.Connection = this.GetConnection();
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    list.Add(this.BuildInstance(reader));
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                {
                    command.Connection.Close();
                }
            }
            return list;
        }

        public bool TryGetWorkflow(Guid workflowInstanceId, out SqlTrackingWorkflowInstance workflowInstance)
        {
            bool flag;
            SqlCommand command = this.BuildCommand(workflowInstanceId);
            SqlDataReader reader = null;
            workflowInstance = null;
            try
            {
                command.Connection = this.GetConnection();
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                if (reader.Read())
                {
                    workflowInstance = this.BuildInstance(reader);
                    return true;
                }
                flag = false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                {
                    command.Connection.Close();
                }
            }
            return flag;
        }

        public string ConnectionString
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._connectionString;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._connectionString = value;
            }
        }
    }
}

