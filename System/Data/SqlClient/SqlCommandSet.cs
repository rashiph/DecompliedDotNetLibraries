namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal sealed class SqlCommandSet
    {
        private SqlCommand _batchCommand = new SqlCommand();
        private List<LocalCommand> _commandList = new List<LocalCommand>();
        internal readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;
        private static readonly Regex SqlIdentifierParser = new Regex(@"^@[\p{Lo}\p{Lu}\p{Ll}\p{Lm}_@#][\p{Lo}\p{Lu}\p{Ll}\p{Lm}\p{Nd}＿_@#\$]*$", RegexOptions.Singleline | RegexOptions.ExplicitCapture);
        private const string SqlIdentifierPattern = @"^@[\p{Lo}\p{Lu}\p{Ll}\p{Lm}_@#][\p{Lo}\p{Lu}\p{Ll}\p{Lm}\p{Nd}＿_@#\$]*$";

        internal SqlCommandSet()
        {
        }

        internal void Append(SqlCommand command)
        {
            SqlParameterCollection parameters;
            int num7;
            ADP.CheckArgumentNull(command, "command");
            Bid.Trace("<sc.SqlCommandSet.Append|API> %d#, command=%d, parameterCount=%d\n", this.ObjectID, command.ObjectID, command.Parameters.Count);
            string commandText = command.CommandText;
            if (ADP.IsEmpty(commandText))
            {
                throw ADP.CommandTextRequired("Append");
            }
            CommandType commandType = command.CommandType;
            switch (commandType)
            {
                case CommandType.Text:
                case CommandType.StoredProcedure:
                {
                    parameters = null;
                    SqlParameterCollection parameters2 = command.Parameters;
                    if (0 < parameters2.Count)
                    {
                        parameters = new SqlParameterCollection();
                        for (int i = 0; i < parameters2.Count; i++)
                        {
                            SqlParameter destination = new SqlParameter();
                            parameters2[i].CopyTo(destination);
                            parameters.Add(destination);
                            if (!SqlIdentifierParser.IsMatch(destination.ParameterName))
                            {
                                throw ADP.BadParameterName(destination.ParameterName);
                            }
                        }
                        foreach (SqlParameter parameter in parameters)
                        {
                            object obj2 = parameter.Value;
                            byte[] src = obj2 as byte[];
                            if (src != null)
                            {
                                int offset = parameter.Offset;
                                int size = parameter.Size;
                                int num5 = src.Length - offset;
                                if ((size != 0) && (size < num5))
                                {
                                    num5 = size;
                                }
                                byte[] dst = new byte[Math.Max(num5, 0)];
                                Buffer.BlockCopy(src, offset, dst, 0, dst.Length);
                                parameter.Offset = 0;
                                parameter.Value = dst;
                            }
                            else
                            {
                                char[] chArray2 = obj2 as char[];
                                if (chArray2 != null)
                                {
                                    int srcOffset = parameter.Offset;
                                    int num4 = parameter.Size;
                                    int num3 = chArray2.Length - srcOffset;
                                    if ((num4 != 0) && (num4 < num3))
                                    {
                                        num3 = num4;
                                    }
                                    char[] chArray = new char[Math.Max(num3, 0)];
                                    Buffer.BlockCopy(chArray2, srcOffset, chArray, 0, chArray.Length * 2);
                                    parameter.Offset = 0;
                                    parameter.Value = chArray;
                                }
                                else
                                {
                                    ICloneable cloneable = obj2 as ICloneable;
                                    if (cloneable != null)
                                    {
                                        parameter.Value = cloneable.Clone();
                                    }
                                }
                            }
                        }
                    }
                    num7 = -1;
                    if (parameters != null)
                    {
                        for (int j = 0; j < parameters.Count; j++)
                        {
                            if (ParameterDirection.ReturnValue == parameters[j].Direction)
                            {
                                num7 = j;
                                break;
                            }
                        }
                    }
                    break;
                }
                case CommandType.TableDirect:
                    throw SQL.NotSupportedCommandType(commandType);

                default:
                    throw ADP.InvalidCommandType(commandType);
            }
            LocalCommand item = new LocalCommand(commandText, parameters, num7, command.CommandType);
            this.CommandList.Add(item);
        }

        internal static void BuildStoredProcedureName(StringBuilder builder, string part)
        {
            if ((part != null) && (0 < part.Length))
            {
                if ('[' == part[0])
                {
                    int num2 = 0;
                    foreach (char ch in part)
                    {
                        if (']' == ch)
                        {
                            num2++;
                        }
                    }
                    if (1 == (num2 % 2))
                    {
                        builder.Append(part);
                        return;
                    }
                }
                SqlServerEscapeHelper.EscapeIdentifier(builder, part);
            }
        }

        internal void Clear()
        {
            Bid.Trace("<sc.SqlCommandSet.Clear|API> %d#", this.ObjectID);
            DbCommand batchCommand = this.BatchCommand;
            if (batchCommand != null)
            {
                batchCommand.Parameters.Clear();
                batchCommand.CommandText = null;
            }
            List<LocalCommand> list = this._commandList;
            if (list != null)
            {
                list.Clear();
            }
        }

        internal void Dispose()
        {
            Bid.Trace("<sc.SqlCommandSet.Dispose|API> %d#", this.ObjectID);
            SqlCommand command = this._batchCommand;
            this._commandList = null;
            this._batchCommand = null;
            if (command != null)
            {
                command.Dispose();
            }
        }

        internal int ExecuteNonQuery()
        {
            int num2;
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<sc.SqlCommandSet.ExecuteNonQuery|API> %d#", this.ObjectID);
            try
            {
                if (this.Connection.IsContextConnection)
                {
                    throw SQL.BatchedUpdatesNotAvailableOnContextConnection();
                }
                this.ValidateCommandBehavior("ExecuteNonQuery", CommandBehavior.Default);
                this.BatchCommand.BatchRPCMode = true;
                this.BatchCommand.ClearBatchCommand();
                this.BatchCommand.Parameters.Clear();
                for (int i = 0; i < this._commandList.Count; i++)
                {
                    LocalCommand command = this._commandList[i];
                    this.BatchCommand.AddBatchCommand(command.CommandText, command.Parameters, command.CmdType);
                }
                num2 = this.BatchCommand.ExecuteBatchRPCCommand();
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num2;
        }

        internal bool GetBatchedAffected(int commandIdentifier, out int recordsAffected, out Exception error)
        {
            error = this.BatchCommand.GetErrors(commandIdentifier);
            int? nullable = this.BatchCommand.GetRecordsAffected(commandIdentifier);
            recordsAffected = nullable.GetValueOrDefault();
            return nullable.HasValue;
        }

        internal SqlParameter GetParameter(int commandIndex, int parameterIndex)
        {
            return this.CommandList[commandIndex].Parameters[parameterIndex];
        }

        internal int GetParameterCount(int commandIndex)
        {
            return this.CommandList[commandIndex].Parameters.Count;
        }

        private void ValidateCommandBehavior(string method, CommandBehavior behavior)
        {
            if ((behavior & ~(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess)) != CommandBehavior.Default)
            {
                ADP.ValidateCommandBehavior(behavior);
                throw ADP.NotSupportedCommandBehavior(behavior & ~(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess), method);
            }
        }

        private SqlCommand BatchCommand
        {
            get
            {
                SqlCommand command = this._batchCommand;
                if (command == null)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return command;
            }
        }

        internal int CommandCount
        {
            get
            {
                return this.CommandList.Count;
            }
        }

        private List<LocalCommand> CommandList
        {
            get
            {
                List<LocalCommand> list = this._commandList;
                if (list == null)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return list;
            }
        }

        internal int CommandTimeout
        {
            set
            {
                this.BatchCommand.CommandTimeout = value;
            }
        }

        internal SqlConnection Connection
        {
            get
            {
                return this.BatchCommand.Connection;
            }
            set
            {
                this.BatchCommand.Connection = value;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal SqlTransaction Transaction
        {
            set
            {
                this.BatchCommand.Transaction = value;
            }
        }

        private sealed class LocalCommand
        {
            internal readonly CommandType CmdType;
            internal readonly string CommandText;
            internal readonly SqlParameterCollection Parameters;
            internal readonly int ReturnParameterIndex;

            internal LocalCommand(string commandText, SqlParameterCollection parameters, int returnParameterIndex, CommandType cmdType)
            {
                this.CommandText = commandText;
                this.Parameters = parameters;
                this.ReturnParameterIndex = returnParameterIndex;
                this.CmdType = cmdType;
            }
        }
    }
}

