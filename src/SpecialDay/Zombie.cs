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


public class SDZombie : SDBase
{
    public override void Setup()
    {
        for(int i = 0; i < 64; i++)
        {
            deathCount[i] = 0;
        }

        LocalizeAnnounce("sd.zombie_start");
        LocalizeAnnounce("sd.damage_enable",delay);

        Lib.SwapAllCT();
    }


    public override void MakeBoss(CCSPlayerController? patientZero, int count)
    {
        if(patientZero.IsLegalAlive())
        {
            LocalizeAnnounce($"sd.patient_zero",patientZero.PlayerName);

            // give the tank the HP and swap him
            patientZero.SetHealth(count * 1000);
            patientZero.SwitchTeam(CsTeam.Terrorist);
            SetupZombie(patientZero);
        }

        else
        {
            Chat.Announce("[ERROR]: ","Error picking patient zero");
        }
    }

    public override void Start()
    {
        LocalizeAnnounce("sd.fight");

        (CCSPlayerController? boss, int count) = PickBoss();
        MakeBoss(boss,count);
    }

    public void ZombieRespawn(CCSPlayerController player, float delay)
    {
        ResurectPlayer(player,3.0f);
    }

    public override void Death(CCSPlayerController? player, CCSPlayerController? attacker,String weapon)
    {
        // Dont run the death event if the sd is not  active
        // to prevent exploits
        if(!player.IsLegal() || state != SDState.ACTIVE)
        {
            return;
        }
        

        // CT died make them a zombie
        if(player.IsCt())
        {
            var left = Lib.GetAliveCt();

            // last player just let them die
            if(left.Count() <= 0)
            {
                return;
            }

            // handle last man standing
            if(left.Count() == 1)
            {
                var last = left[0];

                LocalizeAnnounce("sd.last_man_standing",last.PlayerName);

                last.SetHealth(350);
            }

            player.SwitchTeam(CsTeam.Terrorist);
            
            var pawn = player.Pawn();

            // save death cordinates
            if(pawn != null && pawn.AbsOrigin != null)
            {
                // make sure this is by copy
                deathCord[player.Slot].X = pawn.AbsOrigin.X;
                deathCord[player.Slot].Y = pawn.AbsOrigin.Y;
                deathCord[player.Slot].Z = pawn.AbsOrigin.Z;
                deathCount[player.Slot] += 1;
            }

            // couldn't get the cords just put them at patient zero
            else
            {

                deathCount[player.Slot] = 2;
            }

            // First death has a very fast respawn
            ResurectPlayer(player,0.3f);
        }

        // zombie, respawn them on patient zero if alive
        else
        {
            var patientZero = Utilities.GetPlayerFromSlot(bossSlot);

            if(patientZero.IsLegalAlive())
            {
                ResurectPlayer(player,3.0f);
            }
        }
    }

    public override void PlayerHurt(CCSPlayerController? player,CCSPlayerController? attacker,int health,int damage, int hitgroup) 
    {
        if(!player.IsLegalAlive() || !attacker.IsLegalAlive())
        {
            return;
        }

        if(attacker.Slot == bossSlot)
        {
            // if attacker is patient zero kill them instantly
            player.Slay();
        }


        // only want zombie to ct damage knockback
    /*
        if(!player.IsT() || !attacker.IsCt())
        {
            return;
        }


        // add knockback to player, scaled from damage
        var playerPawn = player.Pawn();
        var attackerPawn = attacker.Pawn();


        if(playerPawn != null && attackerPawn != null && playerPawn.AbsVelocity != null 
            && playerPawn.AbsOrigin != null && attackerPawn.AbsOrigin != null)
        {
            Vector pos = Vec.Sub(attackerPawn.AbsOrigin,playerPawn.AbsOrigin);
            pos = Vec.Normalize(pos);

            float scale = 30.0f;

            // scale it (may need balancing)
            Vector push = Vec.Scale(pos, scale);
        
            //Server.PrintToChatAll($"damage {damage}");
            //Server.PrintToChatAll($"pos {pos.X} {pos.Y} {pos.Z}");
            //Server.PrintToChatAll($"push {push.X} {push.Y} {push.Z}");
        
            playerPawn.AbsVelocity.Add(push);
        }
    */
    }

    public override void End()
    {
        LocalizeAnnounce("sd.zombie_end");
    }

    public override bool WeaponEquip(CCSPlayerController player,String name) 
    {
        // zombie can only have knife
        if(player.IsT())
        {
            return name.Contains("knife");
        }

        return true;
    }

    public void SetupZombie(CCSPlayerController player)
    {
        player.SetVelocity(1.2f);
        player.SetGravity(0.4f);
        player.StripWeapons();
        player.SetColour(Lib.RED);
        player.GiveArmour();
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        if(player.IsCt())
        {
            player.EventGunMenu();
            player.SetColour(Lib.CYAN);
        }

        else if(player.IsT())
        {
            SetupZombie(player);

            if(player.Slot != bossSlot)
            {
                player.SetHealth(250);
            }

            // respawn them on their death cordinates
            if(deathCount[player.Slot] == 1)
            {
                player.Teleport(deathCord[player.Slot],Lib.ANGLE_ZERO,Lib.VEC_ZERO);
            }

            // teleport player to patient zero
            else
            {
                var patientZero = Utilities.GetPlayerFromSlot(bossSlot);

                if(patientZero.IsLegalAlive())
                {
                    var pawn = patientZero.Pawn();

                    if(pawn != null && pawn.AbsOrigin != null)
                    {
                        player.Teleport(pawn.AbsOrigin,Lib.ANGLE_ZERO,Lib.VEC_ZERO);
                    }
                }
            }
        }
    }

    int[] deathCount = new int[64];
    Vector[] deathCord = new Vector[64];
}