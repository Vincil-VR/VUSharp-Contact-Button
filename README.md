# VUSharp Contact Button

https://github.com/user-attachments/assets/a865d055-ab85-4f48-a215-838440a10699



A 3d, tactile, physically interactable button for VRChat Worlds designed as a near-equivalent replacement to a Unity UI Button.  

## Features

- Buttons that use [VRC Contacts](https://creators.vrchat.com/common-components/contacts/) to create physically clickable buttons

- Able to communicate with any UdonBehaviour/UdonSharpBehaviour using SendCustomEvent

- Freely position, rotate, and resize the buttons

- Haptic feedback

- Option to toggle to a fallback system that uses [Interact()](https://creators.vrchat.com/worlds/udon/graph/event-nodes/#interact)

- Non-VR users automatically use the fallback method

## How to install

### VRChat Package Manager

https://vincil-vr.github.io/VUSharp-Contact-Button/

### Unity Package Manager

In your Unity project, go to `Window > Package Manager` then click the top left `+`, click on `Add package from git URL` and paste this link:   

<https://github.com/Vincil-VR/VUSharp-Contact-Button/tree/main/Packages/com.vincil.vusharp.contact-button>  

### Unity Package

Download the latest package from the [latest release](https://github.com/Vincil-VR/VUSharp-Contact-Button/releases/latest)


Then import the contained .unitypackage


## Usage


1. In your Udon script, add a public method that takes no parameters that you want called when the button is clicked.  Optionally you can add a method that will be called when the button is released.

2. Right click in your scene's hierarchy and navigate to the Contact Buttons submenu.  Select the Contact Button of your choosing.

  > Import `TMP Essential Resources` if prompted.

3. Position the button however you want.

4. Select the spawned Contact Button and navigate to the ContactButton component (it should be at the top right under the transform component).  Click the "Add" button under "OnClick Listeners (UdonBehaviour & Method Names)."  In the left field place your Udon script.  In the right enter the name of the method you want called.  Repeat the process for any OnRelease listeners you might have.


## Button Objects

Currently there are two button to choose from: the default Contact Button and a Contact Button preconfigured to play sound when clicked.

## Inspector

<img width="482" height="352" alt="Screenshot 2026-02-12 152534" src="https://github.com/user-attachments/assets/97855e15-0bfa-4edb-9082-cd18786a34f8" />


### Serialized Fields

|        Property        | Result                                                                                                                                                                                           |
| :--------------------: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|      Interactable      | Sets if the button can be interacted with                                                                                                                                                        |
|      Use Haptics       | Sets if controller haptics should be used when the button is interacted with using contacts                                                                                                      |
|      Use Contact       | Sets if the button should use contacts.  If not it will use the classic collider "Interact" method.  Players not in VR will always use the collider "Interact" method regardless of this setting |
| Enabled Emissive Color | The emissive color the button's highlight will be when the button is enabled and not being pressed                                                                                               |
| Pressed Emissive Color | The emissive color the button's highlight will be when the button is being pressed                                                                                                               |
|    Interaction Text    | Text to display when using the fallback Interact() system.  Defaults to display nothing.                                                                                                         |
|       Proximity        | Interaction range for the fallback Interact() system.                                                                                                                                            |
|   OnClick Listeners    | Udon Behaviour / method name pairings that will be called when the button is clicked                                                                                                             |
|  OnRelease Listeners   | Udon Behaviour / method name pairings that will be called when the button is released                                                                                                            |

## Scripting


If using UdonSharp include `using Vincil.VUSharp.UI.ContactButton` and access the script as `ContactButton`

### Properties

|   Property   | Result                                                                                                                                                                                           |
| :----------: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Interactable | Sets if the button can be interacted with                                                                                                                                                        |
|  UseHaptics  | Sets if controller haptics should be used when the button is interacted with using contacts                                                                                                      |
|  UseContact  | Sets if the button should use contacts.  If not it will use the classic collider "Interact" method.  Players not in VR will always use the collider "Interact" method regardless of this setting |

### Functions

|       Function       | Input                                                          | Result                                                                                                    |
| :------------------: | :------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
|  AddOnClickListener  | UdonSharpBehaviour udonSharpBehaviour, string MethodToCallName | Adds the behaviour/method name pairing to list of methods that will be called when the button is clicked  |
|  AddOnClickListener  | UdonBehaviour udonBehaviour, string MethodToCallName           | Same as above                                                                                             |
| AddOnReleaseListener | UdonSharpBehaviour udonSharpBehaviour, string MethodToCallName | Adds the behaviour/method name pairing to list of methods that will be called when the button is released |
| AddOnReleaseListener | UdonBehaviour udonBehaviour, string MethodToCallName           | Same as above                                                                                             |


## Example:

In `Samples>SimpleExample` folder contains a scene with a simple demonstration of the buttons in use.

There is a button for iterating a counter, a button for disabling the prior button, and an additional button that toggles the other buttons between using contacts and the fallback system.

The script used in the example:
```c#
namespace Vincil.VUSharp.UI.ContactButton.Example
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactButtonDemo : UdonSharpBehaviour
    {
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
```
