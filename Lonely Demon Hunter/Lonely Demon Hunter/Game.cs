using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using bEngine;
using bEngine.Helpers.Transitions;

using ldh.Gameplay;

namespace ldh
{    
    class Game : bGame
    {
        public Data data;
        Level level;

        protected override void initSettings()
        {
            base.initSettings();

            horizontalZoom = 3;
            verticalZoom = 3;
            width = 256;
            height = 224;
        }

        protected override void Initialize()
        {
            // Init data managers
            data = Data.getInstance();
            data.initialize();
            
            // Create Initial Level
            executeTransfer();

            base.Initialize();
        }

        public void executeTransfer()
        {
            level = new Level(data.targetMap);
            changeWorld(level, getTransition(data.targetTransition));
        }

        public void setTransferData(string mapName = "", int tileX = -1, int tileY = -1, 
                                    string facing = "", string transition = "default")
        {
            data.targetMap = mapName;
            data.targetMapX = tileX;
            data.targetMapY = tileY;
            data.targetDirection = facing;
            data.targetTransition = transition;
            data.targetColor = Color.Black;
        }

        public void setTransferColor(Color color)
        {
            data.targetColor = color;
        }

        override public Transition defaultTransition()
        {
            return new FadeToColor(this, Color.Black);
        }

        public Transition getTransition(string which)
        {
            switch (which)
            {
                case "default":
                    return defaultTransition();
                case "fadeto":
                    return new FadeToColor(this, data.targetColor);
                case "blinkto":
                    return new BlinkToColor(this, data.targetColor);
                case "nil":
                    return new NilTransition(this);
            }

            return defaultTransition();
        }
    }
}