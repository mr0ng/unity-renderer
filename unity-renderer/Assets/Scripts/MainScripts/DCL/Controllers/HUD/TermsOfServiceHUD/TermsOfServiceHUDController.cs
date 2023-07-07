using DCL.Interface;
using System;

public class TermsOfServiceHUDController : IHUD
{
    [Serializable]
    public class Model
    {
        public int sceneNumber;
        public string sceneName;
        public bool adultContent;
        public bool gamblingContent;
        public string tosURL;
        public string privacyPolicyURL;
        public string emailContactURL;
    }

    internal TermsOfServiceHUDView view;
    internal Model model;

    public TermsOfServiceHUDController()
    {
        view = TermsOfServiceHUDView.CreateView();
        view.Initialize(SendAgreed, SendDeclined, OpenToS, OpenPrivacyPolicy, OpenContactEmail);
    }

    public void ShowTermsOfService(Model model)
    {
        this.model = model;

        if (this.model == null)
        {
            view.SetVisible(false);
            return;
        }

        view.SetData(model.sceneName, model.adultContent, model.gamblingContent, !string.IsNullOrEmpty(model.tosURL), !string.IsNullOrEmpty(model.privacyPolicyURL), !string.IsNullOrEmpty(model.emailContactURL));
        view.SetVisible(true);
    }

    private void SendAgreed(bool dontShowAgain)
    {
        WebInterface.SendTermsOfServiceResponse(model.sceneNumber, true, dontShowAgain);
        view.SetVisible(false);
    }

    private void SendDeclined(bool dontShowAgain)
    {
        WebInterface.SendTermsOfServiceResponse(model.sceneNumber, false, dontShowAgain);
        view.SetVisible(false);
    }

    private void OpenToS()
    {
        if (!string.IsNullOrEmpty(model.tosURL))
            #if DCL_VR
            WebInterface.OpenURL(model.tosURL,true);
            #else
            WebInterface.OpenURL(model.tosURL);
            #endif
    }

    private void OpenPrivacyPolicy()
    {
        if (!string.IsNullOrEmpty(model.privacyPolicyURL))
            #if DCL_VR
            WebInterface.OpenURL(model.privacyPolicyURL,true);
            #else
            WebInterface.OpenURL(model.privacyPolicyURL);
            #endif
    }

    private void OpenContactEmail()
    {
        if (!string.IsNullOrEmpty(model.emailContactURL))
            #if DCL_VR
            WebInterface.OpenURL($"mailto:{model.emailContactURL}",true);
            #else
            WebInterface.OpenURL($"mailto:{model.emailContactURL}");
            #endif
    }

    public void SetVisibility(bool visible) { view.gameObject.SetActive(visible); }

    public void Dispose()
    {
        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }
}
