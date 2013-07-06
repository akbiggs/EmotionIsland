using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class World
    {
        private BufferedList<Player> players = new BufferedList<Player>();
        private BufferedList<Villager> villagers = new BufferedList<Villager>();

        public World()
        {
            this.players.Add(new Player(this, new Vector2(40, 40), PlayerNumber.One));
        }

        public void Add(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferAdd((Player)obj); 
            }
        }

        public void Remove(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferRemove((Player)obj); 
            }
        }

        public void Update()
        {
            foreach (var player in this.players)
            {
                player.Update();
            }
        }

        public void Draw(SpriteBatch spr)
        {
            foreach (var player in players)
            {
                player.Draw(spr);
            }
        }
    }
}