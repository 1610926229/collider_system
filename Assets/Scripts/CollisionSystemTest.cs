using UnityEngine;
using CollisionSystem;
using CollisionSystem.Shape;
using CollisionSystem.Narrowphase;

public class CollisionSystemTest : MonoBehaviour
{
    void Start()
    {
        // 测试创建碰撞形状
        SphereShape sphere = new SphereShape(Vector3.zero, Quaternion.identity, 1.0f);
        BoxShape box = new BoxShape(Vector3.one, Quaternion.identity, new Vector3(1, 1, 1));
        
        Debug.Log("创建SphereShape成功: " + sphere);
        Debug.Log("创建BoxShape成功: " + box);
        
        // 测试碰撞检测
        CollisionDetector detector = new CollisionDetector();
        CollisionContact result;
        bool colliding = detector.DetectCollision(sphere, box, out result);
        
        Debug.Log("碰撞检测结果: " + colliding);
        if (colliding)
        {
            Debug.Log("接触点A: " + result.PointA);
            Debug.Log("接触点B: " + result.PointB);
            Debug.Log("法线: " + result.Normal);
        }
    }
}