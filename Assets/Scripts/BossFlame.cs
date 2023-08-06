using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Effekseer;

public class BossFlame : _Base
{
    [SerializeField]
    public AudioClip seDamage2, seAttack2, seAttack3; //サウンド

    [SerializeField]
    float speed = 1.0f;    　　　　　　　　　　　    //移動速度

    Player pl;              　　　　　　　　　　　   //プレイヤー
    //Player2 pl2;                                     //2P

    enum BossMode{Standby, Setup, Attack};
    BossMode mode = BossMode.Standby;

    //float damageTime;        　　　　　　　　　　  //ダメージ硬直時間
    float atkTime;               　　　　　　　　　 //攻撃間隔
    float moveTime;                              //待ち時間
    int   rndmJump;                              //ランダムでジャンプしたい用

    float atkTarget;                             //攻撃する方を選びたい

    float speedX;
    float dir = 1;                 　　　　　　　   //向き
    //float dir2 = 1;

    float StartBossFightTime = 1.5f;             //ボス戦が始まる時間

    public Slider slider;                        //スライダー

    bool isLeft;                                 //左向き

    public CameraController shake;

    //エフェクト
    EffekseerEffectAsset[] effect = null;
    readonly Vector2 breathPos = new Vector2(3.4f, 4.6f);
    readonly Vector2 impactPos = new Vector2(3.04f, -0.46f);
    

    //リスタート
    public override void Restart()
    {
        //damageTime = 0;
        isLeft = true;                          //左を向いている
        slider.value = 1;                       //スライダーを最大にする
        hp = hpMax;
        pl = GameObject.Find("P1").GetComponent<Player>();
        //pl2 = GameObject.Find("P2").GetComponent<Player2>();
        anim.SetFloat("speed", 0);              //アイドル
        anim.SetInteger("hp", hp);              //hp

        sp = GetComponent<SpriteRenderer>();

        //エフェクトを取得する
        if(effect == null)
        {
            effect = new EffekseerEffectAsset[2];
            effect[0] = Resources.Load<EffekseerEffectAsset>("BreathL");
            effect[1] = Resources.Load<EffekseerEffectAsset>("IMPACT2");
        }
    }

    void Update()
    {
        //モンスターの向き
        dir = pl.transform.position.x > lx ? 1 : -1;   //1P
        //dir2 = pl2.transform.position.x > lx ? 1 : -1; //2P
        speedX = speed * dir;
        scaleX = -scale * dir;

        switch(mode)
        {
            case BossMode.Standby:
                Standby();
                break;
            case BossMode.Setup:
                Setup();
                break;
            case BossMode.Attack:
                Attack();
                break;
        }
    }

    void Standby()
    {
        anim.SetFloat("speed", 0);          　　　　　　//アイドルにする
        StartBossFightTime -= Time.deltaTime;

        if(StartBossFightTime <= 0)
        {
            mode = BossMode.Setup;
            moveTime = Random.Range(2f, 5f);  　　　　　//2~5秒の間で乱数を得る
        }
    }

    void Setup()
    {
        //プレイヤー1との距離を取得
        float dis = Vector2.Distance(pl.transform.position, xy);
        //2Pとの距離
        //float dis2 = Vector2.Distance(pl2.transform.position, xy);
        moveTime -= Time.deltaTime;
        rb.bodyType = RigidbodyType2D.Dynamic;  　　　   　//動くように戻す

        if(hp > 0)
        {
            shake.Shake( 0.2f, 0.25f );                 //カメラを揺らす
            rb.velocity = new Vector3(speedX, rb.velocity.y, 0);
            anim.SetFloat("speed", 1);      　　　　　 　　//歩く
        }

        if(moveTime <= 0 || dis <=5f /*|| dis2 <= 5f*/)
        {
            atkTime = Random.Range(1f, 4f); 　　　　　 　　//1~4秒の間で乱数を得る
            rndmJump = Random.Range(1, 6);               //1~6で乱数を得る
            mode = BossMode.Attack;
        }
    }

    void Attack()
    {
        anim.SetFloat("speed", 0);
        //プレイヤーとの距離を取得
        float dis = Vector2.Distance(pl.transform.position, xy);
        //2Pとの距離を取得
        //float dis2 = Vector2.Distance(pl2.transform.position, xy);

        atkTime  -= Time.deltaTime;
        if(atkTime <= 0)
        {
           
            if(dis > 0 && dis <= 6 /*|| dis2 > 3 && dis2 <= 6*/)
            {
                if(rndmJump == 1)
                {
                    anim.SetTrigger("attackF");
                }
                else
                {
                    anim.SetTrigger("attackN");
                    mode = BossMode.Standby;
                }
            }
            else if(dis > 6 && dis <= 9 /*|| dis2 > 6 && dis2 <= 9*/)
            {
                if(rndmJump == 1)
                {
                    anim.SetTrigger("attackF");
                }
                else
                {
                    shake.Shake( 0.3f, 0.25f);            //カメラを揺らす
                    anim.SetTrigger("attackN2");
                    rb.bodyType = RigidbodyType2D.Static; //動かないようにする
                    mode = BossMode.Standby;
                }
            }
            else if(dis > 9 && dis <= 15 /*|| dis2 > 9 && dis2 <= 15*/)
            {
                if(rndmJump == 1)
                {
                    anim.SetTrigger("attackF");
                }
                else
                {
                    shake.Shake( 0.4f, 0.3f);             //カメラを揺らす   
                    anim.SetTrigger("attackM");
                    Invoke("StartEffect", 1f);
                    mode = BossMode.Standby;
                }
            }
            else if(dis > 15 /*|| dis2 > 15*/)
            {
                anim.SetTrigger("attackF");
            }

            StartBossFightTime = 2f;
            atkTime = 5f;
            rndmJump = 0;
        }
    }

    public void AttackEvent(int num)
    {
        switch(num)
        {
            case 0:     //攻撃開始
                break;
            case 1:     //飛び跳ねる
                this.rb.AddForce(transform.up * 12700);
                atkTarget = Random.Range(0, 4);        //1~4で乱数を得る
                break;
            case -1:    //移動する・攻撃
                if(atkTarget <= 1)
                {
                    if(isLeft)
                    {
                        gx = pl.transform.position.x - 6;
                        isLeft = false;
                        ImpactEffect();
                        SoundPlay(seAttack);
                        SoundPlay(seAttack2);
                    }
                    else
                    {
                        gx = pl.transform.position.x + 6;
                        isLeft = true;
                        ImpactEffect();
                        SoundPlay(seAttack);
                        SoundPlay(seAttack2);
                    }
                }
               /* else if(atkTarget >= 2)
                {
                    if(isLeft)
                    {
                        gx = pl2.transform.position.x - 6;
                        isLeft = false;
                        ImpactEffect();
                        SoundPlay(seAttack);
                        SoundPlay(seAttack2);
                    }
                    else
                    {
                        gx = pl2.transform.position.x + 6;
                        isLeft = true;
                        ImpactEffect();
                        SoundPlay(seAttack);
                        SoundPlay(seAttack2);
                    }
                }*/
                mode = BossMode.Setup;
                break;
        }
    }

    public void ShakeIvent(int num)
    {
        switch(num)
        {
            case 0:     //揺れ開始
                shake.Shake(0.1f, 0.2f);
                break;
            case 1:
                shake.Shake(0.5f, 0.2f);
                ImpactEffect();
                SoundPlay(seAttack);
                break;
            case 2:
                shake.Shake(0.7f, 0.4f);
                ImpactEffect();
                SoundPlay(seAttack);
                break;
            case 3:
                shake.Shake(0.1f, 0.15f);
                ImpactEffect();
                SoundPlay(seAttack);
                break;
            case 4:
                shake.Shake(0.7f, 0.5f);
                ImpactEffect();
                SoundPlay(seAttack);
                break;
            case -1:     //揺れ終わり
                shake.Shake(0.1f, 0.2f);
                break;
        }
    }

    //エフェクト再生
    public void StartEffect()
    {
        EffekseerHandle breath = EffekseerSystem.PlayEffect(effect[0], xy + new Vector2(breathPos.x * dir, breathPos.y));
        SoundPlay(seAttack3);

        //左向きはスケールを設定する
        breath.SetScale(new Vector3(-dir, 1, 1));
    }

    //エフェクト再生2
    public void ImpactEffect()
    {
        EffekseerHandle impact = EffekseerSystem.PlayEffect(effect[1], xy + new Vector2(impactPos.x * dir, impactPos.y));

        //左向きはスケールを設定する
        impact.SetScale(new Vector3(-dir, 1, 1));
    }

　　 //ダメージ処理
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "sword")
        {
            //hp処理
            hp = hp - 60;
            anim.SetInteger("hp", hp);
            pl.StartEffect();
            SoundPlay(seDamage);
            SoundPlay(seDamage2);

            //hpをSliderに反映
            slider.value = (float)hp / (float) hpMax;

            if(hp <= 0)
            {
                hp = 0;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
        else if(col.gameObject.tag == "plFire")
        {
            //hp処理
            hp = hp - 30;
            anim.SetInteger("hp", hp);
            SoundPlay(seDamage);

            //hpをSliderに反映
            slider.value = (float)hp / (float) hpMax;

            if(hp <= 0)
            {
                hp = 0;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
        else if(col.gameObject.tag == "plFireM")
        {
            //hp処理
            hp = hp - 80;
            anim.SetInteger("hp", hp);
            SoundPlay(seDamage);

            //hpをSliderに反映
            slider.value = (float)hp / (float) hpMax;

            if(hp <= 0)
            {
                shake.Shake(0.5f, 0.2f);
                hp = 0;
                rb.bodyType = RigidbodyType2D.Static;
                SoundPlay(seDead);
            }
        }
        else if(col.gameObject.tag == "plFireL")
        {
            //hp処理
            hp = hp - 300;
            anim.SetInteger("hp", hp);
            SoundPlay(seDamage);

            mode = BossMode.Standby;
            StartBossFightTime = 1f;

            //hpをSliderに反映
            slider.value = (float)hp / (float) hpMax;

            if(hp <= 0)
            {
                hp = 0;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }
}
