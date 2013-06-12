namespace System.Globalization
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal static class EncodingTable
    {
        internal unsafe static InternalCodePageDataItem* codePageDataPtr = GetCodePageData();
        internal unsafe static InternalEncodingDataItem* encodingDataPtr = GetEncodingData();
        private static Hashtable hashByCodePage = Hashtable.Synchronized(new Hashtable());
        private static Hashtable hashByName = Hashtable.Synchronized(new Hashtable(StringComparer.OrdinalIgnoreCase));
        private static int lastCodePageItem;
        private static int lastEncodingItem = (GetNumEncodingItems() - 1);

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe InternalCodePageDataItem* GetCodePageData();
        [SecuritySafeCritical]
        internal static unsafe CodePageDataItem GetCodePageDataItem(int codepage)
        {
            int num2;
            CodePageDataItem item = (CodePageDataItem) hashByCodePage[codepage];
            if (item != null)
            {
                return item;
            }
            for (int i = 0; (num2 = codePageDataPtr[i].codePage) != 0; i++)
            {
                if (num2 == codepage)
                {
                    item = new CodePageDataItem(i);
                    hashByCodePage[codepage] = item;
                    return item;
                }
            }
            return null;
        }

        internal static int GetCodePageFromName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            object obj2 = hashByName[name];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            int num = internalGetCodePageFromName(name);
            hashByName[name] = num;
            return num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe InternalEncodingDataItem* GetEncodingData();
        [SecuritySafeCritical]
        internal static unsafe EncodingInfo[] GetEncodings()
        {
            if (lastCodePageItem == 0)
            {
                int index = 0;
                while (codePageDataPtr[index].codePage != 0)
                {
                    index++;
                }
                lastCodePageItem = index;
            }
            EncodingInfo[] infoArray = new EncodingInfo[lastCodePageItem];
            for (int i = 0; i < lastCodePageItem; i++)
            {
                infoArray[i] = new EncodingInfo(codePageDataPtr[i].codePage, new string(codePageDataPtr[i].webName), Environment.GetResourceString("Globalization.cp_" + codePageDataPtr[i].codePage));
            }
            return infoArray;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int GetNumEncodingItems();
        [SecuritySafeCritical]
        private static unsafe int internalGetCodePageFromName(string name)
        {
            int index = 0;
            int lastEncodingItem = EncodingTable.lastEncodingItem;
            while ((lastEncodingItem - index) > 3)
            {
                int num3 = ((lastEncodingItem - index) / 2) + index;
                int num4 = string.nativeCompareOrdinalIgnoreCaseWC(name, encodingDataPtr[num3].webName);
                if (num4 == 0)
                {
                    return encodingDataPtr[num3].codePage;
                }
                if (num4 < 0)
                {
                    lastEncodingItem = num3;
                }
                else
                {
                    index = num3;
                }
            }
            while (index <= lastEncodingItem)
            {
                if (string.nativeCompareOrdinalIgnoreCaseWC(name, encodingDataPtr[index].webName) == 0)
                {
                    return encodingDataPtr[index].codePage;
                }
                index++;
            }
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_EncodingNotSupported"), new object[] { name }), "name");
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe byte* nativeCreateOpenFileMapping(string inSectionName, int inBytesToAllocate, out IntPtr mappedFileHandle);
    }
}

