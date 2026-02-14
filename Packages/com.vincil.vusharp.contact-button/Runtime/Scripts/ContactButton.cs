
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace Vincil.VUSharp.UI.ContactButton
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ContactButton : UdonSharpBehaviour
    {
        [Tooltip("Sets if the button can be interacted with")]
        [SerializeField] bool interactable = true;
        /// <summary>
        /// Sets if the button can be interacted with
        /// </summary>
        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                SetInteractable(value);
            }
        }
        [Tooltip("Sets if controller haptics should be used when the button is interacted with using contacts")]
        [SerializeField] bool useHaptics = true;
        /// <summary>
        /// Sets if controller haptics should be used when the button is interacted with using contacts
        /// </summary>
        public bool UseHaptics
        {
            get => useHaptics;
            set => useHaptics = value;
        }
        [Tooltip("Sets if the button should use contacts.  If not it will use the classic collider \"Interact\" method.  Players not in VR will always use the collider \"Interact\" method regardless of this setting.")]
        [SerializeField] bool useContact = true;
        /// <summary>
        /// Sets if the button should use contacts.  If not it will use the classic collider "Interact" method.  Players not in VR will always use the collider "Interact" method regardless of this setting.
        /// </summary>
        public bool UseContact
        {
            get => useContact;
            set
            {
                useContact = value;
                SetIsUsingContact(value);
            }
        }

        [SerializeField, HideInInspector] protected UdonBehaviour[] onClickEventReceiversArray;
        [SerializeField, HideInInspector] protected string[] onClickEventReceiverMethodNamesArray;

        [SerializeField, HideInInspector] protected UdonBehaviour[] onUnclickEventReceiversArray;
        [SerializeField, HideInInspector] protected string[] onUnclickEventReceiverMethodNamesArray;
        public abstract void AddOnClickListener(UdonSharpBehaviour udonSharpBehaviour, string MethodToCallName);
        public abstract void AddOnClickListener(UdonBehaviour udonBehaviour, string MethodToCallName);
        public abstract void AddOnReleaseListener(UdonSharpBehaviour udonSharpBehaviour, string MethodToCallName);
        public abstract void AddOnReleaseListener(UdonBehaviour udonBehaviour, string MethodToCallName);

        protected abstract void SetInteractable(bool value);

        protected abstract void SetIsUsingContact(bool value);
    }
}
