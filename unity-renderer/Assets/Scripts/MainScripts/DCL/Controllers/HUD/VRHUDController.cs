using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Huds
{
    public class VRHUDController : MonoBehaviour
    {
        public static VRHUDController I { get; private set; }
        private HUDController controller => HUDController.i;
        private readonly Dictionary<HUDElementID, VRHUDHelper> huds = new Dictionary<HUDElementID, VRHUDHelper>();
        BaseVariable<bool> exploreV2IsOpen => DataStore.i.exploreV2.isOpen;

        private void Awake()
        {
            if (I != null) Destroy(this);
            I = this;
        }
        

        public void Register(HUDElementID id, VRHUDHelper helper) => huds.Add(id, helper);

        public void ActivateHud(HUDElementID id)
        {
            if (huds.TryGetValue(id, out VRHUDHelper helper))
            {
                helper.ActivateHud();
            }
        }
    }
}
