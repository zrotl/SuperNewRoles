using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Mode.PantsRoyal;
public static class UpdateHandler
{
    public static void WaitSpawnUpdater()
    {
        bool IsMoveOK = true;
        List<PlayerControl> players = new();
        int NotLoadedCount = 0;
        if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
        {
            foreach (CachedPlayer p in CachedPlayer.AllPlayers)
            {
                if (ModHelpers.IsPositionDistance(p.transform.position, new Vector2(3, 6), 0.5f) ||
                    ModHelpers.IsPositionDistance(p.transform.position, new Vector2(-25, 40), 0.5f) ||
                    ModHelpers.IsPositionDistance(p.transform.position, new Vector2(-1.4f, 2.3f), 0.5f)
                    )
                {
                    IsMoveOK = false;
                    NotLoadedCount++;
                }
                else
                {
                    players.Add(p.PlayerControl);
                }
            }
        }
        if (main.LastCount != players.Count)
        {
            main.LastCount = players.Count;
            StringBuilder name = new();
            name.Append("\n\n\n\n\n\n\n\n<size=300%><color=white>");
            name.Append(ModeHandler.PlayingOnSuperNewRoles);
            name.Append("</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%><color=white>");
            name.AppendFormat(ModTranslation.GetString("CopsSpawnLoading"), NotLoadedCount);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.AmOwner)
                {
                    p.RpcSetNamePrivate(name.ToString());
                }
                else
                {
                    p.SetName(name.ToString());
                }
            }
        }

        int i = 0;
        foreach (PlayerControl p in players)
        {
            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
            {
                p.RpcSnapTo(new Vector2(-30, 30));
                i++;
            }
        }
        if (IsMoveOK)
        {
            main.IsMove = true;
            main.ShowRoleTime = 6;
            main.LastUpdateTime = main.ShowRoleTime + 1;
        }
        return;
    }
    public static void ShowRoleUpdate()
    {
        main.ShowRoleTime -= Time.deltaTime;
        SuperNewRolesPlugin.Logger.LogInfo(main.ShowRoleTime - main.LastUpdateTime);
        if (main.LastUpdateTime - main.ShowRoleTime >= 1)
        {
            //string name = "\n\n\n\n\n<size=300%><color=white>" + SuperNewRoles.Mode.ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%>インポスターが来るまで残り5秒</size>";
            //PlayerControl.LocalPlayer.RpcSetName(name);
            main.LastUpdateTime = main.ShowRoleTime;
            string RoleText = "";
            StringBuilder name = new();
            name.Append("\n\n\n\n\n<size=300%><color=white>{ROLETEXT}</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%>");
            name.Append(ModTranslation.GetString("PantsRoyalStartAt"));
            name.Append(((int)(main.LastUpdateTime + 1)).ToString());
            name.Append(ModTranslation.GetString("second"));
            name.Append("</size>");
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (main.IsPantsHaver(p))
                    RoleText = ModTranslation.GetString("PantsRoyalPantsHaverIntroName");
                else
                    RoleText = ModTranslation.GetString("PantsRoyalPantsDontHaverIntroName");
                string targetnametext = name.Replace("{ROLETEXT}", RoleText).ToString();
                if (!p.AmOwner)
                {
                    p.RpcSetNamePrivate(targetnametext);
                }
                else
                {
                    p.SetName(targetnametext);
                }
            }
        }
        int i = 0;
        foreach (CachedPlayer p in CachedPlayer.AllPlayers)
        {
            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
            {
                p.PlayerControl.RpcSnapTo(new Vector2(-30, 30));
                i++;
            }
        }
        if (main.ShowRoleTime <= 0)
        {
            StringBuilder RoleNameText = ModHelpers.Csb(IntroData.CrewmateIntro.color, IntroData.CrewmateIntro.Name);
            StringBuilder TaskText = ModHelpers.Csb(Color.yellow, "(334/802)");
            StringBuilder NewName;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                CopsRobbers.Main.ClearAndReloads();
                NewName = new();
                NewName.Append("<size=75%>");
                NewName.Append(RoleNameText);
                NewName.Append(TaskText);
                NewName.Append("</size>\n");
                NewName.Append(p.GetDefaultName());
                p.RpcSetName(NewName.ToString());
                //if (CopsRobbersOptions.CRHideName.GetBool() && CopsRobbersOptions.CopsRobbersMode.GetBool()) ModeHandler.HideName();
                if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == (byte)MapNames.Airship)
                {
                    p.RpcSnapTo(main.GetRandomAirshipPosition());
                }
                else
                {
                    Vector2 up = Vector2.up;
                    up = up.Rotate((float)(p.PlayerId - 1) * (360f / (float)PlayerControl.AllPlayerControls.Count));
                    up *= ShipStatus.Instance.SpawnRadius;
                    Vector2 position = ShipStatus.Instance.MeetingSpawnCenter + up + new Vector2(0f, 0.3636f);
                    p.RpcSnapTo(position);
                }
            }
        }
        return;
    }
    public static void TurnTimer()
    {
        main.CurrentTurnData.TurnTimer -= Time.deltaTime;
        if (main.CurrentTurnData.TurnTimer <= 0)
            main.CurrentTurnData.EndTurn();
        else if (main.CurrentTurnData.EndPlayerCount >= ModHelpers.GetAlivePlayerCount())
            main.CurrentTurnData.EndTurn();
        else if (1 >= ModHelpers.GetAlivePlayerCount())
            main.CurrentTurnData.EndTurn();
        else
        {
            StringBuilder RoleNameText = ModHelpers.Csb(IntroData.CrewmateIntro.color, IntroData.CrewmateIntro.Name);
            StringBuilder TaskText = ModHelpers.Csb(Color.yellow, "(334/802)");
            if (main.CurrentTurnData.TurnTimer <= 10)
            {
                if (main.CurrentTurnData.LastUpdateCountdownTime >= 1)
                {
                    int time = (int)main.CurrentTurnData.TurnTimer;
                    // 元の値が整数ならそのまま返す
                    if (main.CurrentTurnData.TurnTimer != time)
                        time++;
                    StringBuilder timetext = new();
                    timetext.Append(ModHelpers.CsHead(IntroData.CrewmateIntro.color));
                    timetext.AppendFormat(ModTranslation.GetString("PantsRoyalTimeRemainingText"), time);
                    timetext.Append("</color>");
                    StringBuilder prefix = new();
                    prefix.Append("<size=125%>");
                    prefix.Append(timetext);
                    prefix.Append("</size>\n<size=75%>");
                    prefix.Append(RoleNameText);
                    prefix.Append(TaskText);
                    prefix.Append("</size>\n");
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        StringBuilder nametext = new();
                        nametext.Append(prefix);
                        nametext.Append(p.GetDefaultName());
                        nametext.Append("\n\n");
                        if (!p.AmOwner)
                        {
                            p.RpcSetNamePrivate(nametext.ToString());
                        }
                        else
                        {
                            p.SetName(nametext.ToString());
                        }
                    }
                    main.CurrentTurnData.LastUpdateCountdownTime = 0;
                }
                else
                {
                    main.CurrentTurnData.LastUpdateCountdownTime += Time.deltaTime;
                }
            }
            else if (10 <= main.CurrentTurnData.LastUpdateCountdownTime || (main.CurrentTurnData.LastUpdateCountdownTime >= 1 && (main.CurrentTurnData.TurnTimer % 10) >= 9.5f))
            {
                int time = (int)main.CurrentTurnData.TurnTimer;
                // 元の値が整数ならそのまま返す
                if (main.CurrentTurnData.TurnTimer != time)
                    time++;
                StringBuilder timetext = new();
                timetext.Append(ModHelpers.CsHead(IntroData.CrewmateIntro.color));
                timetext.AppendFormat(ModTranslation.GetString("PantsRoyalTimeRemainingText"), time);
                timetext.Append("</color>");
                StringBuilder prefix = new();
                prefix.Append("<size=125%>");
                prefix.Append(timetext);
                prefix.Append("</size>\n<size=75%>");
                prefix.Append(RoleNameText);
                prefix.Append(TaskText);
                prefix.Append("</size>\n");
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    StringBuilder nametext = new();
                    nametext.Append(prefix);
                    nametext.Append(p.GetDefaultName());
                    nametext.Append("\n\n");
                    if (!p.AmOwner)
                    {
                        p.RpcSetNamePrivate(nametext.ToString());
                    }
                    else
                    {
                        p.SetName(nametext.ToString());
                    }
                }
                main.CurrentTurnData.LastUpdateCountdownTime = 0;
                main.CurrentTurnData.CooldowntextReseted = false;
            }
            else if (1 <= main.CurrentTurnData.LastUpdateCountdownTime && !main.CurrentTurnData.CooldowntextReseted)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    StringBuilder nametext = new();
                    nametext.Append("<size=75%>");
                    nametext.Append(RoleNameText);
                    nametext.Append(TaskText);
                    nametext.Append("</size>\n");
                    nametext.Append(p.GetDefaultName());
                    if (!p.AmOwner)
                    {
                        p.RpcSetNamePrivate(nametext.ToString());
                    }
                    else
                    {
                        p.SetName(nametext.ToString());
                    }
                }
                main.CurrentTurnData.CooldowntextReseted = true;
            }
            else
            {
                main.CurrentTurnData.LastUpdateCountdownTime += Time.deltaTime;
            }
        }
    }
    public static void TurnStartWait()
    {
        main.CurrentTurnData.StartTimer -= Time.deltaTime;
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            p.RpcSnapTo(new Vector2(-30, 30));
        }
        if (main.CurrentTurnData.LastUpdateStartTimer - main.CurrentTurnData.StartTimer >= 1)
        {
            main.CurrentTurnData.StartTimer -= Time.deltaTime;
            string RoleText = "";
            StringBuilder name = new();
            name.Append("\n\n\n\n\n<size=300%><color=white>{ROLETEXT}</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%>");
            name.Append(ModTranslation.GetString("PantsRoyalStartAt"));
            name.Append(((int)(main.CurrentTurnData.LastUpdateStartTimer + 1)).ToString());
            name.Append(ModTranslation.GetString("second"));
            name.Append("</size>");
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.IsDead())
                    RoleText = ModTranslation.GetString("PantsRoyalDeadName");
                else if (main.IsPantsHaver(p))
                    RoleText = ModTranslation.GetString("PantsRoyalPantsHaverIntroName");
                else
                    RoleText = ModTranslation.GetString("PantsRoyalPantsDontHaverIntroName");
                string targetnametext = name.Replace("{ROLETEXT}", RoleText).ToString();
                if (!p.AmOwner)
                {
                    p.RpcSetNamePrivate(targetnametext);
                }
                else
                {
                    p.SetName(targetnametext);
                }
            }
            main.CurrentTurnData.LastUpdateStartTimer = main.CurrentTurnData.StartTimer;
        }
        if (main.CurrentTurnData.StartTimer <= 0)
        {
            StringBuilder RoleNameText = ModHelpers.Csb(IntroData.CrewmateIntro.color, IntroData.CrewmateIntro.Name);
            StringBuilder TaskText = ModHelpers.Csb(Color.yellow, "(334/802)");
            StringBuilder NewName;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                NewName = new();
                NewName.Append("<size=75%>");
                NewName.Append(RoleNameText);
                NewName.Append(TaskText);
                NewName.Append("</size>\n");
                NewName.Append(p.GetDefaultName());
                p.RpcSetName(NewName.ToString());
                //if (CopsRobbersOptions.CRHideName.GetBool() && CopsRobbersOptions.CopsRobbersMode.GetBool()) ModeHandler.HideName();
                if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == (byte)MapNames.Airship)
                {
                    p.RpcSnapTo(CopsRobbers.Main.GetPosition(CopsRobbers.Main.GetRandomSpawnPosition(p)));
                }
                else
                {
                    Vector2 up = Vector2.up;
                    up = up.Rotate((float)(p.PlayerId - 1) * (360f / (float)PlayerControl.AllPlayerControls.Count));
                    up *= ShipStatus.Instance.SpawnRadius;
                    Vector2 position = ShipStatus.Instance.MeetingSpawnCenter + up + new Vector2(0f, 0.3636f);
                    p.RpcSnapTo(position);
                }
            }
            main.CurrentTurnData.IsStarted = true;
        }
    }
    public static void HudUpdate()
    {
        if (!main.IsStart)
            return;
        else if (!main.IsMove)
            WaitSpawnUpdater();
        else if (main.ShowRoleTime >= 0)
            ShowRoleUpdate();
        else if (main.CurrentTurnData == null)
            return;
        else if (!main.CurrentTurnData.IsStarted)
            TurnStartWait();
        else if (main.CurrentTurnData.TurnTimer > 0)
            TurnTimer();
        else
            Logger.Info("しょりが なかった！");
    }
}