using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class LivingObject : GameObject
    {
        private int invulnerableTimer = 0;
        private int healTimer = 0;

        public int MaxHealth { get; set; }

        private const float KNOCKBACK_SPEED = 2f;

        public bool ShouldHeal
        {
            get { return healTimer >= 30; }
        }

        public bool IsInvulnerable
        {
            get { return this.invulnerableTimer > 0; }
        }

        public int Health { get; private set; }
        public override bool IsAlive { 
            get { return Health > 0; }
            set { 
                if (!value && Health > 0)
                {
                    Health = 0;
                    this.Die();
                };
            } 
        }
        public override bool ShouldRemove { get { return !this.IsAlive; } }

        public LivingObject(World world, Vector2 pos, Vector2 size, Texture2D tex, int health)
            : base(world, pos, size, tex)
        {
            this.Health = health;
            this.MaxHealth = health + 2;
        }

        public override void Update()
        {
            if (this.IsInvulnerable)
            {
                this.invulnerableTimer--;
            }
            base.Update();
        }

        public void TakeDamage(int amount)
        {
            this.TakeDamage(amount, Vector2.Zero);
        }

        public void TakeDamage(int amount, Vector2 direction)
        {
            direction.Normalize();
            if (!this.IsInvulnerable)
            {
                this.Health -= amount;
                this.MakeInvulnerable();
                if (this.Health <= 0)
                {
                    this.Die();
                }
                else
                {
                    if (direction != Vector2.Zero)
                    {
                        this.Velocity = direction*KNOCKBACK_SPEED;
                    }
                    this.PreventMovement(50);
                }
            }
        }

        public virtual void MakeInvulnerable()
        {
            this.invulnerableTimer = 100;
        }

        public virtual void Die()
        {
            for (int i = 0; i < MathExtra.RandomInt(5, 10); i++)
            {
                World.Add(new Bullet(this.World, this.Position, new Vector2(MathExtra.RandomInt(12, 16)), TextureBin.Pixel, null, 50, MathExtra.RandomFloat()*2 + 2, new Vector2(MathExtra.RandomFloat() * (MathExtra.RandomBool() ? -1 : 1), MathExtra.RandomFloat() * (MathExtra.RandomBool() ? -1 : 1))));
            }
            this.World.Remove(this);
        }

        public override void Draw(SpriteBatch spr)
        {
            // apply a flashing effect when damaged
            //if (!(this.IsInvulnerable && this.invulnerableTimer%2 == 0))
                base.Draw(spr);
        }

        public void Heal()
        {
            this.healTimer++;
            if (this.ShouldHeal && this.Health != this.MaxHealth)
            {
                this.Health += 1;
                this.healTimer = 0;
            }
        }
    }
}