namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class MachineKeyValidationConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            return ConvertToEnum((string) data);
        }

        internal static string ConvertFromEnum(MachineKeyValidation enumValue)
        {
            switch (enumValue)
            {
                case MachineKeyValidation.MD5:
                    return "MD5";

                case MachineKeyValidation.SHA1:
                    return "SHA1";

                case MachineKeyValidation.TripleDES:
                    return "3DES";

                case MachineKeyValidation.AES:
                    return "AES";

                case MachineKeyValidation.HMACSHA256:
                    return "HMACSHA256";

                case MachineKeyValidation.HMACSHA384:
                    return "HMACSHA384";

                case MachineKeyValidation.HMACSHA512:
                    return "HMACSHA512";
            }
            throw new ArgumentException(System.Web.SR.GetString("Wrong_validation_enum"));
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            if (!(value is MachineKeyValidation))
            {
                throw new ArgumentException(System.Web.SR.GetString("Config_Invalid_enum_value", new object[] { "SHA1, MD5, 3DES, AES, HMACSHA256, HMACSHA384, HMACSHA512" }));
            }
            return ConvertFromEnum((MachineKeyValidation) value);
        }

        internal static MachineKeyValidation ConvertToEnum(string strValue)
        {
            switch (strValue)
            {
                case "SHA1":
                    return MachineKeyValidation.SHA1;

                case "MD5":
                    return MachineKeyValidation.MD5;

                case "3DES":
                    return MachineKeyValidation.TripleDES;

                case "AES":
                    return MachineKeyValidation.AES;

                case "HMACSHA256":
                    return MachineKeyValidation.HMACSHA256;

                case "HMACSHA384":
                    return MachineKeyValidation.HMACSHA384;

                case "HMACSHA512":
                    return MachineKeyValidation.HMACSHA512;

                case null:
                    return MachineKeyValidation.SHA1;
            }
            if (!strValue.StartsWith("alg:", StringComparison.Ordinal))
            {
                throw new ArgumentException(System.Web.SR.GetString("Wrong_validation_enum"));
            }
            return MachineKeyValidation.Custom;
        }
    }
}

