namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Windows.Forms;

    [SuppressUnmanagedCodeSecurity]
    internal class Com2IManagedPerPropertyBrowsingHandler : Com2ExtendedBrowsingHandler
    {
        internal static Attribute[] GetComponentAttributes(System.Windows.Forms.NativeMethods.IManagedPerPropertyBrowsing target, int dispid)
        {
            int pcAttributes = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr pvariantInitValues = IntPtr.Zero;
            if ((target.GetPropertyAttributes(dispid, ref pcAttributes, ref zero, ref pvariantInitValues) != 0) || (pcAttributes == 0))
            {
                return new Attribute[0];
            }
            ArrayList list = new ArrayList();
            string[] stringsFromPtr = GetStringsFromPtr(zero, pcAttributes);
            object[] variantsFromPtr = GetVariantsFromPtr(pvariantInitValues, pcAttributes);
            if (stringsFromPtr.Length != variantsFromPtr.Length)
            {
                return new Attribute[0];
            }
            for (int i = 0; i < stringsFromPtr.Length; i++)
            {
                string typeName = stringsFromPtr[i];
                System.Type c = System.Type.GetType(typeName);
                Assembly assembly = null;
                if (c != null)
                {
                    assembly = c.Assembly;
                }
                if (c == null)
                {
                    string str2 = "";
                    int startIndex = typeName.LastIndexOf(',');
                    if (startIndex != -1)
                    {
                        str2 = typeName.Substring(startIndex);
                        typeName = typeName.Substring(0, startIndex);
                    }
                    int length = typeName.LastIndexOf('.');
                    if (length == -1)
                    {
                        continue;
                    }
                    string name = typeName.Substring(length + 1);
                    if (assembly == null)
                    {
                        c = System.Type.GetType(typeName.Substring(0, length) + str2);
                    }
                    else
                    {
                        c = assembly.GetType(typeName.Substring(0, length) + str2);
                    }
                    if ((c == null) || !typeof(Attribute).IsAssignableFrom(c))
                    {
                        continue;
                    }
                    if (c != null)
                    {
                        FieldInfo field = c.GetField(name);
                        if ((field != null) && field.IsStatic)
                        {
                            object obj2 = field.GetValue(null);
                            if (obj2 is Attribute)
                            {
                                list.Add(obj2);
                                continue;
                            }
                        }
                    }
                }
                if (typeof(Attribute).IsAssignableFrom(c))
                {
                    Attribute attribute = null;
                    if (!Convert.IsDBNull(variantsFromPtr[i]) && (variantsFromPtr[i] != null))
                    {
                        ConstructorInfo[] constructors = c.GetConstructors();
                        for (int j = 0; j < constructors.Length; j++)
                        {
                            ParameterInfo[] parameters = constructors[j].GetParameters();
                            if ((parameters.Length == 1) && parameters[0].ParameterType.IsAssignableFrom(variantsFromPtr[i].GetType()))
                            {
                                try
                                {
                                    attribute = (Attribute) Activator.CreateInstance(c, new object[] { variantsFromPtr[i] });
                                    list.Add(attribute);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            attribute = (Attribute) Activator.CreateInstance(c);
                            list.Add(attribute);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            Attribute[] array = new Attribute[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private static string[] GetStringsFromPtr(IntPtr ptr, int cStrings)
        {
            if (!(ptr != IntPtr.Zero))
            {
                return new string[0];
            }
            string[] strArray = new string[cStrings];
            for (int i = 0; i < cStrings; i++)
            {
                try
                {
                    IntPtr ptr2 = Marshal.ReadIntPtr(ptr, i * 4);
                    if (ptr2 != IntPtr.Zero)
                    {
                        strArray[i] = Marshal.PtrToStringUni(ptr2);
                        SafeNativeMethods.SysFreeString(new HandleRef(null, ptr2));
                    }
                    else
                    {
                        strArray[i] = "";
                    }
                }
                catch (Exception)
                {
                }
            }
            try
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            catch (Exception)
            {
            }
            return strArray;
        }

        private static object[] GetVariantsFromPtr(IntPtr ptr, int cVariants)
        {
            if (!(ptr != IntPtr.Zero))
            {
                return new object[cVariants];
            }
            object[] objArray = new object[cVariants];
            for (int i = 0; i < cVariants; i++)
            {
                try
                {
                    IntPtr pSrcNativeVariant = (IntPtr) (((long) ptr) + (i * 0x10));
                    if (pSrcNativeVariant != IntPtr.Zero)
                    {
                        objArray[i] = Marshal.GetObjectForNativeVariant(pSrcNativeVariant);
                        SafeNativeMethods.VariantClear(new HandleRef(null, pSrcNativeVariant));
                    }
                    else
                    {
                        objArray[i] = Convert.DBNull;
                    }
                }
                catch (Exception)
                {
                }
            }
            try
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            catch (Exception)
            {
            }
            return objArray;
        }

        private void OnGetAttributes(Com2PropertyDescriptor sender, GetAttributesEvent attrEvent)
        {
            object targetObject = sender.TargetObject;
            if (targetObject is System.Windows.Forms.NativeMethods.IManagedPerPropertyBrowsing)
            {
                Attribute[] componentAttributes = GetComponentAttributes((System.Windows.Forms.NativeMethods.IManagedPerPropertyBrowsing) targetObject, sender.DISPID);
                if (componentAttributes != null)
                {
                    for (int i = 0; i < componentAttributes.Length; i++)
                    {
                        attrEvent.Add(componentAttributes[i]);
                    }
                }
            }
        }

        public override void SetupPropertyHandlers(Com2PropertyDescriptor[] propDesc)
        {
            if (propDesc != null)
            {
                for (int i = 0; i < propDesc.Length; i++)
                {
                    propDesc[i].QueryGetDynamicAttributes += new GetAttributesEventHandler(this.OnGetAttributes);
                }
            }
        }

        public override System.Type Interface
        {
            get
            {
                return typeof(System.Windows.Forms.NativeMethods.IManagedPerPropertyBrowsing);
            }
        }
    }
}

