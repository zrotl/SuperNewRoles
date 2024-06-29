using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Mode.CopsRobbers;

class RoleSystem
{
    public static void RoleSetName()
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("[CopsRobbers:FixedUpdate] SetRoleNamesが" + callerClassName + "." + callerMethodName + "から呼び出されました。");

        bool commsActive = RoleHelpers.IsComms();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            SetRoleName(p, commsActive);
        }
    }
    public static void SetRoleName(PlayerControl player, bool commsActive)
    {
        if (!ModeHandler.IsMode(ModeId.CopsRobbers)) return;
        if (!AmongUsClient.Instance.AmHost) return;

        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;

        //必要がないなら処理しない
        if (player.IsMod()) return;

        string Name = player.GetDefaultName();
        Dictionary<byte, string> ChangePlayers = new();

        /*
        if (player.IsLovers())
        {
            var suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
            PlayerControl Side = player.GetOneSideLovers();
            string name = Side.GetDefaultName();
            if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetDefaultName() + suffix);
            else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
            MySuffix += suffix;
        }
        if (player.IsQuarreled())
        {
            var suffix = ModHelpers.Cs(RoleClass.Quarreled.color, "○");
            PlayerControl Side = player.GetOneSideQuarreled();
            string name = Side.GetDefaultName();
            if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetDefaultName() + suffix);
            else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
            MySuffix += suffix;
        }
        */

        StringBuilder TaskText = new();
        if (!player.IsImpostor())
        {
            try
            {
                if (commsActive)
                {
                    TaskText.AppendCs(Color.yellow, "(?/", TaskCount.TaskDateNoClearCheck(player.Data).Item2.ToString(), ")");
                }
                else
                {
                    var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                    TaskText.AppendCs(Color.yellow, "(", Complete.ToString(), "/", all.ToString(), ")");
                }
            }
            catch { }
        }
        StringBuilder NewName = new();
        NewName.Append("<size=75%>");
        NewName.Append(CustomRoles.GetRoleNameOnColor(player));
        NewName.Append(TaskText);
        NewName.Append("</size>\n");
        if (CopsRobbersOptions.CRHideName.GetBool() && CopsRobbersOptions.CopsRobbersMode.GetBool())
            NewName.Append(" ");
        else
            NewName.AppendCs(CustomRoles.GetRoleColor(player), Name);
        player.RpcSetNamePrivate(NewName.ToString());
    }
    public static void AssignRole()
    {
        foreach (IntroData intro in IntroData.Intros.Values)
        {
            if (!(intro.RoleId is
                RoleId.Workperson or RoleId.HomeSecurityGuard or RoleId.Tuna or RoleId.ToiletFan))
                continue;
            var option = IntroData.GetOption(intro.RoleId);
            if (option == null) continue;
            var selection = option.GetSelection();
            AllRoleSetClass.SetChance(selection, intro.RoleId, intro.Team);
        }
        foreach (RoleInfo info in RoleInfoManager.RoleInfos.Values)
        {
            if (!(info.Role is
                RoleId.Workperson or RoleId.HomeSecurityGuard or RoleId.Tuna or RoleId.ToiletFan))
                continue;
            var option = IntroData.GetOption(info.Role);
            if (option == null) continue;
            var selection = option.GetSelection();
            AllRoleSetClass.SetChance(selection, info.Role, info.Team);
        }
        AllRoleSetClass.CrewOrImpostorSet();
        AllRoleSetClass.AllRoleSet();
        SuperHostRoles.RoleSelectHandler.SetCustomRoles();
        SyncSetting.CustomSyncSettings();
        ChacheManager.ResetChache();
    }
}