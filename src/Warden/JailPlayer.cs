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
using System.Drawing;
using Microsoft.Data.Sqlite;
using McMaster.NETCore.Plugins;


public class JailPlayer
{
    public static void setup_db()
    {
        try
        {
            using (var connection = new SqliteConnection("Data Source=destoer_config.sqlite"))
            {
                connection.Open();

                // create the db
                var create = connection.CreateCommand();

                create.CommandText = "CREATE TABLE IF NOT EXISTS config (steam_id varchar(64) NOT NULL PRIMARY KEY)";

                create.ExecuteNonQuery();

                String[] col_cmd =
                {
                    "ALTER TABLE config ADD COLUMN laser_colour varchar(64) DEFAULT 'Cyan'",
                    "ALTER TABLE config ADD COLUMN marker_colour varchar(64) DEFAULT 'Cyan'",
                    "ALTER TABLE config ADD COLUMN ct_gun varchar(64) DEFAULT 'M4'",
                };


                // start populating our fields
                foreach (var cmd in col_cmd)
                {
                    var col = connection.CreateCommand();
                    col.CommandText = cmd;

                    // this may fail on duplicate table entry
                    // we don't really care it does
                    try
                    {
                        col.ExecuteNonQuery();
                    }

                    catch { }
                }
            }
        }

        // Actually print these, creation should not fail
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    async Task update_player_db(String steam_id, String name, String value)
    {
        try
        {
            using (var connection = new SqliteConnection("Data Source=destoer_config.sqlite"))
            {
                await connection.OpenAsync();

                // modify one of the setting fields
                using var update = connection.CreateCommand();
                update.CommandText = $"UPDATE config SET {name} = '{value}' WHERE steam_id = @steam_id";
                update.Parameters.AddWithValue("@steam_id", steam_id);

                await update.ExecuteNonQueryAsync();
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    async Task insert_player_db(String steam_id)
    {
        using (var connection = new SqliteConnection("Data Source=destoer_config.sqlite"))
        {
            try
            {
                await connection.OpenAsync();

                // add a new steam id
                using var insert_player = connection.CreateCommand();
                insert_player.CommandText = "INSERT OR IGNORE INTO config (steam_id) VALUES (@steam_id)";
                insert_player.Parameters.AddWithValue("@steam_id", steam_id);

                await insert_player.ExecuteNonQueryAsync();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    async Task LoadPlayerDB(String steam_id)
    {
        using (var connection = new SqliteConnection("Data Source=destoer_config.sqlite"))
        {
            try
            {
                await connection.OpenAsync();


                using var query_steam_id = connection.CreateCommand();

                // query steamid
                query_steam_id.CommandText = "SELECT * FROM config WHERE steam_id = @steam_id";
                query_steam_id.Parameters.AddWithValue("@steam_id", steam_id);

                using var reader = await query_steam_id.ExecuteReaderAsync();

                if (reader.Read())
                {
                    // just override this
                    laserColour = Lib.COLOUR_CONFIG_MAP[(String)reader["laser_colour"]];
                    markerColour = Lib.COLOUR_CONFIG_MAP[(String)reader["marker_colour"]];
                    ctGun = (String)reader["ct_gun"];

                    // don't try reloading the player
                    cached = true;
                }


                // steam id does not exist
                // insert a new steam id
                else
                {
                    await insert_player_db(steam_id);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public void LoadPlayer(CCSPlayerController? player)
    {
        if (!player.is_valid())
        {
            return;
        }

        // The database has allready been read before
        // there is not need to do it again
        if (cached)
        {
            return;
        }

        String steam_id = new SteamID(player.SteamID).SteamId2;

        // make sure this doesn't block the main thread
        Task.Run(async () =>
        {
            await LoadPlayerDB(steam_id);
        });
    }

    public void update_player(CCSPlayerController? player, String name, String value)
    {
        if (!player.is_valid())
        {
            return;
        }

        String steam_id = new SteamID(player.SteamID).SteamId2;

        // make sure this doesn't block the main thread
        Task.Run(async () =>
        {
            await update_player_db(steam_id, name, value);
        });
    }

    public void SetLaser(CCSPlayerController? player, String value)
    {
        if (!player.is_valid())
        {
            return;
        }

        player.announce(Warden.WARDEN_PREFIX, $"Laser colour set to {value}");
        laserColour = Lib.COLOUR_CONFIG_MAP[value];

        // save back to the db too
        update_player(player, "laser_colour", value);
    }

    public void SetMarker(CCSPlayerController? player, String value)
    {
        if (!player.is_valid())
        {
            return;
        }

        player.announce(Warden.WARDEN_PREFIX, $"Marker colour set to {value}");
        markerColour = Lib.COLOUR_CONFIG_MAP[value];

        // save back to the db too
        update_player(player, "marker_colour", value);
    }

    public void PurgeRound()
    {
        IsRebel = false;
    }

    public void Reset()
    {
        PurgeRound();

        // TODO: reset client specific settings
        laserColour = Lib.CYAN;
        markerColour = Lib.CYAN;
        ctGun = "M4";
    }

    public void SetRebel(CCSPlayerController? player)
    {
        // allready a rebel don't care
        if (IsRebel)
        {
            return;
        }

        if (JailPlugin.event_active())
        {
            return;
        }

        // ignore if they are in lr
        if (JailPlugin.lr.in_lr(player))
        {
            return;
        }

        // dont care if player is invalid
        if (!player.is_valid())
        {
            return;
        }

        // on T with no warday or sd active
        if (player.IsT())
        {
            if (Config.colourRebel)
            {
                Chat.announce(REBEL_PREFIX, $"{player.PlayerName} is a rebel");
                player.SetColour(Lib.RED);
            }
            IsRebel = true;
        }
    }

    public void GivePardon(CCSPlayerController? player)
    {
        if(player.is_valid_alive() && player.IsT())
        {
            Chat.localize_announce(Warden.WARDEN_PREFIX, "warden.give_pardon",player.PlayerName);
            player.SetColour(Color.FromArgb(255, 255, 255, 255));

            // they are no longer a rebel
            IsRebel = false;
        }      
    }

    public void GiveFreeday(CCSPlayerController? player)
    {
        if(player.is_valid_alive() && player.IsT())
        {
            Chat.localize_announce(Warden.WARDEN_PREFIX, "warden.give_freeday",player.PlayerName);
            player.SetColour(Lib.GREEN);

            // they are no longer a rebel
            IsRebel = false;
        }
    }  

    public void RebelDeath(CCSPlayerController? player, CCSPlayerController? killer)
    {
        // event active dont care
        if (JailPlugin.event_active())
        {
            return;
        }

        // players aernt valid dont care
        if (killer == null || !player.is_valid() || !killer.is_valid())
        {
            return;
        }

        // print death if player is rebel and killer on CT
        if (IsRebel && killer.IsCt())
        {
            Chat.localize_announce(REBEL_PREFIX, "rebel.kill", killer.PlayerName, player.PlayerName);
        }
    }

    public void RebelWeaponFire(CCSPlayerController? player, String weapon)
    {
        if (Config.rebelRequireHit)
        {
            return;
        }

        // ignore weapons players are meant to have
        if (!weapon.Contains("knife") && !weapon.Contains("c4"))
        {
            SetRebel(player);
        }
    }

    public void PlayerHurt(CCSPlayerController? player, CCSPlayerController? attacker, int health, int damage)
    {
        if (!player.is_valid())
        {
            return;
        }
        
        bool isWorld = attacker == null || !attacker.is_valid();

        string localKey = health > 0 ? "logs.format.damage" : "logs.format.kill";
        if (isWorld)
        {
            JailPlugin.logs.AddLocalized(player, localKey + "_self", damage);
        }
        else
        {
            JailPlugin.logs.AddLocalized(player, attacker!, localKey, damage);
        }

        if (isWorld)
        {
            return;
        }

        // ct hit by T they are a rebel
        if (player.IsCt() && attacker.IsT())
        {
            SetRebel(attacker);
        }

        // log any ct damage
        else if (attacker.IsCt())
        {
            //Lib.print_console_all($"CT {attacker.PlayerName} hit {player.PlayerName} for {damage}");
        }
    }


    public static readonly String REBEL_PREFIX = $" {ChatColors.Green}[REBEL]: {ChatColors.White}";

    public static JailConfig Config = new JailConfig();

    public Color laserColour { get; private set; } = Lib.CYAN;
    public Color markerColour { get; private set; } = Lib.CYAN;
    bool cached = false;

    public String ctGun = "M4";

    public bool IsRebel = false;
};
