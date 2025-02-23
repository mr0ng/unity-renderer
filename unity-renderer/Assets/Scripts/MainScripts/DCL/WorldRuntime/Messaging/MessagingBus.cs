using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using DCL.Interface;
using DCL.Models;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using UnityEngine.SceneManagement;

namespace DCL
{
    public enum QueueMode
    {
        Reliable,
        Lossy,
    }

    public class MessagingBus : IDisposable
    {
        public static bool VERBOSE = false;

        public IMessageProcessHandler handler;

        public LinkedList<QueuedSceneMessage> pendingMessages = new LinkedList<QueuedSceneMessage>();
        public bool hasPendingMessages => pendingMessagesCount > 0;

        //NOTE(Brian): This is handled manually. We aren't using pendingMessages.Count because is slow. Used heavily on critical ProcessMessages() loop.
        public int pendingMessagesCount;
        public long processedMessagesCount { get; set; }

        private static bool renderingIsDisabled => !CommonScriptableObjects.rendererState.Get();
        private float timeBudgetValue;

        public CustomYieldInstruction msgYieldInstruction;

        public MessagingBusType type;
        public int debugSceneNumber;

        public MessagingController owner;
        private IMessagingControllersManager manager;

        Dictionary<string, LinkedListNode<QueuedSceneMessage>> unreliableMessages =
            new Dictionary<string, LinkedListNode<QueuedSceneMessage>>();

        public int unreliableMessagesReplaced = 0;

        public bool enabled;

        public float timeBudget
        {

#if DCL_VR
            get => renderingIsDisabled ? 3*timeBudgetValue: timeBudgetValue;
#else
			get => renderingIsDisabled ? float.MaxValue : timeBudgetValue;
#endif
            set => timeBudgetValue = value;
        }

        public MessagingBus(MessagingBusType type, IMessageProcessHandler handler, MessagingController owner)
        {
            Assert.IsNotNull(handler, "IMessageHandler can't be null!");
            this.handler = handler;
            this.enabled = false;
            this.type = type;
            this.owner = owner;
            this.pendingMessagesCount = 0;
            manager = owner.messagingManager;
        }

        public void Start()
        {
            enabled = true;
        }

        public void Stop()
        {
            enabled = false;

            if (msgYieldInstruction is CleanableYieldInstruction cleanableYieldInstruction)
                cleanableYieldInstruction.Cleanup();

            pendingMessagesCount = 0;
        }

        public void Dispose()
        {
            Stop();
        }

        Dictionary<string, LinkedListNode<QueuedSceneMessage>> tempDictionary = new Dictionary<string, LinkedListNode<QueuedSceneMessage>>();

        public void Enqueue(QueuedSceneMessage message, QueueMode queueMode = QueueMode.Reliable)
        {
            lock (unreliableMessages)
            {
                // TODO: If we check here for 'if (message == null || string.IsNullOrEmpty(message.message))' loading the scene breaks, as we get empty messages every frame...
                if (message == null)
                    throw new Exception("A null message?");

                bool enqueued = true;

                // When removing an entity we have to ensure that the enqueued lossy messages after it are processed and not replaced
                if (message is QueuedSceneMessage_Scene { payload: Protocol.RemoveEntity removeEntityPayload })
                {
                    tempDictionary.Clear();

                    foreach (var unreliableMessage in unreliableMessages)
                        if (!unreliableMessage.Key.Contains(removeEntityPayload.entityId))
                            tempDictionary.Add(unreliableMessage.Key, unreliableMessage.Value);

                    unreliableMessages = tempDictionary;
                }

                if (queueMode == QueueMode.Reliable)
                {
                    message.isUnreliable = false;
                    AddReliableMessage(message);
                }
                else
                {
                    LinkedListNode<QueuedSceneMessage> node = null;
                    message.isUnreliable = true;

                    if (unreliableMessages.ContainsKey(message.tag))
                    {
                        node = unreliableMessages[message.tag];

                        if (node.List != null)
                        {
                            node.Value = message;
                            enqueued = false;
                            unreliableMessagesReplaced++;
                        }
                    }

                    if (enqueued)
                    {
                        node = AddReliableMessage(message);
                        unreliableMessages[message.tag] = node;
                    }
                }

                if (enqueued)
                {
                    if (message.type == QueuedSceneMessage.Type.SCENE_MESSAGE)
                    {
                        QueuedSceneMessage_Scene sm = message as QueuedSceneMessage_Scene;
                        ProfilingEvents.OnMessageWillQueue?.Invoke(sm.method);
                    }

                    if (type == MessagingBusType.INIT)
                    {
                        manager.pendingInitMessagesCount++;
                    }

                    if (owner != null)
                    {
                        owner.enabled = true;
                        manager.MarkBusesDirty();
                    }
                }
            }
        }

        public bool ProcessQueue(float timeBudget, out IEnumerator yieldReturn)
        {
            yieldReturn = null;

            // Note (Zak): This check is to avoid calling Time.realtimeSinceStartup
            // unnecessarily because it's pretty slow in JS
            if (timeBudget <= 0 || !enabled || pendingMessagesCount == 0)
                return false;

            float startTime = Time.realtimeSinceStartup;

            while (enabled && pendingMessagesCount > 0 )//&& Time.realtimeSinceStartup - startTime < timeBudget)
            {

                LinkedListNode<QueuedSceneMessage> pendingMessagesFirst;

                lock (pendingMessages)
                {
                    pendingMessagesFirst = pendingMessages.First;
                }

                // Note (Kinerius): This may be caused by 2 threads fighting for messages
                if (pendingMessagesFirst == null)
                {
                    pendingMessagesCount = 0;

                    continue;
                }

                QueuedSceneMessage m = pendingMessagesFirst.Value;

                PerformanceAnalytics.MessagesProcessedTracker.Track();

                RemoveFirstReliableMessage();

                if (m.isUnreliable)
                    RemoveUnreliableMessage(m);

                bool shouldLogMessage = VERBOSE;

                switch (m.type)
                {
                    case QueuedSceneMessage.Type.NONE:
                        break;
                    case QueuedSceneMessage.Type.SCENE_MESSAGE:

                        if (!(m is QueuedSceneMessage_Scene sceneMessage))
                            continue;

                        if (handler.ProcessMessage(sceneMessage, out msgYieldInstruction))
                        {
#if UNITY_EDITOR
                            if (DataStore.i.debugConfig.msgStepByStep)
                            {
                                if (VERBOSE)
                                {
                                    LogMessage(m, this, false);
                                    shouldLogMessage = false;
                                }

                                return true;
                            }
#endif
                        }
                        else
                        {
                            shouldLogMessage = false;
                        }

                        OnMessageProcessed();
                        ProfilingEvents.OnMessageWillDequeue?.Invoke(sceneMessage.method);

                        if (msgYieldInstruction != null)
                        {
                            processedMessagesCount++;

                            msgYieldInstruction = null;
                        }

                        break;
                    case QueuedSceneMessage.Type.LOAD_PARCEL:
                        #if DCL_VR
                        UnityThread.ExecuteInTimeBudgetCoroutine(() =>
                        {
                            handler.LoadParcelScenesExecute(m.message);
                            ProfilingEvents.OnMessageWillDequeue?.Invoke("LoadScene");
                        });
                        #else
                        handler.LoadParcelScenesExecute(m.message);
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("LoadScene");
                        #endif
                        break;
                    case QueuedSceneMessage.Type.UNLOAD_PARCEL:
                        #if DCL_VR
                        UnityThread.ExecuteInTimeBudgetCoroutine(() =>
                        {

                            handler.UnloadParcelSceneExecute(m.sceneNumber);
                            ProfilingEvents.OnMessageWillDequeue?.Invoke("UnloadScene");

                        });
                        #else
                        handler.UnloadParcelSceneExecute(m.sceneNumber);
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("UnloadScene");
                        #endif

                        break;
                    case QueuedSceneMessage.Type.UPDATE_PARCEL:
                    #if DCL_VR
                        UnityThread.ExecuteInTimeBudgetCoroutine(() =>
                        {
                            handler.UpdateParcelScenesExecute(m.message);
                            ProfilingEvents.OnMessageWillDequeue?.Invoke("UpdateScene");
                        });
                        #else
                        handler.UpdateParcelScenesExecute(m.message);
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("UpdateScene");
                        #endif
                        break;
                    case QueuedSceneMessage.Type.UNLOAD_SCENES:
                        #if DCL_VR
                        UnityThread.ExecuteInTimeBudgetCoroutine(() =>
                        {
                            handler.UnloadAllScenes();
                            ProfilingEvents.OnMessageWillDequeue?.Invoke("UnloadAllScenes");
                        });
                        #else
                        handler.UnloadAllScenes();
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("UnloadAllScenes");

                        #endif
                        break;
                }

                OnMessageProcessed();
#if UNITY_EDITOR
                if (shouldLogMessage)
                {
                    LogMessage(m, this);
                }
#endif
            }

            return false;
        }

        public void OnMessageProcessed()
        {
            processedMessagesCount++;

            if (type == MessagingBusType.INIT)
            {
                manager.pendingInitMessagesCount--;
                manager.processedInitMessagesCount++;
            }
        }

        private LinkedListNode<QueuedSceneMessage> AddReliableMessage(QueuedSceneMessage message)
        {
            manager.pendingMessagesCount++;
            pendingMessagesCount++;

            LinkedListNode<QueuedSceneMessage> addReliableMessage;

            lock (pendingMessages)
            {
                addReliableMessage = pendingMessages.AddLast(message);
            }

            return addReliableMessage;
        }

        private void RemoveFirstReliableMessage()
        {
            lock (pendingMessages)
            {
                if (pendingMessages.First != null)
                {
                    pendingMessages.RemoveFirst();
                    pendingMessagesCount--;
                    manager.pendingMessagesCount--;
                }
            }
        }

        private void RemoveUnreliableMessage(QueuedSceneMessage message)
        {
            lock (unreliableMessages)
            {
                unreliableMessages.Remove(message.tag);
            }
        }

        private void LogMessage(QueuedSceneMessage m, MessagingBus bus, bool logType = true)
        {
            string finalTag = WorldStateUtils.TryToGetSceneCoordsFromSceneNumber(bus.debugSceneNumber);

            if (logType)
            {
                Debug.Log(
                    $"#{bus.processedMessagesCount} ... bus = {finalTag}, id = {bus.type}... processing msg... type = {m.type}... message = {m.message}");
            }
            else
            {
                Debug.Log(
                    $"#{bus.processedMessagesCount} ... Bus = {finalTag}, id = {bus.type}... processing msg... {m.message}");
            }
        }
    }
}
