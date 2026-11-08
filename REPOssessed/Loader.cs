using REPOssessed.Cheats.Core;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOssessed
{
    public class Loader : MonoBehaviour
    {
        private static GameObject? Load;

        public static void Init()
        {
            if (Load != null)
            {
                Debug.LogError("REPOssessed is already injected");
                return;
            }
            LoadHarmony();
            Load = new GameObject();
            Load.AddComponent<REPOssessed>();
            Object.DontDestroyOnLoad(Load);
        }

        public static void LoadHarmony()
        {
            LoadEmbeddedAssembly("REPOssessed.Resources.Mono.Cecil.dll");
            LoadEmbeddedAssembly("REPOssessed.Resources.MonoMod.Utils.dll");
            LoadEmbeddedAssembly("REPOssessed.Resources.MonoMod.RuntimeDetour.dll");
            LoadEmbeddedAssembly("REPOssessed.Resources.0Harmony.dll");
        }

        private static void LoadEmbeddedAssembly(string resourceName)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Missing embedded assembly resource: {resourceName}");
            byte[] rawAssembly = new byte[stream.Length];
            stream.Read(rawAssembly, 0, (int)stream.Length);
            AppDomain.CurrentDomain.Load(rawAssembly);
        }
         
        public static void Unload()
        {
            Cheat.instances.Clear();
            Object.Destroy(Load);
        }
    }
}
