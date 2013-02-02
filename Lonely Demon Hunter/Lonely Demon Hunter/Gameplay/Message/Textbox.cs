using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;

namespace ldh.Gameplay.Message
{
    class Textbox : Entity
    {
        public string text;
        public Queue<string> messages;
        public bool active;
        public Entity relativeTo;
        public bool dontShowYet;

        public int borderSize;

        protected Texture2D ownershipMarker;

        public Textbox(int x, int y, string message, Entity relativeTo = null, bool dontShowYet = false)
            : this(x, y, new Queue<string>(new List<string> { message }), relativeTo, dontShowYet)
        {
        }

        public Textbox(int x, int y, Queue<string> messages, Entity relativeTo = null, bool dontShowYet = false)
            : base(x, y)
        {
            this.text = messages.Dequeue();
            this.messages = messages;
            this.active = false;
            color = Color.Wheat;
            borderSize = 8;

            this.dontShowYet = dontShowYet;
            this.relativeTo = relativeTo;

            depth = -10000;
        }

        public override void init()
        {
            base.init();

            ownershipMarker = game.Content.Load<Texture2D>("dialogOwnershipMarker");

            if (!this.dontShowYet)
                show();
        }

        public override void update()
        {
            base.update();

            if (input.pressed(Microsoft.Xna.Framework.Input.Buttons.A))
                hide();

            depth = -10000;
        }

        public void show(Entity relativeTo = null)
        {
            active = true;
            Data.getInstance().pauseEntities = true;

            if (relativeTo != null)
                this.relativeTo = relativeTo;
        }

        public void hide()
        {
            active = false;
            if (messages.Count > 0)
            {
                /*Textbox next = new Textbox(x, y, messages);*/
                /*world.add(next, "entities");*/
                text = messages.Dequeue();
                show(relativeTo);
            }
            else
            {
                Data.getInstance().pauseEntities = false;
                world.remove(this);
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            if (active)
            {
                // Get string size to act accordingly
                Vector2 size = game.gameFont.MeasureString(text);
                // Calculate position
                if (relativeTo != null)
                {
                    // There's someone to use as a reference
                    // Position with center on reference
                    // TODO: Handle cases when text gets outside of screen
                    int boxX = relativeTo.x + relativeTo.getWidth()/2 -
                            (int) size.X / 2;
                    // Position over reference
                    int boxY = relativeTo.y - ownershipMarker.Height * 2 -
                               (int)size.Y;
                    pos = new Vector2(boxX, boxY); 

                    // Position ownership marker
                    Vector2 markerPos = new Vector2(relativeTo.x + relativeTo.getWidth() - 
                                                   ownershipMarker.Width * 2,
                                                   relativeTo.y + borderSize - 
                                                   ownershipMarker.Height * 2);
                    sb.Draw(ownershipMarker, markerPos, Color.Black);
                }
                // Calculate box dimensions
                Rectangle box = new Rectangle(x-borderSize, y-borderSize, 
                                              (int)size.X+borderSize*2, 
                                              (int)size.Y+borderSize*2);
                // Render box
                sb.Draw(bDummyRect.sharedDummyRect(game), box, Color.Black);
                // Render text
                sb.DrawString(game.gameFont, text, pos, color);
            }
        }
    }
}
