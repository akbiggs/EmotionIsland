using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EmotionIsland
{
    public class Player : LivingObject
    {
        private const int START_HEALTH = 3;
        public static Dictionary<int, PlayerNumber> Nums = new Dictionary<int, PlayerNumber>
            {
                {1, PlayerNumber.One},
                {2, PlayerNumber.Two},
                {3, PlayerNumber.Three},
                {4, PlayerNumber.Four}
            };
        public override bool IsSolid
        {
            //get { return true; }
            get { return false; }
        }

        

        private string lastDirection;

        public override FacingDirection FacingDirection
        {
            get
            {
                return this.lastDirection.EndsWith("lower") || this.lastDirection.EndsWith("upper")
                           ? FacingDirection.Left
                           : base.FacingDirection;
            }
        }
        public EmotionBeam Beam { get; set; }

        private EmotionType etype;
        public Emotion Emotion { get; set; }

        public override bool ShouldRemove
        {
            get { return this.Health <= 0; }
        }

        public PlayerIndex PlayerIndex
        {
            get
            {
                if (this.PlayerNumber == PlayerNumber.One) return PlayerIndex.One;
                if (this.PlayerNumber == PlayerNumber.Two) return PlayerIndex.Two;
                if (this.PlayerNumber == PlayerNumber.Three) return PlayerIndex.Three;
                
                return PlayerIndex.Four;
                
            }
        }
        public EmotionType EmotionType { 
            get { return etype; }
            set
            {
                etype = value; 
                this.Emotion = new Emotion(etype); 
            }
        }
        public PlayerNumber PlayerNumber { get; private set; }

        private PlayerKeyBindings keyBindings;
        private int playFireTimer;
        private Color gunColor;

        public Player(World world, Vector2 pos, PlayerNumber pn) : base(world, pos, new Vector2(32, 32), TextureBin.Pixel, START_HEALTH)
        {
            CollidesWithWorld = true;
            
            this.PlayerNumber = pn;
            this.keyBindings = PlayerKeyBindings.FromPlayerNumber(pn);

            Random rand = new Random();
            int randEmotionNum = rand.Next(0, 4);

            switch (randEmotionNum)
            {
                case 0: this.EmotionType = EmotionType.Angry;
                    break;
                case 1: this.EmotionType = EmotionType.Sad;
                    break;
                case 2: this.EmotionType = EmotionType.Terrified;
                    break;
                case 3: this.EmotionType = EmotionType.Happy;
                    break;
                default:
                    this.EmotionType = EmotionType.Neutral;
                    break;
            }
            this.Emotion = new Emotion(this.EmotionType);
            string spritesheetName = "characterSheetWalk_0" + (randEmotionNum+1);

            this.Animations = new List<AnimationSet>
                {
                    new AnimationSet("idle_upperdiag", TextureBin.Get(spritesheetName), 1, 32, 32, this.FrameDuration, 7, true, 0),
                    new AnimationSet("idle_lowerdiag", TextureBin.Get(spritesheetName), 1, 32, 32, FrameDuration, 7, true, 7),
                    new AnimationSet("idle_upper", TextureBin.Get(spritesheetName), 1, 32, 32, FrameDuration, 7, true, 7*2),
                    new AnimationSet("idle_lower", TextureBin.Get(spritesheetName), 1, 32, 32, FrameDuration, 7, true, 7*3),
                    new AnimationSet("idle_side", TextureBin.Get(spritesheetName), 1, 32, 32, FrameDuration, 7, true, 7*4),

                    new AnimationSet("walk_upperdiag", TextureBin.Get(spritesheetName), 6, 32, 32, this.FrameDuration, 7, true, 1),
                    new AnimationSet("walk_lowerdiag", TextureBin.Get(spritesheetName), 6, 32, 32, FrameDuration, 7, true, 7+1),
                    new AnimationSet("walk_upper", TextureBin.Get(spritesheetName), 6, 32, 32, FrameDuration, 7, true, 7*2+1),
                    new AnimationSet("walk_lower", TextureBin.Get(spritesheetName), 6, 32, 32, FrameDuration, 7, true, 7*3+1),
                    new AnimationSet("walk_side", TextureBin.Get(spritesheetName), 6, 32, 32, FrameDuration, 7, true, 7*4+1),
                };

            this.GunTexture = TextureBin.Get("guns");
            this.gunColor = this.Emotion.ToColor();
            this.GunSourceRect = new Rectangle(0, randEmotionNum*32, 32, 32);

            lastDirection = "upper";
            this.ChangeAnimation("walk_upper");
        }

        protected Rectangle GunSourceRect { get; set; }

        protected Texture2D GunTexture
        {
            get; set; 
        }

        public override void Update()
        {
            base.Update();
            if (this.playFireTimer > 0)
                this.playFireTimer--;
            if (this.Beam != null)
            {
                this.Beam.BasePosition = this.Center + this.GetBeamOffset();
            }

            this.HandleMovement();

            if (Input.gps[(int)PlayerIndex].Buttons.X == ButtonState.Pressed)
            {
                this.FireWeaponAt(this.Position + this.GetLastMovementDirection() * 50, EmotionType);
            }
            else if (Input.gps[(int)PlayerIndex].Buttons.B == ButtonState.Pressed)
            {
                this.FireWeaponAt(this.Position + this.GetLastMovementDirection() * 50, EmotionType.Neutral);
            }
            else if (this.Beam != null)
            {
                this.Beam.Stopped = true;
            }
        }

        public override void Die()
        {
            TextureBin.PlaySound("death01");
            base.Die();
        }

        private void FireWeaponAt(Vector2 targetPosition, EmotionType emotion)
        {
            Vector2 direction = targetPosition - this.Position;
            direction.Normalize();
            if (this.Beam == null)
            {
                EmotionBeam beam = new EmotionBeam(World, this.Center + this.GetBeamOffset(), direction, emotion, this);
                this.Beam = beam;
                this.World.Add(this.Beam);
            }
            else
            {
                this.Beam.Emotion = new Emotion(emotion);
                this.Beam.Direction = direction;
            }
            this.Beam.Stopped = false;
            if (this.playFireTimer == 0)
            {
                if (EmotionIsland.nextFade == Color.Transparent)
                    TextureBin.PlaySound("shoot01");
                this.playFireTimer = 8;
            }
        }

        private Vector2 GetBeamOffset()
        {
            return -(new Vector2(6)) +this.GetLastMovementDirection()*20;
        }

        private void HandleMovement()
        {
            this.Velocity = Vector2.Zero;

            const int moveSpeed = 3;
            if (Input.gps[(int)PlayerIndex].ThumbSticks.Left.Y > .25)
            {
                this.Velocity = new Vector2(this.Velocity.X, -moveSpeed);
            }
            else if (Input.gps[(int)PlayerIndex].ThumbSticks.Left.Y < -.25)
            {
                this.Velocity = new Vector2(this.Velocity.X, moveSpeed);
            }
            if (Input.gps[(int)PlayerIndex].ThumbSticks.Left.X < -.25)
            {
                this.Velocity = new Vector2(-moveSpeed, this.Velocity.Y);
            }
            else if (Input.gps[(int)PlayerIndex].ThumbSticks.Left.X > .25)
            {
                this.Velocity = new Vector2(moveSpeed, this.Velocity.Y);
            }

            this.UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (Velocity == Vector2.Zero)
            {
                this.ChangeAnimation("idle_" + lastDirection);
            } 
            else 
            {
                if (Velocity.Y < 0 && Math.Abs(Velocity.X) > 0)
                {
                    this.lastDirection = "upperdiag";
                }
                else if (Velocity.Y > 0 && Math.Abs(Velocity.X) > 0)
                {
                    this.lastDirection = "lowerdiag";
                }
                else if (Math.Abs(Velocity.X) > 0)
                {
                    this.lastDirection = "side";
                }
                else if (Velocity.Y < 0)
                {
                    this.lastDirection = "upper";
                }
                else
                {
                    this.lastDirection = "lower";
                }

                this.ChangeAnimation("walk_" + this.lastDirection);
            }
        }

        public override void Draw(SpriteBatch spr)
        {
            if (lastDirection.Contains("upper"))
            {
                this.DrawGun(spr);
                base.Draw(spr);
            }
            else
            {
                base.Draw(spr);
                this.DrawGun(spr);
            }

            Vector2 startHealthPos = this.Center - new Vector2(6) - new Vector2(20, 20);
            for (int i = 0; i < Health; i++)
            {
                spr.Draw(TextureBin.Get("heart"), startHealthPos + new Vector2(i*20, 0), null, this.Color, 0, Vector2.Zero,
                    1, SpriteEffects.None, 0);
            }
        }

        private void DrawGun(SpriteBatch spr)
        {
            spr.Draw(GunTexture, this.Center, GunSourceRect, this.gunColor, 
                FacingDirection == FacingDirection.Right ? -GetGunRotation() : this.GetGunRotation(), GetGunPosition(), 1,
                FacingDirection == FacingDirection.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        }

        private Vector2 GetGunPosition()
        {
            if (this.lastDirection.Contains("upperdiag"))
            {

            }
            else if (this.lastDirection.Contains("upper"))
            {
                return new Vector2(this.GunTexture.Width/(1.35f), 16);
            }
            else if (this.lastDirection.Contains("lowerdiag"))
            {
                //return -MathHelper.PiOver4;
            }

            if (this.lastDirection.Contains("lower"))
            {
                //return -MathHelper.PiOver2;
            }

            return new Vector2(this.GunTexture.Width/(1.9f), 12);
        }

        public Vector2 GetLastMovementDirection()
        {
            if (this.lastDirection.Contains("upperdiag"))
            {
                return new Vector2(1* (int)this.FacingDirection, -1);
            }
            else if (this.lastDirection.Contains("upper"))
            {
                return new Vector2(0* (int)this.FacingDirection, -1);
                return new Vector2(0, 1) * (int)this.FacingDirection;
            }
            else if (this.lastDirection.Contains("lowerdiag"))
            {
                return new Vector2(1* (int)this.FacingDirection, 1);
                return new Vector2(1, -1) * (int)this.FacingDirection;
                //return -MathHelper.PiOver4;
            }

            if (this.lastDirection.Contains("lower"))
            {
                return new Vector2(0, 1);
            }

            return new Vector2((float)this.FacingDirection, 0);
        }

        private float GetGunRotation()
        {
            if (this.lastDirection.Contains("upperdiag"))
            {
                return MathHelper.PiOver4;
            }
            else if (this.lastDirection.Contains("upper"))
            {
                return MathHelper.PiOver2;
            }
            else if (this.lastDirection.Contains("lowerdiag"))
            {
                return -MathHelper.PiOver4;
            }

            if (this.lastDirection.Contains("lower"))
            {
                return -MathHelper.PiOver2;
            }

            return 0;
        }

        #region Collisions
        public override void OnCollide(GameObject obj)
        {
            if (obj is SlashAttack)
            {
                this.SubTypeCollide((SlashAttack)obj);
            }

            if (obj is Treasure)
            {
                Treasure treasure = (Treasure)obj;
                treasure.Open();
            }

            base.OnCollide(obj);
        }

        private void SubTypeCollide(SlashAttack attack)
        {
            this.TakeDamage(attack.Owner.Damage, attack.Direction);
        }

        #endregion
    }

    internal class PlayerKeyBindings
    {
        public static Dictionary<PlayerNumber, PlayerKeyBindings> mappings = new Dictionary
            <PlayerNumber, PlayerKeyBindings>
            {
                {PlayerNumber.One, new PlayerKeyBindings(Keys.Up, Keys.Left, Keys.Down, Keys.Right, Keys.RightControl)},
                {PlayerNumber.Two, new PlayerKeyBindings(Keys.W, Keys.A, Keys.S, Keys.D, Keys.LeftControl)},
                {PlayerNumber.Three, new PlayerKeyBindings(Keys.J, Keys.H, Keys.K, Keys.L, Keys.Space)},
                {PlayerNumber.Four, new PlayerKeyBindings(Keys.G, Keys.V, Keys.B, Keys.N, Keys.X)},
            };

        public Keys Up { get; private set; }
        public Keys Down { get; private set; }
        public Keys Left { get; private set; }
        public Keys Right { get; private set; }
        public Keys Fire { get; private set; }

        public PlayerKeyBindings(Keys upKey, Keys leftKey, Keys downKey, Keys rightKey, Keys fireKey)
        {
            this.Up = upKey;
            this.Left = leftKey;
            this.Down = downKey;
            this.Right = rightKey;
            this.Fire = fireKey;
        }

        public static PlayerKeyBindings FromPlayerNumber(PlayerNumber n)
        {
            return mappings[n];
        }
    }

    public enum PlayerNumber
    {
        One=1, Two=2, Three=3, Four=4
    }
}