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

    private Coroutine typingCoroutine;
    private NPCData currentNPC;
    private int currentIndex;

    private void Start()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
    }

    public void StartDialogue(NPCData npc)
    {
        currentNPC = npc;
        currentIndex = 0;
        npcCanvasGroup.alpha = 0;

        StartCoroutine(FadeInNPC(1f));
        ShowDialogueNode(currentNPC.dialogues[0]);
    }

    void ShowDialogueNode(DialogueNode node)
    {
        npcNameText.text = node.speakerName;
        typingCoroutine = StartCoroutine(TypeText(node.line));
        optionButton1.gameObject.SetActive(node.hasOptions);
        optionButton2.gameObject.SetActive(node.hasOptions);
        if (node.hasOptions)
        {
            optionButton1.GetComponentInChildren<Text>().text = node.optionTexts[0];
            optionButton2.GetComponentInChildren<Text>().text = node.optionTexts[1];
        }
        else
        {
            StartCoroutine(AutoAdvance(node));
        }
    }

    //UI效果
    IEnumerator FadeInNPC(float duration)
    {
        CanvasGroup cg = npcCanvasGroup;
        if (cg == null) yield break;

        float timer = 0f;
        float startAlpha = 0f;
        float endAlpha = 1f;

        // 确保从透明开始
        cg.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 根据时间进度插值透明度
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null; // 等待下一帧
        }

        // 确保最终完全显现
        cg.alpha = endAlpha;

        yield return new WaitForSeconds(2f);
        //ShowDialogueNode(currentNPC.dialogues[0]);

    }

    IEnumerator TypeText(string fullText)
    {
        // 先清空
        dialogueText.text = "";

        // 逐字添加
        foreach (char c in fullText)
        {
            dialogueText.text += c;

            // 每个字间隔 0.05 秒，可以自己调速度
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator AutoAdvance(DialogueNode node)
    {
        yield return new WaitForSeconds(8f); //显示时间
        int next = node.nextNodeIndex0;
        if (next == -1 || next >= currentNPC.dialogues.Count)
        {
            //决定是否进入调酒界面，区分店长和顾客
            GameManager.Instance.OnDialogueComplete();
        }
        else
        {
            currentIndex = next;
            ShowDialogueNode(currentNPC.dialogues[currentIndex]);
        }
    }

    // 选项按钮的点击回调
    public void OnOptionSelected(int option)
    {
        DialogueNode node = currentNPC.dialogues[currentIndex];
        int next = option == 0 ? node.nextNodeIndex0 : node.nextNodeIndex1;
        if (next == -1 || next >= currentNPC.dialogues.Count)
            GameManager.Instance.EnterMixing();
        else
        {
            currentIndex = next;
            ShowDialogueNode(currentNPC.dialogues[currentIndex]);
        }
    }
}
