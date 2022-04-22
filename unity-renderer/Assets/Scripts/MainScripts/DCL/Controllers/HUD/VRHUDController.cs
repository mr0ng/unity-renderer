using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Huds
{
    public class VRHUDController : MonoBehaviour
    {
        public static Action LoadingStart;
        public static Action LoadingEnd;
        public static VRHUDController I { get; private set; }
        private HUDController controller => HUDController.i;
        private readonly List<VRHUDHelper> huds = new List<VRHUDHelper>();
        BaseVariable<bool> exploreV2IsOpen => DataStore.i.exploreV2.isOpen;

        [SerializeField]
        private GameObject visuals;
        private Transform mainCam;
        
        private void Awake()
        {
            if (I != null)
            {
                Destroy(this);
                return;
            }
            I = this;
            if (Camera.main != null)
                mainCam = Camera.main.transform;
            LoadingStart += HideVisuals;
            LoadingEnd += ShowVisuals;
        }
        private void ShowVisuals() => visuals.SetActive(true);
        private void HideVisuals() => visuals.SetActive(false);

        public static void RaiseLoadingStart()
        {
            LoadingStart?.Invoke();
        }

        public static void RaiseLoadingEnd()
        {
            LoadingEnd?.Invoke();
        }

        public void Register(VRHUDHelper helper) => huds.Add(helper);

        public void ActivateHud()
        {
            for (int i = 0; i < huds.Count; i++)
            {
                var hud = huds[i];
                hud.ShowHud(mainCam);
            }
        }

        public void DeactivateHud()
        {
            for (int i = 0; i < huds.Count; i++)
            {
                huds[i].HidHud();
            }
        }
    }
}
