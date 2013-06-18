namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    internal class MSBuildNameIgnoreCaseComparer : EqualityComparer<string>
    {
        private string constraintString;
        private int endIndex;
        private bool immutable;
        private static MSBuildNameIgnoreCaseComparer immutableComparer = new MSBuildNameIgnoreCaseComparer(true);
        private object lockObject = new object();
        private static MSBuildNameIgnoreCaseComparer mutableComparer = new MSBuildNameIgnoreCaseComparer(false);
        private static ushort runningProcessorArchitecture = 0;
        private int startIndex;

        static MSBuildNameIgnoreCaseComparer()
        {
            NativeMethodsShared.SYSTEM_INFO lpSystemInfo = new NativeMethodsShared.SYSTEM_INFO();
            NativeMethodsShared.GetSystemInfo(ref lpSystemInfo);
            runningProcessorArchitecture = lpSystemInfo.wProcessorArchitecture;
        }

        private MSBuildNameIgnoreCaseComparer(bool immutable)
        {
            this.immutable = immutable;
        }

        public override bool Equals(string x, string y)
        {
            string str;
            string str2;
            int startIndex;
            int length;
            if ((x == null) && (y == null))
            {
                return true;
            }
            if ((x == null) || (y == null))
            {
                return false;
            }
            if (this.immutable)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return true;
                }
                str = x;
                str2 = y;
                startIndex = 0;
                length = y.Length;
            }
            else
            {
                lock (this.lockObject)
                {
                    if (this.constraintString != null)
                    {
                        bool flag = object.ReferenceEquals(x, this.constraintString);
                        bool flag2 = object.ReferenceEquals(y, this.constraintString);
                        if (!flag && !flag2)
                        {
                            ErrorUtilities.ThrowInternalError("Expected to compare to constraint", new object[0]);
                        }
                        str = flag ? y : x;
                        str2 = flag2 ? y : x;
                        startIndex = this.startIndex;
                        length = (this.endIndex - this.startIndex) + 1;
                    }
                    else
                    {
                        if (object.ReferenceEquals(x, y))
                        {
                            return true;
                        }
                        str = x;
                        str2 = y;
                        startIndex = 0;
                        length = y.Length;
                    }
                }
            }
            return Equals(str, str2, startIndex, length);
        }

        public static unsafe bool Equals(string compareToString, string constrainedString, int start, int lengthToCompare)
        {
            if (!object.ReferenceEquals(compareToString, constrainedString))
            {
                if ((compareToString == null) || (constrainedString == null))
                {
                    return false;
                }
                if (lengthToCompare != compareToString.Length)
                {
                    return false;
                }
                if (runningProcessorArchitecture == 6)
                {
                    return (string.Compare(compareToString, 0, constrainedString, start, lengthToCompare, StringComparison.OrdinalIgnoreCase) == 0);
                }
                fixed (char* str = ((char*) compareToString))
                {
                    char* chPtr = str;
                    fixed (char* str2 = ((char*) constrainedString))
                    {
                        char* chPtr2 = str2;
                        for (int i = 0; i < compareToString.Length; i++)
                        {
                            int num2 = chPtr[i];
                            int num3 = chPtr2[i + start];
                            num2 &= 0xdf;
                            num3 &= 0xdf;
                            if (num2 != num3)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public override unsafe int GetHashCode(string obj)
        {
            if (obj == null)
            {
                return 0;
            }
            int startIndex = 0;
            int length = obj.Length;
            if (!this.immutable)
            {
                lock (this.lockObject)
                {
                    if ((this.constraintString != null) && object.ReferenceEquals(obj, this.constraintString))
                    {
                        startIndex = this.startIndex;
                        length = (this.endIndex - this.startIndex) + 1;
                    }
                }
            }
            if (runningProcessorArchitecture == 6)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Substring(startIndex, length));
            }
            fixed (char* str = ((char*) obj))
            {
                char* chPtr = str;
                int num3 = 0x15051505;
                int num4 = num3;
                char* chPtr2 = chPtr + startIndex;
                int* numPtr = (int*) chPtr2;
                while (length > 0)
                {
                    int num5 = numPtr[0] & 0xdf00df;
                    if (length == 1)
                    {
                        num5 &= 0xffff;
                    }
                    num3 = (((num3 << 5) + num3) + (num3 >> 0x1b)) ^ num5;
                    if (length <= 2)
                    {
                        break;
                    }
                    num5 = numPtr[1] & 0xdf00df;
                    if (length == 3)
                    {
                        num5 &= 0xffff;
                    }
                    num4 = (((num4 << 5) + num4) + (num4 >> 0x1b)) ^ num5;
                    numPtr += 2;
                    length -= 4;
                }
                return (num3 + (num4 * 0x5d588b65));
            }
        }

        public T GetValueWithConstraints<T>(IDictionary<string, T> dictionary, string key, int startIndex, int endIndex) where T: class
        {
            T local;
            if (this.immutable)
            {
                ErrorUtilities.ThrowInternalError("immutable", new object[0]);
            }
            ErrorUtilities.VerifyThrowInternalNull(dictionary, "dictionary");
            if (startIndex < 0)
            {
                ErrorUtilities.ThrowInternalError("Invalid start index '{0}' {1} {2}", new object[] { key, startIndex, endIndex });
            }
            if ((key != null) && ((endIndex > key.Length) || (endIndex < startIndex)))
            {
                ErrorUtilities.ThrowInternalError("Invalid end index '{0}' {1} {2}", new object[] { key, startIndex, endIndex });
            }
            lock (this.lockObject)
            {
                this.constraintString = key;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
                try
                {
                    local = dictionary[key];
                }
                finally
                {
                    this.constraintString = null;
                    this.startIndex = 0;
                    this.endIndex = 0;
                }
            }
            return local;
        }

        internal void RemoveConstraintsForUnitTestingOnly()
        {
            if (this.immutable)
            {
                ErrorUtilities.ThrowInternalError("immutable", new object[0]);
            }
            lock (this.lockObject)
            {
                this.constraintString = null;
                this.startIndex = 0;
                this.endIndex = 0;
            }
        }

        internal void SetConstraintsForUnitTestingOnly(string constraintString, int startIndex, int endIndex)
        {
            if (this.immutable)
            {
                ErrorUtilities.ThrowInternalError("immutable", new object[0]);
            }
            if (startIndex < 0)
            {
                ErrorUtilities.ThrowInternalError("Invalid start index '{0}' {1} {2}", new object[] { constraintString, startIndex, endIndex });
            }
            if ((constraintString != null) && ((endIndex > constraintString.Length) || (endIndex < startIndex)))
            {
                ErrorUtilities.ThrowInternalError("Invalid end index '{0}' {1} {2}", new object[] { constraintString, startIndex, endIndex });
            }
            lock (this.lockObject)
            {
                this.constraintString = constraintString;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
            }
        }

        internal static MSBuildNameIgnoreCaseComparer Default
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return immutableComparer;
            }
        }

        internal static MSBuildNameIgnoreCaseComparer Mutable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return mutableComparer;
            }
        }
    }
}

