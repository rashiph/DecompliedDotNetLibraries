namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class IntSizedArray : ICloneable
    {
        internal int[] negObjects;
        internal int[] objects;

        public IntSizedArray()
        {
            this.objects = new int[0x10];
            this.negObjects = new int[4];
        }

        private IntSizedArray(IntSizedArray sizedArray)
        {
            this.objects = new int[0x10];
            this.negObjects = new int[4];
            this.objects = new int[sizedArray.objects.Length];
            sizedArray.objects.CopyTo(this.objects, 0);
            this.negObjects = new int[sizedArray.negObjects.Length];
            sizedArray.negObjects.CopyTo(this.negObjects, 0);
        }

        public object Clone()
        {
            return new IntSizedArray(this);
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    int[] destinationArray = new int[Math.Max((int) (this.negObjects.Length * 2), (int) (-index + 1))];
                    Array.Copy(this.negObjects, 0, destinationArray, 0, this.negObjects.Length);
                    this.negObjects = destinationArray;
                }
                else
                {
                    int[] numArray2 = new int[Math.Max((int) (this.objects.Length * 2), (int) (index + 1))];
                    Array.Copy(this.objects, 0, numArray2, 0, this.objects.Length);
                    this.objects = numArray2;
                }
            }
            catch (Exception)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_CorruptedStream"));
            }
        }

        internal int this[int index]
        {
            get
            {
                if (index < 0)
                {
                    if (-index > (this.negObjects.Length - 1))
                    {
                        return 0;
                    }
                    return this.negObjects[-index];
                }
                if (index > (this.objects.Length - 1))
                {
                    return 0;
                }
                return this.objects[index];
            }
            set
            {
                if (index < 0)
                {
                    if (-index > (this.negObjects.Length - 1))
                    {
                        this.IncreaseCapacity(index);
                    }
                    this.negObjects[-index] = value;
                }
                else
                {
                    if (index > (this.objects.Length - 1))
                    {
                        this.IncreaseCapacity(index);
                    }
                    this.objects[index] = value;
                }
            }
        }
    }
}

