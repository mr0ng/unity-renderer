using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.MapRendererV2.ComponentsFactory;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using KernelConfigurationTypes;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DCLServices.MapRendererV2
{
    public partial class MapRenderer : IMapRenderer
    {
        private class MapLayerStatus
        {
            public readonly IMapLayerController MapLayerController;
            public int ActivityOwners;
            public CancellationTokenSource CTS;

            public MapLayerStatus(IMapLayerController mapLayerController)
            {
                MapLayerController = mapLayerController;
            }
        }

        private static readonly MapLayer[] ALL_LAYERS = EnumUtils.Values<MapLayer>();

        private CancellationToken cancellationToken;

        private readonly IMapRendererComponentsFactory componentsFactory;

        private Dictionary<MapLayer, MapLayerStatus> layers;
        private MapRendererConfiguration configurationInstance;

        private IObjectPool<IMapCameraControllerInternal> mapCameraPool;

        #if DCL_VR
        [SerializeField] private Image parcelHighlightImage;
        [SerializeField] private Image parcelHighlighImagePrefab;
        [SerializeField] private Image parcelHighlighWithContentImagePrefab;
        [SerializeField] private Image selectParcelHighlighImagePrefab;
        [HideInInspector] public bool showCursorCoords = true;
        public TextMeshProUGUI highlightedParcelText;
        private float parcelSizeInMap;
        [HideInInspector] public Vector2Int cursorMapCoords;
        public RectTransform centeredReferenceParcel;
        private PointerEventData uiRaycastPointerEventData = new PointerEventData(EventSystem.current);
        private List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
        private int NAVMAP_CHUNK_LAYER;
        // private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;
        private Vector3[] mapWorldspaceCorners = new Vector3[4];
        private Vector3 worldCoordsOriginInMap;
        List<WorldRange> validWorldRanges = new List<WorldRange>
        {
            new WorldRange(-150, -150, 150, 150) // default range
        };
        [HideInInspector]
        public event System.Action<float, float> OnMovedParcelCursor;
        #endif

        internal IMapCullingController cullingController { get; private set; }

        public MapRenderer(IMapRendererComponentsFactory componentsFactory)
        {
            this.componentsFactory = componentsFactory;
        }

        public void Initialize() { }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            layers = new Dictionary<MapLayer, MapLayerStatus>();

            try
            {
                var components = await componentsFactory.Create(cancellationToken);
                cullingController = components.CullingController;
                mapCameraPool = components.MapCameraControllers;
                configurationInstance = components.ConfigurationInstance;

                foreach (var pair in components.Layers)
                    layers[pair.Key] = new MapLayerStatus(pair.Value);
            }
            catch (OperationCanceledException)
            {
                // just ignore
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
#if DCL_VR
        void Update()
        {


            var scale = centeredReferenceParcel.lossyScale.x < 1f ? 1f : centeredReferenceParcel.lossyScale.x;
            parcelSizeInMap = centeredReferenceParcel.rect.width * scale;

            //the reference parcel has a bottom-left pivot
            PointerHelper.Instance.UpdateCorners(mapWorldspaceCorners, centeredReferenceParcel, ref worldCoordsOriginInMap);

            UpdateCursorMapCoords();

             UpdateParcelHighlight();

            // UpdateParcelHold();
        }
        #endif

        void UpdateCursorMapCoords()
        {
            if (!IsCursorOverMapChunk())
                return;

#if DCL_VR
            cursorMapCoords.x = (int) (PointerHelper.Instance.GetPointerPos().x - worldCoordsOriginInMap.x);
            cursorMapCoords.y = (int) (PointerHelper.Instance.GetPointerPos().y - worldCoordsOriginInMap.y);
            cursorMapCoords /= (int) parcelSizeInMap;

            cursorMapCoords.x = (int)Mathf.Floor(cursorMapCoords.x);
            cursorMapCoords.y = (int)Mathf.Floor(cursorMapCoords.y);
#else
            const int OFFSET = -60; //Map is a bit off centered, we need to adjust it a little.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(atlas.chunksParent, Input.mousePosition, DataStore.i.camera.hudsCamera.Get(), out var mapPoint);
            mapPoint -= Vector2.one * OFFSET;
            mapPoint -= (atlas.chunksParent.sizeDelta / 2f);
            cursorMapCoords = Vector2Int.RoundToInt(mapPoint / MapUtils.PARCEL_SIZE);
#endif
        }
        void UpdateParcelHighlight()
        {
            if (!CoordinatesAreInsideTheWorld((int)cursorMapCoords.x, (int)cursorMapCoords.y))
            {
                if (parcelHighlightImage.gameObject.activeSelf)
                    parcelHighlightImage.gameObject.SetActive(false);

                return;
            }

            if (!parcelHighlightImage.gameObject.activeSelf)
                parcelHighlightImage.gameObject.SetActive(true);

            string previousText = highlightedParcelText.text;
            parcelHighlightImage.rectTransform.SetAsLastSibling();
            parcelHighlightImage.rectTransform.anchoredPosition = MapUtils.CoordsToPosition(cursorMapCoords);
            highlightedParcelText.text = showCursorCoords ? $"{cursorMapCoords.x}, {cursorMapCoords.y}" : string.Empty;

            if (highlightedParcelText.text != previousText && !Input.GetMouseButton(0)) { OnMovedParcelCursor?.Invoke(cursorMapCoords.x, cursorMapCoords.y); }

            // ----------------------------------------------------
            // TODO: Use sceneInfo to highlight whole scene parcels and populate scenes hover info on navmap once we can access all the scenes info
            // var sceneInfo = mapMetadata.GetSceneInfo(cursorMapCoords.x, cursorMapCoords.y);
        }
        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            validWorldRanges = current.validWorldRanges;
        }
        bool CoordinatesAreInsideTheWorld(int xCoord, int yCoord)
        {
            foreach (WorldRange worldRange in validWorldRanges)
            {
                if (worldRange.Contains(xCoord, yCoord)) { return true; }
            }

            return false;
        }
        bool IsCursorOverMapChunk()
        {
#if DCL_VR
            uiRaycastPointerEventData.position = Input.mousePosition;// helper.GetPointerPos();
#else
            uiRaycastPointerEventData.position = Input.mousePosition;
#endif
            EventSystem.current.RaycastAll(uiRaycastPointerEventData, uiRaycastResults);

            return uiRaycastResults.Count > 0 && uiRaycastResults[0].gameObject.layer == NAVMAP_CHUNK_LAYER;
        }
        public IMapCameraController RentCamera(in MapCameraInput cameraInput)
        {
            const int MIN_ZOOM = 5;
            const int MAX_ZOOM = 300;

            // Clamp texture to the maximum size allowed, preserving aspect ratio
            Vector2Int zoomValues = cameraInput.ZoomValues;
            zoomValues.x = Mathf.Max(zoomValues.x, MIN_ZOOM);
            zoomValues.y = Mathf.Min(zoomValues.y, MAX_ZOOM);

            EnableLayers(cameraInput.EnabledLayers);
            var mapCameraController = mapCameraPool.Get();
            mapCameraController.OnReleasing += ReleaseCamera;
            mapCameraController.Initialize(cameraInput.TextureResolution, zoomValues, cameraInput.EnabledLayers);
            mapCameraController.SetPositionAndZoom(cameraInput.Position, cameraInput.Zoom);

            return mapCameraController;
        }

        private void ReleaseCamera(IMapCameraControllerInternal mapCameraController)
        {
            mapCameraController.OnReleasing -= ReleaseCamera;
            DisableLayers(mapCameraController.EnabledLayers);
            mapCameraPool.Release(mapCameraController);
        }

        private void EnableLayers(MapLayer mask)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (EnumUtils.HasFlag(mask, mapLayer)
                    && layers.TryGetValue(mapLayer, out var mapLayerStatus))
                {
                    if (mapLayerStatus.ActivityOwners == 0)
                    {
                        // Cancel deactivation flow
                        ResetCancellationSource(mapLayerStatus);
                        mapLayerStatus.MapLayerController.Enable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                    }

                    mapLayerStatus.ActivityOwners++;
                }
            }
        }

        private void DisableLayers(MapLayer mask)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (EnumUtils.HasFlag(mask, mapLayer)
                    && layers.TryGetValue(mapLayer, out var mapLayerStatus))
                {
                    if (--mapLayerStatus.ActivityOwners == 0)
                    {
                        // Cancel activation flow
                        ResetCancellationSource(mapLayerStatus);
                        mapLayerStatus.MapLayerController.Disable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                    }
                }
            }
        }

        private void ResetCancellationSource(MapLayerStatus mapLayerStatus)
        {
            if (mapLayerStatus.CTS != null)
            {
                mapLayerStatus.CTS.Cancel();
                mapLayerStatus.CTS.Dispose();
            }

            mapLayerStatus.CTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public void Dispose()
        {
            foreach (var status in layers.Values)
            {
                if (status.CTS != null)
                {
                    status.CTS.Cancel();
                    status.CTS.Dispose();
                    status.CTS = null;
                }

                status.MapLayerController.Dispose();
            }

            cullingController?.Dispose();

            if (configurationInstance)
                Utils.SafeDestroy(configurationInstance.gameObject);
        }
    }
}
