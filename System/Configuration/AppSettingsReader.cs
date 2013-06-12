namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;

    public class AppSettingsReader
    {
        private NameValueCollection map = ConfigurationManager.AppSettings;
        private static string NullString = "None";
        private static Type[] paramsArray = new Type[] { stringType };
        private static Type stringType = typeof(string);

        private int GetNoneNesting(string val)
        {
            int indexB = 0;
            int length = val.Length;
            if (length > 1)
            {
                while ((val[indexB] == '(') && (val[(length - indexB) - 1] == ')'))
                {
                    indexB++;
                }
                if ((indexB > 0) && (string.Compare(NullString, 0, val, indexB, length - (2 * indexB), StringComparison.Ordinal) != 0))
                {
                    indexB = 0;
                }
            }
            return indexB;
        }

        public object GetValue(string key, Type type)
        {
            object obj2;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            string val = this.map[key];
            if (val == null)
            {
                throw new InvalidOperationException(System.SR.GetString("AppSettingsReaderNoKey", new object[] { key }));
            }
            if (type == stringType)
            {
                switch (this.GetNoneNesting(val))
                {
                    case 0:
                        return val;

                    case 1:
                        return null;
                }
                return val.Substring(1, val.Length - 2);
            }
            try
            {
                obj2 = Convert.ChangeType(val, type, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                string str2 = (val.Length == 0) ? "AppSettingsReaderEmptyString" : val;
                throw new InvalidOperationException(System.SR.GetString("AppSettingsReaderCantParse", new object[] { str2, key, type.ToString() }));
            }
            return obj2;
        }
    }
}

