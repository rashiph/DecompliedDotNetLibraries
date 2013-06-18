namespace System.Data.Design
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    internal class TableAdapterManagerHelper
    {
        internal static DataRelation[] GetSelfRefRelations(DataTable dataTable)
        {
            List<DataRelation> list = new List<DataRelation>();
            List<DataRelation> list2 = new List<DataRelation>();
            foreach (DataRelation relation in dataTable.ParentRelations)
            {
                if (relation.ChildTable == relation.ParentTable)
                {
                    list.Add(relation);
                    if (relation.ChildKeyConstraint != null)
                    {
                        list2.Add(relation);
                    }
                }
            }
            if (list2.Count > 0)
            {
                return list2.ToArray();
            }
            return list.ToArray();
        }

        internal static DataTable[] GetUpdateOrder(DataSet ds)
        {
            HierarchicalObject[] array = new HierarchicalObject[ds.Tables.Count];
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                DataTable theObject = ds.Tables[i];
                array[i] = new HierarchicalObject(theObject);
            }
            for (int j = 0; j < array.Length; j++)
            {
                DataTable objB = array[j].TheObject as DataTable;
                foreach (Constraint constraint in objB.Constraints)
                {
                    ForeignKeyConstraint constraint2 = constraint as ForeignKeyConstraint;
                    if ((constraint2 != null) && !object.ReferenceEquals(constraint2.RelatedTable, objB))
                    {
                        int index = ds.Tables.IndexOf(constraint2.RelatedTable);
                        array[j].AddUniqueParent(array[index]);
                    }
                }
                foreach (DataRelation relation in objB.ParentRelations)
                {
                    if (!object.ReferenceEquals(relation.ParentTable, objB))
                    {
                        int num4 = ds.Tables.IndexOf(relation.ParentTable);
                        array[j].AddUniqueParent(array[num4]);
                    }
                }
            }
            for (int k = 0; k < array.Length; k++)
            {
                HierarchicalObject obj3 = array[k];
                if (obj3.HasParent)
                {
                    obj3.CheckParents();
                }
            }
            DataTable[] tableArray = new DataTable[array.Length];
            Array.Sort<HierarchicalObject>(array);
            for (int m = 0; m < array.Length; m++)
            {
                HierarchicalObject obj4 = array[m];
                tableArray[m] = (DataTable) obj4.TheObject;
            }
            return tableArray;
        }

        internal class HierarchicalObject : IComparable<TableAdapterManagerHelper.HierarchicalObject>
        {
            internal int Height;
            private List<TableAdapterManagerHelper.HierarchicalObject> parents;
            internal object TheObject;

            internal HierarchicalObject(object theObject)
            {
                this.TheObject = theObject;
            }

            internal void AddUniqueParent(TableAdapterManagerHelper.HierarchicalObject parent)
            {
                if (!this.Parents.Contains(parent))
                {
                    this.Parents.Add(parent);
                }
            }

            internal void CheckParents()
            {
                if (this.HasParent)
                {
                    Stack<TableAdapterManagerHelper.HierarchicalObject> path = new Stack<TableAdapterManagerHelper.HierarchicalObject>();
                    Stack<TableAdapterManagerHelper.HierarchicalObject> work = new Stack<TableAdapterManagerHelper.HierarchicalObject>();
                    work.Push(this);
                    path.Push(this);
                    this.CheckParents(work, path);
                }
            }

            internal void CheckParents(Stack<TableAdapterManagerHelper.HierarchicalObject> work, Stack<TableAdapterManagerHelper.HierarchicalObject> path)
            {
                if (!this.HasParent || (!object.ReferenceEquals(this, path.Peek()) && path.Contains(this)))
                {
                    TableAdapterManagerHelper.HierarchicalObject objA = path.Pop();
                    TableAdapterManagerHelper.HierarchicalObject objB = work.Pop();
                    while (((work.Count > 0) && (path.Count > 0)) && object.ReferenceEquals(objA, objB))
                    {
                        objA = path.Pop();
                        objB = work.Pop();
                    }
                    if (objB != objA)
                    {
                        path.Push(objB);
                        objB.CheckParents(work, path);
                    }
                }
                else if (this.HasParent)
                {
                    TableAdapterManagerHelper.HierarchicalObject item = null;
                    for (int i = this.Parents.Count - 1; i >= 0; i--)
                    {
                        TableAdapterManagerHelper.HierarchicalObject obj5 = this.Parents[i];
                        if (!path.Contains(obj5) && (obj5.Height <= this.Height))
                        {
                            obj5.Height = this.Height + 1;
                            if (obj5.Height > 0x3e8)
                            {
                                return;
                            }
                            work.Push(obj5);
                            item = obj5;
                        }
                    }
                    if (item != null)
                    {
                        path.Push(item);
                        item.CheckParents(work, path);
                    }
                }
            }

            int IComparable<TableAdapterManagerHelper.HierarchicalObject>.CompareTo(TableAdapterManagerHelper.HierarchicalObject other)
            {
                return (other.Height - this.Height);
            }

            internal bool HasParent
            {
                get
                {
                    return ((this.parents != null) && (this.parents.Count > 0));
                }
            }

            internal List<TableAdapterManagerHelper.HierarchicalObject> Parents
            {
                get
                {
                    if (this.parents == null)
                    {
                        this.parents = new List<TableAdapterManagerHelper.HierarchicalObject>();
                    }
                    return this.parents;
                }
            }
        }
    }
}

