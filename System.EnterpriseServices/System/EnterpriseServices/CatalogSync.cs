namespace System.EnterpriseServices
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class CatalogSync
    {
        private bool _set = false;
        private int _version = 0;

        internal CatalogSync()
        {
        }

        internal void Set()
        {
            try
            {
                if (!this._set && ContextUtil.IsInTransaction)
                {
                    this._set = true;
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\CLSID");
                    this._version = (int) key.GetValue("CLBVersion", 0);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                this._set = false;
                this._version = 0;
            }
        }

        internal void Wait()
        {
            if (this._set)
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\CLSID");
                while (true)
                {
                    int num = (int) key.GetValue("CLBVersion", 0);
                    if (num != this._version)
                    {
                        break;
                    }
                    Thread.Sleep(0);
                }
                this._set = false;
            }
        }
    }
}

