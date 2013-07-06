using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml;

namespace EmotionIsland
{
    public enum SpriteIndex
    {
        RunFront = 0, RunBack = 1, IdleFront = 2, IdleBack = 3, DeathFront = 4, DeathBack = 5
    }

    /// <summary>
    /// A sprite set for entities
    /// </summary>
    public class EntitySpriteSet
    {
        /// <summary>
        /// A sprite
        /// </summary>
        public class Sprite
        {
            /// <summary>
            /// Sprite texture
            /// </summary>
            public Texture2D Texture;

            public int Frames;

            /// <summary>
            /// Time in milliseconds between frames
            /// </summary>
            public int FrameRate;

            /// <summary>
            /// Width and height of frame
            /// </summary>
            public int Width, Height;

            /// <summary>
            /// Draw width and height of sprite
            /// </summary>
            public int DWidth, DHeight;

            /// <summary>
            /// Weapon Position
            /// </summary>
            public int weaponX, weaponY;

            /// <summary>
            /// Does this sprite loop
            /// </summary>
            public bool Loop;

            public Sprite()
            {
                this.Loop = true;
                this.Width = this.Height = this.weaponX = this.weaponY = 0;
                this.FrameRate = 0;
            }
        }

        /// <summary>
        /// List of sprites in this sprite set
        /// </summary>
        public List<Sprite> Sprites;

        private SpriteIndex currentSprite;
        /// <summary>
        /// The current sprite being displayed in the spriteset
        /// </summary>
        public SpriteIndex CurrentSprite {

            get {
                return this.currentSprite; 
            }

            set{
                if (this.currentSprite != value)
                {
                    this.currentSprite = value;
                    //Reset animation information on sprite change
                    this.AnimationTimer = 0;
                    this.CurrentFrame = 0;
                }
            }
        }

        /// <summary>
        /// The draw size multiplier
        /// </summary>
        public int DrawMultiplier;

        /// <summary>
        /// Drawing offsets
        /// </summary>
        public int xOffset, yOffset;

        /// <summary>
        /// SpriteSet name
        /// </summary>
        public String Name;

        /// <summary>
        /// Current frame being displayed
        /// </summary>
        public int CurrentFrame;

        /// <summary>
        /// Time since last frame change
        /// </summary>
        public int AnimationTimer;

        public int WeaponX
        {
            get { return this[this.CurrentSprite].weaponX; }
        }

        public int WeaponY
        {
            get { return this[this.CurrentSprite].weaponY; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySpriteSet"/> class
        /// from an XmlNode that contains its definition.
        /// </summary>
        /// <param name="spritesetNode">XmlNode with information of this sprite set.</param>
        public EntitySpriteSet(XmlNode spritesetNode)
        {
            this.Name = spritesetNode.Attributes["name"].Value.ToString();

            this.CurrentSprite = SpriteIndex.IdleFront;

            this.DrawMultiplier = Int32.Parse(spritesetNode.Attributes["drawMultiplier"].Value); //Draw multiplier
            this.xOffset = Int32.Parse(spritesetNode.Attributes["xOffset"].Value); //Destination x offset
            this.yOffset = Int32.Parse(spritesetNode.Attributes["yOffset"].Value); //Destination y offset

            this.Sprites = new List<Sprite>();
            //Load all sprites in spriteset
            foreach (XmlNode image in spritesetNode.ChildNodes)
            {
                if (image.Name != "image")
                {
                    continue;
                }
                Sprite sprite = new Sprite();
                try
                {
                    sprite.Texture = TextureBin.Get(this.Name + image.Attributes["name"].Value); //Texture
                }
                catch (Exception ex)
                {
                    sprite.Texture = TextureBin.Pixel; //No Texture
                }
                sprite.Frames = Int32.Parse(image.Attributes["frames"].Value); // Frame count
                sprite.FrameRate = Int32.Parse(image.Attributes["framerate"].Value); //Frame rate
                sprite.Width = Int32.Parse(image.Attributes["width"].Value); //Frame width
                sprite.Height = Int32.Parse(image.Attributes["height"].Value); //Frame height

                sprite.DWidth = sprite.Width * this.DrawMultiplier;
                sprite.DHeight = sprite.Height * this.DrawMultiplier;

                if (image.Attributes["weaponX"] != null && image.Attributes["weaponY"] != null)
                {
                    sprite.weaponX = Int32.Parse(image.Attributes["weaponX"].Value); //Weapon x position
                    sprite.weaponY = Int32.Parse(image.Attributes["weaponY"].Value); //Weapon y position
                }

                sprite.Loop = Boolean.Parse(image.Attributes["loop"].Value);

                this.Sprites.Add(sprite);
            }
        }

        /// <summary>
        /// Copy constructor of a EntitySpriteSet
        /// </summary>
        /// <param name="source">Copy source</param>
        public EntitySpriteSet(EntitySpriteSet source)
        {
            this.Name = source.Name;
            this.CurrentSprite = SpriteIndex.IdleFront;

            this.xOffset = source.xOffset;
            this.yOffset = source.yOffset;

            this.Sprites = new List<Sprite>();
            foreach (Sprite sourceSprite in source.Sprites)
            {
                Sprite sprite = new Sprite();
                sprite.FrameRate = sourceSprite.FrameRate;
                sprite.Frames = sourceSprite.Frames;
                sprite.Texture = sourceSprite.Texture;
                sprite.Width = sourceSprite.Width;
                sprite.Height = sourceSprite.Height;

                sprite.DHeight = sourceSprite.DHeight;
                sprite.DWidth = sourceSprite.DWidth;

                sprite.Loop = sourceSprite.Loop;
                sprite.weaponX = sourceSprite.weaponX;
                sprite.weaponY = sourceSprite.weaponY;
                this.Sprites.Add(sprite);
            }
        }

        /// <summary>
        /// Updates frame animation.
        /// </summary>
        /// <param name="elapsedTimeMilliseconds">The elapsed time milliseconds.</param>
        public void update(int elapsedTimeMilliseconds)
        {
            this.AnimationTimer += elapsedTimeMilliseconds;
            //If elapsed time has passed frame rate
            if (this.AnimationTimer > this[this.CurrentSprite].FrameRate)
            {
                this.AnimationTimer = 0;
                this.CurrentFrame++;
                if (this.CurrentFrame >= this[this.CurrentSprite].Frames)
                {
                    if (this[this.CurrentSprite].Loop)
                    {
                        this.CurrentFrame = 0;
                    }
                    else
                    {
                        this.CurrentFrame = this[this.CurrentSprite].Frames - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the current sprite to the specified position
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects transformation)
        {
            Rectangle destRectangle = new Rectangle((int)position.X + this.xOffset, (int)position.Y + this.yOffset, this[this.CurrentSprite].DWidth, this[this.CurrentSprite].DHeight);
            spriteBatch.Draw(this[this.CurrentSprite].Texture, destRectangle, this.GetSourceRectangle(this.CurrentFrame), Color.White, 0, Vector2.Zero, transformation, position.Y + this.yOffset);
        }

        /// <summary>
        /// Draws the current sprite to the specified position with no depth
        /// </summary>
        public void DrawUnordered(SpriteBatch spriteBatch, Vector2 position, SpriteEffects transformation)
        {
            Rectangle destRectangle = new Rectangle((int)position.X + this.xOffset, (int)position.Y + this.yOffset, this[this.CurrentSprite].DWidth, this[this.CurrentSprite].DHeight);
            spriteBatch.Draw(this[this.CurrentSprite].Texture, destRectangle, this.GetSourceRectangle(this.CurrentFrame), Color.White, 0, Vector2.Zero, transformation, 0);
        }
        

        public Rectangle GetSourceRectangle(int frame)
        {
            return new Rectangle(frame * this[this.CurrentSprite].Width, 0, this[this.CurrentSprite].Width, this[this.CurrentSprite].Height);
        }

        /// <summary>
        /// Gets the <see cref="Sprite"/> at the specified index.
        /// </summary>
        public Sprite this[SpriteIndex index]
        {
            get { return this.Sprites[(int)index]; }
        }

    }
}
