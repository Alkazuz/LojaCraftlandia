using AdvancedBot.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LojaCraftlandia
{
    public class ItemFrame
    {
        public int EntityID;
        public int X, Y, Z;
        public ItemStack DisplayedItem;
        public int Direction;

        public ItemFrame(int id, double x, double y, double z, int data)
        {
            EntityID = id;
            X = Utils.Floor(x);
            Y = Utils.Floor(y);
            Z = Utils.Floor(z);
            Direction = Math.Abs(data % 4);
        }
        public override string ToString()
        {
            if(DisplayedItem != null)
            {
                return $"Pos=[{X} {Y} {Z}] Dir={Direction} ItemID={DisplayedItem.ID}";
            }
            return $"Pos=[{X} {Y} {Z}] Dir={Direction}";
        }
    }
}
