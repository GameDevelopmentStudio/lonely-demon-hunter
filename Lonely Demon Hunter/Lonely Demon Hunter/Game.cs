using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using bEngine;

namespace ldh
{
    class Game : bGame
    {
        Level level;

        protected override void Initialize()
        {
            // TODO: Init data managers

            // TODO: Create Initial Level
            level = new Level();
            changeWorld(level);

            base.Initialize();
        }
    }
}
