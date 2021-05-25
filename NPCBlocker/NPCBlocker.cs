/*
 * 
 * Welcome to this template plugin.
 * Level: All Levels
 * Purpose: To get a working model for new plugins to be built off.  This plugin will
 * compile immediately, all you have to do is rename TemplatePlugin to reflect 
 * the purpose of the plugin.
 * 
 * You may need to delete the references to TerrariaServer and TShockAPI.  They 
 * could be pointing to my current folder.  Just remove them and then right-click the
 * references folder, go to browse go to the dll folder, and select both.
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using Microsoft.Xna.Framework; // For the SendMessage colors
using System.Linq;

namespace NPCBlocker
{
    //[ApiVersion(1, 17)]
    [ApiVersion(2, 1)]
    public class NPCBlocker : TerrariaPlugin
    {
        private IDbConnection db;
        private Dictionary<int, int> blockedNPC = new Dictionary<int, int>();
        public static Config ConfigFile = new Config();

        public override Version Version
        {
            get { return new Version(2, 1); }
        }

        public override string Name
        {
            get { return "NPC Blocker"; }
        }

        public override string Author
        {
            get { return "Olink, updated by moisterrific + Moneylover3246"; }
        }

        public override string Description
        {
            get { return "Bans specific NPCs from spawning."; }
        }

        public NPCBlocker(Main game)
            : base(game)
        {
            Order = 4;
        }

        public override void Initialize()
        {
            //Commands.ChatCommands.Add(new Command("resnpc", AddNpc, "blacknpc", "blocknpc", "bannpc")); 
            Commands.ChatCommands.Add(new Command("plugin.npc.blocker", AddNpc, "blocknpc", "bannpc", "banmob"));
            //Commands.ChatCommands.Add(new Command("resnpc", DelNpc, "whitenpc", "unblocknpc", "unbannpc"));
            Commands.ChatCommands.Add(new Command("plugin.npc.blocker", DelNpc, "unblocknpc", "unbannpc", "unbanmob"));
            Commands.ChatCommands.Add(new Command("plugin.npc.blocker", Reload, "npc-reload"));
            ServerApi.Hooks.NpcSpawn.Register(this, OnSpawn);
            ServerApi.Hooks.NpcTransform.Register(this, OnTransform);
            StartDB();
            ConfigFile = Config.Read(Path.Combine(TShock.SavePath, "NPCBlocker.json"));
        }

        public void StartDB()
        {
            SetupDb();
            ReadDb();
        }

        private void SetupDb()
        {
            //if (TShock.Config.StorageType.ToLower() == "sqlite")
            if (TShock.Config.Settings.StorageType.ToLower() == "sqlite")
            {
                string sql = Path.Combine(TShock.SavePath, "npc_blocker.sqlite");
                db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
            }
            //else if (TShock.Config.StorageType.ToLower() == "mysql")
            else if (TShock.Config.Settings.StorageType.ToLower() == "mysql")
            {
                try
                {
                    //var hostport = TShock.Config.MySqlHost.Split(':');
                    var hostport = TShock.Config.Settings.MySqlHost.Split(':');
                    db = new MySqlConnection();
                    db.ConnectionString =
                        String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                                      hostport[0],
                                      hostport.Length > 1 ? hostport[1] : "3306",
                                      //TShock.Config.MySqlDbName,
                                      TShock.Config.Settings.MySqlDbName,
                                      TShock.Config.Settings.MySqlUsername,
                                      TShock.Config.Settings.MySqlPassword
                            );
                }
                catch (MySqlException ex)
                {
                    //Log.Error(ex.ToString());
                    TShock.Log.Error(ex.ToString());
                    throw new Exception("MySql not setup correctly");
                }
            }
            else
            {
                throw new Exception("Invalid storage type");
            }

            var table2 = new SqlTable("Blocked_NPC",
                                     new SqlColumn("ID", MySqlDbType.Int32),
                                     new SqlColumn("AmounttoSpawn", MySqlDbType.Int32)
                );
            var creator2 = new SqlTableCreator(db,
                                              db.GetSqlType() == SqlType.Sqlite
                                                ? (IQueryBuilder)new SqliteQueryCreator()
                                                : new MysqlQueryCreator());
            //creator2.EnsureExists(table2);
            creator2.EnsureTableStructure(table2);
        }

        private void ReadDb()
        {
            //String query = "SELECT * FROM Blocked_NPC";
            string query = "SELECT * FROM Blocked_NPC";

            var reader = db.QueryReader(query);

            while (reader.Read())
            {
                int id = reader.Get<int>("ID");
                int amount = reader.Get<int>("AmounttoSpawn");
                blockedNPC.Add(id, amount);
            }
        }

        private void Reload(CommandArgs args)
        {
            ConfigFile = Config.Read(Path.Combine(TShock.SavePath, "NPCBlocker.json"));
        }

        private void AddNpc(CommandArgs args)
        {
            //if (args.Parameters.Count < 1)
            //{
            //    args.Player.SendMessage("You must specify a npc id to add.", Color.Red);
            //    return;
            //}
            //string tile = args.Parameters[0];
            //int id;
            //if (!int.TryParse(tile, out id))
            //{
            //    args.Player.SendMessage(String.Format("Npc id '{0}' is not a valid number.", id), Color.Red);
            //    return;
            //}

            //String query = "INSERT INTO Blocked_NPC (ID) VALUES (@0);";

            //if (db.Query(query, id) != 1)
            //{
            //    //Log.ConsoleError("Inserting into the database has failed!");
            //    TShock.Log.ConsoleError("Inserting into the database has failed!");
            //    args.Player.SendMessage(String.Format("Inserting into the database has failed!", id), Color.Red);
            //}
            //else
            //{
            //    args.Player.SendMessage(String.Format("Successfully banned {0}", id), Color.Red);
            //    blockedNPC.Add(id);
            //}

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! [c/ffff00:Proper syntax: {0}bannpc <name or ID> <amount (default 0)>]", TShock.Config.Settings.CommandSpecifier);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendErrorMessage("Invalid NPC type!");
                return;
            }

            // Change it from NPC ID only to NPC ID or name
            // To-do: add a way to show list of currently banned NPCs and optional NPC amount to be blocked
            // Example: currently blocked NPCs list, blocks more than 5 wraiths from spawning in the world
            var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);

            if (npcs.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid NPC type!");
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
            }
            else
            {
                var npc = npcs[0];
                int id = npc.type;

                //String query = "INSERT INTO Blocked_NPC (ID) VALUES (@0);";
                string query = "INSERT INTO Blocked_NPC (ID) VALUES (@0);";

                //if (db.Query(query, id) != 1) // This check was not working properly if you ban an NPC that's already banned
                //{
                //    //Log.ConsoleError("Inserting into the database has failed!");
                //    TShock.Log.Error("Inserting into the database has failed!");
                //    //args.Player.SendMessage(String.Format("Inserting into the database has failed!", id), Color.Red);
                //    args.Player.SendErrorMessage("Failed to insert into the database! [c/ffff00:{0} (ID: {1}) is already banned!]", npc.FullName, npc.type);
                //}
                //else
                //{
                //    //args.Player.SendMessage(String.Format("Successfully banned {0}", id), Color.Red);
                //    args.Player.SendSuccessMessage("Successfully banned {0} (ID: {1}).", npc.FullName, npc.type);
                //    blockedNPC.Add(id);
                //}

                if ((db.Query(query, id) != 0) && blockedNPC.ContainsKey(id) && args.Parameters.Count == 1)
                {
                    args.Player.SendErrorMessage("{0} (ID: {1}) is already banned.", npc.FullName, npc.type);
                }
                else
                {
                    int amountToSpawn = 0;
                    if (args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out amountToSpawn))
                    {
                        if (amountToSpawn < 0)
                        {
                            args.Player.SendErrorMessage("The amount of NPC's to ban must be greater than or equal to 0");
                            return;
                        }
                        db.Query("UPDATE Blocked_NPC SET AmounttoSpawn=@0 WHERE ID=@1", amountToSpawn, id);
                        blockedNPC.Add(id, amountToSpawn);
                    }
                    else
                    {
                        db.Query("UPDATE Blocked_NPC SET AmounttoSpawn=@0 WHERE ID=@1", 0, id);
                        blockedNPC.Add(id, 0);
                    }
                    if (args.Silent || ConfigFile.BlockBanMessageToOthers)
                    {
                        args.Player.SendSuccessMessage(ConfigFile.NPCAddedMessageSilent, npc.FullName, npc.type);
                    } 
                    else
                    {
                        TSPlayer.All.SendInfoMessage(ConfigFile.NPCAddedMessage, args.Player.Name, npc.FullName, amountToSpawn);
                    }
                }
            }
        }

        private void DelNpc(CommandArgs args)
        {
            //if (args.Parameters.Count < 1)
            //{
            //    args.Player.SendMessage("You must specify a npc id to remove.", Color.Red);
            //    return;
            //}
            //string tile = args.Parameters[0];
            //int id;
            //if (!int.TryParse(tile, out id))
            //{
            //    args.Player.SendMessage(String.Format("Npc id '{0}' is not a valid number.", id), Color.Red);
            //    return;
            //}
            //String query = "DELETE FROM Blocked_NPC WHERE ID = @0;";

            //if (db.Query(query, id) != 1)
            //{
            //    //Log.ConsoleError("Removing from the database has failed!");
            //    TShock.Log.ConsoleError("Removing from the database has failed!");
            //    args.Player.SendMessage(String.Format("Removing from the database has failed!  Are you sure {0} is banned?", id), Color.Red);
            //}
            //else
            //{
            //    args.Player.SendMessage(String.Format("Successfully unbanned {0}", id), Color.Green);
            //    blockedNPC.Remove(id);
            //}

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! [c/ffff00:Proper syntax: {0}unbannpc <name or ID>]", TShock.Config.Settings.CommandSpecifier);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendErrorMessage("Invalid NPC type!");
                return;
            }

            var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);

            if (npcs.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid NPC type!");
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
            }
            else
            {
                var npc = npcs[0];
                int id = npc.type;

                //String query = "DELETE FROM Blocked_NPC WHERE ID = @0;";
                string query = "DELETE FROM Blocked_NPC WHERE ID = @0;";

                //if (db.Query(query, id) != 1)
                //{
                //    //Log.ConsoleError("Removing from the database has failed!");
                //    TShock.Log.Error("Removing from the database has failed!");
                //    //args.Player.SendMessage(String.Format("Removing from the database has failed!  Are you sure {0} is banned?", id), Color.Red);
                //    args.Player.SendErrorMessage("Failed to remove from the database! [c/ffff00:Are you sure {0} (ID: {1}) is banned?]", npc.FullName, npc.type);
                //}
                //else
                //{
                //    //args.Player.SendMessage(String.Format("Successfully unbanned {0}", id), Color.Green);
                //    args.Player.SendSuccessMessage("Successfully unbanned {0} (ID: {1})", npc.FullName, npc.type);
                //    blockedNPC.Remove(id);
                //}

                if (db.Query(query, id) == 0) // Works perfectly
                {
                    args.Player.SendErrorMessage("{0} (ID: {1}) is not banned.", npc.FullName, npc.type);
                }
                else
                {
                    blockedNPC.Remove(id);
                    if (args.Silent)
                    {
                        args.Player.SendSuccessMessage(ConfigFile.NPCRemovedMessageSilent, npc.FullName, npc.type);
                    }
                    else
                    {
                        TSPlayer.All.SendInfoMessage(ConfigFile.NPCRemovedMessage, args.Player.Name, npc.FullName);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnSpawn);
            }

            base.Dispose(disposing);
        }

        private void OnTransform(NpcTransformationEventArgs args)
        {
            if (args.Handled)
                return;
            int npcNetID = Main.npc[args.NpcId].netID;
            if (blockedNPC.ContainsKey(npcNetID))
            {
                int foundNPCCount;
                if (blockedNPC.TryGetValue(npcNetID, out foundNPCCount))
                {
                    if (CheckActiveNPCCount(npcNetID) > foundNPCCount)
                    {
                        Main.npc[args.NpcId].active = false;
                    }
                    return;
                }
                Main.npc[args.NpcId].active = false;
            }
        }

        private void OnSpawn(NpcSpawnEventArgs args)
        {
            if (args.Handled)
                return;
            int npcNetID = Main.npc[args.NpcId].netID;
            if (blockedNPC.ContainsKey(npcNetID))
            {
                int foundNPCCount;
                if (blockedNPC.TryGetValue(npcNetID, out foundNPCCount))
                {
                    if (CheckActiveNPCCount(npcNetID) > foundNPCCount)
                    {
                        args.Handled = true;
                        Main.npc[args.NpcId].active = false;
                        args.NpcId = Main.maxNPCs;
                    }
                    return;
                }
                args.Handled = true;
                Main.npc[args.NpcId].active = false;
                args.NpcId = Main.maxNPCs;
            }
        }

        public static int CheckActiveNPCCount(NPC npc)
        {
            return Main.npc.Where(Npc => Npc != null && Npc.active && Npc.type == npc.type).Count();
        }
        public static int CheckActiveNPCCount(int type)
        {
            return Main.npc.Where(Npc => Npc != null && Npc.active && Npc.type == type).Count();
        }
    }
}
