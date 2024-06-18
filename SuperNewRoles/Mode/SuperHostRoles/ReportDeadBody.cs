using System.Linq;
using HarmonyLib;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Mode.PlusMode;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles;

class ReportDeadBody
{
    public static bool ReportDeadBodyPatch(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (RoleClass.Assassin.TriggerPlayer != null) return false;
        //会議ボタンでもレポートでも起こる処理

        if (target == null) //会議ボタンのみで起こる処理
        {

            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, false) &&
                MapCustom.TheFungleMushroomMixupOption.GetBool() &&
                MapCustom.TheFungleMushroomMixupCantOpenMeeting.GetBool() &&
                __instance.IsMushroomMixupActive())
                return false;
            if (PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting && PlusGameOptions.EmergencyMeetingsCallstate.maxCount != byte.MaxValue && // 会議回数制限が有効で
                PlusGameOptions.EmergencyMeetingsCallstate.maxCount <= Patches.ReportDeadBodyPatch.MeetingCount.emergency) // 全体回数を使い切っているなら
                return false;
            return true;
        };

        //死体レポートのみで起こる処理
        DeadPlayer deadPlayer;
        deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId)?.FirstOrDefault();
        //if (RoleClass.Bait.ReportedPlayer.Contains(target.PlayerId)) return true;
        if (__instance.IsRole(RoleId.Minimalist))
        {
            var a = RoleClass.Minimalist.UseReport;
            return a;
        }
        if (__instance.IsRole(RoleId.Fox))
        {
            var a = RoleClass.Fox.UseReport;
            return a;
        }
        //if (target.Object.IsRole(RoleId.Bait) && (!deadPlayer.killerIfExisting.IsRole(RoleId.Minimalist) || RoleClass.Minimalist.UseReport)) if (!RoleClass.Bait.ReportedPlayer.Contains(target.PlayerId)) { return false; } else { return true; }

        return true;
    }
}