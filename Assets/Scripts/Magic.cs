using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Effekseer;

public class Magic : MonoBehaviour
{
    [SerializeField]
    private float speed = 5; //魔法の移動スピード
    Rigidbody2D rb;
    //エフェクト
   // EffekseerEffectAsset effect;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        //自動破裂
        Destroy(this.gameObject, 1.5f);

        //effect = Resources.Load<EffekseerEffectAsset>("FireHit");   //エフェクトを取得
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Hit")
        {
            Destroy(this.gameObject);    //ぶつかったら破裂
            //EffekseerHandle handle = EffekseerSystem.PlayEffect(effect, transform.position);
        }
    }
}
