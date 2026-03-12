namespace CollisionSystem.Shape
{
    /// <summary>
    /// 形状对结构体
    /// 用于表示两个碰撞体的配对
    /// </summary>
    public struct ShapePair
    {
        /// <summary>
        /// 形状A
        /// </summary>
        public CollisionShape ShapeA;
        
        /// <summary>
        /// 形状B
        /// </summary>
        public CollisionShape ShapeB;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public ShapePair(CollisionShape shapeA, CollisionShape shapeB)
        {
            ShapeA = shapeA;
            ShapeB = shapeB;
        }
        
        /// <summary>
        /// 重写Equals方法
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is ShapePair other)
            {
                return (ShapeA == other.ShapeA && ShapeB == other.ShapeB) || 
                       (ShapeA == other.ShapeB && ShapeB == other.ShapeA);
            }
            return false;
        }
        
        /// <summary>
        /// 重写GetHashCode方法
        /// </summary>
        public override int GetHashCode()
        {
            // 确保形状对的哈希码与顺序无关
            int hashA = ShapeA.GetHashCode();
            int hashB = ShapeB.GetHashCode();
            return hashA < hashB ? (hashA << 16) | hashB : (hashB << 16) | hashA;
        }
    }
}
