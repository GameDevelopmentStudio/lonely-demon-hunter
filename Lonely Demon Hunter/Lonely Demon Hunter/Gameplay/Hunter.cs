using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;

namespace ldh.Gameplay
{
    class Hunter : Entity
    {
        bSpritemap graphic;
        bStamp shadowGraphic;

        String debugText;

        float walkSpeed = 2.0f;
        public string facing;

        public PlayerWeapon activeWeapon;
        Dictionary<int, Point> frameHotspots;

        List<string> weaponBehindPlayerDirectionList;

        public Hunter(int x, int y)
            : base(x, y)
        {
            debugText = "";
        }

        public override void init()
        {
            base.init();

            shadowGraphic = new bStamp(game.Content.Load<Texture2D>("shadow"));
            shadowGraphic.alpha = 0.4f;

            graphic = new bSpritemap(game.Content.Load<Texture2D>("hunter"), 16, 24);
            string[] names = {"s", "sw", "w", "nw", 
                              "n", "ne", "e", "se"};
            int[][] frames = new int[][] { new int[] {  0,  1 }, new int[] {  8,  9 }, 
                                           new int[] { 16, 17 }, new int[] { 24, 25 },  
                                           new int[] { 32, 33 }, new int[] { 40, 41 }, 
                                           new int[] { 48, 49 }, new int[] { 56, 57 } };
            int counter = 0;
            foreach (string name in names)
            {
                int[] tempFrames;

                // Idle animation
                tempFrames = new int[1];
                tempFrames[0] = frames[counter][0];
                graphic.add(new bAnim("idle-" + name, tempFrames, 0.0f));

                // Walk animation
                tempFrames = new int[frames[counter].Length];
                for (int i = 0; i < tempFrames.Length; i++)
                    tempFrames[tempFrames.Length - i - 1] = frames[counter][i];
                graphic.add(new bAnim("walk-" + name, tempFrames, 0.2f));

                counter++;
            }

            facing = "s";

            activeWeapon = new PlayerWeapon(game);

            graphic.play("idle-s");

            // Weapon holding related
            frameHotspots = parseFrameHotspots();
            weaponBehindPlayerDirectionList = new List<string>{"sw", "w", "nw", "n"};

            mask.w = 12;
            mask.h = 9;
            mask.offsetx = 2;
            mask.offsety = 15;
        }
        
        public override void update()
        {
            bool moving = false;
            Vector2 nextPos = pos;
            float walkSpeed = this.walkSpeed;
            if (input.check(Buttons.B))
                walkSpeed *= 2;

            Vector2 inputDirection = input.currentPadState.ThumbSticks.Left;
            inputDirection.Y *= -1;
            Vector2 actualDirection = Vector2.Zero;

            float inputAngle = 0.0f;
            string inputFacing = null;
            
            if (inputDirection.Length() >= input.getJoystickDeadzone())
            {
                inputAngle = MathHelper.ToDegrees((float)Math.Atan2(inputDirection.X, inputDirection.Y)) - 90;

                if (inputAngle < 0)
                    inputAngle += 360;

                if (inputAngle >= 0 && inputAngle < 22.5 || inputAngle >= 337.5)
                {
                    actualDirection = new Vector2(1, 0);
                    inputFacing = "e";
                }
                else if (inputAngle >= 22.5 && inputAngle < 69.5) 
                {
                    actualDirection = new Vector2(1f, -1f);
                    inputFacing = "ne";
                } 
                else if (inputAngle >= 69.5 && inputAngle < 112.5) 
                {
                    actualDirection = new Vector2(0, -1);
                    inputFacing = "n";
                } 
                else if (inputAngle >= 112.5 && inputAngle < 157.5) 
                {
                    actualDirection = new Vector2(-1f, -1f);
                    inputFacing = "nw";
                } 
                else if (inputAngle >= 157.5 && inputAngle < 202.5) 
                {
                    actualDirection = new Vector2(-1, 0);
                    inputFacing = "w";
                } 
                else if (inputAngle >= 202.5 && inputAngle < 247.5) 
                {
                    actualDirection = new Vector2(-1f, 1f);
                    inputFacing = "sw";
                }
                else if (inputAngle >= 247.5 && inputAngle < 292.5)
                {
                    actualDirection = new Vector2(0, 1);
                    inputFacing = "s";
                }
                else
                {
                    actualDirection = new Vector2(1f, 1f);
                    inputFacing = "se";
                }

                facing = inputFacing;
            }

            if (inputDirection.Length() >= input.getJoystickDeadzone() * 1.75)
            {
                moving = true;
                nextPos += actualDirection * walkSpeed;
                moveToContact(nextPos, "entities", solidCheck);
            }

            // TODO: refactor this code to use a handleGraphics() method
            string name;
            if (moving)
                name = "walk-";
            else
                name = "idle-";

            name += facing;

            graphic.play(name);
            activeWeapon.play("idle-" + facing);
            
            graphic.update();
            activeWeapon.update();

            base.update();
        }
        
        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            shadowGraphic.render(sb, x, y + 18);

            if (weaponBehindPlayerDirectionList.Contains(facing))
            {
                renderWeapon(sb);
                renderPlayer(sb);
            }
            else
            {
                renderPlayer(sb);
                renderWeapon(sb);
            }
            

            if (debugText.Length > 0)
            {
                Vector2 textPosition = pos;
                textPosition.Y -= 8;
                sb.DrawString(game.gameFont, debugText, textPosition, Color.White);
            }
        }

        protected void renderPlayer(SpriteBatch sb)
        {
            graphic.render(sb, pos);
        }

        protected void renderWeapon(SpriteBatch sb)
        {
            Point hotspot = frameHotspots[graphic.currentAnim.frame];
            Vector2 hotspotPosition = Utils.add(pos, hotspot);
            activeWeapon.render(sb, hotspotPosition);
            sb.Draw(bDummyRect.sharedDummyRect(game),
                    new Rectangle((int)hotspotPosition.X, (int)hotspotPosition.Y, 1, 1),
                    Color.Coral);
        }

        protected bool solidCheck(bEntity self, bEntity other)
        {
            if (other is Entity)
                return (other as Entity).solid;
            else
                return true;
        }

        protected static Dictionary<int, Point> parseFrameHotspots()
        {
            Dictionary<int, Point> result = new Dictionary<int, Point>();

            string fname = "Assets/hunter.cfg";
            Queue<string> lines = readFile(fname);
            foreach (string line in lines)
            {
                string[] lineData = line.Split(' ');
                result.Add(int.Parse(lineData[0]), 
                           new Point(int.Parse(lineData[1]), int.Parse(lineData[2])));
            }

            return result;
        }

        protected static Queue<string> readFile(string fname)
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

    }
}
