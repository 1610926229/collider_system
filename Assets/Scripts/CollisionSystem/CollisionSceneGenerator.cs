using UnityEngine;
using CollisionSystem.Shape;
using System.Collections.Generic;

namespace CollisionSystem
{
    /// <summary>
    /// 碰撞场景生成器
    /// 用于自动生成静态和动态碰撞体并施加随机力模拟碰撞
    /// </summary>
    public class CollisionSceneGenerator : MonoBehaviour
    {
        [Header("物体数量设置")]
        [Tooltip("静态物体数量")]
        public int staticObjectCount = 5;
        
        [Tooltip("动态物体数量")]
        public int dynamicObjectCount = 10;
        
        [Tooltip("静态动态物体数量（移速为0的动态物体）")]
        public int staticDynamicObjectCount = 3;
        
        [Header("碰撞盒类型设置")]
        [Tooltip("静态物体碰撞盒类型")]
        public ColliderType staticColliderType = ColliderType.Box;
        
        [Tooltip("动态物体碰撞盒类型")]
        public ColliderType dynamicColliderType = ColliderType.Sphere;
        
        [Header("生成区域设置")]
        [Tooltip("生成区域边界")]
        public Vector3 spawnBounds = new Vector3(10, 5, 10);
        
        [Tooltip("静态物体Y轴偏移")]
        public float staticYOffset = 0.5f;
        
        [Tooltip("动态物体Y轴起始高度")]
        public float dynamicYStart = 10f;
        
        [Header("力设置")]
        [Tooltip("随机力大小范围")]
        public Vector2 forceRange = new Vector2(10, 50);
        
        [Tooltip("是否应用随机扭矩")]
        public bool applyRandomTorque = true;
        
        [Tooltip("随机扭矩大小范围")]
        public Vector2 torqueRange = new Vector2(5, 20);
        
        [Header("物体配置")]
        [Tooltip("物体大小范围")]
        public Vector2 objectSizeRange = new Vector2(0.5f, 2f);
        
        [Tooltip("材质")]
        public Material objectMaterial;
        
        // 用于管理生成的物体，避免使用标签系统
        private List<GameObject> _staticObjects = new List<GameObject>();
        private List<GameObject> _dynamicObjects = new List<GameObject>();
        
        /// <summary>
        /// 碰撞盒类型枚举
        /// </summary>
        public enum ColliderType
        {
            Box,
            Sphere,
            Mesh
        }
        
        #region Unity生命周期
        private void Start()
        {
            // 生成静态物体
            GenerateStaticObjects();
            
            // 生成动态物体
            GenerateDynamicObjects();
            
            // 构建碰撞系统的空间结构
            CollisionSystem.Instance.BuildSpatialStructures();
        }
        
        private void Update()
        {
            // 空格键重新生成场景
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RegenerateScene();
            }
        }
        #endregion
        
        /// <summary>
        /// 生成静态物体
        /// </summary>
        private void GenerateStaticObjects()
        {
            for (int i = 0; i < staticObjectCount; i++)
            {
                // 生成随机位置
                Vector3 position = new Vector3(
                    Random.Range(-spawnBounds.x, spawnBounds.x),
                    staticYOffset,  // 静态物体放在地面上
                    Random.Range(-spawnBounds.z, spawnBounds.z)
                );
                
                // 创建物体
                GameObject obj = CreateColliderObject("StaticObject_" + i, position, Quaternion.identity, staticColliderType, true);
                
                // 添加碰撞系统行为组件
                CollisionSystemBehaviour behaviour = obj.AddComponent<CollisionSystemBehaviour>();
                behaviour.isStatic = true;
                // 手动调用注册方法，确保碰撞体被添加到碰撞系统
                behaviour.RegisterToCollisionSystem();
                
                // 将物体添加到列表中，便于后续清理
                _staticObjects.Add(obj);
            }
        }
        
        /// <summary>
        /// 生成动态物体
        /// </summary>
        private void GenerateDynamicObjects()
        {
            for (int i = 0; i < dynamicObjectCount; i++)
            {
                // 生成随机位置
                Vector3 position = new Vector3(
                    Random.Range(-spawnBounds.x, spawnBounds.x),
                    dynamicYStart,  // 动态物体从空中落下
                    Random.Range(-spawnBounds.z, spawnBounds.z)
                );
                
                // 创建物体
                GameObject obj = CreateColliderObject("DynamicObject_" + i, position, Random.rotation, dynamicColliderType, false);
                
                // 添加Rigidbody用于物理模拟
                Rigidbody rb = obj.AddComponent<Rigidbody>();
                rb.useGravity = true;
                
                // 对前staticDynamicObjectCount个动态物体不施加力，使其移速为0
                if (i >= staticDynamicObjectCount)
                {
                    // 施加随机力
                    ApplyRandomForce(rb);
                }
                else
                {
                    // 确保这些物体没有初始速度和扭矩
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    // 设置这些物体的颜色为蓝色，以便区分
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.blue;
                    }
                }
                
                // 添加碰撞系统行为组件
                CollisionSystemBehaviour behaviour = obj.AddComponent<CollisionSystemBehaviour>();
                behaviour.isStatic = false;
                // 手动调用注册方法，确保碰撞体被添加到碰撞系统
                behaviour.RegisterToCollisionSystem();
                
                // 将物体添加到列表中，便于后续清理
                _dynamicObjects.Add(obj);
            }
        }
        
        /// <summary>
        /// 创建带有碰撞盒的物体
        /// </summary>
        private GameObject CreateColliderObject(string name, Vector3 position, Quaternion rotation, ColliderType colliderType, bool isStatic)
        {
            // 创建物体
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            // 设置随机大小
            float size = Random.Range(objectSizeRange.x, objectSizeRange.y);
            obj.transform.localScale = new Vector3(size, size, size);
            
            // 添加碰撞盒
            switch (colliderType)
            {
                case ColliderType.Box:
                    obj.AddComponent<BoxCollider>();
                    break;
                    
                case ColliderType.Sphere:
                    obj.AddComponent<SphereCollider>();
                    break;
                    
                case ColliderType.Mesh:
                    // 创建一个简单的立方体网格
                    MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                    meshFilter.mesh = CreateCubeMesh();
                    obj.AddComponent<MeshCollider>().convex = true;
                    break;
            }
            
            // 添加MeshRenderer和材质
            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            if (objectMaterial != null)
            {
                renderer.material = objectMaterial;
            }
            else
            {
                // 使用默认材质并设置颜色
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = isStatic ? Color.gray : Color.red;
            }
            
            return obj;
        }
        
        /// <summary>
        /// 创建立方体网格
        /// </summary>
        private Mesh CreateCubeMesh()
        {
            Mesh mesh = new Mesh();
            
            // 顶点
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
            
            // 三角形
            int[] triangles = new int[]
            {
                0, 2, 1, 0, 3, 2,  // 前面
                4, 5, 6, 4, 6, 7,  // 后面
                0, 1, 5, 0, 5, 4,  // 下面
                2, 3, 7, 2, 7, 6,  // 上面
                0, 4, 7, 0, 7, 3,  // 左面
                1, 2, 6, 1, 6, 5   // 右面
            };
            
            // UV
            Vector2[] uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            
            // 设置网格属性
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        /// <summary>
        /// 对刚体施加随机力
        /// </summary>
        private void ApplyRandomForce(Rigidbody rb)
        {
            // 生成随机力方向
            Vector3 forceDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-1f, 1f)
            ).normalized;
            
            // 生成随机力大小
            float forceMagnitude = Random.Range(forceRange.x, forceRange.y);
            
            // 施加力
            rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            
            // 施加随机扭矩
            if (applyRandomTorque)
            {
                Vector3 torqueDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized;
                
                float torqueMagnitude = Random.Range(torqueRange.x, torqueRange.y);
                
                rb.AddTorque(torqueDirection * torqueMagnitude, ForceMode.Impulse);
            }
        }
        
        /// <summary>
        /// 重新生成场景
        /// </summary>
        public void RegenerateScene()
        {
            // 清除现有物体
            ClearScene();
            
            // 重新生成场景
            GenerateStaticObjects();
            GenerateDynamicObjects();
            
            // 重新构建碰撞系统的空间结构
            CollisionSystem.Instance.BuildSpatialStructures();
        }
        
        /// <summary>
        /// 清除场景中的物体
        /// </summary>
        private void ClearScene()
        {
            // 销毁所有静态物体
            foreach (GameObject obj in _staticObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _staticObjects.Clear();
            
            // 销毁所有动态物体
            foreach (GameObject obj in _dynamicObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _dynamicObjects.Clear();
        }
        
        private void OnDestroy()
        {
            // 场景关闭时清理生成的物体
            ClearScene();
        }
    }
}