using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EmotionIsland.Helpers;

namespace EmotionIsland
{
    public class EmotionalObject : LivingObject, IEmotionalAI
    {
        private int stunTimer;
        public bool IsStunned { get { return stunTimer > 0; } }

        public Emotion Emotion { get; set; }
        public EmotionType EmotionType { get { return this.Emotion.EmotionType; } }

        public override Color Color { get { return this.Emotion.ToColor(); } set {}}

        public bool HasTargetPosition
        {
            get { return this.NextPosition != Vector2.Zero && this.NextPosition != this.Position; }
        }

        public bool HasEmotionTarget
        {
            get { return this.EmotionalTarget != null && this.EmotionalTarget.IsAlive; }
        }

        public Vector2 NextPosition { get; set; }
        public float MoveSpeed { get; set; }

        public GameObject EmotionalTarget { get; set; }
        public Vector2 DirectionToTarget
        {
            get
            {
                Vector2 direction = EmotionalTarget.Position - this.Position;
                direction.Normalize();
                return direction;
            }
        }

        public EmotionalObject(World world, Vector2 pos, Vector2 size, Texture2D tex, int health, EmotionType emotionType) : base(world, pos, size, tex, health)
        {
            this.Emotion = new Emotion(emotionType);
            if (Math.Abs(this.MoveSpeed) < 0.001f)
            {
                this.MoveSpeed = 2;
            }
        }

        public override void Update()
        {
            this.UpdateAI(this.EmotionType);
            base.Update();
        }
        
        public virtual void UpdateAI(EmotionType emotion)
        {
            if (this.HasEmotionTarget && !this.IsStunned) {
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

                if (this.HasTargetPosition)
                {
                    this.Position = this.Position.PushTowards(this.NextPosition, this.MoveSpeed*Vector2.One);
                    if (this.NextPosition == this.Position)
                    {
                        // side effect to get HasTargetPosition to register as false
                        this.NextPosition = Vector2.Zero;
                    }
                }
            }
            else if (this.IsStunned)
            {
                this.stunTimer--;
            }
        }

        public virtual void InfectWithEmotion(EmotionType type, GameObject source)
        {
            EmotionType old = this.EmotionType;
            this.Stun();

            if (type == EmotionType.Angry)
            {
                this.Anger(source);
            }

            else if (type == EmotionType.Happy)
            {
                this.Excite(source);
            }

            else if (type == EmotionType.Sad)
            {
                this.Depress(source);
            }

            else if (type == EmotionType.Terrified)
            {
                this.Terrify(source);
            }

            if (old != this.EmotionType)
            {
                this.OnEmotionChanged(source);
            }
        }

        public virtual void Anger(GameObject source)
        {
            this.Emotion.IncreaseAnger();
        }

        public virtual void Terrify(GameObject source)
        {
            this.Emotion.IncreaseTerror();
        }

        public virtual void Excite(GameObject source)
        {
            this.Emotion.IncreaseHappiness();
        }

        public virtual void Depress(GameObject source)
        {
            this.Emotion.IncreaseSadness();
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
            this.EmotionalTarget = source;
        }

        public virtual void Stun()
        {
            this.stunTimer = 100;
        }
    }
}