using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EmotionIsland
{
    public class Player : LivingObject
    {
        const int START_HEALTH = 3;

        public EmotionBeam Beam { get; set; }

        private EmotionType etype;
        public Emotion Emotion { get; set; }

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
                World.Remove(this.Beam);
                this.Beam = null;
            }
        }

        private void FireWeaponAt(Vector2 targetPosition)
        {
            if (this.Beam == null)
            {
                Vector2 direction = targetPosition - this.Position;
                direction.Normalize();
                EmotionBeam beam = new EmotionBeam(World, this.Position, direction, EmotionType);
                this.Beam = beam;
                World.Add(this.Beam);
            }
        }

        private void HandleMovement()
        {
            if (Input.IsKeyDown(this.keyBindings.Up))
            {
                this.Position = new Vector2(this.Position.X, this.Position.Y - 5);
            }
            else if (Input.IsKeyDown(this.keyBindings.Down))
            {
                this.Position = new Vector2(this.Position.X, this.Position.Y + 5);
            }

            if (Input.IsKeyDown(this.keyBindings.Left))
            {
                this.Position = new Vector2(this.Position.X - 5, this.Position.Y);
            }
            else if (Input.IsKeyDown(this.keyBindings.Right))
            {
                this.Position = new Vector2(this.Position.X + 5, this.Position.Y);
            }
        }
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
        One, Two, Three, Four
    }
}