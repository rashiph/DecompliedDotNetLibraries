namespace System.Drawing.Imaging
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class ColorMatrix
    {
        private float matrix00;
        private float matrix01;
        private float matrix02;
        private float matrix03;
        private float matrix04;
        private float matrix10;
        private float matrix11;
        private float matrix12;
        private float matrix13;
        private float matrix14;
        private float matrix20;
        private float matrix21;
        private float matrix22;
        private float matrix23;
        private float matrix24;
        private float matrix30;
        private float matrix31;
        private float matrix32;
        private float matrix33;
        private float matrix34;
        private float matrix40;
        private float matrix41;
        private float matrix42;
        private float matrix43;
        private float matrix44;
        public ColorMatrix()
        {
            this.matrix00 = 1f;
            this.matrix11 = 1f;
            this.matrix22 = 1f;
            this.matrix33 = 1f;
            this.matrix44 = 1f;
        }

        public float Matrix00
        {
            get
            {
                return this.matrix00;
            }
            set
            {
                this.matrix00 = value;
            }
        }
        public float Matrix01
        {
            get
            {
                return this.matrix01;
            }
            set
            {
                this.matrix01 = value;
            }
        }
        public float Matrix02
        {
            get
            {
                return this.matrix02;
            }
            set
            {
                this.matrix02 = value;
            }
        }
        public float Matrix03
        {
            get
            {
                return this.matrix03;
            }
            set
            {
                this.matrix03 = value;
            }
        }
        public float Matrix04
        {
            get
            {
                return this.matrix04;
            }
            set
            {
                this.matrix04 = value;
            }
        }
        public float Matrix10
        {
            get
            {
                return this.matrix10;
            }
            set
            {
                this.matrix10 = value;
            }
        }
        public float Matrix11
        {
            get
            {
                return this.matrix11;
            }
            set
            {
                this.matrix11 = value;
            }
        }
        public float Matrix12
        {
            get
            {
                return this.matrix12;
            }
            set
            {
                this.matrix12 = value;
            }
        }
        public float Matrix13
        {
            get
            {
                return this.matrix13;
            }
            set
            {
                this.matrix13 = value;
            }
        }
        public float Matrix14
        {
            get
            {
                return this.matrix14;
            }
            set
            {
                this.matrix14 = value;
            }
        }
        public float Matrix20
        {
            get
            {
                return this.matrix20;
            }
            set
            {
                this.matrix20 = value;
            }
        }
        public float Matrix21
        {
            get
            {
                return this.matrix21;
            }
            set
            {
                this.matrix21 = value;
            }
        }
        public float Matrix22
        {
            get
            {
                return this.matrix22;
            }
            set
            {
                this.matrix22 = value;
            }
        }
        public float Matrix23
        {
            get
            {
                return this.matrix23;
            }
            set
            {
                this.matrix23 = value;
            }
        }
        public float Matrix24
        {
            get
            {
                return this.matrix24;
            }
            set
            {
                this.matrix24 = value;
            }
        }
        public float Matrix30
        {
            get
            {
                return this.matrix30;
            }
            set
            {
                this.matrix30 = value;
            }
        }
        public float Matrix31
        {
            get
            {
                return this.matrix31;
            }
            set
            {
                this.matrix31 = value;
            }
        }
        public float Matrix32
        {
            get
            {
                return this.matrix32;
            }
            set
            {
                this.matrix32 = value;
            }
        }
        public float Matrix33
        {
            get
            {
                return this.matrix33;
            }
            set
            {
                this.matrix33 = value;
            }
        }
        public float Matrix34
        {
            get
            {
                return this.matrix34;
            }
            set
            {
                this.matrix34 = value;
            }
        }
        public float Matrix40
        {
            get
            {
                return this.matrix40;
            }
            set
            {
                this.matrix40 = value;
            }
        }
        public float Matrix41
        {
            get
            {
                return this.matrix41;
            }
            set
            {
                this.matrix41 = value;
            }
        }
        public float Matrix42
        {
            get
            {
                return this.matrix42;
            }
            set
            {
                this.matrix42 = value;
            }
        }
        public float Matrix43
        {
            get
            {
                return this.matrix43;
            }
            set
            {
                this.matrix43 = value;
            }
        }
        public float Matrix44
        {
            get
            {
                return this.matrix44;
            }
            set
            {
                this.matrix44 = value;
            }
        }
        [CLSCompliant(false)]
        public ColorMatrix(float[][] newColorMatrix)
        {
            this.SetMatrix(newColorMatrix);
        }

        internal void SetMatrix(float[][] newColorMatrix)
        {
            this.matrix00 = newColorMatrix[0][0];
            this.matrix01 = newColorMatrix[0][1];
            this.matrix02 = newColorMatrix[0][2];
            this.matrix03 = newColorMatrix[0][3];
            this.matrix04 = newColorMatrix[0][4];
            this.matrix10 = newColorMatrix[1][0];
            this.matrix11 = newColorMatrix[1][1];
            this.matrix12 = newColorMatrix[1][2];
            this.matrix13 = newColorMatrix[1][3];
            this.matrix14 = newColorMatrix[1][4];
            this.matrix20 = newColorMatrix[2][0];
            this.matrix21 = newColorMatrix[2][1];
            this.matrix22 = newColorMatrix[2][2];
            this.matrix23 = newColorMatrix[2][3];
            this.matrix24 = newColorMatrix[2][4];
            this.matrix30 = newColorMatrix[3][0];
            this.matrix31 = newColorMatrix[3][1];
            this.matrix32 = newColorMatrix[3][2];
            this.matrix33 = newColorMatrix[3][3];
            this.matrix34 = newColorMatrix[3][4];
            this.matrix40 = newColorMatrix[4][0];
            this.matrix41 = newColorMatrix[4][1];
            this.matrix42 = newColorMatrix[4][2];
            this.matrix43 = newColorMatrix[4][3];
            this.matrix44 = newColorMatrix[4][4];
        }

        internal float[][] GetMatrix()
        {
            float[][] numArray = new float[5][];
            for (int i = 0; i < 5; i++)
            {
                numArray[i] = new float[5];
            }
            numArray[0][0] = this.matrix00;
            numArray[0][1] = this.matrix01;
            numArray[0][2] = this.matrix02;
            numArray[0][3] = this.matrix03;
            numArray[0][4] = this.matrix04;
            numArray[1][0] = this.matrix10;
            numArray[1][1] = this.matrix11;
            numArray[1][2] = this.matrix12;
            numArray[1][3] = this.matrix13;
            numArray[1][4] = this.matrix14;
            numArray[2][0] = this.matrix20;
            numArray[2][1] = this.matrix21;
            numArray[2][2] = this.matrix22;
            numArray[2][3] = this.matrix23;
            numArray[2][4] = this.matrix24;
            numArray[3][0] = this.matrix30;
            numArray[3][1] = this.matrix31;
            numArray[3][2] = this.matrix32;
            numArray[3][3] = this.matrix33;
            numArray[3][4] = this.matrix34;
            numArray[4][0] = this.matrix40;
            numArray[4][1] = this.matrix41;
            numArray[4][2] = this.matrix42;
            numArray[4][3] = this.matrix43;
            numArray[4][4] = this.matrix44;
            return numArray;
        }

        public float this[int row, int column]
        {
            get
            {
                return this.GetMatrix()[row][column];
            }
            set
            {
                float[][] matrix = this.GetMatrix();
                matrix[row][column] = value;
                this.SetMatrix(matrix);
            }
        }
    }
}

