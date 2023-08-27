using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
//using Effekseer;

public class Magic : MonoBehaviour
{
    [SerializeField]
    private float speed = 5; //魔法の移動スピード
    Rigidbody2D rb;

    Player pl;

    //エフェクト
   // EffekseerEffectAsset effect;

    void Start()
    {
        pl = GameObject.Find("P2").GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        //自動破裂
        Destroy(this.gameObject, 1.5f);

        //P2のスケールをみて撃つ方向を決める
        if (pl.transform.localScale.x > 0)
        {
            rb.velocity = new Vector3(speed, 0, 0);
        }
        else if(pl.transform.localScale.x < 0)
        {
            rb.velocity = new Vector3(-speed, 0, 0);
        }
        //effect = Resources.Load<EffekseerEffectAsset>("FireHit");   //エフェクトを取得
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Hit")
        {
            Destroy(this.gameObject);    //ぶつかったら破裂
            //EffekseerHandle handle = EffekseerSystem.PlayEffect(effect, transform.position);
        }
    }
}
