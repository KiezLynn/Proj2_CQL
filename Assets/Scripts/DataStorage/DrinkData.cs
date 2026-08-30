using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "Beverage", menuName = "酒水")]
public class BeverageData : ScriptableObject
{
    public string beverageName;//名称
    public Sprite ingredientIcon;//图像
    public int price;//标价
    [TextArea] public string description;//说明
    //public List<IngredientData> ingredients;//原料单

}
