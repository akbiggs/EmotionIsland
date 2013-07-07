using System;
using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public class Villager : EmotionalObject
    {
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
            : base(world, pos, new Vector2(32, 32), TextureBin.Pixel, 3, emotion)
        {
        }

        public override void Update()
        {
            if (!this.CanAttack)
            {
                this.attackCoolDownTimer--;
            }
            base.Update();
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
                this.World.Add(new SlashAttack(this.World, this.Position, this, target.Position - this.Position));
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
            if (wanderTimer > 100)
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
        }
        #endregion

        public override void OnCollide(GameObject gameObject)
        {
            if (gameObject is BeamParticle)
            {
                BeamParticle particle = (BeamParticle) gameObject;
                this.InfectWithEmotion(particle.EmotionType, particle.OwnerBeam.Owner);
            }
            else if (gameObject is SlashAttack)
            {
                SlashAttack attack = ((SlashAttack) gameObject);
                if (attack.Owner != this)
                {
                    this.TakeDamage(1, attack.Direction);
                }
            }
            base.OnCollide(gameObject);
        }


    }
}