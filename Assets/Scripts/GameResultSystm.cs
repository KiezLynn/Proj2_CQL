using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResultSystm : MonoBehaviour
{
    public Text resultText;
    public Text earningsText;
    public Button continueButton;

    public void ShowResult()
    {
        float rate = GameManager.Instance.successRate;
        bool satisfied = rate >= 0.6f;
        BeverageData bev = GameManager.Instance.currentBeverage;
        int tip = satisfied ? Mathf.RoundToInt(bev.price * 0.2f) : 0;
        int total = bev.price + tip;

        GameManager.Instance.totalEarnings += total;

        resultText.text = satisfied ? "客人很满意！" : "客人不太满意...";
        earningsText.text = $"酒水：{bev.price}  小费：{tip}  总计：{total}";

        // 续杯判定
        bool another = Random.value < 0.5f;
        if (another)
        {
            continueButton.GetComponentInChildren<Text>().text = "客人还想再点一杯";
            continueButton.onClick.AddListener(() => {
                // 重置调酒状态，重新开始调酒（不清除累计收入）
                GameManager.Instance.selectedIngredients.Clear();
                // 随机换一种酒水
                //GameManager.Instance.currentBeverage = GetRandomBeverage();
                // 切换回调酒状态
                GameManager.Instance.EnterMixing();
            });
        }
        else
        {
            continueButton.GetComponentInChildren<Text>().text = "结束营业";
            continueButton.onClick.AddListener(() => {
                // 显示最终总结
                Debug.Log("今日总收入：" + GameManager.Instance.totalEarnings);
                // 可重新开始游戏
            });
        }
    }

}
