using DCL;
using DCL.Huds;
using DCL.LoadingScreen;
using SignupHUD;
using System;
using System.Collections;
using System.Security;
using UnityEngine;

public class LoadingHudHelper : VRHUDHelper
{
    [SerializeField]
    private LayerMask loadingMask;
    [SerializeField]
    private LayerMask signupMask;
     [SerializeField]
    private LoadingScreenView view;
    [SerializeField]
    private ShowHideAnimator animator;
    private SignupHUDView signUpScreen;
    private bool signedUp = false;
    private bool isFirstLoad = true;
    // protected void Start()
    // {
    //     CrossPlatformManager.SetCameraForLoading(signupMask);
    // }

private IEnumerator FindSignupView()
{
    if (signedUp) yield break;
    CrossPlatformManager.SetCameraForLoading(signupMask);
    while (signUpScreen == null)
    {
        yield return null;
        signUpScreen = SignupHUDView.I;
    }
    if (signUpScreen != null)
    {
        CrossPlatformManager.SetCameraForLoading(signupMask);
        signedUp = true;
        if (DataStore.i.common.isSignUpFlow.Get())
            signUpScreen.OnTermsOfServiceAgreed += OnTermsOfServiceAgreed;

    }

    yield break;
}
    protected void OnDestroy()
    {
        signUpScreen.OnTermsOfServiceAgreed -= OnTermsOfServiceAgreed;
    }

    protected override void SetupHelper()
    {
        #if !DCL_VR
        return;
        #endif
        view.showHideAnimator.OnStartHide+= ()=>{CrossPlatformManager.SetCameraForGame();};
        view.showHideAnimator.OnStartShow+= ()=>{CrossPlatformManager.SetCameraForLoading(loadingMask);};
        CrossPlatformManager.SetCameraForGame();
        // CrossPlatformManager.SetCameraForLoading(signupMask);
        //signUpScreen = SignupHUDView.I;
        StartCoroutine(FindSignupView());
        myTrans.localScale = 0.00075f * Vector3.one;
        DebugConfigComponent.i.ShowWebviewScreen();
        if (myTrans is RectTransform rect)
        {
            rect.sizeDelta = new Vector2(1920, 1080);
        }
        #if DCL_VR
        VRHUDController.I.SetupLoading(animator);
        VRHUDController.LoadingStart += () =>
        {
            if (isFirstLoad)
            {
                CrossPlatformManager.SetCameraForGame();
                isFirstLoad = false;
                return;
            }

            if (DataStore.i.common.isSignUpFlow.Get())
            {
                CrossPlatformManager.SetCameraForLoading(signupMask);
                // Hide loading screen
                animator.Hide();
                myTrans.position += 100 * Vector3.down;
                // view.SetVisible(false,true);
                // Show signup screen
                signUpScreen.SetVisibility(true);
                // Set camera for game
                //CrossPlatformManager.SetCameraForGame();
                DebugConfigComponent.i.HideWebViewScreens();
                // Set signup visibility in DataStore
                DataStore.i.HUDs.signupVisible.Set(true);

                return;
            }

            CrossPlatformManager.SetCameraForLoading(loadingMask);
            var forward = VRHUDController.I.GetForward();
            myTrans.position = Camera.main.transform.position + forward;// + Vector3.up;
            DebugConfigComponent.i.HideWebViewScreens();
            myTrans.forward = forward;
            VRHUDController.LoadingEnd += () =>
            {
                StartCoroutine(ResetLoadingScreen());
            };

        };
        VRHUDController.LoadingEnd += CrossPlatformManager.SetCameraForGame;
        #endif
    }
    private IEnumerator ResetLoadingScreen(){
        CrossPlatformManager.SetCameraForGame();
        view.FadeOut();
        view.enabled = false;
        CrossPlatformManager.SetCameraForGame();
        yield return new WaitForSeconds(0.5f);
        view.enabled = true;
        CrossPlatformManager.SetCameraForGame();
    }
    public void OnTermsOfServiceAgreed()
    {
        // Stop listening to this event
        signUpScreen.OnTermsOfServiceAgreed -= OnTermsOfServiceAgreed;
        DataStore.i.common.isSignUpFlow.Set(false);
        animator.Show();
        VRHUDController.LoadingStart.Invoke();
        CrossPlatformManager.SetCameraForLoading(loadingMask);
        // view.SetVisible(true,true);
        var forward = VRHUDController.I.GetForward();
        myTrans.position = Camera.main.transform.position + forward;
        #if DCL_VR
        DebugConfigComponent.i.HideWebViewScreens();
        #endif
        myTrans.forward = forward;
        view.FadeIn(false, false);


    }
}
