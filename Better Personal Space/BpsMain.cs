using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Better_Personal_Space;
using HarmonyLib;
using MelonLoader;
using ReMod.Core.Managers;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC;
using VRC.DataModel;
using VRC.UI.Core;
using VRC.UI.Elements.Menus;

[assembly: MelonInfo(typeof(BpsMain), "Better Personal Space", "1.0.0", "Fyre", "WIP")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.Yellow)]

namespace Better_Personal_Space
{
    public class BpsMain : MelonMod
    {
        public static MelonLogger.Instance BpsLogger;
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            BpsPlayerManager.OnSceneLoad();
        }

        public override void OnApplicationStart()
        {
            BpsLogger = LoggerInstance;
            
            BpsLogger.Msg("Initializing...");
            ResourceManager.LoadImageResources("bps");
            BpsConfig.SettingsInit();
            OnPreferencesSaved();
            BpsPlayerManager.Init();
            InitializePatches();
            MelonCoroutines.Start(WaitForUI());
            BpsLogger.Msg("Initialized!");
            CheckMods();
        }
        
        public override void OnUpdate()
        {
            if (Player.prop_Player_0 == null) return;

            var myPosition = Player.prop_Player_0.transform.position;
            foreach (var otherPlayer in BpsPlayerManager.HiddenPlayers.Values)
            {
                var distance = (otherPlayer.Pos - myPosition).magnitude;
                if (distance < BpsConfig.PersonalSpace.Value)
                    otherPlayer.HideAvatar();
                else
                {
                    otherPlayer.ShowAvatar();
                }
            }
        }

        private static HarmonyMethod GetLocalPatch(string name)
        {
            return typeof(BpsMain).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod();
        }

        private void InitializePatches()
        {
            foreach (var method in typeof(SelectedUserMenuQM).GetMethods())
            {
                if (!method.Name.StartsWith("Method_Private_Void_IUser_PDM_"))
                    continue;

                if (XrefScanner.XrefScan(method).Count() < 3)
                    continue;

                HarmonyInstance.Patch(method, postfix: GetLocalPatch(nameof(SetUserPatch)));
            }
        }
        private static void SetUserPatch(SelectedUserMenuQM __instance, IUser __0)
        {
            if (__0 == null) return;

            BpsUi.OnSelectUser(__0, __instance.field_Public_Boolean_0);
        }

        private static IEnumerator WaitForUI()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;
            BpsUtils.InitializeNetworkManager();


            while (UIManager.field_Private_Static_UIManager_0 == null) yield return null;
            while (GameObject.Find("UserInterface").GetComponentInChildren<VRC.UI.Elements.QuickMenu>(true) == null)
                yield return null;
            BpsUtils.OnUiManagerInit();
            BpsUi.OnUiManagerInit();
        }

        private static void CheckMods()
        {
            if (!MelonHandler.Mods.Any(x => x.Info.Name.Equals("TabExtension")))
            {
                BpsLogger.Warning("Consider installing TabExtension if your tabs get too crowded on the quick menu!");
            }
        }
    }
}