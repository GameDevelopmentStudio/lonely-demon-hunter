using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

namespace ldh.Gameplay
{
    class Level : bGameState
    {
        string mapName;

        bTilemap tilemap;
        MapCollisionMask solidmap;

        public Level(string mapName) : base()
        {
            this.mapName = mapName;
        }

        public override void init()
        {
            entities.Add("entities", new List<bEntity>());

            // 1. Parse level file
            LevelConfig config = parseLevelFile(mapName);
            
            // 2. Create level elements
            // 2.1. Tilemap
            tilemap = new bTilemap(config.width, config.height, 16, 16, 
                                   game.Content.Load<Texture2D>("tset-test"));
            tilemap.parseTiles(config.tileData);
            
            // 2.2. Solids
            solidmap = new MapCollisionMask(0, 0, config.width, 
                                            config.height, 8, 8, 
                                            config.solidData);
            _add(solidmap, "entities");
            
            // 2.3. Entities
            // TODO: Replace with actual entities! hurr!
            foreach (Entity e in config.entities)
            {
                _add(e, "entities");
            }
            
            // 2.4. Player (place correctly from Game data)
            int playerTargetX = (game as Game).data.targetMapX * 16;
            int playerTargetY = (game as Game).data.targetMapY * 16;
            Hunter player = new Hunter(0, 0);
            player.x = playerTargetX - 2 + (16/2) - (12/2); 
            player.y = playerTargetY - 15 + (16/2) - (8/2);
            _add(player, "entities");
            player.facing = (game as Game).data.targetDirection;

            (game as Game).data.currentMap = (game as Game).data.targetMap;
            (game as Game).data.targetMap = "";
            (game as Game).data.targetMapX = -1;
            (game as Game).data.targetMapY = -1;
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

            tilemap.render(sb, Vector2.Zero);

            foreach (bEntity ge in entities["entities"])
                ge.render(dt, sb);
        }

        protected static LevelConfig parseLevelFile(string mapName)
        {
            String filename = "Assets/maps/" + mapName + ".oel";

            Stack<String> parseStack = new Stack<String>();

            int w = 0, h = 0;
            string tileset;
            string exportMode;
            string[] tiles = { "" }, solids = { "" };
            List<Entity> entities = new List<Entity>();

            using (var stream = System.IO.File.OpenText(filename))
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (parseStack.Count > 0 && parseStack.Peek() == "Entities")
                        {
                            Entity e = parseEntity(reader);
                            if (e != null)
                                entities.Add(e);
                        }
                        else
                        {
                            parseStack.Push(reader.Name);

                            switch (reader.Name)
                            {
                                case "level":
                                    w = int.Parse(reader.GetAttribute("width"));
                                    h = int.Parse(reader.GetAttribute("height"));
                                    break;
                                case "Tiles":
                                    tileset = reader.GetAttribute("tileset");
                                    break;
                                case "Solids":
                                    exportMode = reader.GetAttribute("exportMode");
                                    break;
                                case "Entities":
                                    break;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        String current = parseStack.Pop();
                        switch (current)
                        {
                            case "level":
                                break;
                            case "Tiles":
                                string v = reader.Value;
                                tiles = v.Split('\n');
                                break;
                            case "Solids":
                                v = reader.Value;
                                solids = v.Split('\n');
                                break;
                        }
                        parseStack.Push(current);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        parseStack.Pop();
                    }
                }

                LevelConfig levelConfig = new LevelConfig(w, h, tiles, solids, entities);
                return levelConfig;
            }
        }

        public static Entity parseEntity(XmlReader element)
        {
            Entity e = null;

            // Fetch common attributes
            {
                switch (element.Name)
                {
                    case "Transfer":
                        e = new Transfer(0, 0);
                        Transfer.parseFromFile((e as Transfer), element);
                        break;
                    default:
                        e = new Entity(0, 0);
                        Entity.parseFromFile(e, element);
                        break;
                }
            }

            return e;
        }
    }

    class LevelConfig
    {
        public int width;
        public int height;
        public string[] tileData;
        public string[] solidData;
        public List<Entity> entities;

        public LevelConfig(int width, int height, 
                           string[] tileData, string[] solidData, 
                           List<Entity> entities)
        {
            this.width = width;
            this.height = height;
            this.tileData = tileData;
            this.solidData = solidData;
            this.entities = entities;
        }
    }

    class MapCollisionMask : Entity
    {
        int tileWidth, tileHeight;
        int width, height;
        string[] data;

        public MapCollisionMask(int x, int y, int width, int height, 
                                int tileWidth, int tileHeight, string[] data)
            : base(x, y)
        {
            solid = true;

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.width = width;
            this.height = height;
            this.data = data;
        }

        public override void init()
        {
            mask = new bSolidGrid(width / tileWidth, height / tileHeight, 
                                  tileWidth, tileHeight);
            if (!(mask as bSolidGrid).parseSolids(data))
            {
                Console.WriteLine("Solid data parsing failed!");
                world.remove(this);
            }

            base.init();
        }
    }
}
