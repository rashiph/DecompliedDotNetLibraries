namespace System.Resources
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.Serialization;

    internal class ResXSerializationBinder : SerializationBinder
    {
        private ITypeResolutionService typeResolver;

        internal ResXSerializationBinder(ITypeResolutionService typeResolver)
        {
            this.typeResolver = typeResolver;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (this.typeResolver == null)
            {
                return null;
            }
            typeName = typeName + ", " + assemblyName;
            Type type = this.typeResolver.GetType(typeName);
            if (type == null)
            {
                string[] strArray = typeName.Split(new char[] { ',' });
                if ((strArray == null) || (strArray.Length <= 2))
                {
                    return type;
                }
                string name = strArray[0].Trim();
                for (int i = 1; i < strArray.Length; i++)
                {
                    string str2 = strArray[i].Trim();
                    if (!str2.StartsWith("Version=") && !str2.StartsWith("version="))
                    {
                        name = name + ", " + str2;
                    }
                }
                type = this.typeResolver.GetType(name);
                if (type == null)
                {
                    type = this.typeResolver.GetType(strArray[0].Trim());
                }
            }
            return type;
        }
    }
}

