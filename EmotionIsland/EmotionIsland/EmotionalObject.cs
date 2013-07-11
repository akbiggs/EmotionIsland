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
            if (!this.IsStunned) {
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
                else if (emotion == EmotionType.Admirative) this.AdmirativeUpdate();
                else if (emotion == EmotionType.Hateful) this.HatefulUpdate();
                else if (emotion == EmotionType.Vigilant) this.VigilantUpdate();
                else if (emotion == EmotionType.Amazed) this.AmazedUpdate();
                else
                {
                    this.NeutralUpdate();
                }

                if (this.HasTargetPosition)
                {
                    if (this.NextPosition == this.Position)
                    {
                        // side effect to get HasTargetPosition to register as false
                        this.NextPosition = Vector2.Zero;
                    }
                    else
                    {
                        if (overrideTargetPosition != Vector2.Zero)
                        {
                            this.NextPosition = overrideTargetPosition;
                            overrideTargetPosition = Vector2.Zero;
                        }
                            Vector2 displacement = this.Position.PushTowards(this.NextPosition, this.MoveSpeed * Vector2.One);
                            this.Velocity = displacement - this.Position;
                            if (this.Velocity.LengthSquared() < 0.1f * 0.1f)
                                this.Velocity = Vector2.Zero;
                        
                    }
                }
            }
            else if (this.IsStunned)
            {
                this.stunTimer--;
            }
        }

        public virtual bool InfectWithEmotion(EmotionType type, GameObject source)
        {
            EmotionType old = this.EmotionType;
            this.Stun();

            float angerLevel = this.Emotion.angerLevel;
            float happyLevel = this.Emotion.happyLevel;

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
            else if (type == EmotionType.Neutral)
            {
                if (this.EmotionType == global::EmotionIsland.EmotionType.Angry)
                {
                    this.Terrify(source);
                }
                else if (this.EmotionType == global::EmotionIsland.EmotionType.Terrified)
                {
                    this.Anger(source);
                }
                else if (this.EmotionType == global::EmotionIsland.EmotionType.Happy)
                {
                    this.Depress(source);
                }
                else if (this.EmotionType == global::EmotionIsland.EmotionType.Sad)
                {
                    this.Excite(source);
                }
            }

            if (old != this.EmotionType)
            {
                this.OnEmotionChanged(source);
                return true;
            }

            if (this.Emotion.angerLevel == angerLevel && this.Emotion.happyLevel == happyLevel)
                return false;

            return true;
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

        public virtual void VigilantUpdate()
        {
            this.NeutralUpdate();
        }

        public virtual void HappyUpdate()
        {
        }

        public virtual void AdmirativeUpdate()
        {
        }

        public virtual void AmazedUpdate()
        {
            //this.NeutralUpdate();
        }

        public virtual void HatefulUpdate()
        {
        }

        public virtual void TerrifiedUpdate()
        {
        }

        public virtual void SadUpdate()
        {
            this.NeutralUpdate();
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
            this.stunTimer = 40;
        }
    }
}