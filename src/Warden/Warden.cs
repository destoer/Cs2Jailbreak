

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

public class Warden
{
    public static readonly String WARDEN_PREFIX = "[WARDEN]: ";

    public Warden()
    {
        for(int p = 0; p < jail_players.Length; p++)
        {
            jail_players[p] = new JailPlayer();
        }
    }

    // Give a player warden
    public void set_warden(int? new_slot_opt)
    {
        if(new_slot_opt == null)
        {
            return;
        }

        warden_slot = new_slot_opt.Value;

        var player = Utilities.GetPlayerFromSlot(warden_slot);

        Lib.announce(WARDEN_PREFIX,$"{player.PlayerName} is now the warden");

        // change player color!
        //player.PlayerPawn.Value.Render = 0.0;
    }

    bool is_warden(CCSPlayerController? player)
    {
        return player.slot() == warden_slot;
    }

    public void remove_warden()
    {
        var player = Utilities.GetPlayerFromSlot(warden_slot);

        if(player.is_valid())
        {
            Lib.announce(WARDEN_PREFIX,$"{player.PlayerName} is no longer the warden");
        }

        warden_slot = INAVLID_SLOT;
    }

    public void remove_if_warden(CCSPlayerController? player)
    {
        if(!player.is_valid() || player == null)
        {
            return;
        }

        if(is_warden(player))
        {
            remove_warden();
        }
    }

    public void leave_warden_cmd(CCSPlayerController? player, CommandInfo command)
    {
        remove_if_warden(player);
    }


    public void warday_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a warden to call a warday");
            return;
        }

        // must specify location
        if(command.ArgCount != 2)
        {
            player.PrintToChat($"{WARDEN_PREFIX}Usage !wd <location>");
            return;
        }

        // attempt the start the warday
        String location = command.ArgByIndex(1);

        if(!warday.start_warday(location))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You cannot call a warday for another {Warday.ROUND_LIMIT - warday.round_counter} rounds");
        }
    }

      
    public void wub_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a warden to use wub");
            return;
        }

        block.unblock_all();
    }

    public void wb_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a warden to use wb");
            return;
        }

        block.block_all();
    }

    // debug command
    public void is_rebel_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke == null || !invoke.is_valid())
        {
            return;
        }

        invoke.PrintToConsole("rebels\n");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            int? slot = player.slot();

            if(slot != null)
            {
                invoke.PrintToConsole($"{jail_players[slot.Value].is_rebel} : {player.PlayerName}\n");
            }
        }
    }

    public void take_warden_cmd(CCSPlayerController? player, CommandInfo command)
    {
        // invalid player we dont care
        if(!player.is_valid() || player == null)
        {
            return;
        }

        // player must be alive
        if(!player.PawnIsAlive)
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be alive to warden");
        }        

        // check team is valid
        else if(!player.is_ct())
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a CT to warden");
        }

        // check there is no warden
        else if(warden_slot != INAVLID_SLOT)
        {
            var warden = Utilities.GetPlayerFromSlot(warden_slot);

            player.PrintToChat($"{WARDEN_PREFIX}{warden.PlayerName} is allready a warden");
        }

        // player is valid to take warden
        else
        {
            set_warden(player.slot());
        }
    }

    // reset variables for a new round
    void purge_round()
    {
        warden_slot = INAVLID_SLOT;

        // reset player structs
        foreach(JailPlayer jail_player in jail_players)
        {
            jail_player.purge_round();
        }
    }

    public void map_start()
    {
        warday.map_start();
    }

    public void round_start()
    {
        purge_round();

        // handle submodules
        mute.round_start();
        block.round_start();
        warday.round_start();

        // if there is only one ct automatically give them warden!

        // setup each player
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid_alive())
            {
                continue;
            }

            // handle guns and block
            player.strip_weapons();

            // all players have knifes
            if(player.is_t())
            {
                player.GiveNamedItem("weapon_knife");
            }

            // give ct kevlar deagle m4
            else if(player.is_ct())
            {
                player.GiveNamedItem("item_assaultsuit");
                player.GiveNamedItem("weapon_knife");
                player.GiveNamedItem("weapon_deagle");
                player.GiveNamedItem("weapon_m4a1");
            }
        }
    }

    public void round_end()
    {
        mute.round_end();

        purge_round();
    }


    public void connect(CCSPlayerController? player)
    {
        var slot = player.slot();

        if(slot != null)
        {
            jail_players[slot.Value].reset();
        }

        mute.connect(player);
    }

    public void disconnect(CCSPlayerController? player)
    {
        remove_if_warden(player);
    }

    public void spawn(CCSPlayerController? player)
    {
        mute.spawn(player);
    }   


    public void switch_team(CCSPlayerController? player,int new_team)
    {
        remove_if_warden(player);
        mute.switch_team(player,new_team);
    }

    // warden death has occured
    public void warden_death()
    {
        remove_warden();
    }

    public void death(CCSPlayerController? player, CCSPlayerController? killer)
    {
        // player is no longer on server
        if(!player.is_valid() || player == null)
        {
            return;
        }

        // handle warden death
        remove_if_warden(player);

        // mute player
        mute.death(player);

        var jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            jail_player.rebel_death(player,killer);
        }
    }


    public void weapon_fire(EventWeaponFire @event, GameEventInfo info)
    {
        // attempt to get player and weapon
        var player = @event.Userid;
        String weapon = @event.Weapon;

        // attempt to set rebel
        var jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            jail_player.rebel_weapon_fire(player,weapon);
        }
    }

    // util func to get a jail player
    JailPlayer? jail_player_from_player(CCSPlayerController? player)
    {
        if(!player.is_valid() || player == null)
        {
            return null;
        }

        var slot = player.slot();

        if(slot == null)
        {
            return null;
        }

        return jail_players[slot.Value];
    }

    const int INAVLID_SLOT = -3;   

    int warden_slot = INAVLID_SLOT;

    JailPlayer[] jail_players = new JailPlayer[64];

    Warday warday = new Warday();
    Block block = new Block();
    Mute mute = new Mute();
};