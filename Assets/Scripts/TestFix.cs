using UnityEngine;

public class TestFix : MonoBehaviour
{
    void Start()
    {
        // 确保在场景加载后等待几帧，让所有物体都被正确创建和注册
        Invoke("TestCollisionSystem", 1.0f);
    }
    
    void TestCollisionSystem()
    {
        // 获取碰撞系统实例
        CollisionSystem.CollisionSystem collisionSystem = CollisionSystem.CollisionSystem.Instance;
        
        // 获取性能统计数据
        CollisionSystem.PerformanceStats stats = collisionSystem.GetPerformanceStats();
        
        // 打印碰撞体数量
        Debug.Log("=== 碰撞体数量测试 ===");
        Debug.Log($"静态碰撞体数量: {stats.StaticShapeCount}");
        Debug.Log($"动态碰撞体数量: {stats.DynamicShapeCount}");
        Debug.Log($"总碰撞体数量: {stats.StaticShapeCount + stats.DynamicShapeCount}");
        
        // 手动执行一次碰撞检测
        Debug.Log("\n=== 执行碰撞检测 ===");
        collisionSystem.DetectCollisions();
        
        // 再次获取性能统计数据
        stats = collisionSystem.GetPerformanceStats();
        
        // 打印碰撞检测结果
        Debug.Log("\n=== 碰撞检测结果 ===");
        Debug.Log($"Broadphase候选对数量: {stats.BroadphaseCandidateCount}");
        Debug.Log($"Midphase候选对数量: {stats.MidphaseCandidateCount}");
        Debug.Log($"Narrowphase测试数量: {stats.NarrowphaseTestCount}");
        Debug.Log($"Narrowphase碰撞数量: {stats.NarrowphaseCollisionCount}");
        Debug.Log($"碰撞成功率: {stats.CollisionSuccessRate:P2}");
        
        // 打印空间结构信息
        Debug.Log("\n=== 空间结构信息 ===");
        Debug.Log($"Octree节点数量: {stats.OctreeNodeCount}");
        Debug.Log($"BVH节点数量: {stats.BVHNodeCount}");
    }
}