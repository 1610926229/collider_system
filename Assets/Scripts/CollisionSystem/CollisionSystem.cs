using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using CollisionSystem.Debug;
namespace CollisionSystem
{
    /// <summary>
    /// 碰撞检测系统主类
    /// 管理Broadphase、Midphase和Narrowphase三个阶段的碰撞检测
    /// </summary>
    public class CollisionSystem : MonoBehaviour
    {
        #region 单例模式
        private static CollisionSystem _instance;
        public static CollisionSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CollisionSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CollisionSystem");
                        _instance = go.AddComponent<CollisionSystem>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region 配置参数
        [Header("碰撞系统配置")]
        [Tooltip("是否启用调试可视化")]
        public bool enableDebug = true;
        
        [Tooltip("八叉树最大深度")]
        public int octreeMaxDepth = 8;
        
        [Tooltip("八叉树单元格最大物体数")]
        public int octreeMaxObjectsPerCell = 10;
        
        [Tooltip("是否对动态物体每帧重构空间结构")]
        public bool rebuildDynamicObjectsEachFrame = true;
        
        [Tooltip("是否打印性能统计信息到控制台")]
        public bool printPerformanceStats = false;
        #endregion

        #region 内部组件
        private Broadphase.Octree _octree;
        private Midphase.BVH _bvh;
        private Narrowphase.CollisionDetector _collisionDetector;
        #endregion

        #region 数据存储
        private List<Shape.CollisionShape> _staticShapes = new List<Shape.CollisionShape>();
        private List<Shape.CollisionShape> _dynamicShapes = new List<Shape.CollisionShape>();
        private List<Shape.ShapePair> _broadphaseResults = new List<Shape.ShapePair>();
        private List<Shape.ShapePair> _midphaseResults = new List<Shape.ShapePair>();
        private List<Narrowphase.CollisionContact> _narrowphaseResults = new List<Narrowphase.CollisionContact>();
        
        /// <summary>
        /// 性能统计数据
        /// </summary>
        private PerformanceStats _performanceStats = new PerformanceStats();
        #endregion

        #region 事件系统
        /// <summary>
        /// 碰撞检测事件委托
        /// </summary>
        public delegate void CollisionDetectedDelegate(Narrowphase.CollisionContact contact);
        
        /// <summary>
        /// 碰撞检测事件
        /// </summary>
        public event CollisionDetectedDelegate OnCollisionDetected;
        #endregion

        #region Unity生命周期
        private void Awake()
        {
            // 确保单例实例正确
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            // 初始化各阶段组件
            _octree = new Broadphase.Octree(Vector3.zero, 100f, octreeMaxDepth, octreeMaxObjectsPerCell);
            _bvh = new Midphase.BVH();
            _collisionDetector = new Narrowphase.CollisionDetector();
        }
        
        private void FixedUpdate()
        {
            // 每帧只进行查询，不进行构建
            DetectCollisions();
        }
        
        private void OnDrawGizmos()//绘制辅助信息
        {
            if (enableDebug)
            {
                // 绘制调试信息
                DebugVisualizer.DrawOctree(_octree);
                DebugVisualizer.DrawBVH(_bvh);
                DebugVisualizer.DrawCollisionContacts(_narrowphaseResults);
            }
        }
        
        private void OnDestroy()
        {
            // 清理单例实例引用，确保对象能被完全销毁
            if (_instance == this)
            {
                _instance = null;
            }
            
            // 清空事件订阅，避免内存泄漏
            OnCollisionDetected = null;
            
            // 清空数据存储
            _staticShapes.Clear();
            _dynamicShapes.Clear();
            _broadphaseResults.Clear();
            _midphaseResults.Clear();
            _narrowphaseResults.Clear();
            
            // 释放空间结构资源
            _octree?.Clear();
        }
        #endregion

        #region 公共API
        /// <summary>
        /// 构建空间结构（Broadphase的Octree与Midphase的BVH）
        /// </summary>
        [ContextMenu("构建空间结构")]
        public void BuildSpatialStructures()
        {
            Profiler.BeginSample("CollisionSystem.BuildSpatialStructures");
            
            // 重置性能统计
            _performanceStats.Reset();
            
            // 记录开始时间
            float startTime = Time.realtimeSinceStartup;
            
            // 清空现有结构
            _octree.Clear();
            _bvh.Clear();
            
            // 记录静态和动态物体数量
            _performanceStats.StaticShapeCount = _staticShapes.Count;
            _performanceStats.DynamicShapeCount = _dynamicShapes.Count;
            
            // 构建Broadphase（Octree）
            Profiler.BeginSample("CollisionSystem.BuildOctree");
            float broadphaseBuildStartTime = Time.realtimeSinceStartup;
            
            foreach (var shape in _staticShapes)
            {
                _octree.Insert(shape);
            }
            
            if (!rebuildDynamicObjectsEachFrame)
            {
                foreach (var shape in _dynamicShapes)
                {
                    _octree.Insert(shape);
                }
            }
            
            // 更新Broadphase构建时间
            _performanceStats.BroadphaseBuildTime = (Time.realtimeSinceStartup - broadphaseBuildStartTime) * 1000f;
            
            // 更新Octree统计信息
            _performanceStats.OctreeNodeCount = _octree.NodeCount;
            _performanceStats.OctreeLeafNodeCount = _octree.LeafNodeCount;
            
            Profiler.EndSample();
            
            // 构建Midphase（BVH）
            Profiler.BeginSample("CollisionSystem.BuildBVH");
            float midphaseBuildStartTime = Time.realtimeSinceStartup;
            
            List<Shape.CollisionShape> allShapes = new List<Shape.CollisionShape>();
            allShapes.AddRange(_staticShapes);
            allShapes.AddRange(_dynamicShapes);
            _bvh.Build(allShapes);
            
            // 更新Midphase构建时间
            _performanceStats.MidphaseBuildTime = (Time.realtimeSinceStartup - midphaseBuildStartTime) * 1000f;
            
            // 更新BVH统计信息
            _performanceStats.BVHNodeCount = _bvh.NodeCount;
            _performanceStats.BVHLeafNodeCount = _bvh.LeafNodeCount;
            
            Profiler.EndSample();
            
            // 更新总空间结构构建时间
            _performanceStats.TotalSpatialStructureBuildTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            
            Profiler.EndSample();
        }
        
        /// <summary>
        /// 检测碰撞
        /// </summary>
        public void DetectCollisions()
        {
            Profiler.BeginSample("CollisionSystem.DetectCollisions");
            float totalStartTime = Time.realtimeSinceStartup;
            
            // 清空上一帧的结果
            _broadphaseResults.Clear();
            _midphaseResults.Clear();
            _narrowphaseResults.Clear();
            
            // 如果需要，为动态物体重构空间结构
            if (rebuildDynamicObjectsEachFrame)
            {
                Profiler.BeginSample("CollisionSystem.RebuildDynamicObjects");
                float rebuildStartTime = Time.realtimeSinceStartup;
                
                // 移除所有动态物体
                foreach (var shape in _dynamicShapes)
                {
                    _octree.Remove(shape);
                }
                
                // 重新插入动态物体
                foreach (var shape in _dynamicShapes)
                {
                    _octree.Insert(shape);
                }
                
                // 更新BVH结构，因为动态物体的位置已经改变
                List<Shape.CollisionShape> allShapes = new List<Shape.CollisionShape>();
                allShapes.AddRange(_staticShapes);
                allShapes.AddRange(_dynamicShapes);
                _bvh.Build(allShapes);
                
                // 更新动态物体重构时间
                _performanceStats.SpatialStructureRebuildTime = (Time.realtimeSinceStartup - rebuildStartTime) * 1000f;
                
                // 更新BVH统计信息
                _performanceStats.BVHNodeCount = _bvh.NodeCount;
                _performanceStats.BVHLeafNodeCount = _bvh.LeafNodeCount;
                
                Profiler.EndSample();
            }
            
            // Broadphase：使用Octree获取候选对
            Profiler.BeginSample("CollisionSystem.Broadphase");
            float broadphaseQueryStartTime = Time.realtimeSinceStartup;
            
            _octree.QueryPotentialPairs(_broadphaseResults);
            
            // 更新Broadphase查询时间和候选对数量
            _performanceStats.BroadphaseQueryTime = (Time.realtimeSinceStartup - broadphaseQueryStartTime) * 1000f;
            _performanceStats.BroadphaseCandidateCount = _broadphaseResults.Count;
            
            Profiler.EndSample();
            
            // Midphase：使用BVH进一步裁剪
            Profiler.BeginSample("CollisionSystem.Midphase");
            float midphaseQueryStartTime = Time.realtimeSinceStartup;
            
            foreach (var pair in _broadphaseResults)
            {
                if (_bvh.TestOverlap(pair.ShapeA, pair.ShapeB))
                {
                    _midphaseResults.Add(pair);
                }
            }
            
            // 更新Midphase查询时间和候选对数量
            _performanceStats.MidphaseQueryTime = (Time.realtimeSinceStartup - midphaseQueryStartTime) * 1000f;
            _performanceStats.MidphaseCandidateCount = _midphaseResults.Count;
            
            Profiler.EndSample();
            
            // Narrowphase：精确碰撞检测
            Profiler.BeginSample("CollisionSystem.Narrowphase");
            float narrowphaseStartTime = Time.realtimeSinceStartup;
            
            int collisionCount = 0;
            foreach (var pair in _midphaseResults)
            {
                _performanceStats.NarrowphaseTestCount++;
                
                Narrowphase.CollisionContact contact;
                if (_collisionDetector.DetectCollision(pair.ShapeA, pair.ShapeB, out contact))
                {
                    _narrowphaseResults.Add(contact);
                    collisionCount++;
                    
                    // 触发碰撞事件
                    OnCollisionDetected?.Invoke(contact);
                }
            }
            
            // 更新Narrowphase检测时间和碰撞数量
            _performanceStats.NarrowphaseDetectionTime = (Time.realtimeSinceStartup - narrowphaseStartTime) * 1000f;
            _performanceStats.NarrowphaseCollisionCount = collisionCount;
            
            // 更新碰撞成功率
            _performanceStats.UpdateCollisionSuccessRate();
            
            Profiler.EndSample();
            
            // 更新总碰撞检测时间
            _performanceStats.TotalCollisionDetectionTime = (Time.realtimeSinceStartup - totalStartTime) * 1000f;
            
            // 如果启用，打印性能统计信息
            if (printPerformanceStats)
            {
                UnityEngine.Debug.Log(_performanceStats.GetStatsString());
            }
            
            Profiler.EndSample();
        }
        
        /// <summary>
        /// 注册静态碰撞体
        /// </summary>
        public void RegisterStaticShape(Shape.CollisionShape shape)
        {
            if (!_staticShapes.Contains(shape))
            {
                _staticShapes.Add(shape);
                // 更新性能统计中的静态碰撞体数量
                _performanceStats.StaticShapeCount = _staticShapes.Count;
            }
        }
        
        /// <summary>
        /// 注册动态碰撞体
        /// </summary>
        public void RegisterDynamicShape(Shape.CollisionShape shape)
        {
            if (!_dynamicShapes.Contains(shape))
            {
                _dynamicShapes.Add(shape);
                // 更新性能统计中的动态碰撞体数量
                _performanceStats.DynamicShapeCount = _dynamicShapes.Count;
            }
        }
        
        /// <summary>
        /// 注销碰撞体
        /// </summary>
        public void UnregisterShape(Shape.CollisionShape shape)
        {
            _staticShapes.Remove(shape);
            _dynamicShapes.Remove(shape);
            _octree.Remove(shape);
            
            // 更新性能统计中的碰撞体数量
            _performanceStats.StaticShapeCount = _staticShapes.Count;
            _performanceStats.DynamicShapeCount = _dynamicShapes.Count;
        }
        
        /// <summary>
        /// 获取碰撞结果
        /// </summary>
        public IReadOnlyList<Narrowphase.CollisionContact> GetCollisionResults()
        {
            return _narrowphaseResults;
        }
        
        /// <summary>
        /// 获取性能统计数据
        /// </summary>
        public PerformanceStats GetPerformanceStats()
        {
            return _performanceStats;
        }
        
        /// <summary>
        /// 打印当前性能统计信息到控制台
        /// </summary>
        [ContextMenu("打印性能统计")]
        public void PrintPerformanceStats()
        {
            UnityEngine.Debug.Log(_performanceStats.GetStatsString());
        }
        #endregion
    }
}