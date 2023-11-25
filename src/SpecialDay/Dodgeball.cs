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


public class SDDodgeball : SDBase
{
    public override void setup()
    {
        announce("Dodgeball started");
        announce("Please 15 seconds for damage be enabled");
    }

    public override void start()
    {
        announce("Fight!");
    }

    public override void end()
    {
        announce("Dodgeball is over");
    }

    public override void setup_player(CCSPlayerController player)
    {
        player.strip_weapons(true);
        player.GiveNamedItem("weapon_flashbang");
        weapon_restrict = "flashbang";
    }

    public override void grenade_thrown(CCSPlayerController? player)
    {
        Lib.give_weapon_delay(player,1.4f,"weapon_flashbang");
    }

    public override void player_hurt(CCSPlayerController? player,int damage, int health, int hitgroup)
    {
        if(player != null && player.is_valid_alive())
        {
            player.PlayerPawn.Value.CommitSuicide(true, true);
        }
    }

    public override void ent_created(CEntityInstance entity)
    {
        Lib.remove_ent_delay(entity,1.4f,"flashbang_projectile");
    }

}