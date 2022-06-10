using System.Linq;
using MelonLoader;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using UnityEngine.UI;
using VRC.DataModel;

namespace Better_Personal_Space
{
    public class BpsUi
    {
        private static UiManager _uiManager;

        private static ReMenuToggle _showPlayer, _hidePlayer;
        public static ReMenuButton PersonalSpaceButton;

        public static void OnUiManagerInit()
        {
            _uiManager = new UiManager("Better Personal Space", ResourceManager.GetSprite("bps.menu"));
            
            _uiManager.MainMenu.AddToggle("Personal Space Enabled", "Enable/Disable Bps", ToggleBpsActive,
                BpsConfig.BpsEnabled.Value);
            
            PersonalSpaceButton = _uiManager.MainMenu.AddButton($"Personal Space: {BpsConfig.PersonalSpace.Value}",
                "Size of personal bubble (in meters", () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Personal Space Size",
                        BpsConfig.PersonalSpace.Value.ToString(), InputField.InputType.Standard, false, "Submit",
                        (s, k, t) =>
                        {
                            if (string.IsNullOrEmpty(s)) return;
                            if (!float.TryParse(s, out var personalSpace)) return;

                            if (personalSpace < 0)
                            {
                                personalSpace = 0;
                            }

                            BpsConfig.PersonalSpace.Value = personalSpace;
                            MelonPreferences.Save();
                            PersonalSpaceButton.Text = $"Personal Space: {BpsConfig.PersonalSpace.Value}";
                        }, null);
                });
            
            _uiManager.MainMenu.AddToggle("Hide Friends",
                "Friends are affected by Bps filtering(Toggling off friends will work regardless)", ToggleFriends,
                BpsConfig.HideFriends.Value);
            
            _uiManager.MainMenu.AddToggle("Affect Hidden Avatars", "Bps filters everyone who's avatar is hidden",
                HiddenAvatarsToggle, BpsConfig.AffectHiddenAvatar.Value);
            
            _uiManager.MainMenu.AddToggle("Hide All", "Bps affects all players by default",
                HideAllByDefaultToggle, BpsConfig.HideAllByDefault.Value);

            _showPlayer = _uiManager.TargetMenu.AddToggle("Show Player", "This player is unaffected by Bps",
                _ =>
                {
                    var selectedUser = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                    if (selectedUser == null) return;

                    var player = BpsPlayerFromUserId(selectedUser.GetUserID());
                    player.AlwaysShow = !player.AlwaysShow;
                    if (player.AlwaysShow && player.HidePlayer)
                    {
                        player.HidePlayer = !player.AlwaysShow;
                        _hidePlayer.Toggle(player.HidePlayer, false, true);
                    }

                    BpsPlayerManager.HideOrShowPlayer(player);
                });

            _hidePlayer = _uiManager.TargetMenu.AddToggle("HidePlayer", "This player is always affected by Bps",
                _ =>
                {
                    var selectedUser = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                    if (selectedUser == null) return;

                    var player = BpsPlayerFromUserId(selectedUser.GetUserID());
                    player.HidePlayer = !player.HidePlayer;
                    if (player.AlwaysShow && player.HidePlayer)
                    {
                        player.AlwaysShow = !player.HidePlayer;
                        _showPlayer.Toggle(player.AlwaysShow, false, true);
                    }

                    BpsPlayerManager.HideOrShowPlayer(player);
                });
        }

        public static void OnSelectUser(IUser user, bool isRemote)
        {
            if (isRemote) return;
            var selectedPlayer = BpsPlayerFromUserId(user.prop_String_0);

            _hidePlayer.Toggle(selectedPlayer.HidePlayer, false, true);
            _showPlayer.Toggle(selectedPlayer.AlwaysShow, false, true);
        }

        private static BpsPlayerObject BpsPlayerFromUserId(string uId)
        {
            return BpsPlayerManager.AllPlayers.Values.FirstOrDefault(p => uId == p.UserId);
        }

        private static void ToggleBpsActive(bool value)
        {
            BpsConfig.BpsEnabled.Value = value;
            MelonPreferences.Save();
            BpsPlayerManager.RefreshList();
        }

        private static void ToggleFriends(bool value)
        {
            BpsConfig.HideFriends.Value = value;
            MelonPreferences.Save();
            BpsPlayerManager.RefreshList();
        }

        private static void HiddenAvatarsToggle(bool value)
        {
            BpsConfig.AffectHiddenAvatar.Value = value;
            MelonPreferences.Save();
            BpsPlayerManager.RefreshList();
        }

        private static void HideAllByDefaultToggle(bool value)
        {
            BpsConfig.HideAllByDefault.Value = value;
            MelonPreferences.Save();
            BpsPlayerManager.RefreshList();
        }
    }
}