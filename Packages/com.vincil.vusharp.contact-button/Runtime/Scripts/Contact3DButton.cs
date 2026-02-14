using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Vincil.VUSharp.UI.ContactButton
{
    public abstract class Contact3DButton : ContactButton
    {
        [Tooltip("The emissive color the button's highlight will be when the button is enabled and not being pressed")]
        [ColorUsage(false, false)]
        [SerializeField] protected Color enabledEmissiveColor = Color.white;
        [Tooltip("The emissive color the button's highlight will be when the button is being pressed")]
        [ColorUsage(false, false)]
        [SerializeField] protected Color pressedEmissiveColor = Color.green;
    }
}
