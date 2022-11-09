﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BigDLL4221.Enum;
using BigDLL4221.Models;
using BigDLL4221.Utils;
using HarmonyLib;
using LOR_XML;

namespace BigDLL4221.Harmony
{
    [HarmonyPatch]
    public class EmotionSelectionUnitPatch
    {
        private static FieldInfo _matchField;

        private static readonly FieldInfo
            NeedUnitSelection = AccessTools.Field(typeof(LevelUpUI), "_needUnitSelection");

        private static readonly Predicate<BattleUnitModel> MatchAddon = x =>
            ModParameters.PassiveOptions.Any(y =>
                x.passiveDetail.PassiveList.Any(z =>
                    z.id.packageId == y.Key &&
                    y.Value.Any(v => v.PassiveId == z.id.id && v.BannedEmotionCardSelection))) ||
            ModParameters.KeypageOptions.Any(y =>
                x.Book.BookId.packageId == y.Key &&
                y.Value.Any(z => z.KeypageId == x.Book.BookId.id && z.BannedEmotionCards));

        [HarmonyPatch]
        public class BlockSelectionBubble
        {
            [HarmonyPostfix]
            public static void LevelUpUI_Predicate_Patch(BattleUnitModel x, ref bool __result)
            {
                __result |= MatchAddon(x);
            }

            [HarmonyTargetMethod]
            public static MethodBase LevelUpUI_Predicate_Find()
            {
                var typeInfo = typeof(LevelUpUI).GetTypeInfo().DeclaredNestedTypes
                    .FirstOrDefault(x => x.Name.Contains("<>c"));
                _matchField = typeInfo?.DeclaredFields.FirstOrDefault(x => x.Name.Contains("<>9__55_0"));
                return typeInfo?.DeclaredMethods.FirstOrDefault(x => x.Name.Contains("OnSelectRoutine"));
            }
        }

        [HarmonyPatch]
        public class BlockUiRepeat
        {
            private static FieldInfo _state;

            [HarmonyPrefix]
            public static void LevelUpUI_OnSelectRoutine_Pre(object __instance, ref int __state)
            {
                __state = (int)_state.GetValue(__instance);
            }

            [HarmonyPostfix]
            public static void LevelUpUI_OnSelectRoutine_Post(object __instance, ref int __state)
            {
                if (__state != 1 || (int)_state.GetValue(__instance) != -1 ||
                    !(bool)NeedUnitSelection.GetValue(SingletonBehavior<BattleManagerUI>.Instance.ui_levelup)) return;
                var list = BattleObjectManager.instance.GetAliveList(Faction.Player);
                list.RemoveAll((Predicate<BattleUnitModel>)_matchField.GetValue(null));
                if (list.Count > 0) return;
                StageController.Instance.GetCurrentStageFloorModel().team.egoSelectionPoint--;
                StageController.Instance.GetCurrentStageFloorModel().team.currentSelectEmotionLevel++;
                NeedUnitSelection.SetValue(SingletonBehavior<BattleManagerUI>.Instance.ui_levelup, false);
                foreach (var unit in BattleObjectManager.instance.GetAliveList(Faction.Player))
                    UnitUtil.BattleAbDialog(unit.view.dialogUI, new List<AbnormalityCardDialog>
                    {
                        new AbnormalityCardDialog
                        {
                            id = "EmotionError",
                            dialog = "Emotion Error, can't be used on any character"
                        }
                    }, AbColorType.Negative);
            }

            [HarmonyTargetMethod]
            public static MethodBase LevelUpUIOnSelectRoutine_Find()
            {
                var typeInfo = typeof(LevelUpUI).GetTypeInfo().DeclaredNestedTypes
                    .FirstOrDefault(x => x.Name.Contains("<OnSelectRoutine>d__55"));
                _state = typeInfo?.DeclaredFields.ToList().FirstOrDefault(x => x.Name.Contains("__state"));
                return typeInfo?.DeclaredMethods.ToList().FirstOrDefault(x => x.Name.Contains("MoveNext"));
            }
        }
    }
}