using UnityEngine;
using UnityEngine.InputSystem;


public class GamepadTest : MonoBehaviour
{
    /*//private PlayerInput1 playerInput1_;

    // 入力を受け取るPlayerInput
    [SerializeField] private PlayerInput _playerInput;

    // 攻撃アクション名
    [SerializeField] private string _fireActionName = "Fire";

    // 攻撃アクション
    private InputAction _fireAction;
    
    void Start()
    {
        playerInput1_ = new PlayerInput1();
        playerInput1_.Enable();
    }

     private void Awake()
    {
        // 攻撃アクションをPlayerInputから取得
        _fireAction = _playerInput.actions[_fireActionName];
    }

    void Update()
    {
        if (_fireAction == null) return;

        // 攻撃ボタンの押下状態取得
        var isPressed = _fireAction.IsPressed();

        // ボタンの押下状態をログ出力
        print($"[{_fireActionName}] isPressed = {isPressed}");
    }*/

    // 入力を受け取るPlayerInput
    [SerializeField] private PlayerInput _playerInput;

    // 攻撃アクション名
    [SerializeField] private string _fireActionName = "Fire";

    // 攻撃アクション
    private InputAction _fireAction;

    private void Awake()
    {
        // 攻撃アクションをPlayerInputから取得
        _fireAction = _playerInput.actions[_fireActionName];
    }

    private void Update()
    {
        if (_fireAction == null) return;

        // 攻撃ボタンの押下状態取得
        var isPressed = _fireAction.IsPressed();

        // ボタンの押下状態をログ出力
        print($"[{_fireActionName}] isPressed = {isPressed}");
    }
}
