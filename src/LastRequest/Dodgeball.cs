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


public class LRDodgeball : LRBase
{
    public LRDodgeball(LastRequest manager,LastRequest.LRType type,int lr_slot, int player_slot, String choice) : base(manager,type,lr_slot,player_slot,choice)
    {

    }

    public override void init_player(CCSPlayerController player)
    {    
        weapon_restrict = "flashbang";

        if(player.is_valid_alive())
        {
            player.set_health(1);

            player.GiveNamedItem("weapon_flashbang");

            switch(choice)
            {
                case "Vanilla": 
                {
                    break;
                }
                
                case "Low gravity":
                {
                    player.set_gravity(0.6f);
                    break;
                }
            }
        }
    }
    
    public override void player_hurt(int damage, int health, int hitgroup)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);
    
        if(player.is_valid_alive())
        {
            player.slay();
        }
    }

    public override void grenade_thrown()
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);
        give_lr_nade_delay(1.4f,"weapon_flashbang");
    }

    public override void ent_created(CEntityInstance entity)
    {
        entity.remove_delay(1.4f,"flashbang_projectile");
    }
}