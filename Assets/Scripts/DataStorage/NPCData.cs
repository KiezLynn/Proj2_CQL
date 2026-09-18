using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "NPC")]
public class NPCData : ScriptableObject
{
    [Header("NPC 名称")]
    public string npcName;
    [Header("NPC 立绘")]
    public Sprite ingredientIcon;//图像
    [Header("情绪颜色（用于计算满意条")]
    public string color1, color2, color3;//情绪颜色（用于计算满意条）
    [Header("口味需求（0-10，用于计算满意条）")]
    public int flavorRequirement;//口味需求（0-10，用于计算满意条）
    public List<DialogueNode> dialogues;//对话列表
}
