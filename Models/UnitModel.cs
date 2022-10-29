namespace BigDLL4221.Models
{
    public class UnitModel
    {
        public UnitModel(int id, string packageId, int unitNameId = 0, string name = "",
            bool lockedEmotion = false, int maxEmotionLevel = 0, bool autoPlay = false, bool summonedOnPlay = false,
            XmlVector2 customPos = null, string skinName = "")
        {
            Id = id;
            PackageId = packageId;
            Name = name;
            UnitNameId = unitNameId;
            LockedEmotion = lockedEmotion;
            MaxEmotionLevel = maxEmotionLevel;
            AutoPlay = autoPlay;
            SummonedOnPlay = summonedOnPlay;
            CustomPos = customPos;
            SkinName = skinName;
        }

        public int Id { get; set; }
        public string PackageId { get; set; }
        public string Name { get; set; }
        public int UnitNameId { get; set; }
        public bool LockedEmotion { get; set; }
        public int MaxEmotionLevel { get; set; }
        public bool AutoPlay { get; set; }
        public bool SummonedOnPlay { get; set; }
        public XmlVector2 CustomPos { get; set; }
        public string SkinName { get; set; }
    }
}