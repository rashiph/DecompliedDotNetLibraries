namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.Design;

    internal abstract class BaseAutoFormat<T> : DesignerAutoFormat where T: Control
    {
        private bool _initialized;
        private readonly string _schemeName;
        private readonly string _schemes;

        public BaseAutoFormat(string schemeName, string schemes) : base(System.Design.SR.GetString(schemeName))
        {
            this._schemes = schemes;
            this._schemeName = schemeName;
        }

        public override void Apply(Control control)
        {
            T local = control as T;
            if (local != null)
            {
                this.EnsureInitialized();
                this.Apply(local);
            }
        }

        protected abstract void Apply(T control);
        private void EnsureInitialized()
        {
            if (!this._initialized)
            {
                DataRow schemeDataRow = ControlDesigner.GetSchemeDataRow(this._schemeName, this._schemes);
                this.Initialize(schemeDataRow);
                this._initialized = true;
            }
        }

        protected static bool GetBooleanProperty(string propertyTag, DataRow schemeData)
        {
            object obj2 = schemeData[propertyTag];
            return (((obj2 != null) && !obj2.Equals(DBNull.Value)) && bool.Parse(obj2.ToString()));
        }

        protected static int GetIntProperty(string propertyTag, DataRow schemeData)
        {
            return BaseAutoFormat<T>.GetIntProperty(propertyTag, schemeData, 0);
        }

        protected static int GetIntProperty(string propertyTag, DataRow schemeData, int defaultValue)
        {
            object obj2 = schemeData[propertyTag];
            if ((obj2 != null) && !obj2.Equals(DBNull.Value))
            {
                return int.Parse(obj2.ToString(), CultureInfo.InvariantCulture);
            }
            return defaultValue;
        }

        protected static int GetIntProperty(string propertyTag, int defaultValue, DataRow schemeData)
        {
            return BaseAutoFormat<T>.GetIntProperty(propertyTag, schemeData, defaultValue);
        }

        protected static string GetStringProperty(string propertyTag, DataRow schemeData)
        {
            return BaseAutoFormat<T>.GetStringProperty(propertyTag, schemeData, string.Empty);
        }

        protected static string GetStringProperty(string propertyTag, DataRow schemeData, string defaultValue)
        {
            object obj2 = schemeData[propertyTag];
            if ((obj2 != null) && !obj2.Equals(DBNull.Value))
            {
                return obj2.ToString();
            }
            return defaultValue;
        }

        protected abstract void Initialize(DataRow schemeData);
    }
}

