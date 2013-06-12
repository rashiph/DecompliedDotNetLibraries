namespace System.Security
{
    using System;
    using System.Collections;
    using System.Security.Util;
    using System.Text;

    [Serializable]
    internal sealed class SecurityDocument
    {
        internal const byte c_attribute = 2;
        internal const byte c_children = 4;
        internal const byte c_element = 1;
        internal const int c_growthSize = 0x20;
        internal const byte c_text = 3;
        internal byte[] m_data;

        public SecurityDocument(int numData)
        {
            this.m_data = new byte[numData];
        }

        public SecurityDocument(byte[] data)
        {
            this.m_data = data;
        }

        public SecurityDocument(SecurityElement elRoot)
        {
            this.m_data = new byte[0x20];
            int position = 0;
            this.ConvertElement(elRoot, ref position);
        }

        public void AddString(string str, ref int position)
        {
            this.GuaranteeSize((position + (str.Length * 2)) + 2);
            for (int i = 0; i < str.Length; i++)
            {
                this.m_data[position + (2 * i)] = (byte) (str[i] >> 8);
                this.m_data[(position + (2 * i)) + 1] = (byte) (str[i] & '\x00ff');
            }
            this.m_data[position + (str.Length * 2)] = 0;
            this.m_data[(position + (str.Length * 2)) + 1] = 0;
            position += (str.Length * 2) + 2;
        }

        public void AddToken(byte b, ref int position)
        {
            this.GuaranteeSize(position + 1);
            this.m_data[position++] = b;
        }

        public void AppendString(string str, ref int position)
        {
            if (((position <= 1) || (this.m_data[position - 1] != 0)) || (this.m_data[position - 2] != 0))
            {
                throw new XmlSyntaxException();
            }
            position -= 2;
            this.AddString(str, ref position);
        }

        public void ConvertElement(SecurityElement elCurrent, ref int position)
        {
            this.AddToken(1, ref position);
            this.AddString(elCurrent.m_strTag, ref position);
            if (elCurrent.m_lAttributes != null)
            {
                for (int i = 0; i < elCurrent.m_lAttributes.Count; i += 2)
                {
                    this.AddToken(2, ref position);
                    this.AddString((string) elCurrent.m_lAttributes[i], ref position);
                    this.AddString((string) elCurrent.m_lAttributes[i + 1], ref position);
                }
            }
            if (elCurrent.m_strText != null)
            {
                this.AddToken(3, ref position);
                this.AddString(elCurrent.m_strText, ref position);
            }
            if (elCurrent.InternalChildren != null)
            {
                for (int j = 0; j < elCurrent.InternalChildren.Count; j++)
                {
                    this.ConvertElement((SecurityElement) elCurrent.Children[j], ref position);
                }
            }
            this.AddToken(4, ref position);
        }

        public static int EncodedStringSize(string str)
        {
            return ((str.Length * 2) + 2);
        }

        public string GetAttributeForElement(int position, string attributeName)
        {
            if (this.m_data.Length <= position)
            {
                throw new XmlSyntaxException();
            }
            if (this.m_data[position++] != 1)
            {
                throw new XmlSyntaxException();
            }
            this.GetString(ref position, false);
            while (this.m_data[position] == 2)
            {
                position++;
                string a = this.GetString(ref position);
                string str3 = this.GetString(ref position);
                if (string.Equals(a, attributeName))
                {
                    return str3;
                }
            }
            return null;
        }

        public ArrayList GetChildrenPositionForElement(int position)
        {
            if (this.m_data.Length <= position)
            {
                throw new XmlSyntaxException();
            }
            if (this.m_data[position++] != 1)
            {
                throw new XmlSyntaxException();
            }
            ArrayList list = new ArrayList();
            this.GetString(ref position);
            while (this.m_data[position] == 2)
            {
                position++;
                this.GetString(ref position, false);
                this.GetString(ref position, false);
            }
            if (this.m_data[position] == 3)
            {
                position++;
                this.GetString(ref position, false);
            }
            while (this.m_data[position] != 4)
            {
                list.Add(position);
                this.InternalGetElement(ref position, false);
            }
            position++;
            return list;
        }

        public SecurityElement GetElement(int position, bool bCreate)
        {
            return this.InternalGetElement(ref position, bCreate);
        }

        public SecurityElement GetRootElement()
        {
            return this.GetElement(0, true);
        }

        public string GetString(ref int position)
        {
            return this.GetString(ref position, true);
        }

        public string GetString(ref int position, bool bCreate)
        {
            string str;
            int index = position;
            while (index < (this.m_data.Length - 1))
            {
                if ((this.m_data[index] == 0) && (this.m_data[index + 1] == 0))
                {
                    break;
                }
                index += 2;
            }
            Tokenizer.StringMaker sharedStringMaker = SharedStatics.GetSharedStringMaker();
            try
            {
                if (bCreate)
                {
                    sharedStringMaker._outStringBuilder = null;
                    sharedStringMaker._outIndex = 0;
                    for (int i = position; i < index; i += 2)
                    {
                        char ch = (char) ((this.m_data[i] << 8) | this.m_data[i + 1]);
                        if (sharedStringMaker._outIndex < 0x200)
                        {
                            sharedStringMaker._outChars[sharedStringMaker._outIndex++] = ch;
                        }
                        else
                        {
                            if (sharedStringMaker._outStringBuilder == null)
                            {
                                sharedStringMaker._outStringBuilder = new StringBuilder();
                            }
                            sharedStringMaker._outStringBuilder.Append(sharedStringMaker._outChars, 0, 0x200);
                            sharedStringMaker._outChars[0] = ch;
                            sharedStringMaker._outIndex = 1;
                        }
                    }
                }
                position = index + 2;
                if (bCreate)
                {
                    return sharedStringMaker.MakeString();
                }
                str = null;
            }
            finally
            {
                SharedStatics.ReleaseSharedStringMaker(ref sharedStringMaker);
            }
            return str;
        }

        public string GetTagForElement(int position)
        {
            if (this.m_data.Length <= position)
            {
                throw new XmlSyntaxException();
            }
            if (this.m_data[position++] != 1)
            {
                throw new XmlSyntaxException();
            }
            return this.GetString(ref position);
        }

        public void GuaranteeSize(int size)
        {
            if (this.m_data.Length < size)
            {
                byte[] destinationArray = new byte[((size / 0x20) + 1) * 0x20];
                Array.Copy(this.m_data, 0, destinationArray, 0, this.m_data.Length);
                this.m_data = destinationArray;
            }
        }

        internal SecurityElement InternalGetElement(ref int position, bool bCreate)
        {
            if (this.m_data.Length <= position)
            {
                throw new XmlSyntaxException();
            }
            if (this.m_data[position++] != 1)
            {
                throw new XmlSyntaxException();
            }
            SecurityElement element = null;
            string tag = this.GetString(ref position, bCreate);
            if (bCreate)
            {
                element = new SecurityElement(tag);
            }
            while (this.m_data[position] == 2)
            {
                position++;
                string name = this.GetString(ref position, bCreate);
                string str3 = this.GetString(ref position, bCreate);
                if (bCreate)
                {
                    element.AddAttribute(name, str3);
                }
            }
            if (this.m_data[position] == 3)
            {
                position++;
                string str4 = this.GetString(ref position, bCreate);
                if (bCreate)
                {
                    element.m_strText = str4;
                }
            }
            while (this.m_data[position] != 4)
            {
                SecurityElement child = this.InternalGetElement(ref position, bCreate);
                if (bCreate)
                {
                    element.AddChild(child);
                }
            }
            position++;
            return element;
        }
    }
}

