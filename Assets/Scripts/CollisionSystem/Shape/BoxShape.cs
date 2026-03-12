using UnityEngine;

namespace CollisionSystem.Shape
{
    /// <summary>
    /// 立方体碰撞体
    /// </summary>
    public class BoxShape : CollisionShape
    {
        /// <summary>
        /// 立方体的尺寸（半边长）
        /// </summary>
        public Vector3 HalfExtents { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BoxShape(Vector3 position, Quaternion rotation, Vector3 halfExtents)
            : base(position, rotation)
        {
            Type = CollisionShapeType.Box;
            HalfExtents = halfExtents;
            RawData = halfExtents;
            UpdateBounds();
        }
        
        /// <summary>
        /// 更新边界
        /// </summary>
        public override void UpdateBounds()
        {
            // 计算变换后的半边长
            Vector3 transformedHalfExtents = new Vector3(
                HalfExtents.x * Scale.x,
                HalfExtents.y * Scale.y,
                HalfExtents.z * Scale.z
            );
            
            // 计算AABB
            Vector3 size = transformedHalfExtents * 2f;
            AABB = new Bounds(Position, size);
            
            // 计算包围球
            float radius = transformedHalfExtents.magnitude;
            BoundingSphere = new Sphere(Position, radius);
        }
        
        /// <summary>
        /// 获取顶点列表
        /// </summary>
        public override Vector3[] GetVertices()
        {
            Vector3 transformedHalfExtents = new Vector3(
                HalfExtents.x * Scale.x,
                HalfExtents.y * Scale.y,
                HalfExtents.z * Scale.z
            );
            
            // 立方体的8个顶点
            Vector3[] vertices = new Vector3[8];
            vertices[0] = Position + Rotation * new Vector3(-transformedHalfExtents.x, -transformedHalfExtents.y, -transformedHalfExtents.z);
            vertices[1] = Position + Rotation * new Vector3(transformedHalfExtents.x, -transformedHalfExtents.y, -transformedHalfExtents.z);
            vertices[2] = Position + Rotation * new Vector3(transformedHalfExtents.x, -transformedHalfExtents.y, transformedHalfExtents.z);
            vertices[3] = Position + Rotation * new Vector3(-transformedHalfExtents.x, -transformedHalfExtents.y, transformedHalfExtents.z);
            vertices[4] = Position + Rotation * new Vector3(-transformedHalfExtents.x, transformedHalfExtents.y, -transformedHalfExtents.z);
            vertices[5] = Position + Rotation * new Vector3(transformedHalfExtents.x, transformedHalfExtents.y, -transformedHalfExtents.z);
            vertices[6] = Position + Rotation * new Vector3(transformedHalfExtents.x, transformedHalfExtents.y, transformedHalfExtents.z);
            vertices[7] = Position + Rotation * new Vector3(-transformedHalfExtents.x, transformedHalfExtents.y, transformedHalfExtents.z);
            
            return vertices;
        }
        
        /// <summary>
        /// 获取面列表
        /// </summary>
        public override int[] GetFaces()
        {
            // 立方体的6个面，每个面4个顶点
            int[] faces = new int[6 * 4] {
                0, 1, 2, 3,  // 底面
                4, 7, 6, 5,  // 顶面
                0, 3, 7, 4,  // 左面
                1, 5, 6, 2,  // 右面
                0, 4, 5, 1,  // 前面
                3, 2, 6, 7   // 后面
            };
            
            return faces;
        }
        
        /// <summary>
        /// 获取边列表
        /// </summary>
        public override Edge[] GetEdges()
        {
            // 立方体的12条边
            Edge[] edges = new Edge[12] {
                new Edge(0, 1), new Edge(1, 2), new Edge(2, 3), new Edge(3, 0),  // 底面
                new Edge(4, 5), new Edge(5, 6), new Edge(6, 7), new Edge(7, 4),  // 顶面
                new Edge(0, 4), new Edge(1, 5), new Edge(2, 6), new Edge(3, 7)   // 垂直边
            };
            
            return edges;
        }
        
        /// <summary>
        /// 获取面法线
        /// </summary>
        public override Vector3[] GetFaceNormals()
        {
            // 立方体的6个面法线
            Vector3[] normals = new Vector3[6] {
                Rotation * Vector3.down,   // 底面
                Rotation * Vector3.up,     // 顶面
                Rotation * Vector3.left,   // 左面
                Rotation * Vector3.right,  // 右面
                Rotation * Vector3.forward,// 前面
                Rotation * Vector3.back    // 后面
            };
            
            return normals;
        }
        
        /// <summary>
        /// 检查点是否在立方体内
        /// </summary>
        public override bool ContainsPoint(Vector3 point)
        {
            Vector3 localPoint = Quaternion.Inverse(Rotation) * (point - Position);
            Vector3 transformedHalfExtents = new Vector3(
                HalfExtents.x * Scale.x,
                HalfExtents.y * Scale.y,
                HalfExtents.z * Scale.z
            );
            
            return Mathf.Abs(localPoint.x) <= transformedHalfExtents.x &&
                   Mathf.Abs(localPoint.y) <= transformedHalfExtents.y &&
                   Mathf.Abs(localPoint.z) <= transformedHalfExtents.z;
        }
        
        /// <summary>
        /// 获取立方体在指定方向上的支持点
        /// </summary>
        public override Vector3 Support(Vector3 direction)
        {
            Vector3 localDirection = Quaternion.Inverse(Rotation) * direction;
            Vector3 transformedHalfExtents = new Vector3(
                HalfExtents.x * Scale.x,
                HalfExtents.y * Scale.y,
                HalfExtents.z * Scale.z
            );
            
            // 立方体的支持点在每个轴上取最大或最小值
            Vector3 supportLocal = new Vector3(
                Mathf.Sign(localDirection.x) * transformedHalfExtents.x,
                Mathf.Sign(localDirection.y) * transformedHalfExtents.y,
                Mathf.Sign(localDirection.z) * transformedHalfExtents.z
            );
            
            return Position + Rotation * supportLocal;
        }
    }
}
