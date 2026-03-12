using UnityEngine;

/// <summary>
/// 简单的碰撞系统测试脚本
/// 直接挂载到CollisionSystem对象上使用
/// </summary>
[RequireComponent(typeof(CollisionSystem.CollisionSystem))]
public class SimpleTest : MonoBehaviour
{
    private CollisionSystem.CollisionSystem _collisionSystem;
    
    void Start()
    {
        // 获取碰撞系统组件
        _collisionSystem = GetComponent<CollisionSystem.CollisionSystem>();
        
        // 打印测试信息
        Debug.Log("=== 碰撞系统修复测试开始 ===");
        
        // 等待一帧让所有物体初始化完成
        Invoke("RunTest", 0.1f);
    }
    
    void RunTest()
    {
        // 执行碰撞检测
        Debug.Log("执行碰撞检测...");
        _collisionSystem.DetectCollisions();
        
        // 获取性能统计数据
        CollisionSystem.PerformanceStats stats = _collisionSystem.GetPerformanceStats();
        
        // 打印关键统计信息
        Debug.Log("\n=== 碰撞检测结果统计 ===");
        Debug.Log($"静态碰撞体数量: {stats.StaticShapeCount}");
        Debug.Log($"动态碰撞体数量: {stats.DynamicShapeCount}");
        Debug.Log($"Broadphase候选对: {stats.BroadphaseCandidateCount}");
        Debug.Log($"Midphase候选对: {stats.MidphaseCandidateCount}");
        Debug.Log($"Narrowphase测试: {stats.NarrowphaseTestCount}");
        Debug.Log($"Narrowphase碰撞: {stats.NarrowphaseCollisionCount}");
        Debug.Log($"碰撞成功率: {stats.CollisionSuccessRate:P2}");
        
        // 检查碰撞成功率是否大于0
        if (stats.CollisionSuccessRate > 0)
        {
            Debug.Log("\n✅ 修复成功！碰撞成功率大于0%");
        }
        else
        {
            Debug.Log("\n❌ 修复失败！碰撞成功率仍然为0%");
        }
        
        Debug.Log("\n=== 碰撞系统修复测试结束 ===");
    }
}