using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "Beverage", menuName = "酒水")]
public class BeverageData : ScriptableObject
{
    [Header("名称")]
    public string beverageName;//名称
    [Header("图像")]
    public Sprite ingredientIcon;//图像
    [Header("标价")]
    public int price;//标价
    [Header("说明")]
    [TextArea] public string description;//说明
    //public List<IngredientData> ingredients;//原料单

}
