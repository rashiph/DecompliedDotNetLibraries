namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    internal class DistinguishedName
    {
        private Component[] components;

        public DistinguishedName(string dn)
        {
            this.components = Utils.GetDNComponents(dn);
        }

        public bool Equals(DistinguishedName dn)
        {
            if ((dn == null) || (this.components.GetLength(0) != dn.Components.GetLength(0)))
            {
                return false;
            }
            for (int i = 0; i < this.components.GetLength(0); i++)
            {
                if ((Utils.Compare(this.components[i].Name, dn.Components[i].Name) != 0) || (Utils.Compare(this.components[i].Value, dn.Components[i].Value) != 0))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return (((obj != null) && (obj is DistinguishedName)) && this.Equals((DistinguishedName) obj));
        }

        public override int GetHashCode()
        {
            int num = 0;
            for (int i = 0; i < this.components.GetLength(0); i++)
            {
                num = (num + this.components[i].Name.ToUpperInvariant().GetHashCode()) + this.components[i].Value.ToUpperInvariant().GetHashCode();
            }
            return num;
        }

        public override string ToString()
        {
            string str = this.components[0].Name + "=" + this.components[0].Value;
            for (int i = 1; i < this.components.GetLength(0); i++)
            {
                str = str + "," + this.components[i].Name + "=" + this.components[i].Value;
            }
            return str;
        }

        public Component[] Components
        {
            get
            {
                return this.components;
            }
        }
    }
}

