namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;

    [Serializable]
    internal class SerializableRegistryKey
    {
        public string[] KeyNames;
        public SerializableRegistryKey[] Keys;
        public string[] ValueNames;
        public object[] Values;

        public SerializableRegistryKey(RegistryKey keyToSave)
        {
            this.CopyFromRegistry(keyToSave);
        }

        public void CopyFromRegistry(RegistryKey keyToSave)
        {
            if (keyToSave == null)
            {
                throw new ArgumentNullException("keyToSave");
            }
            this.ValueNames = keyToSave.GetValueNames();
            if (this.ValueNames == null)
            {
                this.ValueNames = new string[0];
            }
            this.Values = new object[this.ValueNames.Length];
            for (int i = 0; i < this.ValueNames.Length; i++)
            {
                this.Values[i] = keyToSave.GetValue(this.ValueNames[i]);
            }
            this.KeyNames = keyToSave.GetSubKeyNames();
            if (this.KeyNames == null)
            {
                this.KeyNames = new string[0];
            }
            this.Keys = new SerializableRegistryKey[this.KeyNames.Length];
            for (int j = 0; j < this.KeyNames.Length; j++)
            {
                this.Keys[j] = new SerializableRegistryKey(keyToSave.OpenSubKey(this.KeyNames[j]));
            }
        }

        public void CopyToRegistry(RegistryKey baseKey)
        {
            if (baseKey == null)
            {
                throw new ArgumentNullException("baseKey");
            }
            if (this.Values != null)
            {
                for (int i = 0; i < this.Values.Length; i++)
                {
                    baseKey.SetValue(this.ValueNames[i], this.Values[i]);
                }
            }
            if (this.Keys != null)
            {
                for (int j = 0; j < this.Keys.Length; j++)
                {
                    RegistryKey key = baseKey.CreateSubKey(this.KeyNames[j]);
                    this.Keys[j].CopyToRegistry(key);
                }
            }
        }
    }
}

