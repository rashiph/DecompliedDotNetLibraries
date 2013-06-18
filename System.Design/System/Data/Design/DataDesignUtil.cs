namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;

    internal sealed class DataDesignUtil
    {
        internal static string DataSetClassName = typeof(DataSet).ToString();

        private DataDesignUtil()
        {
        }

        public static DataColumn CloneColumn(DataColumn column)
        {
            DataColumn destColumn = new DataColumn();
            CopyColumn(column, destColumn);
            return destColumn;
        }

        public static void CopyColumn(DataColumn srcColumn, DataColumn destColumn)
        {
            destColumn.AllowDBNull = srcColumn.AllowDBNull;
            destColumn.AutoIncrement = srcColumn.AutoIncrement;
            destColumn.AutoIncrementSeed = srcColumn.AutoIncrementSeed;
            destColumn.AutoIncrementStep = srcColumn.AutoIncrementStep;
            destColumn.Caption = srcColumn.Caption;
            destColumn.ColumnMapping = srcColumn.ColumnMapping;
            destColumn.ColumnName = srcColumn.ColumnName;
            destColumn.DataType = srcColumn.DataType;
            destColumn.DefaultValue = srcColumn.DefaultValue;
            destColumn.Expression = srcColumn.Expression;
            destColumn.MaxLength = srcColumn.MaxLength;
            destColumn.Prefix = srcColumn.Prefix;
            destColumn.ReadOnly = srcColumn.ReadOnly;
        }

        internal static string[] MapColumnNames(DataColumnMappingCollection mappingCollection, string[] names, MappingDirection direction)
        {
            if ((mappingCollection == null) || (names == null))
            {
                return new string[0];
            }
            ArrayList list = new ArrayList();
            foreach (string str2 in names)
            {
                string sourceColumn;
                try
                {
                    if (direction == MappingDirection.DataSetToSource)
                    {
                        sourceColumn = mappingCollection.GetByDataSetColumn(str2).SourceColumn;
                    }
                    else
                    {
                        DataColumnMapping mapping = mappingCollection[str2];
                        sourceColumn = mapping.DataSetColumn;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    sourceColumn = str2;
                }
                list.Add(sourceColumn);
            }
            return (string[]) list.ToArray(typeof(string));
        }

        internal enum MappingDirection
        {
            SourceToDataSet,
            DataSetToSource
        }
    }
}

