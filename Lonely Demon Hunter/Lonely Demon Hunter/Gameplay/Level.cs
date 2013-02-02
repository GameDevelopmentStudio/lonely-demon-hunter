using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

using ldh.Gameplay.Message;
using ldh.Gameplay.Interactions;

namespace ldh.Gameplay
{
    class Level : bGameState
    {
        string mapName;

        bTilemap tilemap;
        MapCollisionMask solidmap;

        Player player;
        public bCamera2d camera;

        public List<LevelZone> zones;
        public bool isInZone(Rectangle rect, int zoneId)
        {
            foreach (LevelZone zone in zones)
            {
                if (zone.zoneId == zoneId && zone.bounds.Intersects(rect))
                    return true;
            }

            return false;
        }

        public int getZoneOf(Vector2 pos)
        {
            Point pointPos = new Point((int) pos.X, (int) pos.Y);
            foreach (LevelZone zone in zones)
            {
                if (zone.bounds.Contains(pointPos))
                    return zone.zoneId;
            }

            return -1;
        }

        public Level(string mapName) : base()
        {
            this.mapName = mapName;

            usesCamera = true;

            zones = new List<LevelZone>();
        }

        public override void init()
        {
            entities.Add("entities", new List<bEntity>());
            // entities.Add("doors", new List<bEntity>());

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
            
            // 2.2.1. Zones
            zones = config.zones;

            // 2.3. Entities
            // TODO: Replace with actual entities! hurr!
            foreach (Entity e in config.entities)
            {
                /*if (e is Door)
                    _add(e, "doors");
                else*/
                    _add(e, "entities");
            }
            
            // 2.4. Player (place correctly from Game data)
            int playerTargetX = (game as Game).data.targetMapX * 16;
            int playerTargetY = (game as Game).data.targetMapY * 16;
            player = new Player(0, 0);
            player.x = playerTargetX - 2 + (16/2) - (12/2); 
            player.y = playerTargetY - 15 + (16/2) - (8/2);
            _add(player, "entities");
            player.facing = (game as Game).data.targetDirection;

            // 2.5. Camera
            camera = new bCamera2d(game.GraphicsDevice);
            camera.bounds = new Rectangle(solidmap.x, solidmap.y, solidmap.width, solidmap.height);

            // 3. Handle state (Puzzles & so)

            // 5. Tidy up
            (game as Game).data.currentMap = (game as Game).data.targetMap;
            (game as Game).data.targetMap = "";
            (game as Game).data.targetMapX = -1;
            (game as Game).data.targetMapY = -1;

            // 4. Trigger whatever
        }

        protected override bool _add(bEntity e, string category)
        {
            entities[category].Add(e);

            return base._add(e, category);
        }

        public override void update(GameTime dt)
        {
            // Empty call
            base.update(dt);

            /* Pre-step */
            if (bGame.input.pressed(Microsoft.Xna.Framework.Input.Buttons.Start))
            {
                add(new NPC(player.x, player.y - 16), "entities");

                if (new Random().Next(2) == 0)
                {
                    Queue<string> messages = new Queue<string>();
                    messages.Enqueue("This is just");
                    messages.Enqueue("a test message");
                    messages.Enqueue("composed of various");
                    messages.Enqueue("pieces of size increasing");
                    messages.Enqueue("chunks of absolutely stupid and");
                    messages.Enqueue("uterly irrelevant randomly written phrases.");
                    Textbox box = new Textbox(64, 64, messages);
                    _add(box, "entities");
                    box.show(player);
                }
                else
                {
                    Queue<string> messages = new Queue<string>();
                    messages.Enqueue("A thousand years ago a hardened battle took place.\n"+
                                     "Warriors from all around the world came to defend\n"+
                                     "humanity from extintion.");
                    messages.Enqueue("The Demons had managed to open the Demon Gate\n" +
                                     "and where attacking the Human World!");
                    Textbox box = new Textbox((int) camera.viewRectangle.X + 16,
                                              (int)camera.viewRectangle.Y + 16, messages);
                    _add(box, "entities");
                    box.show();
                }
            }

            /* Update entities */
            foreach (bEntity ge in entities["entities"])
                ge.update();
            /*foreach (bEntity ge in entities["doors"])
                ge.update();*/

            /* Handle camera */
            Vector2 cameraPosition = new Vector2(player.x + player.getWidth() / 2, player.y + player.getHeight() / 2);
            camera.Pos = cameraPosition;

            /* Post step */
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            // base.render(dt, sb, matrix);
            matrix *= camera.get_transformation();

            sb.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null,
                    RasterizerState.CullCounterClockwise,
                    null,
                    matrix);


            tilemap.render(sb, Vector2.Zero);

            List<int> visibleZones = new List<int>();
            foreach (LevelZone zone in zones)
            {
                if (!isInZone(player.mask.rect, zone.zoneId))
                {
                    sb.Draw(bDummyRect.sharedDummyRect(game), zone.bounds, Color.Black);
                }
                else
                {
                    visibleZones.Add(zone.zoneId);
                }
            }

            entities["entities"].Sort(delegate(bEntity a, bEntity b)
            {
                return (b as Entity).depth - (a as Entity).depth;
            });

            foreach (bEntity ge in entities["entities"])
            {
                ge.render(dt, sb);
            }
            
            // Hide zones not in view
            // int playerZone = getZoneOf(player.midpos);
            /*List<int> visibleZones = new List<int>();
            foreach (LevelZone zone in zones)
            {
                if (!isInZone(player.mask.rect, zone.zoneId))
                {
                    sb.Draw(bDummyRect.sharedDummyRect(game), zone.bounds, Color.Black);
                }
                else
                {
                    visibleZones.Add(zone.zoneId);
                }
            }*/

            /* Entities are invisible when on darkness
             * but it is automanaged
             * (Or managed wherever else, at least!)
             * /
            // Doors are over the darkness
            foreach (Door d in entities["doors"])
            {
                foreach (int index in visibleZones)
                    if (isInZone(d.mask.rect, index))
                        d.render(dt, sb);
            }*/
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
            List<LevelZone> zones = new List<LevelZone>();

            using (var stream = System.IO.File.OpenText(filename))
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (parseStack.Count > 0 && parseStack.Peek() == "Entities")
                        {
                            Console.WriteLine("Parsing entity");
                            Entity e = parseEntity(reader);
                            if (e != null)
                                entities.Add(e);
                        }
                        else if (parseStack.Count > 0 && parseStack.Peek() == "Zones")
                        {
                            Console.WriteLine("Parsing zone");
                            LevelZone zone = new LevelZone();
                            if (LevelZone.parseFromFile(zone, reader))
                                zones.Add(zone);
                        }
                        else
                        {
                            Console.WriteLine("New element: " + reader.Name);
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
                                case "Zones":
                                    break;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        Console.WriteLine("Text of " + parseStack.Peek());
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
                        Console.WriteLine("End element of " + parseStack.Peek());
                        parseStack.Pop();
                    }
                }

                LevelConfig levelConfig = new LevelConfig(w, h, tiles, solids, entities, zones);
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
                    case "Door":
                        e = new Door(0, 0);
                        Door.parseFromFile((e as Door), element);
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

    class DepthSorter : Comparer<Entity>
    {
        override public int Compare(Entity a, Entity b)
        {
            return a.depth - b.depth;
        }
    }

    class LevelConfig
    {
        public int width;
        public int height;
        public string[] tileData;
        public string[] solidData;
        public List<Entity> entities;
        public List<LevelZone> zones;

        public LevelConfig(int width, int height, 
                           string[] tileData, string[] solidData, 
                           List<Entity> entities, List<LevelZone> zones)
        {
            this.width = width;
            this.height = height;
            this.tileData = tileData;
            this.solidData = solidData;
            this.entities = entities;
            this.zones = zones;
        }
    }

    class MapCollisionMask : Entity
    {
        int tileWidth, tileHeight;
        public int width, height;
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

    class LevelZone
    {
        public int zoneId;
        public Rectangle bounds;

        public LevelZone(int id, Rectangle bounds)
        {
            this.zoneId = id;
            this.bounds = bounds;
        }

        public LevelZone() : this(-1, Rectangle.Empty)
        {

        }

        public static bool parseFromFile(LevelZone zone, XmlReader reader)
        {
            // Fill info into provided entity
            int id = -1;
            int x = -1, y = -1;
            int x2 = -1, y2 = -1;

            try
            {
                id = int.Parse(reader.GetAttribute("zoneId"));
                x = int.Parse(reader.GetAttribute("x"));
                y = int.Parse(reader.GetAttribute("y"));

                Console.WriteLine("Parsing zone " + id);
                bool done = false;
                while (!done)
                {
                    reader.Read();
                    if (reader.NodeType == XmlNodeType.Element &&
                        reader.Name == "node")
                    {
                        x2 = int.Parse(reader.GetAttribute("x"));
                        y2 = int.Parse(reader.GetAttribute("y"));
                    }
                    done = (reader.NodeType == XmlNodeType.EndElement &&
                           reader.Name == "Zone");
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return false;
            }

            zone.zoneId = id;
            zone.bounds.X = x;
            zone.bounds.Y = y;
            zone.bounds.Width = x2 - x;
            zone.bounds.Height = y2 - y;

            return true;
        }
    }
}
