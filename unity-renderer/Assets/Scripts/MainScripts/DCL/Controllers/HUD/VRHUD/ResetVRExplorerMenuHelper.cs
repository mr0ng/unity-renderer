using DCL.Huds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetVRExplorerMenuHelper : MonoBehaviour
{
    public void ResetMenu()
    {
        VRHUDController.I.MoveHud();
    }
}
