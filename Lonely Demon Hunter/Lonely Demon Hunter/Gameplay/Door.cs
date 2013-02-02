using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using System.Xml;

namespace ldh.Gameplay
{
    class Door : Entity
    {
        public bSpritemap graphic;
        public enum Orientation { Front, Side };
        public Orientation orientation;
        public bool open;

        public Dictionary<string, Vector2> graphicPosition;

        public Door(int x, int y)
            : base(x, y)
        {
            graphicPosition = new Dictionary<string,Vector2>();
            orientation = Orientation.Front;
        }

        public override void init()
        {
            base.init();
            
            graphic = new bSpritemap(game.Content.Load<Texture2D>("door"), 16, 34);
            graphic.add(new bAnim("closed-front", new int[]{0}));
            graphic.add(new bAnim("open-front", new int[]{1}));
            graphic.add(new bAnim("closed-side", new int[]{1}));
            graphic.add(new bAnim("open-side", new int[]{0}));

            graphicPosition.Add("closed-front", new Vector2(0, 1));
            graphicPosition.Add("open-front", new Vector2(1, 2));
            graphicPosition.Add("closed-side", new Vector2(0, 2));
            graphicPosition.Add("open-side", new Vector2(0, 12));

            open = false;
        }

        public override void update()
        {
            base.update();

            open = placeMeeting(x, y, "entities", isPlayerEntity);

            graphic.play((open ? "open" : "closed") + "-" +
                        orientation.ToString().ToLower());

            switch (orientation)
            {
                case Orientation.Front:
                    mask.rect.Width = 16;
                    mask.rect.Height = 24;
                    mask.offsetx = 0;
                    mask.offsety = 8;
                    break;
                case Orientation.Side:
                    mask.rect.Width = 4;
                    mask.rect.Height = 32;
                   /* mask.offsetx = -2;
                    mask.offsety = 18;*/
                    break;
            }

            depth = -(y + mask.offsety);
        }

        protected bool isPlayerEntity(bEntity self, bEntity other)
        {
            return (other is Player);
        }

        public static bool parseFromFile(Door entity, XmlReader reader)
        {
            // Fill info into provided entity
            int x, y;
            string orientation;

            try
            {
                x = int.Parse(reader.GetAttribute("x"));
                y = int.Parse(reader.GetAttribute("y"));
                orientation = reader.GetAttribute("orientation");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return false;
            }

            entity.x = x;
            entity.y = y;
            if (orientation == "Front")
                entity.orientation = Orientation.Front;
            else
                entity.orientation = Orientation.Side;

            return true;
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            graphic.render(sb, pos - graphicPosition[graphic.currentAnim.name]);
        }
    }
}
