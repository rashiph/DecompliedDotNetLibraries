namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.IO;

    public class RootedPathValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            return (type == typeof(string));
        }

        public override void Validate(object value)
        {
            string str = value as string;
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Trim();
                if (!string.IsNullOrEmpty(str))
                {
                    if (!Path.IsPathRooted(str))
                    {
                        throw new ConfigurationErrorsException();
                    }
                    char ch = str[0];
                    if ((ch == Path.DirectorySeparatorChar) || (ch == Path.AltDirectorySeparatorChar))
                    {
                        throw new ConfigurationErrorsException();
                    }
                }
            }
        }
    }
}

