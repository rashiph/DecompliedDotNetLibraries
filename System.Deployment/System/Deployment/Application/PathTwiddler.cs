namespace System.Deployment.Application
{
    using System;
    using System.IO;
    using System.Threading;

    internal static class PathTwiddler
    {
        private static object _invalidFileDirNameChars;

        public static string FilterString(string input, char chReplace, bool fMultiReplace)
        {
            return FilterString(input, InvalidFileDirNameChars, chReplace, fMultiReplace);
        }

        private static string FilterString(string input, char[] toFilter, char chReplacement, bool fMultiReplace)
        {
            int length = 0;
            bool flag = false;
            bool flag2 = false;
            if (input != null)
            {
                char[] chArray = input.ToCharArray();
                char[] chArray2 = new char[chArray.Length];
                Array.Sort<char>(toFilter);
                for (int i = 0; i < chArray.Length; i++)
                {
                    if (Array.BinarySearch<char>(toFilter, chArray[i]) < 0)
                    {
                        chArray2[length++] = chArray[i];
                        flag2 = true;
                        if (flag)
                        {
                            flag = false;
                        }
                    }
                    else if (fMultiReplace || !flag)
                    {
                        chArray2[length++] = chReplacement;
                        flag = true;
                    }
                }
                if (flag2 && (length > 0))
                {
                    return new string(chArray2, 0, length);
                }
            }
            return null;
        }

        private static char[] InvalidFileDirNameChars
        {
            get
            {
                if (_invalidFileDirNameChars == null)
                {
                    Interlocked.CompareExchange(ref _invalidFileDirNameChars, Path.GetInvalidFileNameChars(), null);
                }
                return (char[]) _invalidFileDirNameChars;
            }
        }
    }
}

