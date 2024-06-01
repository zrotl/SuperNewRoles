using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using RewiredConsts;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using TMPro;
using UnityEngine;
using static Il2CppSystem.Globalization.CultureInfo;
using static Il2CppSystem.Uri;
using Debugger = SuperNewRoles.Roles.Attribute.Debugger;

namespace SuperNewRoles.Modules;

public class SetNamesClass
{
    public static Dictionary<int, string> AllNames = new();
    public static Dictionary<int, CachedState> CachedPlayersState;

    public static void ApplyPlayerNameColor(PlayerControl player)
    {
        if (player.IsBot()) return;
        player.NameText().color = CachedPlayersState[player.PlayerId].nameColor;
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea voteplayer in MeetingHud.Instance.playerStates)
            {
                voteplayer.NameText.color = CachedPlayersState[player.PlayerId].nameColor;
            }
        }
    }
    public static void SetPlayerNameColor(PlayerControl p, Color color)
    {
        if (p.IsBot()) return;
        CachedPlayersState[p.PlayerId].nameColor = color;
        return;
        if (p.IsBot()) return;
        p.NameText().color = color;
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
            {
                if (p.PlayerId == player.TargetPlayerId)
                {
                    player.NameText.color = color;
                }
            }
        }
    }
    public static void ApplyPlayerNameText(PlayerControl p)
    {
        if (p.IsBot()) return;
        p.NameText().text = CachedPlayersState[p.PlayerId].cachedName;
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
            {
                if (player.TargetPlayerId == p.PlayerId)
                {
                    player.NameText.text = CachedPlayersState[p.PlayerId].cachedName;
                    return;
                }
            }
        }
    }
    public static void SetPlayerNameText(PlayerControl p, string text)
    {
        if (p.IsBot()) return;
        CachedPlayersState[p.PlayerId].cachedName = text;
        return;
        if (p.IsBot()) return;
        p.NameText().text = text;
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
            {
                if (player.TargetPlayerId == p.PlayerId)
                {
                    player.NameText.text = text;
                    return;
                }
            }
        }
    }
    public static void ResetNameTagsAndColors(PlayerControl player)
    {
        SetNamesClass.CachedPlayersState[player.PlayerId].Reset();
        if ((PlayerControl.LocalPlayer.IsImpostor() && (player.IsImpostor() || player.IsRole(RoleId.Spy, RoleId.Egoist))) || (ModeHandler.IsMode(ModeId.HideAndSeek) && player.IsImpostor()))
        {
            SetPlayerNameColor(player, RoleClass.ImpostorRed);
        }
        else
        {
            SetPlayerNameColor(player, Color.white);
        }
    }

    public static void ResetNameTagsAndColors()
    {
        Dictionary<byte, PlayerControl> playersById = ModHelpers.AllPlayersById();

        foreach (var pro in PlayerInfos)
        {
            pro.Value.text = "";
        }
        foreach (var pro in MeetingPlayerInfos)
        {
            pro.Value.text = "";
        }
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
        {
            bool hidename = ModHelpers.HidePlayerName(PlayerControl.LocalPlayer, player);
            player.NameText().text = hidename ? "" : player.CurrentOutfit.PlayerName;
            if ((PlayerControl.LocalPlayer.IsImpostor() && (player.IsImpostor() || player.IsRole(RoleId.Spy, RoleId.Egoist))) || (ModeHandler.IsMode(ModeId.HideAndSeek) && player.IsImpostor()))
            {
                SetPlayerNameColor(player, RoleClass.ImpostorRed);
            }
            else
            {
                SetPlayerNameColor(player, Color.white);
            }
        }
    }
    public static Dictionary<byte, TextMeshPro> PlayerInfos = new();
    public static Dictionary<byte, TextMeshPro> MeetingPlayerInfos = new();

    public static void ApplyPlayerRoleInfoView(PlayerControl player)
    {
        if (player.IsBot()) return;

        if (!PlayerInfos.TryGetValue(player.PlayerId, out TextMeshPro playerInfo) || playerInfo == null)
        {
            playerInfo = UnityEngine.Object.Instantiate(player.NameText(), player.NameText().transform.parent);
            playerInfo.fontSize *= 0.75f;
            playerInfo.gameObject.name = "Info";
            PlayerInfos[player.PlayerId] = playerInfo;
        }
        PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
        if ((!MeetingPlayerInfos.TryGetValue(player.PlayerId, out TextMeshPro meetingInfo) || meetingInfo == null) && playerVoteArea != null)
        {
            meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
            meetingInfo.transform.localPosition += Vector3.down * 0.1f;
            meetingInfo.fontSize = 1.5f;
            meetingInfo.gameObject.name = "Info";
            MeetingPlayerInfos[player.PlayerId] = meetingInfo;
        }
        // Set player name higher to align in middle
        if (meetingInfo != null && playerVoteArea != null)
        {
            var playerName = playerVoteArea.NameText;
            playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f + 0.0683f, -0.1f);
        }

        playerInfo.text = CachedPlayersState[player.PlayerId].cachedInfo;

        if (playerInfo.gameObject.active != player.Visible) playerInfo.gameObject.SetActive(player.Visible);
        if (meetingInfo != null)
        {
            meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : CachedPlayersState[player.PlayerId].cachedMeetingInfo;
            player.NameText().color = CachedPlayersState[player.PlayerId].roleColor;
        }
    }

    public static void SetPlayerRoleInfoView(PlayerControl p, Color roleColors, string roleNames, Dictionary<string, (Color, bool)> attributeRoles, Color? GhostRoleColor = null, string GhostRoleNames = "")
    {
        bool commsActive = RoleHelpers.IsComms();

        //string TaskText = "";
        StringBuilder TaskTextBuilder = new();
        try
        {
            if (p.IsUseTaskTrigger())
            {
                var (complete, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                ModHelpers.AppendCs(TaskTextBuilder, Color.yellow, "(" + (commsActive ? "?" : complete.ToString()) + "/" + all.ToString() + ")");
            }
        }
        catch { }
        //string playerInfoText = "";
        //string meetingInfoText = "";
        StringBuilder playerInfoTextBuilder = new(CustomOptionHolder.GetCsWithTranslation(roleColors, roleNames));
        if (GhostRoleNames != "")
        {
            playerInfoTextBuilder.Insert(0, "(");
            playerInfoTextBuilder.Insert(0, CustomOptionHolder.GetCsWithTranslation((Color)GhostRoleColor, GhostRoleNames));
            playerInfoTextBuilder.Append(")");
        }

        if (attributeRoles.Count != 0)
        {
            foreach (var kvp in attributeRoles)
            {
                if (!kvp.Value.Item2) continue;
                playerInfoTextBuilder.Append(" + ");
                ModHelpers.AppendCs(playerInfoTextBuilder, kvp.Value.Item1, kvp.Key);
            }
        }
        playerInfoTextBuilder.Append(TaskTextBuilder);
        CachedPlayersState[p.PlayerId].cachedInfo = playerInfoTextBuilder.ToString();
        CachedPlayersState[p.PlayerId].cachedMeetingInfo = playerInfoTextBuilder.ToString().Trim();
    }

    public static void SetPlayerRoleInfo(PlayerControl p)
    {
        if (p.IsBot()) return;
        string roleNames;
        Color roleColors;
        string GhostroleNames = "";
        Color? GhostroleColors = null;

        var role = p.GetRole();
        if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && p.IsAlive()))
        {
            if (p.IsImpostor())
            {
                roleNames = "ImpostorName";
                roleColors = RoleClass.ImpostorRed;
            }
            else
            {
                roleNames = "CrewmateName";
                roleColors = RoleClass.CrewmateWhite;
            }
        }
        else if (role == RoleId.Stefinder && RoleClass.Stefinder.IsKill)
        {
            roleNames = IntroData.StefinderIntro.Name;
            roleColors = RoleClass.ImpostorRed;
        }
        else if (p.IsPavlovsTeam())
        {
            var introData = IntroData.PavlovsdogsIntro;
            roleNames = introData.Name + (role == RoleId.Pavlovsdogs ? "(D)" : "(O)");
            roleColors = RoleClass.Pavlovsdogs.color;
        }
        else if (WaveCannonJackal.IwasSidekicked.Contains(p.PlayerId) &&
                !WaveCannonJackal.WaveCannonJackalNewJackalHaveWaveCannon.GetBool())
        {
            if (p.IsRole(RoleId.WaveCannonJackal))
            {
                var introData = IntroData.JackalIntro;
                roleNames = introData.Name;
                roleColors = introData.color;
            }
            else
            {
                var introData = IntroData.SidekickIntro;
                roleNames = introData.Name;
                roleColors = introData.color;
            }
        }
        else
        {
            roleNames = CustomRoles.GetRoleName(role, p);
            roleColors = CustomRoles.GetRoleColor(role, p);
        }

        var GhostRole = p.GetGhostRole();
        if (GhostRole != RoleId.DefaultRole)
        {
            GhostroleNames = CustomRoles.GetRoleName(GhostRole, p);
            GhostroleColors = CustomRoles.GetRoleColor(GhostRole, p);
        }

        Dictionary<string, (Color, bool)> attributeRoles = new(AttributeRoleNameSet(p));

        SetPlayerRoleInfoView(p, roleColors, roleNames, attributeRoles, GhostroleColors, GhostroleNames);
    }

    /// <summary>
    /// 重複役職の役職名を追加する。
    /// key = 役職名, value.Item1 = 役職カラー, value.Item2 = 役職名の表示条件を達しているか,
    /// </summary>
    /// <param name="player">役職名を表示したいプレイヤー</param>
    /// <param name="seePlayer">役職名を見るプレイヤー</param>
    internal static Dictionary<string, (Color, bool)> AttributeRoleNameSet(PlayerControl player, PlayerControl seePlayer = null)
    {
        Dictionary<string, (Color, bool)> attributeRoles = new();
        if (player.IsHauntedWolf())
        {
            if (seePlayer == null) seePlayer = PlayerControl.LocalPlayer;
            var isSeeing = seePlayer.IsDead() || seePlayer.IsRole(RoleId.God, RoleId.Marlin);
            attributeRoles.Add(ModTranslation.GetString("HauntedWolfName"), (HauntedWolf.RoleData.color, isSeeing));
        }
        return attributeRoles;
    }

    /// <summary>
    /// 死亡後, 全員の役職が見る事ができるか判定する。
    /// 見る事ができない状態は, 見る事ができる状態より優先して反映する。
    /// </summary>
    /// <param name="target">役職を見ようとしているプレイヤー (nullなら処理者本人)</param>
    /// <returns> true:見られる / false:見られない </returns>
    public static bool CanGhostSeeRoles(PlayerControl target = null)
    {
        if (target == null) target = PlayerControl.LocalPlayer;
        if (target.IsDead())
        {
            if (target.GetRoleBase() is INameHandler INameHandler) // 役職の個別判定
            {
                if (!INameHandler.CanGhostSeeRole) return false;  // 死後 役職をみえない役職の場合, 最優先で判定する。
            }

            // ゲーム設定による, 基本的な判定
            // (役職の個別判定で死後役職が見られる場合は, ゲーム設定による判定を優先する)
            if (!Mode.PlusMode.PlusGameOptions.PlusGameOptionSetting.GetBool()) return true; // 上位設定 "ゲームオプション" が有効か
            else
            {
                if (!Mode.PlusMode.PlusGameOptions.CanNotGhostSeeRole.GetBool()) return true;
                else if (Mode.PlusMode.PlusGameOptions.OnlyImpostorGhostSeeRole.GetBool()) return target.IsImpostor();
            }
        }
        return false;
    }

    public static void SetPlayerNameColors(PlayerControl player)
    {
        var role = player.GetRole();
        if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && player.IsAlive())) return;
        SetPlayerNameColor(player, CustomRoles.GetRoleColor(player));
    }
    public static void SetPlayerRoleNames(PlayerControl player)
    {
        SetPlayerRoleInfo(player);
    }
    public static string QuarreledSuffix = ModHelpers.GetCs(RoleClass.Quarreled.color, "○");
    public static void QuarreledSet()
    {
        if (PlayerControl.LocalPlayer.IsQuarreled() && PlayerControl.LocalPlayer.IsAlive())
        {
            PlayerControl side = PlayerControl.LocalPlayer.GetOneSideQuarreled();
            if (SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].nameflg) SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.NameText().text + QuarreledSuffix);
            if (!side.Data.Disconnected)
            {
                if (SetNamesClass.CachedPlayersState[side.PlayerId].nameflg) SetPlayerNameText(side, side.NameText().text + QuarreledSuffix);
            }
        }
        if (CanGhostSeeRoles() && RoleClass.Quarreled.QuarreledPlayer != new List<List<PlayerControl>>())
        {
            foreach (List<PlayerControl> ps in RoleClass.Quarreled.QuarreledPlayer)
            {
                foreach (PlayerControl p in ps)
                {
                    if (!p.Data.Disconnected)
                    {
                        if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetPlayerNameText(p, p.NameText().text + QuarreledSuffix);
                    }
                }
            }
        }
    }
    public static void JumboSet()
    {
        foreach (PlayerControl p in RoleClass.Jumbo.BigPlayer)
        {
            if (!RoleClass.Jumbo.JumboSize.ContainsKey(p.PlayerId)) continue;
            if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetPlayerNameText(p, p.NameText().text + $"({(int)(RoleClass.Jumbo.JumboSize[p.PlayerId] * 15)})");
        }
    }
    public static string LoversSuffix = ModHelpers.GetCs(RoleClass.Lovers.color, " ♥");
    public static void LoversSet()
    {
        if ((PlayerControl.LocalPlayer.IsLovers() || (PlayerControl.LocalPlayer.IsFakeLovers() && !PlayerControl.LocalPlayer.IsFakeLoversFake())) && PlayerControl.LocalPlayer.IsAlive())
        {
            PlayerControl side = PlayerControl.LocalPlayer.GetOneSideLovers();
            if (side == null) side = PlayerControl.LocalPlayer.GetOneSideFakeLovers();
            if (SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].nameflg) SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.NameText().text + LoversSuffix);
            if (!side.Data.Disconnected && SetNamesClass.CachedPlayersState[side.PlayerId].nameflg)
                SetPlayerNameText(side, side.NameText().text + LoversSuffix);
        }
        else if ((CanGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God)) && RoleClass.Lovers.LoversPlayer != new List<List<PlayerControl>>())
        {
            foreach (List<PlayerControl> ps in RoleClass.Lovers.LoversPlayer)
            {
                foreach (PlayerControl p in ps)
                {
                    if (!p.Data.Disconnected && SetNamesClass.CachedPlayersState[p.PlayerId].nameflg)
                        SetPlayerNameText(p, p.NameText().text + LoversSuffix);
                }
            }
        }
    }
    public static string DemonSuffix = ModHelpers.GetCs(RoleClass.Demon.color, " ▲");
    public static void DemonSet()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Demon, RoleId.God) || CanGhostSeeRoles())
        {
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (Demon.IsViewIcon(player))
                {
                    //if (!player.NameText().text.Contains(DemonSuffix))
                    if (SetNamesClass.CachedPlayersState[player.PlayerId].nameflg)
                        SetPlayerNameText(player, player.NameText().text + DemonSuffix);
                }
            }
        }
    }
    public static string ArsonistSuffix = ModHelpers.GetCs(RoleClass.Arsonist.color, " §");
    public static void ArsonistSet()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist, RoleId.God) || CanGhostSeeRoles())
        {
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (Arsonist.IsViewIcon(player))
                {
                    //if (!player.NameText().text.Contains(ArsonistSuffix))
                    if (SetNamesClass.CachedPlayersState[player.PlayerId].nameflg)
                        SetPlayerNameText(player, player.NameText().text + ArsonistSuffix);
                }
            }
        }
    }
    public static void MoiraSet()
    {
        if (!Moira.AbilityUsedUp || Moira.AbilityUsedThisMeeting) return;
        if (Moira.Player is null) return;
        if (SetNamesClass.CachedPlayersState[Moira.Player.PlayerId].nameflg) SetPlayerNameText(Moira.Player, Moira.Player.NameText().text += " (→←)");
    }
    public static void CelebritySet()
    {
        foreach (PlayerControl p in
            RoleClass.Celebrity.ChangeRoleView ?
            RoleClass.Celebrity.ViewPlayers :
            RoleClass.Celebrity.CelebrityPlayer)
        {
            if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetPlayerNameColor(p, RoleClass.Celebrity.color);
        }
    }
    public static string Satsumaimo = ModHelpers.GetCs(RoleClass.Arsonist.color, " (C)");
    public static string WhiteSatsumaimo = ModHelpers.GetCs(Palette.White, " (C)");
    public static string RedSatsumaimo = ModHelpers.GetCs(RoleClass.ImpostorRed, " (M)");
    public static void SatsumaimoSet()
    {
        if (CanGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
        {
            foreach (SatsumaAndImo player in RoleBaseManager.GetRoleBases<SatsumaAndImo>())
            {
                //クルーなら
                if (SetNamesClass.CachedPlayersState[player.Player.PlayerId].nameflg && player.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
                //if (!player.Player.NameText().text.Contains(Satsumaimo) && player.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
                {//名前に(C)をつける
                    SetNamesClass.SetPlayerNameText(player.Player, player.Player.NameText().text + WhiteSatsumaimo);
                }
                if (SetNamesClass.CachedPlayersState[player.Player.PlayerId].nameflg && player.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
                //if (!player.Player.NameText().text.Contains(RedSatsumaimo) && player.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
                {
                    SetNamesClass.SetPlayerNameText(player.Player, player.Player.NameText().text + RedSatsumaimo);
                }
            }
        }
        else if (PlayerControl.LocalPlayer.IsRole(RoleId.SatsumaAndImo))
        {
            PlayerControl player = PlayerControl.LocalPlayer;
            SatsumaAndImo satsumaAndImo = RoleBaseManager.GetLocalRoleBase<SatsumaAndImo>();
            if (satsumaAndImo == null)
                return;
            if (SetNamesClass.CachedPlayersState[player.PlayerId].nameflg && satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
            //if (!player.NameText().text.Contains(WhiteSatsumaimo) && satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
            {//名前に(C)をつける
                SetNamesClass.SetPlayerNameText(player, player.NameText().text + WhiteSatsumaimo);
            }
            else if (SetNamesClass.CachedPlayersState[player.PlayerId].nameflg && satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
            //else if (!player.NameText().text.Contains(RedSatsumaimo) && satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
            {
                SetNamesClass.SetPlayerNameText(player, player.NameText().text + RedSatsumaimo);
            }
        }
    }
    public static string PartTimerSuffix = ModHelpers.GetCs(RoleClass.PartTimer.color, "◀");
}
public class CachedState
{
    public PlayerControl player;
    public RoleId role = RoleId.None;
    public bool isHide;
    public bool isAlive;
    public bool isLovers;
    public bool isFakeLovers;
    public bool isFakeLoversFake;
    public bool isArsonistDoused;
    public bool isDemon;
    public bool isQuarreled;
    public bool isMoira;
    public bool isJumbo;
    public bool isCelebrity;
    public bool isSatsumaimo;
    public SatsumaAndImo.SatsumaTeam SatsumaimoState;
    public bool isShapeshifting;
    public bool isPartTimered;
    public bool isStefinderKill;
    public byte PartTimerTarget;
    public PlayerOutfitType outfit;
    public bool isFinderCheck;
    public Tuple<int, int> tasks;
    public bool isCognitiveDeficit;
    public bool isMeeting;
    public string defaultName = "";
    public string cachedName = "";
    public string cachedInfo = "";
    public string cachedMeetingInfo = "";
    public Color nameColor = Color.white;
    public Color roleColor = Color.white;
    public bool nameflg = false;
    public bool infoflg = false;

    //とりあえず変化したかだけ確認、実際に描画するかは別で判定
    public void CheckChangePlayerState()
    {

        bool check = false;

        check = ModHelpers.HidePlayerName(PlayerControl.LocalPlayer, player);
        if (isHide != check)
        {
            isHide = check;
            defaultName = check ? "" : player.CurrentOutfit.PlayerName;
            nameflg = true;
            infoflg = true;
        }
        if (isMeeting != MeetingHud.Instance)
        {
            isMeeting = MeetingHud.Instance;
            nameflg = true;
            infoflg = true;
        }
        check = player.IsAlive();
        if (isAlive != check)
        {
            isAlive = check;
            nameflg = true;
            infoflg = true;
        }
        if (role != player.GetRole())
        {
            role = player.GetRole();
            nameflg = true;
            infoflg = true;
        }
        check = player.shapeshifting;
        if (isShapeshifting != check)
        {
            isShapeshifting = check;
            nameflg = true;
            infoflg = true;
        }
        if (outfit != player.CurrentOutfitType)
        {
            outfit = player.CurrentOutfitType;
            nameflg = true;
            infoflg = true;
        }
        Tuple<int, int> tmp;
        if (player.Data != null)
        {
            tmp = TaskCount.TaskDateNoClearCheck(player.Data);
            if (tasks != tmp)
            {
                tasks = tmp;
                infoflg = true;
            }
        }
        check = player.IsLovers();
        if (isLovers != check)
        {
            isLovers = check;
            nameflg = true;
        }
        check = player.IsFakeLovers();
        if (isFakeLovers != check)
        {
            isFakeLovers = check;
            nameflg = true;
        }
        check = player.IsFakeLoversFake();
        if (isFakeLoversFake != check)
        {
            isFakeLoversFake = check;
            nameflg = true;
        }
        check = Arsonist.IsViewIcon(player);
        if (isArsonistDoused != check)
        {
            isArsonistDoused = check;
            nameflg = true;
        }
        check = Demon.IsViewIcon(player);
        if (isDemon != check)
        {
            isDemon = check;
            nameflg = true;
        }
        check = player.IsQuarreled();
        if (isQuarreled != check)
        {
            isQuarreled = check;
            nameflg = true;
        }
        check = role == RoleId.Moira && Moira.AbilityUsedUp && !Moira.AbilityUsedThisMeeting;
        if (isMoira != check)
        {
            isMoira = check;
            nameflg = true;
        }
        check = RoleClass.Jumbo.JumboSize.ContainsKey(player.PlayerId);
        if (isJumbo != check)
        {
            isJumbo = check;
            nameflg = true;
        }
        check = RoleBaseManager.TryGetRoleBase<SatsumaAndImo>(player, out SatsumaAndImo satsumaimo);
        if (isSatsumaimo != check)
        {
            isSatsumaimo = check;
            nameflg = true;
        }
        if (check && SatsumaimoState != satsumaimo.TeamState)
        {
            SatsumaimoState = satsumaimo.TeamState;
            nameflg = true;
        }
        check = (RoleClass.Celebrity.ChangeRoleView ? RoleClass.Celebrity.ViewPlayers : RoleClass.Celebrity.CelebrityPlayer).Contains(player);
        if (isCelebrity != check)
        {
            isCelebrity = check;
            nameflg = true;
        }
        check = RoleClass.Stefinder.IsKill;
        if (isStefinderKill != check)
        {
            isStefinderKill = check;
            nameflg = true;
        }
        //PlayerControl pc = RoleClass.PartTimer.Data.GetPCByValue(player.PlayerId);
        //check = pc != null;
        //if (isPartTimered != check)
        //{
        //    isPartTimered = check;

        //    if (check)
        //    {
        //        PartTimerTarget = pc.PlayerId;
        //        SetNamesClass.CachedPlayersState[PartTimerTarget].infoflg = true;
        //        SetNamesClass.CachedPlayersState[PartTimerTarget].nameflg = true;
        //    } else
        //    {
        //        SetNamesClass.CachedPlayersState[PartTimerTarget].infoflg = true;
        //        SetNamesClass.CachedPlayersState[PartTimerTarget].nameflg = true;
        //        PartTimerTarget = 255;
        //    }
        //}
        return;
    }

    public void Reset()
    {
        nameColor = Color.white;
        roleColor = Color.white;

        bool hidename = ModHelpers.HidePlayerName(PlayerControl.LocalPlayer, player);
        defaultName = hidename ? "" : player.CurrentOutfit.PlayerName;
        cachedInfo = "";
        cachedMeetingInfo = "";
        cachedName = "";
        if ((PlayerControl.LocalPlayer.IsImpostor() && (player.IsImpostor() || player.IsRole(RoleId.Spy, RoleId.Egoist))) || (ModeHandler.IsMode(ModeId.HideAndSeek) && player.IsImpostor()))
        {
            nameColor = RoleClass.ImpostorRed;
            roleColor = RoleClass.ImpostorRed;
        }
        else
        {
            nameColor = Color.white;
            roleColor = Color.white;
        }

        nameflg = true;
        infoflg = true;
    }
    public void FinishApply()
    {
        nameflg = false;
        infoflg = false;
    }
    public CachedState(PlayerControl p)
    {
        player = p;
        Reset();
    }
}

public class SetNameUpdate
{
    private static Stopwatch sw = new();
    public static void Postfix(PlayerControl __instance)
    {
        sw.Start();
        //role, alive, loversなど, Camoflager/MatrosivkaなどでPlayernameは変化する
        //role, Task数に変化なければPlayerInfoに変化なし
        //sabotage次第では名前が見えなくなる
        //ShapeShift、Doppergengarなどでも変化あるのでは？(Shapeshifting)
        bool playerNameChangeflg;
        bool playerInfoChangeflg;
        RoleId LocalRole = PlayerControl.LocalPlayer.GetRole();
        bool CanSeeAllRole =
            SetNamesClass.CanGhostSeeRoles() ||
            Debugger.canSeeRole ||
            (PlayerControl.LocalPlayer.GetRoleBase() is INameHandler nameHandler &&
            nameHandler.AmAllRoleVisible);
        if (SetNamesClass.CachedPlayersState == null)
        {
            SetNamesClass.CachedPlayersState = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                SetNamesClass.CachedPlayersState[player.PlayerId] = new CachedState(player);
            }
        }

        checkDisablePartTimer();
        checkDisableCognitiveDeficit();
        bool changeflg = false;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            SetNamesClass.CachedPlayersState[player.PlayerId].CheckChangePlayerState();
            if (changeflg) continue;
            if (SetNamesClass.CachedPlayersState[player.PlayerId].infoflg || SetNamesClass.CachedPlayersState[player.PlayerId].nameflg) changeflg = true;
        }
        if (!changeflg) return;

        //SetNamesClass.ResetNameTagsAndColors();

        if (CanSeeAllRole)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (SetNamesClass.CachedPlayersState[player.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(player);
                if (SetNamesClass.CachedPlayersState[player.PlayerId].infoflg) SetNamesClass.SetPlayerRoleNames(player);
            }
        }
        //TODO:神移行時にINameHandlerに移行する
        else if (LocalRole == RoleId.God)
        {
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (RoleClass.IsMeeting || player.IsAlive())
                {
                    if (SetNamesClass.CachedPlayersState[player.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(player);
                    if (SetNamesClass.CachedPlayersState[player.PlayerId].infoflg) SetNamesClass.SetPlayerRoleNames(player);
                }
            }
        }
        else
        {
            if (Madmate.CheckImpostor(PlayerControl.LocalPlayer) ||
                LocalRole == RoleId.MadKiller ||
                LocalRole == RoleId.Marlin ||
                (RoleClass.Demon.IsCheckImpostor && LocalRole == RoleId.Demon) ||
                (LocalRole == RoleId.Safecracker && Safecracker.CheckTask(__instance, Safecracker.CheckTasks.CheckImpostor)))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsImpostorAddedFake())
                    {
                        SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                    }
                }
            }
            switch (LocalRole)
            {
                case RoleId.Finder:
                    if (RoleClass.Finder.IsCheck)
                    {
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.IsMadRoles())
                            {
                                if (RoleClass.Finder.IsCheck != SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].isFinderCheck || SetNamesClass.CachedPlayersState[player.PlayerId].nameflg)
                                    SetNamesClass.SetPlayerNameColor(player, Color.red);
                            }
                        }
                        SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].isFinderCheck = true;
                    }
                    break;
                case RoleId.Dependents:
                    foreach (PlayerControl p in RoleClass.Vampire.VampirePlayer)
                    {
                        if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(p);
                    }
                    break;
                case RoleId.Vampire:
                    foreach (PlayerControl p in RoleClass.Dependents.DependentsPlayer)
                    {
                        if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(p);
                    }
                    break;
                case RoleId.PartTimer:
                    if (RoleClass.PartTimer.IsLocalOn)
                    {
                        if (CustomOptionHolder.PartTimerIsCheckTargetRole.GetBool())
                        {
                            if (SetNamesClass.CachedPlayersState[RoleClass.PartTimer.CurrentTarget.PlayerId].infoflg) SetNamesClass.SetPlayerRoleNames(RoleClass.PartTimer.CurrentTarget);
                            if (SetNamesClass.CachedPlayersState[RoleClass.PartTimer.CurrentTarget.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(RoleClass.PartTimer.CurrentTarget);
                        }
                        else
                        {
                            if (SetNamesClass.CachedPlayersState[RoleClass.PartTimer.CurrentTarget.PlayerId].nameflg) SetNamesClass.SetPlayerNameText(RoleClass.PartTimer.CurrentTarget, RoleClass.PartTimer.CurrentTarget.NameText().text + SetNamesClass.PartTimerSuffix);
                        }
                    }
                    break;
                case RoleId.Fox:
                case RoleId.FireFox:
                    List<PlayerControl> foxs = new(RoleClass.Fox.FoxPlayer);
                    foxs.AddRange(FireFox.FireFoxPlayer);
                    foreach (PlayerControl p in foxs)
                    {
                        if (FireFox.FireFoxIsCheckFox.GetBool() || p.IsRole(PlayerControl.LocalPlayer.GetRole()))
                        {
                            if (SetNamesClass.CachedPlayersState[p.PlayerId].infoflg) SetNamesClass.SetPlayerRoleNames(p);
                            if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(p);
                        }
                    }
                    break;
                case RoleId.TheFirstLittlePig:
                case RoleId.TheSecondLittlePig:
                case RoleId.TheThirdLittlePig:
                    foreach (var players in TheThreeLittlePigs.TheThreeLittlePigsPlayer)
                    {
                        if (!players.Contains(PlayerControl.LocalPlayer)) continue;
                        foreach (PlayerControl p in players)
                        {
                            if (SetNamesClass.CachedPlayersState[p.PlayerId].infoflg) SetNamesClass.SetPlayerRoleNames(p);
                            if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(p);
                        }
                        break;
                    }
                    break;
                case RoleId.OrientalShaman:
                    if (OrientalShaman.OrientalShamanCausative.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out byte value))
                    {
                        if (SetNamesClass.CachedPlayersState[value].infoflg) SetNamesClass.SetPlayerRoleNames(ModHelpers.PlayerById(value));
                        if (SetNamesClass.CachedPlayersState[value].nameflg) SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(value));
                    }
                    //foreach (var date in OrientalShaman.OrientalShamanCausative)
                    //{
                    //    if (date.Key != PlayerControl.LocalPlayer.PlayerId) continue;
                    //    SetNamesClass.SetPlayerRoleNames(ModHelpers.PlayerById(date.Value));
                    //    SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(date.Value));
                    //}
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (SetNamesClass.CachedPlayersState[player.PlayerId].nameflg && OrientalShaman.IsKiller(player))
                            SetNamesClass.SetPlayerNameColors(player);
                    }
                    break;
                case RoleId.ShermansServant:
                    foreach (var date in OrientalShaman.OrientalShamanCausative)
                    {
                        if (date.Value != PlayerControl.LocalPlayer.PlayerId) continue;
                        if (SetNamesClass.CachedPlayersState[date.Key].infoflg) SetNamesClass.SetPlayerRoleNames(ModHelpers.PlayerById(date.Key));
                        if (SetNamesClass.CachedPlayersState[date.Key].nameflg) SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(date.Key));
                    }
                    break;
                case RoleId.Pokerface:
                    Pokerface.PokerfaceTeam team = Pokerface.GetPokerfaceTeam(PlayerControl.LocalPlayer.PlayerId);
                    if (team != null)
                    {
                        foreach (PlayerControl member in team.TeamPlayers)
                        {
                            if (SetNamesClass.CachedPlayersState[member.PlayerId].nameflg) SetNamesClass.SetPlayerNameColor(member, Pokerface.RoleData.color);
                        }
                    }
                    break;
            }
            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
                {
                    if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                }
            }
            if (PlayerControl.LocalPlayer.IsJackalTeam() || JackalFriends.CheckJackal(PlayerControl.LocalPlayer))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    RoleId role = p.GetRole();
                    if ((p.IsJackalTeamJackal() || p.IsJackalTeamSidekick()) && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        if (SetNamesClass.CachedPlayersState[p.PlayerId].infoflg) SetNamesClass.SetPlayerRoleNames(p);
                        if (SetNamesClass.CachedPlayersState[p.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(p);
                    }
                }
            }
            if (SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].infoflg) SetNamesClass.SetPlayerRoleNames(PlayerControl.LocalPlayer);
            if (SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].nameflg) SetNamesClass.SetPlayerNameColors(PlayerControl.LocalPlayer);
        }
        //ここまだ
        CustomRoles.NameHandler(CanSeeAllRole);
        //名前の奴
        if (RoleClass.Camouflager.IsCamouflage)
        {
            if (RoleClass.Camouflager.ArsonistMark)
                SetNamesClass.ArsonistSet();
            if (RoleClass.Camouflager.DemonMark)
                SetNamesClass.DemonSet();
            if (RoleClass.Camouflager.LoversMark)
                SetNamesClass.LoversSet();
            if (RoleClass.Camouflager.QuarreledMark)
                SetNamesClass.QuarreledSet();
        }
        else
        {
            Pavlovsdogs.SetNameUpdate();
            SetNamesClass.ArsonistSet();
            SetNamesClass.DemonSet();
            SetNamesClass.CelebritySet();
            SetNamesClass.QuarreledSet();
            SetNamesClass.LoversSet();
            SetNamesClass.MoiraSet();
        }
        SetNamesClass.SatsumaimoSet();
        SetNamesClass.JumboSet();

        checkEnablePartTimer();
        //if (RoleClass.Stefinder.IsKill)
        //{
        //    if (SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].nameflg) SetNamesClass.SetPlayerNameColor(PlayerControl.LocalPlayer, Color.red);
        //}

        checkEnableCognitiveDeficit();
        //if (ModeHandler.IsMode(ModeId.Default))
        //{
        //    if (Sabotage.SabotageManager.thisSabotage == Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit)
        //    {
        //        foreach (PlayerControl p3 in CachedPlayer.AllPlayers)
        //        {
        //            if (p3.IsAlive() && !Sabotage.CognitiveDeficit.Main.OKPlayers.IsCheckListPlayerControl(p3))
        //            {
        //                if (PlayerControl.LocalPlayer.IsImpostor())
        //                {
        //                    if (!(p3.IsImpostor() || p3.IsRole(RoleId.MadKiller)))
        //                    {
        //                        SetNamesClass.SetPlayerNameColor(p3, new Color32(18, 112, 214, byte.MaxValue));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        sw.Stop();
        Logger.Info(((double)1000 * sw.ElapsedTicks / Stopwatch.Frequency).ToString(), "SetNames");
        sw.Reset();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            //flgたってない場合はいらないかも？
            SetNamesClass.ApplyPlayerRoleInfoView(player);
            SetNamesClass.ApplyPlayerNameColor(player);
            SetNamesClass.ApplyPlayerNameText(player);
            SetNamesClass.CachedPlayersState[player.PlayerId].FinishApply();
        }
    }

    private static void checkEnablePartTimer()
    {
        PlayerControl PartTimerTarget = null;
        try
        {
            PartTimerTarget = RoleClass.PartTimer.Data.GetPCByValue(PlayerControl.LocalPlayer.PlayerId);
        }
        catch { }
        bool check = PartTimerTarget != null;
        if (SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].isPartTimered != check)
        {
            SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].isPartTimered = check;
            SetNamesClass.SetPlayerRoleNames(PartTimerTarget);
            SetNamesClass.SetPlayerNameColors(PartTimerTarget);
            SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].PartTimerTarget = PartTimerTarget.PlayerId;
        }
    }
    private static void checkDisablePartTimer()
    {
        PlayerControl PartTimerTarget = null;
        try
        {
            PartTimerTarget = RoleClass.PartTimer.Data.GetPCByValue(PlayerControl.LocalPlayer.PlayerId);
        }
        catch { }
        bool check = PartTimerTarget != null;
        if (SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].isPartTimered != check)
        {
            //これだとcheckEnablePartTimerに絶対に引っかからないのでは？
            SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].isPartTimered = check;
            if (!check) SetNamesClass.ResetNameTagsAndColors(ModHelpers.GetPlayerControl(SetNamesClass.CachedPlayersState[PlayerControl.LocalPlayer.PlayerId].PartTimerTarget));
        }
    }

    private static void checkEnableCognitiveDeficit()
    {
        if (ModeHandler.IsMode(ModeId.Default))
        {
            if (Sabotage.SabotageManager.thisSabotage == Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit)
            {
                foreach (PlayerControl p3 in CachedPlayer.AllPlayers)
                {
                    if (p3.IsAlive() && !Sabotage.CognitiveDeficit.Main.OKPlayers.IsCheckListPlayerControl(p3))
                    {
                        if (PlayerControl.LocalPlayer.IsImpostor())
                        {
                            if (!(p3.IsImpostor() || p3.IsRole(RoleId.MadKiller)))
                            {
                                bool check = SetNamesClass.CachedPlayersState[p3.PlayerId].isCognitiveDeficit == false;
                                if (check)
                                {
                                    SetNamesClass.SetPlayerNameColor(p3, new Color32(18, 112, 214, byte.MaxValue));
                                    SetNamesClass.CachedPlayersState[p3.PlayerId].isCognitiveDeficit = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void checkDisableCognitiveDeficit()
    {
        if (ModeHandler.IsMode(ModeId.Default))
        {
            if (Sabotage.SabotageManager.thisSabotage != Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit)
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    bool check = SetNamesClass.CachedPlayersState[p.PlayerId].isCognitiveDeficit == true;
                    if (check)
                    {
                        SetNamesClass.ResetNameTagsAndColors(p);
                        SetNamesClass.CachedPlayersState[p.PlayerId].isCognitiveDeficit = false;
                    }
                }
            }
        }
    }
}