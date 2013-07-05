using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class GameObject
    {
        public World World { get; protected set; }

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public Vector2 Size { get; set; }
        public Texture2D Texture { get; protected set; }
        public Color Color { get; set; }

        public GameObject(World world, Vector2 position, Vector2 size, Texture2D texture)
        {
            this.World = world;
            this.Position = position;
            this.Size = size;
            this.Texture = texture;

            this.Color = Color.White;
        }

        public GameObject(World world, Vector2 position, Vector2 initialVelocity, Vector2 size, Texture2D texture)
            : this(world, position, size, texture)
        {
            this.Velocity = initialVelocity;
        }

        public virtual void Update()
        {
            this.Position = new Vector2(this.Position.X + this.Velocity.X, this.Position.Y + this.Velocity.Y);
        }

        public virtual void Draw(SpriteBatch spr)
        {
            spr.Draw(this.Texture, this.Position, null, this.Color, 0, Vector2.Zero, this.Size, SpriteEffects.None, 0);
        }
    }
}