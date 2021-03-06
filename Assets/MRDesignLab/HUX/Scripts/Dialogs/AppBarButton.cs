//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Buttons;
using UnityEngine;

namespace HUX.Interaction
{
    public class AppBarButton : MonoBehaviour
    {
        public const float ButtonWidth = 0.08f;
        public const float ButtonDepth = -0.0001f;
        const float MoveSpeed = 5f;

        public FilterTag VisibleFilterTag;
        public FilterTag HiddenFilterTag;

        public void Initialize (AppBar newParentToolBar, AppBar.ButtonTemplate newTemplate, ButtonIconProfile newCustomProfile)
        {
            // TODO move this into the tag manager
            VisibleFilterTag = new FilterTag();
            VisibleFilterTag.Tag = "Default";
            HiddenFilterTag = new FilterTag();
            HiddenFilterTag.Tag = "Hidden";

            template = newTemplate;
            customIconProfile = newCustomProfile;
            parentToolBar = newParentToolBar;

            cButton = GetComponent<CompoundButton>();
            cButton.MainRenderer.enabled = false;
            text = GetComponent<CompoundButtonText>();
            text.Text = template.Text;
            icon = GetComponent<CompoundButtonIcon>();
            if (customIconProfile != null)
            {
                icon.IconProfile = customIconProfile;
                icon.IconName = string.Empty;
            }
            icon.IconName = template.Icon;
            initialized = true;
            Hide();
        }

        protected void OnEnable ()
        {
            Hide();
        }
        
        protected void Update()
        {
            if (!initialized)
                return;

            RefreshOffsets();

            switch (parentToolBar.State)
            {
                case AppBar.AppBarStateEnum.Default:
                    // Show hide, adjust, remove buttons
                    // The rest are hidden
                    targetPosition = defaultOffset;
                    switch (template.Type)
                    {
                        case AppBar.ButtonTypeEnum.Hide:
                        case AppBar.ButtonTypeEnum.Remove:
                        case AppBar.ButtonTypeEnum.Adjust:
                        case AppBar.ButtonTypeEnum.Custom:
                            Show();
                            break;

                        default:
                            Hide();
                            break;
                    }
                    break;

                case AppBar.AppBarStateEnum.Hidden:
                    // Show show button
                    // The rest are hidden
                    targetPosition = hiddenOffset;
                    switch (template.Type)
                    {
                        case AppBar.ButtonTypeEnum.Show:
                            Show();
                            break;

                        default:
                            Hide();
                            break;
                    }
                    break;

                case AppBar.AppBarStateEnum.Manipulation:
                    // Show done / remove buttons
                    // The rest are hidden
                    targetPosition = manipulationOffset;
                    switch (template.Type)
                    {
                        case AppBar.ButtonTypeEnum.Done:
                        case AppBar.ButtonTypeEnum.Remove:
                            Show();
                            break;

                        default:
                            Hide();
                            break;
                    }
                    break;
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 0.5f);
        }

        private void Hide()
        {
            if (!initialized)
                return;

            icon.Alpha = 0f;
            text.DisableText = true;
            cButton.enabled = false;
            cButton.FilterTag = HiddenFilterTag;
        }

        private void Show()
        {
            if (!initialized)
                return;

            icon.Alpha = 1f;
            text.DisableText = false;
            cButton.enabled = true;
            cButton.FilterTag = VisibleFilterTag;
        }

        private void RefreshOffsets()
        {
            // Apply offset based on total number of buttons
            float xDefaultOffset = (parentToolBar.NumDefaultButtons / 2) * ButtonWidth;
            float xManipulationOffset = (parentToolBar.NumManipulationButtons / 2) * ButtonWidth;

            // For odd numbers of buttons, add an additional 1/2 button offset
            if (parentToolBar.NumDefaultButtons > 1 && parentToolBar.NumDefaultButtons % 2 == 0)
            {
                xDefaultOffset -= (ButtonWidth / 2);
            }
            if (parentToolBar.NumManipulationButtons > 1 && parentToolBar.NumManipulationButtons % 2 == 0)
            {
                xManipulationOffset -= (ButtonWidth / 2);
            }

            defaultOffset = new Vector3(
                template.DefaultPosition * ButtonWidth - xDefaultOffset,
                0f,
                template.DefaultPosition * ButtonDepth);
            manipulationOffset = new Vector3(
                template.ManipulationPosition * ButtonWidth - xManipulationOffset,
                0f,
                template.ManipulationPosition * ButtonDepth);
            hiddenOffset = Vector3.zero;
        }

        private ButtonIconProfile customIconProfile;
        private AppBar.ButtonTemplate template;
        private Vector3 targetPosition;
        private Vector3 defaultOffset;
        private Vector3 hiddenOffset;
        private Vector3 manipulationOffset;
        private CompoundButton cButton;
        private CompoundButtonText text;
        private CompoundButtonIcon icon;
        private AppBar parentToolBar;
        private bool initialized = false;
    }
}