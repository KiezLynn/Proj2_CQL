using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 100f; // 向上飘动的速度
    public float duration = 1.5f;  // 持续时间（秒）

    private TextMeshProUGUI textMeshPro;
    private RectTransform rectTransform;

    public void Setup(string text, Color color)
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        textMeshPro.text = text;
        textMeshPro.color = color;

        // 启动飘字和淡出动画
        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade()
    {
        float elapsed = 0f;
        Color startColor = textMeshPro.color;

        while (elapsed < duration)
        {
            // 向上移动
            rectTransform.anchoredPosition += Vector2.up * (moveSpeed * Time.deltaTime);
            
            // 计算透明度 (从 1 渐变到 0)
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 动画结束后销毁自身
        Destroy(gameObject);
    }
}