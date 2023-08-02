using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Effekseer;

public class Player : _Base
{
    private PlayerInput1 playerInput1_;　//InputSystemで操作

    [SerializeField]
    public AudioClip seSlash1, seSlash2, seSlash3, seMagicS, seJump;  //サウンド

    [SerializeField]
    float speed = 5.0f;                //移動速度
    [SerializeField]
    float dashSpeed = 4f;              //ダッシュの変数宣言
    //現在のスピードを保持しておく変数
    float currentSpeed;

    float dir = 1;                      //向き(1:右, -1:左)
    bool isRight;                       //右向き

    float blendRate = 0;                //ブレンド速度
    readonly float changeRate = 0.05f;  //アニメーションの変化速度

    [SerializeField]
    float jumpForce = 390.0f;           // ジャンプ時に加える力
    
    float jumpThreshold = 2.0f;         // ジャンプ中か判定するための閾値
    bool isGround = true;               // 地面と接地しているか管理するフラグ

    string state;                       //プレイヤーの状態管理
    string prevState;                   //前の状態を保存

    int atkMode = 0;                    //攻撃用

    [SerializeField]
    private GameObject magic;           //魔法プレハブを格納
    [SerializeField]
    private Transform magicPoint;       //アタックポイントを格納

    [SerializeField]
    private float attackTime = 0.3f;   //攻撃の間隔
    private float currentAttackTime;   //攻撃の間隔を管理
    private bool canAttack;            //攻撃可能状態かを指定するフラグ

    public Slider slider;              //スライダー

    /*private bool isShield = false;
    private float pressDuration = 0.5f; // 長押しの時間（秒）
    private float pressTime = 0f;*/

    //エフェクト
    EffekseerEffectAsset[] effect = null;
    readonly Vector2 hitPos = new Vector2(0.66f, 0.595f);
    
    
     //リスタート
    public override void Restart()
    {
        slider.value = 1;             　　　　  //スライダーを最大にする

        slider.gameObject.SetActive(true);

        hp = hpMax;

        isRight = true;                　　　　 //右向き

        currentSpeed = speed;          　　　　 //現在のスピードをスピードに設定

        currentAttackTime = attackTime; 　　　　//currentAttackTimeにattackTimeをセット。

        playerInput1_ = new PlayerInput1();　　//コントローラーで操作する用
        playerInput1_.Enable();

        //エフェクトを取得する
        if(effect == null)
        {
            effect = new EffekseerEffectAsset[1];
            effect[0] = Resources.Load<EffekseerEffectAsset>("effect");
            //effect[1] = Resources.Load<EffekseerEffectAsset>("suka");
        }
    }

    void Update()
    {
        // DebugPrint("HP : " + hp);

        Move();
        Attack();
        Magic();
        Jump();
        Shield();
        ChangeState();
        ChangeAnimation();
        SliderMove();
    }
    
    //移動・回転
    void Move()
    {
        if(isAtkMotion){return;}                 //攻撃中ならば移動できない

        float x = Input.GetAxisRaw("Horizontal");
        if(x > 0)
        {
            dir = 1;
            rb.velocity = new Vector3(currentSpeed * dir, rb.velocity.y, 0);
            blendRate += changeRate;
            if(blendRate > 1){blendRate = 1;}
            anim.SetFloat("speed", blendRate);   //歩き

            //レフトシフトが押されてるときダッシュ
            if(playerInput1_.Player.Dash.ReadValue<float>() != 0f)
            {
                //通常スピードにダッシュスピードをかける
                currentSpeed = speed * dashSpeed;
                anim.SetBool("dash", true);
            }
            //通常時
            else
            {
                //通常スピードに戻す
                currentSpeed = speed;
                anim.SetBool("dash", false);
            }
            //右向きで左入力なら180°回転
            if(isRight && x < 0)
            {
                transform.Rotate(0f, 180f, 0f);
                isRight = false;
            }
            //左向きで右入力なら180°回転
            if(!isRight && x > 0)
            {
                transform.Rotate(0f, 180f, 0f);
                isRight = true;
            }
        }
        else if(x < 0)
        {
            dir = -1;
            rb.velocity = new Vector3(currentSpeed * dir, rb.velocity.y, 0);
            blendRate += changeRate;
            if(blendRate > 1){blendRate = 1;}
            anim.SetFloat("speed", blendRate);   //歩き

            //レフトシフトが押されてるときダッシュ
            if(playerInput1_.Player.Dash.ReadValue<float>() != 0f)
            {
                //通常スピードにダッシュスピードをかける
                currentSpeed = speed * dashSpeed;
                anim.SetBool("dash", true);
            }
            //通常時
            else
            {
                //通常スピードに戻す
                currentSpeed = speed;
                anim.SetBool("dash", false);
            } 
            //右向きで左入力なら180°回転
            if(isRight && x < 0)
            {
                transform.Rotate(0f, 180f, 0f);
                isRight = false;
            }
            //左向きで右入力なら180°回転
            if(!isRight && x > 0)
            {
                transform.Rotate(0f, 180f, 0f);
                isRight = true;
            }
        }
        else
        {
            blendRate -= changeRate;
            if(blendRate < 0){blendRate = 0;}
            anim.SetFloat("speed", blendRate);  //止まる
        }
    }

    //ジャンプ
    void Jump()
    {
        if(isGround)
        {
            if(playerInput1_.Player.Jump.triggered)
            {
                this.rb.AddForce(transform.up * this.jumpForce);
                isGround = false;
                SoundPlay(seJump);
            }
        }
    }

    void ChangeState()
    {
        // 空中にいるかどうかの判定。上下の速度(rigidbody.velocity)が一定の値を超えている場合、空中とみなす
        if(Mathf.Abs(rb.velocity.y) > jumpThreshold)
        {
            isGround = false;
        }

        //接地している場合
        if(isGround)
        {
            // Locomotion
            state = "MOVE";
        }
        //空中にいる場合
        else
        {
             //上昇中
            if(rb.velocity.y > 0)
            {
                state = "JUMP";
            }
            //降下中
            else if(rb.velocity.y < 0)
            {
                state = "FALL";
            }
        }
    }

    //着地判定
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Ground")
        {
            if(!isGround)
            {
                isGround = true;
            }
        }
    }
    void OnTriggerStay2D(Collider2D col)
    {
        if(col.gameObject.tag == "Ground")
        {
            if(!isGround)
            {
                isGround = true;
            }
        }
    }

    void ChangeAnimation()
    {
        //状態が変わった場合のみアニメーションを変更する
        if(prevState != state)
        {
            switch(state)
            {
                case "JUMP":
                     anim.SetBool("jumpUp", true);
                     anim.SetBool("fall", false);
                     anim.SetBool("move", false);
                     break;
                case "FALL":
                     anim.SetBool("jumpUp", false);
                     anim.SetBool("fall", true);
                     anim.SetBool("move", false);
                     break;
                case "MOVE":
                     anim.SetBool("jumpUp", false);
                     anim.SetBool("fall", false);
                     anim.SetBool("move", true);
                     break;
            }
            // 状態の変更を判定するために状態を保存しておく
            prevState = state;
        }
    }

    //物理攻撃
    void Attack()
    {
        //通常・空中攻撃
        switch(atkMode)
        {
            //Attack 1
            case 0:
                if(playerInput1_.Player.Slash.triggered)
                {
                    anim.SetTrigger("attack1");
                    atkMode++;
                    SoundPlay(seSlash1);
                }
                break;
            //Attack 2
            case 1:
                if(playerInput1_.Player.Slash.triggered)
                {
                    anim.SetTrigger("attack1");
                    atkMode++;
                    SoundPlay(seSlash2);
                }
                break;
            //Attack 3
            case 2:
                if(playerInput1_.Player.Slash.triggered)
                {
                    anim.SetTrigger("attack1");
                    atkMode++;
                    SoundPlay(seSlash3);
                }
                break;
        }

        //上攻撃
        if(playerInput1_.Player.SlashOver.triggered)
        {
            anim.SetTrigger("attack2");
            SoundPlay(seSlash1);
        }
    }

    void Magic()
    {
        if(isAtkMotion){return;}                 //攻撃中ならば攻撃できない

        attackTime += Time.deltaTime;            //attackTimeに毎フレームの時間を加算していく

        if(attackTime > currentAttackTime) 
        {
            canAttack = true;                    //指定時間を超えたら攻撃可能にする
        }

         if (playerInput1_.Player.Magic.triggered)
        {
            if (canAttack)
            {                
                //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
                Instantiate(magic, magicPoint.position, transform.rotation);
                canAttack = false;　              //攻撃フラグをfalseにする
                attackTime = 0f;　                //attackTimeを0に戻す
                anim.SetTrigger("magicFiring");
                SoundPlay(seMagicS);
            }
        }
    }

    //攻撃アニメーションのイベント
    public void AttackEvent(int num)
    {
        switch(num)
        {
            case 0:     //攻撃開始
                isAtkMotion = true;
                break;
            case 1:     //攻撃判定が当たる
                isAttack = true;

                //EffekseerHandle handle = EffekseerSystem.PlayEffect(effect[1], xy + new Vector2(handPos.x * dir, handPos.y));

                break;
            case -1:    //攻撃やめ
                isAtkMotion = false;
                isAttack = false;
                atkMode = 0;
                break;
        }
    }

    void Shield()
    {
        //防御
        if(playerInput1_.Player.Shield.ReadValue<float>() > 0f)
        {
            anim.SetBool("shield", true);
            rb.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            anim.SetBool("shield", false);
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        /*//ジャストガード
        if(Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.X))
        {
            anim.SetTrigger("justGuard");
        }*/
    }

    //エフェクト再生
    public void StartEffect()
    {
        EffekseerHandle handle = EffekseerSystem.PlayEffect(effect[0], xy + new Vector2(hitPos.x * dir, hitPos.y));

        //左向きはスケールを設定する
        handle.SetScale(new Vector3(dir, 1, 1));
    }

    //コリジョン(当たり判定)　　col = 当たったゲームオブジェクト
    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "BossAtkHand")
        {
            //hp処理
            hp = hp - 10;

            //hpをSliderに反映
            slider.value = (float)hp / (float) hpMax;

            if(hp < 0) {hp = 0;}
            anim.SetInteger("hp", hp);
            anim.SetTrigger("damage");
            SoundPlay(seDamage);

            if(hp <= 0)
            {
                slider.gameObject.SetActive(false);
                SoundPlay(seDead);
            }
        }

        if(col.gameObject.tag == "Heal")
        {
            hp = hp + 50;
            //hpをSliderに反映
            slider.value = (float)hp / (float) hpMax;
        }
    }

    //HPバーを追従させたい
    void SliderMove()
    {
        slider.transform.position = new Vector3(gx - 0.3f, gy + 2f, slider.transform.position.z);
    }

    /*[SerializeField]
    GameObject target;                                      // ターゲット
    [SerializeField]
    float targetNearDist = 17f, targetFarDist = 100f;       // ターゲットとの距離(far以上ならば計算しない)
    [SerializeField]
    float camAdditionNearY = 1f, camAdditionFarY = -2f;  // カメラのY加算値(カメラが地面ギリギリだと見づらいので補正する)

    readonly float camSize = 29f;                           // ターゲットとの距離が最大の時のカメラサイズ
    readonly float camFarPosY = 24f;                        // ターゲットとの距離が最大の時のY座標

    // カメラコントローラー
    // 戻り値 true : カメラの範囲内
    bool CameraController()
    {
        // ターゲットとプレイヤーの中間を求める
        float center = (target.transform.position.x - transform.position.x) / 2f + transform.position.x;
        // カメラ座標を求めた中間座標にする
        cam.transform.position = new Vector3(center, cam.transform.position.y, cam.transform.position.z);
        // ターゲットとプレイヤーの距離を求める
        //float dist = Vector2.Distance(target.transform.position, transform.position); // こっちだとY 座標も計算するので大ジャンプで計算がおかしくなる
        float dist = Mathf.Abs(target.transform.position.x - transform.position.x); // 単純に x 座標だけで求める

        DebugPrint("dist:" + dist); // 距離を確認する

        // ターゲットとの距離が100以上なら計算しない
        if (dist > targetFarDist) { return false; }

        // ターゲットとの距離が17以下ならカメラのサイズを計算
        if (dist > targetNearDist)
        {
            // 17 の時 size = 7、100の時 size = 29になる式
            cam.orthographicSize = camSize * (dist / targetFarDist);
            //17以下のとき y = 0、100のとき y = 24になる式
            //float y = camFarPosY * ((dist - 17) / 83f);
            float rate = ((dist - targetNearDist) / (targetFarDist - targetNearDist));
            float y = camFarPosY * rate;

            // カメラが下すぎる時の補正値
            y = camAdditionNearY + rate * (camAdditionFarY - camAdditionNearY);

            //DebugPrint("dist:" + dist + "," + y);
            // カメラ位置
            cam.transform.position = new Vector3(cam.transform.position.x, y, cam.transform.position.z);
        }
        return true;
    }


    // 振動化開始
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }

    // 振動は非同期で行う
    IEnumerator DoShake(float duration, float magnitude)
    {
        var pos = transform.localPosition;

        var elapsed = 0f;

        while (elapsed < duration)
        {
            var x = pos.x + Random.Range(-1f, 1f) * magnitude;
            var y = pos.y + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, pos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }
        transform.localPosition = pos;
    }*/

}
