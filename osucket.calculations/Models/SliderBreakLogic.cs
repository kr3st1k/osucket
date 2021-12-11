namespace osucket.Calculations.Models
{
    internal static class SliderBreaks
    {
        public static int SliderBreaksCount { get; set; }

        private static ushort LastCombo { get; set; }
        private static ushort LastMissCount { get; set; }

        public static void ClearValues()
        {
            LastCombo = 0;
            SliderBreaksCount = 0;
            LastMissCount = 0;
        }
        public static void GetSliderBreaks(ushort missCount, ushort combo)
        {
            if (LastMissCount == missCount && LastCombo > combo)
                SliderBreaksCount++;

            LastMissCount = missCount;
            LastCombo = combo;
        }
    }
}