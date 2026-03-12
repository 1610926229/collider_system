using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace CollisionSystem
{
    /// <summary>
    /// 性能统计类
    /// 用于存储和管理碰撞检测系统的性能统计数据
    /// </summary>
    [System.Serializable]
    public class PerformanceStats
    {
        #region 时间统计
        /// <summary>
        /// Broadphase构建时间（毫秒）
        /// </summary>
        public float BroadphaseBuildTime = 0f;
        
        /// <summary>
        /// Broadphase查询时间（毫秒）
        /// </summary>
        public float BroadphaseQueryTime = 0f;
        
        /// <summary>
        /// Midphase构建时间（毫秒）
        /// </summary>
        public float MidphaseBuildTime = 0f;
        
        /// <summary>
        /// Midphase查询时间（毫秒）
        /// </summary>
        public float MidphaseQueryTime = 0f;
        
        /// <summary>
        /// Narrowphase检测时间（毫秒）
        /// </summary>
        public float NarrowphaseDetectionTime = 0f;
        
        /// <summary>
        /// 空间结构重构时间（毫秒）
        /// </summary>
        public float SpatialStructureRebuildTime = 0f;
        
        /// <summary>
        /// 总碰撞检测时间（毫秒）
        /// </summary>
        public float TotalCollisionDetectionTime = 0f;
        
        /// <summary>
        /// 总空间结构构建时间（毫秒）
        /// </summary>
        public float TotalSpatialStructureBuildTime = 0f;
        #endregion
        
        #region 数量统计
        /// <summary>
        /// 静态碰撞体数量
        /// </summary>
        public int StaticShapeCount = 0;
        
        /// <summary>
        /// 动态碰撞体数量
        /// </summary>
        public int DynamicShapeCount = 0;
        
        /// <summary>
        /// Broadphase阶段检测到的候选对数量
        /// </summary>
        public int BroadphaseCandidateCount = 0;
        
        /// <summary>
        /// Midphase阶段检测到的候选对数量
        /// </summary>
        public int MidphaseCandidateCount = 0;
        
        /// <summary>
        /// Narrowphase阶段检测到的碰撞对数量
        /// </summary>
        public int NarrowphaseCollisionCount = 0;
        
        /// <summary>
        /// Narrowphase阶段检测的总对数
        /// </summary>
        public int NarrowphaseTestCount = 0;
        
        /// <summary>
        /// 碰撞检测的成功率（Narrowphase碰撞数 / Narrowphase测试数）
        /// </summary>
        public float CollisionSuccessRate = 0f;
        #endregion
        
        #region 空间结构统计
        /// <summary>
        /// Octree节点数量
        /// </summary>
        public int OctreeNodeCount = 0;
        
        /// <summary>
        /// Octree叶子节点数量
        /// </summary>
        public int OctreeLeafNodeCount = 0;
        
        /// <summary>
        /// BVH节点数量
        /// </summary>
        public int BVHNodeCount = 0;
        
        /// <summary>
        /// BVH叶子节点数量
        /// </summary>
        public int BVHLeafNodeCount = 0;
        #endregion
        
        #region 方法
        /// <summary>
        /// 重置所有性能统计数据
        /// </summary>
        public void Reset()
        {
            // 重置时间统计
            BroadphaseBuildTime = 0f;
            BroadphaseQueryTime = 0f;
            MidphaseBuildTime = 0f;
            MidphaseQueryTime = 0f;
            NarrowphaseDetectionTime = 0f;
            SpatialStructureRebuildTime = 0f;
            TotalCollisionDetectionTime = 0f;
            TotalSpatialStructureBuildTime = 0f;
            
            // 重置数量统计
            StaticShapeCount = 0;
            DynamicShapeCount = 0;
            BroadphaseCandidateCount = 0;
            MidphaseCandidateCount = 0;
            NarrowphaseCollisionCount = 0;
            NarrowphaseTestCount = 0;
            CollisionSuccessRate = 0f;
            
            // 重置空间结构统计
            OctreeNodeCount = 0;
            OctreeLeafNodeCount = 0;
            BVHNodeCount = 0;
            BVHLeafNodeCount = 0;
        }
        
        /// <summary>
        /// 更新碰撞成功率
        /// </summary>
        public void UpdateCollisionSuccessRate()
        {
            if (NarrowphaseTestCount > 0)
            {
                CollisionSuccessRate = (float)NarrowphaseCollisionCount / NarrowphaseTestCount;
            }
            else
            {
                CollisionSuccessRate = 0f;
            }
        }
        
        /// <summary>
        /// 获取性能统计信息的字符串表示
        /// </summary>
        public string GetStatsString()
        {
            StringBuilder sb = new StringBuilder();//
            
            sb.AppendLine("=== 碰撞检测系统性能统计 ===");
            
            // 碰撞体数量
            sb.AppendLine($"静态碰撞体数量: {StaticShapeCount}");
            sb.AppendLine($"动态碰撞体数量: {DynamicShapeCount}");
            sb.AppendLine($"总碰撞体数量: {StaticShapeCount + DynamicShapeCount}");
            sb.AppendLine();
            
            // 时间统计
            sb.AppendLine("=== 时间统计 (毫秒) ===");
            sb.AppendLine($"空间结构构建总时间: {TotalSpatialStructureBuildTime:F3}");
            sb.AppendLine($"  Broadphase构建时间: {BroadphaseBuildTime:F3}");
            sb.AppendLine($"  Midphase构建时间: {MidphaseBuildTime:F3}");
            sb.AppendLine($"碰撞检测总时间: {TotalCollisionDetectionTime:F3}");
            sb.AppendLine($"  Broadphase查询时间: {BroadphaseQueryTime:F3}");
            sb.AppendLine($"  Midphase查询时间: {MidphaseQueryTime:F3}");
            sb.AppendLine($"  Narrowphase检测时间: {NarrowphaseDetectionTime:F3}");
            sb.AppendLine($"动态物体重构时间: {SpatialStructureRebuildTime:F3}");
            sb.AppendLine();
            
            // 碰撞检测数量统计
            sb.AppendLine("=== 碰撞检测数量统计 ===");
            sb.AppendLine($"Broadphase候选对数量: {BroadphaseCandidateCount}");
            sb.AppendLine($"Midphase候选对数量: {MidphaseCandidateCount}");
            sb.AppendLine($"Narrowphase测试数量: {NarrowphaseTestCount}");
            sb.AppendLine($"Narrowphase碰撞数量: {NarrowphaseCollisionCount}");
            sb.AppendLine($"碰撞成功率: {CollisionSuccessRate:P2}");
            sb.AppendLine();
            
            // 空间结构统计
            sb.AppendLine("=== 空间结构统计 ===");
            sb.AppendLine($"Octree节点数量: {OctreeNodeCount}");
            sb.AppendLine($"Octree叶子节点数量: {OctreeLeafNodeCount}");
            sb.AppendLine($"BVH节点数量: {BVHNodeCount}");
            sb.AppendLine($"BVH叶子节点数量: {BVHLeafNodeCount}");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 打印性能统计信息到控制台
        /// </summary>
        public void PrintStats()
        {
            UnityEngine.Debug.Log(GetStatsString());
        }
        #endregion
    }
}
