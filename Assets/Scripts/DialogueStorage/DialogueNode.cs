using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    public string speakerName;
    [TextArea] public string line; // 对话内容
    
    public List<string> optionTexts; // 选项文字（最多2个）
    
    // 选项指向的下一个节点索引，若为-1则结束当前对话流
    public int nextNodeIndex0 = -1;
    public int nextNodeIndex1 = -1;

    // 【优化】通过代码自动判断是否有选项，不需要策划再去手动打勾，减少配表失误
    public bool HasOptions => optionTexts != null && optionTexts.Count > 0;
}