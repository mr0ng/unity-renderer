using System.Collections;
using System.Collections.Generic;
using DCL.Huds;
using UnityEngine;

public class ProfileHudHelper : VRHUDHelper
{
    [SerializeField]
    private IProfileHUDView view;


    protected override void SetupHelper()
    {
        VRHUDController.I.Register(this, true);
        VRHUDController.I.Reparent(myTrans);
    }

    public void Start()
    {
        base.Start();
        view = gameObject.GetComponent<IProfileHUDView>();
    }
    public override void Hide()
    {
        view.SetStartMenuButtonActive(false);
        view.ShowExpanded(true);
    }

    public override void Show()
    {
        view.SetStartMenuButtonActive(true);
        view.ShowExpanded(false);
    }
}
