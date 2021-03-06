// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using UnityEditor;
using UnityEngine;
using HoloToolkit.Unity;

namespace HoloToolkit.Sharing
{
    public static class SharingMenu
    {
        [MenuItem("HoloToolkit/Sharing Service/Launch Sharing Service", false, 100)]
        public static void LaunchSessionServer()
        {
            string filePathName = @"External\HoloToolkit\Sharing\Server\SharingService.exe";

            if (!File.Exists(filePathName))
            {
                Debug.LogError("Sharing service does not exist at location: " + filePathName);
                Debug.LogError("Manually copy SharingService.exe to this path from HoloToolkit-Unity\\External.");
                return;
            }

            ExternalProcess.FindAndLaunch(filePathName, @"-local");
        }

        [MenuItem("HoloToolkit/Sharing Service/Launch Session Manager", false, 101)]
        public static void LaunchSessionUI()
        {
            string filePathName = @"External\HoloToolkit\Sharing\Tools\SessionManager\x86\SessionManager.UI.exe";

            if (!File.Exists(filePathName))
            {
                Debug.LogError("Session Manager UI does not exist at location: " + filePathName);
                Debug.LogError("Manually copy SessionManager.UI.exe to this path from HoloToolkit-Unity\\External.");
                return;
            }

            ExternalProcess.FindAndLaunch(filePathName);
        }

        [MenuItem("HoloToolkit/Sharing Service/Launch Profiler", false, 103)]
        public static void LaunchProfilerX()
        {
            string filePathName = @"External\HoloToolkit\Sharing\Tools\Profiler\x86\ProfilerX.exe";

            if (!File.Exists(filePathName))
            {
                Debug.LogError("Profiler does not exist at location: " + filePathName);
                Debug.LogError("Manually copy ProfilerX.exe to this path from HoloToolkit-Unity\\External.");
                return;
            }

            ExternalProcess.FindAndLaunch(filePathName);
        }
    }
}