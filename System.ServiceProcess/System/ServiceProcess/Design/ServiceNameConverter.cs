namespace System.ServiceProcess.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceProcess;

    internal class ServiceNameConverter : TypeConverter
    {
        private string previousMachineName;
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
            ServiceController controller = (context == null) ? null : (context.Instance as ServiceController);
            string machineName = ".";
            if (controller != null)
            {
                machineName = controller.MachineName;
            }
            if ((this.values == null) || (machineName != this.previousMachineName))
            {
                try
                {
                    ServiceController[] services = ServiceController.GetServices(machineName);
                    string[] values = new string[services.Length];
                    for (int i = 0; i < services.Length; i++)
                    {
                        values[i] = services[i].ServiceName;
                    }
                    this.values = new TypeConverter.StandardValuesCollection(values);
                    this.previousMachineName = machineName;
                }
                catch
                {
                }
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

