
using System;
using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Data;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Vincil.VUSharp.UI.ContactButton
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    internal class Contact3DButtonImplementation : Contact3DButton
    {
        GameObject _clickableButton;
        GameObject clickableButton
        {
            get
            {
                if (_clickableButton == null)
                {
                    _clickableButton = transform.Find("ButtonClickable").gameObject;
                    if (_clickableButton == null)
                    {
                        Debug.LogError($"[ContactButton] Couldn't find a child named ButtonClickable on button {gameObject.name}! This will break the button!", gameObject);
                    }
                }
                return _clickableButton;
            }
        }

        VRCContactReceiver _contactReceiver;
        VRCContactReceiver contactReceiver
        {
            get
            {
                if (_contactReceiver == null)
                {
                    _contactReceiver = GetComponent<VRCContactReceiver>();
                    if (_contactReceiver == null)
                    {
                        Debug.LogError($"[ContactButton] Couldn't find a VRCContactReceiver!", gameObject);
                    }
                }
                return _contactReceiver;
            }
        }

        Renderer _buttonRenderer;
        Renderer buttonRenderer
        {
            get
            {
                if (_buttonRenderer == null && clickableButton != null)
                {
                    _buttonRenderer = clickableButton.GetComponent<Renderer>();
                    if (_buttonRenderer == null)
                    {
                        Debug.LogError($"[ContactButton] Couldn't find a Renderer on the ButtonClickable child of button {gameObject.name}! This will break the button!", gameObject);
                    }
                }
                return _buttonRenderer;
            }
        }

        Collider _interactableCollider;
        Collider interactableCollider
        {
            get
            {
                if (_interactableCollider == null)
                {
                    _interactableCollider = GetComponent<Collider>();
                    if (_interactableCollider == null)
                    {
                        Debug.LogError($"[ContactButton] Couldn't find a Collider on button {gameObject.name}! This will break the button!", gameObject);
                    }
                }
                return _interactableCollider;
            }
        }

        Animator _buttonAnimator;
        Animator buttonAnimator
        {
            get
            {
                if (_buttonAnimator == null)
                {
                    _buttonAnimator = GetComponent<Animator>();
                    if (_buttonAnimator == null)
                    {
                        Debug.LogError($"[ContactButton] Couldn't find an Animator on button {gameObject.name}! This will break the button!", gameObject);
                    }
                }
                return _buttonAnimator;
            }
        }

        MaterialPropertyBlock _enabledMaterialPropertyBlock;
        MaterialPropertyBlock enabledMaterialPropertyBlock
        {
            get
            {
                if (_enabledMaterialPropertyBlock == null)
                {
                    _enabledMaterialPropertyBlock = new MaterialPropertyBlock();
                    _enabledMaterialPropertyBlock.SetColor("_Color", Color.black);
                    _enabledMaterialPropertyBlock.SetColor("_EmissionColor", enabledEmissiveColor);
                }
                return _enabledMaterialPropertyBlock;
            }
        }

        MaterialPropertyBlock _pressedMaterialPropertyBlock;
        MaterialPropertyBlock pressedMaterialPropertyBlock
        {
            get
            {
                if (_pressedMaterialPropertyBlock == null)
                {
                    _pressedMaterialPropertyBlock = new MaterialPropertyBlock();
                    _pressedMaterialPropertyBlock.SetColor("_Color", Color.black);
                    _pressedMaterialPropertyBlock.SetColor("_EmissionColor", pressedEmissiveColor);
                }
                return _pressedMaterialPropertyBlock;
            }
        }

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
        bool isUsingContact = false;

        bool isPlayingClickAnimation = false;

        //bool needsDisabling = false;

        int _buttonHighlightMaterialIndex = -1;
        int buttonHighlightMaterialIndex
        {
            get
            {
                if (_buttonHighlightMaterialIndex == -1)
                {
                    for (int i = 0; i < buttonRenderer.sharedMaterials.Length; i++)
                    {
                        if (buttonRenderer.sharedMaterials[i].name == "ButtonHighlight")
                        {
                            _buttonHighlightMaterialIndex = i;
                        }
                    }

                    if (buttonHighlightMaterialIndex == -1)
                    {
                        Debug.LogError($"[ContactButton] Couldn't find a material named ButtonHighlight on the ButtonClickable renderer for button {gameObject.name}!  This will break highlighting!", gameObject);
                        _buttonHighlightMaterialIndex = 0;
                    }
                }
                return _buttonHighlightMaterialIndex;
            }
        }

        readonly Vector3 buttonLocalFrontDirection = Vector3.back; // note: the button depth calcuations presume that button's thickness is along the z axis and are not tied to this variable

        readonly float buttonClickDepth = 0.015f;
        readonly float buttonReleaseDepth = 0.010f;
        readonly float buttonThickness = 0.05f;
        readonly float buttonMaxPressDepth = 0.022f;

        void Start()
        {
            isInVR = Networking.LocalPlayer.IsUserInVR();

            buttonAnimator.speed = 2f; // Speed up the animation so that it feels more responsive; the default animation is pretty slow otherwise
            buttonAnimator.enabled = !(isInVR && UseContact);

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

            isUsingContact = UseContact && isInVR;
            SetInteractable(Interactable);
        }

        public override void AddOnClickListener(UdonSharpBehaviour udonSharpBehaviour, string MethodToCallName)
        {
            onClickEventReceivers.Add(udonSharpBehaviour);
            onClickEventReceiverMethodNames.Add(MethodToCallName);
        }

        public override void AddOnClickListener(UdonBehaviour udonBehaviour, string MethodToCallName)
        {
            onClickEventReceivers.Add(udonBehaviour);
            onClickEventReceiverMethodNames.Add(MethodToCallName);
        }

        public override void AddOnReleaseListener(UdonSharpBehaviour udonSharpBehaviour, string MethodToCallName)
        {
            onUnclickEventReceivers.Add(udonSharpBehaviour);
            onUnclickEventReceiverMethodNames.Add(MethodToCallName);
        }

        public override void AddOnReleaseListener(UdonBehaviour udonBehaviour, string MethodToCallName)
        {
            onUnclickEventReceivers.Add(udonBehaviour);
            onUnclickEventReceiverMethodNames.Add(MethodToCallName);
        }

        protected override void SetInteractable(bool value)
        {
            if (!isClicked)
            {
                EnableButton(value);
            }
        }

        protected override void SetIsUsingContact(bool value)
        {
            isUsingContact = value && isInVR;
            UpdateColliderOrContactActive();
        }

        /// <summary>
        /// Only public to allow for looping with SendCustomEventDelayedFrames, not meant to be called outside of this script!
        /// </summary>

        public void _TrackContactSenderLoop()
        {
            if (contactSenderToTrack != null && contactSenderToTrack.isValid)
            {
                Vector3 heading = contactSenderToTrack.position - transform.position;

                Vector3 worldAxisDirection = transform.TransformDirection(buttonLocalFrontDirection);

                float signedDistance = Vector3.Dot(heading, worldAxisDirection);

                //assumes the button's local z axis is the axis that gets pressed; also assumes uniform scaling or that the button's thickness is only scaled by the local z scale
                //if these assumptions are not true, the button's behavior may be inconsistent with the visual representation
                signedDistance /= transform.lossyScale.z;
                
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
                else if (isClicked && buttonPressDepth <= buttonReleaseDepth)
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

                clickableButton.transform.localPosition = buttonLocalFrontDirection * -buttonPressDepth;

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
            UpdateColliderOrContactActive();

            if (value)
            {
                if(buttonRenderer == null)
                {
                    Debug.LogError($"buttonRenderer is null");
                }
                if (enabledMaterialPropertyBlock == null)
                {
                    Debug.LogError($"enabledMaterialPropertyBlock is null");
                }
                buttonRenderer.SetPropertyBlock(enabledMaterialPropertyBlock, buttonHighlightMaterialIndex);
            }
            else
            {
                clickableButton.transform.localPosition = Vector3.zero;
                contactSenderToTrack = null;
                buttonRenderer.SetPropertyBlock(null, buttonHighlightMaterialIndex);

                contactReceiver.enabled = false;

                interactableCollider.enabled = false;
            }
        }

        private void UpdateColliderOrContactActive()
        {
            contactReceiver.enabled = isUsingContact && Interactable;

            interactableCollider.enabled = !isUsingContact && !isPlayingClickAnimation && Interactable;
            if (interactableCollider.enabled)
            {
                buttonAnimator.enabled = true;
            }
            else if(!isPlayingClickAnimation)
            {
                buttonAnimator.enabled = false;
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

            if (Interactable)
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
            UpdateColliderOrContactActive();
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

            if (!isTrackingContact && Interactable && isUsingContact)
            {
                ContactSenderProxy contactSender = contactInfo.contactSender;
                if (contactSender.isValid && contactSender.player != null && contactSender.player.isLocal)
                {
                    //determin if the contact is coming from the front or back of the button; if it is coming from behind, disregard it
                    Vector3 heading = contactSender.position - transform.position;
                    Vector3 worldAxisDirection = transform.TransformDirection(buttonLocalFrontDirection);
                    float signedDistance = Vector3.Dot(heading, worldAxisDirection);
                    if (signedDistance < 0) return; // Contact is coming from behind the button, disregard it

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
