using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MixManager : MonoBehaviour
{
    [Header("UI References - Book")]
    public Button[] slotButtons = new Button[4]; 
    public Sprite emptySlotSprite;             
    public TextMeshProUGUI successRateText;            

    [Header("UI References - Popup")]
    public GameObject ingredientPopup; 
    public Image popupIcon;            
    public TextMeshProUGUI popupNameText;         
    public TextMeshProUGUI popupPropertyText;     
    public Image popupColorImage;      
    public Button popupChooseButton;   
    public Button popupCloseButton;    

    [Header("UI References - Process")]
    public Button makeButton;   
    public GameObject jiuPanel; 
    public Image resultDrinkImage; 
    public Button remakeButton; 
    public Button serveButton;  

    private IngredientData pendingIngredient;
    private bool isPendingUnlock = false; // 【新增】标记当前弹窗是否为解锁状态

    private void Start()
    {
        if (makeButton != null) makeButton.onClick.AddListener(OnMake);
        if (serveButton != null) serveButton.onClick.AddListener(OnServe);
        if (remakeButton != null) remakeButton.onClick.AddListener(Initialize);

        if (popupChooseButton != null) popupChooseButton.onClick.AddListener(ConfirmChooseIngredient);
        if (popupCloseButton != null) popupCloseButton.onClick.AddListener(ClosePopup);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i; 
            if (slotButtons[index] != null)
            {
                slotButtons[index].onClick.AddListener(() => RemoveIngredient(index));
            }
        }
    }

    public void Initialize()
    {
        GameManager.Instance.selectedIngredients.Clear();
        GameManager.Instance.successRate = 0f;
        GameManager.Instance.madeBeverage = null;
        pendingIngredient = null;
        
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
            {
                slotButtons[i].image.sprite = emptySlotSprite;
                slotButtons[i].image.color = new Color(1, 1, 1, 0); 
                slotButtons[i].interactable = false; 
            }
        }
        successRateText.text = "0%";
        
        ingredientPopup.SetActive(false);
        
        makeButton.interactable = true;
        serveButton.gameObject.SetActive(false);
        jiuPanel.SetActive(false);
    }

    public void SelectIngredient(IngredientData ing)
    {
        if (GameManager.Instance.selectedIngredients.Count >= 4) return;

        pendingIngredient = ing;
        // 【新增】判断是否需要解锁
        isPendingUnlock = !GameManager.Instance.IsIngredientUnlocked(ing);

        popupIcon.sprite = ing.ingredientIcon;
        popupNameText.text = ing.ingredientName;
        
        string catStr = ing.category == IngredientCategory.Base ? "Base" : "Mix";
        string colorStr = ing.color.ToString();

        // 尝试获取按钮文字组件并修改文字
        TextMeshProUGUI btnText = popupChooseButton.GetComponentInChildren<TextMeshProUGUI>();

        if (isPendingUnlock)
        {
            popupPropertyText.text = $"<color=#FF0000>[Unlocked] cost: ${ing.cost}</color>\n[{catStr}] [Color] {colorStr}\n[Attr] {ing.addPoint}";
            if (btnText != null) btnText.text = "Unlock";
        }
        else
        {
            popupPropertyText.text = $"[{catStr}] [Color] {colorStr}\n[Attr] {ing.addPoint}";
            if (btnText != null) btnText.text = "Choose";
        }

        switch (ing.color)
        {
            case IngredientColor.None: popupColorImage.color = new Color(1, 1, 1, 0.1f); break;
            case IngredientColor.Red: popupColorImage.color = Color.red; break;
            case IngredientColor.Blue: popupColorImage.color = Color.blue; break;
            case IngredientColor.Yellow: popupColorImage.color = Color.yellow; break;
            case IngredientColor.White: popupColorImage.color = Color.white; break;
            case IngredientColor.Black: popupColorImage.color = Color.black; break;
            case IngredientColor.Green: popupColorImage.color = Color.green; break;
        }

        ingredientPopup.SetActive(true);
    }

    void ClosePopup()
    {
        ingredientPopup.SetActive(false);
        pendingIngredient = null;
    }

    void ConfirmChooseIngredient()
    {
        if (pendingIngredient == null) return;

        // 【新增】解锁流程
        if (isPendingUnlock)
        {
            if (GameManager.Instance.UnlockIngredient(pendingIngredient))
            {
                Debug.Log($"解锁成功！花费 {pendingIngredient.cost} 元");
                isPendingUnlock = false; 
                SelectIngredient(pendingIngredient); // 刷新弹窗为"添加"状态
            }
            else
            {
                Debug.LogWarning("余额不足，无法解锁！");
            }
            return; // 等待玩家再次点击"添加"
        }

        // 添加材料流程
        if (GameManager.Instance.selectedIngredients.Count < 4)
        {
            int currentCount = GameManager.Instance.selectedIngredients.Count;

            if (currentCount == 0 && pendingIngredient.category != IngredientCategory.Base)
            {
                Debug.Log("第一个材料必须是基酒！");
                return; 
            }
            if (currentCount > 0 && pendingIngredient.category == IngredientCategory.Base)
            {
                Debug.Log("只能添加一种基酒，请选择配料！");
                return; 
            }

            GameManager.Instance.selectedIngredients.Add(pendingIngredient);
            
            if (slotButtons[currentCount] != null)
            {
                slotButtons[currentCount].image.sprite = pendingIngredient.ingredientIcon;
                slotButtons[currentCount].image.color = new Color(1, 1, 1, 1);
                slotButtons[currentCount].interactable = true; 
            }
            
            UpdateSuccessRate(); // 【修改】仅在添加成功时运算一次
        }
        ClosePopup();
    }

    public void RemoveIngredient(int index)
    {
        if (index < 0 || index >= GameManager.Instance.selectedIngredients.Count) return;

        GameManager.Instance.selectedIngredients.RemoveAt(index);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
            {
                if (i < GameManager.Instance.selectedIngredients.Count)
                {
                    slotButtons[i].image.sprite = GameManager.Instance.selectedIngredients[i].ingredientIcon;
                    slotButtons[i].image.color = new Color(1, 1, 1, 1);
                    slotButtons[i].interactable = true;
                }
                else
                {
                    slotButtons[i].image.sprite = emptySlotSprite;
                    slotButtons[i].image.color = new Color(1, 1, 1, 0);
                    slotButtons[i].interactable = false;
                }
            }
        }
        UpdateSuccessRate(); // 【修改】仅在移除成功时运算一次
    }

    // 【核心修改】严格比对配方以决定最终产出饮品
    void UpdateSuccessRate()
    {
        // 还没选满4个，肯定做不出酒，显示 0%
        if (GameManager.Instance.selectedIngredients.Count < 4)
        {
            GameManager.Instance.successRate = 0f;
            GameManager.Instance.madeBeverage = null;
            successRateText.text = "0%";
            return;
        }

        BeverageData matchedBeverage = null;

        // 遍历所有正常配方库，比对是否严格相符
        foreach (var bev in GameManager.Instance.allBeverages)
        {
            bool baseMatch = false;
            List<IngredientData> selectedAdds = new List<IngredientData>();

            // 分离基酒与配料
            foreach (var ing in GameManager.Instance.selectedIngredients)
            {
                if (ing.category == IngredientCategory.Base)
                {
                    if (ing.addPoint == bev.requiredBaseLiquor) baseMatch = true;
                }
                else
                {
                    selectedAdds.Add(ing);
                }
            }

            // 如果基酒对上了，且配料数量也一致(3个)
            if (baseMatch && selectedAdds.Count == bev.requiredAdditives.Count)
            {
                // 无序对比配料
                List<IngredientData> tempReq = new List<IngredientData>(bev.requiredAdditives);
                bool addMatch = true;
                foreach (var a in selectedAdds)
                {
                    if (tempReq.Contains(a)) tempReq.Remove(a);
                    else { addMatch = false; break; }
                }

                if (addMatch && tempReq.Count == 0)
                {
                    matchedBeverage = bev; 
                    break; // 找到了匹配的配方
                }
            }
        }

        // 评判成功率和生成的酒
        if (matchedBeverage != null)
        {
            GameManager.Instance.madeBeverage = matchedBeverage;

            if (matchedBeverage == GameManager.Instance.currentBeverage)
                GameManager.Instance.successRate = 0.99f; // NPC的目标饮品
            else
                GameManager.Instance.successRate = 0.50f; // 其他合格的饮品
        }
        else
        {
            // 配方不对 = 暗黑料理
            GameManager.Instance.madeBeverage = GameManager.Instance.failedBeverage;
            GameManager.Instance.successRate = 0f; 
        }

        successRateText.text = $"{GameManager.Instance.successRate * 100:F0}%";
    }

    public void OnMake()
    {
        if (GameManager.Instance.selectedIngredients.Count < 4)
        {
            Debug.Log("必须选满 1 种基酒和 3 种配料才能开始制作！");
            return;
        }

        makeButton.interactable = false;
        jiuPanel.SetActive(true);            
        
        // 【修改】展示我们算出的 madeBeverage
        if (resultDrinkImage != null && GameManager.Instance.madeBeverage != null)
        {
            resultDrinkImage.sprite = GameManager.Instance.madeBeverage.beverageIcon; 
            resultDrinkImage.SetNativeSize();
        }

        serveButton.gameObject.SetActive(true);  
    }

    public void OnServe()
    {
        GameManager.Instance.EvaluateDrinkResult();
    }
}