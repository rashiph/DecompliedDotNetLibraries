namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    internal class AssemblyNameEqualityComparer : IEqualityComparer, IEqualityComparer<AssemblyName>
    {
        public bool Equals(object xparam, object yparam)
        {
            return (((xparam == null) && (yparam == null)) || this.Equals(xparam as AssemblyName, yparam as AssemblyName));
        }

        public bool Equals(AssemblyName x, AssemblyName y)
        {
            if ((x == null) || (y == null))
            {
                return false;
            }
            if (!object.ReferenceEquals(x, y))
            {
                if ((x.Name != null) && (y.Name != null))
                {
                    if (string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return false;
                    }
                }
                else if ((x.Name != null) || (y.Name != null))
                {
                    return false;
                }
                if ((x.Version != null) && (y.Version != null))
                {
                    if (x.Version != y.Version)
                    {
                        return false;
                    }
                }
                else if ((x.Version != null) || (y.Version != null))
                {
                    return false;
                }
                if ((x.CultureInfo != null) && (y.CultureInfo != null))
                {
                    if (!x.CultureInfo.Equals(y.CultureInfo))
                    {
                        return false;
                    }
                }
                else if ((x.CultureInfo != null) || (y.CultureInfo != null))
                {
                    return false;
                }
                byte[] publicKeyToken = x.GetPublicKeyToken();
                byte[] curKeyToken = y.GetPublicKeyToken();
                if (!IsSameKeyToken(publicKeyToken, curKeyToken))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(object objparam)
        {
            AssemblyName name = objparam as AssemblyName;
            if (name == null)
            {
                return 0;
            }
            return this.GetHashCode(name);
        }

        public int GetHashCode(AssemblyName obj)
        {
            int num = 0;
            if (obj.Name != null)
            {
                num ^= obj.Name.GetHashCode();
            }
            if (obj.Version != null)
            {
                num ^= obj.Version.GetHashCode();
            }
            if (obj.CultureInfo != null)
            {
                num ^= obj.CultureInfo.GetHashCode();
            }
            byte[] publicKeyToken = obj.GetPublicKeyToken();
            if (publicKeyToken != null)
            {
                int length = publicKeyToken.Length;
                num ^= length.GetHashCode() + 1;
                if (publicKeyToken.Length > 0)
                {
                    num ^= BitConverter.ToUInt64(publicKeyToken, 0).GetHashCode();
                }
            }
            return num;
        }

        public static bool IsSameKeyToken(byte[] reqKeyToken, byte[] curKeyToken)
        {
            bool flag = false;
            if ((reqKeyToken == null) && (curKeyToken == null))
            {
                return true;
            }
            if (((reqKeyToken != null) && (curKeyToken != null)) && (reqKeyToken.Length == curKeyToken.Length))
            {
                flag = true;
                for (int i = 0; i < reqKeyToken.Length; i++)
                {
                    if (reqKeyToken[i] != curKeyToken[i])
                    {
                        return false;
                    }
                }
            }
            return flag;
        }
    }
}

