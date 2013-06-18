namespace System.Windows.Forms.Design
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Reflection;

    public class AxParameterData
    {
        private bool isByRef;
        private bool isIn;
        private bool isOptional;
        private bool isOut;
        private string name;
        private System.Reflection.ParameterInfo paramInfo;
        private Type type;
        private string typeName;

        public AxParameterData(System.Reflection.ParameterInfo info) : this(info, false)
        {
        }

        public AxParameterData(System.Reflection.ParameterInfo info, bool ignoreByRefs)
        {
            this.paramInfo = info;
            this.Name = info.Name;
            this.type = info.ParameterType;
            this.typeName = AxWrapperGen.MapTypeName(info.ParameterType);
            this.isByRef = info.ParameterType.IsByRef && !ignoreByRefs;
            this.isIn = info.IsIn && !ignoreByRefs;
            this.isOut = (info.IsOut && !this.isIn) && !ignoreByRefs;
            this.isOptional = info.IsOptional;
        }

        public AxParameterData(string inname, string typeName)
        {
            this.Name = inname;
            this.typeName = typeName;
        }

        public AxParameterData(string inname, Type type)
        {
            this.Name = inname;
            this.type = type;
            this.typeName = AxWrapperGen.MapTypeName(type);
        }

        public static AxParameterData[] Convert(System.Reflection.ParameterInfo[] infos)
        {
            return Convert(infos, false);
        }

        public static AxParameterData[] Convert(System.Reflection.ParameterInfo[] infos, bool ignoreByRefs)
        {
            if (infos == null)
            {
                return new AxParameterData[0];
            }
            int num = 0;
            AxParameterData[] dataArray = new AxParameterData[infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                dataArray[i] = new AxParameterData(infos[i], ignoreByRefs);
                if ((dataArray[i].Name == null) || (dataArray[i].Name == ""))
                {
                    dataArray[i].Name = "param" + num++;
                }
            }
            return dataArray;
        }

        internal static Type GetByRefBaseType(Type t)
        {
            if (t.IsByRef && t.FullName.EndsWith("&"))
            {
                Type type = t.Assembly.GetType(t.FullName.Substring(0, t.FullName.Length - 1), false);
                if (type != null)
                {
                    t = type;
                }
            }
            return t;
        }

        public FieldDirection Direction
        {
            get
            {
                if (this.IsOut)
                {
                    return FieldDirection.Out;
                }
                if (this.IsByRef)
                {
                    return FieldDirection.Ref;
                }
                return FieldDirection.In;
            }
        }

        public bool IsByRef
        {
            get
            {
                return this.isByRef;
            }
        }

        public bool IsIn
        {
            get
            {
                return this.isIn;
            }
        }

        public bool IsOptional
        {
            get
            {
                return this.isOptional;
            }
        }

        public bool IsOut
        {
            get
            {
                return this.isOut;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    this.name = null;
                }
                else if (((value != null) && (value.Length > 0)) && char.IsUpper(value[0]))
                {
                    char[] chArray = value.ToCharArray();
                    if (chArray.Length > 0)
                    {
                        chArray[0] = char.ToLower(chArray[0], CultureInfo.InvariantCulture);
                    }
                    this.name = new string(chArray);
                }
                else
                {
                    this.name = value;
                }
            }
        }

        internal Type ParameterBaseType
        {
            get
            {
                return GetByRefBaseType(this.ParameterType);
            }
        }

        internal System.Reflection.ParameterInfo ParameterInfo
        {
            get
            {
                return this.paramInfo;
            }
        }

        public Type ParameterType
        {
            get
            {
                return this.type;
            }
        }

        public string TypeName
        {
            get
            {
                if (this.typeName == null)
                {
                    this.typeName = this.ParameterBaseType.FullName;
                }
                else if (this.typeName.EndsWith("&"))
                {
                    this.typeName = this.typeName.TrimEnd(new char[] { '&' });
                }
                return this.typeName;
            }
        }
    }
}

