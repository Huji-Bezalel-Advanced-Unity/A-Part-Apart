namespace _APA.Scripts
{
    using UnityEngine;

    public class InteractableIdentifier
    {
        [Tooltip("The type of interaction this object represents.")]
        public InteractionType Type = InteractionType.None;
    }
}