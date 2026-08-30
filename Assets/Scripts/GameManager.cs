using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DialogueManager dialogueManager;
    public MixManager mixManager;

    [Header("NPC Settings")]
    public List<NPCData> customerNPCs;  // 存放所有顾客 NPC（拖入 #1, #2, #3...）
    private bool isFirstDialogue = true; // 标记是否是引导 NPC 的对话

    [Header("InGame Data")]
    public NPCData currentNPC;//当前出现的NPC
    public BeverageData currentBeverage;//当前NPC需要的酒水
    public List<IngredientData> selectedIngredients = new List<IngredientData>();// 玩家选择的原料列表（最多4个）

    //游戏状态机
    public enum GameState { Dialogue, Mixing, Result }
    public GameState currentState;

    // 对话进度
    private int dialogueIndex = 0;

    // 调酒结果
    public int totalPoints = 0;
    public float successRate = 0f;

    // 收入累计
    public int totalEarnings = 0;
    public bool hasExtraTip = false;
    public bool wantsAnother = false;

    [Header("UI Panel")]
    public GameObject HomePan;
    public GameObject GamePan;
    public GameObject UIPan;
    public GameObject DialoguePan;
    public GameObject TiaojiuPan;
    public GameObject ResultPan;
    public GameObject Scene01;
    public GameObject Scene02;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    // 状态切换方法
    public void StartGame()
    {
        GameInitialize();
    }

    // 随机获取一个顾客 NPC
    public NPCData GetRandomCustomer()
    {
        if (customerNPCs == null || customerNPCs.Count == 0)
        {
            Debug.LogError("No Customer NPC Data");
            return null;
        }
        int randomIndex = Random.Range(0, customerNPCs.Count);
        return customerNPCs[randomIndex];
    }

    public void OnDialogueComplete()
    {
        if (isFirstDialogue)
        {
            //随机选顾客
            isFirstDialogue = false;
            NPCData nextNPC = GetRandomCustomer();
            if (nextNPC != null)
            {
                EnterDialogue(); //开始顾客的对话
            }
        }
        else
        {
            //顾客对话结束进入调酒
            EnterMixing();
        }
    }

    public void EnterDialogue()
    {
        currentState = GameState.Dialogue;
        DialoguePan.SetActive(true);
        TiaojiuPan.SetActive(false);
        ResultPan.SetActive(false);
        // 启动对话（传入当前NPC，从对话列表开头开始）
        dialogueManager.StartDialogue(currentNPC);
    }
    public void EnterMixing()
    {
        currentState = GameState.Mixing;
        DialoguePan.SetActive(false);
        TiaojiuPan.SetActive(true);
        ResultPan.SetActive(false);
        // 重置原料选择
        selectedIngredients.Clear();
        mixManager.Initialize();
    }
    public void EnterResult()
    {
        ResultPan.SetActive(true);
    }

    //UI切换办法
    void GameInitialize()
    {
        HomePan.gameObject.SetActive(true);
        GamePan.gameObject.SetActive(false);
        ResultPan.gameObject.SetActive(false);
        Scene01.gameObject.SetActive(false);
        Scene02.gameObject.SetActive(false);
        DialoguePan.gameObject.SetActive(false);
        TiaojiuPan.gameObject.SetActive(false);
    }

    public void inGame()
    {
        HomePan.gameObject.SetActive(false);
        GamePan.gameObject.SetActive(true);
        Scene01.gameObject.SetActive(true);
        //DialoguePan.gameObject.SetActive(true);

        //Invoke("EnterDialogue", 1f);
        EnterDialogue();
    }

    public void inTiaojiuSet()
    {
        Scene01.gameObject.SetActive(false);
        Scene02.gameObject.SetActive(true);
        DialoguePan.gameObject.SetActive(false);
        TiaojiuPan.gameObject.SetActive(true);
    }


}
