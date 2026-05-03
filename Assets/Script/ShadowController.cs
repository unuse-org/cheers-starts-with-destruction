using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class ShadowController : MonoBehaviour
{
  
  [SerializeField] private Transform Light ; // 光源のTransformを入れる変数
  [SerializeField] private Transform ShadowObject ; // 影用オブジェクトのTransformを入れる変数
  [SerializeField] private LayerMask groundLayer ; // 床のレイヤー
    
  void Update()
{
    // 方向 = 自分の位置 - 光源の位置
    Vector3 direction = transform.position - Light.position;

    // 衝突情報を入れる箱を用意
    RaycastHit hitInfo;
    
    // 自分から影の方向にRayを飛ばす
    // もし何かに当たったら、If文の中を実行
    if(Physics.Raycast(transform.position, direction, out hitInfo, Mathf.Infinity, groundLayer))
    {
      ShadowObject.position = hitInfo.point;
      //影の向きを床の角度に合わせる
      ShadowObject.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

      // 【開発用】自分からヒットした場所まで赤い線を引く（Game画面には映りません）
      Debug.DrawLine(Light.position, hitInfo.point, Color.red);
      
    }

}
}
