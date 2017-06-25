﻿//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Buttons;
using HUX.Receivers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HUX.Interaction
{
    public class AppBar : InteractionReceiver
    {
        /// <summary>
        /// How many custom buttons can be added to the toolbar
        /// </summary>
        public const int MaxCustomButtons = 5;

        /// <summary>
        /// Class used for building toolbar buttons
        /// (not yet in use)
        /// </summary>
        [Serializable]
        public struct ButtonTemplate
        {
            public ButtonTemplate(ButtonTypeEnum type, string name, string icon, string text, int defaultPosition, int manipulationPosition) {
                Type = type;
                Name = name;
                Icon = icon;
                Text = text;
                DefaultPosition = defaultPosition;
                ManipulationPosition = manipulationPosition;
                EventTarget = null;
                OnTappedEvent = null;
            }

            public bool IsEmpty {
                get {
                    return string.IsNullOrEmpty(Name);
                }
            }

            public int DefaultPosition;
            public int ManipulationPosition;
            public ButtonTypeEnum Type;
            public string Name;
            public string Icon;
            public string Text;
            public InteractionReceiver EventTarget;
            public UnityEvent OnTappedEvent;
        }

        [Flags]
        public enum ButtonTypeEnum
        {
            Custom = 0,
            Remove = 1,
            Adjust = 2,
            Hide = 4,
            Show = 8,
            Done = 16,
        }

        public enum AppBarDisplayTypeEnum
        {
            Manipulation,
            Standalone,
        }

        public enum AppBarStateEnum
        {
            Default,
            Manipulation,
            Hidden,
        }

        public BoundingBoxManipulate BoundingBox {
            get {
                return boundingBox;
            }
            set {
                boundingBox = value;
            }
        }

        public GameObject SquareButtonPrefab;

        public int NumDefaultButtons {
            get {
                return numDefaultButtons;
            }
        }

        public int NumManipulationButtons {
            get {
                return numManipulationButtons;
            }
        }

        public bool UseRemove = true;
        public bool UseAdjust = true;
        public bool UseHide = true;

        public ButtonTemplate[] Buttons {
            get {
                return buttons;
            } set {
                buttons = value;
            }
        }

        public ButtonTemplate[] DefaultButtons {
            get {
                return defaultButtons;
            }
        }

        public AppBarDisplayTypeEnum DisplayType = AppBarDisplayTypeEnum.Manipulation;

        public AppBarStateEnum State = AppBarStateEnum.Default;

        /// <summary>
        /// Custom icon profile
        /// If null, the profile in the SquareButtonPrefab object will be used
        /// </summary>
        public ButtonIconProfile CustomButtonIconProfile;

        [SerializeField]
        private ButtonTemplate[] buttons = new ButtonTemplate[MaxCustomButtons];

        [SerializeField]
        private Transform buttonParent;

        [SerializeField]
        private GameObject baseRenderer;

        [SerializeField]
        private GameObject backgroundBar;

        [SerializeField]
        private BoundingBoxManipulate boundingBox;

        public void Reset() {
            State = AppBarStateEnum.Default;
            if (boundingBox != null) {
                boundingBox.AcceptInput = false;
            }
            FollowBoundingBox(false);
            lastTimeTapped = Time.time + coolDownTime;
        }

        public void Start() {
            State = AppBarStateEnum.Default;
            if (Interactibles.Count == 0) {
                RefreshTemplates();
                for (int i = 0; i < defaultButtons.Length; i++) {
                    CreateButton(defaultButtons[i], null);
                }
                for (int i = 0; i < buttons.Length; i++) {
                    CreateButton(buttons[i], CustomButtonIconProfile);
                }
            }
        }

        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs) {
            if (Time.time < lastTimeTapped + coolDownTime)
                return;

            lastTimeTapped = Time.time;

            base.OnTapped(obj, eventArgs);

            switch (obj.name) {
                case "Remove":
                    // Destroy the target object
                    GameObject.Destroy(boundingBox.Target);
                    // Set our bounding box to null so we'll disappear
                    boundingBox = null;
                    break;

                case "Adjust":
                    // Make the bounding box active so users can manipulate it
                    State = AppBarStateEnum.Manipulation;
                    break;

                case "Hide":
                    // Make the bounding box inactive and invisible
                    State = AppBarStateEnum.Hidden;
                    break;

                case "Show":
                    State = AppBarStateEnum.Default;
                    break;

                case "Done":
                    State = AppBarStateEnum.Default;
                    break;

                default:
                    break;
            }
        }

        private void CreateButton(ButtonTemplate template, ButtonIconProfile customIconProfile) {
            if (template.IsEmpty)
                return;

            // TODO find a less inelegant way to do this
            switch (template.Type) {
                case ButtonTypeEnum.Custom:
                    numDefaultButtons++;
                    break;

                case ButtonTypeEnum.Adjust:
                    numDefaultButtons++;
                    break;

                case ButtonTypeEnum.Done:
                    numManipulationButtons++;
                    break;

                case ButtonTypeEnum.Remove:
                    numManipulationButtons++;
                    numDefaultButtons++;
                    break;

                case ButtonTypeEnum.Hide:
                    numDefaultButtons++;
                    break;

                case ButtonTypeEnum.Show:
                    numHiddenButtons++;
                    break;
            }

            GameObject newButton = GameObject.Instantiate(SquareButtonPrefab, buttonParent);
            newButton.name = template.Name;
            newButton.transform.localPosition = Vector3.zero;
            newButton.transform.localRotation = Quaternion.identity;
            AppBarButton mtb = newButton.AddComponent<AppBarButton>();
            mtb.Initialize(this, template, customIconProfile);

            RegisterInteractible(newButton);
        }

        private void FollowBoundingBox(bool smooth) {
            if (boundingBox == null) {
                if (DisplayType == AppBarDisplayTypeEnum.Manipulation)
                {
                    // Hide our buttons
                    baseRenderer.SetActive(false);
                } else
                {
                    baseRenderer.SetActive(true);
                }
                return;
            }

            // Show our buttons
            baseRenderer.SetActive(true);

            // Get positions for each side of the bounding box
            // Choose the one that's closest to us
            forwards[0] = boundingBox.transform.forward;
            forwards[1] = boundingBox.transform.right;
            forwards[2] = -boundingBox.transform.forward;
            forwards[3] = -boundingBox.transform.right;
            Vector3 scale = boundingBox.TargetBoundsLocalScale;
            float maxXYScale = Mathf.Max(scale.x, scale.y);
            float closestSoFar = Mathf.Infinity;
            Vector3 finalPosition = Vector3.zero;
            Vector3 headPosition = Camera.main.transform.position;

            for (int i = 0; i < forwards.Length; i++) {
                Vector3 nextPosition = boundingBox.transform.position +
                (forwards[i] * -maxXYScale) +
                (Vector3.up * (-scale.y * 0.25f));

                float distance = Vector3.Distance(nextPosition, headPosition);
                if (distance < closestSoFar) {
                    closestSoFar = distance;
                    finalPosition = nextPosition;
                }
            }

            // Follow our bounding box
            if (smooth) {
                transform.position = Vector3.Lerp(transform.position, finalPosition, 0.5f);
            } else {
                transform.position = finalPosition;
            }
            // Rotate on the y axis
            Vector3 eulerAngles = Quaternion.LookRotation((boundingBox.transform.position - finalPosition).normalized, Vector3.up).eulerAngles;
            eulerAngles.x = 0f;
            eulerAngles.z = 0f;
            transform.eulerAngles = eulerAngles;
        }

        private void Update() {
            FollowBoundingBox(true);
            
            switch (State) {
                case AppBarStateEnum.Default:
                default:
                    targetBarSize = new Vector3 (numDefaultButtons, 1f, 1f);
                    if (boundingBox != null)
                        boundingBox.AcceptInput = false;
                    break;

                case AppBarStateEnum.Hidden:
                    targetBarSize = new Vector3(numHiddenButtons, 1f, 1f);
                    if (boundingBox != null)
                        boundingBox.AcceptInput = false;
                    break;

                case AppBarStateEnum.Manipulation:
                    targetBarSize = new Vector3(numManipulationButtons, 1f, 1f);
                    if (boundingBox != null)
                        boundingBox.AcceptInput = true;
                    break;
            }

            backgroundBar.transform.localScale = Vector3.Lerp(backgroundBar.transform.localScale, targetBarSize, 0.5f);
        }

        private void RefreshTemplates () {
            int numCustomButtons = 0;
            for (int i = 0; i < buttons.Length; i++) {
                if (!buttons[i].IsEmpty) {
                    numCustomButtons++;
                }
            }
            List<ButtonTemplate> defaultButtonsList = new List<ButtonTemplate>();
            // Create our default button templates based on user preferences
            if (UseRemove) {
                defaultButtonsList.Add(GetDefaultButtonTemplateFromType(ButtonTypeEnum.Remove, numCustomButtons, UseHide, UseAdjust, UseRemove));
            }
            if (UseAdjust) {
                defaultButtonsList.Add(GetDefaultButtonTemplateFromType(ButtonTypeEnum.Adjust, numCustomButtons, UseHide, UseAdjust, UseRemove));
                defaultButtonsList.Add(GetDefaultButtonTemplateFromType(ButtonTypeEnum.Done, numCustomButtons, UseHide, UseAdjust, UseRemove));
            }
            if (UseHide) {
                defaultButtonsList.Add(GetDefaultButtonTemplateFromType(ButtonTypeEnum.Hide, numCustomButtons, UseHide, UseAdjust, UseRemove));
                defaultButtonsList.Add(GetDefaultButtonTemplateFromType(ButtonTypeEnum.Show, numCustomButtons, UseHide, UseAdjust, UseRemove));
            }
            defaultButtons = defaultButtonsList.ToArray();
        }

#if UNITY_EDITOR
        public void EditorRefreshTemplates () {
            RefreshTemplates();
        }

        protected override void OnDrawGizmos() {
            if (Application.isPlaying)
                return;

            FollowBoundingBox(true);
        }
#endif

        private ButtonTemplate[] defaultButtons;
        private Vector3[] forwards = new Vector3[4];
        private Vector3 targetBarSize = Vector3.one;
        private float lastTimeTapped = 0f;
        private float coolDownTime = 0.5f;
        private int numDefaultButtons;
        private int numHiddenButtons;
        private int numManipulationButtons;

        /// <summary>
        /// Generates a template for a default button based on type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ButtonTemplate GetDefaultButtonTemplateFromType(ButtonTypeEnum type, int numCustomButtons, bool useHide, bool useAdjust, bool useRemove) {
            // Button position is based on custom buttons
            // In the app bar, Hide/Show
            switch (type) {
                default:
                    return new ButtonTemplate(
                        ButtonTypeEnum.Custom,
                        "Custom",
                        "",
                        "Custom",
                        0,
                        0);

                case ButtonTypeEnum.Adjust:
                    int adjustPosition = numCustomButtons + 1;
                    if (!useHide) {
                        adjustPosition--;
                    }
                    return new ButtonTemplate(
                        ButtonTypeEnum.Adjust,
                        "Adjust",
                        "EBD2",
                        "Adjust",
                        adjustPosition, // Always next-to-last to appear
                        0);

                case ButtonTypeEnum.Done:
                    return new ButtonTemplate(
                        ButtonTypeEnum.Done,
                        "Done",
                        "E8FB",
                        "Done",
                        0,//-2,
                        0);//-1);

                case ButtonTypeEnum.Hide:
                    return new ButtonTemplate(
                        ButtonTypeEnum.Hide,
                        "Hide",
                        "E76C",
                        "Hide Menu",
                        0,// Always the first to appear
                        0);

                case ButtonTypeEnum.Remove:
                    int removePosition = numCustomButtons + 1;
                    if (useAdjust) {
                        removePosition++;
                    }
                    if (!useHide) {
                        removePosition--;
                    }
                    return new ButtonTemplate(
                        ButtonTypeEnum.Remove,
                        "Remove",
                        "EC90",
                        "Remove",
                        removePosition, // Always the last to appear
                        1);

                case ButtonTypeEnum.Show:
                    return new ButtonTemplate(
                        ButtonTypeEnum.Show,
                        "Show",
                        "E700",
                        //"EC90",
                        "Show Menu",
                        0,//-2,
                        0);
            }
        }
    }
}
