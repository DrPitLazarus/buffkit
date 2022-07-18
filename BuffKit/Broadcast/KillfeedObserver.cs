using Muse.Goi2.Entity;

namespace BuffKit.Broadcast
{
    public class KillfeedObserver
    {
        private static string GetSubjectText(Announcement announcement)
        {
            string result = string.Empty;
            if (announcement.Subject != null && announcement.Subject.Name != null)
            {
                result = EntityExtensions.FilterPlayerPlatform(announcement.Subject.Name, true);
            }
            else if (announcement.Object != null && announcement.Object.Name != null)
            {
                result = "A Harsh World";
            }
            return result;
        }
        private static string GetVerbText(Announcement announcement)
        {
            string result = string.Empty;
            if (announcement.Verb != AnnouncementVerb.None)
            {
                result = announcement.Verb.ToString();
            }
            return result;
        }
        private static string GetObjectText(Announcement announcement)
        {
            string result = string.Empty;
            if (announcement.Object != null && announcement.Object.Name != null)
            {
                if (!string.IsNullOrEmpty(announcement.Object.ShipName))
                {
                    result = "{0}'s {1}".F(new object[]
                    {
                    announcement.Object.ShipName,
                    announcement.Object.Name
                    });
                }
                else
                {
                    result = announcement.Object.Name;
                }
            }
            return result;
        }
        private static string GetWithText(Announcement announcement)
        {
            string result = string.Empty;
            if (announcement.With != null && announcement.With.Name != null)
            {
                if (!string.IsNullOrEmpty(announcement.With.ShipName))
                {
                    result = "{0}'s {1}".F(new object[]
                    {
                    announcement.With.ShipName,
                    announcement.With.Name
                    });
                }
                else
                {
                    result = announcement.With.Name;
                }
                result = " with " + result;
            }
            return result;
        }

        private static string GetSubjectColor(Announcement announcement, string neutralTeamTextCol)
        {
            string colorAsHex = neutralTeamTextCol;
            if (announcement.Subject != null)
            {
                colorAsHex = TeamColors.GetColorAsHex(announcement.Subject.Side);
            }
            return string.Format("#{0}ff", colorAsHex);
        }
        private static string GetObjectColor(Announcement announcement, string defaultTextCol)
        {

            string colorAsHex = defaultTextCol;
            if (announcement.Object != null)
            {
                colorAsHex = TeamColors.GetColorAsHex(announcement.Object.Side);
            }
            return string.Format("#{0}ff", colorAsHex);
        }

        public static void HandleAnnouncement(Announcement announcement, string defaultTextCol, string neutralTeamTextCol)
        {
            string subjectText = GetSubjectText(announcement);
            string verbText = GetVerbText(announcement);
            string objectText = GetObjectText(announcement);
            //string withText = GetWithText(announcement);

            string subjectColor = GetSubjectColor(announcement, neutralTeamTextCol);
            string verbColor = string.Format("#{0}ff", defaultTextCol);
            string objectColor = GetObjectColor(announcement, defaultTextCol);

            string text = string.Format(
                "<color={0}>{1}</color> <color={2}>{3}</color> <color={4}>{5}</color>",
                subjectColor, subjectText,
                verbColor, verbText,
                objectColor, objectText
            );

            MatchDataObserver instance = MatchDataObserver.Instance;
            if (instance != null)
                MatchDataObserver.Instance.AddKillfeedEntry(text);
            else
                MuseLog.Error("Could not send killfeed entry \"" + text + "\" as MatchDataObserver not found");
        }
    }
}
