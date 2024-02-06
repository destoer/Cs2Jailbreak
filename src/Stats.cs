using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Admin;
using MySqlConnector;

public class JailStats
{
    public JailStats()
    {
        for(int i = 0; i < 64; i++)
        {
            playerStats[i] = new PlayerStat();
        }
    }

    public void Win(CCSPlayerController? player, LastRequest.LRType type)
    {
        var lrPlayer = PlayerStatFromPlayer(player);

        if(lrPlayer != null && type != LastRequest.LRType.NONE && player.is_valid())
        {
            int idx = (int)type;
            lrPlayer.win[idx] += 1;
            IncDB(player,type,true);
            Chat.announce(LastRequest.LR_PREFIX,$"{player.PlayerName} won {LastRequest.LR_NAME[idx]} win {lrPlayer.win[idx]} : loss {lrPlayer.loss[idx]}");
        }
    }

    public void Loss(CCSPlayerController? player, LastRequest.LRType type)
    {
        var lrPlayer = PlayerStatFromPlayer(player);

        if(lrPlayer != null && type != LastRequest.LRType.NONE && player.is_valid())
        {
            int idx = (int)type;
            lrPlayer.loss[idx] += 1;
            IncDB(player,type,false);

            Chat.announce(LastRequest.LR_PREFIX,$"{player.PlayerName} lost {LastRequest.LR_NAME[idx]} win {lrPlayer.win[idx]} : loss {lrPlayer.loss[idx]}");
        }        
    }

    PlayerStat? PlayerStatFromPlayer(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return null;
        }

        return playerStats[player.Slot];        
    }



    void PrintStats(CCSPlayerController? invoke, CCSPlayerController? player)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        var lrPlayer = PlayerStatFromPlayer(player);

        if(lrPlayer != null && player.is_valid())
        {
            invoke.PrintToChat($"{LastRequest.LR_PREFIX} lr stats for {player.PlayerName}");

            for(int i = 0; i < LastRequest.LR_SIZE; i++)
            {
                invoke.PrintToChat($"{LastRequest.LR_PREFIX} {LastRequest.LR_NAME[i]} win {lrPlayer.win[i]} : loss {lrPlayer.loss[i]}");
            }
        }
    }

    public void LRStatsCmd(CCSPlayerController? player, CommandInfo command)
    {
        // just do own player for now
        PrintStats(player,player);
    }

    public void PurgePlayer(CCSPlayerController? player)
    {
        var lrPlayer = PlayerStatFromPlayer(player);

        if(lrPlayer != null)
        {
            for(int i = 0; i < LastRequest.LR_SIZE; i++)
            {
                lrPlayer.win[i] = 0;
                lrPlayer.loss[i] = 0;
            }

            lrPlayer.cached = false;
        }
    }

    class PlayerStat
    {
        public int[] win = new int[LastRequest.LR_SIZE];
        public int[] loss = new int[LastRequest.LR_SIZE]; 
        public bool cached = false;
    }

    async void InsertPlayer(String steamID, String playerName)
    {
        var database = await ConnectDB();

        if(database == null)
        {
            return;
        }

        // insert new player
        using var insertPlayer = new MySqlCommand("INSERT IGNORE INTO stats (steamid,name) VALUES (@steam_id, @name)",database);
        insertPlayer.Parameters.AddWithValue("@steam_id",steamID);
        insertPlayer.Parameters.AddWithValue("@name",playerName);

        try 
        {
            await insertPlayer.ExecuteNonQueryAsync();
        } 
        
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public void IncDB(CCSPlayerController? player,LastRequest.LRType type, bool win)
    {
        if(!player.is_valid() || type == LastRequest.LRType.NONE  || player.IsBot)
        {
            return;
        }

        String steamID = new SteamID(player.SteamID).SteamId2;

        // make sure this doesn't block the main thread
        Task.Run(async () =>
        {
            await IncDBAsync(steamID,type,win);
        });
    }

    public async Task IncDBAsync(String steamID,LastRequest.LRType type, bool win)
    {
        var database = await ConnectDB();

        if(database == null)
        {
            return;
        }

        String name = LastRequest.LR_NAME[(int)type].Replace(" ","_");

        if(win)
        {
            name += "_win";
        }

        else
        {
            name += "_loss";
        }

        using var incStat = new MySqlCommand($"UPDATE stats SET {name} = {name} + 1 WHERE steamid = @steam_id",database);
        incStat.Parameters.AddWithValue("@steam_id",steamID);

        try 
        {
            Console.WriteLine($"increment {steamID} : {name} : {win}");
            await incStat.ExecuteNonQueryAsync();
        } 
        
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }


    void read_stats(ulong id, String steamID, String playerName)
    {
         // repull player from steamid if they are still around
        CCSPlayerController? player = Utilities.GetPlayerFromSteamId(id);

        if(!player.is_valid())
        {
            return;
        }

        int slot = player.Slot;

        // allready cached we dont care
        if(playerStats[slot].cached)
        {
            return;
        }

        // make sure this doesn't block the main thread
        Task.Run(async () =>
        {
            await ReadStatsAsync(steamID,playerName,slot);
        });     
    }

    async Task ReadStatsAsync(String steamID, String playerName, int slot)
    {
        var database = await ConnectDB();

        if(database == null)
        {
            return;
        }

        // query steamid
        using var querySteamID = new MySqlCommand("SELECT * FROM stats WHERE steamid = @steam_id",database);
        querySteamID.Parameters.AddWithValue("@steam_id",steamID);

        try
        {
            var reader = await querySteamID.ExecuteReaderAsync();
            
            if(reader.Read())
            {
                //Console.WriteLine($"reading out lr stats {player.PlayerName}");

                for(int i = 0; i < LastRequest.LR_SIZE; i++)
                {
                    String name = LastRequest.LR_NAME[i].Replace(" ","_");

                    playerStats[slot].win[i] = (int)reader[name + "_win"];
                    playerStats[slot].loss[i] = (int)reader[name + "_loss"];
                }

                playerStats[slot].cached = true;
            }

            // failed to pull player stats
            // insert a new entry
            else
            {
                //Console.WriteLine("insert new entry");

                InsertPlayer(steamID,playerName);
            }

            reader.Close();
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public void LoadPlayer(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        // attempt to cache player stats
        String name = player.PlayerName;
        String steamID = new SteamID(player.SteamID).SteamId2;

        read_stats(player.SteamID,steamID,name);
    }


    public void SetupDB(MySqlConnection? database)
    {
        if(database == null)
        {
            Console.WriteLine("Could not open jb database");
            return;
        }

        // Make sure Table exists
        using var tableCmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS stats (steamid varchar(64) PRIMARY KEY,name varchar(64))",database);
        tableCmd.ExecuteNonQuery();

        // Check table size to see if we have the right number of LR's
        // if we dont make the extra tables
        using var colCmd = new MySqlCommand("SHOW COLUMNS FROM stats",database);
        var colReader = colCmd.ExecuteReader();

        int rowCount = 0;
        while(colReader.Read())
        {
            rowCount++;
        }
        colReader.Close();

        int fields = (LastRequest.LR_SIZE * 2) + 2;

        // NOTE: both win and lose i.e * 2 + steamid and name
        if(rowCount != fields)
        {
            // add lr fields
            for(int i = 0; i < LastRequest.LR_SIZE; i++)
            {
                String name = LastRequest.LR_NAME[i].Replace(" ","_");

                try
                {
                    // NOTE: could use NOT Exists put old sql versions dont play nice
                    // ideally we would use an escaped statement but these strings aernt user controlled anyways
                    using var insertTableWin = new MySqlCommand($"ALTER TABLE stats ADD COLUMN {name + "_win"} int DEFAULT 0",database);
                    insertTableWin.ExecuteNonQuery();

                    using var insertTableLoss = new MySqlCommand($"ALTER TABLE stats ADD COLUMN {name + "_loss"} int DEFAULT 0",database);
                    insertTableLoss.ExecuteNonQuery();
                }

                catch {}
            }

            // add warden fields

        }

        Console.WriteLine("Setup jb stats");

    }

    public async Task<MySqlConnection?> ConnectDB()
    {
        // No credentials don't even try a connection
        if(Config.username == "")
        {
            return null;
        }

        try
        {
            MySqlConnection? database = new MySqlConnection(
                $"Server={Config.server};User ID={Config.username};Password={Config.password};Database={Config.database};Port={Config.port}");

            await database.OpenAsync();

            return database;
        }

        catch
        {
            //Console.WriteLine(ex.ToString());
            return null;
        }
    }

    public JailConfig Config = new JailConfig();

    PlayerStat[] playerStats = new PlayerStat[64];
}