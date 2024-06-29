using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles;
public static class ChangeName
{
    public static string GetNowName(this PlayerData<string> pd, PlayerControl p)
    {
        if (!pd.TryGetValue(p, out string result))
            return p.GetDefaultName();
        return result;
    }

    public static void SetRoleName(PlayerControl player, bool IsUnchecked = false)
    {
        //SHRではない場合は処理しない
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        //デバッグが有効の場合は叩いた場所を表示する
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            StringBuilder logstr = new();
            logstr.Append($"[SHR:ChangeName]{player.name}への(IsCommsなしの)SetRoleNameが{callerMethod.DeclaringType.FullName}.{callerMethod.Name}から呼び出されました。");
            Logger.Info(logstr.ToString());
        }
        //コミュ情報を取得して呼ぶ
        SetRoleName(player, RoleHelpers.IsComms(), IsUnchecked);
    }
    private static string GetPlayerName(this PlayerControl player)
    {
        if (ModeHandler.IsMode(ModeId.HideAndSeek))
        {
            if (player.IsImpostor())
                return ModHelpers.Css(RoleClass.ImpostorRed, player.GetDefaultName());
        }
        return player.GetDefaultName();
    }
    public static void SetDefaultNames()
    {
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            StringBuilder logstr = new();
            logstr.Append($"SetDefaultNamesが{callerMethod.DeclaringType.FullName}.{callerMethod.Name}から呼び出されました。");
            SuperNewRolesPlugin.Logger.LogInfo(logstr.ToString());
        }
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            p.RpcSetName(p.GetDefaultName());
        }
    }
    public static void SetRoleNames(bool IsUnchecked = false)
    {
        //SHRではない場合は処理しない
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            StringBuilder logstr = new();
            logstr.Append($"[SHR:ChangeName]SetRoleNamesが{callerMethod.DeclaringType.FullName}.{callerMethod.Name}から呼び出されました。");
            SuperNewRolesPlugin.Logger.LogInfo(logstr.ToString());
        }

        bool commsActive = RoleHelpers.IsComms();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            SetRoleName(p, commsActive, IsUnchecked);
        }
    }
    public static void SetRoleName(PlayerControl player, bool commsActive, bool IsUnchecked = false)
    {
        //SHRではない場合は処理しない
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        //処理できない場合は処理しない
        if (player.IsBot() || !AmongUsClient.Instance.AmHost) return;

        //デバッグが有効の場合は叩いた場所を表示する
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            StringBuilder logstr = new();
            logstr.Append($"[SHR:ChangeName]{player.name}へのSetRoleNamesが{callerMethod.DeclaringType.FullName}.{callerMethod.Name}から呼び出されました。");
            SuperNewRolesPlugin.Logger.LogInfo(logstr.ToString());
        }

        //if (UpdateTime.ContainsKey(player.PlayerId) && UpdateTime[player.PlayerId] > 0) return;

        //UpdateTime[player.PlayerId] = UpdateDefaultTime;

        HashSet<PlayerControl> CanAllRolePlayers = new();
        HashSet<PlayerControl> AlivePlayers = new();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            //導入者ではないかつ
            if (!p.IsMod() &&
                //本人ではないかつ
                p.PlayerId != player.PlayerId &&
                //BOTではない
                !p.IsBot())
            {
                //神、もしくは死亡していてかつ役職が見れる場合
                if (SetNamesClass.CanGhostSeeRoles(p) || p.IsRole(RoleId.God))
                    CanAllRolePlayers.Add(p);
                else
                    AlivePlayers.Add(p);
            }
        }
        bool IsHideAndSeek = ModeHandler.IsMode(ModeId.HideAndSeek);
        //必要がないなら処理しない
        if (player.IsMod() && CanAllRolePlayers.Count < 1 && (!IsHideAndSeek || !player.IsImpostor())) return;

        RoleId PlayerRole = player.GetRole();

        StringBuilder NewName = new();
        StringBuilder MySuffix = new();
        StringBuilder RoleNameText = ModHelpers.Csb(CustomRoles.GetRoleColor(player), CustomRoles.GetRoleName(player));
        PlayerData<string> ChangePlayers = new(needplayerlist: true);
        PlayerSuffixes = new();
        ISupportSHR playerSHR = player.GetRoleBase() as ISupportSHR;

        // カモフラ中は処理を破棄する
        if (!RoleClass.Camouflager.IsCamouflage)
        {
            // スターパッシブ能力 [ カリスマ ] の処理
            List<PlayerControl> CelebrityViewPlayers = RoleClass.Celebrity.ChangeRoleView ?
            RoleClass.Celebrity.ViewPlayers : RoleClass.Celebrity.CelebrityPlayer;

            foreach (PlayerControl viewPlayer in CelebrityViewPlayers)
            {
                if (viewPlayer == player) continue;
                ChangePlayers[viewPlayer.PlayerId] = ModHelpers.Css(RoleClass.Celebrity.color, viewPlayer.GetDefaultName());
            }
        }
        if (Madmate.CheckImpostor(player) || PlayerRole == RoleId.Marlin)
        {
            // カモフラ中は処理を破棄する
            if (!RoleClass.Camouflager.IsCamouflage)
            {
                foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                {
                    if (Impostor.IsImpostor() && !Impostor.IsBot())
                    {
                        ChangePlayers[Impostor] = ModHelpers.Css(RoleClass.ImpostorRed, ChangePlayers.GetNowName(Impostor));
                    }
                }
            }
        }
        else if (JackalFriends.CheckJackal(player))
        {
            // カモフラ中は処理を破棄する
            if (!RoleClass.Camouflager.IsCamouflage)
            {
                foreach (PlayerControl Jackal in PlayerControl.AllPlayerControls)
                {
                    if (!Jackal.IsJackalTeam() || Jackal.IsDead())
                        continue;
                    ChangePlayers[Jackal.PlayerId] = ModHelpers.Css(RoleClass.Jackal.color, ChangePlayers.GetNowName(Jackal));
                }
            }
        }
        else
        {
            if (playerSHR != null)
            {
                playerSHR.BuildName(MySuffix, RoleNameText, ChangePlayers);
            }
            switch (PlayerRole)
            {
                case RoleId.Demon:
                    if (RoleClass.Demon.IsCheckImpostor && !RoleClass.Camouflager.IsCamouflage)
                    {
                        foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                        {
                            if (Impostor.IsImpostor() && !Impostor.IsBot())
                            {
                                ChangePlayers[Impostor.PlayerId] = ModHelpers.Css(RoleClass.ImpostorRed, ChangePlayers.GetNowName(Impostor));
                            }
                        }
                    }
                    foreach (PlayerControl CursePlayer in Demon.GetIconPlayers(player))
                    {
                        if (CursePlayer.IsBot()) continue;
                        if (!RoleClass.Camouflager.IsCamouflage)
                        {
                            AppendSuffix(CursePlayer.PlayerId, RoleClass.Demon.suffix);
                        }
                        else if (RoleClass.Camouflager.DemonMark)
                        {
                            if (!ChangePlayers.Contains(CursePlayer.PlayerId)) ChangePlayers[CursePlayer] = String.Empty;
                            AppendSuffix(CursePlayer.PlayerId, RoleClass.Demon.suffix);
                        }
                    }
                    break;
                case RoleId.Arsonist:
                    foreach (PlayerControl DousePlayer in Arsonist.GetIconPlayers(player))
                    {
                        if (DousePlayer.IsBot()) continue;
                        if (!RoleClass.Camouflager.IsCamouflage)
                        {
                            AppendSuffix(DousePlayer.PlayerId, RoleClass.Arsonist.suffix);
                        }
                        else if (RoleClass.Camouflager.ArsonistMark)
                        {
                            if (!ChangePlayers.Contains(DousePlayer.PlayerId)) ChangePlayers[DousePlayer] = String.Empty;
                            AppendSuffix(DousePlayer.PlayerId, RoleClass.Arsonist.suffix);
                        }
                    }
                    break;
                case RoleId.SatsumaAndImo:
                    SatsumaAndImo imo = player.GetRoleBase<SatsumaAndImo>();
                    if (imo == null)
                        break;
                    MySuffix.Append(imo.GetSuffixText());
                    break;
                case RoleId.Finder:
                    //マッドを表示させられる場合
                    if (RoleClass.Finder.KillCounts[player.PlayerId] >= RoleClass.Finder.CheckMadmateKillCount)
                    {
                        foreach (PlayerControl Player in CachedPlayer.AllPlayers)
                        {
                            if (!Player.IsBot() && Player.IsMadRoles())
                            {
                                ChangePlayers[Player.PlayerId] = ModHelpers.Css(RoleClass.ImpostorRed, ChangePlayers.GetNowName(Player));
                            }
                        }
                    }
                    break;
                case RoleId.Pokerface:
                    Pokerface.PokerfaceTeam pokerfaceTeam = Pokerface.GetPokerfaceTeam(player);
                    if (pokerfaceTeam != null)
                    {
                        foreach (PlayerControl pokerplayer in pokerfaceTeam.TeamPlayers)
                        {
                            if (pokerplayer.PlayerId == player.PlayerId)
                                continue;
                            ChangePlayers[pokerplayer.PlayerId] = ModHelpers.Css(Pokerface.RoleData.color, ChangePlayers.GetNowName(pokerplayer));
                        }
                    }
                    break;
            }
        }
        //プレイヤーがラバーズかつ
        if (player.IsLovers() &&
            //カモフラ中じゃない、もしくはラバーズマークがカモフラ中でも見える場合
            !RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.LoversMark)
        {
            PlayerControl Side = player.GetOneSideLovers();
            string name = Side.GetDefaultName();
            AppendSuffix(Side.PlayerId, RoleClass.Lovers.suffix);
            MySuffix.Append(RoleClass.Lovers.suffix);
        }
        //プレイヤーがクラードかつ
        if (player.IsQuarreled() &&
            //カモフラ中じゃない、もしくはクラードマークがカモフラ中でも見える場合
            !RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.QuarreledMark)
        {
            PlayerControl Side = player.GetOneSideQuarreled();
            string name = Side.GetDefaultName();
            AppendSuffix(Side.PlayerId, RoleClass.Quarreled.suffix);
            MySuffix.Append(RoleClass.Quarreled.suffix);
        }
        //ここで役職名の部分を変える
        switch (PlayerRole)
        {
            case RoleId.Sheriff:
                if (RoleClass.Sheriff.KillCount.TryGetValue(player.PlayerId, out int svalue))
                {
                    RoleNameText.Append(ModHelpers.Csb(IntroData.SheriffIntro.color, svalue.ToString()));
                }
                break;
            case RoleId.RemoteSheriff:
                if (RoleClass.RemoteSheriff.KillCount.TryGetValue(player.PlayerId, out int rsvalue))
                {
                    RoleNameText.Append(ModHelpers.Csb(IntroData.RemoteSheriffIntro.color, rsvalue.ToString()));
                }
                break;
            case RoleId.Mafia:
                if (Mafia.IsKillFlag())
                {
                    RoleNameText.Append(" (OK)");
                }
                break;
        }
        StringBuilder TaskText = new();
        if (player.IsUseTaskTrigger())
        {
            try
            {
                var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                TaskText.Append(ModHelpers.CsHead(Color.yellow));
                if (commsActive)
                {
                    TaskText.Append($"(?/{all})");
                }
                else
                {
                    TaskText.Append($"({Complete}/{all})");
                }
                TaskText.Append("</color>");
            }
            catch
            {
            }
        }

        bool IsDemonVIew = false;
        bool IsArsonistVIew = false;
        bool IsHauntedWolfVIew = false;
        bool IsGhostMechanicVIew = false; // 幽霊役職が1つしかない為, 単独処理
        StringBuilder attributeRoleName = new();

        if ((SetNamesClass.CanGhostSeeRoles(player) || player.IsRole(RoleId.God)) && !IsUnchecked)
        {
            if (Demon.IsViewIcon(player))
            {
                MySuffix.Append(RoleClass.Demon.suffix);
                IsDemonVIew = true;
            }
            if (Arsonist.IsViewIcon(player))
            {
                MySuffix.Append(RoleClass.Arsonist.suffix);
                IsArsonistVIew = true;
            }
            if (player.IsHauntedWolf())
            {
                attributeRoleName.Append(" + ");
                attributeRoleName.Append(ModHelpers.Csb(SuperNewRoles.Roles.Attribute.HauntedWolf.RoleData.color, ModTranslation.GetString("HauntedWolfName")));
                IsHauntedWolfVIew = true;
            }
            if (player.IsGhostRole(RoleId.GhostMechanic))
            {
                attributeRoleName.Append(" + ");
                attributeRoleName.Append(ModHelpers.Csb(RoleClass.GhostMechanic.color, ModTranslation.GetString("GhostMechanicName")));
                IsGhostMechanicVIew = true;
            }
            NewName.Append("(<size=75%>");
            NewName.Append(ModHelpers.Csb(CustomRoles.GetRoleColor(player), CustomRoles.GetRoleName(player)));
            NewName.Append(attributeRoleName);
            NewName.Append(TaskText);
            NewName.Append("</size>)");
            NewName.Append(ModHelpers.CsHead(CustomRoles.GetRoleColor(player)));
            if (!RoleClass.Camouflager.IsCamouflage)
                NewName.Append(player.GetDefaultName());
            NewName.Append(MySuffix);
            NewName.Append("</color>");
        }
        else if (player.IsAlive() || IsUnchecked)
        {
            if (SetNamesClass.CanGhostSeeRoles(player) || player.IsRole(RoleId.God))
            {
                if (Demon.IsViewIcon(player))
                {
                    MySuffix.Append(RoleClass.Demon.suffix);
                    IsDemonVIew = true;
                }
                if (Arsonist.IsViewIcon(player))
                {
                    MySuffix.Append(RoleClass.Arsonist.suffix);
                    IsArsonistVIew = true;
                }
            }
            NewName.Append("(<size=75%>");
            NewName.Append(RoleNameText);
            NewName.Append(attributeRoleName);
            NewName.Append(TaskText);
            NewName.Append("</size>)\n");
            NewName.Append(ModHelpers.CsHead(CustomRoles.GetRoleColor(player)));
            if (!RoleClass.Camouflager.IsCamouflage)
                NewName.Append(player.GetDefaultName());
            NewName.Append(MySuffix);
            NewName.Append("</color>");
        }
        if (!player.IsMod())
        {
            player.RpcSetNamePrivate(NewName.ToString());
            if (player.IsAlive())
            {
                foreach (var ChangePlayerData in (Dictionary<PlayerControl, string>)ChangePlayers)
                {
                    ChangePlayerData.Key.RpcSetNamePrivate(ChangePlayerData.Value, player);
                }
            }
        }
        if (player.IsImpostor() && IsHideAndSeek)
        {
            foreach (PlayerControl AlivePlayer in AlivePlayers)
            {
                if (AlivePlayer.IsMod()) continue;
                player.RpcSetNamePrivate(ModHelpers.Css(RoleClass.ImpostorRed, player.GetDefaultName()), AlivePlayer);
            }
        }
        StringBuilder DieSuffix = new();
        // FIXME : SHRにおいて重複役の名前変更の共通処理が完成していない。
        if (!IsDemonVIew && Demon.IsViewIcon(player))
            DieSuffix.Append(RoleClass.Demon.suffix);
        if (!IsArsonistVIew && Arsonist.IsViewIcon(player))
            DieSuffix.Append(RoleClass.Arsonist.suffix);
        if (!IsHauntedWolfVIew && player.IsHauntedWolf())
        {
            DieSuffix.Append(" + ");
            DieSuffix.Append(ModHelpers.Csb(SuperNewRoles.Roles.Attribute.HauntedWolf.RoleData.color, ModTranslation.GetString("HauntedWolfName")));
        }
        if (!IsGhostMechanicVIew && player.IsGhostRole(RoleId.GhostMechanic))
        {
            DieSuffix.Append(" + ");
            DieSuffix.Append(ModHelpers.Csb(RoleClass.GhostMechanic.color, ModTranslation.GetString("GhostMechanicName")));
        }
        NewName.Append(DieSuffix);
        foreach (PlayerControl DiePlayer in CanAllRolePlayers)
        {
            if (player.PlayerId != DiePlayer.PlayerId &&
                !DiePlayer.Data.Disconnected)
                player.RpcSetNamePrivate(NewName.ToString(), DiePlayer);
        }
    }
    private static Dictionary<byte, StringBuilder> PlayerSuffixes = new();
    private static void AppendSuffix(byte playerid, string suffix)
    {
        if (!PlayerSuffixes.TryGetValue(playerid, out StringBuilder sb)) sb = new();
        sb.Append(suffix);
        PlayerSuffixes[playerid] = sb;
    }
}