using System;

namespace EmotionIsland.Helpers
{
    public static class FloatHelper
    {
        #region Constants

        private const float FAST_SPEED = 40;
        private const float MEDIUM_SPEED = 10;
        private const float SLOW_SPEED = 5;

        private const int FAST_INTERVAL = 30;
        private const int MEDIUM_INTERVAL = 45;
        private const int SLOW_INTERVAL = 60;

        private const string FAST_STRING = "Fast";
        private const string MEDIUM_STRING = "Medium";
        private const string SLOW_STRING = "Slow";
        #endregion

        public static float ParseSpeedString(String speed)
        {
            switch (speed)
            {
                case FAST_STRING:
                    return FAST_SPEED;
                case MEDIUM_STRING:
                    return MEDIUM_SPEED;
                case SLOW_STRING:
                    return SLOW_SPEED;
                default:
                    throw new InvalidOperationException("I don't know this speed you're specifying.");
            }
        }

        public static int RandomSign()
        {
            Random signRandom = new Random();
            return signRandom.Next(2) == 1 ? 1 : -1;
        }

        public static int ParseIntervalString(String speed)
        {
            switch (speed)
            {
                case FAST_STRING:
                    return FAST_INTERVAL;
                case MEDIUM_STRING:
                    return MEDIUM_INTERVAL;
                case SLOW_STRING:
                    return SLOW_INTERVAL;
                default:
                    throw new InvalidOperationException("I don't know this generator interval you're specifying.");
            }
        }
    }
}
