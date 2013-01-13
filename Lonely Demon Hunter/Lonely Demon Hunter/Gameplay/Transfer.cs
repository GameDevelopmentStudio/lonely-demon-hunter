using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;

using bEngine.Helpers;

namespace ldh.Gameplay
{
    public class Transfer : Entity
    {
        public string targetMap;
        public int targetX;
        public int targetY;
        public string targetDirection;
        public string transition;

        public Transfer(int x, int y)
            : base(x, y)
        {
            targetMap = "";
            targetDirection = "";

            targetX = -1;
            targetY = -1;
        }

        public override void init()
        {
            base.init();

            mask.w = 16;
            mask.h = 16;
        }

        public override void update()
        {
            base.update();

            if (placeMeeting(pos, "entities", playerCheck))
            {
                if (targetMap.Length > 0 && targetX >= 0 && targetY >= 0)
                {
                    (game as Game).setTransferData(targetMap, targetX, targetY, targetDirection, transition);
                    (game as Game).setTransferColor(color);
                    (game as Game).executeTransfer();
                }
            }
        }

        protected bool playerCheck(bEngine.bEntity self, bEngine.bEntity other)
        {
            return (other is Hunter) && (mask.rect.Contains(other.mask.rect));
        }

        public static bool parseFromFile(Transfer e, XmlReader reader)
        {
            // Parse basic data
            Entity.parseFromFile(e, reader);

            // Parse teleport data
            e.targetMap = reader.GetAttribute("TargetMap");
            e.targetDirection = reader.GetAttribute("Facing");
            e.targetX = int.Parse(reader.GetAttribute("TargetX"));
            e.targetY = int.Parse(reader.GetAttribute("TargetY"));
            e.transition = reader.GetAttribute("Transition");
            if (reader.GetAttribute("Color") == "#FFFF00")
                e.color = Color.DarkKhaki;
            else
                e.color = Utils.stringToColor(reader.GetAttribute("Color"));

            return true;
        }
    }
}
