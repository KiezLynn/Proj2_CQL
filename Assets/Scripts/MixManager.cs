using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MixManager : MonoBehaviour
{
    public GameObject ingredientButtonPrefab;
    public Transform ingredientGrid;
    public Text successRateText;
    public Button makeButton, serveButton;

    private List<IngredientData> availableIngredients = new List<IngredientData>(); // 从Resources或预设加载

    void Start()
    {
        // 动态生成8个原料按钮
        foreach (var ing in availableIngredients)
        {
            var btn = Instantiate(ingredientButtonPrefab, ingredientGrid).GetComponent<Button>();
            // 设置显示和点击事件
        }
        serveButton.gameObject.SetActive(false);
    }

    public void SelectIngredient(IngredientData ing)
    {
        if (GameManager.Instance.selectedIngredients.Count < 4)
        {
            GameManager.Instance.selectedIngredients.Add(ing);
            UpdateSuccessRate();
        }
    }

    void UpdateSuccessRate()
    {
        int total = 0;
        foreach (var ing in GameManager.Instance.selectedIngredients)
            total += ing.addPoint;
        float rate = total / (float)GameManager.Instance.currentNPC.flavorRequirement;
        rate = Mathf.Clamp01(rate);
        successRateText.text = $"成功率：{rate * 100:F0}%";
        GameManager.Instance.successRate = rate;
    }

    public void Initialize()
    {
        //清除选择
    }

    public void OnMake()
    {
        // 锁定选择，显示制作动画（可选）
        makeButton.interactable = false;
        serveButton.gameObject.SetActive(true);
    }

    public void OnServe()
    {
        GameManager.Instance.EnterResult();
    }
}
