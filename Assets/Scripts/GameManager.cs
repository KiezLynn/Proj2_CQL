using System.Collections.Generic;
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

    public enum GameState { Dialogue, Mixing, Result }
    public GameState currentState;
    
    // 反馈状态枚举与续杯标记
    public enum FeedbackStage { None, QualityFeedback, RefillFeedback, CheckoutFeedback }
    public FeedbackStage currentFeedbackStage = FeedbackStage.None;
    private bool isPlayingFeedback = false; 
    public bool hasRefilled = false; 

    public int totalPoints = 0;
    public float successRate = 0f;
    public int totalEarnings = 0;

    [Header("UI Panel")]
    public GameObject HomePan;
    public GameObject GamePan;
    public GameObject DialoguePan;
    public GameObject TiaojiuPan;
    public GameObject ResultPan;
    public GameObject Scene01;
    public GameObject Scene02;

    void Awake() => Instance = this;

    void Start() => StartGame();

    public void StartGame() => GameInitialize();

    public NPCData GetRandomCustomer()
    {
        if (customerNPCs == null || customerNPCs.Count == 0) return null;
        return customerNPCs[Random.Range(0, customerNPCs.Count)];
    }

    public void EvaluateDrinkResult()
    {
        currentFeedbackStage = FeedbackStage.QualityFeedback;
        bool isSatisfied = (successRate >= 0.5f); 
        
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
                bool willRefill = (!hasRefilled && successRate >= 0.5f && Random.value > 0.5f); 

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
                EnterMixing(); // 续杯再次调酒
            }
            else if (currentFeedbackStage == FeedbackStage.CheckoutFeedback)
            {
                currentFeedbackStage = FeedbackStage.None;
                isPlayingFeedback = false;
                hasRefilled = false; 
                NextCustomer(); // 换下一位客人
            }
        }
        else
        {
            EnterMixing();
        }
    }

    private void NextCustomer()
    {
        NPCData nextNPC = GetRandomCustomer();
        if (nextNPC != null)
        {
            currentNPC = nextNPC;
            // 获取并锁定目标客人的偏好酒水
            currentBeverage = nextNPC.favoriteBeverage; 
            EnterDialogue();
        }
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
}