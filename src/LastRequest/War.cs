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

public class LRWar : LRBase
{
    public LRWar(LastRequest manager,LastRequest.LRType type,int LRSlot, int playerSlot, String choice) : base(manager,type,LRSlot,playerSlot,choice)
    {

    }

    public override void InitPlayer(CCSPlayerController player)
    {    
        // give weapon health and plenty of ammo
        weaponRestrict = Weapon.GUN_LIST[choice];
  
        player.GiveWeapon(weaponRestrict);

        player.SetHealth(1000);


        var shotgun = player.FindWeapon(weaponRestrict);

        if(shotgun != null)
        {
            shotgun.SetAmmo(999,999);
        }
    }
}