using System.Collections;
using System.Linq;
using DCL.Components;
using DCL.Components.Video.Plugin;
using DCL.Models;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Controllers
{
    public static class DCLVideoTextureUtils
    {
        public static float GetClosestDistanceSqr(ISharedComponent disposableComponent, Vector3 fromPosition)
        {
            float dist = int.MaxValue;

            if (disposableComponent.GetAttachedEntities().Count <= 0)
                return dist;

            using (var iterator = disposableComponent.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    IDCLEntity entity = iterator.Current;

                    if (IsEntityVisible(iterator.Current))
                    {
                        var entityDist = (entity.meshRootGameObject.transform.position - fromPosition).sqrMagnitude;
                        if (entityDist < dist)
                            dist = entityDist;
                    }
                }
            }

            return dist;
        }

        public static bool IsComponentVisible(ISharedComponent disposableComponent)
        {
            if (disposableComponent.GetAttachedEntities().Count <= 0)
                return false;

            using (var iterator = disposableComponent.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    bool isEntityVisible = IsEntityVisible(iterator.Current);

                    if (isEntityVisible)
                        return true;
                }
            }

            return false;
        }

        public static bool IsEntityVisible(IDCLEntity entity)
        {
            if (entity.meshesInfo == null)
                return false;

            if (entity.meshesInfo.currentShape == null)
                return false;

            return entity.meshesInfo.currentShape.IsVisible();
        }

        public static void UnsubscribeToEntityShapeUpdate(ISharedComponent component,
            System.Action<IDCLEntity> OnUpdate)
        {
            if (component.GetAttachedEntities().Count <= 0)
                return;

            using (var iterator = component.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var entity = iterator.Current;
                    entity.OnShapeUpdated -= OnUpdate;
                }
            }
        }

        public static void SubscribeToEntityUpdates(ISharedComponent component, System.Action<IDCLEntity> OnUpdate)
        {
            if (component.GetAttachedEntities().Count <= 0)
                return;

            using (var iterator = component.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var entity = iterator.Current;
                    entity.OnShapeUpdated -= OnUpdate;
                    entity.OnShapeUpdated += OnUpdate;
                }
            }
        }

        public static void SetAVProMaterial(ISharedComponent component, MediaPlayer texturePlayer, string mediaPlayer)
        {
            if (component.GetAttachedEntities().Count <= 0)
                return;
            using (var iterator = component.GetAttachedEntities().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var entity = iterator.Current;
                    UnityThread.executeCoroutine(EnsureAndroidMaterial(entity, component, texturePlayer, mediaPlayer));
                }
            }
        }
        private static IEnumerator EnsureAndroidMaterial(IDCLEntity entity, ISharedComponent component,
            MediaPlayer texturePlayer, string mediaPlayer)
        {
            if (entity?.gameObject == null)
            {
                Debug.LogError("Entity or its GameObject is null.");
                yield break;
            }
            MeshRenderer meshRenderer = entity.gameObject.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogError("Mesh renderer not found.");
                yield break;
            }
            Shader androidShader = Shader.Find("Unlit/Texture");
            if (androidShader == null)
            {
                Debug.LogError("Shader not found.");
                yield break; // Exit the coroutine
            }
            Material newMaterial = new Material( androidShader);
            yield return new WaitForSeconds(0.2f);
            while (IsEntityVisible(entity))
            {
                if (texturePlayer == null)
                {
                    GameObject foundGameObject = GameObject.Find(mediaPlayer);
                    if (foundGameObject != null)
                    {
                        foundGameObject.transform.parent = entity.gameObject.transform.parent;
                        texturePlayer = foundGameObject.GetComponent<MediaPlayer>();
                    }
                    if (texturePlayer == null)
                    {
                        Debug.LogError("MediaPlayer GameObject or component not found.");
                        yield return new WaitForSeconds(0.3f);
                        continue;
                    }
                }
                RawImage[] rawImages = entity.gameObject.GetComponentsInChildren<RawImage>();
                if (rawImages.Length > 0)
                {
                    Debug.LogError("AVPro texture already applied.");
                    yield break;
                }
                else
                {
                    GameObject avProUIMGrefab = Resources.Load("AVProVideoImage") as GameObject;
                    GameObject avProInstance = GameObject.Instantiate(avProUIMGrefab,
                        meshRenderer.gameObject.transform.position,
                        meshRenderer.gameObject.transform.rotation);
                    avProInstance.transform.parent = entity.gameObject.transform;
                    avProInstance.transform.localScale = Vector3.one;
                    rawImages = avProInstance.GetComponentsInChildren<RawImage>();
                    ResolveToRenderTexture resolveToRenderTexture =
                        texturePlayer.gameObject.GetComponent<ResolveToRenderTexture>();
                    if (resolveToRenderTexture?.TargetTexture != null)
                    {
                        foreach (RawImage ri in rawImages)
                        {
                            if (ri != null)
                            {
                                ri.material = newMaterial;
                                ri.texture = resolveToRenderTexture.TargetTexture;
                            }
                        }
                        meshRenderer.enabled = false;
                        Debug.Log($"Applied AVProMaterial: {texturePlayer.name}, {entity.entityId}");
                        yield break;
                    }
                    else
                    {
                        Debug.LogError("ResolveToRenderTexture or its TargetTexture is null.");
                        yield return new WaitForSeconds(0.3f);
                    }
                }
            }
        }
    }
}
