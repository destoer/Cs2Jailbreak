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

public class LRKnife : LRBase
{
    public LRKnife(LastRequest manager,LastRequest.LRType type,int lr_slot, int player_slot, String choice) : base(manager,type,lr_slot,player_slot,choice)
    {

    }

    public override void init_player(CCSPlayerController player)
    {    
        // give player a knife and restrict them to it
        player.GiveNamedItem("weapon_knife");
        weapon_restrict = "knife";

        // Handle options
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

            case "High speed":
            {
                player.set_velocity(2.5f);
                break;
            }
                
            case "One hit":
            {
                player.set_health(50);
                break;
            }
        }
    }
}