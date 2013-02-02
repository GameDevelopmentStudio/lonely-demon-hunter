using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;

namespace ldh.Gameplay
{
    class PlayerWeapon
    {
        bGame game;

        bSpritemap graphic;

        protected Dictionary<string, List<Point>> hotspots;

        public Point currentHotspot
        {
            get
            {
                if (graphic.currentAnim == null)
                    return new Point(0, 0);
                return hotspots[graphic.currentAnim.name][graphic.currentAnim.frameIndex];
            }
        }

        public PlayerWeapon(bGame game)
        {
            this.game = game;

            hotspots = new Dictionary<string, List<Point>>();

            string weaponName = "mace";
            
            // Read file
            Queue<string> lines = readFile("Assets/" + weaponName + ".cfg");

            // Begin actual parsing 
            
            // Fetch sheet general info
            int animationsCount, width, height;
            string[] lineValues = lines.Dequeue().Split(new char[]{' '});
            
            if (lineValues.Length < 3)
                throw new Exception("Couldn't parse sheet general info line");

            animationsCount = int.Parse(lineValues[0]);
            width = int.Parse(lineValues[1]);
            height = int.Parse(lineValues[2]);

            // Create graphic with read parameters
            graphic = new bSpritemap(game.Content.Load<Texture2D>(weaponName), width, height);
            
            // Fetch and create animations
            for (int i = 0; i < animationsCount; i++)
            {
                Pair<bAnim, List<Point>> parseResult = parseAnimation(lines);
                bAnim anim = parseResult.first;
                if (anim == null)
                    throw new Exception("Could't parse animation " + i + " of " + animationsCount);
                else
                {
                    hotspots[anim.name] = parseResult.second;
                    graphic.add(anim);
                }
            }
        }

        public void update()
        {
            graphic.update();
        }

        public void render(SpriteBatch sb, Vector2 pos)
        {
            pos = Utils.subtract(pos, currentHotspot);
            graphic.render(sb, pos);
            sb.Draw(bDummyRect.sharedDummyRect(game), 
                    new Rectangle((int) pos.X, (int) pos.Y, 1, 1), 
                    Color.Red);
        }

        public void play(string animation)
        {
            if (graphic.animations.ContainsKey(animation))
                graphic.play(animation);
                
        }

        protected Queue<string> readFile(string fname)
        {
            // Read cfg file
            StreamReader reader = new StreamReader(fname);
            // line by line
            Queue<String> lines = new Queue<string>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                // Remove comments and empty lines
                int index = line.IndexOf('#');
                if (line.Length <= 0 || index == 0)
                    continue;
                else if (index > 0)
                    line = line.Substring(0, index);

                // Replace tabs with spaces
                line = line.Replace('\t', ' ');
                // Remove spaces in front and after
                line = line.Trim();
                // Re-check for empty lines
                if (line.Length <= 0)
                    continue;

                lines.Enqueue(line);
            }

            reader.Close();

            return lines;
        }

        protected Pair<bAnim, List<Point>> parseAnimation(Queue<string> lines)
        {
            Pair<bAnim, List<Point>> result;

            bAnim anim = null;
            
            string line = lines.Dequeue();
            string[] lineData = line.Split(' ');
            
            // Animation info: name frames speed
            string name = lineData[0];
            int frames = int.Parse(lineData[1]);
            float speed = float.Parse(lineData[2]);
            // There sould be enough lines to fill the expected frames
            if (frames > lines.Count)
                return null;

            // Parse each frame
            int[] frameList = new int[frames];
            List<Point> hotspots = new List<Point>();
            for (int i = 0; i < frames; i++)
            {
                // Frame info: index hotspotX hotspotY
                line = lines.Dequeue();
                lineData = line.Split(' ');
                frameList[i] = int.Parse(lineData[0]);
                hotspots.Add(new Point(int.Parse(lineData[1]), int.Parse(lineData[2])));
            }

            anim = new bAnim(name, frameList, speed, false);

            // Return result
            result = new Pair<bAnim, List<Point>>(anim, hotspots);
            return result;
        }
    }
}
