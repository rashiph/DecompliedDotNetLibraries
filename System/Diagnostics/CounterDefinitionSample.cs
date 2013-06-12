namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;

    internal class CounterDefinitionSample
    {
        internal CounterDefinitionSample BaseCounterDefinitionSample;
        private CategorySample categorySample;
        internal readonly int CounterType;
        private long[] instanceValues;
        internal readonly int NameIndex;
        private readonly int offset;
        private readonly int size;

        internal CounterDefinitionSample(Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION perfCounter, CategorySample categorySample, int instanceNumber)
        {
            this.NameIndex = perfCounter.CounterNameTitleIndex;
            this.CounterType = perfCounter.CounterType;
            this.offset = perfCounter.CounterOffset;
            this.size = perfCounter.CounterSize;
            if (instanceNumber == -1)
            {
                this.instanceValues = new long[1];
            }
            else
            {
                this.instanceValues = new long[instanceNumber];
            }
            this.categorySample = categorySample;
        }

        internal CounterSample GetInstanceValue(string instanceName)
        {
            if (!this.categorySample.InstanceNameTable.ContainsKey(instanceName))
            {
                if (instanceName.Length > 0x7f)
                {
                    instanceName = instanceName.Substring(0, 0x7f);
                }
                if (!this.categorySample.InstanceNameTable.ContainsKey(instanceName))
                {
                    throw new InvalidOperationException(SR.GetString("CantReadInstance", new object[] { instanceName }));
                }
            }
            int index = (int) this.categorySample.InstanceNameTable[instanceName];
            long rawValue = this.instanceValues[index];
            long baseValue = 0L;
            if (this.BaseCounterDefinitionSample != null)
            {
                int num4 = (int) this.BaseCounterDefinitionSample.categorySample.InstanceNameTable[instanceName];
                baseValue = this.BaseCounterDefinitionSample.instanceValues[num4];
            }
            return new CounterSample(rawValue, baseValue, this.categorySample.CounterFrequency, this.categorySample.SystemFrequency, this.categorySample.TimeStamp, this.categorySample.TimeStamp100nSec, (PerformanceCounterType) this.CounterType, this.categorySample.CounterTimeStamp);
        }

        internal CounterSample GetSingleValue()
        {
            long rawValue = this.instanceValues[0];
            long baseValue = 0L;
            if (this.BaseCounterDefinitionSample != null)
            {
                baseValue = this.BaseCounterDefinitionSample.instanceValues[0];
            }
            return new CounterSample(rawValue, baseValue, this.categorySample.CounterFrequency, this.categorySample.SystemFrequency, this.categorySample.TimeStamp, this.categorySample.TimeStamp100nSec, (PerformanceCounterType) this.CounterType, this.categorySample.CounterTimeStamp);
        }

        internal InstanceDataCollection ReadInstanceData(string counterName)
        {
            InstanceDataCollection datas = new InstanceDataCollection(counterName);
            string[] array = new string[this.categorySample.InstanceNameTable.Count];
            this.categorySample.InstanceNameTable.Keys.CopyTo(array, 0);
            int[] numArray = new int[this.categorySample.InstanceNameTable.Count];
            this.categorySample.InstanceNameTable.Values.CopyTo(numArray, 0);
            for (int i = 0; i < array.Length; i++)
            {
                long baseValue = 0L;
                if (this.BaseCounterDefinitionSample != null)
                {
                    int index = (int) this.BaseCounterDefinitionSample.categorySample.InstanceNameTable[array[i]];
                    baseValue = this.BaseCounterDefinitionSample.instanceValues[index];
                }
                CounterSample sample = new CounterSample(this.instanceValues[numArray[i]], baseValue, this.categorySample.CounterFrequency, this.categorySample.SystemFrequency, this.categorySample.TimeStamp, this.categorySample.TimeStamp100nSec, (PerformanceCounterType) this.CounterType, this.categorySample.CounterTimeStamp);
                datas.Add(array[i], new InstanceData(array[i], sample));
            }
            return datas;
        }

        private long ReadValue(IntPtr pointer)
        {
            if (this.size == 4)
            {
                return (long) ((ulong) Marshal.ReadInt32((IntPtr) (((long) pointer) + this.offset)));
            }
            if (this.size == 8)
            {
                return Marshal.ReadInt64((IntPtr) (((long) pointer) + this.offset));
            }
            return -1L;
        }

        internal void SetInstanceValue(int index, IntPtr dataRef)
        {
            this.instanceValues[index] = this.ReadValue(dataRef);
        }
    }
}

