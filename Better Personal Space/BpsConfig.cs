using MelonLoader;

namespace Better_Personal_Space
{
    public class BpsConfig
    {
        private static readonly MelonPreferences_Category BpsCategory =
            MelonPreferences.CreateCategory("BPS", "Better Personal Space");

        public static MelonPreferences_Entry<bool> BpsEnabled, HideFriends, HideAllByDefault, AffectHiddenAvatar;
        public static MelonPreferences_Entry<float> PersonalSpace;
        public static void SettingsInit()
        {
            BpsEnabled = BpsCategory.CreateEntry(nameof(BpsEnabled), true, "Enable BPS");
            HideFriends = BpsCategory.CreateEntry(nameof(HideFriends), false, "Hide Friends",
                "Allows HideAllByDefault and AffectHiddenAvatars to hide your friends if true");
            HideAllByDefault = BpsCategory.CreateEntry(nameof(HideAllByDefault), false, "Hide all by default");
            AffectHiddenAvatar = BpsCategory.CreateEntry(nameof(AffectHiddenAvatar), true, "Affect Hidden Avatars");
            PersonalSpace = BpsCategory.CreateEntry(nameof(PersonalSpace), 1.0f, "Personal Space Size");

            PersonalSpace.OnValueChangedUntyped += FixButtonText;
        }

        private static void FixButtonText()
        {
            BpsUi.PersonalSpaceButton.Text = $"Personal Space: {PersonalSpace.Value}";
        }
    }
}