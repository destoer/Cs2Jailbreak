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

    async Task load_player_db(String steam_id)
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
                    laser_colour = Lib.LASER_CONFIG_MAP[(String)reader["laser_colour"]];
                    marker_colour = Lib.LASER_CONFIG_MAP[(String)reader["marker_colour"]];
                    ct_gun = (String)reader["ct_gun"];

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

    public void load_player(CCSPlayerController? player)
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
            await load_player_db(steam_id);
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

    public void set_laser(CCSPlayerController? player, String value)
    {
        if (!player.is_valid())
        {
            return;
        }

        player.announce(Warden.WARDEN_PREFIX, $"Laser colour set to {value}");
        laser_colour = Lib.LASER_CONFIG_MAP[value];

        // save back to the db too
        update_player(player, "laser_colour", value);
    }

    public void set_marker(CCSPlayerController? player, String value)
    {
        if (!player.is_valid())
        {
            return;
        }

        player.announce(Warden.WARDEN_PREFIX, $"Marker colour set to {value}");
        marker_colour = Lib.LASER_CONFIG_MAP[value];

        // save back to the db too
        update_player(player, "marker_colour", value);
    }

    public void purge_round()
    {
        is_rebel = false;
    }

    public void reset()
    {
        purge_round();

        // TODO: reset client specific settings
        laser_colour = Lib.CYAN;
        marker_colour = Lib.CYAN;
        ct_gun = "M4";
    }

    public void set_rebel(CCSPlayerController? player)
    {
        // allready a rebel don't care
        if (is_rebel)
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
        if (player.is_t())
        {
            if (config.colour_rebel)
            {
                Chat.announce(REBEL_PREFIX, $"{player.PlayerName} is a rebel");
                player.set_colour(Lib.RED);
            }
            is_rebel = true;
        }
    }

    public void give_pardon(CCSPlayerController? player)
    {
        if(player.is_valid_alive() && player.is_t())
        {
            Chat.localize_announce(Warden.WARDEN_PREFIX, "warden.give_pardon",player.PlayerName);
            player.set_colour(Color.FromArgb(255, 255, 255, 255));

            // they are no longer a rebel
            is_rebel = false;
        }      
    }

    public void give_freeday(CCSPlayerController? player)
    {
        if(player.is_valid_alive() && player.is_t())
        {
            Chat.localize_announce(Warden.WARDEN_PREFIX, "warden.give_freeday",player.PlayerName);
            player.set_colour(Lib.GREEN);

            // they are no longer a rebel
            is_rebel = false;
        }
    }  

    public void rebel_death(CCSPlayerController? player, CCSPlayerController? killer)
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
        if (is_rebel && killer.is_ct())
        {
            Chat.localize_announce(REBEL_PREFIX, "rebel.kill", killer.PlayerName, player.PlayerName);
        }
    }

    public void rebel_weapon_fire(CCSPlayerController? player, String weapon)
    {
        if (config.rebel_requirehit)
        {
            return;
        }

        // ignore weapons players are meant to have
        if (!weapon.Contains("knife") && !weapon.Contains("c4"))
        {
            set_rebel(player);
        }
    }

    public void player_hurt(CCSPlayerController? player, CCSPlayerController? attacker, int health, int damage)
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
        if (player.is_ct() && attacker.is_t())
        {
            set_rebel(attacker);
        }

        // log any ct damage
        else if (attacker.is_ct())
        {
            //Lib.print_console_all($"CT {attacker.PlayerName} hit {player.PlayerName} for {damage}");
        }
    }


    public static readonly String REBEL_PREFIX = $" {ChatColors.Green}[REBEL]: {ChatColors.White}";

    public static JailConfig config = new JailConfig();

    public Color laser_colour { get; private set; } = Lib.CYAN;
    public Color marker_colour { get; private set; } = Lib.CYAN;
    bool cached = false;

    public String ct_gun = "M4";

    public bool is_rebel = false;
};
