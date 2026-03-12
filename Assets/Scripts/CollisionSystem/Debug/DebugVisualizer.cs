using UnityEngine;
using System.Collections.Generic;
using CollisionSystem.Shape;
using CollisionSystem.Broadphase;
using CollisionSystem.Midphase;
using CollisionSystem.Narrowphase;

namespace CollisionSystem.Debug
{
    /// <summary>
    /// 调试可视化工具类
    /// 用于绘制碰撞检测系统的各种调试信息
    /// </summary>
    public static class DebugVisualizer
    {
        #region 颜色配置
        private static readonly Color AABB_COLOR = Color.yellow;
        private static readonly Color OCTREE_NODE_COLOR = new Color(0.5f, 0.5f, 1f, 0.3f);
        private static readonly Color OCTREE_LEAF_COLOR = new Color(0.5f, 1f, 0.5f, 0.3f);
        private static readonly Color BVH_NODE_COLOR = new Color(1f, 0.5f, 0.5f, 0.3f);
        private static readonly Color BVH_LEAF_COLOR = new Color(1f, 0.7f, 0.3f, 0.3f);
        private static readonly Color COLLISION_CONTACT_COLOR = Color.red;
        private static readonly Color COLLISION_NORMAL_COLOR = Color.green;
        private static readonly Color SHAPE_FACE_COLOR = new Color(0f, 1f, 1f, 0.2f);
        private static readonly Color SHAPE_EDGE_COLOR = new Color(1f, 1f, 0f, 0.5f);
        private static readonly Color SHAPE_FACE_NORMAL_COLOR = new Color(0f, 1f, 0f, 0.7f);
        private static readonly Color COLLIDING_PAIR_COLOR = Color.red;
        #endregion

        #region Octree可视化
        /// <summary>
        /// 绘制八叉树结构
        /// </summary>
        public static void DrawOctree(Octree octree)
        {
            if (octree == null || octree.Root == null)
                return;

            DrawOctreeNode(octree.Root, 0);
        }

        /// <summary>
        /// 递归绘制八叉树节点，根据深度使用不同颜色
        /// </summary>
        private static void DrawOctreeNode(OctreeNode node, int depth)
        {
            if (node == null)
                return;

            // 根据节点深度生成不同的颜色，使不同层级清晰可见
            Color color = GetColorByDepth(depth, node.IsLeaf);
            DrawBounds(node.Bounds, color);

            // 如果不是叶子节点，递归绘制子节点
            if (!node.IsLeaf)
            {
                foreach (var child in node.Children)
                {
                    DrawOctreeNode(child, depth + 1);
                }
            }

            // 绘制节点中的物体
            foreach (var shape in node.Objects)
            {
                DrawAABB(shape.AABB, AABB_COLOR);
            }
        }
        
        /// <summary>
        /// 根据深度获取不同的颜色
        /// </summary>
        private static Color GetColorByDepth(int depth, bool isLeaf)
        {
            // 为不同深度生成不同的颜色
            float hue = (depth % 6) / 6f; // 使用6种基本色相循环
            float saturation = 0.7f;
            float value = 1.0f - (depth * 0.1f); // 深度越深，颜色越暗
            
            Color baseColor = Color.HSVToRGB(hue, saturation, value);
            
            // 叶子节点使用半透明颜色，内部节点使用不透明颜色
            float alpha = isLeaf ? 0.3f : 0.8f;
            
            return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }
        #endregion

        #region BVH可视化
        /// <summary>
        /// 绘制BVH结构
        /// </summary>
        public static void DrawBVH(BVH bvh)
        {
            if (bvh == null || bvh.Root == null)
                return;

            DrawBVHNode(bvh.Root);
        }

        /// <summary>
        /// 递归绘制BVH节点
        /// </summary>
        private static void DrawBVHNode(BVHNode node)
        {
            if (node == null)
                return;

            // 根据是否为叶子节点选择不同的颜色
            Color color = node.IsLeaf ? BVH_LEAF_COLOR : BVH_NODE_COLOR;
            DrawBounds(node.AABB, color);

            // 如果不是叶子节点，递归绘制左右子节点
            if (!node.IsLeaf)
            {
                DrawBVHNode(node.Left);
                DrawBVHNode(node.Right);
            }

            // 绘制叶子节点中的物体
            if (node.IsLeaf)
            {
                DrawAABB(node.Object.AABB, AABB_COLOR);
                DrawShapeDetails(node.Object);
            }
        }
        #endregion

        #region 碰撞结果可视化
        /// <summary>
        /// 绘制碰撞接触点
        /// </summary>
        public static void DrawCollisionContacts(IReadOnlyList<CollisionContact> contacts)
        {
            if (contacts == null)
                return;

            foreach (var contact in contacts)
            {
                DrawCollisionContact(contact);
                DrawShapePairDetails(contact.ShapeA, contact.ShapeB);
            }
        }

        /// <summary>
        /// 绘制单个碰撞接触点
        /// </summary>
        private static void DrawCollisionContact(CollisionContact contact)
        {
            // 绘制碰撞点
            Gizmos.color = COLLISION_CONTACT_COLOR;
            Gizmos.DrawSphere(contact.PointA, 0.1f);
            Gizmos.DrawSphere(contact.PointB, 0.1f);

            // 绘制碰撞法线
            Gizmos.color = COLLISION_NORMAL_COLOR;
            Gizmos.DrawLine(contact.PointA, contact.PointA + contact.Normal * 0.5f);

            // 绘制穿透深度指示器
            Gizmos.color = Color.magenta;
            Vector3 penetrationVector = contact.Normal * contact.PenetrationDepth;
            Gizmos.DrawLine(contact.PointA, contact.PointA + penetrationVector);
            Gizmos.DrawSphere(contact.PointA + penetrationVector, 0.05f);
        }

        /// <summary>
        /// 绘制碰撞对的详细信息
        /// </summary>
        private static void DrawShapePairDetails(CollisionShape shapeA, CollisionShape shapeB)
        {
            // 绘制两个形状的AABB
            DrawAABB(shapeA.AABB, Color.cyan);
            DrawAABB(shapeB.AABB, Color.cyan);

            // 绘制两个形状的详细信息
            DrawShapeDetails(shapeA);
            DrawShapeDetails(shapeB);

            // 绘制连接两个形状中心的线
            Gizmos.color = COLLIDING_PAIR_COLOR;
            Gizmos.DrawLine(shapeA.AABB.center, shapeB.AABB.center);
        }
        #endregion

        #region 形状可视化
        /// <summary>
        /// 绘制形状的详细信息
        /// </summary>
        private static void DrawShapeDetails(CollisionShape shape)
        {
            if (shape == null)
                return;

            // 绘制形状的AABB
            DrawAABB(shape.AABB, AABB_COLOR);

            // 尝试获取形状的顶点、面、边和面法线
            try
            {
                // 绘制面
                var faceIndices = shape.GetFaces();
                var vertices = shape.GetVertices();
                
                if (faceIndices != null && vertices != null)
                {
                    Gizmos.color = SHAPE_FACE_COLOR;
                    
                    // 根据形状类型处理不同的面数据格式
                    switch (shape.Type)
                    {
                        case CollisionShapeType.Box:
                            // 立方体：每个面4个顶点索引
                            for (int i = 0; i < faceIndices.Length; i += 4)
                            {
                                if (i + 3 < faceIndices.Length)
                                {
                                    Vector3 v0 = vertices[faceIndices[i]];
                                    Vector3 v1 = vertices[faceIndices[i + 1]];
                                    Vector3 v2 = vertices[faceIndices[i + 2]];
                                    Vector3 v3 = vertices[faceIndices[i + 3]];
                                    
                                    // 绘制四边形
                                    DrawQuad(v0, v1, v2, v3);
                                }
                            }
                            break;
                            
                        case CollisionShapeType.Sphere:
                            // 球体：每个三角形3个顶点索引
                            for (int i = 0; i < faceIndices.Length; i += 3)
                            {
                                if (i + 2 < faceIndices.Length)
                                {
                                    Vector3 v0 = vertices[faceIndices[i]];
                                    Vector3 v1 = vertices[faceIndices[i + 1]];
                                    Vector3 v2 = vertices[faceIndices[i + 2]];
                                    
                                    // 绘制三角形
                                    DrawTriangle(v0, v1, v2);
                                }
                            }
                            break;
                            
                        case CollisionShapeType.ConvexHull:
                            // 凸多面体：每个面3个或更多顶点索引
                            // 这里假设面索引是按三角形存储的
                            for (int i = 0; i < faceIndices.Length; i += 3)
                            {
                                if (i + 2 < faceIndices.Length)
                                {
                                    Vector3 v0 = vertices[faceIndices[i]];
                                    Vector3 v1 = vertices[faceIndices[i + 1]];
                                    Vector3 v2 = vertices[faceIndices[i + 2]];
                                    
                                    // 绘制三角形
                                    DrawTriangle(v0, v1, v2);
                                }
                            }
                            break;
                    }
                }

                // 绘制边
                var edges = shape.GetEdges();
                if (edges != null && vertices != null)
                {
                    Gizmos.color = SHAPE_EDGE_COLOR;
                    foreach (var edge in edges)
                    {
                        if (edge.StartIndex < vertices.Length && edge.EndIndex < vertices.Length)
                        {
                            Vector3 v1 = vertices[edge.StartIndex];
                            Vector3 v2 = vertices[edge.EndIndex];
                            Gizmos.DrawLine(v1, v2);
                        }
                    }
                }

                // 绘制面法线
                var faceNormals = shape.GetFaceNormals();
                if (faceNormals != null && vertices != null && faceIndices != null)
                {
                    Gizmos.color = SHAPE_FACE_NORMAL_COLOR;
                    for (int i = 0; i < faceNormals.Length; i++)
                    {
                        // 简单实现：使用顶点的平均值作为面中心
                        // 实际应用中可能需要更精确的面中心计算
                        Vector3 faceCenter = Vector3.zero;
                        int vertexCount = 0;
                        
                        // 根据不同形状类型计算面中心
                        switch (shape.Type)
                        {
                            case CollisionShapeType.Box:
                                // 立方体有6个面，每个面4个顶点
                                if (i < 6)
                                {
                                    int baseIndex = i * 4;
                                    if (baseIndex + 3 < faceIndices.Length)
                                    {
                                        faceCenter = (vertices[faceIndices[baseIndex]] + 
                                                    vertices[faceIndices[baseIndex + 1]] + 
                                                    vertices[faceIndices[baseIndex + 2]] + 
                                                    vertices[faceIndices[baseIndex + 3]]) / 4;
                                        vertexCount = 4;
                                    }
                                }
                                break;
                                
                            case CollisionShapeType.Sphere:
                                // 球体的面法线指向球心方向，使用顶点作为面中心
                                if (i < vertices.Length)
                                {
                                    faceCenter = vertices[i];
                                    vertexCount = 1;
                                }
                                break;
                                
                            case CollisionShapeType.ConvexHull:
                                // 凸多面体：假设每个面有3个顶点
                                int faceStartIndex = i * 3;
                                if (faceStartIndex + 2 < faceIndices.Length)
                                {
                                    faceCenter = (vertices[faceIndices[faceStartIndex]] + 
                                                vertices[faceIndices[faceStartIndex + 1]] + 
                                                vertices[faceIndices[faceStartIndex + 2]]) / 3;
                                    vertexCount = 3;
                                }
                                break;
                        }
                        
                        if (vertexCount > 0)
                        {
                            // 绘制面法线
                            Gizmos.DrawLine(faceCenter, faceCenter + faceNormals[i] * 0.3f);
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // 忽略绘制错误，确保调试不会中断主程序
            }
        }

        /// <summary>
        /// 绘制三角形面
        /// </summary>
        private static void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v0);
        }
        
        /// <summary>
        /// 绘制四边形
        /// </summary>
        private static void DrawQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v3);
            Gizmos.DrawLine(v3, v0);
        }
        #endregion

        #region 基本绘图方法
        /// <summary>
        /// 绘制AABB边界框
        /// </summary>
        private static void DrawAABB(Bounds bounds, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        /// <summary>
        /// 绘制边界框
        /// </summary>
        private static void DrawBounds(Bounds bounds, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }

        /// <summary>
        /// 绘制文本标签
        /// </summary>
        public static void DrawText(Vector3 position, string text, Color color)
        {
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(position, text);
        }
        #endregion

        #region 高级可视化方法
        /// <summary>
        /// 绘制所有碰撞体的AABB
        /// </summary>
        public static void DrawAllShapeAABBs(List<CollisionShape> shapes)
        {
            foreach (var shape in shapes)
            {
                DrawAABB(shape.AABB, AABB_COLOR);
            }
        }

        /// <summary>
        /// 绘制所有碰撞对
        /// </summary>
        public static void DrawAllCollisionPairs(List<ShapePair> pairs)
        {
            foreach (var pair in pairs)
            {
                DrawShapePairDetails(pair.ShapeA, pair.ShapeB);
            }
        }

        /// <summary>
        /// 绘制Octree中的所有物体
        /// </summary>
        public static void DrawOctreeObjects(Octree octree)
        {
            if (octree == null)
                return;

            List<CollisionShape> allObjects = new List<CollisionShape>();
            CollectOctreeObjects(octree.Root, allObjects);

            foreach (var shape in allObjects)
            {
                DrawAABB(shape.AABB, Color.green);
            }
        }

        /// <summary>
        /// 收集Octree中的所有物体
        /// </summary>
        private static void CollectOctreeObjects(OctreeNode node, List<CollisionShape> result)
        {
            if (node == null)
                return;

            result.AddRange(node.Objects);

            if (!node.IsLeaf)
            {
                foreach (var child in node.Children)
                {
                    CollectOctreeObjects(child, result);
                }
            }
        }
        #endregion
    }
}