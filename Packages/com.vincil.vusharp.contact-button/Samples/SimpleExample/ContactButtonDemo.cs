
using TMPro;
using UdonSharp;
using UnityEngine;

namespace Vincil.VUSharp.UI.ContactButton.Example
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactButtonDemo : UdonSharpBehaviour
    {
        [SerializeField] ContactButton joinContactButton;
        [SerializeField] TextMeshProUGUI joinButtonTextField;
        [SerializeField] TextMeshProUGUI demoTextField;

        bool hasPlayerJoined = false;

        bool allowPlayerToJoin = true;

        public void Start()
        {
            joinButtonTextField.text = "Join";
            demoTextField.text = "Player can join game!";
        }

        public void _OnAllowJoinButtonClicked()
        {
            allowPlayerToJoin = !allowPlayerToJoin;
            joinContactButton.Interactable = allowPlayerToJoin;

            if (allowPlayerToJoin)
            {
                demoTextField.text = "Player can join game!";
            }
            else
            {
                demoTextField.text = "Player cannot join game!";
                joinButtonTextField.text = "Join";
                hasPlayerJoined = false;
            }
        }

        public void _OnJoinButtonClicked()
        {
            hasPlayerJoined = !hasPlayerJoined;
            if (hasPlayerJoined)
            {
                joinButtonTextField.text = "Leave";
                demoTextField.text = "Player has joined game!";
            }
            else
            {
                joinButtonTextField.text = "Join";
                demoTextField.text = "Player can join game!";
            }
        }
    }
}
