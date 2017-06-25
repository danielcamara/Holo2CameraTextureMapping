﻿//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Dialogs;
using HUX.Interaction;
using HUX.Receivers;
using System.Collections;
using UnityEngine;

namespace HUX
{
    public class LoadingIndicatorExample : InteractionReceiver
    {
        public GameObject SceneObject;

        [Header("How long to spend on each stage of loading")]
        public float LeadInTime = 1.5f;
        public float LoadingTime = 5f;
        public float FinishTime = 1.5f;

        [Header ("Set these to override the defaults set in the LoadingDialog prefab")]
        public GameObject LoadingPrefab = null;
        public Texture2D LoadingIcon = null;

        [Header("Messages displayed during loading")]
        public string LeadInMessage = "(Starting load)";
        public string LoadTextMessage = "Loading with message only";
        public string LoadOrbsMessage = "Loading with Orbs";
        public string LoadIconMessage = "Loading with Icon";
        public string LoadPrefabMessage = "Loading with Prefab";
        public string LoadProgressMessage = "Loading with Progress";
        public string LoadProgressBarMessage = "Loading with Bar";
        public string FinishMessage = "Finished!";

        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        {
            Debug.Log("Loading with button " + obj.name);

            // Don't do anything if we're already showing the dialog
            if (LoadingDialog.Instance.IsLoading)
                return;

            switch (obj.name)
            {
                case "ButtonLoadMessage":
                    LoadingDialog.Instance.Open(
                            LoadingDialog.IndicatorStyleEnum.None,
                            LoadingDialog.ProgressStyleEnum.None,
                            LoadingDialog.MessageStyleEnum.Visible,
                            LeadInMessage);
                    StartCoroutine(LoadOverTime(LoadTextMessage));
                    break;

                case "ButtonLoadOrbs":
                    LoadingDialog.Instance.Open(
                             LoadingDialog.IndicatorStyleEnum.AnimatedOrbs,
                             LoadingDialog.ProgressStyleEnum.None,
                             LoadingDialog.MessageStyleEnum.Visible,
                             LeadInMessage);
                    StartCoroutine(LoadOverTime(LoadOrbsMessage));
                    break;

                case "ButtonLoadIcon":
                    LoadingDialog.Instance.Open(
                        LoadingDialog.IndicatorStyleEnum.StaticIcon,
                        LoadingDialog.ProgressStyleEnum.None,
                        LoadingDialog.MessageStyleEnum.Visible,
                        LeadInMessage,
                        LoadingIcon);
                    StartCoroutine(LoadOverTime(LoadIconMessage));
                    break;

                case "ButtonLoadPrefab":
                    LoadingDialog.Instance.Open(
                        LoadingDialog.IndicatorStyleEnum.Prefab,
                        LoadingDialog.ProgressStyleEnum.None,
                        LoadingDialog.MessageStyleEnum.Visible,
                        LeadInMessage,
                        null,
                        LoadingPrefab);
                    StartCoroutine(LoadOverTime(LoadPrefabMessage));
                    break;

                case "ButtonLoadProgress":
                    LoadingDialog.Instance.Open(
                        LoadingDialog.IndicatorStyleEnum.None,
                        LoadingDialog.ProgressStyleEnum.Percentage,
                        LoadingDialog.MessageStyleEnum.Visible,
                        LeadInMessage);
                    StartCoroutine(LoadOverTime(LoadProgressMessage));
                    break;

                case "ButtonLoadProgressBar":
                    LoadingDialog.Instance.Open(
                        LoadingDialog.IndicatorStyleEnum.None,
                        LoadingDialog.ProgressStyleEnum.ProgressBar,
                        LoadingDialog.MessageStyleEnum.Visible,
                        LeadInMessage);
                    StartCoroutine(LoadOverTime(LoadProgressMessage));
                    break;

                default:
                    break;
            }
            base.OnTapped(obj, eventArgs);
        }

        protected IEnumerator LoadOverTime(string message)
        {
            // Turn off our buttons while we load
            SceneObject.SetActive(false);

            // Wait for lead in time to end (optional)
            float startTime = Time.time;
            yield return new WaitForSeconds(LeadInTime);

            // Progress must be a number from 0-1 (it will be clamped)
            // It will be formatted according to 'ProgressFormat' (0.0 by default) and followed with a '%' character 
            float progress = 0f;
            // While we're in the loading period, update progress and message in 1/4 second intervals
            // Displayed progress is smoothed out so you don't have to update every frame
            startTime = Time.time;
            while (Time.time < startTime + LoadingTime)
            {
                progress = (Time.time - startTime) / LoadingTime;
                LoadingDialog.Instance.SetMessage(message);
                LoadingDialog.Instance.SetProgress(progress);
                yield return new WaitForSeconds(Random.Range (0.15f, 0.5f));
            }

            // Give the user a final notification that loading has finished (optional)
            LoadingDialog.Instance.SetMessage(FinishMessage);
            LoadingDialog.Instance.SetProgress(1f);
            yield return new WaitForSeconds(FinishTime);

            // Close the loading dialog
            // LoadingDialog.Instance.IsLoading will report true until its 'Closing' animation has ended
            // This typically takes about 1 second
            LoadingDialog.Instance.Close();
            while (LoadingDialog.Instance.IsLoading)
            {
                yield return null;
            }

            // Turn everything on again
            SceneObject.SetActive(true);

            yield break;
        }
    }
}