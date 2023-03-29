//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.2
//     from Assets/Scripts/DesktopClient/DesktopInputControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @DesktopInputControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @DesktopInputControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DesktopInputControls"",
    ""maps"": [
        {
            ""name"": ""Desktop"",
            ""id"": ""5d335ec4-aea3-4189-aa84-486ce720de51"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""c6bb1e82-18e6-4f20-aab1-f17a1307c1c5"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MouseX"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d11f5256-c30c-4dd3-974f-7ce38a78369c"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseY"",
                    ""type"": ""PassThrough"",
                    ""id"": ""a48bf302-62f8-4dc5-ace9-24c63a498eec"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Menu"",
                    ""type"": ""Button"",
                    ""id"": ""80bd3820-8cc1-428c-9c04-7c76619d123f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Grab"",
                    ""type"": ""Button"",
                    ""id"": ""ae0ce3d1-ca31-4453-bcfc-bcda807aa9a2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseClick"",
                    ""type"": ""Button"",
                    ""id"": ""abd1d7d8-2e69-40e2-81d2-b9e923cee1cc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""45610ef8-fd7d-49ae-9089-4c0d9631ecf7"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""92aa4d9a-7fe2-41af-ba0c-66a2827b5ed0"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""2995da55-7750-43e5-adc3-28dda41a51ea"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""cd49333f-b369-415b-bf9d-3fbd22f05176"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""7a90d9ab-1529-4078-89b7-8ea87a9d4531"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""81dfc2f2-ad79-4e5f-b752-64f9206414df"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""71144b95-9007-4cd7-9732-9b88199aac7b"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""636e53da-d69e-4278-98ab-cb4f8a31cfc9"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""c9a9a7ed-4185-4b3b-8b47-dce56272a48f"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""457fc1d7-cba1-4b8f-abc4-7a0aab76653f"",
                    ""path"": ""<Mouse>/delta/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseX"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f8b83eab-75e3-40f2-9f3e-60c522864613"",
                    ""path"": ""<Mouse>/delta/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseY"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7faf2894-081c-4c2c-bf70-0626a30e17ff"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Menu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c8d215a-828f-4af6-9745-817782994535"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Grab"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e2130865-b3c3-4213-b21e-77720c8c5d53"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Desktop
        m_Desktop = asset.FindActionMap("Desktop", throwIfNotFound: true);
        m_Desktop_Move = m_Desktop.FindAction("Move", throwIfNotFound: true);
        m_Desktop_MouseX = m_Desktop.FindAction("MouseX", throwIfNotFound: true);
        m_Desktop_MouseY = m_Desktop.FindAction("MouseY", throwIfNotFound: true);
        m_Desktop_Menu = m_Desktop.FindAction("Menu", throwIfNotFound: true);
        m_Desktop_Grab = m_Desktop.FindAction("Grab", throwIfNotFound: true);
        m_Desktop_MouseClick = m_Desktop.FindAction("MouseClick", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Desktop
    private readonly InputActionMap m_Desktop;
    private IDesktopActions m_DesktopActionsCallbackInterface;
    private readonly InputAction m_Desktop_Move;
    private readonly InputAction m_Desktop_MouseX;
    private readonly InputAction m_Desktop_MouseY;
    private readonly InputAction m_Desktop_Menu;
    private readonly InputAction m_Desktop_Grab;
    private readonly InputAction m_Desktop_MouseClick;
    public struct DesktopActions
    {
        private @DesktopInputControls m_Wrapper;
        public DesktopActions(@DesktopInputControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Desktop_Move;
        public InputAction @MouseX => m_Wrapper.m_Desktop_MouseX;
        public InputAction @MouseY => m_Wrapper.m_Desktop_MouseY;
        public InputAction @Menu => m_Wrapper.m_Desktop_Menu;
        public InputAction @Grab => m_Wrapper.m_Desktop_Grab;
        public InputAction @MouseClick => m_Wrapper.m_Desktop_MouseClick;
        public InputActionMap Get() { return m_Wrapper.m_Desktop; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DesktopActions set) { return set.Get(); }
        public void SetCallbacks(IDesktopActions instance)
        {
            if (m_Wrapper.m_DesktopActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMove;
                @MouseX.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseX;
                @MouseX.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseX;
                @MouseX.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseX;
                @MouseY.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseY;
                @MouseY.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseY;
                @MouseY.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseY;
                @Menu.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMenu;
                @Menu.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMenu;
                @Menu.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMenu;
                @Grab.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnGrab;
                @Grab.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnGrab;
                @Grab.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnGrab;
                @MouseClick.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseClick;
                @MouseClick.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseClick;
                @MouseClick.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMouseClick;
            }
            m_Wrapper.m_DesktopActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @MouseX.started += instance.OnMouseX;
                @MouseX.performed += instance.OnMouseX;
                @MouseX.canceled += instance.OnMouseX;
                @MouseY.started += instance.OnMouseY;
                @MouseY.performed += instance.OnMouseY;
                @MouseY.canceled += instance.OnMouseY;
                @Menu.started += instance.OnMenu;
                @Menu.performed += instance.OnMenu;
                @Menu.canceled += instance.OnMenu;
                @Grab.started += instance.OnGrab;
                @Grab.performed += instance.OnGrab;
                @Grab.canceled += instance.OnGrab;
                @MouseClick.started += instance.OnMouseClick;
                @MouseClick.performed += instance.OnMouseClick;
                @MouseClick.canceled += instance.OnMouseClick;
            }
        }
    }
    public DesktopActions @Desktop => new DesktopActions(this);
    public interface IDesktopActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnMouseX(InputAction.CallbackContext context);
        void OnMouseY(InputAction.CallbackContext context);
        void OnMenu(InputAction.CallbackContext context);
        void OnGrab(InputAction.CallbackContext context);
        void OnMouseClick(InputAction.CallbackContext context);
    }
}