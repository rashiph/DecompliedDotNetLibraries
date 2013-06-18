namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    internal sealed class ExecutionTracker
    {
        [DataMember]
        private List<CompensationTokenData> executionOrderedList = new List<CompensationTokenData>();

        public void Add(CompensationTokenData compensationToken)
        {
            this.executionOrderedList.Insert(0, compensationToken);
        }

        public CompensationTokenData Get()
        {
            if (this.Count > 0)
            {
                return this.executionOrderedList[0];
            }
            return null;
        }

        public void Remove(CompensationTokenData compensationToken)
        {
            this.executionOrderedList.Remove(compensationToken);
        }

        public int Count
        {
            get
            {
                return this.executionOrderedList.Count;
            }
        }
    }
}

