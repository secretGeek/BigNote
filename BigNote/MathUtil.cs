namespace BigNote
{
    using System;

    public class MathUtil
    {
        public static bool IsZero(double value)
        {
            return Math.Abs(value) < 2.2204460492503131E-15;
        }
    }
}
