using DCL.VR;
using UnityEngine;

namespace DCL
{
    public class VrAvatarController : MonoBehaviour
    {
        [SerializeField]
        private PlayerAvatarController controller;
        
        private Transform cam;
        private Transform myTrans;
        
        private void Start()
        {
            myTrans = transform;
            controller.ApplyHideAvatarModifier();
            //DCLCharacterController.i.OnUpdateFinish += OnUpdateFinish;
        }
        
        private void OnUpdateFinish(float obj)
        {
            var pos = myTrans.position;
            var camPos = cam.position;
            if (pos != camPos)
                myTrans.position = cam.position;
        }
    }
}