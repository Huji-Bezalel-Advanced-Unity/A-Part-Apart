namespace _APA.Scripts.Managers
{
    using System;
    using APA.Core;
    using UnityEngine;

    public static class EventManager
    {
        public static event Action<string, GameObject> OnObjectActivate; 
        public static event Action<string, GameObject> OnObjectDeactivate;
        public static event Action<string> OnLoadSceneRequested;

        public static event Action<LightInteractionController> OnDarkPlayerStuckInLight;
        public static event Action<LightInteractionController> OnDarkPlayerStuckInLightCamera;

        public static event Action<int> OnBarrierOpened;

        public static event Action<int>
            OnPlayersPassedBarrier; 

        public static event Action<LightInteractionController> OnShowStuckDecisionUI;

        public static void TriggerBarrierOpened(int openedBarrierIndex) => OnBarrierOpened?.Invoke(openedBarrierIndex);

        public static void TriggerPlayersPassedBarrier(int passedBarrierIndex) =>
            OnPlayersPassedBarrier?.Invoke(passedBarrierIndex);

        public static void TriggerDarkPlayerStuckInLight(LightInteractionController player)
        {
            if (player == null)
            {
                Debug.LogWarning("EventManager: TriggerDarkPlayerStuckInLight called with null player.");
                return;
            }

            Debug.Log($"EventManager: Dark Player '{player.gameObject.name}' is STUCK in light!");
            OnDarkPlayerStuckInLight?.Invoke(player);
            OnDarkPlayerStuckInLightCamera?.Invoke(player);
        }

        public static void TriggerShowStuckDecisionUI(LightInteractionController player)
        {
            if (player == null)
            {
                Debug.LogWarning("EventManager: TriggerShowStuckDecisionUI called with null player.");
                return;
            }

            Debug.Log($"EventManager: Requesting to show Stuck Decision UI for player {player.gameObject.name}");
            OnShowStuckDecisionUI?.Invoke(player);
        }

        public static void TriggerLoadScene(string sceneName)
        {
            Debug.Log($"EventManager: Requesting to load scene: {sceneName}");
            OnLoadSceneRequested?.Invoke(sceneName);
        }
        
        public static void TriggerObjectActivate(string objectID, GameObject source) 
        {
            if (string.IsNullOrEmpty(objectID) || source == null) return;
            Debug.Log($"EventManager: Triggering Object Activate for ID: {objectID} from Source: {source.name}");
            OnObjectActivate?.Invoke(objectID, source);
        }

        public static void TriggerObjectDeactivate(string objectID, GameObject source) 
        {
            if (string.IsNullOrEmpty(objectID) || source == null) return;
            Debug.Log($"EventManager: Triggering Object Deactivate for ID: {objectID} from Source: {source.name}");
            OnObjectDeactivate?.Invoke(objectID, source);
        }
    }
}