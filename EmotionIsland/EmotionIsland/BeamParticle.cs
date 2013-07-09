using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class BeamParticle : Particle 
    {
        public EmotionType EmotionType { get; set; }
        public EmotionBeam OwnerBeam { get; set; }

        Vector2 origin;

        public BeamParticle(World world, EmotionBeam owner, EmotionType type, Vector2 position, Vector2 direction) 
            : base(world, position, new Vector2(1), TextureBin.Pixel, direction * 8, -1)
        {
            origin = position;
            this.OwnerBeam = owner;
            this.EmotionType = type;

            string index;
            switch (EmotionType)
            {
                case EmotionType.Angry: index = "1"; break;
                case EmotionType.Sad: index = "2"; break;
                case EmotionType.Terrified: index = "3"; break;
                case EmotionType.Happy: index = "4"; break;
                default: index = "5"; break;
            }

            this.Texture = TextureBin.Get("beam0" + index);
        }

        public override void Update()
        {
            float distance = Vector2.DistanceSquared(origin, Position);
            if (distance > 100000)
            {
                IsAlive = false;
                base.Update();
                return;
            }

            Vector2 originalVelocity = Velocity;

            Random rand = new Random();
            this.Velocity = new Vector2(this.Velocity.X + (float)((rand.NextDouble()*10) * Math.Sin(distance/1000)),
                this.Velocity.Y + (float)((rand.NextDouble() * 10) * Math.Cos(distance / 1000)));

            base.Update();
            Velocity = originalVelocity;
        }

        public override void Draw(SpriteBatch spr)
        {
            Texture2D tex = this.CurAnimation != null ? this.CurAnimation.GetTexture() : this.Texture;
            Rectangle? sourceRectangle = this.CurAnimation != null ? this.CurAnimation.GetFrameRect() : (Rectangle?)null;

            spr.Draw(tex, this.Position, sourceRectangle, Color.White, this.Rotation, Vector2.Zero,
                this.CurAnimation == null ? this.Size : this.Scale,
                FacingDirection == FacingDirection.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
        }

    }
}