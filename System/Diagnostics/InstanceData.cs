namespace System.Diagnostics
{
    using System;

    public class InstanceData
    {
        private string instanceName;
        private CounterSample sample;

        public InstanceData(string instanceName, CounterSample sample)
        {
            this.instanceName = instanceName;
            this.sample = sample;
        }

        public string InstanceName
        {
            get
            {
                return this.instanceName;
            }
        }

        public long RawValue
        {
            get
            {
                return this.sample.RawValue;
            }
        }

        public CounterSample Sample
        {
            get
            {
                return this.sample;
            }
        }
    }
}

