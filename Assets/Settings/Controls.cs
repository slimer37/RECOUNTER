//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.3
//     from Assets/Settings/Controls.inputactions
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

public partial class @Controls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""97713440-d051-4e5e-8956-9edb11fddaa2"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""7b1f852e-55dd-4b56-ad60-22865b72953f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""64752557-5bae-451c-a489-029db6facc05"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""59b4db38-feed-405f-a15a-28c9310b3693"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""ccdb6eae-3c38-4818-8d49-975c7d6860d3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""1073556e-1566-4eec-a7ff-53eccb918781"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""ded73f70-f20f-45ac-abdb-f75ec7b1d11a"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a1a81e0d-022d-4ae4-b160-973c315a4a42"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""32ccdda9-d8d3-4c67-9eac-9c2511208dbf"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""7a881c62-76c9-49d5-bfa6-cb73ee25b447"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""bf98fdcf-22c8-4549-8ea1-f82296a62c2c"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""dddb2190-6b7a-45e7-8bf8-b9e7cdaf6197"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.1,y=0.1)"",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""240c50cb-c065-4da4-a1c1-408f592fdec8"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""31558455-caa3-4a00-9f1e-c88c17da3e20"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""efb58022-976b-4ddf-90d2-b276a7c20002"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Menu"",
            ""id"": ""7b1f7d52-99d0-4e30-99ca-54298d4b748c"",
            ""actions"": [
                {
                    ""name"": ""Exit"",
                    ""type"": ""Button"",
                    ""id"": ""c89f6f4d-01ad-49af-b099-8e4974d8e9ff"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""94188914-1bd9-4fe7-92a9-353a2e9ffa58"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Exit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Placement"",
            ""id"": ""655f113b-9a73-4e85-8079-6f3cb1a85c0c"",
            ""actions"": [
                {
                    ""name"": ""Hold Rotate"",
                    ""type"": ""Button"",
                    ""id"": ""e62c06e1-f19e-42ce-967f-e91e06473b89"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Start Place"",
                    ""type"": ""Button"",
                    ""id"": ""2e455372-520e-42e5-b474-83bb86d54f14"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Confirm Place"",
                    ""type"": ""Button"",
                    ""id"": ""c99b560e-2086-4a40-ab44-40037586da6d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Vertical Move"",
                    ""type"": ""Value"",
                    ""id"": ""5f7eadb8-63a8-4d80-a67e-0b5d62d5e771"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Lateral"",
                    ""type"": ""Value"",
                    ""id"": ""5ac4891a-f259-4fbf-909a-a1cb691d3829"",
                    ""expectedControlType"": ""Delta"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""0366e12e-1867-4a56-97ae-86bc3b49fa55"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hold Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e3aef1b9-81c8-459a-ac1e-6e4f9b72de6b"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Start Place"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bd42b12e-c37a-4366-9324-208e712e887e"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Confirm Place"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b1b7adad-dc67-4362-92c3-ee6156f56b94"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Vertical Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""53122f12-5c02-4e3e-ad04-e0ee909eef1f"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Lateral"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard + Mouse"",
            ""bindingGroup"": ""Keyboard + Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Look = m_Player.FindAction("Look", throwIfNotFound: true);
        m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
        // Menu
        m_Menu = asset.FindActionMap("Menu", throwIfNotFound: true);
        m_Menu_Exit = m_Menu.FindAction("Exit", throwIfNotFound: true);
        // Placement
        m_Placement = asset.FindActionMap("Placement", throwIfNotFound: true);
        m_Placement_HoldRotate = m_Placement.FindAction("Hold Rotate", throwIfNotFound: true);
        m_Placement_StartPlace = m_Placement.FindAction("Start Place", throwIfNotFound: true);
        m_Placement_ConfirmPlace = m_Placement.FindAction("Confirm Place", throwIfNotFound: true);
        m_Placement_VerticalMove = m_Placement.FindAction("Vertical Move", throwIfNotFound: true);
        m_Placement_Lateral = m_Placement.FindAction("Lateral", throwIfNotFound: true);
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

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Look;
    private readonly InputAction m_Player_Sprint;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Interact;
    public struct PlayerActions
    {
        private @Controls m_Wrapper;
        public PlayerActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Look => m_Wrapper.m_Player_Look;
        public InputAction @Sprint => m_Wrapper.m_Player_Sprint;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @Interact => m_Wrapper.m_Player_Interact;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Look.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Sprint.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprint;
                @Sprint.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprint;
                @Sprint.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprint;
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Interact.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Interact.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Interact.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Interact.started += instance.OnInteract;
                @Interact.performed += instance.OnInteract;
                @Interact.canceled += instance.OnInteract;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // Menu
    private readonly InputActionMap m_Menu;
    private IMenuActions m_MenuActionsCallbackInterface;
    private readonly InputAction m_Menu_Exit;
    public struct MenuActions
    {
        private @Controls m_Wrapper;
        public MenuActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Exit => m_Wrapper.m_Menu_Exit;
        public InputActionMap Get() { return m_Wrapper.m_Menu; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuActions set) { return set.Get(); }
        public void SetCallbacks(IMenuActions instance)
        {
            if (m_Wrapper.m_MenuActionsCallbackInterface != null)
            {
                @Exit.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnExit;
                @Exit.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnExit;
                @Exit.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnExit;
            }
            m_Wrapper.m_MenuActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Exit.started += instance.OnExit;
                @Exit.performed += instance.OnExit;
                @Exit.canceled += instance.OnExit;
            }
        }
    }
    public MenuActions @Menu => new MenuActions(this);

    // Placement
    private readonly InputActionMap m_Placement;
    private IPlacementActions m_PlacementActionsCallbackInterface;
    private readonly InputAction m_Placement_HoldRotate;
    private readonly InputAction m_Placement_StartPlace;
    private readonly InputAction m_Placement_ConfirmPlace;
    private readonly InputAction m_Placement_VerticalMove;
    private readonly InputAction m_Placement_Lateral;
    public struct PlacementActions
    {
        private @Controls m_Wrapper;
        public PlacementActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @HoldRotate => m_Wrapper.m_Placement_HoldRotate;
        public InputAction @StartPlace => m_Wrapper.m_Placement_StartPlace;
        public InputAction @ConfirmPlace => m_Wrapper.m_Placement_ConfirmPlace;
        public InputAction @VerticalMove => m_Wrapper.m_Placement_VerticalMove;
        public InputAction @Lateral => m_Wrapper.m_Placement_Lateral;
        public InputActionMap Get() { return m_Wrapper.m_Placement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlacementActions set) { return set.Get(); }
        public void SetCallbacks(IPlacementActions instance)
        {
            if (m_Wrapper.m_PlacementActionsCallbackInterface != null)
            {
                @HoldRotate.started -= m_Wrapper.m_PlacementActionsCallbackInterface.OnHoldRotate;
                @HoldRotate.performed -= m_Wrapper.m_PlacementActionsCallbackInterface.OnHoldRotate;
                @HoldRotate.canceled -= m_Wrapper.m_PlacementActionsCallbackInterface.OnHoldRotate;
                @StartPlace.started -= m_Wrapper.m_PlacementActionsCallbackInterface.OnStartPlace;
                @StartPlace.performed -= m_Wrapper.m_PlacementActionsCallbackInterface.OnStartPlace;
                @StartPlace.canceled -= m_Wrapper.m_PlacementActionsCallbackInterface.OnStartPlace;
                @ConfirmPlace.started -= m_Wrapper.m_PlacementActionsCallbackInterface.OnConfirmPlace;
                @ConfirmPlace.performed -= m_Wrapper.m_PlacementActionsCallbackInterface.OnConfirmPlace;
                @ConfirmPlace.canceled -= m_Wrapper.m_PlacementActionsCallbackInterface.OnConfirmPlace;
                @VerticalMove.started -= m_Wrapper.m_PlacementActionsCallbackInterface.OnVerticalMove;
                @VerticalMove.performed -= m_Wrapper.m_PlacementActionsCallbackInterface.OnVerticalMove;
                @VerticalMove.canceled -= m_Wrapper.m_PlacementActionsCallbackInterface.OnVerticalMove;
                @Lateral.started -= m_Wrapper.m_PlacementActionsCallbackInterface.OnLateral;
                @Lateral.performed -= m_Wrapper.m_PlacementActionsCallbackInterface.OnLateral;
                @Lateral.canceled -= m_Wrapper.m_PlacementActionsCallbackInterface.OnLateral;
            }
            m_Wrapper.m_PlacementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @HoldRotate.started += instance.OnHoldRotate;
                @HoldRotate.performed += instance.OnHoldRotate;
                @HoldRotate.canceled += instance.OnHoldRotate;
                @StartPlace.started += instance.OnStartPlace;
                @StartPlace.performed += instance.OnStartPlace;
                @StartPlace.canceled += instance.OnStartPlace;
                @ConfirmPlace.started += instance.OnConfirmPlace;
                @ConfirmPlace.performed += instance.OnConfirmPlace;
                @ConfirmPlace.canceled += instance.OnConfirmPlace;
                @VerticalMove.started += instance.OnVerticalMove;
                @VerticalMove.performed += instance.OnVerticalMove;
                @VerticalMove.canceled += instance.OnVerticalMove;
                @Lateral.started += instance.OnLateral;
                @Lateral.performed += instance.OnLateral;
                @Lateral.canceled += instance.OnLateral;
            }
        }
    }
    public PlacementActions @Placement => new PlacementActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard + Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
    }
    public interface IMenuActions
    {
        void OnExit(InputAction.CallbackContext context);
    }
    public interface IPlacementActions
    {
        void OnHoldRotate(InputAction.CallbackContext context);
        void OnStartPlace(InputAction.CallbackContext context);
        void OnConfirmPlace(InputAction.CallbackContext context);
        void OnVerticalMove(InputAction.CallbackContext context);
        void OnLateral(InputAction.CallbackContext context);
    }
}
