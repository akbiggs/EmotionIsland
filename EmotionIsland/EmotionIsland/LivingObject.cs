using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class LivingObject : GameObject
    {
        public int Health { get; private set; }

        public LivingObject(World world, Vector2 pos, Vector2 size, Texture2D tex, int health)
            : base(world, pos, size, tex)
        {
            this.Health = health;
        }

        public void TakeDamage(int amount)
        {
            this.Health -= amount;
            if (this.Health <= 0)
            {
                this.Die();
            }
        }

        public void Die()
        {
            this.World.Remove(this);
        }
    }
}