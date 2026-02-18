
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Dynamics;
using VRC.SDK3.Data;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Vincil.VUSharp.UI.ContactButton
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    internal class ContactUIButtonImplementation : ContactUIButton
    {

        Button _uiButton;
        Button uiButton
        {
            get
            {
                if (_uiButton == null)
                {
                    _uiButton = GetComponent<Button>();
                    if (_uiButton == null)
                    {
                        Debug.LogError($"[ContactButton] Couldn't find a UI Button!", gameObject);
                    }
                }
                return _uiButton;
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
                        Debug.LogError($"[ContactButton] Couldn't find a Conctact Receiver!", gameObject);
                    }
                }
                return _contactReceiver;
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

        readonly Vector3 buttonLocalFrontDirection = Vector3.back; // note: the button depth calcuations presume that button's thickness is along the z axis and are not tied to this variable

        Color buttonBaseColor;
        Color buttonDisabledColor;


        readonly float buttonClickSensitivity = 0.001f;
        readonly float buttonReleaseSensitivity = 0.06f;

        void Start()
        {
            isInVR = Networking.LocalPlayer.IsUserInVR();

            buttonBaseColor = uiButton.colors.normalColor;
            buttonDisabledColor = uiButton.colors.disabledColor;

            if(uiButton.transition != Selectable.Transition.ColorTint)
            {
                Debug.LogWarning("[ContactButton] UI Contact button is attatched to a button with a transition not set to Color Tint.  Non-Color Tint transitions are currently not supported.");
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

            isUsingContact = UseContact && isInVR;
            SetInteractable(Interactable);

            _UpdateContactSize();
        }

        private void OnDisable()
        {
            if (isClicked)
            {
                OnUnclicked();
            }
            contactSenderToTrack = null;
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

        public override void _UpdateContactSize()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("[ContactButton] UI contact button isn't attatched to a rectTransform and cannot resize itself properly!");
                return;
            }
            // Get the corners of the RectTransform in world space
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // Calculate the width and height in world space
            float worldWidth = Vector3.Distance(corners[0], corners[3]);
            float worldHeight = Vector3.Distance(corners[0], corners[1]);

            float xSize = worldWidth / transform.lossyScale.x;
            float ySize = worldHeight / transform.lossyScale.y;
            contactReceiver.height = Mathf.Max(xSize, ySize);
            contactReceiver.radius = Mathf.Min(xSize, ySize) / 2;
            if (xSize > ySize)
            {
                contactReceiver.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            else
            {
                contactReceiver.rotation = Quaternion.identity;
            }
            contactReceiver.ApplyConfigurationChanges();
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

                if (!isClicked && signedDistance <= buttonClickSensitivity)
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
                else if (isClicked && signedDistance >= buttonReleaseSensitivity)
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

                SendCustomEventDelayedFrames(nameof(_TrackContactSenderLoop), 1);
            }
            else
            {
                isTrackingContact = false;
            }
        }

        private void EnableButton(bool value)
        {
            if (value)
            {
                UpdateColliderOrContactActive();
            }
            else
            {
                contactReceiver.enabled = false;
                var colors = uiButton.colors;
                colors.disabledColor = buttonDisabledColor;
                contactSenderToTrack = null;
                uiButton.colors = colors;
                uiButton.interactable = false;
            }
        }

        private void UpdateColliderOrContactActive()
        {
            if (isUsingContact)
            {
                contactReceiver.enabled = true;
                var colors = uiButton.colors;
                colors.disabledColor = buttonBaseColor;
                uiButton.colors = colors;
                uiButton.interactable = false;
            }
            else
            {
                contactReceiver.enabled = false;
                var colors = uiButton.colors;
                colors.disabledColor = buttonDisabledColor;
                uiButton.colors = colors;
                uiButton.interactable = true;
            }
        }

        private void OnClicked()
        {
            isClicked = true;
            if (isUsingContact)
            {
                var colors = uiButton.colors;
                colors.disabledColor = colors.pressedColor;
                uiButton.colors = colors;
            }
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
            var colors = uiButton.colors;
            if (isUsingContact)
            {
                colors.disabledColor = buttonBaseColor;
            }
            else
            {
                colors.disabledColor = buttonDisabledColor;
            }
            uiButton.colors = colors;
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

            }
            else
            {
                EnableButton(false);
            }
        }

        public void _OnUIButtonClicked()
        {
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
                contactSenderToTrack = null;
            }
        }
    }
}
