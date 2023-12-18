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
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;

public class SDSpectre : SDBase
{
    public override void setup()
    {
        localise_announce("sd.spectre_start");
        localise_announce("sd.damage_enable",delay);
    }

    public override void make_boss(CCSPlayerController? spectre, int count)
    {
        if(spectre != null && spectre.is_valid_alive())
        {
            localise_announce($"sd.spectre",spectre.PlayerName);

            // give the spectre the HP and swap him
            spectre.set_health(count * 60);
            spectre.SwitchTeam(CsTeam.CounterTerrorist);
            
            setup_player(spectre);
        }

        else
        {
            Lib.announce("[ERROR] ","Error picking spectre");
        }
    }

    public override bool weapon_equip(CCSPlayerController player,String name) 
    {
        // spectre can only carry a knife
        if(is_boss(player))
        {
            return name.Contains("knife") || name.Contains("decoy");
        }

        return true;
    }

    public override void start()
    {
        localise_announce("sd.fight");
        Lib.swap_all_t();

        (boss, int count) = pick_boss();
        make_boss(boss,count);
    }

    public override void end()
    {
        localise_announce("sd.spectre_end");
    }

    public override void setup_player(CCSPlayerController player)
    {
        if(is_boss(player))
        {
            // invis and speed
            player.set_colour(Color.FromArgb(0,0,0,0));
            player.set_velocity(2.5f);

            player.strip_weapons();

            // Work around for colour updates
            player.GiveNamedItem("weapon_decoy");
        }

        else
        {
            player.event_gun_menu();
        }
    }
}