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

            this.villagers.Add(new Villager(this, new Vector2(40, 80), EmotionType.Angry));
            this.villagers.Add(new Villager(this, new Vector2(40, 120), EmotionType.Sad));
            this.villagers.Add(new Villager(this, new Vector2(40, 160), EmotionType.Happy));
            this.villagers.Add(new Villager(this, new Vector2(40, 200), EmotionType.Terrified));
            this.villagers.Add(new Villager(this, new Vector2(40, 240), EmotionType.Neutral));
        }

        public void Add(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferAdd((Player)obj);
            }
            else if (obj is Villager)
            {
                this.villagers.BufferAdd((Villager)obj);
            }
        }

        public void Remove(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferRemove((Player)obj);
            }
            else if (obj is Villager)
            {
                this.villagers.BufferRemove((Villager)obj);
            }
        }

        public void Update()
        {
            foreach (var player in this.players)
            {
                player.Update();
            }

            foreach (var villager in villagers)
            {
                villager.Anger(this.players[0]);
                villager.Update();
            }

            players.ApplyBuffers();
            villagers.ApplyBuffers();
        }

        public void Draw(SpriteBatch spr)
        {
            foreach (var player in players)
            {
                player.Draw(spr);
            }

            foreach (var villager in villagers)
            {
                villager.Draw(spr);
            }
        }
    }
}