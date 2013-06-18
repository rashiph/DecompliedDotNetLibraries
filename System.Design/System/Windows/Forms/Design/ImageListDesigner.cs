namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Forms;

    internal class ImageListDesigner : ComponentDesigner
    {
        private DesignerActionListCollection _actionLists;
        private OriginalImageCollection originalImageCollection;

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "ColorDepth", "ImageSize", "ImageStream", "TransparentColor" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(ImageListDesigner), oldPropertyDescriptor, attributes);
                }
            }
            PropertyDescriptor descriptor2 = (PropertyDescriptor) properties["Images"];
            if (descriptor2 != null)
            {
                Attribute[] array = new Attribute[descriptor2.Attributes.Count];
                descriptor2.Attributes.CopyTo(array, 0);
                properties["Images"] = TypeDescriptor.CreateProperty(typeof(ImageListDesigner), "Images", typeof(OriginalImageCollection), array);
            }
        }

        private bool ShouldSerializeColorDepth()
        {
            return (this.Images.Count == 0);
        }

        private bool ShouldSerializeImageSize()
        {
            return (this.Images.Count == 0);
        }

        private bool ShouldSerializeTransparentColor()
        {
            return !this.TransparentColor.Equals(Color.LightGray);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new ImageListActionList(this));
                }
                return this._actionLists;
            }
        }

        private System.Windows.Forms.ColorDepth ColorDepth
        {
            get
            {
                return this.ImageList.ColorDepth;
            }
            set
            {
                this.ImageList.Images.Clear();
                this.ImageList.ColorDepth = value;
                this.Images.PopulateHandle();
            }
        }

        internal System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return (System.Windows.Forms.ImageList) base.Component;
            }
        }

        private OriginalImageCollection Images
        {
            get
            {
                if (this.originalImageCollection == null)
                {
                    this.originalImageCollection = new OriginalImageCollection(this);
                }
                return this.originalImageCollection;
            }
        }

        private Size ImageSize
        {
            get
            {
                return this.ImageList.ImageSize;
            }
            set
            {
                this.ImageList.Images.Clear();
                this.ImageList.ImageSize = value;
                this.Images.PopulateHandle();
            }
        }

        private ImageListStreamer ImageStream
        {
            get
            {
                return this.ImageList.ImageStream;
            }
            set
            {
                this.ImageList.ImageStream = value;
                this.Images.ReloadFromImageList();
            }
        }

        private Color TransparentColor
        {
            get
            {
                return this.ImageList.TransparentColor;
            }
            set
            {
                this.ImageList.Images.Clear();
                this.ImageList.TransparentColor = value;
                this.Images.PopulateHandle();
            }
        }

        [Editor("System.Windows.Forms.Design.ImageCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        internal class OriginalImageCollection : IList, ICollection, IEnumerable
        {
            private IList list = new ArrayList();
            private ImageListDesigner owner;

            internal OriginalImageCollection(ImageListDesigner owner)
            {
                this.owner = owner;
                this.ReloadFromImageList();
            }

            public int Add(ImageListImage value)
            {
                int num = this.list.Add(value);
                if (value.Name != null)
                {
                    this.owner.ImageList.Images.Add(value.Name, value.Image);
                    return num;
                }
                this.owner.ImageList.Images.Add(value.Image);
                return num;
            }

            public void AddRange(ImageListImage[] values)
            {
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                foreach (ImageListImage image in values)
                {
                    if (image != null)
                    {
                        this.Add(image);
                    }
                }
            }

            private void AssertInvariant()
            {
            }

            public void Clear()
            {
                this.AssertInvariant();
                this.list.Clear();
                this.owner.ImageList.Images.Clear();
            }

            public bool Contains(ImageListImage value)
            {
                return this.list.Contains(value.Image);
            }

            public IEnumerator GetEnumerator()
            {
                return this.list.GetEnumerator();
            }

            public int IndexOf(Image value)
            {
                return this.list.IndexOf(value);
            }

            internal void PopulateHandle()
            {
                for (int i = 0; i < this.list.Count; i++)
                {
                    ImageListImage image = (ImageListImage) this.list[i];
                    this.owner.ImageList.Images.Add(image.Name, image.Image);
                }
            }

            private void RecreateHandle()
            {
                this.owner.ImageList.Images.Clear();
                this.PopulateHandle();
            }

            internal void ReloadFromImageList()
            {
                this.list.Clear();
                StringCollection keys = this.owner.ImageList.Images.Keys;
                for (int i = 0; i < this.owner.ImageList.Images.Count; i++)
                {
                    this.list.Add(new ImageListImage((Bitmap) this.owner.ImageList.Images[i], keys[i]));
                }
            }

            public void Remove(Image value)
            {
                this.AssertInvariant();
                this.list.Remove(value);
                this.RecreateHandle();
            }

            public void RemoveAt(int index)
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentOutOfRangeException(System.Design.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.AssertInvariant();
                this.list.RemoveAt(index);
                this.RecreateHandle();
            }

            public void SetKeyName(int index, string name)
            {
                this[index].Name = name;
                this.owner.ImageList.Images.SetKeyName(index, name);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                this.list.CopyTo(array, index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            int IList.Add(object value)
            {
                if (!(value is ImageListImage))
                {
                    throw new ArgumentException(System.Design.SR.GetString("ImageListDesignerBadImageListImage", new object[] { "value" }));
                }
                return this.Add((ImageListImage) value);
            }

            bool IList.Contains(object value)
            {
                return ((value is ImageListImage) && this.Contains((ImageListImage) value));
            }

            int IList.IndexOf(object value)
            {
                if (value is Image)
                {
                    return this.IndexOf((Image) value);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object value)
            {
                if (value is Image)
                {
                    this.Remove((Image) value);
                }
            }

            public int Count
            {
                get
                {
                    this.AssertInvariant();
                    return this.list.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public ImageListImage this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException(System.Design.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return (ImageListImage) this.list[index];
                }
                set
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException(System.Design.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (value == null)
                    {
                        throw new ArgumentException(System.Design.SR.GetString("InvalidArgument", new object[] { "value", "null" }));
                    }
                    this.AssertInvariant();
                    this.list[index] = value;
                    this.RecreateHandle();
                }
            }

            int ICollection.Count
            {
                get
                {
                    return this.Count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return null;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (!(value is ImageListImage))
                    {
                        throw new ArgumentException(System.Design.SR.GetString("ImageListDesignerBadImageListImage", new object[] { "value" }));
                    }
                    this[index] = (ImageListImage) value;
                }
            }
        }
    }
}

