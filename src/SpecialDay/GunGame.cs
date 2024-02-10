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


public class SDGunGame : SDBase
{
    public override void Setup()
    {
        Random rand = new Random();

        // shuffle the guns
        for (int i = guns.Count()-1; i >= 0; i--)
        {
            int idx = rand.Next(0, i + 1);
            
            String tmp = guns[i];
            guns[i] = guns[idx];
            guns[idx] = tmp;
        }

        for(int i = 0; i < level.Count(); i++)
        {
            level[i] = 0;
        }


        LocalizeAnnounce("sd.gun_game_start");
        LocalizeAnnounce("sd.damage_enable",delay);
    }

    public override void Start()
    {
        LocalizeAnnounce("sd.fight");
    }

    public override void End()
    {
        LocalizeAnnounce("sd.gun_game_end");
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        // give the current level weapon
        player.StripWeapons();
        player.GiveArmour();
        
        int gunLevel = level[player.Slot];

        player.GiveWeapon(guns[gunLevel]);

        player.LocalizePrefix(SpecialDay.SPECIALDAY_PREFIX,"sd.gun_game_level",gunLevel,guns[gunLevel]);

    }


    public override void Death(CCSPlayerController? player, CCSPlayerController? attacker,String weapon)
    {
        if(!player.IsLegal() || !attacker.IsLegalAlive())
        {
            return;
        }


        String curGun = guns[level[attacker.Slot]];

        // give attacker another level if they used the current gun
        if(weapon.Contains(curGun))
        {
            // advance to next level
            level[attacker.Slot] += 1;

            // player has won
            if(level[attacker.Slot] >= guns.Count())
            {
                LocalizeAnnounce("sd.gun_game_win",attacker.PlayerName);

                // end the round
                Player.Nuke();
            }

            else
            {
                SetupPlayer(attacker);
            }
        }

        // decrement the victim level
        else if(weapon.Contains("knife"))
        {
            if(level[player.Slot] <= 0)
            {
                level[player.Slot] -= 1;
            }
        }

        ResurectPlayer(player,0.1f);
    }

    String[] guns = 
    {
        "ak47",
        "m4a1_silencer",
        "nova",
        "p90",
        "deagle",
        "m249",
        "mp5sd",
        "galilar",
        "sg556",
        "bizon",
        "aug",
        "famas",
        "xm1014",
        "ssg08",
        "awp",
    };

    int[] level = new int[64];
}