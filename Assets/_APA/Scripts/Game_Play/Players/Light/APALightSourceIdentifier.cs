namespace _APA.Scripts
{
    using UnityEngine;

    public class LightSourceIdentifier : APAMonoBehaviour
    {
        public Transform SourceTransform { get; private set; }

        void Awake()
        {
            SourceTransform = transform;
        }
    }
}