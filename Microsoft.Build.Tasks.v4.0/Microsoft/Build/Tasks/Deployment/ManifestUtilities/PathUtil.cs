namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Globalization;
    using System.IO;

    internal static class PathUtil
    {
        public static string CanonicalizePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                path = path.TrimEnd(new char[] { Path.DirectorySeparatorChar });
            }
            return path;
        }

        public static string CanonicalizeUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.AbsoluteUri;
        }

        public static string Format(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            Uri uri = new Uri(Resolve(path));
            return uri.AbsoluteUri;
        }

        public static string[] GetPathSegments(string path)
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            path = path.TrimEnd(new char[] { Path.DirectorySeparatorChar });
            return path.Split(new char[] { Path.DirectorySeparatorChar });
        }

        public static bool IsAssembly(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (string.Equals(Path.GetExtension(path), ".application", StringComparison.Ordinal))
            {
                return true;
            }
            if (string.Equals(Path.GetExtension(path), ".manifest", StringComparison.Ordinal))
            {
                return true;
            }
            if (!IsProgramFile(path))
            {
                return false;
            }
            return (IsManagedAssembly(path) || IsNativeAssembly(path));
        }

        public static bool IsDataFile(string path)
        {
            if ((!path.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase)) && (!path.EndsWith(".ldf", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase)))
            {
                return path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        public static bool IsEqualPath(string path1, string path2)
        {
            return (string.Compare(CanonicalizePath(path1), CanonicalizePath(path2), true, CultureInfo.CurrentCulture) == 0);
        }

        public static bool IsLocalPath(string path)
        {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            return (!uri.IsAbsoluteUri || string.IsNullOrEmpty(uri.Host));
        }

        public static bool IsManagedAssembly(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            using (MetadataReader reader = MetadataReader.Create(path))
            {
                return (reader != null);
            }
        }

        public static bool IsNativeAssembly(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            return (string.Equals(Path.GetExtension(path), ".manifest", StringComparison.Ordinal) || (EmbeddedManifestReader.Read(path) != null));
        }

        public static bool IsPEFile(string path)
        {
            byte[] buffer = new byte[2];
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Read(buffer, 0, 2);
            }
            return ((buffer[0] == 0x4d) && (buffer[1] == 90));
        }

        public static bool IsProgramFile(string path)
        {
            if (!path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        public static bool IsUncPath(string path)
        {
            Uri result = null;
            return ((Uri.TryCreate(path, UriKind.Absolute, out result) && (result != null)) && result.IsUnc);
        }

        public static bool IsUrl(string path)
        {
            Uri result = null;
            if (!Uri.TryCreate(path, UriKind.Absolute, out result) || (result == null))
            {
                return false;
            }
            return (!result.IsUnc && !string.IsNullOrEmpty(result.Host));
        }

        public static string Resolve(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (IsUncPath(path))
            {
                return path;
            }
            if (!IsUrl(path))
            {
                return Path.GetFullPath(path);
            }
            Uri uri = new Uri(path);
            if (!string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
            int index = path.IndexOf("localhost", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return path;
            }
            return (path.Substring(0, index) + Environment.MachineName.ToLowerInvariant() + path.Substring(index + "localhost".Length));
        }
    }
}

