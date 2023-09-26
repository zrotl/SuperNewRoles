using System;
using UnityEngine;

namespace SuperNewRoles.CustomObject;
public class RocketDeadbody : CustomAnimation
{
    public RocketDeadbody(IntPtr intPtr) : base(intPtr)
    {
    }
    private Vector3 BasePos;
    private static Vector3 movepos = new(0,0.1f,0);
    private static float FireworksSize = 2;
    private bool IsFirework;
    public override void Awake()
    {
        base.Awake();
        Logger.Info("Awaked");
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        transform.localScale = Vector3.one * 0.45f;
    }
    public void Init(PlayerControl Player)
    {
        CustomAnimationOptions customAnimationOptions = new(GetSprites("SuperNewRoles.Resources.Rocket.RocketPlayer", 2), 10, true);
        base.Init(customAnimationOptions);
        //カラーを変更する
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
        PlayerMaterial.Properties Properties = new()
        {
            MaskLayer = 0,
            MaskType = PlayerMaterial.MaskType.None,
            ColorId = Player.Data.DefaultOutfit.ColorId
        };
        spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);
        transform.position = new(Player.transform.position.x, Player.transform.position.y, -10);
        BasePos = transform.position;
        IsFirework = false;
    }
    public override void Update()
    {
        if (!IsFirework)
        {
            if (Playing)
            {
                transform.position += movepos;
                if ((transform.position - BasePos).y > 6f)
                {
                    IsFirework = true;
                    Options.SetSprites(GetSprites("SuperNewRoles.Resources.Rocket.Fireworks.fireworks_", 20,2), IsLoop:false, frameRate: 30);
                    Options.SetPlayEndDestroy(true);
                    transform.localScale = Vector3.one * FireworksSize;
                    spriteRenderer.sprite = Options.Sprites.FirstOrDefault();
                    return;
                }
            }
            else if (IsRewinding)
            {
                transform.position -= movepos;
            }
        }
        base.Update();
    }
}