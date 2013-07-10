using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public class Emotion
    {
        public static EmotionType RandomEmotion()
        {
            int roll = MathExtra.RandomInt(0, 6);
            if (roll == 0) return EmotionType.Angry;
            if (roll == 1) return EmotionType.Sad;
            if (roll == 2) return EmotionType.Terrified;
            if (roll == 3) return EmotionType.Vigilant;
            if (roll == 4) return EmotionType.Hateful;

            return EmotionType.Neutral;
        }

        private static Dictionary<EmotionType, Color> colorMappings = new Dictionary<EmotionType, Color>
            {
                {EmotionType.Happy, Color.Yellow},
                {EmotionType.Sad, Color.Blue},
                {EmotionType.Angry, Color.Red},
                {EmotionType.Terrified, Color.DarkGreen},
                {EmotionType.Admirative, Color.LightGreen},
                {EmotionType.Amazed, Color.LightBlue},
                {EmotionType.Hateful, Color.Purple},
                {EmotionType.Vigilant, Color.Orange},
                {EmotionType.Neutral, Color.Gray}
            };

        private const float EMOTION_THRESHOLD = 0.5f;
        private const float EMOTION_INCREMENT = 0.15f;

        // range -1 to 1 depending on level of emotion
        private float happyLevel = 0;
        private float angerLevel = 0;

        public EmotionType EmotionType
        {
            get
            {
                if (happyLevel > EMOTION_THRESHOLD && angerLevel > EMOTION_THRESHOLD) return EmotionType.Vigilant;
                if (happyLevel > EMOTION_THRESHOLD && angerLevel < -EMOTION_THRESHOLD) return EmotionType.Admirative;
                if (happyLevel < -EMOTION_THRESHOLD && angerLevel > EMOTION_THRESHOLD) return EmotionType.Hateful;
                if (happyLevel < -EMOTION_THRESHOLD && angerLevel < -EMOTION_THRESHOLD) return EmotionType.Amazed;
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
            else if (emotion == EmotionType.Vigilant)
            {
                happyLevel = 1;
                angerLevel = 1;
            }
            else if (emotion == EmotionType.Hateful)
            {
                angerLevel = 1;
                happyLevel = -1;
            }
            else if (emotion == EmotionType.Amazed)
            {
                happyLevel = -1;
                angerLevel = -1;
            }
            else
            {
                this.happyLevel = 0;
                this.angerLevel = 0;
            }
        }

        public Color ToColor()
        {
            return colorMappings[this.EmotionType];
        }

        public String ToString()
        {
            switch (EmotionType)
            {
                case EmotionType.Angry:
                    return "angry";
                case EmotionType.Sad:
                    return "sad";
                case EmotionType.Terrified:
                    return "scared";
                case EmotionType.Happy:
                    return "happy";
                case EmotionType.Hateful:
                    return "hateful";
                case EmotionType.Vigilant:
                    return "vigilant";
                case EmotionType.Amazed:
                    return "amazed";
                case EmotionType.Admirative:
                    return "admirative";
                default:
                    return "neutral";
            }
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
        Sad,
        Vigilant,
        Admirative,
        Amazed,
        Hateful
    }
}