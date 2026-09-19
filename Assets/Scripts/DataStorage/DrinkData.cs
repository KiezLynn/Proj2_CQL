using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Beverage", menuName = "酒水")]
public class BeverageData : ScriptableObject
{
    [Header("名称")]
    public string beverageName;
    
    [Header("图像")]
    public Sprite beverageIcon;
    
    [Header("标价")]
    public int price;
    
    [Header("颜色标签 (仅做描述说明)")]
    public IngredientColor[] colorTags;
    
    [Header("说明")]
    [TextArea] public string description;

    [Header("--- 配方要求 ---")]
    [Header("要求的基酒类型 (如: Gin, Vodka)")]
    public FlavorLevel requiredBaseLiquor;
    
    [Header("要求的三种配料")]
    public List<IngredientData> requiredAdditives;
}