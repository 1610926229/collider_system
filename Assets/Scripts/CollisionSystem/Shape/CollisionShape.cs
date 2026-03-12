using UnityEngine;

namespace CollisionSystem.Shape
{
    /// <summary>
    /// 碰撞体类型枚举
    /// </summary>
    public enum CollisionShapeType
    {
        Sphere,
        Box,
        Cylinder,//圆柱
        ConvexHull,//凸包
        Mesh//mesh网格
    }
    
    /// <summary>
    /// 碰撞体抽象基类
    /// 系统内部统一使用的碰撞体表示
    /// </summary>
    public abstract class CollisionShape
    {
        #region 基本属性
        /// <summary>
        /// 碰撞体类型
        /// </summary>
        public CollisionShapeType Type { get; protected set; }
        
        /// <summary>
        /// 碰撞体的世界位置
        /// </summary>
        public Vector3 Position { get; set; }
        
        /// <summary>
        /// 碰撞体的世界旋转
        /// </summary>
        public Quaternion Rotation { get; set; }
        
        /// <summary>
        /// 碰撞体的缩放
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.one;
        
        /// <summary>
        /// 碰撞体的AABB包围盒
        /// </summary>
        public Bounds AABB { get; protected set; }
        
        /// <summary>
        /// 碰撞体的包围球
        /// </summary>
        public Sphere BoundingSphere { get; protected set; }
        
        /// <summary>
        /// 原始几何体数据
        /// </summary>
        public object RawData { get; protected set; }
        
        /// <summary>
        /// 对应的Unity物体
        /// </summary>
        public GameObject GameObject { get; set; }
        #endregion
        
        #region 构造函数
        protected CollisionShape(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
            UpdateBounds();
        }
        #endregion
        
        #region 公共方法
        /// <summary>
        /// 更新碰撞体的边界
        /// </summary>
        public virtual void UpdateBounds()
        {
            // 由子类实现具体的边界计算
        }
        
        /// <summary>
        /// 获取碰撞体的顶点列表
        /// </summary>
        public abstract Vector3[] GetVertices();
        
        /// <summary>
        /// 获取碰撞体的面列表
        /// </summary>
        public abstract int[] GetFaces();
        
        /// <summary>
        /// 获取碰撞体的边列表
        /// </summary>
        public abstract Edge[] GetEdges();
        
        /// <summary>
        /// 获取碰撞体的面法线
        /// </summary>
        public abstract Vector3[] GetFaceNormals();
        
        /// <summary>
        /// 检查点是否在碰撞体内
        /// </summary>
        public abstract bool ContainsPoint(Vector3 point);
        
        /// <summary>
        /// 获取碰撞体在指定方向上的支持点
        /// </summary>
        public abstract Vector3 Support(Vector3 direction);
        #endregion
    }
    
    /// <summary>
    /// 球体结构体
    /// </summary>
    public struct Sphere
    {
        public Vector3 Center;
        public float Radius;
        
        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
    
    /// <summary>
    /// 边结构体
    /// </summary>
    public struct Edge
    {
        public int StartIndex;
        public int EndIndex;
        
        public Edge(int startIndex, int endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}
