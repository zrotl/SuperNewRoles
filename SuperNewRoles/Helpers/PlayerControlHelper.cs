using System.Collections.Generic;
using System.Linq;
using System.Text;
using InnerNet;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Patches.ShareGameVersion;

namespace SuperNewRoles;

public static class PlayerControlHelper
{
    public static bool IsMod(this PlayerControl player)
    {
        return player != null && IsMod(player.GetClientId());
    }
    public static bool IsMod(this ClientData player)
    {
        return player != null && IsMod(player.Id);
    }
    public static bool IsMod(this int player)
    {
        return (player == AmongUsClient.Instance.HostId && AmongUsClient.Instance.AmHost)
|| GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(player);
    }
    public static void ClearAllTasks(this PlayerControl player)
    {
        if (player == null) return;
        for (int i = 0; i < player.myTasks.Count; i++)
        {
            PlayerTask playerTask = player.myTasks[i];
            playerTask.OnRemove();
            Object.Destroy(playerTask.gameObject);
        }
        player.myTasks.Clear();

        if (player.Data != null && player.Data.Tasks != null)
            player.Data.Tasks.Clear();
    }
    public static void RefreshRoleDescription(PlayerControl player)
    {
        if (player == null) return;
        Logger.Info($"Set Role Description. player : {player.name}", "RefreshRoleDescription");

        RoleId playerRole = player.GetRole();
        List<RoleId> infos = new() { player.GetRole() };
        if (playerRole == RoleId.Bestfalsecharge && player.IsAlive())
        {
            playerRole = RoleId.DefaultRole;
            infos = new() { RoleId.DefaultRole };
        }

        var toRemove = new List<PlayerTask>();
        foreach (PlayerTask t in player.myTasks)
        {
            var textTask = t.gameObject.GetComponent<ImportantTextTask>();
            if (textTask == null) continue;
            if (textTask.Text.StartsWith(CustomRoles.GetRoleName(player)))
                infos.Remove(playerRole); // TextTask for this RoleInfo does not have to be added, as it already exists
            else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
        }

        foreach (PlayerTask t in toRemove)
        {
            t.OnRemove();
            player.myTasks.Remove(t);
            Object.Destroy(t.gameObject);
        }

        Logger.Info($"Set Role Description. infos : {string.Join(", ", infos)}", "RefreshRoleDescription");
        // Add TextTask for remaining RoleInfos
        foreach (RoleId roleId in infos)
        {
            // Add TextTask for remaining RoleInfos
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);

            StringBuilder taskText = new();
            taskText.Append($"{CustomRoles.GetRoleName(roleId)}: {CustomRoles.GetRoleIntro(roleId)}");
            taskText = ModHelpers.Csb(CustomRoles.GetRoleColor(roleId), taskText);
            if (player.IsLovers() || player.IsFakeLovers())
            {
                StringBuilder loversText = new();
                loversText.Append(ModTranslation.GetString("LoversName"));
                loversText.Append(": ");
                loversText.AppendFormat(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? "");
                loversText = ModHelpers.Csb(RoleClass.Lovers.color, loversText);

                taskText.Append('\n');
                taskText.Append(loversText);
            }
            if (!player.IsGhostRole(RoleId.DefaultRole))
            {
                StringBuilder ghostText = new();
                ghostText.Append($"{CustomRoles.GetRoleName(player.GetGhostRole(), player)}: {CustomRoles.GetRoleIntro(player.GetGhostRole(), player)}");
                ghostText = ModHelpers.Csb(CustomRoles.GetRoleColor(player.GetGhostRole(), player), ghostText);

                taskText.Append("\n");
                taskText.Append(ghostText);
            }

            player.myTasks.Insert(0, task);
        }
    }
}