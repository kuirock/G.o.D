using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Effekseer;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class Player : _Base
{
    [SerializeField]//@@
    int id = 0;                         // 0 : 剣士、1 : 魔法使い

    PlayerInput1 playerInput1_;         // InputSystemで操作

    [SerializeField]
    public AudioClip seSlash1, seSlash2, seSlash3, seMagicS,seMagicL, seJump;  //サウンド

    [SerializeField]
    float speed = 3.0f;                 // 移動速度
    [SerializeField]
    float dashSpeed = 4f;               // ダッシュの変数宣言
    //現在のスピードを保持しておく変数
    //float currentSpeed;

    float dir = 1;                //向き(1:右, -1:左)
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

    /*[SerializeField]
    private GameObject magic;           //魔法プレハブを格納*/
    [SerializeField]
    private Transform magicPoint;       //アタックポイントを格納

    [SerializeField]
    private float attackTime = 0.3f;   //攻撃の間隔
    private float currentAttackTime;   //攻撃の間隔を管理
    private bool canAttack;            //攻撃可能状態かを指定するフラグ

    [SerializeField]
    private GameObject magicS, magicM, magicL;           // P2用 魔法プレハブを格納
    enum MgcMode { PowerS, PowerM, PowerL };  // P2 の魔法用
    MgcMode mode = MgcMode.PowerS;

    public Slider slider;              //スライダー

    /*private bool isShield = false;
    private float pressDuration = 0.5f; // 長押しの時間（秒）
    private float pressTime = 0f;*/

    //エフェクト
    EffekseerEffectAsset[] effect = null;
    readonly Vector2 hitPos = new Vector2(0.66f, 0.595f);


    // 
    Vector2 _moveInputValue;
    int deviceId = 0;                   // コントローラデバイスID保存用


    //リスタート
    public override void Restart()
    {
        // 子のトランスフォームを得る
        GameObject gc = transform.GetChild(0).gameObject;
        anim = gc.GetComponent<Animator>();

        // HPバー
        slider.value = 1;                       // スライダーを最大にする
        slider.gameObject.SetActive(true);
        hp = hpMax;

        isRight = true;                         // 右向き

        currentAttackTime = attackTime;         // currentAttackTimeにattackTimeをセット。

        // InputSystem
        playerInput1_ = new PlayerInput1();     // コントローラーで操作する用
        // 移動
        playerInput1_.Player.Move.started += OnMove;
        playerInput1_.Player.Move.performed += OnMove;
        playerInput1_.Player.Move.canceled += OnMove;
        // ジャンプ
        playerInput1_.Player.Jump.started += OnJump;

        // 剣攻撃
        playerInput1_.Player.Slash.started += OnAttack;
        /*// 剣・上攻撃
        playerInput1_.Player.SlashOver.started += OnAttack;*/

        // 魔法
        if (id == 1)
        {
            playerInput1_.Player.Magic.started += OnMagicCharge;
            //playerInput1_.Player.Magic.performed += OnMagicCharge;
            playerInput1_.Player.Magic.canceled += OnMagicFiring;
        }

        playerInput1_.Enable();

        //エフェクトを取得する
        if (effect == null)
        {
            effect = new EffekseerEffectAsset[1];
            effect[0] = Resources.Load<EffekseerEffectAsset>("effect");
            //effect[1] = Resources.Load<EffekseerEffectAsset>("suka");
        }
    }


    void Update()
    {
        // コントローラーチェック、1P だけはコントローラーなしで動く
        if(id != 0 && id >= Gamepad.all.Count){ return; }
        deviceId = Gamepad.all[id].deviceId;    // deviceId にコントローラーIDが入る

        //Attack();
        //Magic();
        Shield();
        ChangeState();
        ChangeAnimation();
        SliderMove();
    }

    void FixedUpdate()
    {
        Move();                                 // 移動
    }

    void Move()
    {
        float x = _moveInputValue.x;            // ここを変更 InputSyste の値を得る
        if(x != 0)
        {   // 移動、左右どちらも処理する
            dir = x > 0 ? 1: -1;                // 向き
            rb.velocity = new Vector3(speed * x, rb.velocity.y, 0);
            blendRate += changeRate;            // アニメーションブレンド
            if(blendRate > 1){ blendRate = 1;}
            anim.SetFloat("speed", blendRate);
        }
        else
        {   // 停止、アニメーションを徐々に IDLE へ
            blendRate -= changeRate;
            if(blendRate < 0){blendRate = 0;}
            anim.SetFloat("speed", blendRate);
        }
        // 向き
        Vector2 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
    }

    // 移動
    void OnMove(InputAction.CallbackContext context)
    {
        // コントローラー ID チェック
        if (deviceId != context.control.device.deviceId) { return; }

        if (isAtkMotion) { return; }            // 攻撃中ならば移動できない
        _moveInputValue = context.ReadValue<Vector2>();
    }

    // ジャンプ
    void OnJump(InputAction.CallbackContext context)
    {
        // コントローラー ID チェック
        if (deviceId != context.control.device.deviceId) { return; }

        if (!isGround){ return;}
        rb.AddForce(transform.up * jumpForce);
        isGround = false;
        SoundPlay(seJump);
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
    void OnAttack(InputAction.CallbackContext context)
    {
        // コントローラー ID チェック
        if (deviceId != context.control.device.deviceId) { return; }

        //通常・空中攻撃
        switch (atkMode)
        {
            //Attack 1
            case 0:
               //playerInput1_.Player.Slash.started += OnAttack;

                    anim.SetTrigger("attack1");
                    atkMode++;
                    SoundPlay(seSlash1);
                break;
            //Attack 2
            case 1:
               //playerInput1_.Player.Slash.started += OnAttack;

                    anim.SetTrigger("attack1");
                    atkMode++;
                    SoundPlay(seSlash2);
                break;
            //Attack 3
            case 2:
               //playerInput1_.Player.Slash.started += OnAttack;
                
                    anim.SetTrigger("attack1");
                    atkMode = 0;
                    SoundPlay(seSlash3);
                break;
        }

        /*//上攻撃  
        anim.SetTrigger("attack2");
        SoundPlay(seSlash1);*/
    }

    /*void Magic()
    {
        if(isAtkMotion){return;}                 //攻撃中ならば攻撃できない

        attackTime += Time.deltaTime;            //attackTimeに毎フレームの時間を加算していく

        if(attackTime > currentAttackTime) 
        {
            canAttack = true;                    //指定時間を超えたら攻撃可能にする
        }

        if (canAttack)
        {                
            //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
            Instantiate(magic, magicPoint.position, transform.rotation);
            canAttack = false;　              //攻撃フラグをfalseにする
            attackTime = 0f;　                //attackTimeを0に戻す
            anim.SetTrigger("magicFiring");
            SoundPlay(seMagicS);
         }
    }*/
    
    void OnMagicCharge(InputAction.CallbackContext context)
    {
        // コントローラー ID チェック
        if (deviceId != context.control.device.deviceId) { return; }

        anim.SetBool("mgcCharge", true);
        //rb.bodyType = RigidbodyType2D.Static;
    }

    void OnMagicFiring(InputAction.CallbackContext context)
    {

        // コントローラー ID チェック
        //if (deviceId != context.control.device.deviceId) { return; }

        anim.SetBool("mgcCharge", false);
        //rb.bodyType = RigidbodyType2D.Dynamic;

        //switch (mode)
        switch (magic)
        {
            //            case MgcMode.PowerS:
            case 0:
                MagicS();
                canAttack = false;               //攻撃フラグをfalseにする
                attackTime = 0f;                 //attackTimeを0に戻す
                break;
            //            case MgcMode.PowerM:
            case 1:
                MagicM();
                canAttack = false;               //攻撃フラグをfalseにする
                attackTime = 0f;                 //attackTimeを0に戻す
                break;
            //case MgcMode.PowerL:
            case 2:
                MagicL();
                canAttack = false;               //攻撃フラグをfalseにする
                attackTime = 0f;                 //attackTimeを0に戻す
                break;
        }

        magic = 0;
    }

    int magic = 0;
    //魔法チャージのイベント
    public void MagicChargeEvent(int num)
    {

        //溜め段階
        switch (num)
        {
            //溜め開始　sizeS
            case 0:
                magic = -1;
                mode = MgcMode.PowerS;
                break;
            //sizeM
            case 1:
                magic = 1;
                mode = MgcMode.PowerM;
                break;
            //sizeL
            case 2:
                magic = 2;
                mode = MgcMode.PowerL;
                break;
        }
    }

    void MagicS()
    {
        //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
        Instantiate(magicS, magicPoint.position, transform.rotation);
        anim.SetTrigger("magicFiring");
        SoundPlay(seMagicS);
    }

    void MagicM()
    {
        //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
        Instantiate(magicM, magicPoint.position, transform.rotation);
        anim.SetTrigger("magicFiring");
        SoundPlay(seMagicS);
    }

    void MagicL()
    {
        //第一引数に生成するオブジェクト、第二引数にVector3型の座標、第三引数に回転の情報
        Instantiate(magicL, magicPoint.position, transform.rotation);
        anim.SetTrigger("magicFiring");
        SoundPlay(seMagicL);
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
