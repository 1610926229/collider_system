using UnityEngine;
using System.Collections.Generic;

namespace CollisionSystem.Shape
{
    /// <summary>
    /// 凸多面体碰撞体
    /// </summary>
    public class ConvexHullShape : CollisionShape
    {
        /// <summary>
        /// 凸多面体的顶点列表
        /// </summary>
        private List<Vector3> _vertices;
        
        /// <summary>
        /// 凸多面体的面列表
        /// </summary>
        private List<int[]> _faces;
        
        /// <summary>
        /// 凸多面体的面法线
        /// </summary>
        private List<Vector3> _faceNormals;
        
        /// <summary>
        /// 凸多面体的边列表
        /// </summary>
        private List<Edge> _edges;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public ConvexHullShape(Vector3 position, Quaternion rotation, List<Vector3> vertices)
            : base(position, rotation)
        {
            Type = CollisionShapeType.ConvexHull;
            _vertices = new List<Vector3>(vertices);
            RawData = _vertices;
            
            // 计算面、边和面法线
            ComputeFaces();
            ComputeEdges();
            ComputeFaceNormals();
            
            UpdateBounds();
        }
        
        /// <summary>
        /// 更新边界
        /// </summary>
        public override void UpdateBounds()
        {
            if (_vertices.Count == 0)
            {
                AABB = new Bounds(Position, Vector3.zero);
                BoundingSphere = new Sphere(Position, 0f);
                return;
            }
            
            // 计算变换后的顶点
            List<Vector3> transformedVertices = new List<Vector3>();
            foreach (var vertex in _vertices)
            {
                Vector3 scaledVertex = Vector3.Scale(vertex, Scale);
                Vector3 transformedVertex = Position + Rotation * scaledVertex;
                transformedVertices.Add(transformedVertex);
            }
            
            // 计算AABB
            Vector3 min = transformedVertices[0];
            Vector3 max = transformedVertices[0];
            
            foreach (var vertex in transformedVertices)
            {
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }
            
            AABB = new Bounds((min + max) * 0.5f, max - min);
            
            // 计算包围球
            Vector3 center = AABB.center;
            float maxDistanceSquared = 0f;
            
            foreach (var vertex in transformedVertices)
            {
                float distanceSquared = (vertex - center).sqrMagnitude;
                if (distanceSquared > maxDistanceSquared)
                {
                    maxDistanceSquared = distanceSquared;
                }
            }
            
            BoundingSphere = new Sphere(center, Mathf.Sqrt(maxDistanceSquared));
        }
        
        /// <summary>
        /// 获取顶点列表
        /// </summary>
        public override Vector3[] GetVertices()
        {
            Vector3[] result = new Vector3[_vertices.Count];
            for (int i = 0; i < _vertices.Count; i++)
            {
                Vector3 scaledVertex = Vector3.Scale(_vertices[i], Scale);
                result[i] = Position + Rotation * scaledVertex;
            }
            return result;
        }
        
        /// <summary>
        /// 获取面列表
        /// </summary>
        public override int[] GetFaces()
        {
            List<int> faceIndices = new List<int>();
            foreach (var face in _faces)
            {
                faceIndices.AddRange(face);
            }
            return faceIndices.ToArray();
        }
        
        /// <summary>
        /// 获取边列表
        /// </summary>
        public override Edge[] GetEdges()
        {
            return _edges.ToArray();
        }
        
        /// <summary>
        /// 获取面法线
        /// </summary>
        public override Vector3[] GetFaceNormals()
        {
            Vector3[] result = new Vector3[_faceNormals.Count];
            for (int i = 0; i < _faceNormals.Count; i++)
            {
                result[i] = Rotation * _faceNormals[i];
            }
            return result;
        }
        
        /// <summary>
        /// 检查点是否在凸多面体内
        /// </summary>
        public override bool ContainsPoint(Vector3 point)
        {
            // 将点转换到局部坐标系
            Vector3 localPoint = Quaternion.Inverse(Rotation) * (point - Position);
            localPoint = new Vector3(
                localPoint.x / Scale.x,
                localPoint.y / Scale.y,
                localPoint.z / Scale.z
            );
            
            // 检查点是否在所有面的内侧
            for (int i = 0; i < _faces.Count; i++)
            {
                Vector3 normal = _faceNormals[i];
                Vector3 faceVertex = _vertices[_faces[i][0]];
                
                // 计算点到面的距离
                float distance = Vector3.Dot(normal, localPoint - faceVertex);
                
                // 如果点在面的外侧，则不在凸多面体内
                if (distance > Mathf.Epsilon)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取凸多面体在指定方向上的支持点
        /// </summary>
        public override Vector3 Support(Vector3 direction)
        {
            if (_vertices.Count == 0)
            {
                return Position;
            }
            
            // 将方向转换到局部坐标系
            Vector3 localDirection = Quaternion.Inverse(Rotation) * direction;
            localDirection = new Vector3(
                localDirection.x / Scale.x,
                localDirection.y / Scale.y,
                localDirection.z / Scale.z
            );
            
            // 找到具有最大点积的顶点
            int bestIndex = 0;
            float bestDot = Vector3.Dot(localDirection, _vertices[0]);
            
            for (int i = 1; i < _vertices.Count; i++)
            {
                float dot = Vector3.Dot(localDirection, _vertices[i]);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestIndex = i;
                }
            }
            
            // 将支持点转换回世界坐标系
            Vector3 scaledVertex = Vector3.Scale(_vertices[bestIndex], Scale);
            return Position + Rotation * scaledVertex;
        }
        
        #region 私有方法
        /// <summary>
        /// 计算凸多面体的面
        /// </summary>
        private void ComputeFaces()
        {
            // 简单实现：使用Gift Wrapping算法计算凸包
            // 这里假设输入的顶点已经是凸包
            _faces = new List<int[]>();
            
            // 找到一个初始面
            if (_vertices.Count < 3)
            {
                return;
            }
            
            // 简单的三角化（对于教学目的，使用简单的实现）
            for (int i = 2; i < _vertices.Count; i++)
            {
                _faces.Add(new int[] { 0, i - 1, i });
            }
        }
        
        /// <summary>
        /// 计算凸多面体的边
        /// </summary>
        private void ComputeEdges()
        {
            _edges = new List<Edge>();
            HashSet<(int, int)> edgeSet = new HashSet<(int, int)>();
            
            foreach (var face in _faces)
            {
                for (int i = 0; i < face.Length; i++)
                {
                    int v1 = face[i];
                    int v2 = face[(i + 1) % face.Length];
                    
                    // 确保边的唯一性（按顶点索引排序）
                    (int, int) edge = v1 < v2 ? (v1, v2) : (v2, v1);
                    if (!edgeSet.Contains(edge))
                    {
                        edgeSet.Add(edge);
                        _edges.Add(new Edge(v1, v2));
                    }
                }
            }
        }
        
        /// <summary>
        /// 计算凸多面体的面法线
        /// </summary>
        private void ComputeFaceNormals()
        {
            _faceNormals = new List<Vector3>();
            
            foreach (var face in _faces)
            {
                if (face.Length < 3)
                {
                    _faceNormals.Add(Vector3.zero);
                    continue;
                }
                
                // 计算面的法线
                Vector3 v0 = _vertices[face[0]];
                Vector3 v1 = _vertices[face[1]];
                Vector3 v2 = _vertices[face[2]];
                
                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                Vector3 normal = Vector3.Cross(edge1, edge2).normalized;
                
                _faceNormals.Add(normal);
            }
        }
        #endregion
    }
}
