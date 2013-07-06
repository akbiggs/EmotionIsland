using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public class Emotion
    {
        private static Dictionary<EmotionType, Color> colorMappings = new Dictionary<EmotionType, Color>
            {
                {EmotionType.Happy, Color.Yellow},
                {EmotionType.Sad, Color.Blue},
                {EmotionType.Angry, Color.Red},
                {EmotionType.Terrified, Color.DarkGreen},
                {EmotionType.Neutral, Color.Gray}
            };

        private const float EMOTION_THRESHOLD = 0.5f;
        private const float EMOTION_INCREMENT = 0.008f;

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

        public Color ToColor()
        {
            return colorMappings[this.EmotionType];
        }

        public void IncreaseAnger()
        {
            this.angerLevel += EMOTION_INCREMENT;
            this.angerLevel = MathHelper.Clamp(this.angerLevel, -1, 1);
        }

        public void IncreaseTerror()
        {
            this.angerLevel -= EMOTION_INCREMENT;
            this.angerLevel = MathHelper.Clamp(this.angerLevel, -1, 1);
        }

        public void IncreaseHappiness()
        {
            this.happyLevel += EMOTION_INCREMENT;
            this.happyLevel = MathHelper.Clamp(this.happyLevel, -1, 1);
            
        }

        public void IncreaseSadness()
        {
            this.happyLevel -= EMOTION_INCREMENT;
            this.happyLevel = MathHelper.Clamp(this.happyLevel, -1, 1);
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