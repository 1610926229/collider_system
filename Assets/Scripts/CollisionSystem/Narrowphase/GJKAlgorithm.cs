using UnityEngine;
using System.Collections.Generic;
using CollisionSystem.Shape;

namespace CollisionSystem.Narrowphase
{
    /// <summary>
    /// GJK（Gilbert-Johnson-Keerthi）算法类
    /// 用于检测两个凸体是否相交
    /// </summary>
    public static class GJKAlgorithm
    {
        /// <summary>
        /// 最大迭代次数
        /// </summary>
        private const int MaxIterations = 32;
        
        /// <summary>
        /// 精度阈值
        /// </summary>
        private const float Epsilon = 1e-6f;
        
        /// <summary>
        /// 检测两个凸体是否相交
        /// </summary>
        public static bool DetectCollision(CollisionShape shapeA, CollisionShape shapeB)
        {
            // 初始化单纯形
            List<Vector3> simplex = new List<Vector3>();
            
            // 选择初始搜索方向（从形状A到形状B的向量）
            Vector3 direction = shapeB.Position - shapeA.Position;
            
            // 如果方向为零向量，选择一个任意方向
            if (direction.sqrMagnitude < Epsilon)
            {
                direction = Vector3.right;
            }
            
            // 获取第一个支持点
            Vector3 support = Support(shapeA, shapeB, direction);
            simplex.Add(support);
            
            // 反转搜索方向
            direction = -direction;
            
            // 迭代寻找单纯形
            for (int i = 0; i < MaxIterations; i++)
            {
                // 获取新的支持点
                support = Support(shapeA, shapeB, direction);
                
                // 如果新的支持点在搜索方向上的投影小于等于零，说明两个形状不相交
                if (Vector3.Dot(support, direction) <= 0)
                {
                    return false;
                }
                
                // 将新的支持点添加到单纯形
                simplex.Add(support);
                
                // 更新单纯形并判断是否包含原点
                if (UpdateSimplex(ref simplex, ref direction))
                {
                    return true;
                }
            }
            
            // 迭代次数过多，默认认为相交
            return true;
        }
        
        /// <summary>
        /// 计算两个形状在指定方向上的支持点之差
        /// </summary>
        private static Vector3 Support(CollisionShape shapeA, CollisionShape shapeB, Vector3 direction)
        {
            // 获取形状A在方向d上的支持点
            Vector3 supportA = shapeA.Support(direction);
            
            // 获取形状B在方向-d上的支持点
            Vector3 supportB = shapeB.Support(-direction);
            
            // 返回 Minkowski 差
            return supportA - supportB;
        }
        
        /// <summary>
        /// 更新单纯形并判断是否包含原点
        /// </summary>
        private static bool UpdateSimplex(ref List<Vector3> simplex, ref Vector3 direction)
        {
            switch (simplex.Count)
            {
                case 2: return UpdateLineSimplex(ref simplex, ref direction);
                case 3: return UpdateTriangleSimplex(ref simplex, ref direction);
                case 4: return UpdateTetrahedronSimplex(ref simplex, ref direction);
                default: return false;
            }
        }
        
        /// <summary>
        /// 更新线段单纯形
        /// </summary>
        private static bool UpdateLineSimplex(ref List<Vector3> simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[1];
            Vector3 b = simplex[0];
            
            Vector3 ab = b - a;
            Vector3 ao = -a;
            
            // 计算投影
            float abao = Vector3.Dot(ab, ao);
            
            if (abao > 0)
            {
                // 原点在ab和ao之间
                direction = Vector3.Cross(Vector3.Cross(ab, ao), ab).normalized;
            }
            else
            {
                // 原点在ao方向
                direction = ao;
                simplex.RemoveAt(1);
            }
            
            return false;
        }
        
        /// <summary>
        /// 更新三角形单纯形
        /// </summary>
        private static bool UpdateTriangleSimplex(ref List<Vector3> simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[2];
            Vector3 b = simplex[1];
            Vector3 c = simplex[0];
            
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ao = -a;
            
            Vector3 abc = Vector3.Cross(ab, ac);
            
            // 计算法线
            Vector3 abPerp = Vector3.Cross(abc, ab);
            if (Vector3.Dot(abPerp, ao) > 0)
            {
                // 原点在abPerp方向
                simplex.RemoveAt(0);
                direction = abPerp.normalized;
                return false;
            }
            
            Vector3 acPerp = Vector3.Cross(ac, abc);
            if (Vector3.Dot(acPerp, ao) > 0)
            {
                // 原点在acPerp方向
                simplex.RemoveAt(1);
                direction = acPerp.normalized;
                return false;
            }
            
            // 检查原点在三角形的哪一侧
            float abcAo = Vector3.Dot(abc, ao);
            if (abcAo > 0)
            {
                // 原点在正面
                direction = abc.normalized;
            }
            else
            {
                // 原点在反面
                direction = -abc.normalized;
            }
            
            return false;
        }
        
        /// <summary>
        /// 更新四面体单纯形
        /// </summary>
        private static bool UpdateTetrahedronSimplex(ref List<Vector3> simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[3];
            Vector3 b = simplex[2];
            Vector3 c = simplex[1];
            Vector3 d = simplex[0];
            
            Vector3 ao = -a;
            
            // 检查四个面
            Vector3 abc = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(abc, ao) > 0)
            {
                // 原点在abc面的正面
                simplex.RemoveAt(0);
                direction = abc.normalized;
                return false;
            }
            
            Vector3 acd = Vector3.Cross(c - a, d - a);
            if (Vector3.Dot(acd, ao) > 0)
            {
                // 原点在acd面的正面
                simplex.RemoveAt(2);
                direction = acd.normalized;
                return false;
            }
            
            Vector3 adb = Vector3.Cross(d - a, b - a);
            if (Vector3.Dot(adb, ao) > 0)
            {
                // 原点在adb面的正面
                simplex.RemoveAt(1);
                direction = adb.normalized;
                return false;
            }
            
            Vector3 bcd = Vector3.Cross(c - b, d - b);
            if (Vector3.Dot(bcd, -(b)) > 0)
            {
                // 原点在bcd面的正面
                simplex.RemoveAt(3);
                direction = bcd.normalized;
                return false;
            }
            
            // 原点在四面体内部，两个形状相交
            return true;
        }
    }
}
