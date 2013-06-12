namespace System.Runtime.Remoting.Activation
{
    using System;

    internal class ActivationAttributeStack
    {
        private object[] activationAttributes = new object[4];
        private object[] activationTypes = new object[4];
        private int freeIndex = 0;

        internal ActivationAttributeStack()
        {
        }

        internal object[] Peek(Type typ)
        {
            if ((this.freeIndex != 0) && (this.activationTypes[this.freeIndex - 1] == typ))
            {
                return (object[]) this.activationAttributes[this.freeIndex - 1];
            }
            return null;
        }

        internal void Pop(Type typ)
        {
            if ((this.freeIndex != 0) && (this.activationTypes[this.freeIndex - 1] == typ))
            {
                this.freeIndex--;
                this.activationTypes[this.freeIndex] = null;
                this.activationAttributes[this.freeIndex] = null;
            }
        }

        internal void Push(Type typ, object[] attr)
        {
            if (this.freeIndex == this.activationTypes.Length)
            {
                object[] destinationArray = new object[this.activationTypes.Length * 2];
                object[] objArray2 = new object[this.activationAttributes.Length * 2];
                Array.Copy(this.activationTypes, destinationArray, this.activationTypes.Length);
                Array.Copy(this.activationAttributes, objArray2, this.activationAttributes.Length);
                this.activationTypes = destinationArray;
                this.activationAttributes = objArray2;
            }
            this.activationTypes[this.freeIndex] = typ;
            this.activationAttributes[this.freeIndex] = attr;
            this.freeIndex++;
        }
    }
}

