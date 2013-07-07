using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public class Villager : EmotionalObject
    {
        public override bool IsSolid
        {
            get { return true; }
        }
        
        private string lastDirection;

        public override FacingDirection FacingDirection
        {
            get
            {
                return this.lastDirection.EndsWith("lower") || this.lastDirection.EndsWith("upper")
                           ? FacingDirection.Left
                           : base.FacingDirection;
            }
        }

        public override int Damage
        {
            get { return EmotionType == EmotionType.Hateful ? 2 : 1; }
        }
        private GameObject body, head;
        private int wanderTimer = 0;
        public Vector2 WanderDirection {
            get 
            {
                Vector2 direction = (this.Position - this.NextPosition); direction.Normalize();
                return direction;
            }
            set 
            { 
                this.NextPosition = this.Position + value * wanderOffset;
                wanderOffset = MathExtra.RandomInt(50, 120);
            }
        }

        private int wanderOffset = 100;
        private int attackCoolDownTimer;

        protected bool CanAttack
        {
            get { return this.attackCoolDownTimer == 0; }
        }

        public override Color Color { get { return this.Emotion.ToColor(); } }

        public Villager(World world, Vector2 pos, EmotionType emotion) 
            : base(world, pos, new Vector2(32), TextureBin.Pixel, 3, emotion)
        {
            CollidesWithWorld = true;
            this.body = new GameObject(this.World, this.Position, this.Size, TextureBin.Pixel);
            this.head = new GameObject(this.World, this.Position, this.Size, TextureBin.Pixel);

            this.body.Animations = new List<AnimationSet>
                {
                    new AnimationSet("idle_upper", TextureBin.Get("body"), 1, 32, 32, FrameDuration, 9, true, 0),
                    new AnimationSet("idle_lower", TextureBin.Get("body"), 1, 32, 32, FrameDuration, 9, true, 9),
                    new AnimationSet("idle_side", TextureBin.Get("body"), 1, 32, 32, FrameDuration, 9, true, 18),

                    new AnimationSet("walk_upper", TextureBin.Get("body"), 6, 32, 32, FrameDuration, 9, true, 1),
                    new AnimationSet("walk_lower", TextureBin.Get("body"), 6, 32, 32, FrameDuration, 9, true, 9 + 1),
                    new AnimationSet("walk_side", TextureBin.Get("body"), 8, 32, 32, FrameDuration, 9, true, 18 + 1),
                };

            this.body.ChangeAnimation("walk_lower");

            this.head.Animations = new List<AnimationSet>
                {
                    new AnimationSet("happy_lower", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 0),
                    new AnimationSet("happy_lowerdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 1),
                    new AnimationSet("happy_side", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 2),
                    new AnimationSet("happy_upperdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 3),
                    new AnimationSet("happy_upper", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 4),

                    new AnimationSet("angry_lower", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 10 + 0),
                    new AnimationSet("angry_lowerdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 10 + 1),
                    new AnimationSet("angry_side", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 10 + 2),
                    new AnimationSet("angry_upperdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 10 + 3),
                    new AnimationSet("angry_upper", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 10 + 4),

                    new AnimationSet("sad_lower", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 20 + 0),
                    new AnimationSet("sad_lowerdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 20 + 1),
                    new AnimationSet("sad_side", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 20 + 2),
                    new AnimationSet("sad_upperdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 20 + 3),
                    new AnimationSet("sad_upper", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 20 + 4),

                    new AnimationSet("scared_lower", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 30 + 0),
                    new AnimationSet("scared_lowerdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 30 + 1),
                    new AnimationSet("scared_side", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 30 + 2),
                    new AnimationSet("scared_upperdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 30 + 3),
                    new AnimationSet("scared_upper", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 30 + 4),

                    new AnimationSet("neutral_lower", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 40 + 0),
                    new AnimationSet("neutral_lowerdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 40 + 1),
                    new AnimationSet("neutral_side", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 40 + 2),
                    new AnimationSet("neutral_upperdiag", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 40 + 3),
                    new AnimationSet("neutral_upper", TextureBin.Get("emotions"), 1, 32, 32, 1, 5, false, 40 + 4),
                };

            this.head.ChangeAnimation(this.Emotion.ToString() + "_lower");

            this.body.Scale = Vector2.One*2;
            this.head.Scale = Vector2.One*2;
        }
        public override void Update()
        {
            Player player = this.FindClosestPlayer();
            if (player != null && Vector2.DistanceSquared(player.Position, this.Position) >= 10000000)
            {
                World.Remove(this);
            }
            else
            {
                this.head.Position = this.Position;
                this.body.Position = this.Position;

                this.UpdateAnimation();

                this.body.FacingDirection = this.FacingDirection;
                this.body.Update();

                this.head.FacingDirection = this.FacingDirection;
                this.head.Update();

                if (!this.CanAttack)
                {
                    this.attackCoolDownTimer--;
                }

                base.Update();

                if (this.Velocity == Vector2.Zero)
                {
                    this.body.ChangeAnimation("idle_" + this.GetEnding());
                }
            }
        }

        public void UpdateAnimation()
        {
            if (Velocity.Y < 0 && Math.Abs(Velocity.X) > 0)
            {
                this.lastDirection = "upperdiag";
            }
            else if (Velocity.Y > 0 && Math.Abs(Velocity.X) > 0)
            {
                this.lastDirection = "lowerdiag";
            }
            else if (Math.Abs(Velocity.X) > 0)
            {
                this.lastDirection = "side";
            }
            else if (Velocity.Y < 0)
            {
                this.lastDirection = "upper";
            }
            else
            {
                this.lastDirection = "lower";
            }

            this.UpdateBodyAnimation();
            this.UpdateHeadAnimation();
        }

        private void UpdateBodyAnimation()
        {
            string beginning;
            string ending;
            if (this.Velocity.Length() <= 0.05f)
            {
                beginning = "idle";
            }
            else
            {
                beginning = "walk";
            }
            ending = this.GetEnding();

            this.body.ChangeAnimation(beginning + "_" + ending);
        }

        private string GetEnding()
        {
            string ending;
            if (this.lastDirection.Contains("upper"))
                ending = "upper";
            else if (this.lastDirection.Contains("lower"))
                ending = "lower";
            else
                ending = "side";
            return ending;
        }

        private void UpdateHeadAnimation()
        {
            this.head.ChangeAnimation(this.Emotion.ToString() + "_" + lastDirection);
        }

        #region AI
        
        public override void HappyUpdate()
        {
            this.NextPosition = this.EmotionalTarget.Position;
            if (Vector2.DistanceSquared(this.Position, EmotionalTarget.Position) < Math.Pow(5, 2))
            {
                this.Love(this.EmotionalTarget);
            }

            base.HappyUpdate();
        }

        public override void SadUpdate()
        {
            // wait in place

            base.SadUpdate();
        }

        private void Love(GameObject target)
        {
            if (target is LivingObject)
            {
                ((LivingObject)target).Heal();
            }
        }

        public override void AngryUpdate()
        {
            this.NextPosition = this.EmotionalTarget.Position;
            if (Vector2.DistanceSquared(this.Position, EmotionalTarget.Position) < Math.Pow(10, 2))
            {
                this.Attack(this.EmotionalTarget);
            }

            base.AngryUpdate();
        }

        private void Attack(GameObject target)
        {
            if (this.CanAttack)
            {
                Vector2 direction = target.Position - this.Position;
                direction.Normalize();
                this.World.Add(new SlashAttack(this.World, this.Position + direction, this, direction));
                this.attackCoolDownTimer = 30;
            }
        }

        public override void TerrifiedUpdate()
        {
            Vector2 fleeDirection = -DirectionToTarget;
            if (Vector2.DistanceSquared(this.Position, this.EmotionalTarget.Position) < Math.Pow(200, 2))
            {
                this.NextPosition = this.EmotionalTarget.Position + 200*fleeDirection;
            }

            base.TerrifiedUpdate();
        }

        public override void NeutralUpdate()
        {
            ++wanderTimer;
            if (wanderTimer > 60)
            {
                this.WanderDirection = this.PickRandomDirection();
                this.wanderTimer = 0;
            }

            base.NeutralUpdate();
        }

        public override void OnEmotionChanged(GameObject source)
        {
            if (this.EmotionType == EmotionType.Neutral)
            {
                this.wanderTimer = 0;
                this.WanderDirection = this.PickRandomDirection();
            }
            else if (this.EmotionType == EmotionType.Angry)
            {
                this.EmotionalTarget = this.FindClosestVillager();
            }
            else
            {
                base.OnEmotionChanged(source);
            }
        }

        private Villager FindClosestVillager()
        {
            float closestDistanceSquared = 10000000;
            Villager closestVillager = null;

            foreach (var villager in this.World.Villagers)
            {
                if (villager != this)
                {
                    float distanceSquared = Vector2.DistanceSquared(this.Position, villager.Position);
                    if (distanceSquared < closestDistanceSquared)
                    {
                        closestDistanceSquared = distanceSquared;
                        closestVillager = villager;
                    }
                }
            }
            return closestVillager;
        }

        private Player FindClosestPlayer()
        {
            float closestDistanceSquared = 10000000;
            Player closestPlayer = null;

            foreach (var player in this.World.Players)
            {
                float distanceSquared = Vector2.DistanceSquared(this.Position, player.Position);
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestPlayer = player;
                }
            }
            return closestPlayer;
        }

        private Vector2 PickRandomDirection()
        {
            Random random = new Random();
            if (random.Next(2) == 0)
            {
                if (random.Next(2) == 0)
                {
                    return new Vector2(1, 0);
                }
                else
                {
                    return new Vector2(-1, 0);
                }
            }
            else
            {
                if (random.Next(2) == 0)
                {
                    return new Vector2(0, 1);
                }
                else
                {
                    return new Vector2(0, -1);
                    
                }
            }

            return new Vector2(0, 0);
        }
        #endregion

        public override void OnCollide(GameObject gameObject)
        {
            if (gameObject is BeamParticle)
            {
                BeamParticle particle = (BeamParticle) gameObject;
                this.InfectWithEmotion(particle.EmotionType, particle.OwnerBeam.Owner);
                particle.OwnerBeam.Particles.BufferRemove(particle);
            }
            else if (gameObject is SlashAttack)
            {
                SlashAttack attack = ((SlashAttack) gameObject);
                if (attack.Owner != this)
                {
                    this.TakeDamage(attack.Owner.Damage, attack.Direction);
                }
            }
            else if (gameObject is Villager)
            {
                Villager villager = ((Villager) gameObject);
                if (this.IsAngry && gameObject != this && (villager.EmotionType != EmotionType.Angry || villager.EmotionType != EmotionType.Hateful))
                {
                    this.Attack(gameObject);
                }
            }
            base.OnCollide(gameObject);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spr)
        {
            body.Draw(spr);
            head.Draw(spr);
        }

        protected bool IsAngry
        {
            get { return this.EmotionType == EmotionType.Angry || this.EmotionType == EmotionType.Hateful || this.EmotionType == EmotionType.Vigilant; }
        }
    }
}