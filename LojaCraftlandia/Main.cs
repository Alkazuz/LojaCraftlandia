using AdvancedBot;
using AdvancedBot.client;
using AdvancedBot.Plugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LojaCraftlandia
{
    public class Main : IPlugin
    {
        public static ConcurrentDictionary<int, ItemFrame> itemFrames = new ConcurrentDictionary<int, ItemFrame>();
        CommandLojaCL command;
        public void onClientConnect(MinecraftClient client)
        {
            if (client.CmdManager.GetCommand("lojacl") == null)
            {
                command = new CommandLojaCL(client);
                client.CmdManager.Commands.Add(command);
            }
        }

        public void onReceiveChat(string chat, byte pos, MinecraftClient client)
        {
        }

        public void OnReceivePacket(ReadBuffer pkt, MinecraftClient client)
        {
            switch (pkt.ID)
            {
                case 0x0E:
                    { //spawn object
                        int entityId = pkt.ReadVarInt();
                        if (pkt.ReadByte() == 71)
                        {
                            double x = pkt.ReadInt() / 32.0;
                            double y = pkt.ReadInt() / 32.0;
                            double z = pkt.ReadInt() / 32.0;
                            pkt.Skip(2); //pitch, yaw
                            int data = pkt.ReadInt();
                            itemFrames[entityId] = new ItemFrame(entityId, x, y, z, data);
                        }
                        break;
                    }
                case 0x1C:
                    { //entity metadata
                        int entityId = pkt.ReadVarInt();
                        if (itemFrames.TryGetValue(entityId, out var frame))
                        {
                            for (byte item; (item = pkt.ReadByte()) != 0x7F;)
                            {
                                int index = item & 0x1F;
                                int metaType = item >> 5;

                                switch (metaType)
                                {
                                    case 0: pkt.ReadByte(); break;
                                    case 1: pkt.ReadShort(); break;
                                    case 2: pkt.ReadInt(); break;
                                    case 3: pkt.ReadFloat(); break;
                                    case 4: pkt.ReadString(); break;
                                    case 5:
                                        var stack = pkt.ReadItemStack();
                                        if (index == 8)
                                        {
                                            frame.DisplayedItem = stack;
                                            Program.FrmMain.DebugConsole(frame.ToString());
                                        }
                                        break;
                                    case 6:
                                    case 7: pkt.Skip(12); break;
                                }
                            }
                        }
                        break;
                    }
                default: break; ;
            }
        }

        public void onSendChat(string chat, MinecraftClient client)
        {
        }

        public void OnSendPacket(IPacket packet, MinecraftClient client)
        {
        }

        public void Tick()
        {
        }

        public void Unload()
        {
        }
    }
}
