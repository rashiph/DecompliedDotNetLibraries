namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public sealed class ObjectStatementCollection : IEnumerable
    {
        private List<TableEntry> _table;
        private int _version;

        internal ObjectStatementCollection()
        {
        }

        private void AddOwner(object statementOwner, CodeStatementCollection statements)
        {
            if (this._table == null)
            {
                this._table = new List<TableEntry>();
            }
            else
            {
                for (int i = 0; i < this._table.Count; i++)
                {
                    if (object.ReferenceEquals(this._table[i].Owner, statementOwner))
                    {
                        if (this._table[i].Statements != null)
                        {
                            throw new InvalidOperationException();
                        }
                        if (statements != null)
                        {
                            this._table[i] = new TableEntry(statementOwner, statements);
                        }
                        return;
                    }
                }
            }
            this._table.Add(new TableEntry(statementOwner, statements));
            this._version++;
        }

        public bool ContainsKey(object statementOwner)
        {
            if (statementOwner == null)
            {
                throw new ArgumentNullException("statementOwner");
            }
            return ((this._table != null) && (this[statementOwner] != null));
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new TableEnumerator(this);
        }

        public void Populate(ICollection statementOwners)
        {
            if (statementOwners == null)
            {
                throw new ArgumentNullException("statementOwners");
            }
            foreach (object obj2 in statementOwners)
            {
                this.Populate(obj2);
            }
        }

        public void Populate(object owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this.AddOwner(owner, null);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public CodeStatementCollection this[object statementOwner]
        {
            get
            {
                if (statementOwner == null)
                {
                    throw new ArgumentNullException("statementOwner");
                }
                if (this._table != null)
                {
                    for (int i = 0; i < this._table.Count; i++)
                    {
                        if (object.ReferenceEquals(this._table[i].Owner, statementOwner))
                        {
                            if (this._table[i].Statements == null)
                            {
                                this._table[i] = new TableEntry(statementOwner, new CodeStatementCollection());
                            }
                            return this._table[i].Statements;
                        }
                    }
                    foreach (TableEntry entry in this._table)
                    {
                        if (object.ReferenceEquals(entry.Owner, statementOwner))
                        {
                            return entry.Statements;
                        }
                    }
                }
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TableEntry
        {
            public object Owner;
            public CodeStatementCollection Statements;
            public TableEntry(object owner, CodeStatementCollection statements)
            {
                this.Owner = owner;
                this.Statements = statements;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TableEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private ObjectStatementCollection _table;
            private int _version;
            private int _position;
            public TableEnumerator(ObjectStatementCollection table)
            {
                this._table = table;
                this._version = this._table._version;
                this._position = -1;
            }

            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }
            public DictionaryEntry Entry
            {
                get
                {
                    if (this._version != this._table._version)
                    {
                        throw new InvalidOperationException();
                    }
                    if (((this._position < 0) || (this._table._table == null)) || (this._position >= this._table._table.Count))
                    {
                        throw new InvalidOperationException();
                    }
                    if (this._table._table[this._position].Statements == null)
                    {
                        this._table._table[this._position] = new ObjectStatementCollection.TableEntry(this._table._table[this._position].Owner, new CodeStatementCollection());
                    }
                    ObjectStatementCollection.TableEntry entry = this._table._table[this._position];
                    return new DictionaryEntry(entry.Owner, entry.Statements);
                }
            }
            public object Key
            {
                get
                {
                    return this.Entry.Key;
                }
            }
            public object Value
            {
                get
                {
                    return this.Entry.Value;
                }
            }
            public bool MoveNext()
            {
                if ((this._table._table != null) && ((this._position + 1) < this._table._table.Count))
                {
                    this._position++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this._position = -1;
            }
        }
    }
}

