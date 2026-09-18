using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 必须引入 TextMeshPro 命名空间

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    // 将原先的 Text 替换为 TextMeshProUGUI
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public Button optionButton1, optionButton2;
    public CanvasGroup npcCanvasGroup;
    public Image npcImage;

    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine;
    private NPCData currentNPC;
    private DialogueNode currentNode;
    private int currentIndex;

    private bool isTyping = false;

    private void Start()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
    }

    public void StartDialogue(NPCData npc)
    {
        currentNPC = npc;
        currentIndex = 0;
        if (currentNPC.dialogues == null || currentNPC.dialogues.Count == 0)
        {
            Debug.LogWarning(currentNPC.npcName + " 没有配置任何对话！直接进入调酒。");
            GameManager.Instance.EnterMixing();
            return;
        }
        if (npcImage != null && currentNPC.ingredientIcon != null)
        {
            npcImage.sprite = currentNPC.ingredientIcon;
        }
        npcCanvasGroup.alpha = 0;

        StartCoroutine(FadeInNPC(1f));
        ShowDialogueNode(currentNPC.dialogues[0]);
    }

    void ShowDialogueNode(DialogueNode node)
    {
        currentNode = node;
        npcNameText.text = node.speakerName;

        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);

        typingCoroutine = StartCoroutine(TypeText(node.line));
    }

    public void OnDialogueClicked()
    {
        if (isTyping)
        {
            // 打断打字，直接显示全部
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            CompleteTyping();
        }
        else if (currentNode != null && !currentNode.hasOptions)
        {
            // 已经是全显示状态，直接进入下一句
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            AdvanceToNextNode();
        }
    }

    // --- 核心修改：使用 TextMeshPro 的 maxVisibleCharacters 机制 ---
    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        
        // 1. 先把完整文本赋给组件
        dialogueText.text = fullText;
        
        // 2. 将可见字符设为0，相当于隐藏全部文本
        dialogueText.maxVisibleCharacters = 0;
        
        // 3. 强制更新网格，这样TMP会自动解析富文本，并计算出真正的可见字符总数
        dialogueText.ForceMeshUpdate();

        // 4. 获取纯文本的可见字符总数（不包含富文本标签）
        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        // 5. 逐字增加可见字符数
        while (visibleCount < totalVisibleCharacters)
        {
            visibleCount++;
            dialogueText.maxVisibleCharacters = visibleCount;
            yield return new WaitForSeconds(0.05f); // 每个字的间隔时间
        }

        CompleteTyping();
    }

    void CompleteTyping()
    {
        isTyping = false;
        
        // 确保打断时也能瞬间显示出全部文本
        dialogueText.maxVisibleCharacters = 99999; 

        if (currentNode.hasOptions)
        {
            optionButton1.gameObject.SetActive(true);
            optionButton2.gameObject.SetActive(true);
            
            // 注意：如果你的选项按钮子物体也换成了 TextMeshPro，这里需要改成 GetComponentInChildren<TextMeshProUGUI>()
            // 如果选项还是普通 Text，则保持原样不变
            optionButton1.GetComponentInChildren<Text>().text = currentNode.optionTexts[0];
            optionButton2.GetComponentInChildren<Text>().text = currentNode.optionTexts[1];
        }
        else
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance());
        }
    }

    IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(3f);
        AdvanceToNextNode();
    }

    void AdvanceToNextNode()
    {
        int next = currentNode.nextNodeIndex0;
        if (next == -1 || next >= currentNPC.dialogues.Count)
        {
            GameManager.Instance.OnDialogueComplete();
        }
        else
        {
            currentIndex = next;
            ShowDialogueNode(currentNPC.dialogues[currentIndex]);
        }
    }
    
    /// <summary>
    /// 跳转到指定段落
    /// </summary>
    /// <param name="targetIndex">段落下标</param>
    public void JumpToNode(int targetIndex)
    {
        if (currentNPC == null || currentNPC.dialogues == null) return;

        if (targetIndex >= 0 && targetIndex < currentNPC.dialogues.Count)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);

            currentIndex = targetIndex;
            ShowDialogueNode(currentNPC.dialogues[currentIndex]);
        }
        else
        {
            Debug.LogError($"跳转失败：目标段落索引越界。");
        }
    }

    IEnumerator FadeInNPC(float duration)
    {
        CanvasGroup cg = npcCanvasGroup;
        if (cg == null) yield break;

        float timer = 0f;
        cg.alpha = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    public void OnOptionSelected(int option)
    {
        int next = option == 0 ? currentNode.nextNodeIndex0 : currentNode.nextNodeIndex1;
        if (next == -1 || next >= currentNPC.dialogues.Count)
        {
            GameManager.Instance.OnDialogueComplete();
        }
        else
        {
            currentIndex = next;
            ShowDialogueNode(currentNPC.dialogues[currentIndex]);
        }
    }
}