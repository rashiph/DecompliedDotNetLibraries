namespace System.Drawing.Printing
{
    using System;
    using System.Drawing;

    public sealed class PrinterUnitConvert
    {
        private PrinterUnitConvert()
        {
        }

        public static double Convert(double value, PrinterUnit fromUnit, PrinterUnit toUnit)
        {
            double num = UnitsPerDisplay(fromUnit);
            double num2 = UnitsPerDisplay(toUnit);
            return ((value * num2) / num);
        }

        public static Point Convert(Point value, PrinterUnit fromUnit, PrinterUnit toUnit)
        {
            return new Point(Convert(value.X, fromUnit, toUnit), Convert(value.Y, fromUnit, toUnit));
        }

        public static Margins Convert(Margins value, PrinterUnit fromUnit, PrinterUnit toUnit)
        {
            return new Margins { DoubleLeft = Convert(value.DoubleLeft, fromUnit, toUnit), DoubleRight = Convert(value.DoubleRight, fromUnit, toUnit), DoubleTop = Convert(value.DoubleTop, fromUnit, toUnit), DoubleBottom = Convert(value.DoubleBottom, fromUnit, toUnit) };
        }

        public static Rectangle Convert(Rectangle value, PrinterUnit fromUnit, PrinterUnit toUnit)
        {
            return new Rectangle(Convert(value.X, fromUnit, toUnit), Convert(value.Y, fromUnit, toUnit), Convert(value.Width, fromUnit, toUnit), Convert(value.Height, fromUnit, toUnit));
        }

        public static Size Convert(Size value, PrinterUnit fromUnit, PrinterUnit toUnit)
        {
            return new Size(Convert(value.Width, fromUnit, toUnit), Convert(value.Height, fromUnit, toUnit));
        }

        public static int Convert(int value, PrinterUnit fromUnit, PrinterUnit toUnit)
        {
            return (int) Math.Round(Convert((double) value, fromUnit, toUnit));
        }

        private static double UnitsPerDisplay(PrinterUnit unit)
        {
            switch (unit)
            {
                case PrinterUnit.Display:
                    return 1.0;

                case PrinterUnit.ThousandthsOfAnInch:
                    return 10.0;

                case PrinterUnit.HundredthsOfAMillimeter:
                    return 25.4;

                case PrinterUnit.TenthsOfAMillimeter:
                    return 2.54;
            }
            return 1.0;
        }
    }
}

