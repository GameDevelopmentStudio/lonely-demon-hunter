using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;

namespace ldh.Gameplay.Interactions
{
    public class NPC : Entity, IActivable
    {
        public bSpritemap graphic;
        public bStamp shadowGraphic;

        public NPC(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            graphic = new bSpritemap(game.Content.Load<Texture2D>("npc"), 16, 24);
            graphic.add(new bAnim("idle", new int[] { 0, 1 }, 0.3f));
            graphic.play("idle");

            shadowGraphic = new bStamp(game.Content.Load<Texture2D>("shadow"));
            shadowGraphic.alpha = 0.4f;

            /*mask.offsety = 8;
            mask.w = 16;
            mask.h = 16;*/

            mask.w = 12;
            mask.h = 9;
            mask.offsetx = 2;
            mask.offsety = 15;

            solid = true;

            depth = -(y + mask.offsety);
        }

        public override void update()
        {
            base.update();

            depth = -(y + mask.offsety);
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            shadowGraphic.render(sb, x, y + 18);
            graphic.render(sb, pos);
        }

        public bool activate(Entity by)
        {
            world.add(new Message.Textbox(x, y, "Howdy1", this), "entities");

            return true;
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
