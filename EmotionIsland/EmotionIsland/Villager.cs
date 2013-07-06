using System;
using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public class Villager : EmotionalObject
    {
        public override Color Color { get { return this.Emotion.ToColor(); } }

        public Villager(World world, Vector2 pos, EmotionType emotion) 
            : base(world, pos, new Vector2(32, 32), TextureBin.Pixel, 3, emotion)
        {
        }

        public override void HappyUpdate()
        {
            this.NextPosition = this.EmotionalTarget.Position;
            if (Vector2.DistanceSquared(this.Position, EmotionalTarget.Position) < Math.Pow(5, 2))
            {
                this.Love(this.EmotionalTarget);
            }

            base.HappyUpdate();
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
            this.World.Add(new SlashAttack(this.World, this.Position, this, target.Position - this.Position));
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
    }
}