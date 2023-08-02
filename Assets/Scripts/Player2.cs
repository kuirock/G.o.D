using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Effekseer;

public class Player2 : _Base
{
    private PlayerInput2 playerInput2_;　//InputSystemで操作

    [SerializeField]
    public AudioClip  seMagicS, seMagicL, seJump;  //サウンド

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

    [SerializeField]
    private GameObject magicS, magicM, magicL;           //魔法プレハブを格納
    [SerializeField]
    private Transform magicPoint;       //アタックポイントを格納

    enum MgcMode{PowerS, PowerM, PowerL};
    MgcMode mode = MgcMode.PowerS;


    [SerializeField]
    private float attackTime = 0.3f;   //攻撃の間隔
    private float currentAttackTime;   //攻撃の間隔を管理
    private bool canAttack;            //攻撃可能状態かを指定するフラグ


    public Slider slider;              //スライダー

    /*//エフェクト
    EffekseerEffectAsset[] effect = null;
    readonly Vector2 handPos = new Vector2(1.77f, 1.57f);
    */
    
     //リスタート
    public override void Restart()
    {

        slider.value = 1;               //スライダーを最大にする

        slider.gameObject.SetActive(true);

        hp = hpMax;

        isRight = true;                 //右向き

        currentSpeed = speed;           //現在のスピードをスピードに設定

        currentAttackTime = attackTime; //currentAttackTimeにattackTimeをセット。

        playerInput2_ = new PlayerInput2();　　//コントローラーで操作する用
        playerInput2_.Enable();

        /*//エフェクトを取得する
        if(effect == null)
        {
            effect = new EffekseerEffectAsset[2];
            effect[0] = Resources.Load<EffekseerEffectAsset>("Attack");
            effect[1] = Resources.Load<EffekseerEffectAsset>("suka");
        }
        */
    }

    void Update()
    {
        // DebugPrint("HP : " + hp);

        Move();
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
            if(playerInput2_.Player.Dash.ReadValue<float>() != 0f)
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
            if(playerInput2_.Player.Dash.ReadValue<float>() != 0f)
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
            if(playerInput2_.Player.Jump.triggered)
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

    void Magic()
    {
        attackTime += Time.deltaTime;            //attackTimeに毎フレームの時間を加算していく

        if(attackTime > currentAttackTime) 
        {
            canAttack = true;                    //指定時間を超えたら攻撃可能にする
        }

        if (Input.GetKey(KeyCode.Z))
        {
            anim.SetBool("mgcCharge", true);
            rb.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            anim.SetBool("mgcCharge", false);
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        switch(mode)
        {
            case MgcMode.PowerS:
                PowerS();
                break;
            case MgcMode.PowerM:
                PowerM();
                break;
            case MgcMode.PowerL:
                PowerL();
                break;
        }
    }

    public void MagicChargeEvent(int num)
    {
        //溜め段階
        switch(num)
        {
            //溜め開始　sizeS
            case 0:
                mode = MgcMode.PowerS;
                break;
            //sizeM
            case 1:
                mode = MgcMode.PowerM;
                break;
            //sizeL
            case 2:
                mode = MgcMode.PowerL;
                break;
        }
    }

    void PowerS()
    {
        if(Input.GetKeyUp(KeyCode.Z))
        {
            if (canAttack)
            {                
                //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
                Instantiate(magicS, magicPoint.position, transform.rotation);
                canAttack = false;　              //攻撃フラグをfalseにする
                attackTime = 0f;　                //attackTimeを0に戻す
                anim.SetTrigger("magicFiring");
                SoundPlay(seMagicS);
            }
        }
    }

    void PowerM()
    {
        if(Input.GetKeyUp(KeyCode.Z))
        {
            if (canAttack)
            {                
                //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
                Instantiate(magicM, magicPoint.position, transform.rotation);
                canAttack = false;　              //攻撃フラグをfalseにする
                attackTime = 0f;　                //attackTimeを0に戻す
                anim.SetTrigger("magicFiring");
                SoundPlay(seMagicS);
            }
        }
    }

    void PowerL()
    {
        if(Input.GetKeyUp(KeyCode.Z))
        {
            if (canAttack)
            {                
                //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
                Instantiate(magicL, magicPoint.position, transform.rotation);
                canAttack = false;　              //攻撃フラグをfalseにする
                attackTime = 0f;　                //attackTimeを0に戻す
                anim.SetTrigger("magicFiring");
                SoundPlay(seMagicL);
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
                break;
        }
    }


    void Shield()
    {
        //防御
        if(playerInput2_.Player.Shield.ReadValue<float>() > 0f)
        {
            anim.SetBool("shield", true);
        }
        else
        {
            anim.SetBool("shield", false);
        }

        /*//ジャストガード
        if(Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.X))
        {
            anim.SetTrigger("justGuard");
        }*/
    }

    /*//エフェクト再生
    public void StartEffect()
    {
        EffekseerHandle handle = EffekseerSystem.PlayEffect(effect[0], xy + new Vector2(handPos.x * dir, handPos.y));

        //左向きはスケールを設定する
        handle.SetScale(new Vector3(dir, 1, 1));
    }
    */

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
}
