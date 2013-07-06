using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class EmotionalObject : LivingObject, IEmotionalAI
    {
        public Emotion Emotion { get; set; }
        public EmotionType EmotionType { get { return this.Emotion.EmotionType; } }

        public Vector2 TargetPosition { get; set; }

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
    }
}