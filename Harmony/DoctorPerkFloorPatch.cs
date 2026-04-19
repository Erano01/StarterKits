using System;
using System.Reflection;
using HarmonyLib;

namespace StarterKits.Harmony
{
    /// <summary>
    /// Keeps Doctor kit perk levels at a player-specific floor without touching base attributes.
    /// Applies only when the player has skDoctorFloorEnabled custom var.
    /// </summary>
    public static class DoctorPerkFloorPatch
    {
        private static bool loggedFirstHit;

        [HarmonyPatch(typeof(ProgressionValue), "get_Level")]
        [HarmonyPostfix]
        private static void PostfixGetProperty(ProgressionValue __instance, ref int __result)
        {
            ApplyFloor(__instance, ref __result);
        }

        [HarmonyPatch(typeof(ProgressionValue), "GetLevel")]
        [HarmonyPostfix]
        private static void PostfixGetMethod(ProgressionValue __instance, ref int __result)
        {
            ApplyFloor(__instance, ref __result);
        }

        private static void ApplyFloor(ProgressionValue __instance, ref int __result)
        {
            try
            {
                string progressionName = ResolveProgressionName(__instance);
                if (string.IsNullOrEmpty(progressionName))
                {
                    return;
                }

                if (!string.Equals(progressionName, "perkPhysician", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(progressionName, "perkCharismaticNature", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                EntityPlayer player = ResolveOwnerPlayer(__instance) ?? ResolvePrimaryPlayer();
                if (player?.Buffs == null || !player.Buffs.HasCustomVar("skDoctorFloorEnabled"))
                {
                    return;
                }

                int floor = 0;
                if (string.Equals(progressionName, "perkPhysician", StringComparison.OrdinalIgnoreCase))
                {
                    floor = (int)player.Buffs.GetCustomVar("skDoctorFloorPhysician");
                }
                else if (string.Equals(progressionName, "perkCharismaticNature", StringComparison.OrdinalIgnoreCase))
                {
                    floor = (int)player.Buffs.GetCustomVar("skDoctorFloorCharismatic");
                }

                if (floor > __result)
                {
                    __result = floor;
                }

                if (!loggedFirstHit)
                {
                    loggedFirstHit = true;
                    Log.Out("[StarterKits] DoctorPerkFloorPatch active.");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[StarterKits] DoctorPerkFloorPatch exception: {ex.Message}");
            }
        }

        private static string ResolveProgressionName(ProgressionValue value)
        {
            if (value == null)
            {
                return null;
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

        private static EntityPlayer ResolveOwnerPlayer(ProgressionValue value)
        {
            if (value == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = value.GetType();

            object progression = ReadMember(t, value, "Progression", flags)
                              ?? ReadMember(t, value, "progression", flags)
                              ?? ReadMember(t, value, "Parent", flags)
                              ?? ReadMember(t, value, "parent", flags);
            if (progression == null)
            {
                return null;
            }

            Type pt = progression.GetType();
            object entity = ReadMember(pt, progression, "Entity", flags)
                         ?? ReadMember(pt, progression, "entity", flags)
                         ?? ReadMember(pt, progression, "Owner", flags)
                         ?? ReadMember(pt, progression, "owner", flags)
                         ?? ReadMember(pt, progression, "Parent", flags)
                         ?? ReadMember(pt, progression, "parent", flags);

            if (entity is EntityPlayer ep)
            {
                return ep;
            }

            if (entity is EntityAlive alive)
            {
                return alive as EntityPlayer;
            }

            return null;
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
