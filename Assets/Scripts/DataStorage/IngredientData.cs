using UnityEngine;

public enum IngredientCategory
{
    Base,   // 基酒-Base
    // Mix1,   // 配料1-Mix
    // Mix2,   // 配料2-Mix
    Mix3    // 配料
}

public enum IngredientColor
{
    None,   // 无色
    Red,
    Blue,
    Yellow,
    White,
    Black,
    Green
}

// 定义枚举：口味影响数值，直接赋予整数值以对应配料表
public enum FlavorLevel
{
    SodaWater = -3, // 口味-3 (苏打水)
    Sprite = -2, // 口味-2 (雪碧)
    Zero = 0,    // 无口味影响/默认
    BitterExtract = 1,   // 口味+1 (苦精)
    Cognac = 5,  // 口味5 (Cognac)
    Gin = 6,  // 口味6 (Gin类)
    Rum = 7,  // 口味7 (Rum类)
    Whisky = 8,  // 口味8 (Whisky)
    Tequila = 9,  // 口味9 (Tequila类)
    Vodka = 10 // 口味10 (Vodka类)
}

[CreateAssetMenu(fileName = "Ingredient", menuName = "原料")]
public class IngredientData : ScriptableObject
{
    [Header("名称")]
    public string ingredientName;
    
    [Header("图像")]
    public Sprite ingredientIcon;
    
    [Header("类别（基酒/配料）")]
    public IngredientCategory category;
    
    [Header("颜色属性（用于UI显示）")]
    public IngredientColor color;
    
    [Header("解锁花费 (0代表初始拥有)")]
    public int cost;
    
    [Header("口味属性（对应数值）")]
    public FlavorLevel addPoint; // 改为枚举类型，Inspector中将变为下拉选项
}