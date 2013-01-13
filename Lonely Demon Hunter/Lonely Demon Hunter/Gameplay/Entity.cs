using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using bEngine;

namespace ldh.Gameplay
{
    public class Entity : bEntity
    {
        public string tag;
        public bool solid;

        public Entity(int x, int y)
            : base(x, y)
        {
            tag = "";
            solid = false;
        }

        public static bool parseFromFile(Entity entity, XmlReader reader)
        {
            // Fill info into provided entity
            string tag;
            int id, x, y;

            try
            {
                id = int.Parse(reader.GetAttribute("id"));
                x = int.Parse(reader.GetAttribute("x"));
                y = int.Parse(reader.GetAttribute("y"));
                tag = reader.GetAttribute("tag");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return false;
            }

            entity.id = id;
            entity.tag = tag;
            entity.x = x;
            entity.y = y;

            return true;
        }
    }
}
