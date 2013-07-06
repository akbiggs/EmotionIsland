using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class EmotionalObject : LivingObject, IEmotionalAI
    {
        public Emotion Emotion { get; private set; }
        public EmotionType EmotionType { get { return this.Emotion.EmotionType; } }

        public Vector2 NextPosition { get; set; }
        public GameObject EmotionalTarget { get; set; }

        public EmotionalObject(World world, Vector2 pos, Vector2 size, Texture2D tex, int health, EmotionType emotionType) : base(world, pos, size, tex, health)
        {
            this.Emotion = new Emotion(emotionType);
        }
        
        public virtual void UpdateAI(EmotionType emotion)
        {
            if (emotion == EmotionType.Angry)
            {
                this.AngryUpdate();
            }
            else if (emotion == EmotionType.Happy)
            {
                this.HappyUpdate();
            }
            else if (emotion == EmotionType.Sad)
            {
                this.SadUpdate();
            }
            else if (emotion == EmotionType.Terrified)
            {
                this.TerrifiedUpdate();
            }
            else
            {
                this.NeutralUpdate();
            }
        }

        public virtual void Anger(GameObject source)
        {
            EmotionType old = this.EmotionType;
            this.Emotion.IncreaseAnger();
            if (old != this.EmotionType)
            {
                this.OnEmotionChanged(source);
            }
        }

        public virtual void Terrify(GameObject source)
        {
            EmotionType old = this.EmotionType;
            this.Emotion.IncreaseTerror();
            if (old != this.EmotionType)
            {
                this.OnEmotionChanged(source);
            }
        }

        public virtual void Excite(GameObject source)
        {
            EmotionType old = this.EmotionType;
            this.Emotion.IncreaseHappiness();
            if (old != this.EmotionType)
            {
                this.OnEmotionChanged(source);
            }
        }

        public virtual void Depress(GameObject source)
        {
            EmotionType old = this.EmotionType;
            this.Emotion.IncreaseSadness();
            if (old != this.EmotionType)
            {
                this.OnEmotionChanged(source);
            }
        }


        public virtual void AngryUpdate()
        {
        }

        public virtual void HappyUpdate()
        {
        }

        public virtual void TerrifiedUpdate()
        {
        }

        public virtual void SadUpdate()
        {
        }

        public virtual void NeutralUpdate()
        {
        }

        public virtual void OnEmotionChanged(GameObject source)
        {
            Debug.WriteLine("I changed!");
            this.EmotionalTarget = source;
        }
    }
}