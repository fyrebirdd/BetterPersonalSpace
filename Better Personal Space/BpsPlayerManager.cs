using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;

namespace Better_Personal_Space
{
    public class BpsPlayerManager
    {
        public static readonly Dictionary<int, BpsPlayerObject> AllPlayers = new();
        public static readonly Dictionary<int, BpsPlayerObject> HiddenPlayers = new();

        public static void Init()
        {
            BpsUtils.OnPlayerJoined += OnPlayerJoin;
            BpsUtils.OnPlayerLeft += OnPlayerLeft;
            BpsUtils.OnFriended += OnFriended;
            BpsUtils.OnUnfriended += OnUnfriended;
            BpsUtils.OnPlayerModerationSent += OnPlayerModerationAdd;
            BpsUtils.OnPlayerModerationRemoved += OnPlayerModerationRemove;
            BpsUtils.OnAvatarChanged += OnAvatarChanged;
        }

        public static void OnSceneLoad()
        {
            AllPlayers.Clear();
            HiddenPlayers.Clear();
        }

        private static void OnPlayerJoin(Player newJoiner)
        {
            if (newJoiner == null || newJoiner.prop_APIUser_0 == null ||
                newJoiner == BpsUtils.GetPlayerFromApiUser(APIUser.CurrentUser)) return;

            var photonId = newJoiner.prop_VRCPlayer_0.prop_PlayerNet_0.prop_PhotonView_0.field_Private_Int32_0;

            if (AllPlayers.ContainsKey(photonId)) return;

            var newPlayer = new BpsPlayerObject
            {
                PhotonId = photonId,
                UserId = newJoiner.prop_APIUser_0.id,
                Player = newJoiner,
                Avatar = newJoiner.prop_VRCPlayer_0.prop_VRCAvatarManager_0.prop_GameObject_0,
                IsFriend = APIUser.IsFriendsWith(newJoiner.prop_APIUser_0.id),
                AvatarHidden = BpsUtils.IsAvatarExplicitlyHidden(newJoiner.prop_APIUser_0),
                AlwaysShow = false,
                HidePlayer = false
            };
            AllPlayers.Add(newPlayer.PhotonId, newPlayer);
            HideOrShowPlayer(newPlayer);
        }

        private static void OnPlayerLeft(Player leavingPlayer)
        {
            if (leavingPlayer.prop_APIUser_0 == null) return;

            var leavingPlayerId = leavingPlayer.prop_APIUser_0.id;
            BpsPlayerObject leaver = null;

            foreach (var otherPlayer in AllPlayers.Values.Where(otherPlayer => leavingPlayerId == otherPlayer.UserId))
                leaver = otherPlayer;

            if (leaver == null) return;
            if (HiddenPlayers.ContainsKey(leaver.PhotonId)) HiddenPlayers.Remove(leaver.PhotonId);
            AllPlayers.Remove(leaver.PhotonId);
        }

        private static void OnFriended(APIUser newFriend)
        {
            foreach (var otherPlayer in AllPlayers.Values.Where(otherPlayer => otherPlayer.UserId == newFriend.id))
            {
                otherPlayer.IsFriend = true;
                HideOrShowPlayer(otherPlayer);
            }
        }

        private static void OnUnfriended(string userId)
        {
            foreach (var player in AllPlayers.Values.Where(player => player.UserId == userId))
            {
                player.IsFriend = false;
                HideOrShowPlayer(player);
            }
        }

        private static void OnPlayerModerationAdd(string userId, ApiPlayerModeration.ModerationType type)
        {
            if (type != ApiPlayerModeration.ModerationType.HideAvatar) return;
            foreach (var otherPlayer in AllPlayers.Values.Where(otherPlayer => otherPlayer.UserId == userId))
            {
                otherPlayer.AvatarHidden = true;
                HideOrShowPlayer(otherPlayer);
            }
        }

        private static void OnPlayerModerationRemove(string userId, ApiPlayerModeration.ModerationType type)
        {
            if (type != ApiPlayerModeration.ModerationType.HideAvatar) return;
            foreach (var otherPlayer in AllPlayers.Values.Where(otherPlayer => otherPlayer.UserId == userId))
            {
                otherPlayer.AvatarHidden = false;
                HideOrShowPlayer(otherPlayer);
            }
        }

        private static void OnAvatarChanged(VRCAvatarManager avMan, GameObject newAvatar)
        {
            var photonId = avMan.field_Private_VRCPlayer_0.prop_PlayerNet_0.prop_PhotonView_0.field_Private_Int32_0;
            if (!AllPlayers.ContainsKey(photonId)) return;
            AllPlayers[photonId].SetAvatar(newAvatar);
        }

        public static void HideOrShowPlayer(BpsPlayerObject otherPlayer)
        {
            if (!BpsConfig.BpsEnabled.Value) return;
            var addPlayerToHidden = false;
            if (otherPlayer.IsFriend)
            {
                if (BpsConfig.HideFriends.Value)
                    if (BpsConfig.AffectHiddenAvatar.Value && otherPlayer.AvatarHidden ||
                        BpsConfig.HideAllByDefault.Value)
                        addPlayerToHidden = true;
            }
            else
            {
                if (BpsConfig.AffectHiddenAvatar.Value && otherPlayer.AvatarHidden || BpsConfig.HideAllByDefault.Value)
                    addPlayerToHidden = true;
            }

            if (otherPlayer.AlwaysShow) addPlayerToHidden = false;
            if (otherPlayer.HidePlayer) addPlayerToHidden = true;

            switch (addPlayerToHidden)
            {
                case true when !HiddenPlayers.ContainsKey(otherPlayer.PhotonId):
                    HiddenPlayers.Add(otherPlayer.PhotonId, otherPlayer);
                    break;
                case false when HiddenPlayers.ContainsKey(otherPlayer.PhotonId):
                    HiddenPlayers.Remove(otherPlayer.PhotonId);
                    break;
            }
        }

        public static void RefreshList()
        {
            if (!BpsConfig.BpsEnabled.Value)
            {
                foreach (var hiddenPlayer in HiddenPlayers.Values)
                {
                    hiddenPlayer.ShowAvatar();
                }
                HiddenPlayers.Clear();
            }
            
            foreach (var hiddenPlayer in HiddenPlayers.Values)
            {
                hiddenPlayer.ShowAvatar();
            }
            HiddenPlayers.Clear();
            foreach (var otherPlayer in AllPlayers.Values)
            {
                if (!BpsConfig.BpsEnabled.Value) return;
                var addPlayerToHidden = false;
                if (otherPlayer.IsFriend)
                {
                    if (BpsConfig.HideFriends.Value)
                        if (BpsConfig.AffectHiddenAvatar.Value && otherPlayer.AvatarHidden ||
                            BpsConfig.HideAllByDefault.Value)
                            addPlayerToHidden = true;
                }
                else
                {
                    if (BpsConfig.AffectHiddenAvatar.Value && otherPlayer.AvatarHidden ||
                        BpsConfig.HideAllByDefault.Value) addPlayerToHidden = true;
                }

                if (otherPlayer.AlwaysShow) addPlayerToHidden = false;
                if (otherPlayer.HidePlayer) addPlayerToHidden = true;

                if (addPlayerToHidden) HiddenPlayers.Add(otherPlayer.PhotonId, otherPlayer);
            }
        }
    }
}