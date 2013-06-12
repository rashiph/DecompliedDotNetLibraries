namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    [ListBindable(false)]
    public class NumericUpDownAccelerationCollection : MarshalByRefObject, ICollection<NumericUpDownAcceleration>, IEnumerable<NumericUpDownAcceleration>, IEnumerable
    {
        private List<NumericUpDownAcceleration> items = new List<NumericUpDownAcceleration>();

        public void Add(NumericUpDownAcceleration acceleration)
        {
            if (acceleration == null)
            {
                throw new ArgumentNullException("acceleration");
            }
            int index = 0;
            while (index < this.items.Count)
            {
                if (acceleration.Seconds < this.items[index].Seconds)
                {
                    break;
                }
                index++;
            }
            this.items.Insert(index, acceleration);
        }

        public void AddRange(params NumericUpDownAcceleration[] accelerations)
        {
            if (accelerations == null)
            {
                throw new ArgumentNullException("accelerations");
            }
            NumericUpDownAcceleration[] accelerationArray = accelerations;
            for (int i = 0; i < accelerationArray.Length; i++)
            {
                if (accelerationArray[i] == null)
                {
                    throw new ArgumentNullException(System.Windows.Forms.SR.GetString("NumericUpDownAccelerationCollectionAtLeastOneEntryIsNull"));
                }
            }
            foreach (NumericUpDownAcceleration acceleration2 in accelerations)
            {
                this.Add(acceleration2);
            }
        }

        public void Clear()
        {
            this.items.Clear();
        }

        public bool Contains(NumericUpDownAcceleration acceleration)
        {
            return this.items.Contains(acceleration);
        }

        public void CopyTo(NumericUpDownAcceleration[] array, int index)
        {
            this.items.CopyTo(array, index);
        }

        public bool Remove(NumericUpDownAcceleration acceleration)
        {
            return this.items.Remove(acceleration);
        }

        IEnumerator<NumericUpDownAcceleration> IEnumerable<NumericUpDownAcceleration>.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public NumericUpDownAcceleration this[int index]
        {
            get
            {
                return this.items[index];
            }
        }
    }
}

