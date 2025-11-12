using System;
using Duckov.UI;
using Duckov.Modding;
using Duckov.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting.FullSerializer;
using System.Runtime.CompilerServices;


namespace WeightViewer
{
    public class ModBehaviour: Duckov.Modding.ModBehaviour
    {
        void Awake()
        {
            Log("Loaded!!!");
        }
        void OnDestroy()
        {
            Log("Destroy!!!");

        }
        void OnEnable()
        {
            View.OnActiveViewChanged += OnActiveViewChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            if (ModConfigAPI.IsAvailable())
            {
                SetupModConfig();
                LoadConfigFromModConfig();
            }

        }
        void OnDisable()
        {
            View.OnActiveViewChanged -= OnActiveViewChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnActiveViewChanged()
        {
            Log($"ActiveView: {View.ActiveView?.name}");
            isDirty = true;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log($"{scene.name}");
            if (hudObject == null)
            {
                CreateUI_HealthHUD();
                if (hudObject == null) return;
            }
            isDirty = true;
        }
        void OnSceneUnloaded(Scene scene)
        {
            Log($"{scene.name}");
            isDirty = true;
        }

        public CharacterMainControl? character;
        private GameObject? hudObject;
        private Text? hudWeightText;
        private Text? hudHeavyText;
        private Text? hudOverText;

        private GameObject? viewObject;
        private Text? viewHeavyText;
        private Text? viewOverText;

        private const float updateInterval = 30.0f;
        private float weightCooldown = updateInterval + 1f;
        private float prevTotalWeight = 0f;
        private float prevMaxWeight = 0f;
        private bool isDirty = true;

        Color normalColor = new Color32(200, 225, 110, 255);
        Color heavyColor = new Color32(255, 190, 30, 255);
        Color overColor = new Color32(255, 120, 120, 255);

        public class ModConfig
        {
            public bool showMain = true;
            public bool showInventory = true;
        }

        private ModConfig config = new ModConfig();
        private const string MOD_NAME = "WeightViewer";


        void CreateUI_HealthHUD()
        {
            if (hudObject != null)
            {
                return;
            }

            HealthHUD hud = HUDManager.FindFirstObjectByType<HealthHUD>();
            if (hud == null)
            {
                Log("not found healthHUD");
                return;
            }

            hudObject = new GameObject("HudWeightCanvas");
            hudObject.transform.SetParent(hud.transform, false);

            Canvas canvas = hudObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            
            GameObject backgroundPanel = new GameObject("WeightBackgroundPanel");
            backgroundPanel.transform.SetParent(hudObject.transform, false);

            Image backgroundImage = backgroundPanel.AddComponent<Image>();
            backgroundImage.color = new Color32(0, 0, 0, 220);
            RectTransform panelRect = backgroundPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);

            panelRect.anchoredPosition = new Vector2(-80f, -8f);
            panelRect.sizeDelta = new Vector2(280f, 40f);

            hudWeightText = CreateTextElement(backgroundPanel.transform, "weightText", 20, normalColor);
            hudHeavyText = CreateTextElement(backgroundPanel.transform, "heavyWeight", 20, heavyColor);
            hudOverText = CreateTextElement(backgroundPanel.transform, "overWeight", 20, overColor);

            hudHeavyText.alignment = TextAnchor.MiddleRight;
            hudOverText.alignment = TextAnchor.MiddleRight;

            SetPos(hudWeightText.rectTransform, 5f, 0f);
            SetPos(hudHeavyText.rectTransform, -85f, 0f);
            SetPos(hudOverText.rectTransform, -5f, 0f);
        }

        void CreateUI_LootView()
        {
            if (viewObject != null)
            {
                return;
            }

            WeightBarHUD[] huds = HUDManager.FindObjectsByType<WeightBarHUD>(FindObjectsSortMode.None);
            if (huds.Length == 0)
            {
                Log("not found WeightBarHUD");
                return;
            }

            WeightBarHUD hud = huds[0];
            bool found = false;
            foreach (WeightBarHUD h in huds)
            {
                if (h.transform.parent.name == "BarArea")
                {
                    hud = h;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Log("not found BarArea");
                return;
            }

            viewObject = new GameObject("ViewWeightCanvas");
            viewObject.transform.SetParent(hud.weightText.transform, false);

            Canvas canvas = viewObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;

            GameObject backgroundPanel = new GameObject("ViewWeightBackgroundPanel");
            backgroundPanel.transform.SetParent(viewObject.transform, false);

            Image backgroundImage = backgroundPanel.AddComponent<Image>();
            backgroundImage.color = new Color32(0, 0, 0, 255);
            RectTransform panelRect = backgroundPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);

            panelRect.anchoredPosition = new Vector2(130f, 29f);
            panelRect.sizeDelta = new Vector2(210f, 42f);

            viewHeavyText = CreateTextElement(backgroundPanel.transform, "heavyWeight", 22, heavyColor);
            viewOverText = CreateTextElement(backgroundPanel.transform, "overWeight", 22, overColor);

            viewHeavyText.alignment = TextAnchor.MiddleRight;
            viewOverText.alignment = TextAnchor.MiddleRight;

            SetPos(viewHeavyText.rectTransform, -110f, 0f);
            SetPos(viewOverText.rectTransform, -20f, 0f);

        }
        Text CreateTextElement(Transform parent, string id, int size, Color color)
        {
            GameObject textObject = new GameObject(id);
            textObject.transform.SetParent(parent, false);
            Text txt = textObject.AddComponent<Text>();

            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.text = id;
            txt.fontSize = size;
            txt.fontStyle = FontStyle.Normal;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = color;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(1, -1);
            return txt;
        }
        void SetPos(RectTransform rect, float x, float y)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.offsetMin = new Vector2(x + 10, y + 10);
            rect.offsetMax = new Vector2(x - 10, y - 10);
        }

        public void Update()
        {
            UpdateWeight();

            weightCooldown += Time.deltaTime;
            if (isDirty || weightCooldown > updateInterval)
            {
                RefreshWeightText(prevTotalWeight, prevMaxWeight);
                weightCooldown = 0f;
                isDirty = false;
            }

        }

        void UpdateWeight()
        {
            CharacterMainControl main = CharacterMainControl.Main;
            if (!main)
            {
                return;
            }
            float totalWeight = main.CharacterItem.TotalWeight;
            float maxWeight = main.MaxWeight;
            CA_Carry carry = main.carryAction;
            if (carry != null && carry.Running)
            {
                totalWeight += carry.GetWeight();
            }

            if (prevTotalWeight != totalWeight || prevMaxWeight != maxWeight)
            {
                prevTotalWeight = totalWeight;
                prevMaxWeight = maxWeight;
                isDirty = true;
            }
        }

        void RefreshWeightText(float totalWeight, float maxWeight)
        {
            float heavyWeight = maxWeight * 0.75f;
            string heavy;
            string over;
            Color color;
            if (totalWeight > maxWeight)
            {
                heavy = $"+{totalWeight - heavyWeight:F1}";
                over = $"+{totalWeight - maxWeight:F1}";
                color = overColor;
            }
            else if (totalWeight > heavyWeight)
            {
                heavy = $"+{totalWeight - heavyWeight:F1}";
                over = $"-{maxWeight - totalWeight:F1}";
                color = heavyColor;
            }
            else
            {
                heavy = $"-{heavyWeight - totalWeight:F1}";
                over = $"-{maxWeight - totalWeight:F1}";
                color = normalColor;
            }

            if (hudObject)
            {
                if (config.showMain)
                {
                    hudWeightText.color = color;
                    hudWeightText.text = $"{totalWeight:F1}kg";
                    hudHeavyText.text = $"{heavy}kg";
                    hudOverText.text = $"{over}kg";
                } 
                else
                {
                    hudObject.SetActive(false);
                }
            }

            CreateUI_LootView();
            if (viewObject)
            {
                if (config.showInventory)
                {
                    viewHeavyText.text = $"{heavy}kg";
                    viewOverText.text = $"{over}kg";
                }
                else
                {
                    viewObject.SetActive(false);
                }
            }
        }

        void Log(String str, string callerName = "")
        //void Log(String str, [CallerMemberName] string callerName = "")
        {
            //Debug.Log($"{Time.time:F4}|[WV] {callerName}() {str}");
        }


        ///////////////////////////////////////////
        // ModConfig
        
        private void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Log("ModConfigAPI not available!");
                return;
            }

            try
            {
                ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);
                ModConfigAPI.SafeAddBoolDropdownList(
                    MOD_NAME,
                    "showMain",
                    ModLocalization.GetTranslation(ModLocalization.ConfigShowMainKey),
                    config.showMain
                );
                ModConfigAPI.SafeAddBoolDropdownList(
                    MOD_NAME,
                    "showInventory",
                    ModLocalization.GetTranslation(ModLocalization.ConfigShowInventoryKey),
                    config.showInventory
                );

                Log("ModConfig setup completed successfully!");
            }
            catch (Exception ex)
            {
                Log($"Error setting up ModConfig: {ex.Message}");
            }
        }

        private void LoadConfigFromModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                return;
            }

            try
            {
                config.showMain = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showMain", config.showMain);
                config.showInventory = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showInventory", config.showInventory);

                Log($"Config loaded - showMain: {config.showMain}, showInventory: {config.showInventory}");
            }
            catch (Exception ex)
            {
                Log($"Error loading config: {ex.Message}");
            }
        }

        private void OnModConfigOptionsChanged(string key)
        {
            if (!key.StartsWith(MOD_NAME + "_"))
            {
                return;
            }

            Log($"Config changed: {key}");
            LoadConfigFromModConfig();
        }
    }

}
