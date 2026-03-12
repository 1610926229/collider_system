using UnityEngine;
using System.Collections.Generic;
using CollisionSystem.Shape;

namespace CollisionSystem.Narrowphase
{
    /// <summary>
    /// SAT（Separating Axis Theorem）算法类
    /// 用于检测两个凸体是否相交，并计算碰撞信息
    /// </summary>
    public static class SATAlgorithm
    {
        /// <summary>
        /// 精度阈值
        /// </summary>
        private const float Epsilon = 1e-6f;
        
        /// <summary>
        /// 检测两个凸体是否相交，并计算碰撞信息
        /// </summary>
        public static bool DetectCollision(
            CollisionShape shapeA,
            CollisionShape shapeB,
            out CollisionContact contact
        )
        {
            contact = new CollisionContact(
                shapeA,
                shapeB,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                0f
            );
            
            // 获取两个形状的所有可能分离轴
            List<Vector3> axes = GetAllAxes(shapeA, shapeB);
            
            // 初始化最小穿透深度和对应的法线
            float minPenetrationDepth = float.MaxValue;
            Vector3 bestAxis = Vector3.zero;
            
            // 检查所有分离轴
            foreach (var axis in axes)
            {
                // 计算两个形状在当前轴上的投影
                float minA, maxA, minB, maxB;
                ProjectShape(shapeA, axis, out minA, out maxA);
                ProjectShape(shapeB, axis, out minB, out maxB);
                
                // 检查投影是否重叠
                if (!Overlap(minA, maxA, minB, maxB))
                {
                    // 找到分离轴，两个形状不相交
                    return false;
                }
                
                // 计算穿透深度
                float penetrationDepth = CalculatePenetrationDepth(minA, maxA, minB, maxB);
                
                // 保存最小的穿透深度和对应的轴
                if (penetrationDepth < minPenetrationDepth)
                {
                    minPenetrationDepth = penetrationDepth;
                    bestAxis = axis;
                }
            }
            
            // 如果没有找到分离轴，两个形状相交
            if (minPenetrationDepth < float.MaxValue)
            {
                // 确保法线方向正确（从形状B指向形状A）
                Vector3 normal = bestAxis;
                Vector3 centerDiff = shapeA.Position - shapeB.Position;
                if (Vector3.Dot(normal, centerDiff) < 0)
                {
                    normal = -normal;
                }
                
                // 计算碰撞点
                Vector3 pointA, pointB;
                if (CalculateContactPoints(shapeA, shapeB, normal, out pointA, out pointB))
                {
                    contact = new CollisionContact(
                        shapeA,
                        shapeB,
                        pointA,
                        pointB,
                        normal,
                        minPenetrationDepth
                    );
                }
                else
                {
                    // 如果无法计算精确的碰撞点，使用形状中心
                    pointA = shapeA.Position;
                    pointB = shapeB.Position;
                    contact = new CollisionContact(
                        shapeA,
                        shapeB,
                        pointA,
                        pointB,
                        normal,
                        minPenetrationDepth
                    );
                }
                
                return true;
            }
            
            return false;
        }
        
        #region 辅助方法
        /// <summary>
        /// 获取两个形状的所有可能分离轴
        /// </summary>
        private static List<Vector3> GetAllAxes(CollisionShape shapeA, CollisionShape shapeB)
        {
            List<Vector3> axes = new List<Vector3>();
            
            // 获取形状A的所有面法线
            Vector3[] normalsA = shapeA.GetFaceNormals();
            foreach (var normal in normalsA)
            {
                if (!IsAxisDuplicate(axes, normal))
                {
                    axes.Add(normal);
                }
            }
            
            // 获取形状B的所有面法线
            Vector3[] normalsB = shapeB.GetFaceNormals();
            foreach (var normal in normalsB)
            {
                if (!IsAxisDuplicate(axes, normal))
                {
                    axes.Add(normal);
                }
            }
            
            // 获取两个形状边的叉积
            Edge[] edgesA = shapeA.GetEdges();
            Edge[] edgesB = shapeB.GetEdges();
            
            Vector3[] verticesA = shapeA.GetVertices();
            Vector3[] verticesB = shapeB.GetVertices();
            
            foreach (var edgeA in edgesA)
            {
                Vector3 edgeAVector = verticesA[edgeA.EndIndex] - verticesA[edgeA.StartIndex];
                
                foreach (var edgeB in edgesB)
                {
                    Vector3 edgeBVector = verticesB[edgeB.EndIndex] - verticesB[edgeB.StartIndex];
                    
                    // 计算边的叉积，得到可能的分离轴
                    Vector3 axis = Vector3.Cross(edgeAVector, edgeBVector).normalized;
                    
                    // 如果叉积不为零向量，且不是重复轴，添加到轴列表
                    if (axis.sqrMagnitude > Epsilon && !IsAxisDuplicate(axes, axis))
                    {
                        axes.Add(axis);
                    }
                }
            }
            
            return axes;
        }
        
        /// <summary>
        /// 检查轴是否重复
        /// </summary>
        private static bool IsAxisDuplicate(List<Vector3> axes, Vector3 axis)
        {
            foreach (var existingAxis in axes)
            {
                // 如果两个轴的点积接近1或-1，说明它们几乎相同
                if (Mathf.Abs(Vector3.Dot(existingAxis, axis)) > 1 - Epsilon)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 将形状投影到指定轴上
        /// </summary>
        private static void ProjectShape(CollisionShape shape, Vector3 axis, out float min, out float max)
        {
            Vector3[] vertices = shape.GetVertices();
            
            if (vertices.Length == 0)
            {
                min = 0f;
                max = 0f;
                return;
            }
            
            // 计算第一个顶点的投影
            min = Vector3.Dot(axis, vertices[0]);
            max = min;
            
            // 计算其他顶点的投影，并更新min和max
            for (int i = 1; i < vertices.Length; i++)
            {
                float projection = Vector3.Dot(axis, vertices[i]);
                if (projection < min)
                {
                    min = projection;
                }
                if (projection > max)
                {
                    max = projection;
                }
            }
        }
        
        /// <summary>
        /// 检查两个投影区间是否重叠
        /// </summary>
        private static bool Overlap(float minA, float maxA, float minB, float maxB)
        {
            return minA <= maxB && minB <= maxA;
        }
        
        /// <summary>
        /// 计算穿透深度
        /// </summary>
        private static float CalculatePenetrationDepth(float minA, float maxA, float minB, float maxB)
        {
            float overlap1 = maxA - minB;
            float overlap2 = maxB - minA;
            return Mathf.Min(overlap1, overlap2);
        }
        
        /// <summary>
        /// 计算碰撞点
        /// </summary>
        private static bool CalculateContactPoints(
            CollisionShape shapeA,
            CollisionShape shapeB,
            Vector3 normal,
            out Vector3 pointA,
            out Vector3 pointB
        )
        {
            pointA = Vector3.zero;
            pointB = Vector3.zero;
            
            // 获取形状A在碰撞法线上的所有顶点
            List<Vector3> verticesA = new List<Vector3>(shapeA.GetVertices());
            List<Vector3> relevantVerticesA = GetRelevantVertices(verticesA, normal);
            
            // 获取形状B在碰撞法线上的所有顶点（法线方向相反）
            List<Vector3> verticesB = new List<Vector3>(shapeB.GetVertices());
            List<Vector3> relevantVerticesB = GetRelevantVertices(verticesB, -normal);
            
            // 如果没有相关顶点，返回false
            if (relevantVerticesA.Count == 0 || relevantVerticesB.Count == 0)
            {
                return false;
            }
            
            // 计算两个形状的相关顶点的中心
            pointA = CalculateCentroid(relevantVerticesA);
            pointB = CalculateCentroid(relevantVerticesB);
            
            return true;
        }
        
        /// <summary>
        /// 获取在指定方向上最相关的顶点
        /// </summary>
        private static List<Vector3> GetRelevantVertices(List<Vector3> vertices, Vector3 direction)
        {
            List<Vector3> relevantVertices = new List<Vector3>();
            
            if (vertices.Count == 0)
            {
                return relevantVertices;
            }
            
            // 找到在指定方向上投影最大的顶点
            float maxProjection = Vector3.Dot(direction, vertices[0]);
            relevantVertices.Add(vertices[0]);
            
            for (int i = 1; i < vertices.Count; i++)
            {
                float projection = Vector3.Dot(direction, vertices[i]);
                
                if (Mathf.Abs(projection - maxProjection) < Epsilon)
                {
                    // 投影相同，添加到相关顶点列表
                    relevantVertices.Add(vertices[i]);
                }
                else if (projection > maxProjection)
                {
                    // 找到更大的投影，更新最大投影并清空列表
                    maxProjection = projection;
                    relevantVertices.Clear();
                    relevantVertices.Add(vertices[i]);
                }
            }
            
            return relevantVertices;
        }
        
        /// <summary>
        /// 计算顶点列表的质心
        /// </summary>
        private static Vector3 CalculateCentroid(List<Vector3> vertices)
        {
            if (vertices.Count == 0)
            {
                return Vector3.zero;
            }
            
            Vector3 centroid = Vector3.zero;
            foreach (var vertex in vertices)
            {
                centroid += vertex;
            }
            
            return centroid / vertices.Count;
        }
        #endregion
    }
}
