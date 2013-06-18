namespace System.Drawing.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [Serializable]
    public class ToolboxItemContainer : ISerializable
    {
        private const short _clipboardVersion = 1;
        private IDataObject _dataObject;
        private ICollection _filter;
        private const string _hashClipboardFormat = "CF_TOOLBOXITEMCONTAINER_HASH";
        private int _hashCode;
        private const string _itemClipboardFormat = "CF_TOOLBOXITEMCONTAINER_CONTENTS";
        private const string _localClipboardFormat = "CF_TOOLBOXITEMCONTAINER";
        private const string _serializationFormats = "TbxIC_DataObjectFormats";
        private const string _serializationValues = "TbxIC_DataObjectValues";
        private ToolboxItem _toolboxItem;

        public ToolboxItemContainer(ToolboxItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this._toolboxItem = item;
            this.UpdateFilter(item);
            this._hashCode = item.DisplayName.GetHashCode();
            if (item.AssemblyName != null)
            {
                this._hashCode ^= item.AssemblyName.GetHashCode();
            }
            if (item.TypeName != null)
            {
                this._hashCode ^= item.TypeName.GetHashCode();
            }
            if (this._hashCode == 0)
            {
                this._hashCode = item.DisplayName.GetHashCode();
            }
        }

        public ToolboxItemContainer(IDataObject data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this._dataObject = data;
        }

        protected ToolboxItemContainer(SerializationInfo info, StreamingContext context)
        {
            string[] strArray = (string[]) info.GetValue("TbxIC_DataObjectFormats", typeof(string[]));
            object[] objArray = (object[]) info.GetValue("TbxIC_DataObjectValues", typeof(object[]));
            DataObject obj2 = new DataObject();
            for (int i = 0; i < strArray.Length; i++)
            {
                obj2.SetData(strArray[i], objArray[i]);
            }
            this._dataObject = obj2;
        }

        internal static bool ContainsFormat(IDataObject dataObject)
        {
            return dataObject.GetDataPresent("CF_TOOLBOXITEMCONTAINER");
        }

        public override bool Equals(object obj)
        {
            ToolboxItemContainer container = obj as ToolboxItemContainer;
            if (container == this)
            {
                return true;
            }
            if (container == null)
            {
                return false;
            }
            if (((this._toolboxItem != null) && (container._toolboxItem != null)) && this._toolboxItem.Equals(container._toolboxItem))
            {
                return true;
            }
            if (((this._dataObject != null) && (container._dataObject != null)) && this._dataObject.Equals(container._dataObject))
            {
                return true;
            }
            ToolboxItem toolboxItem = this.GetToolboxItem(null);
            ToolboxItem item2 = container.GetToolboxItem(null);
            return (((toolboxItem != null) && (item2 != null)) && toolboxItem.Equals(item2));
        }

        public virtual ICollection GetFilter(ICollection creators)
        {
            ICollection is2 = this._filter;
            if (this._filter == null)
            {
                if (this._dataObject.GetDataPresent("CF_TOOLBOXITEMCONTAINER"))
                {
                    byte[] data = (byte[]) this._dataObject.GetData("CF_TOOLBOXITEMCONTAINER");
                    if (data != null)
                    {
                        BinaryReader reader = new BinaryReader(new MemoryStream(data));
                        if (reader.ReadInt16() != 1)
                        {
                            this._filter = new ToolboxItemFilterAttribute[0];
                        }
                        else
                        {
                            short num2 = reader.ReadInt16();
                            ToolboxItemFilterAttribute[] attributeArray = new ToolboxItemFilterAttribute[num2];
                            for (short i = 0; i < num2; i = (short) (i + 1))
                            {
                                string filterString = reader.ReadString();
                                short num4 = reader.ReadInt16();
                                attributeArray[i] = new ToolboxItemFilterAttribute(filterString, (ToolboxItemFilterType) num4);
                            }
                            this._filter = attributeArray;
                        }
                    }
                    else
                    {
                        this._filter = new ToolboxItemFilterAttribute[0];
                    }
                    return this._filter;
                }
                if (creators != null)
                {
                    foreach (ToolboxItemCreator creator in creators)
                    {
                        if (this._dataObject.GetDataPresent(creator.Format))
                        {
                            ToolboxItem item = creator.Create(this._dataObject);
                            if (item != null)
                            {
                                return MergeFilter(item);
                            }
                        }
                    }
                }
            }
            return is2;
        }

        public override int GetHashCode()
        {
            if (((this._hashCode == 0) && (this._dataObject != null)) && this._dataObject.GetDataPresent("CF_TOOLBOXITEMCONTAINER_HASH"))
            {
                this._hashCode = (int) this._dataObject.GetData("CF_TOOLBOXITEMCONTAINER_HASH");
            }
            if (this._hashCode == 0)
            {
                this._hashCode = base.GetHashCode();
            }
            return this._hashCode;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            IDataObject toolboxData = this.ToolboxData;
            string[] formats = toolboxData.GetFormats();
            object[] objArray = new object[formats.Length];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = toolboxData.GetData(formats[i]);
            }
            info.AddValue("TbxIC_DataObjectFormats", formats);
            info.AddValue("TbxIC_DataObjectValues", objArray);
        }

        public virtual ToolboxItem GetToolboxItem(ICollection creators)
        {
            ToolboxItem item = this._toolboxItem;
            if (this._toolboxItem == null)
            {
                if (this._dataObject.GetDataPresent("CF_TOOLBOXITEMCONTAINER_CONTENTS"))
                {
                    string exceptionString = null;
                    try
                    {
                        ToolboxItemSerializer data = (ToolboxItemSerializer) this._dataObject.GetData("CF_TOOLBOXITEMCONTAINER_CONTENTS");
                        if (data == null)
                        {
                            exceptionString = System.Drawing.Design.SR.GetString("ToolboxServiceToolboxItemSerializerNotFound");
                        }
                        else
                        {
                            this._toolboxItem = data.ToolboxItem;
                        }
                    }
                    catch (Exception exception)
                    {
                        exceptionString = exception.Message;
                    }
                    if (this._toolboxItem == null)
                    {
                        this._toolboxItem = new BrokenToolboxItem(exceptionString);
                    }
                    return this._toolboxItem;
                }
                if (creators != null)
                {
                    foreach (ToolboxItemCreator creator in creators)
                    {
                        if (this._dataObject.GetDataPresent(creator.Format))
                        {
                            item = creator.Create(this._dataObject);
                            if (item != null)
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            return item;
        }

        private static ICollection MergeFilter(ToolboxItem item)
        {
            ICollection filter = item.Filter;
            ArrayList list = new ArrayList();
            foreach (Attribute attribute in TypeDescriptor.GetAttributes(item))
            {
                if (attribute is ToolboxItemFilterAttribute)
                {
                    list.Add(attribute);
                }
            }
            if ((filter == null) || (filter.Count == 0))
            {
                return list;
            }
            if (list.Count > 0)
            {
                Hashtable hashtable = new Hashtable(list.Count + filter.Count);
                foreach (Attribute attribute2 in list)
                {
                    hashtable[attribute2.TypeId] = attribute2;
                }
                foreach (Attribute attribute3 in filter)
                {
                    hashtable[attribute3.TypeId] = attribute3;
                }
                ToolboxItemFilterAttribute[] array = new ToolboxItemFilterAttribute[hashtable.Values.Count];
                hashtable.Values.CopyTo(array, 0);
                return array;
            }
            return filter;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }

        public void UpdateFilter(ToolboxItem item)
        {
            this._filter = MergeFilter(item);
        }

        public bool IsCreated
        {
            get
            {
                return (this._toolboxItem != null);
            }
        }

        public bool IsTransient
        {
            get
            {
                return ((this._toolboxItem != null) && this._toolboxItem.IsTransient);
            }
        }

        public virtual IDataObject ToolboxData
        {
            get
            {
                if (this._dataObject == null)
                {
                    MemoryStream output = new MemoryStream();
                    DataObject obj2 = new DataObject();
                    BinaryWriter writer = new BinaryWriter(output);
                    writer.Write((short) 1);
                    writer.Write((short) this._filter.Count);
                    foreach (ToolboxItemFilterAttribute attribute in this._filter)
                    {
                        writer.Write(attribute.FilterString);
                        writer.Write((short) attribute.FilterType);
                    }
                    writer.Flush();
                    output.Close();
                    obj2.SetData("CF_TOOLBOXITEMCONTAINER", output.GetBuffer());
                    obj2.SetData("CF_TOOLBOXITEMCONTAINER_HASH", this._hashCode);
                    obj2.SetData("CF_TOOLBOXITEMCONTAINER_CONTENTS", new ToolboxItemSerializer(this._toolboxItem));
                    this._dataObject = obj2;
                }
                return this._dataObject;
            }
        }

        private class BrokenToolboxItem : ToolboxItem
        {
            private string _exceptionString;

            public BrokenToolboxItem(string exceptionString) : base(typeof(Component))
            {
                this._exceptionString = exceptionString;
                this.Lock();
            }

            protected override IComponent[] CreateComponentsCore(IDesignerHost host)
            {
                if (this._exceptionString != null)
                {
                    throw new InvalidOperationException(System.Drawing.Design.SR.GetString("ToolboxServiceBadToolboxItemWithException", new object[] { this._exceptionString }));
                }
                throw new InvalidOperationException(System.Drawing.Design.SR.GetString("ToolboxServiceBadToolboxItem"));
            }
        }

        [Serializable]
        private sealed class ToolboxItemSerializer : ISerializable
        {
            private const string _assemblyNameKey = "AssemblyName";
            private static BinaryFormatter _formatter;
            private const string _streamKey = "Stream";
            private System.Drawing.Design.ToolboxItem _toolboxItem;

            internal ToolboxItemSerializer(System.Drawing.Design.ToolboxItem toolboxItem)
            {
                this._toolboxItem = toolboxItem;
            }

            private ToolboxItemSerializer(SerializationInfo info, StreamingContext context)
            {
                AssemblyName name = (AssemblyName) info.GetValue("AssemblyName", typeof(AssemblyName));
                byte[] buffer = (byte[]) info.GetValue("Stream", typeof(byte[]));
                if (_formatter == null)
                {
                    _formatter = new BinaryFormatter();
                }
                SerializationBinder binder = _formatter.Binder;
                _formatter.Binder = new ToolboxItemContainer.ToolboxSerializationBinder(name);
                try
                {
                    this._toolboxItem = (System.Drawing.Design.ToolboxItem) _formatter.Deserialize(new MemoryStream(buffer));
                }
                finally
                {
                    _formatter.Binder = binder;
                }
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (_formatter == null)
                {
                    _formatter = new BinaryFormatter();
                }
                MemoryStream serializationStream = new MemoryStream();
                _formatter.Serialize(serializationStream, this._toolboxItem);
                serializationStream.Close();
                info.AddValue("AssemblyName", this._toolboxItem.GetType().Assembly.GetName());
                info.AddValue("Stream", serializationStream.GetBuffer());
            }

            internal System.Drawing.Design.ToolboxItem ToolboxItem
            {
                get
                {
                    return this._toolboxItem;
                }
            }
        }

        private class ToolboxSerializationBinder : SerializationBinder
        {
            private Hashtable _assemblies = new Hashtable();
            private AssemblyName _name;
            private string _namePart;

            public ToolboxSerializationBinder(AssemblyName name)
            {
                this._name = name;
                this._namePart = name.Name + ",";
            }

            public override System.Type BindToType(string assemblyName, string typeName)
            {
                AssemblyName name;
                Assembly assembly = (Assembly) this._assemblies[assemblyName];
                if (assembly != null)
                {
                    goto Label_00A8;
                }
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch (FileNotFoundException)
                {
                }
                catch (BadImageFormatException)
                {
                }
                catch (IOException)
                {
                }
                if (assembly != null)
                {
                    goto Label_0092;
                }
                if (assemblyName.StartsWith(this._namePart))
                {
                    name = this._name;
                    try
                    {
                        assembly = Assembly.Load(name);
                        goto Label_0067;
                    }
                    catch (FileNotFoundException)
                    {
                        goto Label_0067;
                    }
                    catch (BadImageFormatException)
                    {
                        goto Label_0067;
                    }
                    catch (IOException)
                    {
                        goto Label_0067;
                    }
                }
                name = new AssemblyName(assemblyName);
            Label_0067:
                if (assembly == null)
                {
                    string codeBase = name.CodeBase;
                    if (((codeBase != null) && (codeBase.Length > 0)) && File.Exists(codeBase))
                    {
                        assembly = Assembly.LoadFrom(codeBase);
                    }
                }
            Label_0092:
                if (assembly != null)
                {
                    this._assemblies[assemblyName] = assembly;
                }
            Label_00A8:
                if (assembly != null)
                {
                    return assembly.GetType(typeName);
                }
                return null;
            }
        }
    }
}

