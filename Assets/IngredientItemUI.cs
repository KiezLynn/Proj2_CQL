using UnityEngine;
using UnityEngine.UI;

public class IngredientItemUI : MonoBehaviour
{
    [Header("查找配置")]
    [Tooltip("相对于 Resources 文件夹的路径。例如放在 Resources/Liquors 下，这里填 Liquors/")]
    public string resourcePath = "Liquors/"; 

    // 运行时自动加载的数据将被缓存在这里
    private IngredientData myIngredientData; 
    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        
        // 1. 启动时自动加载对应路径下的原料数据
        LoadIngredientData();

        // 2. 自动给按钮绑定点击事件
        if (myButton != null && myIngredientData != null)
        {
            myButton.onClick.AddListener(OnItemClicked);
        }
    }

    void LoadIngredientData()
    {
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
        if (myIngredientData != null)
        {
            FindObjectOfType<MixManager>().SelectIngredient(myIngredientData);
        }
    }
}