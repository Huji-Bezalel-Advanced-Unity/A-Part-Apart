using UnityEngine;

namespace _APA.Scripts.Managers
{
    public class SplitSequenceManager : MonoBehaviour
    {
        public static SplitSequenceManager Instance { get; private set; }

        [Header("Audio")] [SerializeField] private AudioClip sequenceSound;
        [SerializeField] private AudioClip afterSound;
        private AudioSource audioSource;

        [Header("UI")] [SerializeField] private GameObject screenPanel;

        private bool isListeningForInput = false;
        private bool pressedArrow = false;
        private bool pressedDW = false;
        private bool afterSoundPlayed = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void ShowSplitScreen()
        {
            screenPanel.SetActive(true);
            PlaySequenceSound();
        }

        private void PlaySequenceSound()
        {
            audioSource.PlayOneShot(sequenceSound);
            Invoke(nameof(StartListeningForInput), 50);
        }

        private void StartListeningForInput()
        {
            isListeningForInput = true;
        }

        private void Update()
        {
            if (!isListeningForInput || afterSoundPlayed)
                return;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                pressedArrow = true;

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W))
                pressedDW = true;

            if (pressedArrow && pressedDW)
            {
                afterSoundPlayed = true;
                PlayAfterSoundAndClose();
            }
        }

        private void PlayAfterSoundAndClose()
        {
            isListeningForInput = false;
            audioSource.PlayOneShot(afterSound);
            Invoke(nameof(CloseScreen), afterSound.length);
        }

        private void CloseScreen()
        {
            screenPanel.SetActive(false);

            isListeningForInput = false;
            pressedArrow = false;
            pressedDW = false;
            afterSoundPlayed = false;
        }
        private void HandleShowScreenRequest(LightInteractionController player)
        {
            ShowSplitScreen();
        }

        private void OnEnable()
        {
            EventManager.OnShowStuckDecisionUI += HandleShowScreenRequest;
        }

        private void OnDisable()
        {
            EventManager.OnShowStuckDecisionUI -= HandleShowScreenRequest;
        }
        

    }
}