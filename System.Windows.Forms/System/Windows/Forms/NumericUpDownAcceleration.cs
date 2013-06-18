namespace System.Windows.Forms
{
    using System;

    public class NumericUpDownAcceleration
    {
        private decimal increment;
        private int seconds;

        public NumericUpDownAcceleration(int seconds, decimal increment)
        {
            if (seconds < 0)
            {
                throw new ArgumentOutOfRangeException("seconds", seconds, System.Windows.Forms.SR.GetString("NumericUpDownLessThanZeroError"));
            }
            if (increment < 0M)
            {
                throw new ArgumentOutOfRangeException("increment", increment, System.Windows.Forms.SR.GetString("NumericUpDownLessThanZeroError"));
            }
            this.seconds = seconds;
            this.increment = increment;
        }

        public decimal Increment
        {
            get
            {
                return this.increment;
            }
            set
            {
                if (value < 0M)
                {
                    throw new ArgumentOutOfRangeException("increment", value, System.Windows.Forms.SR.GetString("NumericUpDownLessThanZeroError"));
                }
                this.increment = value;
            }
        }

        public int Seconds
        {
            get
            {
                return this.seconds;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("seconds", value, System.Windows.Forms.SR.GetString("NumericUpDownLessThanZeroError"));
                }
                this.seconds = value;
            }
        }
    }
}

