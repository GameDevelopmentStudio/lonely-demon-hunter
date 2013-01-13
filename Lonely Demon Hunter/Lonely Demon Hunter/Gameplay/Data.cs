using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace ldh.Gameplay
{
    public class Data
    {
        public string playerName;
        public Dictionary<string, bool> gameSwitches;

        public string currentMap;

        public string targetMap;
        public int targetMapX;
        public int targetMapY;
        public string targetDirection;
        public string targetTransition;
        public Color targetColor;


        protected Data()
        {
            gameSwitches = new Dictionary<string, bool>();
        }

        public void initialize()
        {
            playerName = "Hunter";
            gameSwitches.Clear();

            currentMap = "";

            // First map
            targetMap = "test";
            
            targetMapX = 7;
            targetMapY = 6;
            targetDirection = "s";
            targetTransition = "default";
            targetColor = Color.Black;
        }

        /* Singleton */
        static Data instance;
        public static Data getInstance()
        {
            if (Data.instance == null)
                Data.instance = new Data();
            return Data.instance;
        }
    }
}
