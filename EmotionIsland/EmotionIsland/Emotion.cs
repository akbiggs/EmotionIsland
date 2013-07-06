namespace EmotionIsland
{
    public class Emotion
    {
        private const float EMOTION_THRESHOLD = 0.5f;

        // range -1 to 1 depending on level of emotion
        private float happyLevel = 0;
        private float angerLevel = 0;

        public EmotionType EmotionType
        {
            get
            {
                if (happyLevel < -EMOTION_THRESHOLD)
                {
                    return EmotionType.Sad;
                }

                if (angerLevel < -EMOTION_THRESHOLD)
                {
                    return EmotionType.Terrified;
                }

                if (happyLevel > EMOTION_THRESHOLD)
                {
                    return EmotionType.Happy;
                }

                if (angerLevel > EMOTION_THRESHOLD)
                {
                    return EmotionType.Angry;
                }

                return EmotionType.Neutral;
            }
        }

        public Emotion(EmotionType emotion)
        {
            if (emotion == EmotionType.Angry)
            {
                angerLevel = 1;
            }
            else if (emotion == EmotionType.Happy)
            {
                happyLevel = 1;
            }
            else if (emotion == EmotionType.Terrified)
            {
                angerLevel = -1;
            }
            else if (emotion == EmotionType.Sad)
            {
                happyLevel = -1;
            }
        }
    }

    public enum EmotionType
    {
        Neutral,
        Angry,
        Terrified,
        Happy,
        Sad
    }
}