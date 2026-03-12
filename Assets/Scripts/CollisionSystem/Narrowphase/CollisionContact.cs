using UnityEngine;
using CollisionSystem.Shape;

namespace CollisionSystem.Narrowphase
{
    /// <summary>
    /// 碰撞接触信息类
    /// 存储两个碰撞体之间的碰撞信息
    /// </summary>
    public struct CollisionContact
    {
        /// <summary>
        /// 碰撞体A
        /// </summary>
        public CollisionShape ShapeA { get; private set; }
        
        /// <summary>
        /// 碰撞体B
        /// </summary>
        public CollisionShape ShapeB { get; private set; }
        
        /// <summary>
        /// 碰撞点（在碰撞体A上）
        /// </summary>
        public Vector3 PointA { get; private set; }
        
        /// <summary>
        /// 碰撞点（在碰撞体B上）
        /// </summary>
        public Vector3 PointB { get; private set; }
        
        /// <summary>
        /// 碰撞法线（从碰撞体B指向碰撞体A）
        /// </summary>
        public Vector3 Normal { get; private set; }
        
        /// <summary>
        /// 穿透深度
        /// </summary>
        public float PenetrationDepth { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CollisionContact(
            CollisionShape shapeA,
            CollisionShape shapeB,
            Vector3 pointA,
            Vector3 pointB,
            Vector3 normal,
            float penetrationDepth
        )
        {
            ShapeA = shapeA;
            ShapeB = shapeB;
            PointA = pointA;
            PointB = pointB;
            Normal = normal;
            PenetrationDepth = penetrationDepth;
        }
        
        /// <summary>
        /// 更新碰撞信息
        /// </summary>
        public void Update(
            Vector3 pointA,
            Vector3 pointB,
            Vector3 normal,
            float penetrationDepth
        )
        {
            PointA = pointA;
            PointB = pointB;
            Normal = normal;
            PenetrationDepth = penetrationDepth;
        }
        
        /// <summary>
        /// 交换碰撞体A和B
        /// </summary>
        public CollisionContact Swap()
        {
            return new CollisionContact(
                ShapeB,
                ShapeA,
                PointB,
                PointA,
                -Normal,
                PenetrationDepth
            );
        }
        
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                "CollisionContact: {0} - {1}, Normal: {2}, Penetration: {3}",
                ShapeA.GameObject != null ? ShapeA.GameObject.name : "ShapeA",
                ShapeB.GameObject != null ? ShapeB.GameObject.name : "ShapeB",
                Normal.ToString(),
                PenetrationDepth.ToString()
            );
        }
    }
}
