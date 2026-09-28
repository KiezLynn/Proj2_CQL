using UnityEngine;

public class QuitWebGLControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL
        // 如果是 WebGL 平台，直接把“退出游戏”按钮隐藏掉
        gameObject.SetActive(false); 
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}