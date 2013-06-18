namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReplicationCursorCollection : ReadOnlyCollectionBase
    {
        private DirectoryServer server;

        internal ReplicationCursorCollection(DirectoryServer server)
        {
            this.server = server;
        }

        private int Add(ReplicationCursor cursor)
        {
            return base.InnerList.Add(cursor);
        }

        internal void AddHelper(string partition, object cursors, bool advanced, IntPtr info)
        {
            int cNumCursors = 0;
            if (advanced)
            {
                cNumCursors = ((DS_REPL_CURSORS_3) cursors).cNumCursors;
            }
            else
            {
                cNumCursors = ((DS_REPL_CURSORS) cursors).cNumCursors;
            }
            IntPtr zero = IntPtr.Zero;
            for (int i = 0; i < cNumCursors; i++)
            {
                if (advanced)
                {
                    zero = (IntPtr) ((((long) info) + (Marshal.SizeOf(typeof(int)) * 2)) + (i * Marshal.SizeOf(typeof(DS_REPL_CURSOR_3))));
                    DS_REPL_CURSOR_3 structure = new DS_REPL_CURSOR_3();
                    Marshal.PtrToStructure(zero, structure);
                    ReplicationCursor cursor = new ReplicationCursor(this.server, partition, structure.uuidSourceDsaInvocationID, structure.usnAttributeFilter, structure.ftimeLastSyncSuccess, structure.pszSourceDsaDN);
                    this.Add(cursor);
                }
                else
                {
                    zero = (IntPtr) ((((long) info) + (Marshal.SizeOf(typeof(int)) * 2)) + (i * Marshal.SizeOf(typeof(DS_REPL_CURSOR))));
                    DS_REPL_CURSOR ds_repl_cursor = new DS_REPL_CURSOR();
                    Marshal.PtrToStructure(zero, ds_repl_cursor);
                    ReplicationCursor cursor2 = new ReplicationCursor(this.server, partition, ds_repl_cursor.uuidSourceDsaInvocationID, ds_repl_cursor.usnAttributeFilter);
                    this.Add(cursor2);
                }
            }
        }

        public bool Contains(ReplicationCursor cursor)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return base.InnerList.Contains(cursor);
        }

        public void CopyTo(ReplicationCursor[] values, int index)
        {
            base.InnerList.CopyTo(values, index);
        }

        public int IndexOf(ReplicationCursor cursor)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return base.InnerList.IndexOf(cursor);
        }

        public ReplicationCursor this[int index]
        {
            get
            {
                return (ReplicationCursor) base.InnerList[index];
            }
        }
    }
}

