using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EmotionIsland.Helpers;

namespace EmotionIsland
{
    public class GameObject
    {
        public virtual bool IsSolid { get { return false; } }

        public virtual float Rotation { get { return 0; } }

        public virtual int Damage { get { return 1; } }
        public World World { get; protected set; }
        public bool PreventVelocityChanges { get { return this.velocityDoNotChangeTimer > 0; } }

        private FacingDirection facingDirection = FacingDirection.None;
        public virtual FacingDirection FacingDirection {
            get
            {
                if (facingDirection == FacingDirection.None)
                    return LastMovement.X < 0 ? FacingDirection.Left : FacingDirection.Right;
                else
                    return facingDirection;
            }
            set { facingDirection = value; }
        }
        
        public Vector2 LastMovement = new Vector2();

        public int FrameDuration { get; set; }

        public List<AnimationSet> Animations = new List<AnimationSet>();
        public AnimationSet CurAnimation;
        private int velocityDoNotChangeTimer;

        public virtual bool ShouldRemove { get { return !this.IsAlive; } }
        public virtual bool IsAlive { get; set; }

        public bool CollidesWithWorld;

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
            this.Scale = Vector2.One;
            this.World = world;
            this.Position = position;
            this.Size = size;
            this.Texture = texture;

            this.Color = Color.White;
            this.IsAlive = true;
            CollidesWithWorld = false;
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
            if (gameObject.IsSolid)
            {
                Rectangle intersection = Rectangle.Intersect(this.BBox, gameObject.BBox);
                if (intersection.Width > intersection.Height)
                {
                    if (this.Center.X < gameObject.Center.X)
                        this.Position = new Vector2(this.Position.X - (intersection.Width + 1), this.Position.Y);
                    else
                        this.Position = new Vector2(this.Position.X + (intersection.Width + 1), this.Position.Y);

                    this.HandleCollision(gameObject);
                }
                else
                {

                    if (this.Center.Y < gameObject.Center.Y)
                        this.Position = new Vector2(this.Position.X, this.Position.Y - (intersection.Height + 1));
                    else
                        this.Position = new Vector2(this.Position.X, this.Position.Y + (intersection.Height + 1));
                    this.HandleCollision(gameObject);
                }
            }
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
                    if (this.velocityDoNotChangeTimer == 0)
                    {
                        this.Velocity = Vector2.Zero;
                    } 
                }
                if (this.CurAnimation != null)
                {
                    this.CurAnimation.Update();
                }

                if (this.Velocity.X != 0)
                {
                    this.LastMovement = this.Velocity;
                }

                Vector2 originalLocation = Position;

                int tileX = ((int)Center.X / 32);
                int tileY = ((int)Center.Y / 32);

                if (tileX < 0 || tileX >= World.width || tileY < 0 || tileY >= World.height)
                {
                    IsAlive = false;
                    return;
                }

                int currentTileBlock = World.collisionMap[tileX + tileY * World.width];
                int newTileBlock = 0;

                if (velocity.X != 0)
                {
                    this.Position = new Vector2(this.Position.X + this.Velocity.X, this.Position.Y);

                    if (CollidesWithWorld)
                    {
                        tileX = ((int)Center.X / 32);
                        tileY = ((int)Center.Y / 32);

                        if (tileX < 0)
                            return;

                        newTileBlock = World.collisionMap[tileX + tileY * World.width];

                        if (currentTileBlock != newTileBlock)
                        {
                            if ((Velocity.X > 0 && (newTileBlock & (int)World.BlockTiles.Right) != 0) ||
                                (Velocity.X < 0 && (newTileBlock & (int)World.BlockTiles.Left) != 0))
                                Position = new Vector2(originalLocation.X, Position.Y);
                        }
                    }
                }

                if (velocity.Y != 0)
                {
                    this.Position = new Vector2(this.Position.X, this.Position.Y + Velocity.Y);
                    if (CollidesWithWorld)
                    {
                        tileX = ((int)Center.X / 32);
                        tileY = ((int)Center.Y / 32);

                        newTileBlock = World.collisionMap[tileX + tileY * World.width];

                        if (currentTileBlock != newTileBlock)
                        {
                            if ((Velocity.Y > 0 && (newTileBlock & (int)World.BlockTiles.Down) != 0) ||
                                (Velocity.Y < 0 && (newTileBlock & (int)World.BlockTiles.Up) != 0))
                                Position = new Vector2(Position.X, originalLocation.Y);
                        }
                    }
                }
                
                
            }
        }

        public virtual void Draw(SpriteBatch spr)
        {
            Texture2D tex = this.CurAnimation != null ? this.CurAnimation.GetTexture() : this.Texture;
            Rectangle? sourceRectangle = this.CurAnimation != null ? this.CurAnimation.GetFrameRect() : (Rectangle?) null;

            spr.Draw(tex, this.Position, sourceRectangle, this.Color, this.Rotation, Vector2.Zero, 
                this.CurAnimation == null ? this.Size : this.Scale,
                FacingDirection == FacingDirection.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 
                0);
        }

        public virtual Vector2 Scale { get; set; }

        /// <summary>
        ///     Changes the animation being played. Doesn't do anything if called with the name of the currently
        ///     playing animation.
        /// </summary>
        /// <param name="name">The name of the new animation.</param>
        /// <exception cref="System.InvalidOperationException">Specified animation doesn't exist.</exception>
        public virtual void ChangeAnimation(string name)
        {
            if (CurAnimation == null || !CurAnimation.IsCalled(name))
            {
                AnimationSet newAnimation = GetAnimationByName(name);
                if (newAnimation == null)
                    throw new InvalidOperationException("Specified animation doesn't exist.");
                newAnimation.Reset();
                newAnimation.Update();
                CurAnimation = newAnimation;
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