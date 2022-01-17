using AdvancedBot.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LojaCraftlandia
{
    public class PlacaInfo
    {
        public Double compra;
        public Double venda;
        public int X;
        public int Y;
        public int Z;
        public ItemStack item;

        public Double Lucro()
        {
            return venda - compra;
        }



    }
}
