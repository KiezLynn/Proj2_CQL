using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "NPC")]
public class NPCData : ScriptableObject
{
    [Header("NPC 名称")]
    public string npcName;
    
    [Header("NPC 立绘")]
    public Sprite ingredientIcon;
    
    [Header("情绪颜色（最多3种，对应CSV颜色1/2/3）")]
    public IngredientColor[] colors; 
    
    [Header("口味/基酒偏好")]
    public FlavorLevel flavorRequirement; 

    // 【核心新增】：必须加上这个字段，GameManager 才能读取到该客人点的是哪杯酒！
    [Header("顾客点单的目标酒水 (配方)")]
    public BeverageData favoriteBeverage;

    [Header("--- 对话区块 (对应CSV字段) ---")]
    [Header("【1. 开局点单阶段】")]
    public List<DialogueNode> openingDialogues;

    [Header("【2. 调酒结算反馈】")]
    public DialogueNode satisfiedDialogue;    // 满意反馈
    public DialogueNode dissatisfiedDialogue; // 不满意反馈

    [Header("【3. 营业流转】")]
    public DialogueNode refillDialogue;       // 续杯对话
    public DialogueNode checkoutDialogue;     // 结账对话
}