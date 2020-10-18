﻿using System.Diagnostics;
using MessagePack;
using NPOI.SS.Formula.Functions;
using Sakaba.Domain;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Sakaba.Editor {
	public partial class Menu
	{
        [MenuItem("MasterMemory/CodeGenerate")]
        private static void Generate()
        {
            ExecuteMasterMemoryCodeGenerator();
            ExecuteMessagePackCodeGenerator();
        }

        private static void ExecuteMasterMemoryCodeGenerator() {
            UnityEngine.Debug.Log($"{nameof(ExecuteMasterMemoryCodeGenerator)} : start");

            var exProcess = new Process();

            var rootPath = Application.dataPath + "/..";
            var filePath = rootPath + "/GeneratorTools";
            var exeFileName = "";
    #if UNITY_EDITOR_WIN
            exeFileName = "/MasterMemory.Generator.exe";
    #elif UNITY_EDITOR_OSX
            exeFileName = "/osx-x64/MasterMemory.Generator";
    #elif UNITY_EDITOR_LINUX
            exeFileName = "/linux-x64/MasterMemory.Generator";
    #else
            return;
    #endif

            var psi = new ProcessStartInfo() {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = filePath + exeFileName,
                Arguments = $@"-i ""{Application.dataPath}/Sakaba/Scripts"" -o ""{Application.dataPath}/Sakaba/Generated"" -n ""MD""",
            };

            var p = Process.Start(psi);

            p.EnableRaisingEvents = true;
            p.Exited += (object sender, System.EventArgs e) => {
                var data = p.StandardOutput.ReadToEnd();
                UnityEngine.Debug.Log($"{data}");
                UnityEngine.Debug.Log($"{nameof(ExecuteMasterMemoryCodeGenerator)} : end");
                p.Dispose();
                p = null;
            };
        }

        private static void ExecuteMessagePackCodeGenerator() {
            UnityEngine.Debug.Log($"{nameof(ExecuteMessagePackCodeGenerator)} : start");

            var exProcess = new Process();

            var rootPath = Application.dataPath + "/..";
            var filePath = rootPath + "/GeneratorTools";
            var exeFileName = "";
    #if UNITY_EDITOR_WIN
            exeFileName = "/mpc.exe";
    #elif UNITY_EDITOR_OSX
            exeFileName = "/osx-x64/mpc";
    #elif UNITY_EDITOR_LINUX
            exeFileName = "/linux-x64/mpc";
    #else
            return;
    #endif

            var psi = new ProcessStartInfo() {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = filePath + exeFileName,
                Arguments = $@"-i ""{Application.dataPath}/../Assembly-CSharp.csproj"" -o ""{Application.dataPath}/Sakaba/Generated/MessagePack.Generated.cs""",
            };

            var p = Process.Start(psi);

            p.EnableRaisingEvents = true;
            p.Exited += (object sender, System.EventArgs e) => {
                var data = p.StandardOutput.ReadToEnd();
                UnityEngine.Debug.Log($"{data}");
                UnityEngine.Debug.Log($"{nameof(ExecuteMessagePackCodeGenerator)} : end");
                p.Dispose();
                p = null;
            };
        }
	}
}