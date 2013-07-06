using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class Particle : GameObject
    {
        private const int INFINITE_LIFESPAN = 999999;
        public int Lifespan { get; private set; }
        public override bool ShouldRemove
        {
            get { return this.Lifespan <= 0; }
        }

        public Particle(World world, Vector2 position, Vector2 size, Texture2D texture, Vector2 velocity, int lifespan) 
            : base(world, position, size, texture)
        {
            if (lifespan == -1)
            {
                this.Lifespan = INFINITE_LIFESPAN;
            }

            this.Velocity = velocity;
        }

        public override void Update()
        {
            this.Lifespan -= 1;
            base.Update();
        }
    }
}