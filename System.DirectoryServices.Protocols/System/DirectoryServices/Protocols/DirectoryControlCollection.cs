namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DirectoryControlCollection : CollectionBase
    {
        public DirectoryControlCollection()
        {
            Utility.CheckOSVersion();
        }

        public int Add(DirectoryControl control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            return base.List.Add(control);
        }

        public void AddRange(DirectoryControl[] controls)
        {
            if (controls == null)
            {
                throw new ArgumentNullException("controls");
            }
            DirectoryControl[] controlArray = controls;
            for (int i = 0; i < controlArray.Length; i++)
            {
                if (controlArray[i] == null)
                {
                    throw new ArgumentException(Res.GetString("ContainNullControl"), "controls");
                }
            }
            base.InnerList.AddRange(controls);
        }

        public void AddRange(DirectoryControlCollection controlCollection)
        {
            if (controlCollection == null)
            {
                throw new ArgumentNullException("controlCollection");
            }
            int count = controlCollection.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(controlCollection[i]);
            }
        }

        public bool Contains(DirectoryControl value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(DirectoryControl[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(DirectoryControl value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, DirectoryControl value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            base.List.Insert(index, value);
        }

        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is DirectoryControl))
            {
                throw new ArgumentException(Res.GetString("InvalidValueType", new object[] { "DirectoryControl" }), "value");
            }
        }

        public void Remove(DirectoryControl value)
        {
            base.List.Remove(value);
        }

        public DirectoryControl this[int index]
        {
            get
            {
                return (DirectoryControl) base.List[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base.List[index] = value;
            }
        }
    }
}

