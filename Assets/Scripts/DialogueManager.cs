using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text npcNameText;
    public Text dialogueText;
    public Button optionButton1, optionButton2;
    public CanvasGroup npcCanvasGroup;
    // 用于绑定并修改NPC的立绘组件
    public Image npcImage;

    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine; // 用于管理自动播放协程
    private NPCData currentNPC;
    private DialogueNode currentNode;       // 记录当前对话节点
    private int currentIndex;

    private bool isTyping = false;          // 标记当前是否正在打字
    private string currentFullText = "";    // 记录当前句子的完整文本

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
            // 可选：如果每个NPC画幅大小不同，可以取消下面这行的注释来让图片恢复原始比例
            npcImage.SetNativeSize(); 
        }
        npcCanvasGroup.alpha = 0;

        StartCoroutine(FadeInNPC(1f));
        ShowDialogueNode(currentNPC.dialogues[0]);
    }

    void ShowDialogueNode(DialogueNode node)
    {
        currentNode = node;
        currentFullText = node.line;
        npcNameText.text = node.speakerName;

        // 隐藏选项，等打字结束后再显示
        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);

        // 停止上一次可能遗留的协程，防止文字跳动或跳转冲突
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentFullText));
    }

    // 提供给外部（如对话框背景Button）点击的公共方法
    public void OnDialogueClicked()
    {
        if (isTyping)
        {
            // 如果正在打字，打断打字协程，直接显示全部
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            CompleteTyping();
        }
        else if (currentNode != null && !currentNode.hasOptions)
        {
            // 如果打字已经完成且当前无选项，再次点击直接跳过等待，进入下一句
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            AdvanceToNextNode();
        }
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        CompleteTyping();
    }

    // 打字完成后的统一处理逻辑
    void CompleteTyping()
    {
        isTyping = false;
        dialogueText.text = currentFullText; // 确保文本完整显示无遗漏

        if (currentNode.hasOptions)
        {
            optionButton1.gameObject.SetActive(true);
            optionButton2.gameObject.SetActive(true);
            optionButton1.GetComponentInChildren<Text>().text = currentNode.optionTexts[0];
            optionButton2.GetComponentInChildren<Text>().text = currentNode.optionTexts[1];
        }
        else
        {
            // 文本完全显示完毕后，开始计算等待时间（例如3秒）
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance());
        }
    }

    IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(3f); // 全文显示后的停留等待时间
        AdvanceToNextNode();
    }

    // 进入下一节点的统一逻辑
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

    IEnumerator FadeInNPC(float duration)
    {
        CanvasGroup cg = npcCanvasGroup;
        if (cg == null) yield break;

        float timer = 0f;
        float startAlpha = 0f;
        float endAlpha = 1f;

        cg.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
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