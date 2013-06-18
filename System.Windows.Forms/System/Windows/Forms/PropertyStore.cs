namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal class PropertyStore
    {
        private static int currentKey;
        private IntegerEntry[] intEntries;
        private ObjectEntry[] objEntries;

        public bool ContainsInteger(int key)
        {
            bool flag;
            this.GetInteger(key, out flag);
            return flag;
        }

        public bool ContainsObject(int key)
        {
            bool flag;
            this.GetObject(key, out flag);
            return flag;
        }

        public static int CreateKey()
        {
            return currentKey++;
        }

        [Conditional("DEBUG_PROPERTYSTORE")]
        private void Debug_VerifyLocateIntegerEntry(int index, short entryKey, int length)
        {
            int num = length - 1;
            int num2 = 0;
            int num3 = 0;
            do
            {
                num3 = (num + num2) / 2;
                short key = this.intEntries[num3].Key;
                if (key != entryKey)
                {
                    if (entryKey < key)
                    {
                        num = num3 - 1;
                    }
                    else
                    {
                        num2 = num3 + 1;
                    }
                }
            }
            while (num >= num2);
            if (entryKey > this.intEntries[num3].Key)
            {
                num3++;
            }
        }

        [Conditional("DEBUG_PROPERTYSTORE")]
        private void Debug_VerifyLocateObjectEntry(int index, short entryKey, int length)
        {
            int num = length - 1;
            int num2 = 0;
            int num3 = 0;
            do
            {
                num3 = (num + num2) / 2;
                short key = this.objEntries[num3].Key;
                if (key != entryKey)
                {
                    if (entryKey < key)
                    {
                        num = num3 - 1;
                    }
                    else
                    {
                        num2 = num3 + 1;
                    }
                }
            }
            while (num >= num2);
            if (entryKey > this.objEntries[num3].Key)
            {
                num3++;
            }
        }

        public Color GetColor(int key)
        {
            bool flag;
            return this.GetColor(key, out flag);
        }

        public Color GetColor(int key, out bool found)
        {
            object obj2 = this.GetObject(key, out found);
            if (found)
            {
                ColorWrapper wrapper = obj2 as ColorWrapper;
                if (wrapper != null)
                {
                    return wrapper.Color;
                }
            }
            found = false;
            return Color.Empty;
        }

        public int GetInteger(int key)
        {
            bool flag;
            return this.GetInteger(key, out flag);
        }

        public int GetInteger(int key, out bool found)
        {
            int num2;
            short num3;
            short entryKey = this.SplitKey(key, out num3);
            found = false;
            if (this.LocateIntegerEntry(entryKey, out num2) && (((((int) 1) << num3) & this.intEntries[num2].Mask) != 0))
            {
                found = true;
                switch (num3)
                {
                    case 0:
                        return this.intEntries[num2].Value1;

                    case 1:
                        return this.intEntries[num2].Value2;

                    case 2:
                        return this.intEntries[num2].Value3;

                    case 3:
                        return this.intEntries[num2].Value4;
                }
            }
            return 0;
        }

        public object GetObject(int key)
        {
            bool flag;
            return this.GetObject(key, out flag);
        }

        public object GetObject(int key, out bool found)
        {
            int num;
            short num2;
            short entryKey = this.SplitKey(key, out num2);
            found = false;
            if (this.LocateObjectEntry(entryKey, out num) && (((((int) 1) << num2) & this.objEntries[num].Mask) != 0))
            {
                found = true;
                switch (num2)
                {
                    case 0:
                        return this.objEntries[num].Value1;

                    case 1:
                        return this.objEntries[num].Value2;

                    case 2:
                        return this.objEntries[num].Value3;

                    case 3:
                        return this.objEntries[num].Value4;
                }
            }
            return null;
        }

        public Padding GetPadding(int key)
        {
            bool flag;
            return this.GetPadding(key, out flag);
        }

        public Padding GetPadding(int key, out bool found)
        {
            object obj2 = this.GetObject(key, out found);
            if (found)
            {
                PaddingWrapper wrapper = obj2 as PaddingWrapper;
                if (wrapper != null)
                {
                    return wrapper.Padding;
                }
            }
            found = false;
            return Padding.Empty;
        }

        public Rectangle GetRectangle(int key)
        {
            bool flag;
            return this.GetRectangle(key, out flag);
        }

        public Rectangle GetRectangle(int key, out bool found)
        {
            object obj2 = this.GetObject(key, out found);
            if (found)
            {
                RectangleWrapper wrapper = obj2 as RectangleWrapper;
                if (wrapper != null)
                {
                    return wrapper.Rectangle;
                }
            }
            found = false;
            return Rectangle.Empty;
        }

        public Size GetSize(int key, out bool found)
        {
            object obj2 = this.GetObject(key, out found);
            if (found)
            {
                SizeWrapper wrapper = obj2 as SizeWrapper;
                if (wrapper != null)
                {
                    return wrapper.Size;
                }
            }
            found = false;
            return Size.Empty;
        }

        private bool LocateIntegerEntry(short entryKey, out int index)
        {
            if (this.intEntries != null)
            {
                int length = this.intEntries.Length;
                if (length <= 0x10)
                {
                    index = 0;
                    int num2 = length / 2;
                    if (this.intEntries[num2].Key <= entryKey)
                    {
                        index = num2;
                    }
                    if (this.intEntries[index].Key == entryKey)
                    {
                        return true;
                    }
                    num2 = (length + 1) / 4;
                    if (this.intEntries[index + num2].Key <= entryKey)
                    {
                        index += num2;
                        if (this.intEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }
                    num2 = (length + 3) / 8;
                    if (this.intEntries[index + num2].Key <= entryKey)
                    {
                        index += num2;
                        if (this.intEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }
                    num2 = (length + 7) / 0x10;
                    if (this.intEntries[index + num2].Key <= entryKey)
                    {
                        index += num2;
                        if (this.intEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }
                    if (entryKey > this.intEntries[index].Key)
                    {
                        index++;
                    }
                    return false;
                }
                int num3 = length - 1;
                int num4 = 0;
                int num5 = 0;
                do
                {
                    num5 = (num3 + num4) / 2;
                    short key = this.intEntries[num5].Key;
                    if (key == entryKey)
                    {
                        index = num5;
                        return true;
                    }
                    if (entryKey < key)
                    {
                        num3 = num5 - 1;
                    }
                    else
                    {
                        num4 = num5 + 1;
                    }
                }
                while (num3 >= num4);
                index = num5;
                if (entryKey > this.intEntries[num5].Key)
                {
                    index++;
                }
                return false;
            }
            index = 0;
            return false;
        }

        private bool LocateObjectEntry(short entryKey, out int index)
        {
            if (this.objEntries != null)
            {
                int length = this.objEntries.Length;
                if (length <= 0x10)
                {
                    index = 0;
                    int num2 = length / 2;
                    if (this.objEntries[num2].Key <= entryKey)
                    {
                        index = num2;
                    }
                    if (this.objEntries[index].Key == entryKey)
                    {
                        return true;
                    }
                    num2 = (length + 1) / 4;
                    if (this.objEntries[index + num2].Key <= entryKey)
                    {
                        index += num2;
                        if (this.objEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }
                    num2 = (length + 3) / 8;
                    if (this.objEntries[index + num2].Key <= entryKey)
                    {
                        index += num2;
                        if (this.objEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }
                    num2 = (length + 7) / 0x10;
                    if (this.objEntries[index + num2].Key <= entryKey)
                    {
                        index += num2;
                        if (this.objEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }
                    if (entryKey > this.objEntries[index].Key)
                    {
                        index++;
                    }
                    return false;
                }
                int num3 = length - 1;
                int num4 = 0;
                int num5 = 0;
                do
                {
                    num5 = (num3 + num4) / 2;
                    short key = this.objEntries[num5].Key;
                    if (key == entryKey)
                    {
                        index = num5;
                        return true;
                    }
                    if (entryKey < key)
                    {
                        num3 = num5 - 1;
                    }
                    else
                    {
                        num4 = num5 + 1;
                    }
                }
                while (num3 >= num4);
                index = num5;
                if (entryKey > this.objEntries[num5].Key)
                {
                    index++;
                }
                return false;
            }
            index = 0;
            return false;
        }

        public void RemoveInteger(int key)
        {
            int num;
            short num2;
            short entryKey = this.SplitKey(key, out num2);
            if (this.LocateIntegerEntry(entryKey, out num) && (((((int) 1) << num2) & this.intEntries[num].Mask) != 0))
            {
                this.intEntries[num].Mask = (short) (this.intEntries[num].Mask & ~((short) (((int) 1) << num2)));
                if (this.intEntries[num].Mask == 0)
                {
                    IntegerEntry[] destinationArray = new IntegerEntry[this.intEntries.Length - 1];
                    if (num > 0)
                    {
                        Array.Copy(this.intEntries, 0, destinationArray, 0, num);
                    }
                    if (num < destinationArray.Length)
                    {
                        Array.Copy(this.intEntries, num + 1, destinationArray, num, (this.intEntries.Length - num) - 1);
                    }
                    this.intEntries = destinationArray;
                }
                else
                {
                    switch (num2)
                    {
                        case 0:
                            this.intEntries[num].Value1 = 0;
                            return;

                        case 1:
                            this.intEntries[num].Value2 = 0;
                            return;

                        case 2:
                            this.intEntries[num].Value3 = 0;
                            return;

                        case 3:
                            this.intEntries[num].Value4 = 0;
                            return;
                    }
                }
            }
        }

        public void RemoveObject(int key)
        {
            int num;
            short num2;
            short entryKey = this.SplitKey(key, out num2);
            if (this.LocateObjectEntry(entryKey, out num) && (((((int) 1) << num2) & this.objEntries[num].Mask) != 0))
            {
                this.objEntries[num].Mask = (short) (this.objEntries[num].Mask & ~((short) (((int) 1) << num2)));
                if (this.objEntries[num].Mask == 0)
                {
                    if (this.objEntries.Length == 1)
                    {
                        this.objEntries = null;
                    }
                    else
                    {
                        ObjectEntry[] destinationArray = new ObjectEntry[this.objEntries.Length - 1];
                        if (num > 0)
                        {
                            Array.Copy(this.objEntries, 0, destinationArray, 0, num);
                        }
                        if (num < destinationArray.Length)
                        {
                            Array.Copy(this.objEntries, num + 1, destinationArray, num, (this.objEntries.Length - num) - 1);
                        }
                        this.objEntries = destinationArray;
                    }
                }
                else
                {
                    switch (num2)
                    {
                        case 0:
                            this.objEntries[num].Value1 = null;
                            return;

                        case 1:
                            this.objEntries[num].Value2 = null;
                            return;

                        case 2:
                            this.objEntries[num].Value3 = null;
                            return;

                        case 3:
                            this.objEntries[num].Value4 = null;
                            return;
                    }
                }
            }
        }

        public void SetColor(int key, Color value)
        {
            bool flag;
            object obj2 = this.GetObject(key, out flag);
            if (!flag)
            {
                this.SetObject(key, new ColorWrapper(value));
            }
            else
            {
                ColorWrapper wrapper = obj2 as ColorWrapper;
                if (wrapper != null)
                {
                    wrapper.Color = value;
                }
                else
                {
                    this.SetObject(key, new ColorWrapper(value));
                }
            }
        }

        public void SetInteger(int key, int value)
        {
            int num;
            short num2;
            short entryKey = this.SplitKey(key, out num2);
            if (!this.LocateIntegerEntry(entryKey, out num))
            {
                if (this.intEntries != null)
                {
                    IntegerEntry[] destinationArray = new IntegerEntry[this.intEntries.Length + 1];
                    if (num > 0)
                    {
                        Array.Copy(this.intEntries, 0, destinationArray, 0, num);
                    }
                    if ((this.intEntries.Length - num) > 0)
                    {
                        Array.Copy(this.intEntries, num, destinationArray, num + 1, this.intEntries.Length - num);
                    }
                    this.intEntries = destinationArray;
                }
                else
                {
                    this.intEntries = new IntegerEntry[1];
                }
                this.intEntries[num].Key = entryKey;
            }
            switch (num2)
            {
                case 0:
                    this.intEntries[num].Value1 = value;
                    break;

                case 1:
                    this.intEntries[num].Value2 = value;
                    break;

                case 2:
                    this.intEntries[num].Value3 = value;
                    break;

                case 3:
                    this.intEntries[num].Value4 = value;
                    break;
            }
            this.intEntries[num].Mask = (short) ((((int) 1) << num2) | ((ushort) this.intEntries[num].Mask));
        }

        public void SetObject(int key, object value)
        {
            int num;
            short num2;
            short entryKey = this.SplitKey(key, out num2);
            if (!this.LocateObjectEntry(entryKey, out num))
            {
                if (this.objEntries != null)
                {
                    ObjectEntry[] destinationArray = new ObjectEntry[this.objEntries.Length + 1];
                    if (num > 0)
                    {
                        Array.Copy(this.objEntries, 0, destinationArray, 0, num);
                    }
                    if ((this.objEntries.Length - num) > 0)
                    {
                        Array.Copy(this.objEntries, num, destinationArray, num + 1, this.objEntries.Length - num);
                    }
                    this.objEntries = destinationArray;
                }
                else
                {
                    this.objEntries = new ObjectEntry[1];
                }
                this.objEntries[num].Key = entryKey;
            }
            switch (num2)
            {
                case 0:
                    this.objEntries[num].Value1 = value;
                    break;

                case 1:
                    this.objEntries[num].Value2 = value;
                    break;

                case 2:
                    this.objEntries[num].Value3 = value;
                    break;

                case 3:
                    this.objEntries[num].Value4 = value;
                    break;
            }
            this.objEntries[num].Mask = (short) (((ushort) this.objEntries[num].Mask) | (((int) 1) << num2));
        }

        public void SetPadding(int key, Padding value)
        {
            bool flag;
            object obj2 = this.GetObject(key, out flag);
            if (!flag)
            {
                this.SetObject(key, new PaddingWrapper(value));
            }
            else
            {
                PaddingWrapper wrapper = obj2 as PaddingWrapper;
                if (wrapper != null)
                {
                    wrapper.Padding = value;
                }
                else
                {
                    this.SetObject(key, new PaddingWrapper(value));
                }
            }
        }

        public void SetRectangle(int key, Rectangle value)
        {
            bool flag;
            object obj2 = this.GetObject(key, out flag);
            if (!flag)
            {
                this.SetObject(key, new RectangleWrapper(value));
            }
            else
            {
                RectangleWrapper wrapper = obj2 as RectangleWrapper;
                if (wrapper != null)
                {
                    wrapper.Rectangle = value;
                }
                else
                {
                    this.SetObject(key, new RectangleWrapper(value));
                }
            }
        }

        public void SetSize(int key, Size value)
        {
            bool flag;
            object obj2 = this.GetObject(key, out flag);
            if (!flag)
            {
                this.SetObject(key, new SizeWrapper(value));
            }
            else
            {
                SizeWrapper wrapper = obj2 as SizeWrapper;
                if (wrapper != null)
                {
                    wrapper.Size = value;
                }
                else
                {
                    this.SetObject(key, new SizeWrapper(value));
                }
            }
        }

        private short SplitKey(int key, out short element)
        {
            element = (short) (key & 3);
            return (short) (key & 0xfffffffcL);
        }

        private sealed class ColorWrapper
        {
            public System.Drawing.Color Color;

            public ColorWrapper(System.Drawing.Color color)
            {
                this.Color = color;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IntegerEntry
        {
            public short Key;
            public short Mask;
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ObjectEntry
        {
            public short Key;
            public short Mask;
            public object Value1;
            public object Value2;
            public object Value3;
            public object Value4;
        }

        private sealed class PaddingWrapper
        {
            public System.Windows.Forms.Padding Padding;

            public PaddingWrapper(System.Windows.Forms.Padding padding)
            {
                this.Padding = padding;
            }
        }

        private sealed class RectangleWrapper
        {
            public System.Drawing.Rectangle Rectangle;

            public RectangleWrapper(System.Drawing.Rectangle rectangle)
            {
                this.Rectangle = rectangle;
            }
        }

        private sealed class SizeWrapper
        {
            public System.Drawing.Size Size;

            public SizeWrapper(System.Drawing.Size size)
            {
                this.Size = size;
            }
        }
    }
}

