using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Vincil.VUSharp.UI.ContactButton.Example
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DemoButtonManager : UdonSharpBehaviour
    {
        [SerializeField] TextMeshProUGUI toggleUsingContactsUIField;

        private bool isUsingContacts = true;

        private DataList managedButtons = new DataList();
        private DataList managedCanvasColliders = new DataList();

        public void AddButtonsToManager(ContactButton[] contactButtons)
        {
            foreach (ContactButton button in contactButtons)
            {
                managedButtons.Add(button);
            }
        }

        public void AddCanvasColliderToManager(Collider canvasCollider)
        {
            managedCanvasColliders.Add(canvasCollider);
        }

        public void _OnToggleUsingContactsButtonClicked()
        {
            isUsingContacts = !isUsingContacts;
            for (int i = managedButtons.Count - 1; i >= 0; i--)
            {
                ContactButton button = (ContactButton)managedButtons[i].Reference;
                if(button != null)
                {
                    button.UseContact = isUsingContacts;
                }
                else
                {
                    managedButtons.RemoveAt(i);
                }
            }
            for (int i = managedCanvasColliders.Count - 1; i >= 0; i--)
            {
                Collider collider = (Collider)managedCanvasColliders[i].Reference;
                if (collider != null)
                {
                    collider.enabled = !Networking.LocalPlayer.IsUserInVR() || !isUsingContacts;
                }
                else
                {
                    managedCanvasColliders.RemoveAt(i);
                }
            }
            toggleUsingContactsUIField.text = isUsingContacts ? "Stop Using Contacts" : "Start Using Contacts";
        }
    }
}
