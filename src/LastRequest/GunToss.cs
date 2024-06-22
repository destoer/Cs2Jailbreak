using System.Numerics;
using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

public class LRGunToss : LRBase
{
    public LRGunToss(LastRequest manager,LastRequest.LRType type,int LRSlot, int playerSlot, String choice) : base(manager,type,LRSlot,playerSlot,choice)
    {
        
    }

    public override void InitPlayer(CCSPlayerController player)
    {    
        weaponRestrict = "deagle";
        player.GiveWeapon("knife");
        player.GiveWeapon("deagle");
        // empty ammo so players dont shoot eachother
        var deagle = player.FindWeapon("weapon_deagle");

        if(deagle != null)
        {
            // colour gun to player team!
            deagle.SetColour(player.IsT()? Lib.RED : Lib.CYAN);
            
            deagle.SetAmmo(0,0);
        }         
    }

    public override bool WeaponEquip(String name) 
    {
        return name.Contains("knife") || name.Contains("deagle");  
    }
    

    public override void WeaponDropped(CCSPlayer_ItemServices itemServices, CBasePlayerWeapon weapon)
    {
        //TODO: implement beacon and shit
    }
}