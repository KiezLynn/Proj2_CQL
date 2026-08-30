using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Ingredient", menuName = "原料")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;//名称
    public Sprite ingredientIcon;//图像
    public string category;//类别（基酒/配料）
    public string color;//颜色属性（用于UI显示）
    public int addPoint;//加点值
    public int flavorIntensity;//口味强度（0-10）
    [TextArea] public string description;//说明
}