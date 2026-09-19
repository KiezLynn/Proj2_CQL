using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MixManager : MonoBehaviour
{
    [Header("UI References - Shelves")]
    public GameObject ingredientsBox01; // 基酒货架
    public GameObject ingredientsBox02; // 配料货架
    public Button switchBoxButton;      // 右上角切换箭头按钮

    [Header("UI References - Book")]
    public Image[] slotIcons = new Image[4]; // 替换为Image
    public Sprite emptySlotSprite;           // 空槽位占位图
    public TextMeshProUGUI successRateText;           

    [Header("UI References - Popup")]
    public GameObject ingredientPopup; // 弹窗面板
    public Image popupIcon;            // 弹窗图标
    public TextMeshProUGUI popupNameText;         // 弹窗名字
    public TextMeshProUGUI popupPropertyText;     // 弹窗颜色/属性
    public Image popupColorImage;     // 颜色
    public Button popupChooseButton;   // 确认添加
    public Button popupCloseButton;    // 关闭弹窗

    [Header("UI References - Process")]
    public Button makeButton;   
    public GameObject jiuPanel; 
    public Button remakeButton; 
    public Button serveButton;  

    private bool isShowingBox01 = true;
    private IngredientData pendingIngredient;

    private void Start()
    {
        if (switchBoxButton != null) switchBoxButton.onClick.AddListener(ToggleIngredientBox);
        if (makeButton != null) makeButton.onClick.AddListener(OnMake);
        if (serveButton != null) serveButton.onClick.AddListener(OnServe);
        if (remakeButton != null) remakeButton.onClick.AddListener(Initialize);

        if (popupChooseButton != null) popupChooseButton.onClick.AddListener(ConfirmChooseIngredient);
        if (popupCloseButton != null) popupCloseButton.onClick.AddListener(ClosePopup);
    }

    public void Initialize()
    {
        GameManager.Instance.selectedIngredients.Clear();
        GameManager.Instance.successRate = 0f;
        pendingIngredient = null;
        
        foreach (var img in slotIcons)
        {
            img.sprite = emptySlotSprite;
            img.color = new Color(1, 1, 1, 0);
        }
        successRateText.text = "0%";
        
        isShowingBox01 = true;
        ingredientsBox01.SetActive(true);
        ingredientsBox02.SetActive(false);
        ingredientPopup.SetActive(false);
        
        makeButton.interactable = true;
        serveButton.gameObject.SetActive(false);
        jiuPanel.SetActive(false);
    }

    void ToggleIngredientBox()
    {
        isShowingBox01 = !isShowingBox01;
        ingredientsBox01.SetActive(isShowingBox01);
        ingredientsBox02.SetActive(!isShowingBox01);
    }

    public void SelectIngredient(IngredientData ing)
    {
        if (GameManager.Instance.selectedIngredients.Count >= 4) return;

        pendingIngredient = ing;
        popupIcon.sprite = ing.ingredientIcon;
        popupNameText.text = ing.ingredientName;
        
        // 提取枚举信息并展示在弹窗中
        string catStr = ing.category == IngredientCategory.Base ? "基酒" : "配料";
        string colorStr = ing.color.ToString();
        popupPropertyText.text = $"[Color] {colorStr}\n[Attr] {ing.addPoint}";

        switch (ing.color)
        {
            case IngredientColor.None:
                popupColorImage.color = new Color(1, 1, 1, 0.1f);
                break;
            case IngredientColor.Red:
                popupColorImage.color = Color.red;
                break;
            case IngredientColor.Blue:
                popupColorImage.color = Color.blue;
                break;
            case IngredientColor.Yellow:
                popupColorImage.color = Color.yellow;
                break;
            case IngredientColor.White:
                popupColorImage.color = Color.white;
                break;
            case IngredientColor.Black:
                popupColorImage.color = Color.black;
                break;
            case IngredientColor.Green:
                popupColorImage.color = Color.green;
                break;
            default:
                break;
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
        if (pendingIngredient != null && GameManager.Instance.selectedIngredients.Count < 4)
        {
            int currentCount = GameManager.Instance.selectedIngredients.Count;
            GameManager.Instance.selectedIngredients.Add(pendingIngredient);
            
            slotIcons[currentCount].sprite = pendingIngredient.ingredientIcon;
            slotIcons[currentCount].color = new Color(1, 1, 1, 1);
            
            UpdateSuccessRate();
        }
        ClosePopup();
    }

    void UpdateSuccessRate()
    {
        BeverageData targetBeverage = GameManager.Instance.currentBeverage;
        if (targetBeverage == null) return;

        bool hasCorrectBaseLiquor = false;
        int correctAdditivesCount = 0;
        List<IngredientData> checkList = new List<IngredientData>(targetBeverage.requiredAdditives);

        foreach (var ing in GameManager.Instance.selectedIngredients)
        {
            // 通过 category 判断是否是基酒，通过 addPoint 匹配酒的类型
            if (ing.category == IngredientCategory.Base) 
            {
                if (ing.addPoint == targetBeverage.requiredBaseLiquor)
                    hasCorrectBaseLiquor = true;
            }
            else
            {
                if (checkList.Contains(ing))
                {
                    correctAdditivesCount++;
                    checkList.Remove(ing); 
                }
            }
        }

        float rate = 0f;
        if (hasCorrectBaseLiquor && correctAdditivesCount == 3) rate = 0.99f;
        else if (hasCorrectBaseLiquor || correctAdditivesCount == 3) rate = 0.50f;
        else rate = 0f;

        GameManager.Instance.successRate = rate;
        successRateText.text = $"{rate * 100:F0}%";
    }

    public void OnMake()
    {
        if (GameManager.Instance.selectedIngredients.Count == 0) return;
        makeButton.interactable = false;
        jiuPanel.SetActive(true);            
        serveButton.gameObject.SetActive(true);  
    }

    public void OnServe()
    {
        GameManager.Instance.EvaluateDrinkResult();
    }
}