﻿//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;
using System;
using System.Collections;
using HUX.Utility;

namespace HUX.Buttons
{
    /// <summary> 
    /// Mesh button is a mesh renderer interactible with state data for button state
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimButton : Button
    {

        /// <summary>
        /// Mesh filter object for mesh button.
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// On state change swap out the active mesh based on the state
        /// </summary>
        public override void OnStateChange(ButtonStateEnum newState)
        {
            if (_animator == null)
                _animator = this.GetComponent<Animator>();

            _animator.SetInteger("State", (int)newState);

            base.OnStateChange(newState);
        }
    }
}