namespace System.Deployment.Application
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static Assembly _assembly = null;
        private static ResourceManager _resources = null;
        private static object lockObject = new object();

        public static Icon GetIcon(string iconName)
        {
            InitializeReferenceToAssembly();
            using (Stream stream = _assembly.GetManifestResourceStream(iconName))
            {
                return new Icon(stream);
            }
        }

        public static Image GetImage(string imageName)
        {
            Image image;
            InitializeReferenceToAssembly();
            Stream manifestResourceStream = null;
            try
            {
                manifestResourceStream = _assembly.GetManifestResourceStream(imageName);
                image = Image.FromStream(manifestResourceStream);
            }
            catch
            {
                if (manifestResourceStream != null)
                {
                    manifestResourceStream.Close();
                }
                throw;
            }
            return image;
        }

        public static string GetString(string s)
        {
            if (_resources == null)
            {
                lock (lockObject)
                {
                    if (_resources == null)
                    {
                        InitializeReferenceToAssembly();
                        _resources = new ResourceManager("System.Deployment", _assembly);
                    }
                }
            }
            return _resources.GetString(s);
        }

        private static void InitializeReferenceToAssembly()
        {
            if (_assembly == null)
            {
                lock (lockObject)
                {
                    if (_assembly == null)
                    {
                        _assembly = Assembly.GetExecutingAssembly();
                    }
                }
            }
        }
    }
}

