using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))] // 确保挂载此脚本的物体有 Button 和 Image 组件
public class IngredientItemUI : MonoBehaviour
{
    [Header("查找配置")]
    [Tooltip("相对于 Resources 文件夹的路径。例如放在 Resources/Liquors 下，这里填 Liquors/")]
    public string resourcePath = "Liquors/"; 

    [Header("运行时数据 (自动加载或手动拖拽)")]
    private IngredientData myIngredientData; 
    
    private Button myButton;
    private Image myImage;

    void Start()
    {
        myButton = GetComponent<Button>();
        myImage = GetComponent<Image>();
        
        // 1. 启动时自动加载对应路径下的原料数据
        LoadIngredientData();

        // 2. 自动给按钮绑定点击事件
        if (myButton != null && myIngredientData != null)
        {
            myButton.onClick.AddListener(OnItemClicked);
        }

        // 3. 监听 GameManager 广播的“有材料解锁了”事件，自动刷新颜色
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onIngredientUnlocked += UpdateVisual;
        }

        // 4. 初始刷新一次颜色
        UpdateVisual();
    }

    void OnDestroy()
    {
        // 记得注销事件，防止内存泄漏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onIngredientUnlocked -= UpdateVisual;
        }
    }

    void LoadIngredientData()
    {
        // 如果你已经在 Inspector 面板上手动拖拽赋值了，就跳过自动加载
        if (myIngredientData != null) return;

        // 拼接路径，例如 "Liquors/Gin"
        string fullPath = resourcePath + gameObject.name.Trim();
        
        // 核心API：从 Resources 文件夹中读取指定类型的资产
        myIngredientData = Resources.Load<IngredientData>(fullPath);

        if (myIngredientData == null)
        {
            Debug.LogError($"[自动加载失败] 无法在 Resources/{resourcePath} 下找到名为 '{gameObject.name.Trim()}' 的原料数据！请检查拼写。");
        }
    }

    void OnItemClicked()
    {
        if (myIngredientData != null && GameManager.Instance != null)
        {
            // 推荐直接通过 GameManager 调用，比 FindObjectOfType 性能更好
            GameManager.Instance.mixManager.SelectIngredient(myIngredientData);
        }
    }

    // 更新颜色：黑灰色 或 原图色
    public void UpdateVisual()
    {
        if (myIngredientData == null || GameManager.Instance == null || myImage == null) return;

        if (GameManager.Instance.IsIngredientUnlocked(myIngredientData))
        {
            myImage.color = Color.white; // 解锁状态，正常颜色
        }
        else
        {
            myImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // 锁定状态，黑灰色
        }
    }
}