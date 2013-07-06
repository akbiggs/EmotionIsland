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
            this.World.Add(new SlashAttack(this.World, this.Position, target.Position - this.Position));
        }

        public override void TerrifiedUpdate()
        {
            base.TerrifiedUpdate();
        }
    }
}