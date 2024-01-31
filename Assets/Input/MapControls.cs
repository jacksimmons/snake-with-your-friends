//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/Input/MapControls.inputactions
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

public partial class @MapControls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @MapControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MapControls"",
    ""maps"": [
        {
            ""name"": ""Edit"",
            ""id"": ""484eb4bf-ad29-41fc-aafe-dd381f3e1ed2"",
            ""actions"": [
                {
                    ""name"": ""Pan"",
                    ""type"": ""Value"",
                    ""id"": ""8e16e945-1ed1-4bf5-8a57-68d65d95b61f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""a3cd2eb9-3be8-4240-86e8-28b5522758c4"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Rotate"",
                    ""type"": ""Value"",
                    ""id"": ""a7d11355-714f-4a9a-9e79-1cb030ef9903"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""ChangeTool"",
                    ""type"": ""Value"",
                    ""id"": ""a244fe12-2ec8-411b-9607-4678fba40f80"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Draw"",
                    ""type"": ""Button"",
                    ""id"": ""b6360f9c-818b-4d54-8f81-c2d9932a398f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Erase"",
                    ""type"": ""Button"",
                    ""id"": ""c2c204e3-52f5-4c93-96c6-46630e56cd52"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""955611b5-ac1a-4dfd-8259-610ab37d5560"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""ChangeBrush"",
                    ""type"": ""Value"",
                    ""id"": ""be09dfbb-bdbb-4a5f-a8db-e21d6360f40b"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Tool_Brush"",
                    ""type"": ""Button"",
                    ""id"": ""479e20b0-4c58-4500-b24c-304ae2efed54"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Tool_Fill"",
                    ""type"": ""Button"",
                    ""id"": ""7c563971-3d0d-413b-a413-d2219f11f781"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Tool_Pick"",
                    ""type"": ""Button"",
                    ""id"": ""d63c2349-e6ed-4769-9f11-8d8860d67114"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""6231f1c5-180e-4676-8d76-a82b99fdb4a1"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""fc78831e-94ba-4c06-bd88-ceb1d903cd41"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""834c4e92-29e1-4295-b787-7e403c95546d"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""f9b6f898-2fc5-49f0-9f81-e4be43dac9b9"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""246b7d64-99de-4c01-8f14-506339a47e74"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e56ba717-e253-406a-8493-300be2f6c4d0"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""2966ab94-8f4e-4f66-a3de-982da717bd72"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""ece7a3d0-8560-4361-96e6-dff797793530"",
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
                    ""id"": ""89992c9d-7e02-4a4d-afad-fe9d442137ad"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""7124e757-c1ab-469a-bbef-2626742e1996"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e50fa028-4c42-41a0-b267-37e54b9641b8"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""2bd8411e-6049-4c5d-b80b-e4a8ff7410eb"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""d4beb8e9-da13-4b9a-831b-06e7dabe008f"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""23949e5c-d13b-4292-bb4f-f55848f2c95c"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""ceb8ca9e-734a-4f24-8674-409a39b8eb2a"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Controller"",
                    ""id"": ""6cc74be1-6260-4721-8eda-299f5082440d"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""46ae6c17-70ca-42bb-8f30-1a228f60c014"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""a052d626-9927-4d57-8913-05e4fbc3b2d3"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""ccdc675c-6ed1-4c53-9617-4de470c83f05"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeTool"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""646d2e52-cf8a-444a-b10d-a065101731d9"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeTool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""33cb3533-d524-4305-b923-6340ad4847d3"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeTool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Controller"",
                    ""id"": ""00d81ef8-4fa6-4792-9445-0bc9ad32ccaf"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeTool"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""737d3f7a-b0b2-4edc-9a39-76beaae69f80"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeTool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""25819f44-a607-4e28-ad1e-1aeaed71ce3e"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeTool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""305391fa-4b93-48b2-8f3f-9e9dea536cce"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Draw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""23138cb9-240d-4a8e-8a53-31a1e5488060"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Draw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f5283d8c-57ff-472e-8307-87fc8a02fc78"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Erase"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""751b1a97-0df4-472c-b1f8-f43b637e18d5"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Erase"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Controller"",
                    ""id"": ""699b335c-e4d1-46ee-aee5-594666952f52"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""0dbb9287-c04c-4485-b771-90fcc2687f22"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""2ee1132c-3df2-4ecd-ae98-b65fe11fe861"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""7e618dce-5bab-4af5-9e67-e181534048a8"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=0.001)"",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Controller"",
                    ""id"": ""83538f94-f21d-4c89-b2b8-495b27601e1e"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeBrush"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""7214edca-9b38-4761-bd9c-b10629368cce"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeBrush"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""9b964390-4d30-4dfd-8ba4-4defe32e152e"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeBrush"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""46818a3a-a8a6-414d-9ec3-2a017e104e72"",
                    ""path"": ""<Keyboard>/b"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tool_Brush"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""67f1d7db-7ebd-43c0-8ec2-69c64f7c3225"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tool_Fill"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5fcd1e95-be96-4ebd-8562-7c80d89c9ec7"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tool_Pick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Edit
        m_Edit = asset.FindActionMap("Edit", throwIfNotFound: true);
        m_Edit_Pan = m_Edit.FindAction("Pan", throwIfNotFound: true);
        m_Edit_Move = m_Edit.FindAction("Move", throwIfNotFound: true);
        m_Edit_Rotate = m_Edit.FindAction("Rotate", throwIfNotFound: true);
        m_Edit_ChangeTool = m_Edit.FindAction("ChangeTool", throwIfNotFound: true);
        m_Edit_Draw = m_Edit.FindAction("Draw", throwIfNotFound: true);
        m_Edit_Erase = m_Edit.FindAction("Erase", throwIfNotFound: true);
        m_Edit_Zoom = m_Edit.FindAction("Zoom", throwIfNotFound: true);
        m_Edit_ChangeBrush = m_Edit.FindAction("ChangeBrush", throwIfNotFound: true);
        m_Edit_Tool_Brush = m_Edit.FindAction("Tool_Brush", throwIfNotFound: true);
        m_Edit_Tool_Fill = m_Edit.FindAction("Tool_Fill", throwIfNotFound: true);
        m_Edit_Tool_Pick = m_Edit.FindAction("Tool_Pick", throwIfNotFound: true);
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

    // Edit
    private readonly InputActionMap m_Edit;
    private List<IEditActions> m_EditActionsCallbackInterfaces = new List<IEditActions>();
    private readonly InputAction m_Edit_Pan;
    private readonly InputAction m_Edit_Move;
    private readonly InputAction m_Edit_Rotate;
    private readonly InputAction m_Edit_ChangeTool;
    private readonly InputAction m_Edit_Draw;
    private readonly InputAction m_Edit_Erase;
    private readonly InputAction m_Edit_Zoom;
    private readonly InputAction m_Edit_ChangeBrush;
    private readonly InputAction m_Edit_Tool_Brush;
    private readonly InputAction m_Edit_Tool_Fill;
    private readonly InputAction m_Edit_Tool_Pick;
    public struct EditActions
    {
        private @MapControls m_Wrapper;
        public EditActions(@MapControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Pan => m_Wrapper.m_Edit_Pan;
        public InputAction @Move => m_Wrapper.m_Edit_Move;
        public InputAction @Rotate => m_Wrapper.m_Edit_Rotate;
        public InputAction @ChangeTool => m_Wrapper.m_Edit_ChangeTool;
        public InputAction @Draw => m_Wrapper.m_Edit_Draw;
        public InputAction @Erase => m_Wrapper.m_Edit_Erase;
        public InputAction @Zoom => m_Wrapper.m_Edit_Zoom;
        public InputAction @ChangeBrush => m_Wrapper.m_Edit_ChangeBrush;
        public InputAction @Tool_Brush => m_Wrapper.m_Edit_Tool_Brush;
        public InputAction @Tool_Fill => m_Wrapper.m_Edit_Tool_Fill;
        public InputAction @Tool_Pick => m_Wrapper.m_Edit_Tool_Pick;
        public InputActionMap Get() { return m_Wrapper.m_Edit; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(EditActions set) { return set.Get(); }
        public void AddCallbacks(IEditActions instance)
        {
            if (instance == null || m_Wrapper.m_EditActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_EditActionsCallbackInterfaces.Add(instance);
            @Pan.started += instance.OnPan;
            @Pan.performed += instance.OnPan;
            @Pan.canceled += instance.OnPan;
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Rotate.started += instance.OnRotate;
            @Rotate.performed += instance.OnRotate;
            @Rotate.canceled += instance.OnRotate;
            @ChangeTool.started += instance.OnChangeTool;
            @ChangeTool.performed += instance.OnChangeTool;
            @ChangeTool.canceled += instance.OnChangeTool;
            @Draw.started += instance.OnDraw;
            @Draw.performed += instance.OnDraw;
            @Draw.canceled += instance.OnDraw;
            @Erase.started += instance.OnErase;
            @Erase.performed += instance.OnErase;
            @Erase.canceled += instance.OnErase;
            @Zoom.started += instance.OnZoom;
            @Zoom.performed += instance.OnZoom;
            @Zoom.canceled += instance.OnZoom;
            @ChangeBrush.started += instance.OnChangeBrush;
            @ChangeBrush.performed += instance.OnChangeBrush;
            @ChangeBrush.canceled += instance.OnChangeBrush;
            @Tool_Brush.started += instance.OnTool_Brush;
            @Tool_Brush.performed += instance.OnTool_Brush;
            @Tool_Brush.canceled += instance.OnTool_Brush;
            @Tool_Fill.started += instance.OnTool_Fill;
            @Tool_Fill.performed += instance.OnTool_Fill;
            @Tool_Fill.canceled += instance.OnTool_Fill;
            @Tool_Pick.started += instance.OnTool_Pick;
            @Tool_Pick.performed += instance.OnTool_Pick;
            @Tool_Pick.canceled += instance.OnTool_Pick;
        }

        private void UnregisterCallbacks(IEditActions instance)
        {
            @Pan.started -= instance.OnPan;
            @Pan.performed -= instance.OnPan;
            @Pan.canceled -= instance.OnPan;
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Rotate.started -= instance.OnRotate;
            @Rotate.performed -= instance.OnRotate;
            @Rotate.canceled -= instance.OnRotate;
            @ChangeTool.started -= instance.OnChangeTool;
            @ChangeTool.performed -= instance.OnChangeTool;
            @ChangeTool.canceled -= instance.OnChangeTool;
            @Draw.started -= instance.OnDraw;
            @Draw.performed -= instance.OnDraw;
            @Draw.canceled -= instance.OnDraw;
            @Erase.started -= instance.OnErase;
            @Erase.performed -= instance.OnErase;
            @Erase.canceled -= instance.OnErase;
            @Zoom.started -= instance.OnZoom;
            @Zoom.performed -= instance.OnZoom;
            @Zoom.canceled -= instance.OnZoom;
            @ChangeBrush.started -= instance.OnChangeBrush;
            @ChangeBrush.performed -= instance.OnChangeBrush;
            @ChangeBrush.canceled -= instance.OnChangeBrush;
            @Tool_Brush.started -= instance.OnTool_Brush;
            @Tool_Brush.performed -= instance.OnTool_Brush;
            @Tool_Brush.canceled -= instance.OnTool_Brush;
            @Tool_Fill.started -= instance.OnTool_Fill;
            @Tool_Fill.performed -= instance.OnTool_Fill;
            @Tool_Fill.canceled -= instance.OnTool_Fill;
            @Tool_Pick.started -= instance.OnTool_Pick;
            @Tool_Pick.performed -= instance.OnTool_Pick;
            @Tool_Pick.canceled -= instance.OnTool_Pick;
        }

        public void RemoveCallbacks(IEditActions instance)
        {
            if (m_Wrapper.m_EditActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IEditActions instance)
        {
            foreach (var item in m_Wrapper.m_EditActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_EditActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public EditActions @Edit => new EditActions(this);
    public interface IEditActions
    {
        void OnPan(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnRotate(InputAction.CallbackContext context);
        void OnChangeTool(InputAction.CallbackContext context);
        void OnDraw(InputAction.CallbackContext context);
        void OnErase(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
        void OnChangeBrush(InputAction.CallbackContext context);
        void OnTool_Brush(InputAction.CallbackContext context);
        void OnTool_Fill(InputAction.CallbackContext context);
        void OnTool_Pick(InputAction.CallbackContext context);
    }
}