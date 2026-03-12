using UnityEngine;
using CollisionSystem.Shape;

namespace CollisionSystem.Narrowphase
{
    /// <summary>
    /// 碰撞检测器类
    /// 作为窄阶段碰撞检测的统一入口，负责调用GJK和SAT算法
    /// </summary>
    public class CollisionDetector
    {
        /// <summary>
        /// 检测两个碰撞体是否相交，并计算碰撞信息
        /// </summary>
        /// <param name="shapeA">碰撞体A</param>
        /// <param name="shapeB">碰撞体B</param>
        /// <param name="contact">碰撞信息</param>
        /// <returns>是否发生碰撞</returns>
        public bool DetectCollision(
            CollisionShape shapeA,
            CollisionShape shapeB,
            out CollisionContact contact
        )
        {
            contact = new CollisionContact();
            
            // 首先使用GJK算法快速检测是否相交
            if (!GJKAlgorithm.DetectCollision(shapeA, shapeB))
            {
                // GJK检测不相交，直接返回false
                return false;
            }
            
            // GJK检测相交，使用SAT算法计算精确的碰撞信息
            return SATAlgorithm.DetectCollision(shapeA, shapeB, out contact);
        }
        
        /// <summary>
        /// 仅检测两个碰撞体是否相交，不计算碰撞信息
        /// </summary>
        /// <param name="shapeA">碰撞体A</param>
        /// <param name="shapeB">碰撞体B</param>
        /// <returns>是否发生碰撞</returns>
        public bool DetectCollision(CollisionShape shapeA, CollisionShape shapeB)
        {
            // 仅使用GJK算法检测是否相交
            return GJKAlgorithm.DetectCollision(shapeA, shapeB);
        }
    }
}
