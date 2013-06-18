namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Data.Common;

    internal sealed class SqlStatistics
    {
        internal long _buffersReceived;
        internal long _buffersSent;
        internal long _bytesReceived;
        internal long _bytesSent;
        internal long _closeTimestamp;
        internal long _connectionTime;
        internal long _cursorOpens;
        internal long _executionTime;
        internal long _iduCount;
        internal long _iduRows;
        internal long _networkServerTime;
        internal long _openTimestamp;
        internal long _preparedExecs;
        internal long _prepares;
        internal long _selectCount;
        internal long _selectRows;
        internal long _serverRoundtrips;
        internal long _startExecutionTimestamp;
        internal long _startFetchTimestamp;
        internal long _startNetworkServerTimestamp;
        internal long _sumResultSets;
        internal long _transactions;
        internal long _unpreparedExecs;
        private bool _waitForDoneAfterRow;
        private bool _waitForReply;

        internal SqlStatistics()
        {
        }

        internal void ContinueOnNewConnection()
        {
            this._startExecutionTimestamp = 0L;
            this._startFetchTimestamp = 0L;
            this._waitForDoneAfterRow = false;
            this._waitForReply = false;
        }

        internal IDictionary GetHashtable()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("BuffersReceived", this._buffersReceived);
            hashtable.Add("BuffersSent", this._buffersSent);
            hashtable.Add("BytesReceived", this._bytesReceived);
            hashtable.Add("BytesSent", this._bytesSent);
            hashtable.Add("CursorOpens", this._cursorOpens);
            hashtable.Add("IduCount", this._iduCount);
            hashtable.Add("IduRows", this._iduRows);
            hashtable.Add("PreparedExecs", this._preparedExecs);
            hashtable.Add("Prepares", this._prepares);
            hashtable.Add("SelectCount", this._selectCount);
            hashtable.Add("SelectRows", this._selectRows);
            hashtable.Add("ServerRoundtrips", this._serverRoundtrips);
            hashtable.Add("SumResultSets", this._sumResultSets);
            hashtable.Add("Transactions", this._transactions);
            hashtable.Add("UnpreparedExecs", this._unpreparedExecs);
            hashtable.Add("ConnectionTime", ADP.TimerToMilliseconds(this._connectionTime));
            hashtable.Add("ExecutionTime", ADP.TimerToMilliseconds(this._executionTime));
            hashtable.Add("NetworkServerTime", ADP.TimerToMilliseconds(this._networkServerTime));
            return hashtable;
        }

        internal void ReleaseAndUpdateExecutionTimer()
        {
            if (this._startExecutionTimestamp > 0L)
            {
                long num;
                ADP.TimerCurrent(out num);
                this._executionTime += num - this._startExecutionTimestamp;
                this._startExecutionTimestamp = 0L;
            }
        }

        internal void ReleaseAndUpdateNetworkServerTimer()
        {
            if (this._waitForReply && (this._startNetworkServerTimestamp > 0L))
            {
                long num;
                ADP.TimerCurrent(out num);
                this._networkServerTime += num - this._startNetworkServerTimestamp;
                this._startNetworkServerTimestamp = 0L;
            }
            this._waitForReply = false;
        }

        internal bool RequestExecutionTimer()
        {
            if (this._startExecutionTimestamp == 0L)
            {
                ADP.TimerCurrent(out this._startExecutionTimestamp);
                return true;
            }
            return false;
        }

        internal void RequestNetworkServerTimer()
        {
            if (this._startNetworkServerTimestamp == 0L)
            {
                ADP.TimerCurrent(out this._startNetworkServerTimestamp);
            }
            this._waitForReply = true;
        }

        internal void Reset()
        {
            this._buffersReceived = 0L;
            this._buffersSent = 0L;
            this._bytesReceived = 0L;
            this._bytesSent = 0L;
            this._connectionTime = 0L;
            this._cursorOpens = 0L;
            this._executionTime = 0L;
            this._iduCount = 0L;
            this._iduRows = 0L;
            this._networkServerTime = 0L;
            this._preparedExecs = 0L;
            this._prepares = 0L;
            this._selectCount = 0L;
            this._selectRows = 0L;
            this._serverRoundtrips = 0L;
            this._sumResultSets = 0L;
            this._transactions = 0L;
            this._unpreparedExecs = 0L;
            this._waitForDoneAfterRow = false;
            this._waitForReply = false;
            this._startExecutionTimestamp = 0L;
            this._startNetworkServerTimestamp = 0L;
        }

        internal void SafeAdd(ref long value, long summand)
        {
            if ((0x7fffffffffffffffL - value) > summand)
            {
                value += summand;
            }
            else
            {
                value = 0x7fffffffffffffffL;
            }
        }

        internal long SafeIncrement(ref long value)
        {
            if (value < 0x7fffffffffffffffL)
            {
                value += 1L;
            }
            return value;
        }

        internal static SqlStatistics StartTimer(SqlStatistics statistics)
        {
            if ((statistics != null) && !statistics.RequestExecutionTimer())
            {
                statistics = null;
            }
            return statistics;
        }

        internal static void StopTimer(SqlStatistics statistics)
        {
            if (statistics != null)
            {
                statistics.ReleaseAndUpdateExecutionTimer();
            }
        }

        internal void UpdateStatistics()
        {
            if (this._closeTimestamp >= this._openTimestamp)
            {
                this.SafeAdd(ref this._connectionTime, this._closeTimestamp - this._openTimestamp);
            }
            else
            {
                this._connectionTime = 0x7fffffffffffffffL;
            }
        }

        internal bool WaitForDoneAfterRow
        {
            get
            {
                return this._waitForDoneAfterRow;
            }
            set
            {
                this._waitForDoneAfterRow = value;
            }
        }

        internal bool WaitForReply
        {
            get
            {
                return this._waitForReply;
            }
        }
    }
}

