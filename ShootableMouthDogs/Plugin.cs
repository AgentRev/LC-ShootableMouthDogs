using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace ShootableMouthDogs;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public void Awake()
    {
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} loaded!");
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }
}

[HarmonyPatch(typeof(Physics))]
public class PhysicsPatch
{
    // Filters out NavMeshAgent hits from SphereCastNonAlloc, since they block gun damage don't really serve a purpose for raycast detection.
    [HarmonyPatch(nameof(Physics.SphereCastNonAlloc))]
    [HarmonyPatch(new Type[] { typeof(Ray), typeof(float), typeof(RaycastHit[]), typeof(float), typeof(int), typeof(QueryTriggerInteraction) })]
    static void Postfix(RaycastHit[] results, int layerMask, ref int __result)
    {
        if ((layerMask & 524288) == 0) return; // abort if layerMask doesn't include the one from ShootGun's SphereCastNonAlloc

        int hitCount = 0;

        for (int i = 0; i < __result; i++)
        {
            if (results[i].transform?.GetComponent<NavMeshAgent>() == null)
            {
                if (i > hitCount) results[hitCount] = results[i]; // RaycastHit struct shallow copy
                hitCount++;
            }
        }

        __result = hitCount;
    }
}
