using System;
using System.Collections.Generic;
using TMPro; // 引入文字UI命名空间
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DialogueManager dialogueManager;
    public MixManager mixManager;

    [Header("NPC Settings")]
    public List<NPCData> customerNPCs;  
    private bool isFirstDialogue = true; 

    [Header("InGame Data")]
    public NPCData currentNPC; 
    public BeverageData currentBeverage; // 当前NPC需要的酒水配方
    public List<IngredientData> selectedIngredients = new List<IngredientData>(); 
    
    [Header("Beverages Library")]
    public List<BeverageData> allBeverages; 
    public BeverageData failedBeverage;     
    public BeverageData madeBeverage;       

    public enum GameState { Dialogue, Mixing, Result }
    public GameState currentState;
    
    public enum FeedbackStage { None, QualityFeedback, RefillFeedback, CheckoutFeedback }
    public FeedbackStage currentFeedbackStage = FeedbackStage.None;
    private bool isPlayingFeedback = false; 
    public bool hasRefilled = false; 

    [Header("Player Economy (经济系统)")]
    public float successRate = 0f;
    public int totalEarnings = 100; // Balance (余额)，初始100

    // 顾客账单临时变量
    private int currentCustomerIncome = 0; // Income (本单饮品总售价)
    private int currentCustomerTip = 0;    // Tip (本单小费)
    
    [Header("Economy UI (UI面板绑定)")]
    public TextMeshProUGUI balanceText; // 绑定显示 Balance(余额) 的 Text
    public TextMeshProUGUI incomeText;  // 绑定显示 Income(收入) 的 Text
    public TextMeshProUGUI tipText;     // 绑定显示 Tip(小费) 的 Text
    public TextMeshProUGUI totalText;   // 绑定显示 Total(总计) 的 Text
    
    [Header("Floating Animation")]
    public GameObject floatingTextPrefab; // 飘字预制体
    public Transform incomeSpawnPoint;    // 收入飘字生成位置（例如绑定在Income Text上）
    public Transform tipSpawnPoint;       // 小费飘字生成位置（例如绑定在Tip Text上）
    
    // 已解锁的配料列表
    public List<IngredientData> unlockedIngredients = new List<IngredientData>();
    public Action onIngredientUnlocked; 

    [Header("UI Panel")]
    public GameObject HomePan;
    public GameObject GamePan;
    public GameObject DialoguePan;
    public GameObject TiaojiuPan;
    public GameObject ResultPan;
    public GameObject Scene01;
    public GameObject Scene02;

    void Awake() => Instance = this;

    void Start() 
    {
        UpdateEconomyUI(); // 初始化刷新一下金钱显示
        StartGame();
    }

    public void StartGame() => GameInitialize();

    public bool IsIngredientUnlocked(IngredientData ing)
    {
        if (ing.cost <= 0) return true; 
        return unlockedIngredients.Contains(ing);
    }

    public bool UnlockIngredient(IngredientData ing)
    {
        if (totalEarnings >= ing.cost)
        {
            totalEarnings -= ing.cost;
            unlockedIngredients.Add(ing);
            UpdateEconomyUI(); // 花钱后实时刷新余额UI
            onIngredientUnlocked?.Invoke(); 

            // 【新增】触发解锁扣除金币的飘字动画
            if (ing.cost > 0)
            {
                // 直接使用 balanceText.transform 作为生成位置，红色的扣费数字会从余额处往上飘
                CreateFloatingText($"-{ing.cost}", Color.red, incomeSpawnPoint);
            }

            return true;
        }
        return false;
    }

    public NPCData GetRandomCustomer()
    {
        if (customerNPCs == null || customerNPCs.Count == 0) return null;
        return customerNPCs[UnityEngine.Random.Range(0, customerNPCs.Count)];
    }

    public void EvaluateDrinkResult()
    {
        currentFeedbackStage = FeedbackStage.QualityFeedback;
        bool isSatisfied = (successRate >= 0.5f); 
        
        // 1. 读取饮品的标价，加入账单 (如果是暗黑料理，请在它的配置里把 price 设为 0)
        if (madeBeverage != null)
        {
            currentCustomerIncome += madeBeverage.price;
        }
        
        // 2. 调出NPC对应饮品，奖励 10 元小费 (失败自然不满足，为0)
        if (successRate >= 0.99f)
        {
            currentCustomerTip += 10;
        }

        // 3. 实时刷新收银台面板
        UpdateEconomyUI();

        if (isSatisfied) EnterFeedback(currentNPC.satisfiedDialogue);
        else EnterFeedback(currentNPC.dissatisfiedDialogue);
    }

    public void OnDialogueComplete()
    {
        if (isFirstDialogue)
        {
            isFirstDialogue = false;
            NextCustomer();
            return;
        }

        if (isPlayingFeedback)
        {
            if (currentFeedbackStage == FeedbackStage.QualityFeedback)
            {
                // 满意且没续过杯，50%概率触发续杯
                bool willRefill = (!hasRefilled && successRate >= 0.5f && UnityEngine.Random.value >= 0.5f); 

                if (willRefill)
                {
                    hasRefilled = true; 
                    currentFeedbackStage = FeedbackStage.RefillFeedback;
                    EnterFeedback(currentNPC.refillDialogue); 
                }
                else
                {
                    currentFeedbackStage = FeedbackStage.CheckoutFeedback;
                    EnterFeedback(currentNPC.checkoutDialogue); 
                }
            }
            else if (currentFeedbackStage == FeedbackStage.RefillFeedback)
            {
                currentFeedbackStage = FeedbackStage.None;
                isPlayingFeedback = false;
                EnterMixing(); 
            }
            else if (currentFeedbackStage == FeedbackStage.CheckoutFeedback)
            {
                // 【最后结账阶段】
                int total = currentCustomerIncome + currentCustomerTip;
                totalEarnings += total; 

                // --- 触发动画 ---
                // 1. 饮品收入飘字
                if (currentCustomerIncome > 0)
                {
                    CreateFloatingText($"+{currentCustomerIncome}", Color.green, incomeSpawnPoint);
                }
                else if (currentCustomerIncome < 0)
                {
                    CreateFloatingText($"{currentCustomerIncome}", Color.red, incomeSpawnPoint); // 负数(暗黑料理)显示红色
                }

                // 2. 小费飘字
                if (currentCustomerTip > 0)
                {
                    CreateFloatingText($"+{currentCustomerTip}", new Color(1f, 0.8f, 0f), tipSpawnPoint); // 小费显示金色
                }

                Debug.Log($"结账完毕！收入: {currentCustomerIncome}，小费: {currentCustomerTip}，总入账: {total}。当前总余额: {totalEarnings}");
                UpdateEconomyUI();

                currentFeedbackStage = FeedbackStage.None;
                isPlayingFeedback = false;
                hasRefilled = false; 
                NextCustomer(); 
            }
        }
        else
        {
            EnterMixing();
        }
    }

    private void NextCustomer()
    {
        // 迎接新客人时，清空账单并刷新UI
        currentCustomerIncome = 0;
        currentCustomerTip = 0;
        UpdateEconomyUI();

        NPCData nextNPC = GetRandomCustomer();
        if (nextNPC != null)
        {
            currentNPC = nextNPC;
            currentBeverage = nextNPC.favoriteBeverage; 
            EnterDialogue();
        }
    }

    // 统一更新经济系统的数值显示
    public void UpdateEconomyUI()
    {
        if (balanceText != null) balanceText.text = totalEarnings.ToString();
        if (incomeText != null) incomeText.text = currentCustomerIncome.ToString();
        if (tipText != null) tipText.text = currentCustomerTip.ToString();
        if (totalText != null) totalText.text = (currentCustomerIncome + currentCustomerTip).ToString();
    }

    public void EnterDialogue()
    {
        currentState = GameState.Dialogue;
        isPlayingFeedback = false; 
        
        DialoguePan.SetActive(true);
        TiaojiuPan.SetActive(false);
        ResultPan.SetActive(false);
        
        dialogueManager.StartOpeningDialogue(currentNPC);
    }
    
    public void EnterFeedback(DialogueNode feedbackNode)
    {
        currentState = GameState.Dialogue;
        isPlayingFeedback = true; 
        
        Scene01.SetActive(true);
        Scene02.SetActive(false);
        DialoguePan.SetActive(true);
        TiaojiuPan.SetActive(false);
        ResultPan.SetActive(false);

        dialogueManager.PlaySingleFeedback(currentNPC, feedbackNode);
    }

    public void EnterMixing()
    {
        currentState = GameState.Mixing;
        DialoguePan.SetActive(false);
        TiaojiuPan.SetActive(true);
        ResultPan.SetActive(false);
        Scene01.SetActive(false);
        Scene02.SetActive(true);
    
        mixManager.Initialize();
    }
    
    public void EnterResult()
    {
        currentState = GameState.Result;
        ResultPan.SetActive(true);
    }

    void GameInitialize()
    {
        HomePan.SetActive(true);
        GamePan.SetActive(false);
        ResultPan.SetActive(false);
        Scene01.SetActive(false);
        Scene02.SetActive(false);
        DialoguePan.SetActive(false);
        TiaojiuPan.SetActive(false);
    }

    public void inGame()
    {
        HomePan.SetActive(false);
        GamePan.SetActive(true);
        Scene01.SetActive(true);
        EnterDialogue();
    }
    
    // 辅助生成飘字的方法
    public void CreateFloatingText(string message, Color color, Transform spawnParent)
    {
        if (floatingTextPrefab == null || spawnParent == null) return;

        // 生成在指定的 UI 父节点下
        GameObject go = Instantiate(floatingTextPrefab, spawnParent);
        // 重置位置到父节点中心
        go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
        
        // 【关键修复】强制激活物体，防止预制体默认处于隐藏状态导致协程报错
        go.SetActive(true); 

        FloatingText floatingText = go.GetComponent<FloatingText>();
        if (floatingText != null)
        {
            floatingText.Setup(message, color);
        }
    }
}