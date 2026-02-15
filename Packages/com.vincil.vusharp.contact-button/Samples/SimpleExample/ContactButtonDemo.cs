
using TMPro;
using UdonSharp;
using UnityEngine;

namespace Vincil.VUSharp.UI.ContactButton.Example
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactButtonDemo : UdonSharpBehaviour
    {
        [SerializeField] DemoButtonManager buttonManager;
        [SerializeField] ContactButton allowClickingContactButton;
        [SerializeField] ContactButton clickerContactButton;
        [SerializeField] TextMeshProUGUI clickerButtonTextField;
        [SerializeField] TextMeshProUGUI toggleUsingContactButtonTextField;
        [SerializeField] TextMeshProUGUI demoTextField;

        int timesButtonClicked = 0;

        bool allowPlayerToClick = true;
        bool usingContacts = true;

        public void Start()
        {
            toggleUsingContactButtonTextField.text = "Stop Using Contacts";
            clickerButtonTextField.text = "I Can Be Clicked";
            demoTextField.text = $"Times button has been clicked:\n{timesButtonClicked}";
            buttonManager.AddButtonToManager(clickerContactButton);
            buttonManager.AddButtonToManager(allowClickingContactButton);
        }

        public void _OnAllowButtonClicked()
        {
            allowPlayerToClick = !allowPlayerToClick;
            clickerContactButton.Interactable = allowPlayerToClick;

            if (allowPlayerToClick)
            {
                clickerButtonTextField.text = "I Can Be Clicked";
            }
            else
            {
                clickerButtonTextField.text = "I Can't Be Clicked";
            }
        }

        public void _OnClickButtonClicked()
        {
            timesButtonClicked++;
            demoTextField.text = $"Times button has been clicked:\n{timesButtonClicked}";
        }

        public void _OnToggleUseContactButtonClicked()
        {
            usingContacts = !usingContacts;
            if (usingContacts)
            {
                toggleUsingContactButtonTextField.text = "Stop Using Contacts";
            }
            else
            {
                toggleUsingContactButtonTextField.text = "Start Using Contacts";
            }
            clickerContactButton.UseContact = usingContacts;
            allowClickingContactButton.UseContact = usingContacts;
        }
    }
}
