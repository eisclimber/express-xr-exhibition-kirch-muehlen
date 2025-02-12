using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using ExPresSXR.Rig;
using ExPresSXR.Editor.Utility;

namespace ExPresSXR.Editor.SetupDialogs
{
    class SetupDialogInitial : SetupDialogBase
    {
        public const string BASIC_SCENE_NAME = "Basic Scene (ExPresS XR)";
        public const string EXHIBITION_EXPORT_SCENE_NAME = "Exhibition Export Scene";
        public const string EXHIBITION_TUTORIAL_SCENE_NAME = "Exhibition Tutorial Scene";
        public const string EXPERIMENTATION_EXPORT_SCENE_NAME = "Experimentation Export Scene";
        public const string EXPERIMENTATION_TUTORIAL_SCENE_NAME = "Experimentation Tutorial Scene";
        public const string GENERAL_EXPORT_SCENE_NAME = "General Export Scene";
        public const string INTERACTION_TUTORIAL_SCENE_NAME = "Interaction Tutorial Scene";
        public const string MOBILE_EXPORT_SCENE_NAME = "Mobile Export Scene";
        public const string MOVEMENT_TUTORIAL_SCENE_NAME = "Movement Tutorial Scene";


        [MenuItem("ExPresS XR/Tutorials.../Initial Setup", false, 0)]
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            EditorWindow window = GetWindow<SetupDialogInitial>("Initial Setup");
            window.minSize = defaultWindowSize;
        }

        public override string uxmlName
        {
            get => "Assets/ExPresS XR/Editor/Setup Dialogs/Initial Setup/initial-setup.uxml";
        }

        private VisualElement step2Container;
        private VisualElement step3Container;
        private VisualElement step4Container;
        private VisualElement step5Container;


        [SerializeField]
        private DialogInputMethod _inputMethod;
        public DialogInputMethod inputMethod
        {
            get => _inputMethod;
            set
            {
                SwitchStepValue(step2Container, (int)_inputMethod, (int)value);

                _inputMethod = value;

                // Disable Controller Options for Eye/HeadGaze devices in the next container
                step3Container.Q<Button>("choice-1-button").SetEnabled(_inputMethod != DialogInputMethod.None);

                step3Container.Q<Button>("choice-2-button").SetEnabled(_inputMethod == DialogInputMethod.Controller);
                step3Container.Q<Button>("choice-3-button").SetEnabled(_inputMethod == DialogInputMethod.Controller);
                step3Container.Q<Button>("choice-4-button").SetEnabled(_inputMethod == DialogInputMethod.Controller);

                // Disable Step 3 if no controllers were used
                stepsContainer.Q<Button>("step-3").SetEnabled(_inputMethod == DialogInputMethod.Controller);
            }
        }

        [SerializeField]
        private DialogMovementPreset _movementPreset;
        public DialogMovementPreset movementPreset
        {
            get => _movementPreset;
            set
            {
                SwitchStepValue(step3Container, (int)_movementPreset, (int)value);

                _movementPreset = value;
            }
        }


        [SerializeField]
        private InteractionOptions _interactionOptions;
        public InteractionOptions interactionOptions
        {
            get => _interactionOptions;
            set
            {
                _interactionOptions = value;
            }
        }


        [SerializeField]
        private LaunchOption _launchOption;
        public LaunchOption launchOption
        {
            get => _launchOption;
            set
            {
                SwitchStepValue(step5Container, (int)_launchOption, (int)value);

                _launchOption = value;
            }
        }

        protected override void AssignStepContainersRefs()
        {
            step2Container = contentContainer.Q<VisualElement>("step-2-input-method");
            step3Container = contentContainer.Q<VisualElement>("step-3-controls-presets");
            step4Container = contentContainer.Q<VisualElement>("step-4-input-options");
            step5Container = contentContainer.Q<VisualElement>("step-5-further-steps");
        }

        // Expand this method and add bindings for each step
        protected override void BindUiElements()
        {
            // Add behavior the ui elements of each step
            BindStep2();
            BindStep3();
            BindStep4();
            BindStep5();

            // Bind remaining UI Elements
            base.BindUiElements();
        }

        private void BindStep2()
        {
            for (int i = 0; i < Enum.GetNames(typeof(DialogInputMethod)).Length; i++)
            {
                Button button = step2Container.Q<Button>($"choice-{i + 1}-button");
                if (button != null)
                {
                    // Move first entry ('None') to the last button
                    DialogInputMethod j = (DialogInputMethod)i;
                    button.clickable.clicked += () => { inputMethod = j; };
                    button.style.backgroundColor = j == _inputMethod ? Color.gray : Color.black;
                }
            }
        }

        private void BindStep3()
        {
            for (int i = 0; i < Enum.GetNames(typeof(DialogMovementPreset)).Length; i++)
            {
                Button button = step3Container.Q<Button>($"choice-{i + 1}-button");
                if (button != null)
                {
                    DialogMovementPreset j = (DialogMovementPreset)i;
                    button.clickable.clicked += () => { movementPreset = j; };
                    button.style.backgroundColor = j == movementPreset ? Color.gray : Color.black;
                }
            }

            contentContainer.Query<Button>("movement-next-button").ForEach((nextButton) =>
            {
                nextButton.clickable.clicked += () =>
                {
                    int nextStepDelta = inputMethod == DialogInputMethod.Controller ? 1 : 2;
                    currentStep += nextStepDelta;
                };
            });
        }

        private void BindStep4()
        {
            // Start at i=1 to ignore 'None'
            for (int i = 1; i < Enum.GetNames(typeof(InteractionOptions)).Length; i++)
            {
                Toggle toggle = step4Container.Q<Toggle>($"option-toggle-{i}");
                if (toggle != null)
                {
                    InteractionOptions j = (InteractionOptions)(1 << (i - 1));
                    toggle.value = interactionOptions.HasFlag(j);
                    toggle.RegisterValueChangedCallback(evt => { EnableInteractionOptionsFlag(j, evt.newValue); });
                }
            }
        }

        private void BindStep5()
        {
            for (int i = 0; i < Enum.GetNames(typeof(LaunchOption)).Length; i++)
            {
                Button button = step5Container.Q<Button>($"choice-{i + 1}-button");
                if (button != null)
                {
                    LaunchOption j = (LaunchOption)i;
                    button.clickable.clicked += () => { launchOption = j; };
                    button.style.backgroundColor = j == launchOption ? Color.gray : Color.black;
                }
            }

            contentContainer.Query<Button>("movement-back-button").ForEach((nextButton) =>
            {
                nextButton.clickable.clicked += () =>
                {
                    int nextStepDelta = inputMethod == DialogInputMethod.Controller ? 1 : 2;
                    currentStep -= nextStepDelta;
                };
            });
        }

        protected override void FinalizeSetup()
        {
            InputMethod input = DialogToNormalMovementPreset(inputMethod);
            MovementPreset preset = DialogToNormalMovementPreset(movementPreset);

            // Create Rig
            CreationUtils.InstantiateAndConfigureExPresSXRRig(input, preset, interactionOptions);

            // Show Tutorials
            ShowTutorialsSetupDialogs();

            // Open ProBuilder-Window
            EditorApplication.ExecuteMenuItem("Tools/ProBuilder/ProBuilder Window");

            // Close the editor window
            Close();
        }


        private void ShowTutorialsSetupDialogs()
        {
            if (launchOption == LaunchOption.Exhibition || launchOption == LaunchOption.Both)
            {
                SetupDialogExhibitionTutorial.ShowWindow();
            }

            if (launchOption == LaunchOption.Experimentation || launchOption == LaunchOption.Both)
            {
                SetupDialogExperimentationTutorial.ShowWindow();
            }
        }


        private void EnableInteractionOptionsFlag(InteractionOptions flagToChange, bool enableFlag)
        {
            if (enableFlag)
            {
                interactionOptions |= flagToChange;
            }
            else
            {
                interactionOptions &= ~flagToChange;
            }
        }


        public string GetSceneNameFromLaunchOption()
        {
            return launchOption switch
            {
                LaunchOption.Exhibition => EXHIBITION_TUTORIAL_SCENE_NAME,
                LaunchOption.Experimentation => EXPERIMENTATION_TUTORIAL_SCENE_NAME,
                LaunchOption.Both => GENERAL_EXPORT_SCENE_NAME,
                _ => BASIC_SCENE_NAME
            };
        }

        public MovementPreset DialogToNormalMovementPreset(DialogMovementPreset preset)
        {
            return preset switch
            {
                DialogMovementPreset.Teleport => MovementPreset.Teleport,
                DialogMovementPreset.Joystick => MovementPreset.Joystick,
                DialogMovementPreset.GrabWorldMotion => MovementPreset.GrabWorldMotion,
                DialogMovementPreset.GrabWorldManipulation => MovementPreset.GrabWorldManipulation,
                _ => MovementPreset.None
            };
        }

        public InputMethod DialogToNormalMovementPreset(DialogInputMethod input)
        {
            return input switch
            {
                DialogInputMethod.Controller => InputMethod.Controller,
                DialogInputMethod.HeadGaze => InputMethod.HeadGaze,
                DialogInputMethod.EyeGaze => InputMethod.EyeGaze,
                _ => InputMethod.None
            };
        }
    }

    public enum DialogInputMethod
    {
        Controller,
        HeadGaze,
        EyeGaze,
        None
    }


    public enum DialogMovementPreset
    {
        Teleport,
        Joystick,
        GrabWorldMotion,
        GrabWorldManipulation,
        None
    }

    public enum LaunchOption
    {
        Exhibition,
        Experimentation,
        Both,
        None
    }
}