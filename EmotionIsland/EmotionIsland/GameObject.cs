using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class GameObject
    {
        public World World { get; protected set; }
        public bool PreventVelocityChanges { get { return this.velocityDoNotChangeTimer > 0; } }

        public FacingDirection FacingDirection {
            get { return lastMovement.X < 0 ? FacingDirection.Left : FacingDirection.Right; }
        }
        
        Vector2 lastMovement = new Vector2();

        public int FrameDuration { get; set; }

        public List<AnimationSet> Animations = new List<AnimationSet>();
        private AnimationSet curAnimation;
        private int velocityDoNotChangeTimer;

        public virtual bool ShouldRemove { get { return !this.IsAlive; } }
        public virtual bool IsAlive { get; set; }

        // TODO: Implement this. We need this for garbage collecting dead stuff.
        public bool IsOffscreen { get { return false; } }

        public Rectangle BBox { 
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            }
        }

        public Vector2 Position { get; set; }

        public Vector2 Center { get { return this.Position + this.Size/2; } }
        public Vector2 Left { get { return this.Position + new Vector2(0, this.Size.Y/2); } }
        public Vector2 Right { get { return this.Position + new Vector2(this.Size.X, this.Size.Y); } }
        public Vector2 Top { get { return this.Position + new Vector2(this.Size.X/2, 0); } }
        public Vector2 Bottom { get { return this.Position + new Vector2(this.Size.X/2, this.Size.Y); } }

        private Vector2 velocity;
        public Vector2 Velocity { get { return velocity; } set { if (!this.PreventVelocityChanges) velocity = value; } }

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
            this.IsAlive = true;

            this.FrameDuration = 4;
        }

        public virtual void HandleCollision(GameObject obj)
        {
            if (this.BBox.Intersects(obj.BBox))
            {
                this.OnCollide(obj);
            }
        }

        public virtual void OnCollide(GameObject gameObject)
        {
        }

        public virtual void Update()
        {
            if (ShouldRemove)
            {
                this.World.Remove(this);
            }
            else
            {
                if (this.PreventVelocityChanges)
                {
                    this.velocityDoNotChangeTimer--;
                }
                if (this.curAnimation != null)
                {
                    this.curAnimation.Update();
                }

                if (this.Velocity.X != 0)
                {
                    this.lastMovement = this.Velocity;
                }

                Vector2 originalLocation = Position;


                int tileX = 0;
                int tileY = 0;
                int currentTileBlock = World.collisionMap[((int)Center.X / 32) + ((int)Center.Y / 32) * World.width];
                int newTileBlock = 0;

                if (velocity.X != 0)
                {
                    this.Position = new Vector2(this.Position.X + this.Velocity.X, this.Position.Y);

                    tileX = ((int)Center.X / 32);
                    tileY = ((int)Center.Y / 32);

                    if (tileX < 0)
                        return;

                    newTileBlock = World.collisionMap[tileX + tileY * World.width];

                    if (newTileBlock == (int)World.BlockTiles.All ||
                        (Velocity.X > 0 && newTileBlock != currentTileBlock && newTileBlock == (int)World.BlockTiles.Right) ||
                        (Velocity.X < 0 && newTileBlock == (int)World.BlockTiles.Left))
                        Position = new Vector2(originalLocation.X, Position.Y);
                }

                if (velocity.Y != 0)
                {
                    this.Position = new Vector2(this.Position.X, this.Position.Y + Velocity.Y);
                    tileX = ((int)Center.X / 32);
                    tileY = ((int)Center.Y / 32);
                    if (World.collisionMap[tileX + tileY * World.width] == 1)
                        Position = new Vector2(Position.X, originalLocation.Y);
                }
                
                
            }
        }

        public virtual void Draw(SpriteBatch spr)
        {
            Texture2D tex = this.curAnimation != null ? this.curAnimation.GetTexture() : this.Texture;
            Rectangle? sourceRectangle = this.curAnimation != null ? this.curAnimation.GetFrameRect() : (Rectangle?) null;

            spr.Draw(tex, this.Position, sourceRectangle, this.Color, 0, Vector2.Zero, 
                this.curAnimation == null ? this.Size : Vector2.One,
                FacingDirection == FacingDirection.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 
                0);
        }

        /// <summary>
        ///     Changes the animation being played. Doesn't do anything if called with the name of the currently
        ///     playing animation.
        /// </summary>
        /// <param name="name">The name of the new animation.</param>
        /// <exception cref="System.InvalidOperationException">Specified animation doesn't exist.</exception>
        protected virtual void ChangeAnimation(string name)
        {
            if (curAnimation == null || !curAnimation.IsCalled(name))
            {
                AnimationSet newAnimation = GetAnimationByName(name);
                if (newAnimation == null)
                    throw new InvalidOperationException("Specified animation doesn't exist.");
                newAnimation.Reset();
                newAnimation.Update();
                curAnimation = newAnimation;
            }
        }

        private AnimationSet GetAnimationByName(string name)
        {
            return Animations.Find(animset => animset.IsCalled(name));
        }

        public void PreventMovement(int time)
        {
            this.velocityDoNotChangeTimer = time;
        }
    }
}