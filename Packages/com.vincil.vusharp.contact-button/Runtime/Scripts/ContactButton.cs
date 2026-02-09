
using System;
using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Vincil.VUSharp.UI.ContactButton
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactButton : UdonSharpBehaviour
    {
        [SerializeField] bool interactable = true;
        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                SetInteractable(value);
            }
        }

        [SerializeField] bool useHaptics = true;
        public bool UseHaptics
        {
            get => useHaptics;
            set => useHaptics = value;
        }

        [ColorUsage(false, false)]
        [SerializeField] Color enabledEmissiveColor = Color.white;
        [ColorUsage(false, false)]
        [SerializeField] Color pressedEmissiveColor = Color.green;

        [SerializeField] GameObject clickableButton;

        [SerializeField, HideInInspector] UdonBehaviour[] onClickEventReceiversArray;
        [SerializeField, HideInInspector] string[] onClickEventReceiverMethodNamesArray;

        [SerializeField, HideInInspector] UdonBehaviour[] onUnclickEventReceiversArray;
        [SerializeField, HideInInspector] string[] onUnclickEventReceiverMethodNamesArray;

        Renderer buttonRenderer;

        Collider interactableCollider;
        Animator buttonAnimator;

        MaterialPropertyBlock enabledMaterialPropertyBlock;
        MaterialPropertyBlock pressedMaterialPropertyBlock;

        DataList onClickEventReceivers = new DataList();
        DataList onClickEventReceiverMethodNames = new DataList();

        DataList onUnclickEventReceivers = new DataList();
        DataList onUnclickEventReceiverMethodNames = new DataList();

        ContactSenderProxy contactSenderToTrack;
        bool isUsingFinger = false;
        bool isUsingRightFinger = false;

        bool isTrackingContact = false;

        bool isClicked = false;

        bool isInVR = false;

        bool isPlayingClickAnimation = false;

        //bool needsDisabling = false;

        int buttonHighlightMaterialIndex = -1;
        readonly float buttonClickDepth = 0.015f;
        readonly float buttonThickness = 0.05f;
        readonly float buttonMaxPressDepth = 0.022f;

        void Start()
        {
            isInVR = Networking.LocalPlayer.IsUserInVR();

            interactableCollider = GetComponent<Collider>();
            buttonAnimator = GetComponent<Animator>();

            buttonAnimator.enabled = !isInVR;

            buttonRenderer = clickableButton.GetComponent<Renderer>();
            enabledMaterialPropertyBlock = new MaterialPropertyBlock();
            enabledMaterialPropertyBlock.SetColor("_Color", Color.black);
            enabledMaterialPropertyBlock.SetColor("_EmissionColor", enabledEmissiveColor);
            pressedMaterialPropertyBlock = new MaterialPropertyBlock();
            pressedMaterialPropertyBlock.SetColor("_Color", Color.black);
            pressedMaterialPropertyBlock.SetColor("_EmissionColor", pressedEmissiveColor);

            for (int i = 0; i < buttonRenderer.sharedMaterials.Length; i++)
            {
                if (buttonRenderer.sharedMaterials[i].name == "ButtonHighlight")
                {
                    buttonHighlightMaterialIndex = i;
                }
            }

            if (buttonHighlightMaterialIndex == -1)
            {
                Debug.LogError($"[ContactButton] Couldn't find a material named ButtonHighlight on the ButtonClickable renderer for button {gameObject.name}!  This will break highlighting!", gameObject);
                buttonHighlightMaterialIndex = 0;
            }

            if (onClickEventReceiversArray != null && onClickEventReceiversArray.Length > 0)
            {
                for (int i = 0; i < onClickEventReceiversArray.Length; i++)
                {
                    onClickEventReceivers.Add(onClickEventReceiversArray[i]);
                    onClickEventReceiverMethodNames.Add(onClickEventReceiverMethodNamesArray[i]);
                }
            }

            if (onUnclickEventReceiversArray != null && onUnclickEventReceiversArray.Length > 0)
            {
                for (int i = 0; i < onUnclickEventReceiversArray.Length; i++)
                {
                    onUnclickEventReceivers.Add(onUnclickEventReceiversArray[i]);
                    onUnclickEventReceiverMethodNames.Add(onUnclickEventReceiverMethodNamesArray[i]);
                }
            }

            SetInteractable(interactable);
        }

        public void AddOnClickListener(UdonSharpBehaviour udonSharpBehaviour, string MethodToCallName)
        {
            onClickEventReceivers.Add(udonSharpBehaviour);
            onClickEventReceiverMethodNames.Add(MethodToCallName);
        }

        public void AddOnClickListener(UdonBehaviour udonBehaviour, string MethodToCallName)
        {
            onClickEventReceivers.Add(udonBehaviour);
            onClickEventReceiverMethodNames.Add(MethodToCallName);
        }

        private void SetInteractable(bool value)
        {
            if (!isClicked)
            {
                EnableButton(value);
            }
        }


        /// <summary>
        /// Only public to allow for looping with SendCustomEventDelayedFrames, not meant to be called outside of this script!
        /// </summary>

        public void _TrackContactSenderLoop()
        {
            if (contactSenderToTrack != null && contactSenderToTrack.isValid)
            {
                Vector3 heading = contactSenderToTrack.position - transform.position;

                // Vector3.back in local space is where the button is facing 
                Vector3 worldAxisDirection = transform.TransformDirection(Vector3.back);

                float signedDistance = Vector3.Dot(heading, worldAxisDirection);

                float buttonPressDepth = Mathf.Clamp(buttonThickness - signedDistance, 0, buttonMaxPressDepth);

                if (!isClicked && buttonPressDepth >= buttonClickDepth)
                {
                    if (UseHaptics && isUsingFinger)
                    {
                        if (isUsingRightFinger)
                        {
                            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.3f, 0.4f, .5f);
                        }
                        else
                        {
                            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.3f, 0.4f, .5f);
                        }
                        
                    }
                    OnClicked();
                }
                else if (isClicked && buttonPressDepth < buttonClickDepth)
                {
                    if (UseHaptics && isUsingFinger)
                    {
                        if (isUsingRightFinger)
                        {
                            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.1f, 0.25f, 0.25f);
                        }
                        else
                        {
                            Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.1f, 0.25f, 0.25f);
                        }

                    }
                    OnUnclicked();
                }

                clickableButton.transform.localPosition = new Vector3(0, 0, buttonPressDepth);

                SendCustomEventDelayedFrames(nameof(_TrackContactSenderLoop), 1);
            }
            else
            {
                clickableButton.transform.localPosition = Vector3.zero;
                isTrackingContact = false;
            }
        }

        private void EnableButton(bool value)
        {
            interactableCollider.enabled = !isInVR && !isPlayingClickAnimation && value;

            if (value)
            {
                buttonRenderer.SetPropertyBlock(enabledMaterialPropertyBlock, buttonHighlightMaterialIndex);
            }
            else
            {
                Debug.Log($"[ContactButton] Zeroing button");
                clickableButton.transform.localPosition = Vector3.zero;
                contactSenderToTrack = null;
                buttonRenderer.SetPropertyBlock(null, buttonHighlightMaterialIndex);
            }
        }

        private void OnClicked()
        {
            isClicked = true;
            buttonRenderer.SetPropertyBlock(pressedMaterialPropertyBlock, buttonHighlightMaterialIndex);
            for (int i = onClickEventReceivers.Count - 1; i >= 0; i--)
            {
                IUdonEventReceiver eventReceiver = (IUdonEventReceiver)onClickEventReceivers[i].Reference;
                string methodName = onClickEventReceiverMethodNames[i].String;
                if (eventReceiver != null && !string.IsNullOrEmpty(methodName))
                {
                    eventReceiver.SendCustomEvent(methodName);
                }
                else
                {
                    onClickEventReceivers.RemoveAt(i);
                    onClickEventReceiverMethodNames.RemoveAt(i);
                }
            }
        }

        private void OnUnclicked()
        {
            isClicked = false;
            for (int i = onUnclickEventReceivers.Count - 1; i >= 0; i--)
            {
                IUdonEventReceiver eventReceiver = (IUdonEventReceiver)onUnclickEventReceivers[i].Reference;
                string methodName = onUnclickEventReceiverMethodNames[i].String;
                if (eventReceiver != null && !string.IsNullOrEmpty(methodName))
                {
                    eventReceiver.SendCustomEvent(methodName);
                }
                else
                {
                    onUnclickEventReceivers.RemoveAt(i);
                    onUnclickEventReceiverMethodNames.RemoveAt(i);
                }
            }

            if (interactable)
            {
                buttonRenderer.SetPropertyBlock(enabledMaterialPropertyBlock, buttonHighlightMaterialIndex);
            }
            else
            {
                EnableButton(false);
            }
        }

        /// <summary>
        /// Used for animation event; do not use
        /// </summary>
        public void _OnClickedAnimationEvent()
        {
            OnClicked();
        }

        /// <summary>
        /// Used for animation event; do not use
        /// </summary>
        public void _OnUnClickedAnimationEvent()
        {
            OnUnclicked();
        }

        /// <summary>
        /// Used for animation event; do not use
        /// </summary>
        public void _OnAnimationEndedAnimationEvent()
        {
            isPlayingClickAnimation = false;
            if (interactable)
            {
                interactableCollider.enabled = true;
            }
        }

        /// <summary>
        /// VRChat Event; do not use
        /// </summary>
        public override void Interact()
        {
            isPlayingClickAnimation = true;
            interactableCollider.enabled = false;
            buttonAnimator.SetTrigger("Interact");
        }

        /// <summary>
        /// VRChat Event; do not use
        /// </summary>
        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            if (!isInVR) return; // ContactReceiver should only used when player in VR, so ignore contact events if not in VR

            if (!isTrackingContact && Interactable)
            {
                ContactSenderProxy contactSender = contactInfo.contactSender;
                if (contactSender.isValid && contactSender.player != null && contactSender.player.isLocal)
                {
                    contactSenderToTrack = contactSender;
                    string[] matchingTags = contactInfo.matchingTags;
                    isUsingFinger = Array.IndexOf(contactInfo.matchingTags, "Finger") >= 0;
                    if (isUsingFinger)
                    {
                        isUsingRightFinger = Array.IndexOf(contactInfo.matchingTags, "FingerR") >= 0;
                    }                    
                    isTrackingContact = true;
                    _TrackContactSenderLoop();
                }
            }
        }
        /// <summary>
        /// VRChat Event; do not use
        /// </summary>
        public override void OnContactExit(ContactExitInfo contactInfo)
        {
            if (!isInVR) return; // ContactReceiver should only used when player in VR, so ignore contact events if not in VR

            if (contactInfo.contactSender == contactSenderToTrack)
            {
                if (isClicked)
                {
                    OnUnclicked();
                }
                clickableButton.transform.localPosition = Vector3.zero;
                contactSenderToTrack = null;
            }
        }
    }
}
