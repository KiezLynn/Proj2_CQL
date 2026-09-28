using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTools : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// 场景跳转
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadGameScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGameScene()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    /// <summary>
    /// 重新开始
    /// </summary>
    public void RestartCurrentLevel()
    {
        // 重新加载当前活动的场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
