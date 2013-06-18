namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("DefaultValue")]
    public class Parameter : ICloneable, IStateManager
    {
        private ParameterCollection _owner;
        private bool _tracking;
        private StateBag _viewState;

        public Parameter()
        {
        }

        public Parameter(string name)
        {
            this.Name = name;
        }

        protected Parameter(Parameter original)
        {
            this.DefaultValue = original.DefaultValue;
            this.Direction = original.Direction;
            this.Name = original.Name;
            this.ConvertEmptyStringToNull = original.ConvertEmptyStringToNull;
            this.Size = original.Size;
            this.Type = original.Type;
            this.DbType = original.DbType;
        }

        public Parameter(string name, System.Data.DbType dbType)
        {
            this.Name = name;
            this.DbType = dbType;
        }

        public Parameter(string name, TypeCode type)
        {
            this.Name = name;
            this.Type = type;
        }

        public Parameter(string name, System.Data.DbType dbType, string defaultValue)
        {
            this.Name = name;
            this.DbType = dbType;
            this.DefaultValue = defaultValue;
        }

        public Parameter(string name, TypeCode type, string defaultValue)
        {
            this.Name = name;
            this.Type = type;
            this.DefaultValue = defaultValue;
        }

        protected virtual Parameter Clone()
        {
            return new Parameter(this);
        }

        public static TypeCode ConvertDbTypeToTypeCode(System.Data.DbType dbType)
        {
            switch (dbType)
            {
                case System.Data.DbType.AnsiString:
                case System.Data.DbType.String:
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.StringFixedLength:
                    return TypeCode.String;

                case System.Data.DbType.Byte:
                    return TypeCode.Byte;

                case System.Data.DbType.Boolean:
                    return TypeCode.Boolean;

                case System.Data.DbType.Currency:
                case System.Data.DbType.Decimal:
                case System.Data.DbType.VarNumeric:
                    return TypeCode.Decimal;

                case System.Data.DbType.Date:
                case System.Data.DbType.DateTime:
                case System.Data.DbType.Time:
                case System.Data.DbType.DateTime2:
                    return TypeCode.DateTime;

                case System.Data.DbType.Double:
                    return TypeCode.Double;

                case System.Data.DbType.Int16:
                    return TypeCode.Int16;

                case System.Data.DbType.Int32:
                    return TypeCode.Int32;

                case System.Data.DbType.Int64:
                    return TypeCode.Int64;

                case System.Data.DbType.SByte:
                    return TypeCode.SByte;

                case System.Data.DbType.Single:
                    return TypeCode.Single;

                case System.Data.DbType.UInt16:
                    return TypeCode.UInt16;

                case System.Data.DbType.UInt32:
                    return TypeCode.UInt32;

                case System.Data.DbType.UInt64:
                    return TypeCode.UInt64;
            }
            return TypeCode.Object;
        }

        public static System.Data.DbType ConvertTypeCodeToDbType(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return System.Data.DbType.Boolean;

                case TypeCode.Char:
                    return System.Data.DbType.StringFixedLength;

                case TypeCode.SByte:
                    return System.Data.DbType.SByte;

                case TypeCode.Byte:
                    return System.Data.DbType.Byte;

                case TypeCode.Int16:
                    return System.Data.DbType.Int16;

                case TypeCode.UInt16:
                    return System.Data.DbType.UInt16;

                case TypeCode.Int32:
                    return System.Data.DbType.Int32;

                case TypeCode.UInt32:
                    return System.Data.DbType.UInt32;

                case TypeCode.Int64:
                    return System.Data.DbType.Int64;

                case TypeCode.UInt64:
                    return System.Data.DbType.UInt64;

                case TypeCode.Single:
                    return System.Data.DbType.Single;

                case TypeCode.Double:
                    return System.Data.DbType.Double;

                case TypeCode.Decimal:
                    return System.Data.DbType.Decimal;

                case TypeCode.DateTime:
                    return System.Data.DbType.DateTime;

                case TypeCode.String:
                    return System.Data.DbType.String;
            }
            return System.Data.DbType.Object;
        }

        protected internal virtual object Evaluate(HttpContext context, Control control)
        {
            return null;
        }

        public System.Data.DbType GetDatabaseType()
        {
            System.Data.DbType dbType = this.DbType;
            if (dbType == System.Data.DbType.Object)
            {
                return ConvertTypeCodeToDbType(this.Type);
            }
            if (this.Type != TypeCode.Empty)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Parameter_TypeNotSupported", new object[] { this.Name }));
            }
            return dbType;
        }

        internal object GetValue(object value, bool ignoreNullableTypeChanges)
        {
            System.Data.DbType dbType = this.DbType;
            if (dbType == System.Data.DbType.Object)
            {
                return GetValue(value, this.DefaultValue, this.Type, this.ConvertEmptyStringToNull, ignoreNullableTypeChanges);
            }
            if (this.Type != TypeCode.Empty)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Parameter_TypeNotSupported", new object[] { this.Name }));
            }
            return GetValue(value, this.DefaultValue, dbType, this.ConvertEmptyStringToNull, ignoreNullableTypeChanges);
        }

        internal static object GetValue(object value, string defaultValue, System.Data.DbType dbType, bool convertEmptyStringToNull, bool ignoreNullableTypeChanges)
        {
            if (((dbType != System.Data.DbType.DateTimeOffset) && (dbType != System.Data.DbType.Time)) && (dbType != System.Data.DbType.Guid))
            {
                TypeCode type = ConvertDbTypeToTypeCode(dbType);
                return GetValue(value, defaultValue, type, convertEmptyStringToNull, ignoreNullableTypeChanges);
            }
            value = HandleNullValue(value, defaultValue, convertEmptyStringToNull);
            if (value == null)
            {
                return null;
            }
            if (ignoreNullableTypeChanges && IsNullableType(value.GetType()))
            {
                return value;
            }
            if (dbType == System.Data.DbType.DateTimeOffset)
            {
                if (value is DateTimeOffset)
                {
                    return value;
                }
                return DateTimeOffset.Parse(value.ToString(), CultureInfo.CurrentCulture);
            }
            if (dbType == System.Data.DbType.Time)
            {
                if (value is TimeSpan)
                {
                    return value;
                }
                return TimeSpan.Parse(value.ToString(), CultureInfo.CurrentCulture);
            }
            if (dbType != System.Data.DbType.Guid)
            {
                return null;
            }
            if (value is Guid)
            {
                return value;
            }
            return new Guid(value.ToString());
        }

        internal static object GetValue(object value, string defaultValue, TypeCode type, bool convertEmptyStringToNull, bool ignoreNullableTypeChanges)
        {
            if (type == TypeCode.DBNull)
            {
                return DBNull.Value;
            }
            value = HandleNullValue(value, defaultValue, convertEmptyStringToNull);
            if (value == null)
            {
                return null;
            }
            if ((type == TypeCode.Object) || (type == TypeCode.Empty))
            {
                return value;
            }
            if (ignoreNullableTypeChanges && IsNullableType(value.GetType()))
            {
                return value;
            }
            return (value = Convert.ChangeType(value, type, CultureInfo.CurrentCulture));
        }

        private static object HandleNullValue(object value, string defaultValue, bool convertEmptyStringToNull)
        {
            if (convertEmptyStringToNull)
            {
                string str = value as string;
                if ((str != null) && (str.Length == 0))
                {
                    value = null;
                }
            }
            if (value == null)
            {
                if (convertEmptyStringToNull && string.IsNullOrEmpty(defaultValue))
                {
                    defaultValue = null;
                }
                if (defaultValue == null)
                {
                    return null;
                }
                value = defaultValue;
            }
            return value;
        }

        private static bool IsNullableType(System.Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                this.ViewState.LoadViewState(savedState);
            }
        }

        protected void OnParameterChanged()
        {
            if (this._owner != null)
            {
                this._owner.CallOnParametersChanged();
            }
        }

        protected virtual object SaveViewState()
        {
            if (this._viewState == null)
            {
                return null;
            }
            return this._viewState.SaveViewState();
        }

        protected internal virtual void SetDirty()
        {
            this.ViewState.SetDirty(true);
        }

        internal void SetOwner(ParameterCollection owner)
        {
            this._owner = owner;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        void IStateManager.LoadViewState(object savedState)
        {
            this.LoadViewState(savedState);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        public override string ToString()
        {
            return this.Name;
        }

        protected virtual void TrackViewState()
        {
            this._tracking = true;
            if (this._viewState != null)
            {
                this._viewState.TrackViewState();
            }
        }

        internal void UpdateValue(HttpContext context, Control control)
        {
            object obj2 = this.ViewState["ParameterValue"];
            object obj3 = this.Evaluate(context, control);
            this.ViewState["ParameterValue"] = obj3;
            if (((obj3 == null) && (obj2 != null)) || ((obj3 != null) && !obj3.Equals(obj2)))
            {
                this.OnParameterChanged();
            }
        }

        [WebCategory("Parameter"), DefaultValue(true), WebSysDescription("Parameter_ConvertEmptyStringToNull")]
        public bool ConvertEmptyStringToNull
        {
            get
            {
                object obj2 = this.ViewState["ConvertEmptyStringToNull"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                if (this.ConvertEmptyStringToNull != value)
                {
                    this.ViewState["ConvertEmptyStringToNull"] = value;
                    this.OnParameterChanged();
                }
            }
        }

        [WebSysDescription("Parameter_DbType"), DefaultValue(13), WebCategory("Parameter")]
        public System.Data.DbType DbType
        {
            get
            {
                object obj2 = this.ViewState["DbType"];
                if (obj2 == null)
                {
                    return System.Data.DbType.Object;
                }
                return (System.Data.DbType) obj2;
            }
            set
            {
                if ((value < System.Data.DbType.AnsiString) || (value > System.Data.DbType.DateTimeOffset))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.DbType != value)
                {
                    this.ViewState["DbType"] = value;
                    this.OnParameterChanged();
                }
            }
        }

        [WebSysDescription("Parameter_DefaultValue"), DefaultValue((string) null), WebCategory("Parameter")]
        public string DefaultValue
        {
            get
            {
                object obj2 = this.ViewState["DefaultValue"];
                return (obj2 as string);
            }
            set
            {
                if (this.DefaultValue != value)
                {
                    this.ViewState["DefaultValue"] = value;
                    this.OnParameterChanged();
                }
            }
        }

        [DefaultValue(1), WebSysDescription("Parameter_Direction"), WebCategory("Parameter")]
        public ParameterDirection Direction
        {
            get
            {
                object obj2 = this.ViewState["Direction"];
                if (obj2 == null)
                {
                    return ParameterDirection.Input;
                }
                return (ParameterDirection) obj2;
            }
            set
            {
                if (this.Direction != value)
                {
                    this.ViewState["Direction"] = value;
                    this.OnParameterChanged();
                }
            }
        }

        protected bool IsTrackingViewState
        {
            get
            {
                return this._tracking;
            }
        }

        [WebCategory("Parameter"), WebSysDescription("Parameter_Name"), DefaultValue("")]
        public string Name
        {
            get
            {
                object obj2 = this.ViewState["Name"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                if (this.Name != value)
                {
                    this.ViewState["Name"] = value;
                    this.OnParameterChanged();
                }
            }
        }

        [Browsable(false)]
        internal object ParameterValue
        {
            get
            {
                return this.GetValue(this.ViewState["ParameterValue"], false);
            }
        }

        [DefaultValue(0), WebSysDescription("Parameter_Size"), WebCategory("Parameter")]
        public int Size
        {
            get
            {
                object obj2 = this.ViewState["Size"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                if (this.Size != value)
                {
                    this.ViewState["Size"] = value;
                    this.OnParameterChanged();
                }
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        [WebCategory("Parameter"), WebSysDescription("Parameter_Type"), DefaultValue(0)]
        public TypeCode Type
        {
            get
            {
                object obj2 = this.ViewState["Type"];
                if (obj2 == null)
                {
                    return TypeCode.Empty;
                }
                return (TypeCode) obj2;
            }
            set
            {
                if ((value < TypeCode.Empty) || (value > TypeCode.String))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.Type != value)
                {
                    this.ViewState["Type"] = value;
                    this.OnParameterChanged();
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected StateBag ViewState
        {
            get
            {
                if (this._viewState == null)
                {
                    this._viewState = new StateBag();
                    if (this._tracking)
                    {
                        this._viewState.TrackViewState();
                    }
                }
                return this._viewState;
            }
        }
    }
}

