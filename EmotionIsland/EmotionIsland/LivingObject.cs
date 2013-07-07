using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class LivingObject : GameObject
    {
        private int invulnerableTimer = 0;
        private int healTimer = 0;

        private const float KNOCKBACK_SPEED = 3;

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

        public void Die()
        {
            this.World.Remove(this);
        }

        public override void Draw(SpriteBatch spr)
        {
            // apply a flashing effect when damaged
            if (!(this.IsInvulnerable && this.invulnerableTimer%2 == 0))
                base.Draw(spr);
        }

        public void Heal()
        {
            this.healTimer++;
            if (this.ShouldHeal)
            {
                this.Health += 1;
                this.healTimer = 0;
            }
        }
    }
}