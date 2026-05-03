using UnityEngine;

// このスクリプトを追加すると、自動的にCharacterControllerも追加されます
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. 接地判定（地面にいるか？）
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f; // 地面にいたら落下速度をリセット
        }

        // 2. キー入力の取得 (WASD / 矢印キー)
        // Horizontal: A/D キー, Vertical: W/S キー
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // 3. 移動方向の決定
        // カメラの向きに関係なく、ワールド座標基準で動きます
        Vector3 move = new Vector3(moveX, 0, moveZ);
        
        // 移動実行
        controller.Move(move * Time.deltaTime * moveSpeed);

        // キャラクターを進行方向に向ける
        if (move != Vector3.zero)
        {
            transform.forward = move;
        }

        // 4. ジャンプ処理
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        // 5. 重力処理
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}