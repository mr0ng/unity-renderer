using System;

namespace DCL.Components
{
    public class GetAnchorPointsHandler : IDisposable
    {
        public event Action OnAvatarRemoved;

        private Action<IAvatarAnchorPoints> onAvatarFound;
        private bool cleaned = false;

        private UserProfile ownPlayerProfile => UserProfile.GetOwnUserProfile();

        private IAnchorPointsGetterHandler ownPlayerAnchorPointsGetterHandler => new OwnPlayerGetAnchorPointsHandler();
        private IAnchorPointsGetterHandler otherPlayerAnchorPointsGetterHandler => new OtherPlayerGetAnchorPointsHandler();
        private IAnchorPointsGetterHandler currentAnchorPointsGetterHandler;

        /// <summary>
        /// Search for an `avatarId` that could be the own player or any other player.
        /// If that `avatarId` is loaded it will call `onSuccess`, otherwise it will wait
        /// for that `avatarId` to be loaded
        /// </summary>
        public void SearchAnchorPoints(string avatarId, Action<IAvatarAnchorPoints> onSuccess, bool supportNullId = false)
        {
            CleanUp();
            cleaned = false;

            if (string.IsNullOrEmpty(avatarId) && !supportNullId)
                return;

            string ownUserId = ownPlayerProfile.userId;

            onAvatarFound = onSuccess;

            void GetOwnProfileUpdated(UserProfile profile)
            {
                ownPlayerProfile.OnUpdate -= GetOwnProfileUpdated;
                ownUserId = profile.userId;
                string targetId = string.IsNullOrEmpty(avatarId) ? ownUserId : avatarId;

                if (cleaned)
                {
                    return;
                }

                currentAnchorPointsGetterHandler = GetHandler(targetId, ownUserId);
                currentAnchorPointsGetterHandler.OnAvatarFound += OnAvatarFoundEvent;
                currentAnchorPointsGetterHandler.OnAvatarRemoved += OnAvatarRemovedEvent;
                currentAnchorPointsGetterHandler.GetAnchorPoints(targetId);
            }

            if (string.IsNullOrEmpty(ownUserId))
            {
                // Unsubscribe first in case of multiple calls.
                ownPlayerProfile.OnUpdate -= GetOwnProfileUpdated;
                ownPlayerProfile.OnUpdate += GetOwnProfileUpdated;
            }
            else
            {
                GetOwnProfileUpdated(ownPlayerProfile);
            }
        }

        /// <summary>
        /// Cancel the active search/waiting of the `avatarId`
        /// </summary>
        public void CancelCurrentSearch()
        {
            CleanUp();
        }

        public void Dispose()
        {
            CleanUp();
            ownPlayerAnchorPointsGetterHandler.Dispose();
            otherPlayerAnchorPointsGetterHandler.Dispose();
        }

        private void CleanUp()
        {
            onAvatarFound = null;
            cleaned = true;

            if (currentAnchorPointsGetterHandler != null)
            {
                currentAnchorPointsGetterHandler.CleanUp();
                currentAnchorPointsGetterHandler.OnAvatarFound -= OnAvatarFoundEvent;
                currentAnchorPointsGetterHandler.OnAvatarRemoved -= OnAvatarRemovedEvent;
            }
        }

        private IAnchorPointsGetterHandler GetHandler(string id, string ownPlayerId)
        {
            return id == ownPlayerId ? ownPlayerAnchorPointsGetterHandler : otherPlayerAnchorPointsGetterHandler;
        }

        private void OnAvatarFoundEvent(string id, IAvatarAnchorPoints anchorPoints)
        {
            onAvatarFound?.Invoke(anchorPoints);
        }

        private void OnAvatarRemovedEvent(string id)
        {
            OnAvatarRemoved?.Invoke();
        }
    }
}