using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class EmotionBeam
    {
        private const int PARTICLE_DELAY = 5;

        public bool Stopped { get { return this.stopped; } set
        {
            this.stopped = value; if (value) this.Timer = 0; } }

        public BufferedList<BeamParticle> Particles = new BufferedList<BeamParticle>();
        private bool stopped;

        public World World { get; set; }
        public Vector2 BasePosition { get; set; }
        public Emotion Emotion { get; set; }
        public Vector2 Direction { get; set; }
        public Color Color { get; set; }

        public Player Owner { get; set; }

        public int Timer { get; set; }

        public EmotionBeam(World world, Vector2 pos, Vector2 direction, EmotionType emotionType, Player owner)
        {
            this.Owner = owner;
            this.World = world;
            this.BasePosition = pos;
            this.Emotion = new Emotion(emotionType);

            direction.Normalize();
            this.Direction = direction;

            this.Timer = 0;
            this.Color = Emotion.ToColor();
        }

        public void Update()
        {
            if (!this.Stopped)
            {
                if (this.Timer%PARTICLE_DELAY == 0)
                {
                    this.SpawnParticle();
                }
                this.Timer++;
            }

            foreach (var particle in this.Particles)
            {
                particle.Update();
            }

            Particles.ApplyBuffers();
        }

        private void SpawnParticle()
        {
            BeamParticle particle = new BeamParticle(this.World, this, this.Emotion.EmotionType, this.BasePosition,
                                                     this.Direction);
            particle.Color = this.Color;
            this.Particles.Add(particle);
        }

        public void Draw(SpriteBatch spr)
        {
            foreach (var particle in this.Particles)
            {
                particle.Draw(spr);
            }
        }
    }
}