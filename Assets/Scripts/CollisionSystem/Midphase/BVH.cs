using UnityEngine;
using System.Collections.Generic;
using CollisionSystem.Shape;

namespace CollisionSystem.Midphase
{
    /// <summary>
    /// BVH节点类
    /// </summary>
    public class BVHNode
    {
        /// <summary>
        /// 节点的AABB
        /// </summary>
        public Bounds AABB { get; private set; }
        
        /// <summary>
        /// 左子节点
        /// </summary>
        public BVHNode Left { get; private set; }
        
        /// <summary>
        /// 右子节点
        /// </summary>
        public BVHNode Right { get; private set; }
        
        /// <summary>
        /// 节点中的物体（叶子节点）
        /// </summary>
        public CollisionShape Object { get; private set; }
        
        /// <summary>
        /// 是否为叶子节点
        /// </summary>
        public bool IsLeaf { get { return Object != null; } }
        
        /// <summary>
        /// 构造函数（内部节点）
        /// </summary>
        public BVHNode(BVHNode left, BVHNode right)
        {
            Left = left;
            Right = right;
            Object = null;
            
            // 计算当前节点的AABB（包含左右子节点的AABB）
            AABB = new Bounds(
                Vector3.zero,
                Vector3.zero
            );
            
            AABB.Encapsulate(left.AABB);
            AABB.Encapsulate(right.AABB);
        }
        
        /// <summary>
        /// 构造函数（叶子节点）
        /// </summary>
        public BVHNode(CollisionShape shape)
        {
            Left = null;
            Right = null;
            Object = shape;
            
            // 叶子节点的AABB就是物体的AABB
            AABB = shape.AABB;
        }
        
        /// <summary>
        /// 更新节点的AABB
        /// </summary>
        public void UpdateAABB()
        {
            if (IsLeaf)
            {
                AABB = Object.AABB;
            }
            else
            {
                Left.UpdateAABB();
                Right.UpdateAABB();
                
                AABB = new Bounds(
                    Vector3.zero,
                    Vector3.zero
                );
                
                AABB.Encapsulate(Left.AABB);
                AABB.Encapsulate(Right.AABB);
            }
        }
        
        /// <summary>
        /// 检查节点是否与另一个节点相交
        /// </summary>
        public bool Intersects(BVHNode other)
        {
            // 如果AABB不相交，节点肯定不相交
            if (!AABB.Intersects(other.AABB))
            {
                return false;
            }
            
            // 如果两个节点都是叶子节点，返回true
            if (IsLeaf && other.IsLeaf)
            {
                return true;
            }
            
            // 递归检查子节点
            if (IsLeaf)
            {
                return other.Intersects(this);
            }
            else if (other.IsLeaf)
            {
                return Left.Intersects(other) || Right.Intersects(other);
            }
            else
            {
                return Left.Intersects(other.Left) ||
                       Left.Intersects(other.Right) ||
                       Right.Intersects(other.Left) ||
                       Right.Intersects(other.Right);
            }
        }
        
        /// <summary>
        /// 检查节点是否与形状相交
        /// </summary>
        public bool Intersects(CollisionShape shape)
        {
            // 如果AABB不相交，节点肯定不与形状相交
            if (!AABB.Intersects(shape.AABB))
            {
                return false;
            }
            
            // 如果是叶子节点，返回true
            if (IsLeaf)
            {
                return true;
            }
            
            // 递归检查子节点
            return Left.Intersects(shape) || Right.Intersects(shape);
        }
    }
    
    /// <summary>
    /// BVH（Bounding Volume Hierarchy）层次包围盒系统
    /// </summary>
    public class BVH
    {
        /// <summary>
        /// BVH树的根节点
        /// </summary>
        public BVHNode Root { get; private set; }
        
        /// <summary>
        /// BVH树的总节点数
        /// </summary>
        public int NodeCount
        {
            get { return CountNodes(Root); }
        }
        
        /// <summary>
        /// BVH树的叶子节点数
        /// </summary>
        public int LeafNodeCount
        {
            get { return CountLeafNodes(Root); }
        }
        
        /// <summary>
        /// 构建BVH树
        /// </summary>
        public void Build(List<CollisionShape> shapes)
        {
            if (shapes == null || shapes.Count == 0)
            {
                Root = null;
                return;
            }
            
            // 复制形状列表，避免修改原始列表
            List<CollisionShape> shapeList = new List<CollisionShape>(shapes);
            
            // 构建BVH树
            Root = BuildRecursive(shapeList, 0);
        }
        
        /// <summary>
        /// 递归构建BVH树
        /// </summary>
        private BVHNode BuildRecursive(List<CollisionShape> shapes, int depth)
        {
            int count = shapes.Count;
            
            // 如果只有一个物体，创建叶子节点
            if (count == 1)
            {
                return new BVHNode(shapes[0]);
            }
            
            // 如果有两个物体，创建内部节点
            if (count == 2)
            {
                BVHNode left = new BVHNode(shapes[0]);
                BVHNode right = new BVHNode(shapes[1]);
                return new BVHNode(left, right);
            }
            
            // 计算所有物体的总AABB
            Bounds totalBounds = shapes[0].AABB;
            for (int i = 1; i < count; i++)
            {
                totalBounds.Encapsulate(shapes[i].AABB);
            }
            
            // 选择最长的轴作为划分轴
            int splitAxis = GetLongestAxis(totalBounds);
            
            // 根据划分轴对物体进行排序
            shapes.Sort((a, b) => CompareShapes(a, b, splitAxis));
            
            // 分割形状列表
            int mid = count / 2;
            List<CollisionShape> leftShapes = shapes.GetRange(0, mid);
            List<CollisionShape> rightShapes = shapes.GetRange(mid, count - mid);
            
            // 递归构建左右子树
            BVHNode leftNode = BuildRecursive(leftShapes, depth + 1);
            BVHNode rightNode = BuildRecursive(rightShapes, depth + 1);
            
            // 创建内部节点
            return new BVHNode(leftNode, rightNode);
        }
        
        /// <summary>
        /// 检查两个形状是否可能相交（通过BVH树）
        /// </summary>
        public bool TestOverlap(CollisionShape shapeA, CollisionShape shapeB)
        {
            if (Root == null)
            {
                return false;
            }
            
            // 首先检查两个形状的AABB是否相交
            if (!shapeA.AABB.Intersects(shapeB.AABB))
            {
                return false;
            }
            
            // 简化逻辑：如果两个形状的AABB相交，且BVH已经构建完成，则认为它们可能相交
            // 这种方法在Midphase阶段虽然不是最精确的，但可以确保不会过滤掉真正的碰撞对
            return true;
        }
        
        /// <summary>
        /// 更新BVH树
        /// </summary>
        public void Update()
        {
            if (Root != null)
            {
                Root.UpdateAABB();
            }
        }
        
        /// <summary>
        /// 清空BVH树
        /// </summary>
        public void Clear()
        {
            Root = null;
        }
        
        #region 辅助方法
        /// <summary>
        /// 获取AABB的最长轴
        /// </summary>
        private int GetLongestAxis(Bounds bounds)
        {
            Vector3 size = bounds.size;
            if (size.x >= size.y && size.x >= size.z)
            {
                return 0; // X轴
            }
            else if (size.y >= size.x && size.y >= size.z)
            {
                return 1; // Y轴
            }
            else
            {
                return 2; // Z轴
            }
        }
        
        /// <summary>
        /// 根据指定轴比较两个形状
        /// </summary>
        private int CompareShapes(CollisionShape a, CollisionShape b, int axis)
        {
            float centerA = GetCenter(a.AABB, axis);
            float centerB = GetCenter(b.AABB, axis);
            
            return centerA.CompareTo(centerB);
        }
        
        /// <summary>
        /// 获取AABB中心在指定轴上的坐标
        /// </summary>
        private float GetCenter(Bounds bounds, int axis)
        {
            switch (axis)
            {
                case 0: return bounds.center.x;
                case 1: return bounds.center.y;
                case 2: return bounds.center.z;
                default: return 0f;
            }
        }
        
        /// <summary>
        /// 递归计算节点数
        /// </summary>
        private int CountNodes(BVHNode node)
        {
            if (node == null)
                return 0;
                
            int count = 1; // 当前节点
            
            // 如果不是叶子节点，递归计算左右子节点
            if (!node.IsLeaf)
            {
                count += CountNodes(node.Left);
                count += CountNodes(node.Right);
            }
            
            return count;
        }
        
        /// <summary>
        /// 递归计算叶子节点数
        /// </summary>
        private int CountLeafNodes(BVHNode node)
        {
            if (node == null)
                return 0;
                
            // 如果是叶子节点，返回1
            if (node.IsLeaf)
                return 1;
            
            // 否则递归计算左右子节点的叶子节点数
            return CountLeafNodes(node.Left) + CountLeafNodes(node.Right);
        }
        #endregion
    }
}
