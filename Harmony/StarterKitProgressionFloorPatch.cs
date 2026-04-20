using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace StarterKits.Harmony
{
    /// <summary>
    /// Keeps starter kit progression floors active without touching base attributes.
    /// Floors are stored in player buff custom vars: skFloor_<progressionName>.
    /// </summary>
    public static class StarterKitProgressionFloorPatch
    {
        private static bool loggedFirstHit;
        private const string FloorEnabledVar = "skFloorEnabled";
        private const string FloorVarPrefix = "skFloor_";

        public static void Register(HarmonyLib.Harmony harmony)
        {
            if (harmony == null)
            {
                return;
            }

            var patched = new List<string>();

            TryPatch(
                harmony,
                AccessTools.PropertyGetter(typeof(ProgressionValue), "Level"),
                typeof(StarterKitProgressionFloorPatch).GetMethod(nameof(PostfixGetProperty), BindingFlags.Static | BindingFlags.NonPublic),
                patched,
                "ProgressionValue.Level(get)");

            TryPatch(
                harmony,
                AccessTools.Method(typeof(ProgressionValue), "GetLevel"),
                typeof(StarterKitProgressionFloorPatch).GetMethod(nameof(PostfixGetMethod), BindingFlags.Static | BindingFlags.NonPublic),
                patched,
                "ProgressionValue.GetLevel()");

            TryPatch(
                harmony,
                AccessTools.Method(typeof(ProgressionValue), "CalculatedMaxLevel", new[] { typeof(EntityAlive) }),
                typeof(StarterKitProgressionFloorPatch).GetMethod(nameof(PostfixCalculatedMaxLevel), BindingFlags.Static | BindingFlags.NonPublic),
                patched,
                "ProgressionValue.CalculatedMaxLevel(EntityAlive)");

            TryPatch(
                harmony,
                AccessTools.Method(typeof(ProgressionClass), "GetCalculatedMaxLevel", new[] { typeof(EntityAlive), typeof(ProgressionValue) }),
                typeof(StarterKitProgressionFloorPatch).GetMethod(nameof(PostfixStaticCalculatedMaxLevel), BindingFlags.Static | BindingFlags.NonPublic),
                patched,
                "ProgressionClass.GetCalculatedMaxLevel(EntityAlive, ProgressionValue)");

            TryPatch(
                harmony,
                AccessTools.Method(typeof(ProgressionValue), "CalculatedLevel", new[] { typeof(EntityAlive) }),
                typeof(StarterKitProgressionFloorPatch).GetMethod(nameof(PostfixCalculatedLevel), BindingFlags.Static | BindingFlags.NonPublic),
                patched,
                "ProgressionValue.CalculatedLevel(EntityAlive)");

            TryPatch(
                harmony,
                AccessTools.Method(typeof(ProgressionValue), "GetCalculatedLevel", new[] { typeof(EntityAlive) }),
                typeof(StarterKitProgressionFloorPatch).GetMethod(nameof(PostfixGetCalculatedLevel), BindingFlags.Static | BindingFlags.NonPublic),
                patched,
                "ProgressionValue.GetCalculatedLevel(EntityAlive)");

            TryPatch(
                harmony,
                AccessTools.Method(typeof(ProgressionValue), "IsLocked", new[] { typeof(EntityAlive) }),
                typeof(StarterKitProgressionFloorPatch).GetMethod(nameof(PostfixIsLocked), BindingFlags.Static | BindingFlags.NonPublic),
                patched,
                "ProgressionValue.IsLocked(EntityAlive)");

            Log.Out($"[StarterKits] StarterKitFloorPatch register complete. Patched targets: {string.Join(", ", patched)}");
        }

        [HarmonyPatch(typeof(ProgressionValue), "get_Level")]
        [HarmonyPostfix]
        private static void PostfixGetProperty(ProgressionValue __instance, ref int __result)
        {
            ApplyLevelFloor(__instance, ResolvePrimaryPlayer(), ref __result);
        }

        [HarmonyPatch(typeof(ProgressionValue), "GetLevel")]
        [HarmonyPostfix]
        private static void PostfixGetMethod(ProgressionValue __instance, ref int __result)
        {
            ApplyLevelFloor(__instance, ResolvePrimaryPlayer(), ref __result);
        }

        [HarmonyPatch(typeof(ProgressionValue), "CalculatedMaxLevel")]
        [HarmonyPostfix]
        private static void PostfixCalculatedMaxLevel(ProgressionValue __instance, EntityAlive _ea, ref int __result)
        {
            ApplyMaxFloor(__instance, _ea, ref __result);
        }

        [HarmonyPatch(typeof(ProgressionValue), "CalculatedLevel")]
        [HarmonyPostfix]
        private static void PostfixCalculatedLevel(ProgressionValue __instance, EntityAlive _ea, ref int __result)
        {
            ApplyLevelFloor(__instance, ResolvePlayer(_ea), ref __result);
        }

        [HarmonyPatch(typeof(ProgressionValue), "GetCalculatedLevel")]
        [HarmonyPostfix]
        private static void PostfixGetCalculatedLevel(ProgressionValue __instance, EntityAlive _ea, ref float __result)
        {
            try
            {
                int floor = GetConfiguredFloor(__instance, ResolvePlayer(_ea));
                if (floor > 0 && __result < floor)
                {
                    __result = floor;
                    LogFirstHit("GetCalculatedLevel");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] StarterKitProgressionFloorPatch CalcLevel(float) exception: {ex.Message}");
            }
        }

        [HarmonyPatch(typeof(ProgressionClass), "GetCalculatedMaxLevel")]
        [HarmonyPostfix]
        private static void PostfixStaticCalculatedMaxLevel(EntityAlive _ea, ProgressionValue _pv, ref int __result)
        {
            ApplyMaxFloor(_pv, _ea, ref __result);
        }

        [HarmonyPatch(typeof(ProgressionValue), "IsLocked")]
        [HarmonyPostfix]
        private static void PostfixIsLocked(ProgressionValue __instance, EntityAlive _ea, ref bool __result)
        {
            try
            {
                int floor = GetConfiguredFloor(__instance, ResolvePlayer(_ea));
                if (floor > 0)
                {
                    __result = false;
                    LogFirstHit("IsLocked");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] StarterKitProgressionFloorPatch IsLocked exception: {ex.Message}");
            }
        }

        private static void ApplyLevelFloor(ProgressionValue value, EntityPlayer player, ref int result)
        {
            try
            {
                int floor = GetConfiguredFloor(value, player);
                if (floor > result)
                {
                    result = floor;
                    LogFirstHit("Level");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] StarterKitProgressionFloorPatch Level exception: {ex.Message}");
            }
        }

        private static void ApplyMaxFloor(ProgressionValue value, EntityAlive entity, ref int result)
        {
            try
            {
                int floor = GetConfiguredFloor(value, ResolvePlayer(entity));
                if (floor > result)
                {
                    result = floor;
                    LogFirstHit("CalculatedMaxLevel");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] StarterKitProgressionFloorPatch Max exception: {ex.Message}");
            }
        }

        private static int GetConfiguredFloor(ProgressionValue value, EntityPlayer player)
        {
            if (player?.Buffs == null)
            {
                return 0;
            }

            if (!player.Buffs.HasCustomVar(FloorEnabledVar) || player.Buffs.GetCustomVar(FloorEnabledVar) < 0.5f)
            {
                return 0;
            }

            string progressionName = ResolveProgressionName(value);
            if (string.IsNullOrEmpty(progressionName))
            {
                return 0;
            }

            string floorVar = FloorVarPrefix + progressionName;
            if (!player.Buffs.HasCustomVar(floorVar))
            {
                return 0;
            }

            int floor = (int)player.Buffs.GetCustomVar(floorVar);
            return floor > 0 ? floor : 0;
        }

        private static string ResolveProgressionName(ProgressionValue value)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                string typedName = value.ProgressionClass?.Name;
                if (!string.IsNullOrWhiteSpace(typedName))
                {
                    return typedName;
                }
            }
            catch
            {
                // Fallback to reflection below for compatibility.
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = value.GetType();

            object directName = ReadMember(t, value, "Name", flags);
            if (directName is string directString && !string.IsNullOrWhiteSpace(directString))
            {
                return directString;
            }

            object progressionClass = ReadMember(t, value, "ProgressionClass", flags)
                                   ?? ReadMember(t, value, "progressionClass", flags)
                                   ?? ReadMember(t, value, "Class", flags);
            if (progressionClass != null)
            {
                Type ct = progressionClass.GetType();
                object className = ReadMember(ct, progressionClass, "Name", flags)
                                ?? ReadMember(ct, progressionClass, "name", flags);
                if (className is string classString && !string.IsNullOrWhiteSpace(classString))
                {
                    return classString;
                }
            }

            return null;
        }

        private static EntityPlayer ResolvePlayer(EntityAlive entity)
        {
            if (entity is EntityPlayer player)
            {
                return player;
            }

            return ResolvePrimaryPlayer();
        }

        private static EntityPlayer ResolvePrimaryPlayer()
        {
            try
            {
                return GameManager.Instance?.World?.GetPrimaryPlayer();
            }
            catch
            {
                return null;
            }
        }

        private static void LogFirstHit(string source)
        {
            if (loggedFirstHit)
            {
                return;
            }

            loggedFirstHit = true;
            Log.Out($"[StarterKits] StarterKitFloorPatch active via {source}.");
        }

        private static void TryPatch(
            HarmonyLib.Harmony harmony,
            MethodBase target,
            MethodInfo postfix,
            List<string> patched,
            string label)
        {
            try
            {
                if (target == null || postfix == null)
                {
                    Log.Warning($"[StarterKits] StarterKitFloorPatch target not found: {label}");
                    return;
                }

                harmony.Patch(target, postfix: new HarmonyMethod(postfix));
                patched.Add(label);
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] StarterKitFloorPatch patch failed for {label}: {ex.Message}");
            }
        }

        private static object ReadMember(Type type, object instance, string name, BindingFlags flags)
        {
            PropertyInfo p = type.GetProperty(name, flags);
            if (p != null && p.CanRead)
            {
                return p.GetValue(instance, null);
            }

            FieldInfo f = type.GetField(name, flags);
            if (f != null)
            {
                return f.GetValue(instance);
            }

            return null;
        }
    }
}
