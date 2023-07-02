using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

[assembly: ModInfo("Butchering Fix")]

namespace ButcheringFix
{
    public class Core : ModSystem
    {
        private static ICoreClientAPI _capi;

        public const string HarmonyID = "craluminum2413.butcheringfix";

        public static bool Enabled { get; set; }

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);
            new Harmony(HarmonyID).PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);
            _capi = capi;
            capi.Input.RegisterHotKey("butcheringfix", Lang.Get("butcheringfix:Toggle"), GlKeys.B, HotkeyType.CharacterControls, shiftPressed: true);
            capi.Input.SetHotKeyHandler("butcheringfix", x => Toggle(x, capi));
            capi.World.Logger.Event("started 'Butchering Fix' mod");
        }

        private bool Toggle(KeyCombination t1, ICoreClientAPI capi)
        {
            Enabled = !Enabled;
            capi.TriggerChatMessage(Lang.Get("butcheringfix:Toggle-" + Enabled.ToString().ToLowerInvariant()));
            return true;
        }

        public override void Dispose()
        {
            new Harmony(HarmonyID).UnpatchAll();
            base.Dispose();
        }

        [HarmonyPatch]
        public static class Patches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Block), nameof(Block.GetSelectionBoxes))]
            public static bool Block_GetSelectionBoxes_Patch(Cuboidf[] __result, IBlockAccessor blockAccessor, BlockPos pos)
            {
                if (!Enabled) return true;

                var nearestEntity = _capi.World.GetNearestEntity(pos.ToVec3d(), 2, 2, Matches);
                if (nearestEntity != null)
                {
                    __result = null;
                    return false;
                }

                return true;
            }
        }

        private static bool Matches(Entity entity) => entity?.HasBehavior<EntityBehaviorHarvestable>() == true && entity?.Alive == false;
    }
}
