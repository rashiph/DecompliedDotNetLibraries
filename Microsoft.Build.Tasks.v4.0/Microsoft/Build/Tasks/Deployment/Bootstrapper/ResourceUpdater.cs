namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal class ResourceUpdater
    {
        private const int ERROR_SHARING_VIOLATION = -2147024864;
        private ArrayList fileResources = new ArrayList();
        private ArrayList stringResources = new ArrayList();

        public void AddFileResource(string filename, string key)
        {
            this.fileResources.Add(new FileResource(filename, key));
        }

        public void AddStringResource(int type, string name, string data)
        {
            this.stringResources.Add(new StringResource(type, name, data));
        }

        private byte[] StringToByteArray(string str)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            byte[] array = new byte[bytes.Length + 2];
            bytes.CopyTo(array, 0);
            array[array.Length - 2] = 0;
            array[array.Length - 1] = 0;
            return array;
        }

        public bool UpdateResources(string filename, BuildResults results)
        {
            bool flag = true;
            int num = 20;
            bool flag2 = false;
            if ((this.stringResources.Count == 0) && (this.fileResources.Count == 0))
            {
                return true;
            }
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = Microsoft.Build.Tasks.Deployment.Bootstrapper.NativeMethods.BeginUpdateResourceW(filename, false);
                while (((IntPtr.Zero == zero) && (Marshal.GetHRForLastWin32Error() == -2147024864)) && (num > 0))
                {
                    zero = Microsoft.Build.Tasks.Deployment.Bootstrapper.NativeMethods.BeginUpdateResourceW(filename, false);
                    num--;
                    Thread.Sleep(100);
                }
                if (IntPtr.Zero == zero)
                {
                    results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", new object[] { string.Format("Unable to begin updating resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error()) }));
                    return false;
                }
                flag2 = true;
                if (!(zero != IntPtr.Zero))
                {
                    return flag;
                }
                foreach (StringResource resource in this.stringResources)
                {
                    byte[] buffer = this.StringToByteArray(resource.Data);
                    if (!Microsoft.Build.Tasks.Deployment.Bootstrapper.NativeMethods.UpdateResourceW(zero, (IntPtr) resource.Type, resource.Name, 0, buffer, buffer.Length))
                    {
                        results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", new object[] { string.Format("Unable to update resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error()) }));
                        return false;
                    }
                }
                if (this.fileResources.Count <= 0)
                {
                    return flag;
                }
                int num2 = 0;
                byte[] data = this.StringToByteArray(this.fileResources.Count.ToString("G", CultureInfo.InvariantCulture));
                if (!Microsoft.Build.Tasks.Deployment.Bootstrapper.NativeMethods.UpdateResourceW(zero, (IntPtr) 0x2a, "COUNT", 0, data, data.Length))
                {
                    results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", new object[] { string.Format("Unable to update count resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error()) }));
                    return false;
                }
                foreach (FileResource resource2 in this.fileResources)
                {
                    int count = 0;
                    byte[] buffer3 = null;
                    using (FileStream stream = File.OpenRead(resource2.Filename))
                    {
                        count = (int) stream.Length;
                        buffer3 = new byte[count];
                        stream.Read(buffer3, 0, count);
                    }
                    string lpName = string.Format(CultureInfo.InvariantCulture, "FILEDATA{0}", new object[] { num2 });
                    if (!Microsoft.Build.Tasks.Deployment.Bootstrapper.NativeMethods.UpdateResourceW(zero, (IntPtr) 0x2a, lpName, 0, buffer3, count))
                    {
                        results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", new object[] { string.Format("Unable to update data resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error()) }));
                        return false;
                    }
                    string str2 = string.Format(CultureInfo.InvariantCulture, "FILEKEY{0}", new object[] { num2 });
                    byte[] buffer4 = this.StringToByteArray(resource2.Key);
                    if (!Microsoft.Build.Tasks.Deployment.Bootstrapper.NativeMethods.UpdateResourceW(zero, (IntPtr) 0x2a, str2, 0, buffer4, buffer4.Length))
                    {
                        results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", new object[] { string.Format("Unable to update key resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error()) }));
                        return false;
                    }
                    num2++;
                }
            }
            finally
            {
                if (flag2 && !Microsoft.Build.Tasks.Deployment.Bootstrapper.NativeMethods.EndUpdateResource(zero, false))
                {
                    results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", new object[] { string.Format("Unable to finish updating resource for {0} with error {1:X}", filename, Marshal.GetHRForLastWin32Error()) }));
                    flag = false;
                }
            }
            return flag;
        }

        private class FileResource
        {
            public string Filename;
            public string Key;

            public FileResource(string filename, string key)
            {
                this.Filename = filename;
                this.Key = key;
            }
        }

        private class StringResource
        {
            public string Data;
            public string Name;
            public int Type;

            public StringResource(int type, string name, string data)
            {
                this.Type = type;
                this.Name = name;
                this.Data = data;
            }
        }
    }
}

