namespace System.Management.Instrumentation
{
    using System;
    using System.Collections;
    using System.Management;
    using System.Reflection;

    internal class SchemaMapping
    {
        private string className;
        private string classPath;
        private Type classType;
        private CodeWriter code = new CodeWriter();
        private string codeClassName;
        private System.Management.Instrumentation.InstrumentationType instrumentationType;
        private ManagementClass newClass;

        public SchemaMapping(Type type, SchemaNaming naming, Hashtable mapTypeToConverterClassName)
        {
            this.codeClassName = (string) mapTypeToConverterClassName[type];
            this.classType = type;
            bool flag = false;
            string baseClassName = ManagedNameAttribute.GetBaseClassName(type);
            this.className = ManagedNameAttribute.GetMemberName(type);
            this.instrumentationType = InstrumentationClassAttribute.GetAttribute(type).InstrumentationType;
            this.classPath = naming.NamespaceName + ":" + this.className;
            if (baseClassName == null)
            {
                this.newClass = new ManagementClass(naming.NamespaceName, "", null);
                this.newClass.SystemProperties["__CLASS"].Value = this.className;
            }
            else
            {
                ManagementClass class2 = new ManagementClass(naming.NamespaceName + ":" + baseClassName);
                if (this.instrumentationType == System.Management.Instrumentation.InstrumentationType.Instance)
                {
                    bool flag2 = false;
                    try
                    {
                        QualifierData data = class2.Qualifiers["abstract"];
                        if (data.Value is bool)
                        {
                            flag2 = (bool) data.Value;
                        }
                    }
                    catch (ManagementException exception)
                    {
                        if (exception.ErrorCode != ManagementStatus.NotFound)
                        {
                            throw;
                        }
                    }
                    if (!flag2)
                    {
                        throw new Exception(RC.GetString("CLASSINST_EXCEPT"));
                    }
                }
                this.newClass = class2.Derive(this.className);
            }
            CodeWriter writer = this.code.AddChild("public class " + this.codeClassName + " : IWmiConverter");
            CodeWriter writer2 = writer.AddChild(new CodeWriter());
            writer2.Line("static ManagementClass managementClass = new ManagementClass(@\"" + this.classPath + "\");");
            writer2.Line("static IntPtr classWbemObjectIP;");
            writer2.Line("static Guid iidIWbemObjectAccess = new Guid(\"49353C9A-516B-11D1-AEA6-00C04FB68820\");");
            writer2.Line("internal ManagementObject instance = managementClass.CreateInstance();");
            writer2.Line("object reflectionInfoTempObj = null ; ");
            writer2.Line("FieldInfo reflectionIWbemClassObjectField = null ; ");
            writer2.Line("IntPtr emptyWbemObject = IntPtr.Zero ; ");
            writer2.Line("IntPtr originalObject = IntPtr.Zero ; ");
            writer2.Line("bool toWmiCalled = false ; ");
            writer2.Line("IntPtr theClone = IntPtr.Zero;");
            writer2.Line("public static ManagementObject emptyInstance = managementClass.CreateInstance();");
            writer2.Line("public IntPtr instWbemObjectAccessIP;");
            CodeWriter writer3 = writer.AddChild("static " + this.codeClassName + "()");
            writer3.Line("classWbemObjectIP = (IntPtr)managementClass;");
            writer3.Line("IntPtr wbemObjectAccessIP;");
            writer3.Line("Marshal.QueryInterface(classWbemObjectIP, ref iidIWbemObjectAccess, out wbemObjectAccessIP);");
            writer3.Line("int cimType;");
            CodeWriter writer4 = writer.AddChild("public " + this.codeClassName + "()");
            writer4.Line("IntPtr wbemObjectIP = (IntPtr)instance;");
            writer4.Line("originalObject = (IntPtr)instance;");
            writer4.Line("Marshal.QueryInterface(wbemObjectIP, ref iidIWbemObjectAccess, out instWbemObjectAccessIP);");
            writer4.Line("FieldInfo tempField = instance.GetType().GetField ( \"_wbemObject\", BindingFlags.Instance | BindingFlags.NonPublic );");
            writer4.Line("if ( tempField == null )");
            writer4.Line("{");
            writer4.Line("   tempField = instance.GetType().GetField ( \"wbemObject\", BindingFlags.Instance | BindingFlags.NonPublic ) ;");
            writer4.Line("}");
            writer4.Line("reflectionInfoTempObj = tempField.GetValue (instance) ;");
            writer4.Line("reflectionIWbemClassObjectField = reflectionInfoTempObj.GetType().GetField (\"pWbemClassObject\", BindingFlags.Instance | BindingFlags.NonPublic );");
            writer4.Line("emptyWbemObject = (IntPtr) emptyInstance;");
            CodeWriter writer5 = writer.AddChild("~" + this.codeClassName + "()");
            writer5.AddChild("if(instWbemObjectAccessIP != IntPtr.Zero)").Line("Marshal.Release(instWbemObjectAccessIP);");
            writer5.Line("if ( toWmiCalled == true )");
            writer5.Line("{");
            writer5.Line("\tMarshal.Release (originalObject);");
            writer5.Line("}");
            CodeWriter writer6 = writer.AddChild("public void ToWMI(object obj)");
            writer6.Line("toWmiCalled = true ;");
            writer6.Line("if(instWbemObjectAccessIP != IntPtr.Zero)");
            writer6.Line("{");
            writer6.Line("    Marshal.Release(instWbemObjectAccessIP);");
            writer6.Line("    instWbemObjectAccessIP = IntPtr.Zero;");
            writer6.Line("}");
            writer6.Line("if(theClone != IntPtr.Zero)");
            writer6.Line("{");
            writer6.Line("    Marshal.Release(theClone);");
            writer6.Line("    theClone = IntPtr.Zero;");
            writer6.Line("}");
            writer6.Line("IWOA.Clone_f(12, emptyWbemObject, out theClone) ;");
            writer6.Line("Marshal.QueryInterface(theClone, ref iidIWbemObjectAccess, out instWbemObjectAccessIP) ;");
            writer6.Line("reflectionIWbemClassObjectField.SetValue ( reflectionInfoTempObj, theClone ) ;");
            writer6.Line(string.Format("{0} instNET = ({0})obj;", type.FullName.Replace('+', '.')));
            writer.AddChild("public static explicit operator IntPtr(" + this.codeClassName + " obj)").Line("return obj.instWbemObjectAccessIP;");
            writer2.Line("public ManagementObject GetInstance() {return instance;}");
            PropertyDataCollection properties = this.newClass.Properties;
            switch (this.instrumentationType)
            {
                case System.Management.Instrumentation.InstrumentationType.Instance:
                    properties.Add("ProcessId", CimType.String, false);
                    properties.Add("InstanceId", CimType.String, false);
                    properties["ProcessId"].Qualifiers.Add("key", true);
                    properties["InstanceId"].Qualifiers.Add("key", true);
                    this.newClass.Qualifiers.Add("dynamic", true, false, false, false, true);
                    this.newClass.Qualifiers.Add("provider", naming.DecoupledProviderInstanceName, false, false, false, true);
                    break;

                case System.Management.Instrumentation.InstrumentationType.Abstract:
                    this.newClass.Qualifiers.Add("abstract", true, false, false, false, true);
                    break;
            }
            int num = 0;
            bool flag3 = false;
            foreach (MemberInfo info in type.GetMembers())
            {
                if (((info is FieldInfo) || (info is PropertyInfo)) && (info.GetCustomAttributes(typeof(IgnoreMemberAttribute), false).Length <= 0))
                {
                    Type fieldType;
                    if (info is FieldInfo)
                    {
                        FieldInfo info2 = info as FieldInfo;
                        if (info2.IsStatic)
                        {
                            ThrowUnsupportedMember(info);
                        }
                    }
                    else if (info is PropertyInfo)
                    {
                        PropertyInfo info3 = info as PropertyInfo;
                        if (!info3.CanRead)
                        {
                            ThrowUnsupportedMember(info);
                        }
                        MethodInfo getMethod = info3.GetGetMethod();
                        if ((null == getMethod) || getMethod.IsStatic)
                        {
                            ThrowUnsupportedMember(info);
                        }
                        if (getMethod.GetParameters().Length > 0)
                        {
                            ThrowUnsupportedMember(info);
                        }
                    }
                    string memberName = ManagedNameAttribute.GetMemberName(info);
                    if (info is FieldInfo)
                    {
                        fieldType = (info as FieldInfo).FieldType;
                    }
                    else
                    {
                        fieldType = (info as PropertyInfo).PropertyType;
                    }
                    bool isArray = false;
                    if (fieldType.IsArray)
                    {
                        if (fieldType.GetArrayRank() != 1)
                        {
                            ThrowUnsupportedMember(info);
                        }
                        isArray = true;
                        fieldType = fieldType.GetElementType();
                    }
                    string str3 = null;
                    string str4 = null;
                    if (mapTypeToConverterClassName.Contains(fieldType))
                    {
                        str4 = (string) mapTypeToConverterClassName[fieldType];
                        str3 = ManagedNameAttribute.GetMemberName(fieldType);
                    }
                    bool flag5 = false;
                    if (fieldType == typeof(object))
                    {
                        flag5 = true;
                        if (!flag)
                        {
                            flag = true;
                            writer2.Line("static Hashtable mapTypeToConverter = new Hashtable();");
                            foreach (DictionaryEntry entry in mapTypeToConverterClassName)
                            {
                                string introduced55 = ((Type) entry.Key).FullName.Replace('+', '.');
                                writer3.Line(string.Format("mapTypeToConverter[typeof({0})] = typeof({1});", introduced55, (string) entry.Value));
                            }
                        }
                    }
                    string str5 = "prop_" + num;
                    string str6 = "handle_" + num++;
                    writer2.Line("static int " + str6 + ";");
                    writer3.Line(string.Format("IWOA.GetPropertyHandle_f27(27, wbemObjectAccessIP, \"{0}\", out cimType, out {1});", memberName, str6));
                    writer2.Line("PropertyData " + str5 + ";");
                    writer4.Line(string.Format("{0} = instance.Properties[\"{1}\"];", str5, memberName));
                    if (flag5)
                    {
                        CodeWriter writer8 = writer6.AddChild(string.Format("if(instNET.{0} != null)", info.Name));
                        writer6.AddChild("else").Line(string.Format("{0}.Value = null;", str5));
                        if (isArray)
                        {
                            writer8.Line(string.Format("int len = instNET.{0}.Length;", info.Name));
                            writer8.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
                            writer8.Line("IWmiConverter[] embeddedConverters = new IWmiConverter[len];");
                            CodeWriter writer10 = writer8.AddChild("for(int i=0;i<len;i++)");
                            CodeWriter writer11 = writer10.AddChild(string.Format("if((instNET.{0}[i] != null) && mapTypeToConverter.Contains(instNET.{0}[i].GetType()))", info.Name));
                            writer11.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}[i].GetType()];", info.Name));
                            writer11.Line("embeddedConverters[i] = (IWmiConverter)Activator.CreateInstance(type);");
                            writer11.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", info.Name));
                            writer11.Line("embeddedObjects[i] = embeddedConverters[i].GetInstance();");
                            writer10.AddChild("else").Line(string.Format("embeddedObjects[i] = SafeAssign.GetManagementObject(instNET.{0}[i]);", info.Name));
                            writer8.Line(string.Format("{0}.Value = embeddedObjects;", str5));
                        }
                        else
                        {
                            CodeWriter writer12 = writer8.AddChild(string.Format("if(mapTypeToConverter.Contains(instNET.{0}.GetType()))", info.Name));
                            writer12.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}.GetType()];", info.Name));
                            writer12.Line("IWmiConverter converter = (IWmiConverter)Activator.CreateInstance(type);");
                            writer12.Line(string.Format("converter.ToWMI(instNET.{0});", info.Name));
                            writer12.Line(string.Format("{0}.Value = converter.GetInstance();", str5));
                            writer8.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.GetInstance(instNET.{1});", str5, info.Name));
                        }
                    }
                    else if (str3 != null)
                    {
                        CodeWriter writer13;
                        if (fieldType.IsValueType)
                        {
                            writer13 = writer6;
                        }
                        else
                        {
                            writer13 = writer6.AddChild(string.Format("if(instNET.{0} != null)", info.Name));
                            writer6.AddChild("else").Line(string.Format("{0}.Value = null;", str5));
                        }
                        if (isArray)
                        {
                            writer13.Line(string.Format("int len = instNET.{0}.Length;", info.Name));
                            writer13.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
                            writer13.Line(string.Format("{0}[] embeddedConverters = new {0}[len];", str4));
                            CodeWriter writer15 = writer13.AddChild("for(int i=0;i<len;i++)");
                            writer15.Line(string.Format("embeddedConverters[i] = new {0}();", str4));
                            if (fieldType.IsValueType)
                            {
                                writer15.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", info.Name));
                            }
                            else
                            {
                                writer15.AddChild(string.Format("if(instNET.{0}[i] != null)", info.Name)).Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", info.Name));
                            }
                            writer15.Line("embeddedObjects[i] = embeddedConverters[i].instance;");
                            writer13.Line(string.Format("{0}.Value = embeddedObjects;", str5));
                        }
                        else
                        {
                            writer2.Line(string.Format("{0} lazy_embeddedConverter_{1} = null;", str4, str5));
                            CodeWriter writer18 = writer.AddChild(string.Format("{0} embeddedConverter_{1}", str4, str5)).AddChild("get");
                            writer18.AddChild(string.Format("if(null == lazy_embeddedConverter_{0})", str5)).Line(string.Format("lazy_embeddedConverter_{0} = new {1}();", str5, str4));
                            writer18.Line(string.Format("return lazy_embeddedConverter_{0};", str5));
                            writer13.Line(string.Format("embeddedConverter_{0}.ToWMI(instNET.{1});", str5, info.Name));
                            writer13.Line(string.Format("{0}.Value = embeddedConverter_{0}.instance;", str5));
                        }
                    }
                    else if (!isArray)
                    {
                        if ((fieldType == typeof(byte)) || (fieldType == typeof(sbyte)))
                        {
                            writer6.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", fieldType, info.Name));
                            writer6.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 1, ref instNET_{1});", str6, info.Name));
                        }
                        else if (((fieldType == typeof(short)) || (fieldType == typeof(ushort))) || (fieldType == typeof(char)))
                        {
                            writer6.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", fieldType, info.Name));
                            writer6.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref instNET_{1});", str6, info.Name));
                        }
                        else if (((fieldType == typeof(uint)) || (fieldType == typeof(int))) || (fieldType == typeof(float)))
                        {
                            writer6.Line(string.Format("IWOA.WriteDWORD_f31(31, instWbemObjectAccessIP, {0}, instNET.{1});", str6, info.Name));
                        }
                        else if (((fieldType == typeof(ulong)) || (fieldType == typeof(long))) || (fieldType == typeof(double)))
                        {
                            writer6.Line(string.Format("IWOA.WriteQWORD_f33(33, instWbemObjectAccessIP, {0}, instNET.{1});", str6, info.Name));
                        }
                        else if (fieldType == typeof(bool))
                        {
                            writer6.Line(string.Format("if(instNET.{0})", info.Name));
                            writer6.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolTrue);", str6));
                            writer6.Line("else");
                            writer6.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolFalse);", str6));
                        }
                        else if (fieldType == typeof(string))
                        {
                            writer6.AddChild(string.Format("if(null != instNET.{0})", info.Name)).Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, (instNET.{1}.Length+1)*2, instNET.{1});", str6, info.Name));
                            writer6.AddChild("else").Line(string.Format("IWOA.Put_f5(5, instWbemObjectAccessIP, \"{0}\", 0, ref nullObj, 8);", memberName));
                            if (!flag3)
                            {
                                flag3 = true;
                                writer2.Line("object nullObj = DBNull.Value;");
                            }
                        }
                        else if ((fieldType == typeof(DateTime)) || (fieldType == typeof(TimeSpan)))
                        {
                            writer6.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 52, SafeAssign.WMITimeToString(instNET.{1}));", str6, info.Name));
                        }
                        else
                        {
                            writer6.Line(string.Format("{0}.Value = instNET.{1};", str5, info.Name));
                        }
                    }
                    else if ((fieldType == typeof(DateTime)) || (fieldType == typeof(TimeSpan)))
                    {
                        writer6.AddChild(string.Format("if(null == instNET.{0})", info.Name)).Line(string.Format("{0}.Value = null;", str5));
                        writer6.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.WMITimeArrayToStringArray(instNET.{1});", str5, info.Name));
                    }
                    else
                    {
                        writer6.Line(string.Format("{0}.Value = instNET.{1};", str5, info.Name));
                    }
                    CimType propertyType = CimType.String;
                    if (info.DeclaringType == type)
                    {
                        bool flag6 = true;
                        try
                        {
                            PropertyData data2 = this.newClass.Properties[memberName];
                            CimType type1 = data2.Type;
                            if (data2.IsLocal)
                            {
                                throw new ArgumentException(string.Format(RC.GetString("MEMBERCONFLILCT_EXCEPT"), info.Name), info.Name);
                            }
                        }
                        catch (ManagementException exception2)
                        {
                            if (exception2.ErrorCode != ManagementStatus.NotFound)
                            {
                                throw;
                            }
                            flag6 = false;
                        }
                        if (!flag6)
                        {
                            if (str3 != null)
                            {
                                propertyType = CimType.Object;
                            }
                            else if (flag5)
                            {
                                propertyType = CimType.Object;
                            }
                            else if (fieldType == typeof(ManagementObject))
                            {
                                propertyType = CimType.Object;
                            }
                            else if (fieldType == typeof(sbyte))
                            {
                                propertyType = CimType.SInt8;
                            }
                            else if (fieldType == typeof(byte))
                            {
                                propertyType = CimType.UInt8;
                            }
                            else if (fieldType == typeof(short))
                            {
                                propertyType = CimType.SInt16;
                            }
                            else if (fieldType == typeof(ushort))
                            {
                                propertyType = CimType.UInt16;
                            }
                            else if (fieldType == typeof(int))
                            {
                                propertyType = CimType.SInt32;
                            }
                            else if (fieldType == typeof(uint))
                            {
                                propertyType = CimType.UInt32;
                            }
                            else if (fieldType == typeof(long))
                            {
                                propertyType = CimType.SInt64;
                            }
                            else if (fieldType == typeof(ulong))
                            {
                                propertyType = CimType.UInt64;
                            }
                            else if (fieldType == typeof(float))
                            {
                                propertyType = CimType.Real32;
                            }
                            else if (fieldType == typeof(double))
                            {
                                propertyType = CimType.Real64;
                            }
                            else if (fieldType == typeof(bool))
                            {
                                propertyType = CimType.Boolean;
                            }
                            else if (fieldType == typeof(string))
                            {
                                propertyType = CimType.String;
                            }
                            else if (fieldType == typeof(char))
                            {
                                propertyType = CimType.Char16;
                            }
                            else if (fieldType == typeof(DateTime))
                            {
                                propertyType = CimType.DateTime;
                            }
                            else if (fieldType == typeof(TimeSpan))
                            {
                                propertyType = CimType.DateTime;
                            }
                            else
                            {
                                ThrowUnsupportedMember(info);
                            }
                            try
                            {
                                properties.Add(memberName, propertyType, isArray);
                            }
                            catch (ManagementException exception3)
                            {
                                ThrowUnsupportedMember(info, exception3);
                            }
                            if (fieldType == typeof(TimeSpan))
                            {
                                PropertyData data3 = properties[memberName];
                                data3.Qualifiers.Add("SubType", "interval", false, true, true, true);
                            }
                            if (str3 != null)
                            {
                                PropertyData data4 = properties[memberName];
                                data4.Qualifiers["CIMTYPE"].Value = "object:" + str3;
                            }
                        }
                    }
                }
            }
            writer3.Line("Marshal.Release(wbemObjectAccessIP);");
        }

        public static void ThrowUnsupportedMember(MemberInfo mi)
        {
            ThrowUnsupportedMember(mi, null);
        }

        public static void ThrowUnsupportedMember(MemberInfo mi, Exception innerException)
        {
            throw new ArgumentException(string.Format(RC.GetString("UNSUPPORTEDMEMBER_EXCEPT"), mi.Name), mi.Name, innerException);
        }

        public string ClassName
        {
            get
            {
                return this.className;
            }
        }

        public string ClassPath
        {
            get
            {
                return this.classPath;
            }
        }

        public Type ClassType
        {
            get
            {
                return this.classType;
            }
        }

        public CodeWriter Code
        {
            get
            {
                return this.code;
            }
        }

        public string CodeClassName
        {
            get
            {
                return this.codeClassName;
            }
        }

        public System.Management.Instrumentation.InstrumentationType InstrumentationType
        {
            get
            {
                return this.instrumentationType;
            }
        }

        public ManagementClass NewClass
        {
            get
            {
                return this.newClass;
            }
        }
    }
}

