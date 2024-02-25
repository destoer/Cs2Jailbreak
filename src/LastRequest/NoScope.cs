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

public class LRNoScope : LRBase
{
    public LRNoScope(LastRequest manager,LastRequest.LRType type,int LRSlot, int playerSlot, String choice) : base(manager,type,LRSlot,playerSlot,choice)
    {

    }

    void GiveWeapon(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }


        player.StripWeapons(true);

        switch(choice)
        {
            case "Scout":
            {
                weaponRestrict = "ssg08";
                player.GiveWeapon("ssg08");
                break;
            }

            case "Awp":
            {
                weaponRestrict = "awp";
                player.GiveWeapon("awp");
                break;
            }
        }
    }

    public override void WeaponFire(String name)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

        var weapon = player.FindWeapon(name);
        weapon.SetAmmo(5,-1);
    }

    public override void InitPlayer(CCSPlayerController player)
    {
        GiveWeapon(player);
    }

    public override void WeaponZoom()
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

        // re give the weapons so they cannot zoom
        GiveWeapon(player);
    }
}