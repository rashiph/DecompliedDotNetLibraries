namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class CorrelationDataSourceHelper : ICorrelationDataSource
    {
        private ICollection<CorrelationDataDescription> dataSources;

        public CorrelationDataSourceHelper(ICollection<CorrelationDataDescription> dataSources)
        {
            if (dataSources.IsReadOnly)
            {
                this.dataSources = dataSources;
            }
            else
            {
                this.dataSources = new ReadOnlyCollection<CorrelationDataDescription>(new List<CorrelationDataDescription>(dataSources));
            }
        }

        private CorrelationDataSourceHelper(ICollection<CorrelationDataDescription> dataSource1, ICollection<CorrelationDataDescription> dataSource2)
        {
            List<CorrelationDataDescription> list = new List<CorrelationDataDescription>(dataSource1);
            foreach (CorrelationDataDescription description in dataSource2)
            {
                list.Add(description);
            }
            this.dataSources = new ReadOnlyCollection<CorrelationDataDescription>(list);
        }

        public static ICorrelationDataSource Combine(ICorrelationDataSource dataSource1, ICorrelationDataSource dataSource2)
        {
            if (dataSource1 == null)
            {
                return dataSource2;
            }
            if (dataSource2 == null)
            {
                return dataSource1;
            }
            return new CorrelationDataSourceHelper(dataSource1.DataSources, dataSource2.DataSources);
        }

        ICollection<CorrelationDataDescription> ICorrelationDataSource.DataSources
        {
            get
            {
                return this.dataSources;
            }
        }
    }
}

