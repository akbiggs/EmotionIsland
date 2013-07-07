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
        const int START_HEALTH = 300;

        private string lastDirection;

        public EmotionBeam Beam { get; set; }

        private EmotionType etype;
        public Emotion Emotion { get; set; }

        public override bool ShouldRemove
        {
            get { return this.Health <= 0; }
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

        public Player(World world, Vector2 pos, PlayerNumber pn) : base(world, pos, new Vector2(32, 32), TextureBin.Pixel, START_HEALTH)
        {
            
            this.PlayerNumber = pn;
            this.keyBindings = PlayerKeyBindings.FromPlayerNumber(pn);
            switch (pn)
            {
                case PlayerNumber.One: this.EmotionType = EmotionType.Angry;
                    break;
                case PlayerNumber.Two: this.EmotionType = EmotionType.Happy;
                    break;
                case PlayerNumber.Three: this.EmotionType = EmotionType.Sad;
                    break;
                case PlayerNumber.Four: this.EmotionType = EmotionType.Terrified;
                    break;
                default:
                    this.EmotionType = EmotionType.Neutral;
                    break;
            }
            this.Emotion = new Emotion(this.EmotionType);
            string spritesheetName = "characterSheetWalk_0" + (int) (this.PlayerNumber);

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

            lastDirection = "upper";
            this.ChangeAnimation("walk_upper");
        }

        public override void Update()
        {
            base.Update();

            if (this.Beam != null)
            {
                this.Beam.BasePosition = this.Position;
            }

            this.HandleMovement();

            if (Input.IsKeyDown(this.keyBindings.Fire))
            {
                this.FireWeaponAt(Input.MousePosition);
            }
            else if (this.Beam != null)
            {
                this.Beam.Stopped = true;
            }
        }

        private void FireWeaponAt(Vector2 targetPosition)
        {
            Vector2 direction = targetPosition - this.Position;
            direction.Normalize();
            if (this.Beam == null)
            {
                EmotionBeam beam = new EmotionBeam(World, this.Position, direction, EmotionType, this);
                this.Beam = beam;
                this.World.Add(this.Beam);
            }
            else
            {
                this.Beam.Direction = direction;
            }
            this.Beam.Stopped = false;
        }

        private void HandleMovement()
        {
            this.Velocity = Vector2.Zero;

            const int moveSpeed = 3;
            if (Input.IsKeyDown(this.keyBindings.Up))
            {
                this.Velocity = new Vector2(this.Velocity.X, -moveSpeed);
            }
            else if (Input.IsKeyDown(this.keyBindings.Down))
            {
                this.Velocity = new Vector2(this.Velocity.X, moveSpeed);
            }

            if (Input.IsKeyDown(this.keyBindings.Left))
            {
                this.Velocity = new Vector2(-moveSpeed, this.Velocity.Y);
            }
            else if (Input.IsKeyDown(this.keyBindings.Right))
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

        #region Collisions
        public override void OnCollide(GameObject obj)
        {
            if (obj is SlashAttack)
            {
                this.SubTypeCollide((SlashAttack)obj);
            }
        }

        private void SubTypeCollide(SlashAttack attack)
        {
            this.TakeDamage(1, attack.Direction);
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