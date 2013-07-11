using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace EmotionIsland
{

    public class HighScores
    {
        public class Score{
            public DateTime time;
            public String name;
            public int livesUsed;
        }

        public List<Score> Scores;

        public HighScores()
        {
            Scores = new List<Score>();
            try
            {
                XmlReader reader = XmlReader.Create(File.OpenRead("eshighscores.xml"));
                XmlDocument doc = new XmlDocument();
                XmlNode scores = doc.ReadNode(reader);
                scores = doc.ReadNode(reader);
                foreach (XmlNode score in scores.ChildNodes)
                {
                    Scores.Add(new Score()
                    {
                        time = DateTime.Parse(score.Attributes["time"].Value.ToString()),
                        livesUsed = Int32.Parse(score.Attributes["lives"].Value.ToString()),
                        name = score.Attributes["name"].Value.ToString()
                    });
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void addScore(Score score)
        {
            Scores.Add(score);
        }

        public void saveScores()
        {
            try
            {
                XmlWriter writer = new XmlTextWriter("eshighscores.xml", Encoding.Default);
                XmlDocument doc = new XmlDocument();

                XmlNode rootNode = doc.CreateElement("scores");

                foreach (Score score in Scores)
                {
                    XmlNode scoreNode = doc.CreateElement("score");

                    XmlAttribute time = doc.CreateAttribute("time");
                    time.Value = score.time.ToString();
                    scoreNode.Attributes.Append(time);

                    XmlAttribute lives = doc.CreateAttribute("lives");
                    lives.Value = score.livesUsed.ToString();
                    scoreNode.Attributes.Append(lives);

                    XmlAttribute name = doc.CreateAttribute("name");
                    name.Value = score.name.ToString();
                    scoreNode.Attributes.Append(name);

                    rootNode.AppendChild(scoreNode);
                }

                doc.AppendChild(rootNode);
                doc.Save(writer);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
