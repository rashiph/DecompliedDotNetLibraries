namespace System.Xml.Schema
{
    using System;
    using System.Reflection;
    using System.Text;

    internal class KeySequence
    {
        private int dim;
        private int hashcode;
        private TypedObject[] ks;
        private int poscol;
        private int posline;

        public KeySequence(TypedObject[] ks)
        {
            this.hashcode = -1;
            this.ks = ks;
            this.dim = ks.Length;
            this.posline = this.poscol = 0;
        }

        internal KeySequence(int dim, int line, int col)
        {
            this.hashcode = -1;
            this.dim = dim;
            this.ks = new TypedObject[dim];
            this.posline = line;
            this.poscol = col;
        }

        public override bool Equals(object other)
        {
            KeySequence sequence = (KeySequence) other;
            for (int i = 0; i < this.ks.Length; i++)
            {
                if (!this.ks[i].Equals(sequence.ks[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            if (this.hashcode == -1)
            {
                this.hashcode = 0;
                for (int i = 0; i < this.ks.Length; i++)
                {
                    this.ks[i].SetDecimal();
                    if (this.ks[i].IsDecimal)
                    {
                        for (int j = 0; j < this.ks[i].Dim; j++)
                        {
                            this.hashcode += this.ks[i].Dvalue[j].GetHashCode();
                        }
                    }
                    else
                    {
                        Array array = this.ks[i].Value as Array;
                        if (array != null)
                        {
                            XmlAtomicValue[] valueArray = array as XmlAtomicValue[];
                            if (valueArray != null)
                            {
                                for (int k = 0; k < valueArray.Length; k++)
                                {
                                    this.hashcode += ((XmlAtomicValue) valueArray.GetValue(k)).TypedValue.GetHashCode();
                                }
                            }
                            else
                            {
                                for (int m = 0; m < ((Array) this.ks[i].Value).Length; m++)
                                {
                                    this.hashcode += ((Array) this.ks[i].Value).GetValue(m).GetHashCode();
                                }
                            }
                        }
                        else
                        {
                            this.hashcode += this.ks[i].Value.GetHashCode();
                        }
                    }
                }
            }
            return this.hashcode;
        }

        internal bool IsQualified()
        {
            for (int i = 0; i < this.ks.Length; i++)
            {
                if ((this.ks[i] == null) || (this.ks[i].Value == null))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.ks[0].ToString());
            for (int i = 1; i < this.ks.Length; i++)
            {
                builder.Append(" ");
                builder.Append(this.ks[i].ToString());
            }
            return builder.ToString();
        }

        public object this[int index]
        {
            get
            {
                return this.ks[index];
            }
            set
            {
                this.ks[index] = (TypedObject) value;
            }
        }

        public int PosCol
        {
            get
            {
                return this.poscol;
            }
        }

        public int PosLine
        {
            get
            {
                return this.posline;
            }
        }
    }
}

