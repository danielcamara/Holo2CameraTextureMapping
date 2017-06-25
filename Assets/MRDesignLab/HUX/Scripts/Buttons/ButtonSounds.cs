﻿//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace HUX.Buttons
{
    /// <summary>
    /// A convenient way to play sounds in response to button actions / states
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSounds : MonoBehaviour
    {
        const float MinTimeBetweenSameClip = 0.1f;

        // Direct interaction clips
        public AudioClip ButtonCancelled;
        public AudioClip ButtonHeld;
        public AudioClip ButtonPressed;
        public AudioClip ButtonReleased;

        // State change clips
        public AudioClip ButtonObservation;
        public AudioClip ButtonObservationTargeted;
        public AudioClip ButtonTargeted;

        private AudioSource audioSource;
        private static string lastClipName;
        private static float lastClipTime;

        void Start ()
        {
            Button button = GetComponent<Button>();
            button.OnButtonCancelled += OnButtonCancelled;
            button.OnButtonHeld += OnButtonHeld;
            button.OnButtonPressed += OnButtonPressed;
            button.OnButtonReleased += OnButtonReleased;
            button.StateChange += StateChange;

            audioSource = GetComponent<AudioSource>();
        }

        void StateChange(Button.ButtonStateEnum newState)
        {
            switch (newState)
            {
                case Button.ButtonStateEnum.Observation:
                    PlayClip(ButtonObservation);
                    break;

                case Button.ButtonStateEnum.ObservationTargeted:
                    PlayClip(ButtonObservationTargeted);
                    break;

                case Button.ButtonStateEnum.Targeted:
                    PlayClip(ButtonTargeted);
                    break;

                default:
                    break;
            }
        }

        void OnButtonCancelled(GameObject go)
        {
            PlayClip(ButtonCancelled);
        }

        void OnButtonHeld(GameObject go)
        {
            PlayClip(ButtonHeld);
        }

        void OnButtonPressed(GameObject go)
        {
            PlayClip(ButtonPressed);
        }

        void OnButtonReleased (GameObject go)
        {
            PlayClip(ButtonReleased);
        }

        void PlayClip (AudioClip clip)
        {
            if (clip != null)
            {
                // Don't play the clip if we're spamming it
                if (clip.name == lastClipName && Time.realtimeSinceStartup < MinTimeBetweenSameClip)
                    return;

                lastClipName = clip.name;
                lastClipTime = Time.realtimeSinceStartup;
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(clip);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position);
                }
            }
        }
    }
}