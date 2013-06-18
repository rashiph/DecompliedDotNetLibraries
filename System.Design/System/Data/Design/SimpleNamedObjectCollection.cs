namespace System.Data.Design
{
    using System.Collections;

    internal class SimpleNamedObjectCollection : ArrayList, INamedObjectCollection, ICollection, IEnumerable
    {
        private static SimpleNameService myNameService;

        public INameService GetNameService()
        {
            return this.NameService;
        }

        protected virtual INameService NameService
        {
            get
            {
                if (myNameService == null)
                {
                    myNameService = new SimpleNameService();
                }
                return myNameService;
            }
        }
    }
}

