using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _APA.Scripts
{
    public class MainCameraController : APAMonoBehaviour
    {
        [Header("Players")]
        [SerializeField] private Transform player1;
        [SerializeField] private Transform player2;

        [Header("Camera")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Vector2 followOffset;
        [SerializeField] private float smoothTime = 0.3f;

        [Header("Boundaries")]
        public List<Transform> sectionBoundaryPoints;
        [SerializeField] private int currentMinBoundaryIndex = 0;
        [SerializeField] private int currentMaxBoundaryIndex = 1;

        [Header("Settings")]
        [SerializeField] private bool forcePlayersInView = true;

        private float _cameraHalfWidth;
        private Vector3 _velocity;
        private float _minX, _maxX;
        private bool _darkPlayerStuckHandled = false;

        void Start()
        {
            if (!ValidateSetup()) return;

            RecalculateCameraHalfWidth();
            UpdateViewBoundaries();

            transform.position = new Vector3(CalculateTargetX(), transform.position.y, transform.position.z);

            // EventManager.OnBarrierOpened += HandleBarrierOpened;
            // EventManager.OnPlayersPassedBarrier += HandlePlayersPassedBarrier;
            // EventManager.OnDarkPlayerStuckInLightCamera += HandleDarkPlayerStuck;
            Manager.EventManager.AddListener(APAEventName.OnBarrierOpened,HandleBarrierOpened);
            Manager.EventManager.AddListener(APAEventName.OnPlayersPassedBarrier,HandlePlayersPassedBarrier);
            Manager.EventManager.AddListener(APAEventName.OnDarkPlayerStuckInLight,HandleDarkPlayerStuck);
        }

        void OnDestroy()
        {
            // EventManager.OnBarrierOpened -= HandleBarrierOpened;
            // EventManager.OnPlayersPassedBarrier -= HandlePlayersPassedBarrier;
            // EventManager.OnDarkPlayerStuckInLightCamera -= HandleDarkPlayerStuck;
            Manager.EventManager.RemoveListener(APAEventName.OnBarrierOpened, HandleBarrierOpened);
            Manager.EventManager.RemoveListener(APAEventName.OnPlayersPassedBarrier, HandlePlayersPassedBarrier);
            Manager.EventManager.RemoveListener(APAEventName.OnDarkPlayerStuckInLight, HandleDarkPlayerStuck);
        }

        void LateUpdate()
        {
            RecalculateCameraHalfWidth();
            float x = forcePlayersInView ? EnsurePlayersVisible(CalculateTargetX()) : CalculateTargetX();
            Vector3 target = new Vector3(x, transform.position.y + followOffset.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, smoothTime);
        }

        float CalculateTargetX()
        {
            float midpoint = (player1.position.x + player2.position.x) / 2f + followOffset.x;
            float minX = _minX + _cameraHalfWidth;
            float maxX = _maxX - _cameraHalfWidth;
            return Mathf.Clamp(midpoint, minX, maxX);
        }

        void RecalculateCameraHalfWidth()
        {
            _cameraHalfWidth = mainCamera.orthographic
                ? mainCamera.orthographicSize * mainCamera.aspect
                : Mathf.Abs(mainCamera.transform.position.z - transform.position.z) * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * mainCamera.aspect;
        }

        void UpdateViewBoundaries()
        {
            _minX = GetBoundaryX(currentMinBoundaryIndex, -Mathf.Infinity);
            _maxX = GetBoundaryX(currentMaxBoundaryIndex, Mathf.Infinity);
        }

        float GetBoundaryX(int index, float fallback)
        {
            return (index >= 0 && index < sectionBoundaryPoints.Count && sectionBoundaryPoints[index] != null)
                ? sectionBoundaryPoints[index].position.x
                : fallback;
        }

        bool ValidateSetup()
        {
            if (!mainCamera) mainCamera = Camera.main;
            if (!mainCamera || !player1 || !player2 || sectionBoundaryPoints.Count < 2)
            {
                Debug.LogError("CameraController: Missing references or not enough boundary points.", this);
                enabled = false;
                return false;
            }

            foreach (var point in sectionBoundaryPoints)
            {
                if (!point)
                {
                    Debug.LogError("CameraController: Null boundary point detected.", this);
                    enabled = false;
                    return false;
                }
            }

            return true;
        }

        float EnsurePlayersVisible(float targetX)
        {
            float leftEdge = targetX - _cameraHalfWidth;
            float rightEdge = targetX + _cameraHalfWidth;

            float minPlayerX = Mathf.Min(player1.position.x, player2.position.x);
            float maxPlayerX = Mathf.Max(player1.position.x, player2.position.x);

            if (minPlayerX < leftEdge - 0.1f)
                targetX = minPlayerX + _cameraHalfWidth - followOffset.x;
            else if (maxPlayerX > rightEdge + 0.1f)
                targetX = maxPlayerX - _cameraHalfWidth - followOffset.x;

            return Mathf.Clamp(targetX, _minX + _cameraHalfWidth, _maxX - _cameraHalfWidth);
        }

        void HandleBarrierOpened(object index)
        {
            // int newMax = index + 1;
            int newMax = int.Parse(index.ToString());
            if (newMax > currentMinBoundaryIndex && newMax > currentMaxBoundaryIndex && newMax < sectionBoundaryPoints.Count)
            {
                currentMaxBoundaryIndex = newMax;
                UpdateViewBoundaries();
                _velocity = Vector3.zero;
            }
        }

        void HandlePlayersPassedBarrier(object inx)
        {
            int index = int.Parse(inx.ToString());
            
            if (index >= 0 && index < sectionBoundaryPoints.Count - 1)
            {
                currentMinBoundaryIndex = index;
                currentMaxBoundaryIndex = Mathf.Max(currentMaxBoundaryIndex, index + 1);
                UpdateViewBoundaries();
                _velocity = Vector3.zero;
            }
        }

        void HandleDarkPlayerStuck(object _)
        {
            if (_darkPlayerStuckHandled) return;
            _darkPlayerStuckHandled = true;
            ForceAdvanceSection();
        }

        public void ForceAdvanceSection()
        {
            if (currentMaxBoundaryIndex < sectionBoundaryPoints.Count - 1)
            {
                currentMinBoundaryIndex = currentMaxBoundaryIndex;
                currentMaxBoundaryIndex++;
                UpdateViewBoundaries();
                _velocity = Vector3.zero;
            }
        }
    }
}
