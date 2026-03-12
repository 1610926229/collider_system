using UnityEngine;
using CollisionSystem.Shape;
using CollisionSystem.Converter;

namespace CollisionSystem
{
    /// <summary>
    /// CollisionSystem行为组件
    /// 用于将Unity Collider注册到碰撞系统中
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollisionSystemBehaviour : MonoBehaviour
    {
        [Header("碰撞系统配置")]
        [Tooltip("是否为静态碰撞体")]
        public bool isStatic = false;
        
        [Tooltip("是否在启用时自动注册")]
        public bool autoRegister = true;
        
        [Tooltip("是否在禁用时自动注销")]
        public bool autoUnregister = true;
        
        #region 内部变量
        private CollisionShape _collisionShape;
        private Collider _unityCollider;
        #endregion
        
        #region Unity生命周期
        private void Awake()
        {
            _unityCollider = GetComponent<Collider>();
        }
        
        private void OnEnable()
        {
            if (autoRegister)
            {
                RegisterToCollisionSystem();
            }
        }
        
        private void OnDisable()
        {
            if (autoUnregister)
            {
                UnregisterFromCollisionSystem();
            }
        }
        
        private void Update()
        {
            // 如果是动态物体，更新碰撞体的位置和旋转
            if (!isStatic && _collisionShape != null)
            {
                _collisionShape.Position = transform.position;
                _collisionShape.Rotation = transform.rotation;
                _collisionShape.UpdateBounds();
            }
        }
        
        private void OnDestroy()
        {
            // 确保在对象销毁时从碰撞系统中注销
            UnregisterFromCollisionSystem();
        }
        #endregion
        
        #region 公共方法
        /// <summary>
        /// 将碰撞体注册到碰撞系统
        /// </summary>
        public void RegisterToCollisionSystem()
        {
            // 转换Unity Collider为CollisionShape
            _collisionShape = ColliderConverter.ConvertToCollisionShape(_unityCollider, transform);
            
            if (_collisionShape != null)
            {
                // 根据类型注册到碰撞系统
                if (isStatic)
                {
                    CollisionSystem.Instance.RegisterStaticShape(_collisionShape);
                }
                else
                {
                    CollisionSystem.Instance.RegisterDynamicShape(_collisionShape);
                }
            }
        }
        
        /// <summary>
        /// 从碰撞系统注销碰撞体
        /// </summary>
        public void UnregisterFromCollisionSystem()
        {
            if (_collisionShape != null)
            {
                CollisionSystem.Instance.UnregisterShape(_collisionShape);
                _collisionShape = null;
            }
        }
        
        /// <summary>
        /// 获取当前的CollisionShape
        /// </summary>
        public CollisionShape GetCollisionShape()
        {
            return _collisionShape;
        }
        #endregion
    }
}
