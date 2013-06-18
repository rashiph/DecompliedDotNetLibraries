namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class QualifiedId : IEquatable<QualifiedId>
    {
        private byte[] compressedId;

        public QualifiedId(Activity element)
        {
            int num = 0;
            Stack<int> stack = new Stack<int>();
            int internalId = element.InternalId;
            num += GetEncodedSize(internalId);
            stack.Push(internalId);
            for (IdSpace space = element.MemberOf; (space != null) && (space.ParentId != 0); space = space.Parent)
            {
                num += GetEncodedSize(space.ParentId);
                stack.Push(space.ParentId);
            }
            this.compressedId = new byte[num];
            for (int i = 0; stack.Count > 0; i += Encode(stack.Pop(), this.compressedId, i))
            {
            }
        }

        public QualifiedId(byte[] bytes)
        {
            this.compressedId = bytes;
        }

        public byte[] AsByteArray()
        {
            return this.compressedId;
        }

        private static int Decode(byte[] buffer, int offset, out int value)
        {
            int num = 0;
            value = 0;
            while (offset < buffer.Length)
            {
                int num2 = buffer[offset];
                value |= (num2 & 0x7f) << (num * 7);
                num++;
                if ((num2 & 0x80) == 0)
                {
                    return num;
                }
                offset++;
            }
            return num;
        }

        private static int Encode(int value, byte[] bytes, int offset)
        {
            int num = 1;
            while ((value & 0xffffff80L) != 0L)
            {
                bytes[offset++] = (byte) ((value & 0x7f) | 0x80);
                num++;
                value = value >> 7;
            }
            bytes[offset] = (byte) value;
            return num;
        }

        public bool Equals(QualifiedId rhs)
        {
            return Equals(this.compressedId, rhs.compressedId);
        }

        public static bool Equals(byte[] lhs, byte[] rhs)
        {
            if (lhs.Length != rhs.Length)
            {
                return false;
            }
            for (int i = 0; i < lhs.Length; i++)
            {
                if (lhs[i] != rhs[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static int GetEncodedSize(int value)
        {
            int num = 1;
            while ((value & 0xffffff80L) != 0L)
            {
                num++;
                value = value >> 7;
            }
            return num;
        }

        public static QualifiedId Parse(string value)
        {
            QualifiedId id;
            if (!TryParse(value, out id))
            {
                throw FxTrace.Exception.AsError(new FormatException(System.Activities.SR.InvalidActivityIdFormat));
            }
            return id;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            int offset = 0;
            while (offset < this.compressedId.Length)
            {
                int num2;
                if (flag)
                {
                    builder.Append('.');
                }
                offset += Decode(this.compressedId, offset, out num2);
                builder.Append(num2);
                flag = true;
            }
            return builder.ToString();
        }

        public static bool TryGetElementFromRoot(Activity root, QualifiedId id, out Activity targetElement)
        {
            return TryGetElementFromRoot(root, id.compressedId, out targetElement);
        }

        public static bool TryGetElementFromRoot(Activity root, byte[] idBytes, out Activity targetElement)
        {
            Activity activity = root;
            IdSpace memberOf = root.MemberOf;
            int offset = 0;
            while (offset < idBytes.Length)
            {
                int num2;
                offset += Decode(idBytes, offset, out num2);
                if (memberOf == null)
                {
                    targetElement = null;
                    return false;
                }
                activity = memberOf[num2];
                if (activity == null)
                {
                    targetElement = null;
                    return false;
                }
                memberOf = activity.ParentOf;
            }
            targetElement = activity;
            return true;
        }

        public static bool TryParse(string value, out QualifiedId result)
        {
            string[] strArray = value.Split(new char[] { '.' });
            int[] numArray = new int[strArray.Length];
            int num = 0;
            for (int i = 0; i < strArray.Length; i++)
            {
                int num3;
                if (!int.TryParse(strArray[i], out num3) || (num3 < 0))
                {
                    result = null;
                    return false;
                }
                numArray[i] = num3;
                num += GetEncodedSize(numArray[i]);
            }
            byte[] bytes = new byte[num];
            int offset = 0;
            for (int j = 0; j < numArray.Length; j++)
            {
                offset += Encode(numArray[j], bytes, offset);
            }
            result = new QualifiedId(bytes);
            return true;
        }
    }
}

