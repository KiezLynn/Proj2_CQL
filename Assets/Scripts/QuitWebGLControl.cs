using UnityEngine;
using UnityEngine.UI;

public class QuitWebGLControl : MonoBehaviour
{
    private Button quitButton;
    void Start()
    {
#if !UNITY_WEBGL
        quitButton = GetComponent<Button>();
        GameTools gameTools = FindObjectOfType<GameTools>();
        if(gameTools)
            quitButton.onClick.AddListener(gameTools.ExitGameScene);
        else
            Debug.LogError("场景中未找到 GameTools 组件，无法绑定退出按钮！");
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}