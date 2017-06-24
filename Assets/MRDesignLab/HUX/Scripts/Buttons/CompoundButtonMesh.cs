﻿//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System;
using UnityEngine;

namespace HUX.Buttons
{
    /// <summary>
    /// Mesh button is a mesh renderer interactible with state data for button state
    /// </summary>
    [RequireComponent(typeof(CompoundButton))]
    public class CompoundButtonMesh : MonoBehaviour
    {
        const float AnimationSpeedMultiplier = 25f;

        public ButtonMeshProfile MeshProfile;

        /// <summary>
        /// Transform that scale and offset will be applied to.
        /// </summary>
        public Transform TargetTransform;

        /// <summary>
        /// Mesh renderer button for mesh button.
        /// </summary>
        public MeshRenderer Renderer;

        /// <summary>
        /// Mesh filter object for mesh button.
        /// </summary>
        public MeshFilter MeshFilter;
        
        /// <summary>
        /// Mesh Button State Data Set
        /// </summary>
        [Serializable]
        public class MeshButtonDatum
        {
            /// <summary>
            /// Constructor for mesh button datum
            /// </summary>
            public MeshButtonDatum(Button.ButtonStateEnum state) { this.ActiveState = state; this.Name = state.ToString(); }

            /// <summary>
            /// Name string for datum entry
            /// </summary>
            public string Name;
            /// <summary>
            /// Button state the datum is active in
            /// </summary>
            public Button.ButtonStateEnum ActiveState = Button.ButtonStateEnum.Observation;
            /// <summary>
            /// Button mesh color to use in active state
            /// </summary>
            public Color StateColor = Color.white;
            /// <summary>
            /// Button mesh shader property to use in active state
            /// </summary>
            public float StateValue = 0f;
            /// <summary>
            /// Offset to translate mesh to in active state.
            /// </summary>
            public Vector3 Offset;
            /// <summary>
            /// Scale for mesh button in active state
            /// </summary>
            public Vector3 Scale;
        }

        private MeshButtonDatum currentDatum;

        /// <summary>
        /// The material used by button's Renderer after being modified
        /// </summary>
        private Material instantiatedMaterial;

        /// <summary>
        /// The material used by button's Renderer before being modified
        /// </summary>
        private Material sharedMaterial;

        #if UNITY_EDITOR
        /// <summary>
        /// Called by CompoundButtonSaveInterceptor
        /// Prevents saving a scene with instanced materials
        /// </summary>
        public void OnWillSaveScene ()
        {
            if (Renderer != null && instantiatedMaterial != null)
            {
                Renderer.sharedMaterial = sharedMaterial;
                GameObject.DestroyImmediate(instantiatedMaterial);
            }
        }
        #endif

        protected void Start ()
        {
            Button b = GetComponent<Button>();
            if (b == null)
            {
                Debug.LogError("No button attached to CompoundButtonMesh in " + name);
                enabled = false;
                return;
            }       

            if (MeshProfile == null)
            {
                Debug.LogError("No profile selected for CompoundButtonMesh in " + name);
                enabled = false;
                return;
            }

            b.StateChange += StateChange;
            // Disable this script if we're not using smooth changes
            enabled = MeshProfile.SmoothStateChanges;
            // Set the current datum so our first state is activated
            currentDatum = MeshProfile.ButtonStates[(int)Button.ButtonStateEnum.Observation];
            UpdateButtonProperties(false);
        }

        /// <summary>
        /// On state change swap out the active mesh based on the state
        /// </summary>
        protected void StateChange(Button.ButtonStateEnum newState)
        {
            if (newState == Button.ButtonStateEnum.Pressed)
            {
                lastTimePressed = Time.time;
            }

            currentDatum = MeshProfile.ButtonStates[(int)newState];
                        
            // If we're not using smooth states, just set them now
            if (!MeshProfile.SmoothStateChanges)
            {
                TargetTransform.localScale = currentDatum.Scale;
                TargetTransform.localPosition = currentDatum.Offset;

                if (Renderer != null)
                {
                    if (instantiatedMaterial == null)
                    {
                        sharedMaterial = Renderer.sharedMaterial;
                        instantiatedMaterial = new Material(sharedMaterial);
                        Renderer.sharedMaterial = instantiatedMaterial;
                    }

                    if (!string.IsNullOrEmpty(MeshProfile.ColorPropertyName))
                    {
                        Renderer.sharedMaterial.SetColor(MeshProfile.ColorPropertyName, currentDatum.StateColor);
                    }
                    if (!string.IsNullOrEmpty(MeshProfile.ValuePropertyName))
                    {
                        Renderer.sharedMaterial.SetFloat(MeshProfile.ValuePropertyName, currentDatum.StateValue);
                    }
                }
            }
        }

        protected void OnDisable()
        {
            StateChange(Button.ButtonStateEnum.Disabled);
            UpdateButtonProperties(false);
        }

        protected void Update ()
        {
            UpdateButtonProperties(true);
        }

        protected void UpdateButtonProperties(bool smooth)
        {
            if (currentDatum == null)
                return;

            MeshButtonDatum datum = currentDatum;

            // If we're using sticky events, and we're still not past the 'sticky' pressed time, use that datum
            if (MeshProfile.StickyPressedEvents && Time.time < lastTimePressed + MeshProfile.StickyPressedTime)
            {
                datum = MeshProfile.ButtonStates[(int)Button.ButtonStateEnum.Pressed];
            }

            if (TargetTransform != null)
            {
                if (smooth)
                {
                    TargetTransform.localScale = Vector3.Lerp(
                        TargetTransform.localScale, datum.Scale,
                        Time.deltaTime * MeshProfile.AnimationSpeed * AnimationSpeedMultiplier);
                    TargetTransform.localPosition = Vector3.Lerp(
                        TargetTransform.localPosition, datum.Offset,
                        Time.deltaTime * MeshProfile.AnimationSpeed * AnimationSpeedMultiplier);
                } else
                {
                    TargetTransform.localScale = datum.Scale;
                    TargetTransform.localPosition = datum.Offset;
                }
            }

            // Set the color from the datum 
            if (Renderer != null)
            {
                if (instantiatedMaterial == null)
                {
                    sharedMaterial = Renderer.sharedMaterial;
                    instantiatedMaterial = new Material(sharedMaterial);
                    Renderer.sharedMaterial = instantiatedMaterial;
                }

                if (!string.IsNullOrEmpty(MeshProfile.ColorPropertyName))
                {
                    if (smooth)
                    {
                        Renderer.sharedMaterial.SetColor(
                            MeshProfile.ColorPropertyName,
                            Color.Lerp(Renderer.material.GetColor(MeshProfile.ColorPropertyName),
                            datum.StateColor,
                            Time.deltaTime * MeshProfile.AnimationSpeed * AnimationSpeedMultiplier));
                    } else
                    {
                        Renderer.sharedMaterial.SetColor(
                            MeshProfile.ColorPropertyName,
                            datum.StateColor);
                    }
                }
                if (!string.IsNullOrEmpty(MeshProfile.ValuePropertyName))
                {
                    if (smooth)
                    {
                        Renderer.sharedMaterial.SetFloat(
                            MeshProfile.ValuePropertyName,
                            Mathf.Lerp(Renderer.material.GetFloat(MeshProfile.ValuePropertyName),
                            datum.StateValue,
                            Time.deltaTime * MeshProfile.AnimationSpeed * AnimationSpeedMultiplier));
                    } else
                    {
                        Renderer.sharedMaterial.SetFloat(MeshProfile.ValuePropertyName, datum.StateValue);
                    }
                }
            }

        }

        private float lastTimePressed = 0f;
    }
}