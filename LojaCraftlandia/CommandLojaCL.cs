using AdvancedBot;
using AdvancedBot.client;
using AdvancedBot.client.Commands;
using AdvancedBot.client.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LojaCraftlandia
{
    class CommandLojaCL : CommandBase
    {
        public CommandLojaCL(MinecraftClient cli)
            : base(cli, "LojaCL", "Procura melhor item em promoção.", "lojacl")
        {
            ToggleText = "§6Macro {0}";
        }

        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(args);
            if (!IsToggled)
            {
                step = 0;
                best = null;
                lastCommand = 0;
                lastFind = 0;
            }
                return CommandResult.Success;
        }
        List<PlacaInfo> placadata = new List<PlacaInfo>();
        public PlacaInfo best = null;
        int step = 0;
        public long lastCommand = 0;
        public long antiBug = 0;
        public int stepLoja = 0;
        public bool onlySell = false;
        public long lastFind = 0;
        public bool canFind = true;
        // 0 = esperando
        // 1 = indo para placa
        // 2 = está na placa;
        public override void Tick()
        {
            if (IsToggled)
            {
                try
                {

                    Inventory inv = Client.OpenWindow;

                    if (inv == null && !canFind && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                    {
                        canFind = true;
                        step = 0;
                    }

                    if (step == 1)
                    {
                        if (best != null)
                        {
                            Entity p = Client.Player;
                            double distance = Utils.DistTo(p.PosX, p.PosY, p.PosZ, best.X, best.Y, best.Z);
                            const int MAX_DISTANCE = 80;
                            if (distance > MAX_DISTANCE + 8)
                            {
                                Client.SetPath(-671, 7, 628);
                                return;
                            }
                            if (Client.CurrentPath == null || Client.CurrentPath.Finished())
                            {
                                step = 2;
                                Task.Run(async () =>
                                {
                                    Client.Player.LookTo(best.X, best.Y, best.Z);
                                    Thread.Sleep(1000);
                                    Client.SendMessage("$clickblock " + best.X + " " + best.Y + " " + best.Z + " 0");
                                });
                                
                            }
                            if (distance >= 10)
                            {
                                Client.SetPath(best.X, best.Y - 1, best.Z);
                            }
                        }
                        else
                        {
                            if (Utils.GetTimestamp() - antiBug >= 10000)
                            {
                                antiBug = Utils.GetTimestamp();
                                Client.SetPath(0, 100, 0);
                            }
                        }
                    }
                   
                    if (inv != null && (inv.Title.Contains("Comprar") && inv.Title.Contains("Vender")))
                    {
                        int delay = new Random().Next(1200, 2000);
                        if (Utils.GetTimestamp() - lastCommand >= delay)
                        {

                            lastCommand = Utils.GetTimestamp();
                            if (onlySell)
                            {
                                Task.Run(async () =>
                                {
                                    Thread.Sleep(2000);
                                    inv.Click(Client, (short)7, false, true);
                                    Thread.Sleep(1000);
                                    Client.SendPacket(new PacketCloseWindow(Client.OpenWindow.WindowID));
                                    Client.OpenWindow = null;
                                    Inventory.ClickedItem = null;
                                    canFind = true;
                                });
                            }
                            else
                            {
                                Task.Run(async () =>
                                {
                                    inv.Click(Client, (short)2, false, true);
                                    Thread.Sleep(new Random().Next(delay / 4, delay));
                                    inv.Click(Client, (short)7, false, true);
                                    Thread.Sleep(new Random().Next(delay / 2, delay));
                                    inv.Click(Client, (short)7, false, true);
                                    canFind = true;
                                });
                            }
                            
                        }
                    }
                    else
                    {
                        step = 0;
                        canFind = true;
                    }

                    if (inv == null && step == 0 || (!inv.Title.Contains("Comprar") && !inv.Title.Contains("Vender")) && step == 0)
                    {
                        if (!canFind) return;
                        if (Utils.GetTimestamp() - lastFind <= 5000) return;
                        lastFind = Utils.GetTimestamp();
                        List<PlacaInfo> placaInfos = new List<PlacaInfo>();
                        Client.PrintToChat("§eProcurando melhor item em promoção...");
                        for (int x = -150; x <= 150; x++)
                        {
                            for (int y = -5; y <= 5; y++)
                            {
                                for (int z = -150; z <= 150; z++)
                                {
                                    try
                                    {
                                        int PosX = Client.Player.BlockX + x;
                                        int PosY = Client.Player.BlockY + y;
                                        int PosZ = Client.Player.BlockZ + z;
                                        string[] sign = Client.World.GetSignText(PosX, PosY, PosZ);
                                        if (sign != null)
                                        {
                                            if (sign[0].Contains("Comprar:") && sign[2].Contains("Vender:"))
                                            {
                                                PlacaInfo placaInfo = new PlacaInfo()
                                                {
                                                    compra = Double.Parse(Utils.StripColorCodes(sign[1])),
                                                    venda = Double.Parse(Utils.StripColorCodes(sign[3])),
                                                    X = PosX,
                                                    Y = PosY,
                                                    Z = PosZ,
                                                };
                                                foreach(ItemFrame itemFrame in Main.itemFrames.Values)
                                                {
                                                    if(itemFrame.DisplayedItem != null)
                                                    {
                                                        if(itemFrame.X == PosX && itemFrame.Z == PosZ)
                                                        {
                                                            placaInfo.item = itemFrame.DisplayedItem;

                                                        }
                                                    }
                                                }
                                                if (byCoords(PosX, PosY + 1, PosZ) == null)
                                                {
                                                   
                                                    if(placaInfo.item != null)
                                                    {
                                                        placadata.Add(placaInfo);
                                                    }
                                                }
                                                placaInfos.Add(placaInfo);
                                            }
                                        }
                                    }
                                    catch (Exception ex) { }
                                }
                            }
                        }
                        best = getBest(placaInfos);
                        if (best == null)
                        {
                            Client.PrintToChat("§cNenhuma promoção em bom preço no momento.");
                            best = getInInventory();
                            if (best != null && Client.CurrentPath == null)
                            {
                                step = 1;

                                Client.PrintToChat("§aVendendo itens do inventário enquanto não há promoções.");
                                Client.SetPath(best.X, best.Y - 1, best.Z);
                                onlySell = true;
                                canFind = false;
                            }
                            else
                            {
                                Entity p = Client.Player;
                                double distance = Utils.DistTo(p.PosX, p.PosY, p.PosZ, -678, 6, 663);
                                if (distance <= 3 && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                {
                                    Client.SetPath(-656, 6, 640);
                                }else
                                if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, -656, 6, 640) <= 3 && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                {
                                    Client.SetPath( -678, 6, 617);
                                }
                                else
                                if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, -678, 6, 617) <= 3  && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                {
                                    Client.SetPath(-701, 6, 640);
                                }
                                else
                                if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, -701, 6, 640) <= 3 && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                {
                                    Client.SetPath(-678, 6, 663);
                                }
                                else
                                {
                                    if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, -678, 6, 663) <= 79 && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                    {
                                        Client.SetPath(-678, 6, 663);
                                    }
                                    else if(Utils.DistTo(p.PosX, p.PosY, p.PosZ, -701, 6, 640) <= 79 && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                    {
                                        Client.SetPath(-701, 6, 640);
                                    }
                                    else if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, -656, 6, 640) <= 79 && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                    {
                                        Client.SetPath(-656, 6, 640);
                                    }
                                    else if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, -678, 6, 663) <= 79 && (Client.CurrentPath == null || Client.CurrentPath.Finished()))
                                    {
                                        Client.SetPath(-678, 6, 663);
                                    }
                                    else
                                    {
                                        if(Client.CurrentPath == null || Client.CurrentPath.Finished()){
                                            Client.SendMessage("/home loja");
                                        }
                                    }
                                }


                            }

                            return;
                        }
                        else
                        {
                            step = 1;
                            onlySell = false;
                            Client.PrintToChat("§aMelhor placa encontrada XYZ: §e" + best.X + " " + best.Y + " " + best.Z);
                            Client.PrintToChat("§cID do item: §a"+best.item.ID+" §cLucro: §e" + best.Lucro());
                            Client.SetPath(best.X, best.Y - 1, best.Z);
                            canFind = false;

                        }
                    }
                }catch(Exception ex) { }
            }
        }
        public PlacaInfo byCoords(int x, int y, int z)
        {
            foreach (PlacaInfo placaInfo in placadata)
            {
                if(placaInfo.X == x && placaInfo.Z == z && placaInfo.Y == y)
                {
                    return placaInfo;
                }
            }
            return null;
        }

            public PlacaInfo getInInventory()
        {
            Inventory pinv = Client.Inventory;
            for(int i = 0; i < pinv.Slots.Length; i++)
            {
                ItemStack item = pinv.Slots[i];
                if (item != null)
                {
                    foreach (PlacaInfo placaInfo in placadata)
                    {
                        if(placaInfo.item != null)
                        {
                            if(placaInfo.item.ID == item.ID)
                            {
                                return placaInfo;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public PlacaInfo getBest(List<PlacaInfo> list)
        {
            PlacaInfo best = null;
            foreach (PlacaInfo placaInfo in list)
            {
                if (placaInfo.Z >= 700) continue; ;
                if (placaInfo.compra >= placaInfo.venda) continue;
                if (best != null)
                {
                    if(placaInfo.Lucro() > best.Lucro())
                    {
                        best = placaInfo;
                    }
                }
                else
                {
                    best = placaInfo;
                }
            }
            return best;
        }

    }
}
