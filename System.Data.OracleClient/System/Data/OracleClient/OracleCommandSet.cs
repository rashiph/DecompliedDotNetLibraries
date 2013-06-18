namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal sealed class OracleCommandSet : IDisposable
    {
        private OracleCommand _batchCommand;
        private ArrayList _commandList;
        private static int _commentGroup;
        private bool _dirty;
        private static int _identifierGroup;
        private static int _otherGroup;
        private static int _parameterMarkerGroup;
        private static int _queryGroup;
        private static Regex _sqlTokenParser;
        private static readonly string _sqlTokenPattern = "[\\s]+|(?<string>'([^']|'')*')|(?<comment>(/\\*([^\\*]|\\*[^/])*\\*/)|(--.*))|(?<parametermarker>:[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_#$]+)|(?<query>select)|(?<identifier>([\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_#$]+)|(\"([^\"]|\"\")*\"))|(?<other>.)";
        private static int _stringGroup;
        private Hashtable _usedParameterNames;
        private static readonly string Body_Prefix = "begin\n";
        private static readonly string Body_Suffix = "end;";
        private static readonly string Command_NonQuerySuffix_Part2 = " := sql%rowcount;\n";
        private static readonly string Command_QueryPrefix_Part1 = "open ";
        private static readonly string Command_QueryPrefix_Part2 = " for ";
        private static readonly string Command_QuerySuffix_Part2 = " := ";
        private static readonly string Command_QuerySuffix_Part3 = ";\n";
        private static readonly string Command_Suffix_Part1 = ";\n:";
        private static readonly string Declarations_CursorType = " refcursortype;\n";
        private static readonly string Declarations_Prefix = "declare\ntype refcursortype is ref cursor;\n";

        public OracleCommandSet() : this(null, null)
        {
        }

        public OracleCommandSet(OracleConnection connection, OracleTransaction transaction)
        {
            this._usedParameterNames = new Hashtable(StringComparer.OrdinalIgnoreCase);
            this._commandList = new ArrayList();
            this._batchCommand = new OracleCommand();
            this.Connection = connection;
            this.Transaction = transaction;
        }

        public void Append(OracleCommand command)
        {
            System.Data.Common.ADP.CheckArgumentNull(command, "command");
            if (System.Data.Common.ADP.IsEmpty(command.CommandText))
            {
                throw System.Data.Common.ADP.CommandTextRequired("Append");
            }
            ICollection parameters = command.Parameters;
            OracleParameter[] array = new OracleParameter[parameters.Count];
            parameters.CopyTo(array, 0);
            string[] parameterNames = new string[array.Length];
            if (0 < array.Length)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    parameterNames[i] = array[i].ParameterName;
                    OracleParameter destination = command.CreateParameter();
                    array[i].CopyTo(destination);
                    object obj2 = destination.Value;
                    if (obj2 is byte[])
                    {
                        byte[] src = (byte[]) obj2;
                        int offset = destination.Offset;
                        int size = destination.Size;
                        int num4 = src.Length - offset;
                        if ((size != 0) && (size < num4))
                        {
                            num4 = size;
                        }
                        byte[] dst = new byte[Math.Max(num4, 0)];
                        Buffer.BlockCopy(src, offset, dst, 0, dst.Length);
                        destination.Offset = 0;
                        destination.Value = dst;
                    }
                    else if (obj2 is char[])
                    {
                        char[] chArray2 = (char[]) obj2;
                        int srcOffset = destination.Offset;
                        int num3 = destination.Size;
                        int num2 = chArray2.Length - srcOffset;
                        if ((num3 != 0) && (num3 < num2))
                        {
                            num2 = num3;
                        }
                        char[] chArray = new char[Math.Max(num2, 0)];
                        Buffer.BlockCopy(chArray2, srcOffset, chArray, 0, chArray.Length * 2);
                        destination.Offset = 0;
                        destination.Value = chArray;
                    }
                    else if (obj2 is ICloneable)
                    {
                        destination.Value = ((ICloneable) obj2).Clone();
                    }
                    array[i] = destination;
                }
            }
            string statementText = command.StatementText;
            bool isQuery = false;
            LocalParameter[] parameterInsertionPoints = this.ParseText(command, statementText, out isQuery);
            LocalCommand command2 = new LocalCommand(statementText, isQuery, array, parameterNames, parameterInsertionPoints);
            this._dirty = true;
            this.CommandList.Add(command2);
        }

        public void Clear()
        {
            DbCommand batchCommand = this.BatchCommand;
            if (batchCommand != null)
            {
                batchCommand.Parameters.Clear();
                batchCommand.CommandText = null;
            }
            ArrayList list = this._commandList;
            if (list != null)
            {
                list.Clear();
            }
            Hashtable hashtable = this._usedParameterNames;
            if (hashtable != null)
            {
                hashtable.Clear();
            }
        }

        public void Dispose()
        {
            DbCommand command = this._batchCommand;
            this._batchCommand = null;
            this._commandList = null;
            this._usedParameterNames = null;
            if (command != null)
            {
                command.Dispose();
            }
        }

        public int ExecuteNonQuery()
        {
            this.GenerateBatchCommandText();
            return this.BatchCommand.ExecuteNonQuery();
        }

        private void GenerateBatchCommandText()
        {
            if (this._dirty)
            {
                DbCommand batchCommand = this.BatchCommand;
                StringBuilder builder2 = new StringBuilder();
                StringBuilder builder = new StringBuilder();
                int num6 = 1;
                int num5 = 1;
                int num4 = 1;
                batchCommand.Parameters.Clear();
                builder2.Append(Declarations_Prefix);
                builder.Append(Body_Prefix);
                foreach (LocalCommand command in this.CommandList)
                {
                    string str;
                    foreach (DbParameter parameter2 in command.Parameters)
                    {
                        string str4;
                        do
                        {
                            str4 = "p" + num4.ToString(CultureInfo.InvariantCulture);
                            num4++;
                        }
                        while (this._usedParameterNames.ContainsKey(str4));
                        parameter2.ParameterName = str4;
                        batchCommand.Parameters.Add(parameter2);
                    }
                    do
                    {
                        str = "r" + num6.ToString(CultureInfo.InvariantCulture) + "_" + num4.ToString(CultureInfo.InvariantCulture);
                        num4++;
                    }
                    while (this._usedParameterNames.ContainsKey(str));
                    OracleParameter parameter = new OracleParameter {
                        CommandSetResult = num6++,
                        Direction = ParameterDirection.Output,
                        ParameterName = str
                    };
                    batchCommand.Parameters.Add(parameter);
                    int length = builder.Length;
                    if (command.IsQuery)
                    {
                        string str2 = "c" + num5.ToString(CultureInfo.InvariantCulture);
                        num5++;
                        builder2.Append(str2);
                        builder2.Append(Declarations_CursorType);
                        builder.Append(Command_QueryPrefix_Part1);
                        builder.Append(str2);
                        builder.Append(Command_QueryPrefix_Part2);
                        length = builder.Length;
                        builder.Append(command.CommandText);
                        builder.Append(Command_Suffix_Part1);
                        builder.Append(str);
                        builder.Append(Command_QuerySuffix_Part2);
                        builder.Append(str2);
                        builder.Append(Command_QuerySuffix_Part3);
                        parameter.OracleType = OracleType.Cursor;
                    }
                    else
                    {
                        string commandText = command.CommandText;
                        builder.Append(commandText.TrimEnd(new char[] { ';' }));
                        builder.Append(Command_Suffix_Part1);
                        builder.Append(str);
                        builder.Append(Command_NonQuerySuffix_Part2);
                        parameter.OracleType = OracleType.Int32;
                        command.ResultParameter = parameter;
                    }
                    foreach (LocalParameter parameter4 in command.ParameterInsertionPoints)
                    {
                        DbParameter parameter3 = command.Parameters[parameter4.ParameterIndex];
                        string str3 = ":" + parameter3.ParameterName;
                        builder.Remove(length + parameter4.InsertionPoint, parameter4.RemovalLength);
                        builder.Insert(length + parameter4.InsertionPoint, str3);
                        length += str3.Length - parameter4.RemovalLength;
                    }
                }
                builder.Append(Body_Suffix);
                builder2.Append(builder);
                batchCommand.CommandText = builder2.ToString();
                this._dirty = false;
            }
        }

        internal bool GetBatchedRecordsAffected(int commandIndex, out int recordsAffected)
        {
            OracleParameter resultParameter = ((LocalCommand) this.CommandList[commandIndex]).ResultParameter;
            if (resultParameter != null)
            {
                if (resultParameter.Value is int)
                {
                    recordsAffected = (int) resultParameter.Value;
                    return true;
                }
                recordsAffected = -1;
                return false;
            }
            recordsAffected = -1;
            return true;
        }

        internal DbParameter GetParameter(int commandIndex, int parameterIndex)
        {
            return ((LocalCommand) this.CommandList[commandIndex]).Parameters[parameterIndex];
        }

        public int GetParameterCount(int commandIndex)
        {
            return ((LocalCommand) this.CommandList[commandIndex]).Parameters.Length;
        }

        private static Regex GetSqlTokenParser()
        {
            Regex regex = _sqlTokenParser;
            if (regex == null)
            {
                regex = new Regex(_sqlTokenPattern, RegexOptions.ExplicitCapture);
                _commentGroup = regex.GroupNumberFromName("comment");
                _identifierGroup = regex.GroupNumberFromName("identifier");
                _parameterMarkerGroup = regex.GroupNumberFromName("parametermarker");
                _queryGroup = regex.GroupNumberFromName("query");
                _stringGroup = regex.GroupNumberFromName("string");
                _otherGroup = regex.GroupNumberFromName("other");
                _sqlTokenParser = regex;
            }
            return regex;
        }

        private LocalParameter[] ParseText(OracleCommand command, string commandText, out bool isQuery)
        {
            OracleParameterCollection parameters = command.Parameters;
            ArrayList list = new ArrayList();
            Regex sqlTokenParser = GetSqlTokenParser();
            isQuery = false;
            bool flag = false;
            for (Match match = sqlTokenParser.Match(commandText); Match.Empty != match; match = match.NextMatch())
            {
                if (!match.Groups[_commentGroup].Success)
                {
                    if ((match.Groups[_identifierGroup].Success || match.Groups[_stringGroup].Success) || match.Groups[_otherGroup].Success)
                    {
                        flag = true;
                    }
                    else if (match.Groups[_queryGroup].Success)
                    {
                        if (!flag)
                        {
                            isQuery = true;
                        }
                    }
                    else if (match.Groups[_parameterMarkerGroup].Success)
                    {
                        string parameterName = match.Groups[_parameterMarkerGroup].Value.Substring(1);
                        this._usedParameterNames[parameterName] = null;
                        int index = parameters.IndexOf(parameterName);
                        if (0 > index)
                        {
                            string str2 = ":" + parameterName;
                            index = parameters.IndexOf(str2);
                        }
                        if (0 <= index)
                        {
                            list.Add(new LocalParameter(index, match.Index, match.Length));
                        }
                    }
                }
            }
            LocalParameter[] array = new LocalParameter[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private OracleCommand BatchCommand
        {
            get
            {
                OracleCommand command = this._batchCommand;
                if (command == null)
                {
                    throw System.Data.Common.ADP.ObjectDisposed(base.GetType().Name);
                }
                return command;
            }
        }

        public int CommandCount
        {
            get
            {
                return this.CommandList.Count;
            }
        }

        private ArrayList CommandList
        {
            get
            {
                ArrayList list = this._commandList;
                if (list == null)
                {
                    throw System.Data.Common.ADP.ObjectDisposed(base.GetType().Name);
                }
                return list;
            }
        }

        public int CommandTimeout
        {
            set
            {
                this.BatchCommand.CommandTimeout = value;
            }
        }

        public OracleConnection Connection
        {
            set
            {
                this.BatchCommand.Connection = value;
            }
        }

        internal OracleTransaction Transaction
        {
            set
            {
                this.BatchCommand.Transaction = value;
            }
        }

        private sealed class LocalCommand
        {
            internal readonly string CommandText;
            internal readonly bool IsQuery;
            internal readonly OracleCommandSet.LocalParameter[] ParameterInsertionPoints;
            internal readonly string[] ParameterNames;
            internal readonly DbParameter[] Parameters;
            internal OracleParameter ResultParameter;

            internal LocalCommand(string commandText, bool isQuery, DbParameter[] parameters, string[] parameterNames, OracleCommandSet.LocalParameter[] parameterInsertionPoints)
            {
                this.CommandText = commandText;
                this.IsQuery = isQuery;
                this.Parameters = parameters;
                this.ParameterNames = parameterNames;
                this.ParameterInsertionPoints = parameterInsertionPoints;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LocalParameter
        {
            internal readonly int ParameterIndex;
            internal readonly int InsertionPoint;
            internal readonly int RemovalLength;
            internal LocalParameter(int parameterIndex, int insertionPoint, int removalLength)
            {
                this.ParameterIndex = parameterIndex;
                this.InsertionPoint = insertionPoint;
                this.RemovalLength = removalLength;
            }
        }
    }
}

