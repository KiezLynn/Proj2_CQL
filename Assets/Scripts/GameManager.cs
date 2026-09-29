using System;
using System.Collections.Generic;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DialogueManager dialogueManager;
    public MixManager mixManager;

    [Header("NPC Settings")]
    public List<NPCData> customerNPCs;  
    private bool isFirstDialogue = true; 
    
    // 【新增】用于记录当前这一轮还未出场的NPC队列
    private List<NPCData> upcomingNPCs = new List<NPCData>();

    [Header("InGame Data")]
    public NPCData currentNPC; 
    public BeverageData currentBeverage; 
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

    // 当前顾客账单临时变量（用于单次结账计算和飘字，依然每轮归零）
    private int currentCustomerIncome = 0; 
    private int currentCustomerTip = 0;    

    // 全局累计变量（用于UI面板的常驻显示，永不归零）
    public int accumulatedIncome = 0;
    public int accumulatedTip = 0;

    [Header("Economy UI (UI面板绑定)")]
    public TextMeshProUGUI balanceText; 
    public TextMeshProUGUI incomeText;  
    public TextMeshProUGUI tipText;     
    public TextMeshProUGUI totalText;   
    
    [Header("Pay Bill & Game Over UI")]
    public GameObject payBillPanel;         // 结算弹框图层
    public TextMeshProUGUI billSalesText;   // 弹框中的 Sales (Scales) 金额文本
    public TextMeshProUGUI billTipText;     // 弹框中的 Tip 金额文本
    public TextMeshProUGUI billTotalText;   // 弹框中的 Total 金额文本
    public Button billConfirmButton;        // 弹框下方的确认按钮 (打钩按钮)
    
    [Header("Floating Animation")]
    public GameObject floatingTextPrefab; 
    public Transform incomeSpawnPoint;    
    public Transform tipSpawnPoint;       
    
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
    
    public GameObject gameOverPanel;        // GameOver图层

    void Awake() => Instance = this;

    void Start() 
    {
        if (billConfirmButton != null)
        {
            billConfirmButton.onClick.AddListener(OnConfirmBill);
        }
        if (payBillPanel != null) payBillPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        UpdateEconomyUI(); 
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
            UpdateEconomyUI(); 
            onIngredientUnlocked?.Invoke(); 

            if (ing.cost > 0)
            {
                CreateFloatingText($"-{ing.cost}", Color.red, incomeSpawnPoint);
            }

            if (totalEarnings <= 0)
            {
                TriggerGameOver();
            }

            return true;
        }
        return false;
    }

    // 【修改】获取下一个NPC的逻辑
    public NPCData GetRandomCustomer()
    {
        if (customerNPCs == null || customerNPCs.Count == 0) return null;

        // 如果本轮出场队列为空，则重新填满并打乱顺序
        if (upcomingNPCs.Count == 0)
        {
            ShuffleAndRefillNPCs();
        }

        // 队列先进先出：取出第一个NPC，然后将其从出场队列中移除
        NPCData nextNPC = upcomingNPCs[0];
        upcomingNPCs.RemoveAt(0);
        return nextNPC;
    }

    // 【新增】洗牌并重置NPC队列的方法
    private void ShuffleAndRefillNPCs()
    {
        upcomingNPCs.Clear();
        upcomingNPCs.AddRange(customerNPCs);

        // 使用 Fisher-Yates 洗牌算法随机打乱列表
        for (int i = upcomingNPCs.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            NPCData temp = upcomingNPCs[i];
            upcomingNPCs[i] = upcomingNPCs[j];
            upcomingNPCs[j] = temp;
        }
        
        Debug.Log("所有NPC均已出场，已重新生成随机出场队列！");
    }

    public void EvaluateDrinkResult()
    {
        currentFeedbackStage = FeedbackStage.QualityFeedback;
        bool isSatisfied = (successRate >= 0.5f); 
        
        if (madeBeverage != null)
        {
            currentCustomerIncome += madeBeverage.price; // 仅记录本单
        }
        
        if (successRate >= 0.99f)
        {
            currentCustomerTip += 10; // 仅记录本单
        }

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
                currentFeedbackStage = FeedbackStage.None;
                isPlayingFeedback = false;
                hasRefilled = false; 
                
                ShowPayBillPanel();
            }
        }
        else
        {
            EnterMixing();
        }
    }
    
    private void ShowPayBillPanel()
    {
        if (payBillPanel != null)
        {
            payBillPanel.SetActive(true);
            
            if (billSalesText != null) billSalesText.text = currentCustomerIncome.ToString();
            if (billTipText != null) billTipText.text = currentCustomerTip.ToString();
            
            int total = currentCustomerIncome + currentCustomerTip;
            if (billTotalText != null) billTotalText.text = total.ToString();
        }
        else
        {
            OnConfirmBill();
        }
    }
    
    private void OnConfirmBill()
    {
        if (payBillPanel != null) payBillPanel.SetActive(false);
        
        accumulatedIncome += currentCustomerIncome;
        accumulatedTip += currentCustomerTip;
        
        int thisOrderTotal = currentCustomerIncome + currentCustomerTip;
        totalEarnings += thisOrderTotal; 

        if (currentCustomerIncome > 0)
        {
            CreateFloatingText($"+{currentCustomerIncome}", Color.green, incomeSpawnPoint);
        }
        else if (currentCustomerIncome < 0)
        {
            CreateFloatingText($"{currentCustomerIncome}", Color.red, incomeSpawnPoint); 
        }

        if (currentCustomerTip > 0)
        {
            CreateFloatingText($"+{currentCustomerTip}", new Color(1f, 0.8f, 0f), tipSpawnPoint); 
        }

        Debug.Log($"结账完毕！本单入账: {thisOrderTotal}。当前总余额: {totalEarnings}");
        
        UpdateEconomyUI();

        if (totalEarnings <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            NextCustomer(); 
        }
    }
    
    private void TriggerGameOver()
    {
        Debug.Log("余额不足，Game Over！");
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        DialoguePan.SetActive(false);
        TiaojiuPan.SetActive(false);
    }

    private void NextCustomer()
    {
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

    public void UpdateEconomyUI()
    {
        if (balanceText != null) balanceText.text = totalEarnings.ToString();
        if (incomeText != null) incomeText.text = accumulatedIncome.ToString();
        if (tipText != null) tipText.text = accumulatedTip.ToString();
        if (totalText != null) totalText.text = (accumulatedIncome + accumulatedTip).ToString();
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
    
    public void CreateFloatingText(string message, Color color, Transform spawnParent)
    {
        if (floatingTextPrefab == null || spawnParent == null) return;

        GameObject go = Instantiate(floatingTextPrefab, spawnParent);
        go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
        
        go.SetActive(true); 

        FloatingText floatingText = go.GetComponent<FloatingText>();
        if (floatingText != null)
        {
            floatingText.Setup(message, color);
        }
    }
}