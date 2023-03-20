using System;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace BigDLL4221.Utils
{
    public static class StageButtonTool
    {
        public static bool IsTurningPage;

        public static UICharacterList enemyCharacterList;

        public static UICharacterList librarianCharacterList;

        public static int currentEnemyUnitIndex;

        public static int currentLibrarianUnitIndex;

        public static Button EnemyUP;

        public static Button EnemyDown;

        public static Button LibrarianUP;

        public static Button LibrarianDown;

        static StageButtonTool()
        {
            var transform = UIPanelTool.GetEnemyCharacterListPanel().transform.Find("PanelActiveController");
            var parent = UIPanelTool.GetLibrarianCharacterListPanel().transform.Find("PanelActiveController");
            var sprite = transform.Find("[Rect]StageEnemyList/[Button]Left/Image").GetComponent<Image>().sprite;
            ButtonColor.OnEnterColor = new Color(0.13333334f, 1f, 0.89411765f);
            EnemyUP = UtilTools.CreateButton(transform, sprite, new Vector2(0.07f, 0.07f), new Vector2(550f, -105f));
            EnemyUP.name = "[Button]up";
            EnemyUP.image.color = new Color(1f, 1f, 1f);
            EnemyUP.transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
            EnemyUP.gameObject.AddComponent<ButtonColor>().Image = EnemyUP.image;
            LibrarianUP = UtilTools.CreateButton(parent, sprite, new Vector2(0.07f, 0.07f), new Vector2(-550f, -95f));
            LibrarianUP.name = "[Button]up";
            LibrarianUP.image.color = new Color(1f, 1f, 1f);
            LibrarianUP.transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
            LibrarianUP.gameObject.AddComponent<ButtonColor>().Image = LibrarianUP.image;
            EnemyDown = UtilTools.CreateButton(transform, sprite, new Vector2(0.07f, 0.07f), new Vector2(550f, -270f));
            EnemyDown.name = "[Button]down";
            EnemyDown.image.color = new Color(1f, 1f, 1f);
            EnemyDown.gameObject.AddComponent<ButtonColor>().Image = EnemyDown.image;
            LibrarianDown =
                UtilTools.CreateButton(parent, sprite, new Vector2(0.07f, 0.07f), new Vector2(-550f, -260f));
            LibrarianDown.name = "[Button]down";
            LibrarianDown.image.color = new Color(1f, 1f, 1f);
            LibrarianDown.gameObject.AddComponent<ButtonColor>().Image = LibrarianDown.image;
            Init();
            EnemyUP.gameObject.SetActive(false);
            EnemyDown.gameObject.SetActive(false);
            LibrarianUP.gameObject.SetActive(false);
            LibrarianDown.gameObject.SetActive(false);
        }

        public static void Init()
        {
            EnemyUP.onClick.AddListener(OnClickEnemyUP);
            EnemyDown.onClick.AddListener(OnClickEnemyDown);
            LibrarianUP.onClick.AddListener(OnClickLibrarianUP);
            LibrarianDown.onClick.AddListener(OnClickLibrarianDown);
        }

        public static void RefreshEnemy()
        {
            if (enemyCharacterList == null) enemyCharacterList = UIPanelTool.GetEnemyCharacterListPanel().CharacterList;
            currentEnemyUnitIndex = 0;
            var originalColor = enemyCharacterList.slotList[0].originalColor;
            EnemyUP.gameObject.GetComponent<ButtonColor>().Image.color = originalColor;
            EnemyDown.gameObject.GetComponent<ButtonColor>().Image.color = originalColor;
            EnemyUP.gameObject.GetComponent<ButtonColor>().DefaultColor = originalColor;
            EnemyDown.gameObject.GetComponent<ButtonColor>().DefaultColor = originalColor;
            EnemyUP.gameObject.SetActive(false);
            if (UIPanel.Controller.CurrentUIPhase != UIPhase.BattleSetting) return;
            if (Singleton<StageController>.Instance.GetStageModel() != null &&
                Singleton<StageController>.Instance.GetCurrentWaveModel().UnitList.Count > 5)
            {
                EnemyDown.gameObject.SetActive(true);
                return;
            }

            EnemyDown.gameObject.SetActive(false);
        }

        public static void RefreshLibrarian()
        {
            if (librarianCharacterList == null)
                librarianCharacterList = UIPanelTool.GetLibrarianCharacterListPanel().CharacterList;
            currentLibrarianUnitIndex = 0;
            var originalColor = librarianCharacterList.slotList[0].originalColor;
            LibrarianUP.gameObject.GetComponent<ButtonColor>().Image.color = originalColor;
            LibrarianDown.gameObject.GetComponent<ButtonColor>().Image.color = originalColor;
            LibrarianUP.gameObject.GetComponent<ButtonColor>().DefaultColor = originalColor;
            LibrarianDown.gameObject.GetComponent<ButtonColor>().DefaultColor = originalColor;
            LibrarianUP.gameObject.SetActive(false);
            if (UIPanel.Controller.CurrentUIPhase == UIPhase.BattleSetting)
            {
                if (Singleton<StageController>.Instance.GetCurrentStageFloorModel() != null &&
                    Singleton<StageController>.Instance.GetCurrentStageFloorModel().GetUnitBattleDataList().Count > 5)
                    LibrarianDown.gameObject.SetActive(true);
                else
                    LibrarianDown.gameObject.SetActive(false);
            }

            IsTurningPage = false;
        }

        public static void OnClickEnemyUP()
        {
            try
            {
                if (currentEnemyUnitIndex <= 0)
                {
                    EnemyUP.gameObject.SetActive(false);
                }
                else
                {
                    currentEnemyUnitIndex -= 5;
                    if (currentEnemyUnitIndex <= 0)
                    {
                        currentEnemyUnitIndex = 0;
                        EnemyUP.gameObject.SetActive(false);
                    }

                    UpdateEnemyCharacterList();
                    if (Singleton<StageController>.Instance.GetStageModel() != null &&
                        Singleton<StageController>.Instance.GetCurrentWaveModel().UnitList.Count -
                        currentEnemyUnitIndex > 5)
                        EnemyDown.gameObject.SetActive(true);
                    else
                        EnemyDown.gameObject.SetActive(false);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void OnClickEnemyDown()
        {
            try
            {
                if (Singleton<StageController>.Instance.GetStageModel() != null &&
                    Singleton<StageController>.Instance.GetCurrentWaveModel().UnitList.Count - currentEnemyUnitIndex <=
                    5)
                {
                    EnemyDown.gameObject.SetActive(false);
                }
                else
                {
                    currentEnemyUnitIndex += 5;
                    EnemyUP.gameObject.SetActive(true);
                    UpdateEnemyCharacterList();
                    if (Singleton<StageController>.Instance.GetStageModel() != null &&
                        Singleton<StageController>.Instance.GetCurrentWaveModel().UnitList.Count -
                        currentEnemyUnitIndex <= 5)
                        EnemyDown.gameObject.SetActive(false);
                    else
                        EnemyDown.gameObject.SetActive(true);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void OnClickLibrarianUP()
        {
            try
            {
                if (currentLibrarianUnitIndex <= 0)
                {
                    LibrarianUP.gameObject.SetActive(false);
                }
                else
                {
                    currentLibrarianUnitIndex -= 5;
                    if (currentLibrarianUnitIndex <= 0)
                    {
                        currentLibrarianUnitIndex = 0;
                        LibrarianUP.gameObject.SetActive(false);
                    }

                    UpdateLibrarianCharacterList();
                    if (Singleton<StageController>.Instance.GetCurrentStageFloorModel() != null &&
                        Singleton<StageController>.Instance.GetCurrentStageFloorModel().GetUnitBattleDataList().Count -
                        currentLibrarianUnitIndex > 5)
                        LibrarianDown.gameObject.SetActive(true);
                    else
                        LibrarianDown.gameObject.SetActive(false);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void OnClickLibrarianDown()
        {
            try
            {
                if (Singleton<StageController>.Instance.GetCurrentStageFloorModel() != null &&
                    Singleton<StageController>.Instance.GetCurrentStageFloorModel().GetUnitBattleDataList().Count -
                    currentLibrarianUnitIndex <= 5)
                {
                    LibrarianDown.gameObject.SetActive(false);
                }
                else
                {
                    currentLibrarianUnitIndex += 5;
                    LibrarianUP.gameObject.SetActive(true);
                    UpdateLibrarianCharacterList();
                    if (Singleton<StageController>.Instance.GetCurrentStageFloorModel() != null &&
                        Singleton<StageController>.Instance.GetCurrentStageFloorModel().GetUnitBattleDataList().Count -
                        currentLibrarianUnitIndex <= 5)
                        LibrarianDown.gameObject.SetActive(false);
                    else
                        LibrarianDown.gameObject.SetActive(true);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void UpdateEnemyCharacterList()
        {
            var currentWaveModel = Singleton<StageController>.Instance.GetCurrentWaveModel();
            if (currentWaveModel == null) return;
            if (UIPanel.Controller.CurrentUIPhase != UIPhase.BattleSetting) return;
            var num = currentWaveModel.GetUnitBattleDataList().Count - currentEnemyUnitIndex;
            if (num <= 0) return;
            var range = currentWaveModel.GetUnitBattleDataList()
                .GetRange(currentEnemyUnitIndex, Math.Min(5, num));
            UIPanelTool.GetEnemyCharacterListPanel().SetCharacterRenderer(range, false);
            enemyCharacterList.InitBattleEnemyList(range);
        }

        public static void UpdateLibrarianCharacterList()
        {
            var currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
            if (currentStageFloorModel == null) return;

            if (UIPanel.Controller.CurrentUIPhase != UIPhase.BattleSetting) return;
            var num = currentStageFloorModel.GetUnitBattleDataList().Count - currentLibrarianUnitIndex;
            if (num <= 0) return;
            var range = currentStageFloorModel.GetUnitBattleDataList()
                .GetRange(currentLibrarianUnitIndex, Math.Min(5, num));
            UIPanelTool.GetLibrarianCharacterListPanel().SetCharacterRenderer(range, false);
            IsTurningPage = true;
            librarianCharacterList.InitUnitListFromBattleData(range);
        }
    }
}