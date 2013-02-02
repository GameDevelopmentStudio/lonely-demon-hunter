using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ldh.Gameplay.Interactions
{
    public interface IActivable
    {
        bool activate(Entity by);
    }
}
