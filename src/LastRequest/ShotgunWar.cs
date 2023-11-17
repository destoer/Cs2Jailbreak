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

public class LRShotgunWar : LRBase
{
    public LRShotgunWar(LastRequest manager,int lr_slot, int player_slot, String choice) : base(manager,"Shotgun war",lr_slot,player_slot,choice)
    {

    }

    public override void init_player(CCSPlayerController player)
    {    
        // give shotty health and plenty of ammo
        weapon_restrict = "xm1014";
        player.GiveNamedItem("weapon_xm1014");

        player.set_health(1000);


        var shotgun = Lib.find_weapon(player,"xm1014");

        if(shotgun != null)
        {
            shotgun.set_ammo(999,999);
        }
    }
}