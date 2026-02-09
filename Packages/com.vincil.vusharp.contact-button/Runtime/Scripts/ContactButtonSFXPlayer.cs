using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;

namespace Vincil.VuSharp.ContactButton
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    internal class ContactButtonSFXPlayer : UdonSharpBehaviour
    {
        [SerializeField] AudioClip clickSFX;
        [SerializeField] AudioClip unclickSFX;
        AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void _OnButtonClicked()
        {
            if (audioSource != null && clickSFX != null)
            {
                audioSource.PlayOneShot(clickSFX);
            }
        }

        public void _OnButtonUnclicked()
        {
            if (audioSource != null && unclickSFX != null)
            {
                audioSource.PlayOneShot(unclickSFX);
            }
        }
    }
}
