using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public struct PosAndSprite
{
    public Vector2 pos;
    public Sprite sprite;
}

public class IngredientsMoveControl : MonoBehaviour
{
    [Header("移动 object")]
    public GameObject moveObject;
    [Header("初始位置 和 图标")]
    public PosAndSprite initData;
    [Header("移动之后的位置 和 图标")]
    public PosAndSprite moveData;
    [Header("动画时长")]
    public float duration;
    
    private bool isInintPos = false;
    
    private Image btnImage;
    private Button _button;
    void Start()
    {
        btnImage = GetComponent<Image>();
        _button = GetComponent<Button>();
        if(moveObject) moveObject.transform.localPosition = initData.pos;
        if(btnImage) btnImage.sprite = initData.sprite;
        
        _button.onClick.AddListener(Move);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move()
    {
        if (moveObject)
        {
            Sprite sprite;
            Vector2 initPos, movePos;
            if (!isInintPos)
            {
                sprite = moveData.sprite;
                initPos = initData.pos;
                movePos = moveData.pos;
            }
            else
            {
                sprite = initData.sprite;
                initPos = moveData.pos;
                movePos = initData.pos;
            }
            if(btnImage) btnImage.sprite = sprite;
            Tween.LocalPosition(moveObject.transform, initPos, movePos, duration)
                .OnComplete(target:this, target => {target.isInintPos = !target.isInintPos;});
        }
    }
}
