namespace System.Xml
{
    using System;
    using System.Runtime;

    internal abstract class ArrayHelper<TArgument, TArray>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ArrayHelper()
        {
        }

        public TArray[] ReadArray(XmlDictionaryReader reader, TArgument localName, TArgument namespaceUri, int maxArrayLength)
        {
            TArray[][] localArray = null;
            TArray[] array = null;
            int num3;
            int num = 0;
            int num2 = 0;
            if (reader.TryGetArrayLength(out num3))
            {
                if (num3 > maxArrayLength)
                {
                    XmlExceptionHelper.ThrowMaxArrayLengthOrMaxItemsQuotaExceeded(reader, maxArrayLength);
                }
                if (num3 > 0xffff)
                {
                    num3 = 0xffff;
                }
            }
            else
            {
                num3 = 0x20;
            }
        Label_0036:
            array = new TArray[num3];
            int offset = 0;
            while (offset < array.Length)
            {
                int num5 = this.ReadArray(reader, localName, namespaceUri, array, offset, array.Length - offset);
                if (num5 == 0)
                {
                    break;
                }
                offset += num5;
            }
            if (num2 > (maxArrayLength - offset))
            {
                XmlExceptionHelper.ThrowMaxArrayLengthOrMaxItemsQuotaExceeded(reader, maxArrayLength);
            }
            num2 += offset;
            if ((offset >= array.Length) && (reader.NodeType != XmlNodeType.EndElement))
            {
                if (localArray == null)
                {
                    localArray = new TArray[0x20][];
                }
                localArray[num++] = array;
                num3 *= 2;
                goto Label_0036;
            }
            if ((num2 == array.Length) && (num <= 0))
            {
                return array;
            }
            TArray[] destinationArray = new TArray[num2];
            int destinationIndex = 0;
            for (int i = 0; i < num; i++)
            {
                Array.Copy(localArray[i], 0, destinationArray, destinationIndex, localArray[i].Length);
                destinationIndex += localArray[i].Length;
            }
            Array.Copy(array, 0, destinationArray, destinationIndex, num2 - destinationIndex);
            return destinationArray;
        }

        protected abstract int ReadArray(XmlDictionaryReader reader, TArgument localName, TArgument namespaceUri, TArray[] array, int offset, int count);
        public void WriteArray(XmlDictionaryWriter writer, string prefix, TArgument localName, TArgument namespaceUri, XmlDictionaryReader reader)
        {
            int num;
            if (reader.TryGetArrayLength(out num))
            {
                num = Math.Min(num, 0x100);
            }
            else
            {
                num = 0x100;
            }
            TArray[] array = new TArray[num];
            while (true)
            {
                int count = this.ReadArray(reader, localName, namespaceUri, array, 0, array.Length);
                if (count == 0)
                {
                    return;
                }
                this.WriteArray(writer, prefix, localName, namespaceUri, array, 0, count);
            }
        }

        protected abstract void WriteArray(XmlDictionaryWriter writer, string prefix, TArgument localName, TArgument namespaceUri, TArray[] array, int offset, int count);
    }
}

