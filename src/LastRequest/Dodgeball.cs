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
    public LRDodgeball(LastRequest manager,int lr_slot, int player_slot, String choice) : base(manager,"Dodgeball",lr_slot,player_slot,choice)
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
    
        if(player != null && player.is_valid_alive())
        {
            player.PlayerPawn.Value.CommitSuicide(true, true);
        }
    }

    public override void grenade_thrown()
    {
        if(JailPlugin.global_ctx == null)
        {
            return;
        }

        JailPlugin.global_ctx.AddTimer(1.4f,() => 
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);
        
            if(player != null && player.is_valid_alive())
            {
                player.GiveNamedItem("weapon_flashbang");
            }
        });
    }

    public override void ent_created(CEntityInstance entity)
    {
        if(JailPlugin.global_ctx == null)
        {
            return;
        }

        // remove projectile
        if(entity.DesignerName == "flashbang_projectile")
        {
            if(JailPlugin.global_ctx != null)
            {
                JailPlugin.global_ctx.AddTimer(1.4f,() => 
                {
                    if(entity.IsValid)
                    {
                        entity.Remove();
                    }
                });
            }
        }
    }
}