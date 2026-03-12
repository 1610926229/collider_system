using UnityEngine;

namespace CollisionSystem.Shape
{
    /// <summary>
    /// 球体碰撞体
    /// </summary>
    public class SphereShape : CollisionShape
    {
        /// <summary>
        /// 球体半径
        /// </summary>
        public float Radius { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SphereShape(Vector3 position, Quaternion rotation, float radius)
            : base(position, rotation)
        {
            Type = CollisionShapeType.Sphere;
            Radius = radius;
            RawData = radius;
            UpdateBounds();
        }
        
        /// <summary>
        /// 更新边界
        /// </summary>
        public override void UpdateBounds()
        {
            float scaledRadius = Radius * Mathf.Max(Scale.x, Mathf.Max(Scale.y, Scale.z));
            
            AABB = new Bounds(Position, Vector3.one * scaledRadius * 2f);
            BoundingSphere = new Sphere(Position, scaledRadius);
        }
        
        /// <summary>
        /// 获取顶点列表（用于可视化和某些算法）
        /// </summary>
        public override Vector3[] GetVertices()
        {
            // 生成球体的近似顶点（用于可视化）
            int segments = 8;
            Vector3[] vertices = new Vector3[(segments + 1) * (segments + 1)];
            float scaledRadius = Radius * Mathf.Max(Scale.x, Mathf.Max(Scale.y, Scale.z));
            
            int index = 0;
            for (int lat = 0; lat <= segments; lat++)
            {
                float theta = lat * Mathf.PI / segments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                for (int lon = 0; lon <= segments; lon++)
                {
                    float phi = lon * 2 * Mathf.PI / segments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    
                    Vector3 vertex = new Vector3(
                        cosPhi * sinTheta,
                        cosTheta,
                        sinPhi * sinTheta
                    );
                    
                    // 应用缩放和变换
                    vertex = Vector3.Scale(vertex * Radius, Scale);
                    vertex = Rotation * vertex + Position;
                    
                    vertices[index++] = vertex;
                }
            }
            
            return vertices;
        }
        
        /// <summary>
        /// 获取面列表（用于可视化）
        /// </summary>
        public override int[] GetFaces()
        {
            int segments = 8;
            int[] faces = new int[segments * segments * 6];
            int index = 0;
            
            for (int lat = 0; lat < segments; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int first = (lat * (segments + 1)) + lon;
                    int second = first + segments + 1;
                    
                    faces[index++] = first;
                    faces[index++] = second;
                    faces[index++] = first + 1;
                    
                    faces[index++] = second;
                    faces[index++] = second + 1;
                    faces[index++] = first + 1;
                }
            }
            
            return faces;
        }
        
        /// <summary>
        /// 获取边列表（用于可视化）
        /// </summary>
        public override Edge[] GetEdges()
        {
            int segments = 8;
            Edge[] edges = new Edge[(segments + 1) * segments * 2];
            int index = 0;
            
            // 经线
            for (int lat = 0; lat <= segments; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    edges[index++] = new Edge(
                        lat * (segments + 1) + lon,
                        lat * (segments + 1) + lon + 1
                    );
                }
            }
            
            // 纬线
            for (int lon = 0; lon <= segments; lon++)
            {
                for (int lat = 0; lat < segments; lat++)
                {
                    edges[index++] = new Edge(
                        lat * (segments + 1) + lon,
                        (lat + 1) * (segments + 1) + lon
                    );
                }
            }
            
            return edges;
        }
        
        /// <summary>
        /// 获取面法线
        /// </summary>
        public override Vector3[] GetFaceNormals()
        {
            // 球体的面法线就是顶点方向
            Vector3[] vertices = GetVertices();
            Vector3[] normals = new Vector3[vertices.Length];
            
            for (int i = 0; i < vertices.Length; i++)
            {
                normals[i] = (vertices[i] - Position).normalized;
            }
            
            return normals;
        }
        
        /// <summary>
        /// 检查点是否在球体内
        /// </summary>
        public override bool ContainsPoint(Vector3 point)
        {
            Vector3 localPoint = Quaternion.Inverse(Rotation) * (point - Position);
            localPoint = new Vector3(localPoint.x / Scale.x, localPoint.y / Scale.y, localPoint.z / Scale.z);
            
            return localPoint.magnitude <= Radius;
        }
        
        /// <summary>
        /// 获取球体在指定方向上的支持点
        /// </summary>
        public override Vector3 Support(Vector3 direction)
        {
            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                direction = Vector3.forward;
            }
            
            direction = direction.normalized;
            Vector3 localSupport = direction * Radius;
            
            // 应用缩放和变换
            localSupport = Vector3.Scale(localSupport, Scale);
            return Rotation * localSupport + Position;
        }
    }
}
