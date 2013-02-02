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

using ldh.Gameplay.Interactions;

namespace ldh.Gameplay
{
    class Player : Entity
    {
        bSpritemap graphic;
        bStamp shadowGraphic;

        public Vector2 midpos 
        { 
            get 
            { 
                return new Vector2(x + getWidth() / 2, y + getHeight() / 2); 
            } 
        }

        String debugText;

        float walkSpeed = 2.0f;
        public string facing;

        public PlayerWeapon activeWeapon;
        Dictionary<int, Point> frameHotspots;

        List<string> weaponBehindPlayerDirectionList;

        public enum PlayerState { Idle, Attack };
        public PlayerState state;

        public Player(int x, int y)
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

            // Add idle and walk animations
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

            // Add attack animations
            for (int i = 0; i < frames.Length; i++)
            {
                graphic.add(new bAnim("fire-" + names[i], new int[1] { i * 8 + 2 }, 0.2f, false));
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

            state = PlayerState.Idle;
        }
        
        protected Vector2 getFacingPosition(string direction = null, int delta = 8)
        {
            Vector2 position = new Vector2(x, y);
            if (direction == null)
                direction = facing;

            if (direction.IndexOf("n") >= 0)
                position.Y -= delta;
            else if (direction.IndexOf("s") >= 0)
                position.Y += delta;
            if (direction.IndexOf("w") >= 0)
                position.X -= delta;
            else if (direction.IndexOf("e") >= 0)
                position.X += delta;

            return position;
        }

        public override void update()
        {
            if (onPause())
                return;

            bool moving = false;
            Vector2 nextPos = pos;
            float walkSpeed = this.walkSpeed;
            if (input.check(Buttons.B))
                walkSpeed *= 2;
            if (state == PlayerState.Idle)
            {
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

                if (input.pressed(Buttons.X))
                {
                    IActivable activable = null;
                    
                    activable = (instancePlace(getFacingPosition(), 
                                               "entities", null, 
                                               activableCheck) as IActivable);
                    if (activable == null)
                    {
                        state = PlayerState.Attack;
                        graphic.play("fire-" + facing);
                    }
                    else
                    {
                        activable.activate(this);
                    }
                }
                else if (inputDirection.Length() >= input.getJoystickDeadzone() * 1.75)
                {
                    moving = true;
                    nextPos += actualDirection * walkSpeed;
                    moveToContact(nextPos, "entities", solidCheck);
                }
            }
            else if (state == PlayerState.Attack)
            {
                if (graphic.currentAnim.finished)
                    state = PlayerState.Idle;
            }

            // TODO: refactor this code to use a handleGraphics() method
            string name;
            if (state == PlayerState.Idle)
            {
                if (moving)
                    name = "walk-";
                else
                    name = "idle-";
            }
            else if (state == PlayerState.Attack)
            {
                name = "fire-";
            }
            else
            {
                // WTF state are you?
                name = "idle-";
            }

            name += facing;

            graphic.play(name);

            if (state == PlayerState.Idle)
                activeWeapon.play("idle-" + facing);
            else
                activeWeapon.play("fire-" + facing);
            
            graphic.update();
            activeWeapon.update();

            depth = -(y + mask.offsety);

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

        protected bool activableCheck(bEntity self, bEntity other)
        {
            return (other is IActivable);
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

        public override int getWidth()
        {
            return graphic.spriteWidth;
        }

        public override int getHeight()
        {
            return graphic.spriteHeight;
        }
    }
}
