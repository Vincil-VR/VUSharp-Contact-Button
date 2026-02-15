using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Vincil.VUSharp.UI.ContactButton.Example
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactUIButtonDemo : UdonSharpBehaviour
    {
        [SerializeField] DemoButtonManager buttonManager;

        [SerializeField] ContactUIButton[] numberedButtons;
        [SerializeField] ContactUIButton[] optionsButtons;
        [SerializeField] ContactUIButton allowClickingButton;

        [SerializeField] TextMeshProUGUI buttonPressedTextField;       

        [SerializeField] GridLayoutGroup gridLayoutGroup;

        bool buttonsEnabled = true;

        private void Start()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            bool isInVR = Networking.LocalPlayer.IsUserInVR();
            foreach (Collider collider in colliders)
            {
                buttonManager.AddCanvasColliderToManager(collider);
                collider.enabled = !isInVR;
            }

            foreach(ContactUIButton button in numberedButtons)
            {
                buttonManager.AddButtonToManager(button);
            }
            foreach (ContactUIButton button in optionsButtons)
            {
                buttonManager.AddButtonToManager(button);
            }
            buttonManager.AddButtonToManager(allowClickingButton);
        }

        public void _OnMakeTallButtonClicked()
        {
            gridLayoutGroup.cellSize = new Vector2(100,200);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gridLayoutGroup.transform);
            foreach (ContactUIButton button in numberedButtons)
            {
                button._UpdateContactSize();
            }
        }

        public void _OnMakeWideButtonClicked()
        {
            gridLayoutGroup.cellSize = new Vector2(200, 100);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gridLayoutGroup.transform);
            foreach (ContactUIButton button in numberedButtons)
            {
                button._UpdateContactSize();
            }
        }

        public void _OnToggleButtonsEnabledButtonClicked()
        {
            buttonsEnabled = !buttonsEnabled;
            foreach (ContactUIButton button in numberedButtons)
            {
                button.Interactable = buttonsEnabled;
            }
            foreach (ContactUIButton button in optionsButtons)
            {
                button.Interactable = buttonsEnabled;
            }
        }

        public void _OnButton0Clicked()
        {
            SetButtonPressedText(0);
        }
        public void _OnButton1Clicked()
        {
            SetButtonPressedText(1);
        }
        public void _OnButton2Clicked()
        {
            SetButtonPressedText(2);
        }
        public void _OnButton3Clicked()
        {
            SetButtonPressedText(3);
        }
        public void _OnButton4Clicked()
        {
            SetButtonPressedText(4);
        }
        public void _OnButton5Clicked()
        {
            SetButtonPressedText(5);
        }

        private void SetButtonPressedText(int num)
        {
            buttonPressedTextField.text = $"Last Button Pressed: {num}";
        }
    }
}
