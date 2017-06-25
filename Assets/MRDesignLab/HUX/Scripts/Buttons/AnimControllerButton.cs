//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;
using System;
using System.Collections;
using HUX;

namespace HUX.Buttons
{
    /// <summary>
    /// Anim controller button offers as simple way to link button states to animation controller parameters
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimControllerButton : Button
    {
        [Serializable]
        public struct AnimatorControllerAction
        {
            public ButtonStateEnum ButtonState;
            public string ParamName;
            public AnimatorControllerParameterType ParamType;
            public bool BoolValue;
            public int IntValue;
            public float FloatValue;
            public bool InvalidParam;
        }

        /// <summary>
        /// List of animation actions
        /// </summary>
        [HideInInspector]
        public AnimatorControllerAction[] AnimActions;

        /// <summary>
        /// Animator
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// On state change
        /// </summary>
        public override void OnStateChange(ButtonStateEnum newState)
        {
            if (_animator == null)
                _animator = this.GetComponent<Animator>();

            if (AnimActions == null)
            {
                //TODO error?
                base.OnStateChange(newState);
                return;
            }

            for (int i = 0; i < AnimActions.Length; i++)
            {
                if (AnimActions[i].ButtonState == newState)
                {
                    switch (AnimActions[i].ParamType)
                    {
                        case AnimatorControllerParameterType.Bool:
                            _animator.SetBool(AnimActions[i].ParamName, AnimActions[i].BoolValue);
                            break;

                        case AnimatorControllerParameterType.Float:
                            _animator.SetFloat(AnimActions[i].ParamName, AnimActions[i].FloatValue);
                            break;

                        case AnimatorControllerParameterType.Int:
                            _animator.SetInteger(AnimActions[i].ParamName, AnimActions[i].IntValue);
                            break;

                        case AnimatorControllerParameterType.Trigger:
                            _animator.SetTrigger(AnimActions[i].ParamName);
                            break;

                        default:
                            break;
                    }
                    break;
                }
            }

            base.OnStateChange(newState);
        }
    }
}