using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using CollisionSystem.Shape;

namespace CollisionSystem.Converter
{
    /// <summary>
    /// NavMesh转换器
    /// 将Unity的NavMesh转换为自定义的CollisionShape
    /// </summary>
    public static class NavMeshConverter
    {
        /// <summary>
        /// 从NavMesh中提取数据并转换为CollisionShape列表
        /// </summary>
        /// <param name="agentTypeID">NavMesh代理类型ID，默认为0</param>
        /// <param name="areaMask">NavMesh区域掩码，默认为-1（所有区域）</param>
        /// <returns>转换后的CollisionShape列表</returns>
        public static List<CollisionShape> ConvertNavMeshToCollisionShapes(
            int agentTypeID = 0,
            int areaMask = -1
        )
        {
            List<CollisionShape> shapes = new List<CollisionShape>();
            
            // 获取NavMesh的三角化数据
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            
            // 如果没有三角化数据，返回空列表
            if (triangulation.vertices == null || triangulation.vertices.Length == 0)
            {
                UnityEngine.Debug.LogWarning("No NavMesh data found!");
                return shapes;
            }
            
            // 将NavMesh数据分割为多个凸包
            List<List<int>> convexHulls = SplitIntoConvexHulls(triangulation);
            
            // 为每个凸包创建ConvexHullShape
            foreach (var hullIndices in convexHulls)
            {
                // 提取凸包的顶点
                List<Vector3> hullVertices = new List<Vector3>();
                foreach (int index in hullIndices)
                {
                    hullVertices.Add(triangulation.vertices[index]);
                }
                
                // 创建ConvexHullShape
                if (hullVertices.Count >= 3)
                {
                    // 计算凸包的中心位置
                    Vector3 center = Vector3.zero;
                    foreach (var vertex in hullVertices)
                    {
                        center += vertex;
                    }
                    center /= hullVertices.Count;
                    
                    ConvexHullShape convexHullShape = new ConvexHullShape(center, Quaternion.identity, hullVertices);
                    shapes.Add(convexHullShape);
                }
            }
            
            return shapes;
        }
        
        /// <summary>
        /// 将NavMesh数据分割为多个凸包
        /// </summary>
        private static List<List<int>> SplitIntoConvexHulls(NavMeshTriangulation triangulation)
        {
            List<List<int>> convexHulls = new List<List<int>>();
            
            // 创建一个顶点是否已使用的标记数组
            bool[] vertexUsed = new bool[triangulation.vertices.Length];
            
            // 遍历所有三角形，将其分组为凸包
            for (int i = 0; i < triangulation.indices.Length; i += 3)
            {
                // 获取当前三角形的三个顶点索引
                int v0 = triangulation.indices[i];
                int v1 = triangulation.indices[i + 1];
                int v2 = triangulation.indices[i + 2];
                
                // 检查是否有未使用的顶点
                bool hasUnusedVertex = !vertexUsed[v0] || !vertexUsed[v1] || !vertexUsed[v2];
                
                if (hasUnusedVertex)
                {
                    // 创建一个新的凸包
                    List<int> hullIndices = new List<int>();
                    
                    // 添加当前三角形的顶点
                    if (!vertexUsed[v0])
                    {
                        hullIndices.Add(v0);
                        vertexUsed[v0] = true;
                    }
                    if (!vertexUsed[v1])
                    {
                        hullIndices.Add(v1);
                        vertexUsed[v1] = true;
                    }
                    if (!vertexUsed[v2])
                    {
                        hullIndices.Add(v2);
                        vertexUsed[v2] = true;
                    }
                    
                    // 寻找与当前凸包相连的其他三角形
                    for (int j = i + 3; j < triangulation.indices.Length; j += 3)
                    {
                        int u0 = triangulation.indices[j];
                        int u1 = triangulation.indices[j + 1];
                        int u2 = triangulation.indices[j + 2];
                        
                        // 检查是否与当前凸包共享边
                        bool sharesEdge = (hullIndices.Contains(u0) && hullIndices.Contains(u1)) ||
                                          (hullIndices.Contains(u1) && hullIndices.Contains(u2)) ||
                                          (hullIndices.Contains(u2) && hullIndices.Contains(u0));
                        
                        if (sharesEdge)
                        {
                            // 添加未使用的顶点到凸包
                            if (!vertexUsed[u0])
                            {
                                hullIndices.Add(u0);
                                vertexUsed[u0] = true;
                            }
                            if (!vertexUsed[u1])
                            {
                                hullIndices.Add(u1);
                                vertexUsed[u1] = true;
                            }
                            if (!vertexUsed[u2])
                            {
                                hullIndices.Add(u2);
                                vertexUsed[u2] = true;
                            }
                        }
                    }
                    
                    convexHulls.Add(hullIndices);
                }
            }
            
            return convexHulls;
        }
        
        /// <summary>
        /// 将NavMesh数据转换为CollisionShape并注册到碰撞系统
        /// </summary>
        /// <param name="agentTypeID">NavMesh代理类型ID，默认为0</param>
        /// <param name="areaMask">NavMesh区域掩码，默认为-1（所有区域）</param>
        /// <returns>注册的CollisionShape列表</returns>
        public static List<CollisionShape> RegisterNavMeshToCollisionSystem(
            int agentTypeID = 0,
            int areaMask = -1
        )
        {
            // 转换NavMesh数据为CollisionShape列表
            List<CollisionShape> shapes = ConvertNavMeshToCollisionShapes(agentTypeID, areaMask);
            
            // 将所有形状注册到碰撞系统
            foreach (var shape in shapes)
            {
                CollisionSystem.Instance.RegisterStaticShape(shape);
            }
            
            return shapes;
        }
        
        /// <summary>
        /// 从碰撞系统中注销NavMesh转换的CollisionShape
        /// </summary>
        /// <param name="shapes">要注销的CollisionShape列表</param>
        public static void UnregisterNavMeshShapesFromCollisionSystem(List<CollisionShape> shapes)
        {
            if (shapes == null) return;
            
            foreach (var shape in shapes)
            {
                CollisionSystem.Instance.UnregisterShape(shape);
            }
        }
        
        /// <summary>
        /// 创建一个测试用的NavMesh碰撞体可视化对象
        /// </summary>
        /// <param name="shapes">CollisionShape列表</param>
        /// <param name="parent">父对象，默认为null</param>
        public static void CreateNavMeshVisualization(
            List<CollisionShape> shapes,
            Transform parent = null
        )
        {
            if (shapes == null || shapes.Count == 0) return;
            
            // 创建一个父对象用于组织可视化对象
            GameObject navMeshVisualization = new GameObject("NavMeshVisualization");
            if (parent != null)
            {
                navMeshVisualization.transform.SetParent(parent);
            }
            
            // 为每个形状创建可视化对象
            for (int i = 0; i < shapes.Count; i++)
            {
                CollisionShape shape = shapes[i];
                
                // 只处理ConvexHullShape类型
                if (shape.Type != CollisionShapeType.ConvexHull)
                {
                    continue;
                }
                
                ConvexHullShape convexHullShape = (ConvexHullShape)shape;
                
                // 创建一个新的GameObject
                GameObject hullObject = new GameObject($"NavMeshHull_{i}");
                hullObject.transform.SetParent(navMeshVisualization.transform);
                hullObject.transform.position = shape.Position;
                hullObject.transform.rotation = shape.Rotation;
                
                // 添加MeshFilter和MeshRenderer组件
                MeshFilter meshFilter = hullObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = hullObject.AddComponent<MeshRenderer>();
                
                // 创建一个简单的Mesh来可视化凸包
                Mesh mesh = CreateConvexHullMesh(convexHullShape);
                meshFilter.mesh = mesh;
                
                // 设置材质
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                meshRenderer.material = material;
            }
        }
        
        /// <summary>
        /// 根据ConvexHullShape创建Mesh
        /// </summary>
        private static Mesh CreateConvexHullMesh(ConvexHullShape convexHullShape)
        {
            Mesh mesh = new Mesh();
            
            // 获取凸包的顶点
            Vector3[] vertices = convexHullShape.GetVertices();
            
            // 获取凸包的面索引
            int[] faceIndices = convexHullShape.GetFaces();
            
            // 获取凸包的面法线用于确定面的顶点顺序
            Vector3[] faceNormals = convexHullShape.GetFaceNormals();
            
            // 创建三角形索引
            List<int> triangles = new List<int>();
            
            // ConvexHullShape的GetFaces返回的是展平的面索引数组，每个面的顶点数不固定
            // 这里假设每个面至少有3个顶点，并且面与面之间没有分隔符
            // 实际使用时需要根据ConvexHullShape的具体实现来处理
            int i = 0;
            while (i < faceIndices.Length)
            {
                // 简单实现：假设每个面有3个顶点（三角形）
                // 实际的ConvexHullShape可能需要更复杂的处理
                if (i + 2 < faceIndices.Length)
                {
                    triangles.Add(faceIndices[i]);
                    triangles.Add(faceIndices[i + 1]);
                    triangles.Add(faceIndices[i + 2]);
                    i += 3;
                }
                else
                {
                    break;
                }
            }
            
            // 设置Mesh属性
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
    }
}
