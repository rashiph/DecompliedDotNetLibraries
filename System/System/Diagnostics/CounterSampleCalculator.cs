namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public static class CounterSampleCalculator
    {
        private static bool perfCounterDllLoaded;

        public static float ComputeCounterValue(CounterSample newSample)
        {
            return ComputeCounterValue(CounterSample.Empty, newSample);
        }

        public static float ComputeCounterValue(CounterSample oldSample, CounterSample newSample)
        {
            int counterType = (int) newSample.CounterType;
            if (oldSample.SystemFrequency == 0L)
            {
                if ((((counterType != 0x20020400) && (counterType != 0x10000)) && ((counterType != 0) && (counterType != 0x10100))) && ((counterType != 0x100) && (counterType != 0x42030500)))
                {
                    return 0f;
                }
            }
            else if (oldSample.CounterType != newSample.CounterType)
            {
                throw new InvalidOperationException(SR.GetString("MismatchedCounterTypes"));
            }
            if (counterType == 0x30240500)
            {
                return GetElapsedTime(oldSample, newSample);
            }
            Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER newPdhValue = new Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER();
            Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER oldPdhValue = new Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER();
            FillInValues(oldSample, newSample, oldPdhValue, newPdhValue);
            LoadPerfCounterDll();
            Microsoft.Win32.NativeMethods.PDH_FMT_COUNTERVALUE pFmtValue = new Microsoft.Win32.NativeMethods.PDH_FMT_COUNTERVALUE();
            long systemFrequency = newSample.SystemFrequency;
            int error = Microsoft.Win32.SafeNativeMethods.FormatFromRawValue((uint) counterType, 0x9200, ref systemFrequency, newPdhValue, oldPdhValue, pFmtValue);
            switch (error)
            {
                case 0:
                    return (float) pFmtValue.data;

                case -2147481640:
                case -2147481642:
                case -2147481643:
                    return 0f;
            }
            throw new Win32Exception(error, SR.GetString("PerfCounterPdhError", new object[] { error.ToString("x", CultureInfo.InvariantCulture) }));
        }

        private static void FillInValues(CounterSample oldSample, CounterSample newSample, Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER oldPdhValue, Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER newPdhValue)
        {
            int counterType = (int) newSample.CounterType;
            switch (counterType)
            {
                case 0:
                case 0x100:
                case 0x10000:
                case 0x400500:
                case 0x10100:
                case 0x400400:
                    newPdhValue.FirstValue = newSample.RawValue;
                    newPdhValue.SecondValue = 0L;
                    oldPdhValue.FirstValue = oldSample.RawValue;
                    oldPdhValue.SecondValue = 0L;
                    return;

                case 0x410400:
                case 0x650500:
                case 0x450400:
                case 0x10410400:
                case 0x20610500:
                    newPdhValue.FirstValue = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp;
                    oldPdhValue.FirstValue = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp;
                    return;

                case 0x550500:
                    newPdhValue.FirstValue = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp100nSec;
                    oldPdhValue.FirstValue = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp100nSec;
                    return;

                case 0x450500:
                case 0x10410500:
                case 0x20410500:
                case 0x22410500:
                case 0x21410500:
                case 0x23410500:
                    newPdhValue.FirstValue = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp;
                    oldPdhValue.FirstValue = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp;
                    switch (counterType)
                    {
                        case 0x22410500:
                        case 0x23410500:
                            newPdhValue.FirstValue *= (uint) newSample.CounterFrequency;
                            if (oldSample.CounterFrequency != 0L)
                            {
                                oldPdhValue.FirstValue *= (uint) oldSample.CounterFrequency;
                            }
                            break;
                    }
                    if ((counterType & 0x2000000) == 0x2000000)
                    {
                        newPdhValue.MultiCount = (int) newSample.BaseValue;
                        oldPdhValue.MultiCount = (int) oldSample.BaseValue;
                    }
                    return;

                case 0x20020400:
                case 0x20020500:
                case 0x20470500:
                case 0x20670500:
                case 0x20c20400:
                case 0x20570500:
                case 0x30020400:
                case 0x40020500:
                    newPdhValue.FirstValue = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.BaseValue;
                    oldPdhValue.FirstValue = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.BaseValue;
                    return;

                case 0x20510500:
                case 0x22510500:
                case 0x21510500:
                case 0x23510500:
                    newPdhValue.FirstValue = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp100nSec;
                    oldPdhValue.FirstValue = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp100nSec;
                    if ((counterType & 0x2000000) == 0x2000000)
                    {
                        newPdhValue.MultiCount = (int) newSample.BaseValue;
                        oldPdhValue.MultiCount = (int) oldSample.BaseValue;
                    }
                    return;
            }
            newPdhValue.FirstValue = 0L;
            newPdhValue.SecondValue = 0L;
            oldPdhValue.FirstValue = 0L;
            oldPdhValue.SecondValue = 0L;
        }

        private static float GetElapsedTime(CounterSample oldSample, CounterSample newSample)
        {
            if (newSample.RawValue == 0L)
            {
                return 0f;
            }
            float counterFrequency = oldSample.CounterFrequency;
            if ((oldSample.UnsignedRawValue >= newSample.CounterTimeStamp) || (counterFrequency <= 0f))
            {
                return 0f;
            }
            float num2 = ((ulong) newSample.CounterTimeStamp) - oldSample.UnsignedRawValue;
            return (num2 / counterFrequency);
        }

        private static void LoadPerfCounterDll()
        {
            if (!perfCounterDllLoaded)
            {
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                if (Microsoft.Win32.SafeNativeMethods.LoadLibrary(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "perfcounter.dll")) == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                perfCounterDllLoaded = true;
            }
        }
    }
}

