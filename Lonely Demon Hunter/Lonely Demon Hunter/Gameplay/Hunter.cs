using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

namespace ldh.Gameplay
{
    class Hunter : bEntity
    {
        bSpritemap graphic;
        String debugText;

        float walkSpeed = 2.0f;
        string facing;

        public Hunter(int x, int y)
            : base(x, y)
        {
            debugText = "";
        }

        public override void init()
        {
            base.init();

            graphic = new bSpritemap(game.Content.Load<Texture2D>("hunter"), 16, 24);
            string[] names = {"s", "sw", "w", "nw", 
                              "n", "ne", "e", "se"};
            int[][] frames = new int[][] { new int[] { 0,  1 }, new int[] { 8, 9 }, 
                                           new int[] { 16, 17 }, new int[] { 24, 25 },  
                                           new int[] { 32, 33}, new int[] {40, 41}, 
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

            graphic.play("idle-s");
        }

        public override void update()
        {
            base.update();

            bool moving = false;

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
                    actualDirection = new Vector2((float)Math.Cos(MathHelper.ToRadians(315)),
                                                  (float)Math.Sin(MathHelper.ToRadians(315)));
                    inputFacing = "ne";
                } 
                else if (inputAngle >= 69.5 && inputAngle < 112.5) 
                {
                    actualDirection = new Vector2(0, -1);
                    inputFacing = "n";
                } 
                else if (inputAngle >= 112.5 && inputAngle < 157.5) 
                {
                    actualDirection = new Vector2((float)Math.Cos(MathHelper.ToRadians(225)),
                                                  (float)Math.Sin(MathHelper.ToRadians(225)));
                    inputFacing = "nw";
                } 
                else if (inputAngle >= 157.5 && inputAngle < 202.5) 
                {
                    actualDirection = new Vector2(-1, 0);
                    inputFacing = "w";
                } 
                else if (inputAngle >= 202.5 && inputAngle < 247.5) 
                {
                    actualDirection = new Vector2((float)Math.Cos(MathHelper.ToRadians(135)),
                                                  (float)Math.Sin(MathHelper.ToRadians(135)));
                    inputFacing = "sw";
                }
                else if (inputAngle >= 247.5 && inputAngle < 292.5)
                {
                    actualDirection = new Vector2(0, 1);
                    inputFacing = "s";
                }
                else
                {
                    actualDirection = new Vector2((float)Math.Cos(MathHelper.ToRadians(45)),
                                                  (float)Math.Sin(MathHelper.ToRadians(45)));
                    inputFacing = "se";
                }

                facing = inputFacing;
            }

            if (inputDirection.Length() >= input.getJoystickDeadzone() * 1.75)
            {
                moving = true;
                pos += actualDirection * walkSpeed;
            }

            // TODO: refactor this code to use a handleGraphics() method
            string name;
            if (moving)
                name = "walk-";
            else
                name = "idle-";

            name += facing;

            graphic.play(name);
            
            graphic.update();
        }
        
        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            graphic.render(sb, pos);

            if (debugText.Length > 0)
            {
                Vector2 textPosition = pos;
                textPosition.Y -= 8;
                sb.DrawString(game.gameFont, debugText, textPosition, Color.White);
            }
        }
    }
}
