using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public class BeamParticle : Particle 
    {
        public EmotionType EmotionType { get; set; }
        public EmotionBeam OwnerBeam { get; set; }
        public BeamParticle(World world, EmotionBeam owner, EmotionType type, Vector2 position, Vector2 direction) 
            : base(world, position, new Vector2(12, 12), TextureBin.Pixel, direction * 5, -1)
        {
            this.OwnerBeam = owner;
            this.EmotionType = type;
        }
    }
}