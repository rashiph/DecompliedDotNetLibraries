namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;

    [Serializable, TypeConverter("System.Diagnostics.Design.CounterCreationDataConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class CounterCreationData
    {
        private string counterHelp;
        private string counterName;
        private PerformanceCounterType counterType;

        public CounterCreationData()
        {
            this.counterType = PerformanceCounterType.NumberOfItems32;
            this.counterName = string.Empty;
            this.counterHelp = string.Empty;
        }

        public CounterCreationData(string counterName, string counterHelp, PerformanceCounterType counterType)
        {
            this.counterType = PerformanceCounterType.NumberOfItems32;
            this.counterName = string.Empty;
            this.counterHelp = string.Empty;
            this.CounterType = counterType;
            this.CounterName = counterName;
            this.CounterHelp = counterHelp;
        }

        [DefaultValue(""), MonitoringDescription("CounterHelp")]
        public string CounterHelp
        {
            get
            {
                return this.counterHelp;
            }
            set
            {
                PerformanceCounterCategory.CheckValidHelp(value);
                this.counterHelp = value;
            }
        }

        [MonitoringDescription("CounterName"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue("")]
        public string CounterName
        {
            get
            {
                return this.counterName;
            }
            set
            {
                PerformanceCounterCategory.CheckValidCounter(value);
                this.counterName = value;
            }
        }

        [MonitoringDescription("CounterType"), DefaultValue(0x10000)]
        public PerformanceCounterType CounterType
        {
            get
            {
                return this.counterType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(PerformanceCounterType), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(PerformanceCounterType));
                }
                this.counterType = value;
            }
        }
    }
}

