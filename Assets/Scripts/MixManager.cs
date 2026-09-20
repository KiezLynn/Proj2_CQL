using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MixManager : MonoBehaviour
{
    [Header("UI References - Book")]
    public Button[] slotButtons = new Button[4]; // 既是按钮也是图片载体
    public Sprite emptySlotSprite;             // 空槽位占位图
    public TextMeshProUGUI successRateText;            

    [Header("UI References - Popup")]
    public GameObject ingredientPopup; // 弹窗面板
    public Image popupIcon;            // 弹窗图标
    public TextMeshProUGUI popupNameText;         // 弹窗名字
    public TextMeshProUGUI popupPropertyText;     // 弹窗颜色/属性
    public Image popupColorImage;      // 颜色
    public Button popupChooseButton;   // 确认添加
    public Button popupCloseButton;    // 关闭弹窗

    [Header("UI References - Process")]
    public Button makeButton;   
    public GameObject jiuPanel; 
    public Image resultDrinkImage; // 用于在调酒完成后显示生成的饮品图片
    public Button remakeButton; 
    public Button serveButton;  

    private IngredientData pendingIngredient;

    private void Start()
    {
        if (makeButton != null) makeButton.onClick.AddListener(OnMake);
        if (serveButton != null) serveButton.onClick.AddListener(OnServe);
        if (remakeButton != null) remakeButton.onClick.AddListener(Initialize);

        if (popupChooseButton != null) popupChooseButton.onClick.AddListener(ConfirmChooseIngredient);
        if (popupCloseButton != null) popupCloseButton.onClick.AddListener(ClosePopup);

        // 为四个槽位绑定点击取消事件
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i; // 解决闭包问题
            if (slotButtons[index] != null)
            {
                slotButtons[index].onClick.AddListener(() => RemoveIngredient(index));
            }
        }
    }

    void FixedUpdate()
    {
        UpdateSuccessRate();
    }

    public void Initialize()
    {
        GameManager.Instance.selectedIngredients.Clear();
        GameManager.Instance.successRate = 0f;
        pendingIngredient = null;
        
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
            {
                slotButtons[i].image.sprite = emptySlotSprite;
                slotButtons[i].image.color = new Color(1, 1, 1, 0); // 隐藏空槽位的颜色或设为透明
                slotButtons[i].interactable = false; // 空槽位不可点击
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
        popupIcon.sprite = ing.ingredientIcon;
        popupNameText.text = ing.ingredientName;
        
        string colorStr = ing.color.ToString();
        popupPropertyText.text = $"[Color] {colorStr}\n[Attr] {ing.addPoint}";

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
        if (pendingIngredient != null && GameManager.Instance.selectedIngredients.Count < 4)
        {
            int currentCount = GameManager.Instance.selectedIngredients.Count;

            // 限制: 第一个只能是基酒，后三个只能是配料
            if (currentCount == 0 && pendingIngredient.category != IngredientCategory.Base)
            {
                Debug.Log("第一个材料必须是基酒！");
                return; // 可在此处添加UI提示弹窗反馈给玩家
            }
            if (currentCount > 0 && pendingIngredient.category == IngredientCategory.Base)
            {
                Debug.Log("只能添加一种基酒，请选择配料！");
                return; // 可在此处添加UI提示弹窗反馈给玩家
            }

            GameManager.Instance.selectedIngredients.Add(pendingIngredient);
            
            if (slotButtons[currentCount] != null)
            {
                slotButtons[currentCount].image.sprite = pendingIngredient.ingredientIcon;
                slotButtons[currentCount].image.color = new Color(1, 1, 1, 1);
                slotButtons[currentCount].interactable = true; // 允许被点击取消
            }
            
            UpdateSuccessRate();
        }
        ClosePopup();
    }

    // 点击槽位取消选择的功能
    public void RemoveIngredient(int index)
    {
        if (index < 0 || index >= GameManager.Instance.selectedIngredients.Count) return;

        // 从列表中移除
        GameManager.Instance.selectedIngredients.RemoveAt(index);

        // 重新刷新槽位UI（往前递补空位）
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

        UpdateSuccessRate();
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
        // 限制：必须选满 1种基酒 + 3种配料 才可以制作
        if (GameManager.Instance.selectedIngredients.Count < 4)
        {
            Debug.Log("必须选满 1 种基酒和 3 种配料才能开始制作！");
            return;
        }

        makeButton.interactable = false;
        jiuPanel.SetActive(true);            
        
        // 显示对应酒的图片
        if (resultDrinkImage != null)
        {
            // 如果成功率大于等于50%，显示目标酒的图片
            if (GameManager.Instance.successRate >= 0.5f && GameManager.Instance.currentBeverage != null)
            {
                resultDrinkImage.sprite = GameManager.Instance.currentBeverage.beverageIcon; 
            }
            else
            {
                // 如果失败（成功率低于50%），需要从某个地方获取表示“失败”的饮品数据
                // 假设 GameManager 中有一个引用指向一个代表失败的 BeverageData
                if (GameManager.Instance.failedBeverage != null) 
                {
                     resultDrinkImage.sprite = GameManager.Instance.failedBeverage.beverageIcon;
                }
                else
                {
                    Debug.LogWarning("未配置失败饮品的数据 (GameManager.Instance.failedBeverage)");
                    // 如果没有配置失败饮品数据，可以考虑在这里隐藏图片或者保持空白
                }
            }
            resultDrinkImage.SetNativeSize();
        }

        serveButton.gameObject.SetActive(true);  
    }

    public void OnServe()
    {
        GameManager.Instance.EvaluateDrinkResult();
    }
}