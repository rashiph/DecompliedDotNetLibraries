namespace System.Deployment.Application
{
    using System;
    using System.IO;
    using System.Threading;

    internal static class UriHelper
    {
        private static char[] _directorySeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private static object _invalidRelativePathChars;

        public static bool IsSupportedScheme(Uri uri)
        {
            if (!(uri.Scheme == Uri.UriSchemeFile) && !(uri.Scheme == Uri.UriSchemeHttp))
            {
                return (uri.Scheme == Uri.UriSchemeHttps);
            }
            return true;
        }

        public static bool IsValidRelativeFilePath(string path)
        {
            if (((path == null) || (path.Length == 0)) || (path.IndexOfAny(InvalidRelativePathChars) >= 0))
            {
                return false;
            }
            if (Path.IsPathRooted(path))
            {
                return false;
            }
            string str = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            string fullPath = Path.GetFullPath(Path.Combine(Path.DirectorySeparatorChar.ToString(), str));
            string pathRoot = Path.GetPathRoot(fullPath);
            string strA = fullPath.Substring(pathRoot.Length);
            if ((strA.Length > 0) && (strA[0] == '\\'))
            {
                strA = strA.Substring(1);
            }
            return (string.Compare(strA, str, StringComparison.Ordinal) == 0);
        }

        public static string NormalizePathDirectorySeparators(string path)
        {
            if (path == null)
            {
                return null;
            }
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static bool PathContainDirectorySeparators(string path)
        {
            if (path == null)
            {
                return false;
            }
            return (path.IndexOfAny(_directorySeparators) >= 0);
        }

        public static Uri UriFromRelativeFilePath(Uri baseUri, string path)
        {
            if (!IsValidRelativeFilePath(path))
            {
                throw new ArgumentException(Resources.GetString("Ex_InvalidRelativePath"));
            }
            if (path.IndexOf('%') >= 0)
            {
                path = path.Replace("%", Uri.HexEscape('%'));
            }
            if (path.IndexOf('#') >= 0)
            {
                path = path.Replace("#", Uri.HexEscape('#'));
            }
            Uri uri = new Uri(baseUri, path);
            ValidateSupportedScheme(uri);
            return uri;
        }

        public static void ValidateSupportedScheme(Uri uri)
        {
            if (!IsSupportedScheme(uri))
            {
                throw new InvalidDeploymentException(ExceptionTypes.UriSchemeNotSupported, Resources.GetString("Ex_NotSupportedUriScheme"));
            }
        }

        public static void ValidateSupportedSchemeInArgument(Uri uri, string argumentName)
        {
            if (!IsSupportedScheme(uri))
            {
                throw new ArgumentException(Resources.GetString("Ex_NotSupportedUriScheme"), argumentName);
            }
        }

        private static char[] InvalidRelativePathChars
        {
            get
            {
                if (_invalidRelativePathChars == null)
                {
                    char[] invalidPathChars = Path.GetInvalidPathChars();
                    char[] array = new char[invalidPathChars.Length + 3];
                    invalidPathChars.CopyTo(array, 0);
                    int length = invalidPathChars.Length;
                    array[length++] = Path.VolumeSeparatorChar;
                    array[length++] = '*';
                    array[length++] = '?';
                    Interlocked.CompareExchange(ref _invalidRelativePathChars, array, null);
                }
                return (char[]) _invalidRelativePathChars;
            }
        }
    }
}

