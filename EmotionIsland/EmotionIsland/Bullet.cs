using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class Bullet : GameObject
    {
        public int Lifespan { get; set; }
        public Vector2 Direction { get; set; }

        public override bool ShouldRemove
        {
            get
            {
                return this.Lifespan <= 0;
            }
        }

        public Bullet(World world, Vector2 spawnPosition, Vector2 size, Texture2D texture, int lifespan, float speed, Vector2 direction) 
            : base(world, spawnPosition + direction*2, size, texture)
        {
            this.Lifespan = lifespan;
            this.Direction = direction;
            this.Velocity = speed*direction;
        }

        public override void Update()
        {
            this.Lifespan--;
            base.Update();
        }
    }
}