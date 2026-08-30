using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DialogueNode
{
    public string speakerName;
    [TextArea] public string line;//对话内容
    public bool hasOptions;//是否有选项
    public List<string> optionTexts;//选项文字（最多2个）
    // 选项指向的下一个节点索引，若为-1则结束对话进入调酒
    public int nextNodeIndex0;
    public int nextNodeIndex1;
}

