using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCL.Helpers;
using DCL;

public class TeleportPromptHUDView : MonoBehaviour
{
    [SerializeField] internal ShowHideAnimator contentAnimator;
    [SerializeField] public GameObject content;
    [SerializeField] internal Animator teleportHUDAnimator;
    [SerializeField] internal GraphicRaycaster teleportRaycaster;

    [Header("Images")]
    [SerializeField] private RawImage imageSceneThumbnail;

    [SerializeField] private Image imageGotoCrowd;
    [SerializeField] internal Image imageGotoMagic;
    [SerializeField] private Texture2D nullImage;

    [Header("Containers")]
    [SerializeField] private GameObject containerCoords;

    [SerializeField] private GameObject containerMagic;
    [SerializeField] private GameObject containerCrowd;
    [SerializeField] private GameObject containerScene;
    [SerializeField] private GameObject containerEvent;
    [SerializeField] private GameObject creatorContainer;

    [Header("Scene info")]
    [SerializeField] private TextMeshProUGUI textCoords;

    [SerializeField] private TextMeshProUGUI textSceneName;
    [SerializeField] private TextMeshProUGUI textSceneOwner;

    [Header("Event info")]
    [SerializeField] private TextMeshProUGUI textEventInfo;

    [SerializeField] private TextMeshProUGUI textEventName;
    [SerializeField] private TextMeshProUGUI textEventAttendees;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button backgroundCatcher;

    public event Action OnCloseEvent;
    public event Action OnTeleportEvent;
    public event Action<bool> OnSetVisibility;

    IWebRequestAsyncOperation fetchParcelImageOp;
    Texture2D downloadedBanner;
    private HUDCanvasCameraModeController hudCanvasCameraModeController;
    private static readonly int IDLE = Animator.StringToHash("Idle");
    private static readonly int OUT = Animator.StringToHash("Out");
    private static readonly int IN = Animator.StringToHash("In");

    private void Awake()
    {
        hudCanvasCameraModeController = new HUDCanvasCameraModeController(content.GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);
        cancelButton.onClick.AddListener(OnClosePressed);
        backgroundCatcher.onClick.AddListener(OnClosePressed);
        #if DCL_VR
        continueButton.onClick.AddListener(OnTeleportPressed);
        contentAnimator.OnWillFinishHide += (animator) => Hide();

        contentAnimator.Hide();
        OnSetVisibility?.Invoke(false);
        content.GetComponent<Canvas>().enabled = false;
        #endif
    }

    public void Reset()
    {
        containerCoords.SetActive(false);
        containerCrowd.SetActive(false);
        containerMagic.SetActive(false);
        containerScene.SetActive(false);
        containerEvent.SetActive(false);

        imageSceneThumbnail.gameObject.SetActive(false);
        imageGotoCrowd.gameObject.SetActive(false);
        imageGotoMagic.gameObject.SetActive(false);
    }

    public void SetLoadingCompleted()
    {
        teleportHUDAnimator.SetTrigger(IDLE);
    }

    public void SetInAnimation()
    {
        #if DCL_VR
        SetVisibility(true);
        #endif
        teleportRaycaster.enabled = true;
        teleportHUDAnimator.SetTrigger(IN);
    }

    public void SetOutAnimation()
    {
        teleportRaycaster.enabled = false;
        teleportHUDAnimator.SetTrigger(OUT);
    }

    public void ShowTeleportToMagic()
    {
#if DCL_VR
        OnSetVisibility?.Invoke(true);
#endif
        containerMagic.SetActive(true);
        imageGotoMagic.gameObject.SetActive(true);
    }

    public void ShowTeleportToCrowd()
    {
        #if DCL_VR
        OnSetVisibility?.Invoke(true);
        #endif
        containerCrowd.SetActive(true);
        imageGotoCrowd.gameObject.SetActive(true);
    }

    public void ShowTeleportToCoords(string coords, string sceneName, string sceneCreator, string previewImageUrl)
    {
        #if DCL_VR
        OnSetVisibility?.Invoke(true);
        #endif
        containerCoords.SetActive(true);
        containerScene.SetActive(true);

        textCoords.text = coords;
        textSceneName.text = !string.IsNullOrEmpty(sceneName) ? sceneName : "Untitled Scene";
        creatorContainer.SetActive(!string.IsNullOrEmpty(sceneCreator));
        textSceneOwner.text = sceneCreator;
        SetParcelImage(previewImageUrl);
    }

    public void SetEventInfo(string eventName, string eventStatus, int attendeesCount)
    {
        containerEvent.SetActive(true);
        textEventInfo.text = eventStatus;
        textEventName.text = eventName;
        textEventAttendees.text = $"+{attendeesCount}";
    }
    //VR helper
    public void SetVisibility(bool visible)
    {
        OnSetVisibility?.Invoke(visible);
    }
    //VR helper
    private void Hide()
    {
        content.GetComponent<Canvas>().enabled = false;
        SetVisibility((false));
        if (fetchParcelImageOp != null)
            fetchParcelImageOp.Dispose();

        if (downloadedBanner != null)
        {
            UnityEngine.Object.Destroy(downloadedBanner);
            downloadedBanner = null;
        }
	}


    private AssetPromise_Texture texturePromise;

    public void SetParcelImage(string imageUrl)
    {
        containerMagic.SetActive(false);
        imageSceneThumbnail.gameObject.SetActive(true);

        if (string.IsNullOrEmpty(imageUrl))
        {
            DisplayThumbnail(nullImage);
            return;
        }

        if (texturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(texturePromise);

        texturePromise = new AssetPromise_Texture(imageUrl, storeTexAsNonReadable: false);
        texturePromise.OnSuccessEvent += (textureAsset) => { DisplayThumbnail(textureAsset.texture); };
        texturePromise.OnFailEvent += (x, e) => DisplayThumbnail(nullImage);
        AssetPromiseKeeper_Texture.i.Keep(texturePromise);
    }

    private void DisplayThumbnail(Texture2D texture)
    {
        imageSceneThumbnail.texture = texture;
    }

    private void OnClosePressed()
    {
        OnCloseEvent?.Invoke();
#if DCL_VR //TODO: test that this is still needed for VR or if new updates make this obsolete.
        contentAnimator.Hide(true);
        OnSetVisibility?.Invoke(false);
        AudioScriptableObjects.dialogClose.Play(true);
        //transform.position += 20*Vector3.down;
#endif
    }

    private void OnTeleportPressed()
    {
        OnTeleportEvent?.Invoke();
 #if DCL_VR
         OnSetVisibility?.Invoke(false);
         contentAnimator.Hide(true);
         //transform.position += 20*Vector3.down;
 #endif
    }

    private void OnDestroy()
    {
        hudCanvasCameraModeController?.Dispose();
        if (downloadedBanner != null)
        {
            UnityEngine.Object.Destroy(downloadedBanner);
            downloadedBanner = null;
        }

        if (fetchParcelImageOp != null)
            fetchParcelImageOp.Dispose();
    }
}
