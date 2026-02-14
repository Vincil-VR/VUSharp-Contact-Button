using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace Vincil.VUSharp.UI.ContactButton
{
    public abstract class ContactUIButton : ContactButton
    {
        public abstract void _UpdateContactSize();
    }
}