using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

namespace ldh
{
    class Level : bGameState
    {
        public Level() : base()
        {
        }

        public override void init()
        {
            entities.Add("entities", new List<bEntity>());

            _add(new TestEntity(0, 0), "entities");
        }

        protected override bool _add(bEntity e, string category)
        {
            entities[category].Add(e);

            return base._add(e, category);
        }

        public override void  update(GameTime dt)
        {
            base.update(dt);

            foreach (bEntity ge in entities["entities"])
                ge.update();
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);

            foreach (bEntity ge in entities["entities"])
                ge.render(dt, sb);
        }
    }

    class TestEntity : bEntity
    {
        bSpritemap graphic;

        public TestEntity(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            graphic = new bSpritemap(game.Content.Load<Texture2D>("fullframe"), 256, 256);
            int[] f = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
            graphic.add(new bAnim("idle", f));
            graphic.add(new bAnim("idle-slow", f, 0.2f));
            graphic.play("idle");

            base.init();
        }

        public override void update()
        {
            base.update();

            if (input.check(Microsoft.Xna.Framework.Input.Buttons.A))
                graphic.play("idle-slow");
            else
                graphic.play("idle");

            graphic.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            graphic.render(sb, pos);
        }
    }
}
