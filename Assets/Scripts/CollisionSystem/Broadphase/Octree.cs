using UnityEngine;
using System.Collections.Generic;
using CollisionSystem.Shape;

namespace CollisionSystem.Broadphase
{
    /// <summary>
    /// 八叉树节点类
    /// </summary>
    public class OctreeNode
    {
        /// <summary>
        /// 节点的AABB
        /// </summary>
        public Bounds Bounds { get; private set; }
        
        /// <summary>
        /// 节点的深度
        /// </summary>
        public int Depth { get; private set; }
        
        /// <summary>
        /// 节点中的物体列表
        /// </summary>
        public List<CollisionShape> Objects { get; private set; }
        
        /// <summary>
        /// 子节点列表（8个子节点）
        /// </summary>
        public OctreeNode[] Children { get; private set; }
        
        /// <summary>
        /// 是否为叶子节点
        /// </summary>
        public bool IsLeaf { get { return Children == null; } }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public OctreeNode(Bounds bounds, int depth)
        {
            Bounds = bounds;
            Depth = depth;
            Objects = new List<CollisionShape>();
            Children = null;
        }
        
        /// <summary>
        /// 细分节点，创建8个子节点
        /// </summary>
        public void Subdivide(int maxDepth, int maxObjectsPerCell)
        {
            if (Depth >= maxDepth || Objects.Count <= maxObjectsPerCell)
            {
                return;
            }
            
            // 如果已经有子节点，不需要再细分
            if (Children != null)
            {
                return;
            }
            
            // 创建8个子节点
            Children = new OctreeNode[8];
            
            Vector3 center = Bounds.center;
            Vector3 halfSize = Bounds.extents;
            Vector3 quarterSize = halfSize * 0.5f;
            
            // 子节点的位置偏移
            Vector3[] offsets = new Vector3[8] {
                new Vector3(-quarterSize.x, -quarterSize.y, -quarterSize.z),
                new Vector3(quarterSize.x, -quarterSize.y, -quarterSize.z),
                new Vector3(quarterSize.x, -quarterSize.y, quarterSize.z),
                new Vector3(-quarterSize.x, -quarterSize.y, quarterSize.z),
                new Vector3(-quarterSize.x, quarterSize.y, -quarterSize.z),
                new Vector3(quarterSize.x, quarterSize.y, -quarterSize.z),
                new Vector3(quarterSize.x, quarterSize.y, quarterSize.z),
                new Vector3(-quarterSize.x, quarterSize.y, quarterSize.z)
            };
            
            // 创建子节点
            for (int i = 0; i < 8; i++)
            {
                Vector3 childCenter = center + offsets[i];
                Bounds childBounds = new Bounds(childCenter, halfSize);
                Children[i] = new OctreeNode(childBounds, Depth + 1);
            }
            
            // 将物体分配到子节点
            List<CollisionShape> remainingObjects = new List<CollisionShape>();
            foreach (var obj in Objects)
            {
                bool placed = false;
                foreach (var child in Children)
                {
                    if (child.Bounds.Contains(obj.AABB.min) && child.Bounds.Contains(obj.AABB.max))
                    {
                        child.Insert(obj, maxDepth, maxObjectsPerCell);
                        placed = true;
                        break;
                    }
                }
                
                if (!placed)
                {
                    remainingObjects.Add(obj);
                }
            }
            
            Objects = remainingObjects;
        }
        
        /// <summary>
        /// 插入物体到节点
        /// </summary>
        public void Insert(CollisionShape shape, int maxDepth, int maxObjectsPerCell)
        {
            // 如果物体不在当前节点的AABB内，不插入
            if (!Bounds.Intersects(shape.AABB))
            {
                return;
            }
            
            // 如果是叶子节点
            if (IsLeaf)
            {
                Objects.Add(shape);
                
                // 如果节点中的物体数超过阈值，尝试细分
                if (Objects.Count > maxObjectsPerCell && Depth < maxDepth)
                {
                    Subdivide(maxDepth, maxObjectsPerCell);
                }
            }
            else
            {
                // 尝试插入到子节点
                bool placed = false;
                foreach (var child in Children)
                {
                    if (child.Bounds.Contains(shape.AABB.min) && child.Bounds.Contains(shape.AABB.max))
                    {
                        child.Insert(shape, maxDepth, maxObjectsPerCell);
                        placed = true;
                        break;
                    }
                }
                
                // 如果没有子节点完全包含物体，将物体留在当前节点
                if (!placed)
                {
                    Objects.Add(shape);
                }
            }
        }
        
        /// <summary>
        /// 从节点中移除物体
        /// </summary>
        public bool Remove(CollisionShape shape)
        {
            // 如果物体不在当前节点的AABB内，返回false
            if (!Bounds.Intersects(shape.AABB))
            {
                return false;
            }
            
            // 尝试从当前节点移除
            if (Objects.Remove(shape))
            {
                return true;
            }
            
            // 如果不是叶子节点，尝试从子节点移除
            if (!IsLeaf)
            {
                foreach (var child in Children)
                {
                    if (child.Remove(shape))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 查询与指定形状相交的所有形状
        /// </summary>
        public void QueryIntersecting(CollisionShape shape, List<CollisionShape> result)
        {
            // 如果当前节点与形状不相交，返回
            if (!Bounds.Intersects(shape.AABB))
            {
                return;
            }
            
            // 检查当前节点中的所有物体
            foreach (var obj in Objects)
            {
                if (obj != shape && obj.AABB.Intersects(shape.AABB))
                {
                    result.Add(obj);
                }
            }
            
            // 如果不是叶子节点，递归查询子节点
            if (!IsLeaf)
            {
                foreach (var child in Children)
                {
                    child.QueryIntersecting(shape, result);
                }
            }
        }
        
        /// <summary>
        /// 收集节点中的所有物体
        /// </summary>
        public void CollectAllObjects(List<CollisionShape> result)
        {
            result.AddRange(Objects);
            
            if (!IsLeaf)
            {
                foreach (var child in Children)
                {
                    child.CollectAllObjects(result);
                }
            }
        }
        
        /// <summary>
        /// 清空节点
        /// </summary>
        public void Clear()
        {
            Objects.Clear();
            
            if (!IsLeaf)
            {
                foreach (var child in Children)
                {
                    child.Clear();
                }
                Children = null;
            }
        }
    }
    
    /// <summary>
    /// 八叉树类
    /// </summary>
    public class Octree
    {
        /// <summary>
        /// 八叉树的根节点
        /// </summary>
        public OctreeNode Root { get; private set; }
        
        /// <summary>
        /// 八叉树的最大深度
        /// </summary>
        public int MaxDepth { get; private set; }
        
        /// <summary>
        /// 八叉树单元格的最大物体数
        /// </summary>
        public int MaxObjectsPerCell { get; private set; }
        
        /// <summary>
        /// 八叉树的总节点数
        /// </summary>
        public int NodeCount
        {
            get { return CountNodes(Root); }
        }
        
        /// <summary>
        /// 八叉树的叶子节点数
        /// </summary>
        public int LeafNodeCount
        {
            get { return CountLeafNodes(Root); }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public Octree(Vector3 center, float size, int maxDepth, int maxObjectsPerCell)
        {
            Bounds rootBounds = new Bounds(center, Vector3.one * size);
            Root = new OctreeNode(rootBounds, 0);
            MaxDepth = maxDepth;
            MaxObjectsPerCell = maxObjectsPerCell;
        }
        
        /// <summary>
        /// 插入物体到八叉树
        /// </summary>
        public void Insert(CollisionShape shape)
        {
            Root.Insert(shape, MaxDepth, MaxObjectsPerCell);
        }
        
        /// <summary>
        /// 从八叉树中移除物体
        /// </summary>
        public bool Remove(CollisionShape shape)
        {
            return Root.Remove(shape);
        }
        
        /// <summary>
        /// 查询与指定形状相交的所有形状
        /// </summary>
        public void QueryIntersecting(CollisionShape shape, List<CollisionShape> result)
        {
            Root.QueryIntersecting(shape, result);
        }
        
        /// <summary>
        /// 查询所有可能相交的形状对
        /// </summary>
        public void QueryPotentialPairs(List<ShapePair> result)
        {
            // 收集所有物体
            List<CollisionShape> allObjects = new List<CollisionShape>();
            Root.CollectAllObjects(allObjects);
            
            // 对每个物体，查询可能相交的物体
            HashSet<ShapePair> processedPairs = new HashSet<ShapePair>();
            
            foreach (var obj in allObjects)
            {
                List<CollisionShape> potentialColliders = new List<CollisionShape>();
                QueryIntersecting(obj, potentialColliders);
                
                // 生成形状对
                foreach (var collider in potentialColliders)
                {
                    ShapePair pair = new ShapePair(obj, collider);
                    if (!processedPairs.Contains(pair))
                    {
                        result.Add(pair);
                        processedPairs.Add(pair);
                    }
                }
            }
        }
        
        /// <summary>
        /// 收集八叉树中的所有物体
        /// </summary>
        public List<CollisionShape> CollectAllObjects()
        {
            List<CollisionShape> result = new List<CollisionShape>();
            Root.CollectAllObjects(result);
            return result;
        }
        
        /// <summary>
        /// 清空八叉树
        /// </summary>
        public void Clear()
        {
            Root.Clear();
        }
        
        /// <summary>
        /// 递归计算节点数
        /// </summary>
        private int CountNodes(OctreeNode node)
        {
            if (node == null)
                return 0;
                
            int count = 1; // 当前节点
            
            // 如果不是叶子节点，递归计算子节点
            if (!node.IsLeaf)
            {
                foreach (var child in node.Children)
                {
                    count += CountNodes(child);
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// 递归计算叶子节点数
        /// </summary>
        private int CountLeafNodes(OctreeNode node)
        {
            if (node == null)
                return 0;
                
            // 如果是叶子节点，返回1
            if (node.IsLeaf)
                return 1;
            
            // 否则递归计算子节点的叶子节点数
            int count = 0;
            foreach (var child in node.Children)
            {
                count += CountLeafNodes(child);
            }
            
            return count;
        }
    }
}
