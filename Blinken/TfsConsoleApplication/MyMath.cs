namespace TfsConsoleApplication
{
    public static class MyMath
    {
        /// <param name="value">[0.0 - 1.0]</param>
        public static double Lerp(double value, double min, double max)
        {
            double diff = max - min;
            double scaledValue = diff * value + min;
            return scaledValue;
        }

        /// <param name="value">[sourceMin - sourceMax]</param>
        public static double Map(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            double sourceProgress = (value - sourceMin) / (sourceMax - sourceMin);
            double targetProgress = Lerp(sourceProgress, targetMin, targetMax);
            return targetProgress;
        }
    }
}
