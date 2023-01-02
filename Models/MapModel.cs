using System;
using System.Collections.Generic;

namespace BigDLL4221.Models
{
    public class MapModel
    {
        public MapModel(Type component = null, string stage = null, bool isPlayer = true, bool oneTurnEgo = true,
            float bgx = 0.5f, float bgy = 0.5f, float fx = 0.5f, float fy = 407.5f / 1080f, float underx = 0.5f,
            float undery = 0.2777778f, bool initBgm = true,
            List<LorId> originalMapStageIds = null)
        {
            Component = component;
            Stage = stage;
            IsPlayer = isPlayer;
            OneTurnEgo = oneTurnEgo;
            Bgx = bgx;
            Bgy = bgy;
            Fx = fx;
            Fy = fy;
            UnderX = underx;
            UnderY = undery;
            InitBgm = initBgm;
            OriginalMapStageIds = originalMapStageIds ?? new List<LorId>();
        }

        public Type Component { get; set; }
        public string Stage { get; set; }
        public bool IsPlayer { get; set; }
        public bool OneTurnEgo { get; set; }
        public float Bgx { get; set; }
        public float Bgy { get; set; }
        public float Fx { get; set; }
        public float Fy { get; set; }
        public float UnderX { get; set; }
        public float UnderY { get; set; }
        public bool InitBgm { get; set; }
        public List<LorId> OriginalMapStageIds { get; set; }
    }
}