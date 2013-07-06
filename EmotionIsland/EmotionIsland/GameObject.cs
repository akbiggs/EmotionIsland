using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class GameObject
    {
        public World World { get; protected set; }

        public virtual bool ShouldRemove { get { return false; } }

        // TODO: Implement this. We need this for garbage collecting dead stuff.
        public bool IsOffscreen { get { return false; } }

        public Vector2 Position { get; set; }

        public Vector2 Center { get { return this.Position + this.Size/2; } }
        public Vector2 Left { get { return this.Position + new Vector2(0, this.Size.Y/2); } }
        public Vector2 Right { get { return this.Position + new Vector2(this.Size.X, this.Size.Y); } }
        public Vector2 Top { get { return this.Position + new Vector2(this.Size.X/2, 0); } }
        public Vector2 Bottom { get { return this.Position + new Vector2(this.Size.X/2, this.Size.Y); } }

        public Vector2 Velocity { get; set; }

        public Vector2 Size { get; set; }
        public Texture2D Texture { get; protected set; }
        public virtual Color Color { get; set; }

        public GameObject(World world, Vector2 position, Vector2 size, Texture2D texture)
        {
            this.World = world;
            this.Position = position;
            this.Size = size;
            this.Texture = texture;

            this.Color = Color.White;
        }

        public virtual void Update()
        {
            if (this.ShouldRemove)
            {
                this.World.Remove(this);
            }
            else
            {
                this.Position = new Vector2(this.Position.X + this.Velocity.X, this.Position.Y + this.Velocity.Y);
            }
        }

        public virtual void Draw(SpriteBatch spr)
        {
            spr.Draw(this.Texture, this.Position, null, this.Color, 0, Vector2.Zero, this.Size, SpriteEffects.None, 0);
        }
    }
}