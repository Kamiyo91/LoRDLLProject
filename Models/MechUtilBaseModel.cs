using System.Collections.Generic;
using BigDLL4221.Enum;
using LOR_XML;

namespace BigDLL4221.Models
{
    public class MechUtilBaseModel
    {
        public MechUtilBaseModel(BattleUnitModel owner, LorId thisPassiveId, EgoOptions egoOptions = null,
            int surviveHp = 0, int recoverToHp = 0, bool survive = false, bool recoverLightOnSurvive = false,
            bool dieOnFightEnd = false, List<AbnormalityCardDialog> surviveAbDialogList = null,
            AbColorType surviveAbDialogColor = AbColorType.Negative, BattleUnitBuf nearDeathBuffType = null,
            Dictionary<LorId, PersonalCardOptions> personalCards = null)
        {
            Owner = owner;
            ThisPassiveId = thisPassiveId;
            EgoOptions = egoOptions;
            SurviveHp = surviveHp;
            RecoverToHp = recoverToHp;
            Survive = survive;
            RecoverLightOnSurvive = recoverLightOnSurvive;
            DieOnFightEnd = dieOnFightEnd;
            SurviveAbDialogList = surviveAbDialogList ?? new List<AbnormalityCardDialog>();
            SurviveAbDialogColor = surviveAbDialogColor;
            NearDeathBuffType = nearDeathBuffType;
            PersonalCards = personalCards ?? new Dictionary<LorId, PersonalCardOptions>();
        }

        public BattleUnitModel Owner { get; set; }
        public LorId ThisPassiveId { get; set; }
        public EgoOptions EgoOptions { get; set; }
        public int SurviveHp { get; set; }
        public int RecoverToHp { get; set; }
        public bool Survive { get; set; }
        public bool RecoverLightOnSurvive { get; set; }
        public bool DieOnFightEnd { get; set; }
        public List<AbnormalityCardDialog> SurviveAbDialogList { get; set; }
        public AbColorType SurviveAbDialogColor { get; set; }
        public BattleUnitBuf NearDeathBuffType { get; set; }
        public Dictionary<LorId, PersonalCardOptions> PersonalCards { get; set; }
    }

    public class EgoOptions
    {
        public EgoOptions(BattleUnitBuf egoType, LorId egoCardId, string skinName = "", bool refreshUI = false,
            Dictionary<LorId, MapModel> egoMaps = null, List<LorId> additionalPassiveIds = null,
            List<AbnormalityCardDialog> egoAbDialogList = null, AbColorType egoAbColorColor = AbColorType.Negative,
            int duration = 0)
        {
            EgoType = egoType;
            EgoCardId = egoCardId;
            EgoMaps = egoMaps ?? new Dictionary<LorId, MapModel>();
            EgoActivated = false;
            AdditionalPassiveIds = additionalPassiveIds ?? new List<LorId>();
            RefreshUI = refreshUI;
            SkinName = skinName;
            EgoAbDialogList = egoAbDialogList ?? new List<AbnormalityCardDialog>();
            EgoAbColorColor = egoAbColorColor;
            Duration = duration;
            Count = 0;
            ActivatedMap = null;
        }

        public BattleUnitBuf EgoType { get; set; }
        public LorId EgoCardId { get; set; }
        public Dictionary<LorId, MapModel> EgoMaps { get; set; }
        public bool EgoActivated { get; set; }
        public List<LorId> AdditionalPassiveIds { get; set; }
        public bool RefreshUI { get; set; }
        public string SkinName { get; set; }
        public List<AbnormalityCardDialog> EgoAbDialogList { get; set; }
        public AbColorType EgoAbColorColor { get; set; }
        public int Duration { get; set; }
        public int Count { get; set; }
        public MapModel ActivatedMap { get; set; }
    }

    public class PersonalCardOptions
    {
        public PersonalCardOptions(bool egoPersonalCard = false, bool onPlayCard = false, bool expireAfterUse = false)
        {
            ExpireAfterUse = expireAfterUse;
            EgoPersonalCard = egoPersonalCard;
            OnPlayCard = onPlayCard;
        }

        public bool ExpireAfterUse { get; set; }
        public bool EgoPersonalCard { get; set; }
        public bool OnPlayCard { get; set; }
    }
}