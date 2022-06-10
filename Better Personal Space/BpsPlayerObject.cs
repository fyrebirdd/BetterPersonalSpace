using UnityEngine;
using VRC;

namespace Better_Personal_Space
{
    public class BpsPlayerObject
    {
        public bool AlwaysShow;
        public GameObject Avatar;
        public bool AvatarHidden;
        public bool HidePlayer;

        public bool IsFriend;
        public int PhotonId;
        public Player Player;
        public string UserId;
        public Vector3 Pos => Player.transform.position;

        private bool _isCurrentlyHidden;
        
        public void HideAvatar()
        {
            if (Avatar == null) return;
            Avatar.SetActive(false);
            _isCurrentlyHidden = true;
        }

        public void ShowAvatar()
        {
            if (Avatar == null || !_isCurrentlyHidden) return;
            Avatar.SetActive(true);
            _isCurrentlyHidden = false;
        }
        public void SetAvatar(GameObject newAvatar)
        {
            if (newAvatar == null) return;
            Avatar = newAvatar;
            Avatar.SetActive(true);
        }
    }
}