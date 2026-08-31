using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MixManager : MonoBehaviour
{
    [Header("UI References - Shelves")]
    public GameObject ingredientsBox01; // 基酒货架
    public GameObject ingredientsBox02; // 配料货架
    public Button switchBoxButton;      // 右上角切换箭头按钮

    [Header("UI References - Book")]
    public Text[] slotTexts = new Text[4]; // 左下角书本上的4个文本框
    public Text successRateText;           // 书本上的成功率文本

    [Header("UI References - Process")]
    public Button makeButton;   // 右下角的摇酒壶 (MakeBT)
    public GameObject jiuPanel; // 调酒完成后的结果展示面板
    public Button remakeButton; // JiuPanel中的重做按钮
    public Button serveButton;  // NextBT 上酒按钮

    private bool isShowingBox01 = true;

    private void Start()
    {
        // 绑定按钮事件
        if (switchBoxButton != null) switchBoxButton.onClick.AddListener(ToggleIngredientBox);
        if (makeButton != null) makeButton.onClick.AddListener(OnMake);
        if (serveButton != null) serveButton.onClick.AddListener(OnServe);
        if (remakeButton != null) remakeButton.onClick.AddListener(Initialize);
    }

    public void Initialize()
    {
        // 1. 清空后台记录的原料列表
        GameManager.Instance.selectedIngredients.Clear();
        GameManager.Instance.successRate = 0f;
        
        // 2. 重置书本UI显示
        foreach (var txt in slotTexts)
        {
            txt.text = "等待添加..."; // 或置空 ""
        }
        successRateText.text = "0%";
        
        // 3. 恢复货架与流程按钮状态
        isShowingBox01 = true;
        ingredientsBox01.SetActive(true);
        ingredientsBox02.SetActive(false);
        
        makeButton.interactable = true;
        serveButton.gameObject.SetActive(false);
        jiuPanel.SetActive(false);
    }

    // 切换基酒与配料货架
    void ToggleIngredientBox()
    {
        isShowingBox01 = !isShowingBox01;
        ingredientsBox01.SetActive(isShowingBox01);
        ingredientsBox02.SetActive(!isShowingBox01);
    }

    // 玩家点击货架上的酒瓶时调用
    public void SelectIngredient(IngredientData ing)
    {
        int currentCount = GameManager.Instance.selectedIngredients.Count;
        if (currentCount < 4)
        {
            // 加入列表
            GameManager.Instance.selectedIngredients.Add(ing);
            
            // 更新左下角书本对应的文本框，显示被点击的原料名称
            slotTexts[currentCount].text = ing.ingredientName;
            
            UpdateSuccessRate();
        }
        else
        {
            Debug.Log("杯子满了，最多只能加4种原料！");
        }
    }

    // 计算成功率
    void UpdateSuccessRate()
    {
        int total = 0;
        foreach (var ing in GameManager.Instance.selectedIngredients)
        {
            total += ing.addPoint;
        }

        // 获取顾客需求（保底为1，防止新手引导NPC需求为0时出现除以0的报错）
        int req = GameManager.Instance.currentNPC.flavorRequirement;
        if (req <= 0) req = 1; 

        // 计算比例并限制在 0~1 之间
        float rate = total / (float)req;
        rate = Mathf.Clamp01(rate);
        
        Debug.Log($"req: {req} , rate: {rate}");
        
        // 刷新 UI
        successRateText.text = $"{rate * 100:F0}%";
        GameManager.Instance.successRate = rate;
    }

    // 玩家点击右下角的摇酒壶【MakeBT】
    public void OnMake()
    {
        if (GameManager.Instance.selectedIngredients.Count == 0)
        {
            Debug.Log("还没添加任何原料！");
            return;
        }
        
        makeButton.interactable = false;
        jiuPanel.SetActive(true);            // 弹出中间的完成酒杯
        serveButton.gameObject.SetActive(true);  // 显示 NextBT
    }

    // 玩家点击【NextBT】
    public void OnServe()
    {
        GameManager.Instance.EnterResult();
    }
}