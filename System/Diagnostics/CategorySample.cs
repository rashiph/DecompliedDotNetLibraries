namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal class CategorySample
    {
        internal readonly long CounterFrequency;
        internal Hashtable CounterTable;
        internal readonly long CounterTimeStamp;
        private System.Diagnostics.CategoryEntry entry;
        internal Hashtable InstanceNameTable;
        internal bool IsMultiInstance;
        private PerformanceCounterLib library;
        internal readonly long SystemFrequency;
        internal readonly long TimeStamp;
        internal readonly long TimeStamp100nSec;

        internal unsafe CategorySample(byte[] data, System.Diagnostics.CategoryEntry entry, PerformanceCounterLib library)
        {
            this.entry = entry;
            this.library = library;
            int nameIndex = entry.NameIndex;
            Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK structure = new Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK();
            fixed (byte* numRef = data)
            {
                IntPtr ptr = new IntPtr((void*) numRef);
                Marshal.PtrToStructure(ptr, structure);
                this.SystemFrequency = structure.PerfFreq;
                this.TimeStamp = structure.PerfTime;
                this.TimeStamp100nSec = structure.PerfTime100nSec;
                ptr = (IntPtr) (((long) ptr) + structure.HeaderLength);
                int numObjectTypes = structure.NumObjectTypes;
                if (numObjectTypes == 0)
                {
                    this.CounterTable = new Hashtable();
                    this.InstanceNameTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    return;
                }
                Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE perf_object_type = null;
                bool flag = false;
                for (int i = 0; i < numObjectTypes; i++)
                {
                    perf_object_type = new Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE();
                    Marshal.PtrToStructure(ptr, perf_object_type);
                    if (perf_object_type.ObjectNameTitleIndex == nameIndex)
                    {
                        flag = true;
                        break;
                    }
                    ptr = (IntPtr) (((long) ptr) + perf_object_type.TotalByteLength);
                }
                if (!flag)
                {
                    throw new InvalidOperationException(SR.GetString("CantReadCategoryIndex", new object[] { nameIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                this.CounterFrequency = perf_object_type.PerfFreq;
                this.CounterTimeStamp = perf_object_type.PerfTime;
                int numCounters = perf_object_type.NumCounters;
                int numInstances = perf_object_type.NumInstances;
                if (numInstances == -1)
                {
                    this.IsMultiInstance = false;
                }
                else
                {
                    this.IsMultiInstance = true;
                }
                ptr = (IntPtr) (((long) ptr) + perf_object_type.HeaderLength);
                CounterDefinitionSample[] sampleArray = new CounterDefinitionSample[numCounters];
                this.CounterTable = new Hashtable(numCounters);
                for (int j = 0; j < sampleArray.Length; j++)
                {
                    Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION perf_counter_definition = new Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION();
                    Marshal.PtrToStructure(ptr, perf_counter_definition);
                    sampleArray[j] = new CounterDefinitionSample(perf_counter_definition, this, numInstances);
                    ptr = (IntPtr) (((long) ptr) + perf_counter_definition.ByteLength);
                    int counterType = sampleArray[j].CounterType;
                    if (!PerformanceCounterLib.IsBaseCounter(counterType))
                    {
                        if (counterType != 0x40000200)
                        {
                            this.CounterTable[sampleArray[j].NameIndex] = sampleArray[j];
                        }
                    }
                    else if (j > 0)
                    {
                        sampleArray[j - 1].BaseCounterDefinitionSample = sampleArray[j];
                    }
                }
                if (!this.IsMultiInstance)
                {
                    this.InstanceNameTable = new Hashtable(1, StringComparer.OrdinalIgnoreCase);
                    this.InstanceNameTable["systemdiagnosticsperfcounterlibsingleinstance"] = 0;
                    for (int k = 0; k < sampleArray.Length; k++)
                    {
                        sampleArray[k].SetInstanceValue(0, ptr);
                    }
                }
                else
                {
                    string[] instanceNamesFromIndex = null;
                    this.InstanceNameTable = new Hashtable(numInstances, StringComparer.OrdinalIgnoreCase);
                    for (int m = 0; m < numInstances; m++)
                    {
                        string str;
                        Microsoft.Win32.NativeMethods.PERF_INSTANCE_DEFINITION perf_instance_definition = new Microsoft.Win32.NativeMethods.PERF_INSTANCE_DEFINITION();
                        Marshal.PtrToStructure(ptr, perf_instance_definition);
                        if ((perf_instance_definition.ParentObjectTitleIndex > 0) && (instanceNamesFromIndex == null))
                        {
                            instanceNamesFromIndex = this.GetInstanceNamesFromIndex(perf_instance_definition.ParentObjectTitleIndex);
                        }
                        if (((instanceNamesFromIndex != null) && (perf_instance_definition.ParentObjectInstance >= 0)) && (perf_instance_definition.ParentObjectInstance < (instanceNamesFromIndex.Length - 1)))
                        {
                            str = instanceNamesFromIndex[perf_instance_definition.ParentObjectInstance] + "/" + Marshal.PtrToStringUni((IntPtr) (((long) ptr) + perf_instance_definition.NameOffset));
                        }
                        else
                        {
                            str = Marshal.PtrToStringUni((IntPtr) (((long) ptr) + perf_instance_definition.NameOffset));
                        }
                        string key = str;
                        int num10 = 1;
                        while (true)
                        {
                            if (!this.InstanceNameTable.ContainsKey(key))
                            {
                                this.InstanceNameTable[key] = m;
                                break;
                            }
                            key = str + "#" + num10.ToString(CultureInfo.InvariantCulture);
                            num10++;
                        }
                        ptr = (IntPtr) (((long) ptr) + perf_instance_definition.ByteLength);
                        for (int n = 0; n < sampleArray.Length; n++)
                        {
                            sampleArray[n].SetInstanceValue(m, ptr);
                        }
                        ptr = (IntPtr) (((long) ptr) + Marshal.ReadInt32(ptr));
                    }
                }
            }
        }

        internal CounterDefinitionSample GetCounterDefinitionSample(string counter)
        {
            for (int i = 0; i < this.entry.CounterIndexes.Length; i++)
            {
                int num2 = this.entry.CounterIndexes[i];
                string strA = (string) this.library.NameTable[num2];
                if ((strA != null) && (string.Compare(strA, counter, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    CounterDefinitionSample sample = (CounterDefinitionSample) this.CounterTable[num2];
                    if (sample != null)
                    {
                        return sample;
                    }
                    foreach (CounterDefinitionSample sample2 in this.CounterTable.Values)
                    {
                        if ((sample2.BaseCounterDefinitionSample != null) && (sample2.BaseCounterDefinitionSample.NameIndex == num2))
                        {
                            return sample2.BaseCounterDefinitionSample;
                        }
                    }
                    throw new InvalidOperationException(SR.GetString("CounterLayout"));
                }
            }
            throw new InvalidOperationException(SR.GetString("CantReadCounter", new object[] { counter }));
        }

        internal unsafe string[] GetInstanceNamesFromIndex(int categoryIndex)
        {
            fixed (byte* numRef = this.library.GetPerformanceData(categoryIndex.ToString(CultureInfo.InvariantCulture)))
            {
                IntPtr ptr = new IntPtr((void*) numRef);
                Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK structure = new Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK();
                Marshal.PtrToStructure(ptr, structure);
                ptr = (IntPtr) (((long) ptr) + structure.HeaderLength);
                int numObjectTypes = structure.NumObjectTypes;
                Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE perf_object_type = null;
                bool flag = false;
                for (int i = 0; i < numObjectTypes; i++)
                {
                    perf_object_type = new Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE();
                    Marshal.PtrToStructure(ptr, perf_object_type);
                    if (perf_object_type.ObjectNameTitleIndex == categoryIndex)
                    {
                        flag = true;
                        break;
                    }
                    ptr = (IntPtr) (((long) ptr) + perf_object_type.TotalByteLength);
                }
                if (!flag)
                {
                    return new string[0];
                }
                int numCounters = perf_object_type.NumCounters;
                int numInstances = perf_object_type.NumInstances;
                ptr = (IntPtr) (((long) ptr) + perf_object_type.HeaderLength);
                if (numInstances == -1)
                {
                    return new string[0];
                }
                CounterDefinitionSample[] sampleArray = new CounterDefinitionSample[numCounters];
                for (int j = 0; j < sampleArray.Length; j++)
                {
                    Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION perf_counter_definition = new Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION();
                    Marshal.PtrToStructure(ptr, perf_counter_definition);
                    ptr = (IntPtr) (((long) ptr) + perf_counter_definition.ByteLength);
                }
                string[] strArray = new string[numInstances];
                for (int k = 0; k < numInstances; k++)
                {
                    Microsoft.Win32.NativeMethods.PERF_INSTANCE_DEFINITION perf_instance_definition = new Microsoft.Win32.NativeMethods.PERF_INSTANCE_DEFINITION();
                    Marshal.PtrToStructure(ptr, perf_instance_definition);
                    strArray[k] = Marshal.PtrToStringUni((IntPtr) (((long) ptr) + perf_instance_definition.NameOffset));
                    ptr = (IntPtr) (((long) ptr) + perf_instance_definition.ByteLength);
                    ptr = (IntPtr) (((long) ptr) + Marshal.ReadInt32(ptr));
                }
                return strArray;
            }
        }

        internal InstanceDataCollectionCollection ReadCategory()
        {
            InstanceDataCollectionCollection collections = new InstanceDataCollectionCollection();
            for (int i = 0; i < this.entry.CounterIndexes.Length; i++)
            {
                int num2 = this.entry.CounterIndexes[i];
                string counterName = (string) this.library.NameTable[num2];
                if ((counterName != null) && (counterName != string.Empty))
                {
                    CounterDefinitionSample sample = (CounterDefinitionSample) this.CounterTable[num2];
                    if (sample != null)
                    {
                        collections.Add(counterName, sample.ReadInstanceData(counterName));
                    }
                }
            }
            return collections;
        }
    }
}

