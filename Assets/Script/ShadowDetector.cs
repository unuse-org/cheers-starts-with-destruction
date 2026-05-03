using UnityEngine;

public class ShadowDetector : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("シーン上の光源（Directional Lightなど）をここにドラッグ&ドロップ")]
    [SerializeField] private Transform lightSource; 
    
    [Tooltip("影を作る障害物として認識するレイヤー")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("レイを発射する位置のオフセット（足元ではなく胸や頭から飛ばすため）")]
    [SerializeField] private Vector3 rayOffset = new Vector3(0, 1.0f, 0);

    // 外部から現在の状態を取得するためのプロパティ
    public bool IsInShadow { get; private set; }

    void Update()
    {
        CheckShadow();
    }

    void CheckShadow()
    {
        // 1. レイの発射位置（プレイヤーの座標 + 高さ調整）
        Vector3 rayOrigin = transform.position + rayOffset;

        // 2. 光源への方向ベクトル（光源の位置 - 発射位置）
        Vector3 directionToLight = lightSource.position - rayOrigin;
        
        // 光源までの距離（Point Lightなどの場合、これ以上遠くは判定しないため）
        float distance = directionToLight.magnitude;

        // 3. レイキャスト実行
        // (発射位置, 方向, 当たった情報, 最大距離, 対象レイヤー)
        if (Physics.Raycast(rayOrigin, directionToLight, out RaycastHit hit, distance, obstacleLayer))
        {
            // 【影の中】障害物に当たった = 光が遮られている
            IsInShadow = true;

            // デバッグ用：障害物まで「青い線」を表示
            Debug.DrawRay(rayOrigin, directionToLight.normalized * hit.distance, Color.blue);
            // Debug.Log($"影の中です（遮蔽物: {hit.collider.name}）");
        }
        else
        {
            // 【光の中】障害物に当たらず光源まで届いた
            IsInShadow = false;

            // デバッグ用：光源まで「黄色い線」を表示
            Debug.DrawRay(rayOrigin, directionToLight, Color.yellow);
        }
    }
}