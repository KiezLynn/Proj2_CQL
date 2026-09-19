using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public Button optionButton1, optionButton2;
    public CanvasGroup npcCanvasGroup;
    public Image npcImage;

    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine;
    
    private NPCData currentNPC;
    private DialogueNode currentNode;
    private List<DialogueNode> currentDialogueList; // 当前正在播放的对话列表
    private int currentIndex;
    private bool isTyping = false;

    private void Start()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
    }

    // 播放：开局阶段的长对话序列
    public void StartOpeningDialogue(NPCData npc)
    {
        currentNPC = npc;
        currentDialogueList = npc.openingDialogues;
        currentIndex = 0;

        if (currentDialogueList == null || currentDialogueList.Count == 0)
        {
            Debug.LogWarning(currentNPC.npcName + " 没有配置开局对话！");
            GameManager.Instance.EnterMixing();
            return;
        }

        UpdateNPCImage();
        npcCanvasGroup.alpha = 0;

        StartCoroutine(FadeInNPC(1f));
        ShowDialogueNode(currentDialogueList[0]);
    }

    // 播放：结算或状态流转阶段的“单句对话”（如满意、不满意、续杯）
    public void PlaySingleFeedback(NPCData npc, DialogueNode node)
    {
        // 【新增安全校验】：如果传入的节点为空（策划没配），或者这句台词没写内容，直接跳过并触发结束回调
        if (node == null || string.IsNullOrEmpty(node.line))
        {
            Debug.Log($"[{npc.npcName}] 没有配置对应的反馈对话，直接跳过该对话环节。");
            GameManager.Instance.OnDialogueComplete();
            return;
        }

        currentNPC = npc;
        currentDialogueList = null; // 独立节点，不依赖上下文列表
        
        UpdateNPCImage();
        npcCanvasGroup.alpha = 1; // 确保立绘已经显示

        ShowDialogueNode(node);
    }

    private void UpdateNPCImage()
    {
        if (npcImage != null && currentNPC != null && currentNPC.ingredientIcon != null)
        {
            npcImage.sprite = currentNPC.ingredientIcon;
        }
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
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            CompleteTyping();
        }
        else if (currentNode != null && !currentNode.HasOptions) // 使用优化后的 HasOptions
        {
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            AdvanceToNextNode();
        }
    }

    // TMP的富文本打字机效果[cite: 1]
    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = fullText;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        while (visibleCount < totalVisibleCharacters)
        {
            visibleCount++;
            dialogueText.maxVisibleCharacters = visibleCount;
            yield return new WaitForSeconds(0.05f);
        }

        CompleteTyping();
    }

    void CompleteTyping()
    {
        isTyping = false;
        dialogueText.maxVisibleCharacters = 99999; 

        if (currentNode.HasOptions) // 使用优化后的 HasOptions
        {
            optionButton1.gameObject.SetActive(true);
            optionButton2.gameObject.SetActive(true);
            
            optionButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentNode.optionTexts[0];
            optionButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentNode.optionTexts[1];
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
        
        // 如果当前是单句反馈（currentDialogueList为null）或者遇到 -1 节点，统一交由 GameManager 处理
        if (currentDialogueList == null || next == -1 || next >= currentDialogueList.Count)
        {
            GameManager.Instance.OnDialogueComplete();
        }
        else
        {
            currentIndex = next;
            ShowDialogueNode(currentDialogueList[currentIndex]);
        }
    }

    public void OnOptionSelected(int option)
    {
        int next = option == 0 ? currentNode.nextNodeIndex0 : currentNode.nextNodeIndex1;
        
        if (currentDialogueList == null || next == -1 || next >= currentDialogueList.Count)
        {
            GameManager.Instance.OnDialogueComplete();
        }
        else
        {
            currentIndex = next;
            ShowDialogueNode(currentDialogueList[currentIndex]);
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
}