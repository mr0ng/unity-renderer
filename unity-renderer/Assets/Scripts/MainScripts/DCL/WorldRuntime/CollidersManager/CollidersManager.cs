using DCL.Components;
using DCL.Configuration;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class CollidersManager : Singleton<CollidersManager>
    {
        private Dictionary<Collider, ColliderInfo> colliderInfo = new ();
        private Dictionary<IDCLEntity, List<Collider>> collidersByEntity = new ();
        private static CollidersManager instance = null;

        public static void Release()
        {
            if (instance != null)
            {
                using (var iterator = instance.collidersByEntity.Keys.GetEnumerator())
                {
                    while (iterator.MoveNext())
                        iterator.Current.OnCleanupEvent -= instance.OnEntityCleanUpEvent;
                }

                instance = null;
            }
        }

        void AddOrUpdateColliderInfo(Collider collider, ColliderInfo info)
        {
            // Note (Zak): This could be achieved in one line
            // just by doing colliderInfo[collider] = info;
            // but nobody likes it that way... :'(
            if (colliderInfo.ContainsKey(collider))
                colliderInfo[collider] = info;
            else
                colliderInfo.Add(collider, info);
        }

        void RemoveColliderInfo(Collider collider)
        {
            if (colliderInfo.ContainsKey(collider))
                colliderInfo.Remove(collider);
        }

        public void RemoveEntityCollider(IDCLEntity entity, Collider collider)
        {
            if (entity == null || collider == null || !collidersByEntity.ContainsKey(entity))
                return;

            collidersByEntity[entity].Remove(collider);
            RemoveColliderInfo(collider);
        }

        public void AddOrUpdateEntityCollider(IDCLEntity entity, Collider collider)
        {
            if (!collidersByEntity.ContainsKey(entity))
                collidersByEntity.Add(entity, new List<Collider>());

            List<Collider> collidersList = collidersByEntity[entity];

            if (!collidersList.Contains(collider))
                collidersList.Add(collider);

            ColliderInfo info = new ColliderInfo();
            info.entity = entity;
            info.meshName = GetMeshName(collider);
            info.scene = entity.scene;
            AddOrUpdateColliderInfo(collider, info);

            // Note (Zak): avoid adding the event multiple times
            entity.OnCleanupEvent -= OnEntityCleanUpEvent;
            entity.OnCleanupEvent += OnEntityCleanUpEvent;
        }

        private static string GetMeshName(Collider collider)
        {
            string originalName = collider.transform.name.ToLower();

            // Old GLTF
            if (originalName.Contains("primitive")) { return collider.transform.parent != null ? collider.transform.parent.name : ""; }

            return collider.transform.name;
        }

        void RemoveAllEntityColliders(IDCLEntity entity)
        {
            if (collidersByEntity.ContainsKey(entity))
            {
                List<Collider> collidersList = collidersByEntity[entity];
                int count = collidersList.Count;

                for (int i = 0; i < count; i++)
                    RemoveColliderInfo(collidersList[i]);

                collidersByEntity.Remove(entity);
            }
        }

        void OnEntityCleanUpEvent(ICleanableEventDispatcher dispatcher)
        {
            dispatcher.OnCleanupEvent -= OnEntityCleanUpEvent;

            RemoveAllEntityColliders((IDCLEntity)dispatcher);
        }

        public bool GetColliderInfo(Collider collider, out ColliderInfo info)
        {
            if (collider != null && colliderInfo.ContainsKey(collider))
            {
                info = colliderInfo[collider];
                return true;
            }
            else { info = new ColliderInfo(); }

            return false;
        }

        public void ConfigureColliders(IDCLEntity entity, bool hasCollision = true, bool filterByColliderName = true)
        {
            ConfigureColliders(entity.meshRootGameObject, hasCollision, filterByColliderName, entity);
        }

        public void ConfigureColliders(GameObject meshGameObject, bool hasCollision, bool filterByColliderName = false, IDCLEntity entity = null, int colliderLayer = -1)
        {
            if (meshGameObject == null)
                return;

            if (entity != null)
                entity.meshesInfo.colliders.Clear();

            if (colliderLayer == -1)
                colliderLayer = DCL.Configuration.PhysicsLayers.defaultLayer;

            Collider collider;
            int onClickLayer = PhysicsLayers.onPointerEventLayer; // meshes can have an OnPointerEvent child collider that should be ignored
            MeshFilter[] meshFilters = meshGameObject.GetComponentsInChildren<MeshFilter>(true);

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].gameObject.layer == onClickLayer)
                    continue;

                // NOTE(Kinerius): By design every collider shouldn't have a renderer, so if we detect it, we remove it,
                // but this step should not be done on this domain, we should be able to remove this check safely in the future
                if (filterByColliderName)
                {
                    if (!IsCollider(meshFilters[i].transform))
                        continue;

                    // NOTE(Kinerius): This is dangerous and might cause some null references since some objects like MeshesInfo contains references to this
                    Object.Destroy(meshFilters[i].GetComponent<Renderer>());
                }

                collider = meshFilters[i].GetComponent<Collider>();

                //HACK(Pravus): Hack to bring back compatibility with old builder scenes that have withCollision = false in the JS code.
                //              Remove when we fix this changing the property name or something similar.
                bool shouldCreateCollider = hasCollision || filterByColliderName;

                if (shouldCreateCollider)
                {
                    if (collider == null)
                        collider = meshFilters[i].gameObject.AddComponent<MeshCollider>();

                    if (collider is MeshCollider)
                        ((MeshCollider)collider).sharedMesh = meshFilters[i].sharedMesh;

                    if (entity != null)
                        AddOrUpdateEntityCollider(entity, collider);
                }

                if (collider != null)
                {
                    collider.gameObject.layer = colliderLayer;
                    collider.enabled = shouldCreateCollider;

                    if (entity != null)
                        entity.meshesInfo.colliders.Add(collider);
                }
            }
        }

        private static bool IsCollider(Transform transform)
        {
            bool transformName = transform.name.ToLower().Contains("_collider");
            bool parentName = transform.parent.name.ToLower().Contains("_collider");

            return parentName || transformName;
        }
    }
}
