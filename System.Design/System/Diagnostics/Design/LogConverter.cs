namespace System.Diagnostics.Design
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class LogConverter : TypeConverter
    {
        private string oldMachineName;
        private TypeConverter.StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return ((string) value).Trim();
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            EventLog log = (context == null) ? null : (context.Instance as EventLog);
            string machineName = ".";
            if (log != null)
            {
                machineName = log.MachineName;
            }
            if ((this.values == null) || (machineName != this.oldMachineName))
            {
                try
                {
                    EventLog[] eventLogs = EventLog.GetEventLogs(machineName);
                    object[] values = new object[eventLogs.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = eventLogs[i].Log;
                    }
                    this.values = new TypeConverter.StandardValuesCollection(values);
                    this.oldMachineName = machineName;
                }
                catch (Exception)
                {
                }
            }
            return this.values;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

