using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class _Base : MonoBehaviour
{

    [SerializeField]
    public int hpMax;                              // hp 最大値(初期値)

    [SerializeField]
    public AudioClip seAttack, seDamage, seDead;   //サウンド

    public AudioSource snd   { get; set; }           //サウンド
    public Animator    anim  { get; set; }           //アニメーション
    public Rigidbody2D rb    { get; set; }           //物理演算
    public SpriteRenderer sp { get; set; }           //スプライト

    public bool  isAttack    { get; set; }          //攻撃中?
    public bool  isAtkMotion { get; set; }          //攻撃モーションか?
    public int   hp          { get; set; }          //体力
    public float scale       { get; set; }          //横の拡大率

    #if UNITY_EDITOR                                //エディターのみ有効
    public TextMeshProUGUI debug;
    #endif

    //リスタート
    public virtual void Restart()
    {}

    void Start()
    {
        anim  = GetComponent<Animator>();
        rb    = GetComponent<Rigidbody2D>();
        snd   = gameObject.AddComponent<AudioSource>();     //サウンドを追加する
        scale = gameObject.transform.localScale.x;          //元のサイズ
        sp    = GetComponent<SpriteRenderer>();
        #if UNITY_EDITOR
            //debug = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
        #endif

        Restart();
    }

    //効果音再生
    public void SoundPlay(AudioClip se)
    {
        snd.PlayOneShot(se);
    }

    //スケール(反転に使用する)
    public float scaleX
    {
        get      //読み
        {
            return transform.localScale.x;
        }
        set     //書き
        {
            Vector3 scale = transform.localScale;
            scale.x = value;
            transform.localScale = scale;
        }
    }

    // ローカル座標
     public float lx
    {
        get      //読み
        {
            return transform.localPosition.x;
        }
        set     //書き
        {
            Vector3 localPos = transform.localPosition;
            localPos.x = value;
            transform.localPosition = localPos;
        }
    }

    //グローバル座標
    public float gx
    {
        get       //読み
        {
            return transform.position.x;
        }
        set       //書き
        {
            Vector3 pos = transform.position;
            pos.x = value;
            transform.position = pos;
        }
    }

    //グローバル座標
    public float gy
    {
        get       //読み
        {
            return transform.position.y;
        }
        set       //書き
        {
            Vector3 pos = transform.position;
            pos.y = value;
            transform.position = pos;
        }
    }

    // xy座標
    public Vector2 xy
    {
        get       //読み
        {
            return transform.position;
        }
        set      //書き
        {
            transform.position = value;
        }
    }

    //デバッグ情報表示
    public void DebugPrint(string str)
    {
    #if UNITY_EDITOR
            if (debug == null)
            {
                debug = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
            }
           debug.text = str;
        #endif
    }


}
