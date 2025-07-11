using System;
using UnityEngine;

namespace _APA.Scripts
{
    public class ProgressiveBarrier : APAMonoBehaviour
    {
        public enum BarrierState { BlockingForward, Open, WaitingForPlayersToPass, BlockingBackward }

        [Header("Barrier Setup")]
        [SerializeField] private int boundaryPointIndex = -1;
        [SerializeField] private string unlockSignalID;
        [SerializeField, Min(1)] private int requiredUnlockSignals = 1;
        [SerializeField] private string sceneToLoadOnUnlock = "";
        [SerializeField] private Transform player1Transform;
        [SerializeField] private Transform player2Transform;

        private Collider2D blockingCollider;
        private BarrierState currentState;
        private int unlockCount = 0;
        private float barrierX;

        void Awake()
        {
            blockingCollider = GetComponent<Collider2D>();
            if (!blockingCollider || !player1Transform || !player2Transform)
            {
                Debug.LogError($"[Barrier] '{name}' missing references.", this);
                enabled = false;
                return;
            }
            barrierX = transform.position.x;
            SetState(BarrierState.BlockingForward);
        }

        void OnEnable()
        {
            if (currentState == BarrierState.BlockingForward && !string.IsNullOrEmpty(unlockSignalID))
                // EventManager.OnObjectActivate += HandleUnlockSignal;
                Manager.EventManager.AddListener(APAEventName.OnObjectActivate,OnObjectActivated );

        }

        void OnDisable()
        {
            Manager.EventManager.RemoveListener(APAEventName.OnObjectActivate,OnObjectActivated);
            // EventManager.OnObjectActivate -= HandleUnlockSignal;
        }

        void Update()
        {
            if (currentState != BarrierState.WaitingForPlayersToPass) return;
            if (player1Transform.position.x > barrierX + 1f && player2Transform.position.x > barrierX + 1f)
            {
                SetState(BarrierState.BlockingBackward);
                if (boundaryPointIndex >= 0)
                    Manager.EventManager.InvokeEvent(APAEventName.OnPlayersPassedBarrier, boundaryPointIndex);
                    // EventManager.TriggerPlayersPassedBarrier(boundaryPointIndex);
            }
        }

        private void HandleUnlockSignal(string id, GameObject _) 
        {
            if (id != unlockSignalID || currentState != BarrierState.BlockingForward || ++unlockCount < requiredUnlockSignals) return;

            if (boundaryPointIndex >= 0)
                EventManager.TriggerBarrierOpened(boundaryPointIndex);
            if (!string.IsNullOrEmpty(sceneToLoadOnUnlock))
                EventManager.TriggerLoadScene(sceneToLoadOnUnlock);

            SetState(BarrierState.Open);
            // EventManager.OnObjectActivate -= HandleUnlockSignal;
            Manager.EventManager.RemoveListener(APAEventName.OnObjectActivate,OnObjectActivated);
        }
        private void OnObjectActivated(object data)
        {
            if (data is not Tuple<string, GameObject> tuple) return;

            HandleUnlockSignal(tuple.Item1, tuple.Item2);
        }


        private void SetState(BarrierState newState)
        {
            currentState = newState;
            switch (newState)
            {
                case BarrierState.BlockingForward:
                case BarrierState.BlockingBackward:
                    blockingCollider.enabled = true;
                    blockingCollider.isTrigger = false;
                    break;
                case BarrierState.Open:
                    blockingCollider.enabled = false;
                    SetState(BarrierState.WaitingForPlayersToPass);
                    break;
                case BarrierState.WaitingForPlayersToPass:
                    // No collider changes
                    break;
            }
        }

        public void ResetBarrier()
        {
            unlockCount = 0;
            SetState(BarrierState.BlockingForward);
            if (isActiveAndEnabled) OnEnable();
        }
    }
}
