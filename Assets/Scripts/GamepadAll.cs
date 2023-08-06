using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamepadAll : MonoBehaviour
{
    /// <summary>情報を表示させるテキストオブジェクト一覧。</summary>
    [SerializeField] private Text[] TextObjects;

    StringBuilder Builder = new StringBuilder();
    StringBuilder BuilderButton = new StringBuilder();

    // 更新はフレームごとに1回呼び出されます
    void Update()
    {
        if (TextObjects == null)
        {
            Debug.Log($"{nameof(TextObjects)} が null です。");
            return;
        }

        // Gamepad.all で接続されているすべてのゲームパッドを列挙できる
        // TextObjects の数以上の情報は載せられないので、少ない方の数で for する
        for (int i = 0; i < Gamepad.all.Count || i < TextObjects.Length; i++)
        {
            var gamepad = Gamepad.all[i];
            var textObject = TextObjects[i];

            Builder.Clear();
            BuilderButton.Clear();

            Builder.AppendLine($"deviceId:{gamepad.deviceId}");
            Builder.AppendLine($"name:{gamepad.name}");

            // 操作されたボタンなどの情報を取得
            var leftStickValue = gamepad.leftStick.ReadValue();
            var rightStickValue = gamepad.rightStick.ReadValue();
            var dpadValue = gamepad.dpad.ReadValue();

            if (leftStickValue.magnitude > 0f) Builder.AppendLine($"LeftStick:{leftStickValue.normalized * leftStickValue.magnitude}");
            if (rightStickValue.magnitude > 0f) Builder.AppendLine($"RightStick:{rightStickValue.normalized * rightStickValue.magnitude}");
            if (dpadValue.magnitude > 0f) Builder.AppendLine($"Dpad:{dpadValue.normalized * dpadValue.magnitude}");

            if (gamepad.aButton.isPressed) BuilderButton.Append($"A ");
            if (gamepad.bButton.isPressed) BuilderButton.Append($"B ");
            if (gamepad.xButton.isPressed) BuilderButton.Append($"X ");
            if (gamepad.yButton.isPressed) BuilderButton.Append($"Y ");

            if (gamepad.startButton.isPressed) BuilderButton.Append($"Start ");
            if (gamepad.selectButton.isPressed) BuilderButton.Append($"Select ");

            if (gamepad.leftStickButton.isPressed) BuilderButton.Append($"LeftStickButton ");
            if (gamepad.rightStickButton.isPressed) BuilderButton.Append($"RightStickButton ");

            if (gamepad.leftShoulder.isPressed) BuilderButton.Append($"LeftShoulder ");
            if (gamepad.rightShoulder.isPressed) BuilderButton.Append($"RightShoulder ");

            if (BuilderButton.Length >= 1) Builder.AppendLine(BuilderButton.ToString());

            var leftTriggerValue = gamepad.leftTrigger.ReadValue();
            var rightTriggerValue = gamepad.rightTrigger.ReadValue();

            if (leftTriggerValue > 0 || rightTriggerValue > 0)
            {
                Builder.AppendLine($"Trigger:({leftTriggerValue:f2}, {rightTriggerValue:f2})");
            }

            // 取得した情報を表示
            textObject.text = Builder.ToString();
        }
    }
}

