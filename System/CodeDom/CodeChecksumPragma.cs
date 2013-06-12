namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeChecksumPragma : CodeDirective
    {
        private Guid checksumAlgorithmId;
        private byte[] checksumData;
        private string fileName;

        public CodeChecksumPragma()
        {
        }

        public CodeChecksumPragma(string fileName, Guid checksumAlgorithmId, byte[] checksumData)
        {
            this.fileName = fileName;
            this.checksumAlgorithmId = checksumAlgorithmId;
            this.checksumData = checksumData;
        }

        public Guid ChecksumAlgorithmId
        {
            get
            {
                return this.checksumAlgorithmId;
            }
            set
            {
                this.checksumAlgorithmId = value;
            }
        }

        public byte[] ChecksumData
        {
            get
            {
                return this.checksumData;
            }
            set
            {
                this.checksumData = value;
            }
        }

        public string FileName
        {
            get
            {
                if (this.fileName != null)
                {
                    return this.fileName;
                }
                return string.Empty;
            }
            set
            {
                this.fileName = value;
            }
        }
    }
}

