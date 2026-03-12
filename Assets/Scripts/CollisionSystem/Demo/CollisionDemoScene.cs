using UnityEngine;

namespace CollisionSystem.Demo
{
    /// <summary>
    /// 碰撞演示场景控制器
    /// 用于设置演示场景和控制碰撞生成
    /// </summary>
    public class CollisionDemoScene : MonoBehaviour
    {
        [Header("场景设置")]
        [Tooltip("地面尺寸")]
        public Vector3 groundSize = new Vector3(20, 1, 20);
        
        [Tooltip("地面位置")]
        public Vector3 groundPosition = new Vector3(0, -0.5f, 0);
        
        [Tooltip("碰撞系统配置")]
        public CollisionSystem collisionSystem;
        
        [Tooltip("场景生成器配置")]
        public CollisionSceneGenerator sceneGenerator;
        
        private void Start()
        {
            SetupDemoScene();
        }
        
        /// <summary>
        /// 设置演示场景
        /// </summary>
        public void SetupDemoScene()
        {
            // 创建地面
            CreateGround();
            
            // 如果没有碰撞系统管理器，创建一个
            if (collisionSystem == null)
            {
                GameObject collisionSystemObj = new GameObject("CollisionSystemManager");
                collisionSystem = collisionSystemObj.AddComponent<CollisionSystem>();
                
                // 启用调试可视化以便查看碰撞
                collisionSystem.enableDebug = true;
            }
            
            // 如果没有场景生成器，创建一个
            if (sceneGenerator == null)
            {
                GameObject generatorObj = new GameObject("CollisionSceneGenerator");
                sceneGenerator = generatorObj.AddComponent<CollisionSceneGenerator>();
            }
        }
        
        /// <summary>
        /// 创建地面
        /// </summary>
        private void CreateGround()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                return; // 地面已存在
            }
            
            ground = new GameObject("Ground");
            ground.transform.position = groundPosition;
            ground.transform.localScale = groundSize;
            
            // 添加碰撞盒
            BoxCollider groundCollider = ground.AddComponent<BoxCollider>();
            groundCollider.isTrigger = false;
            
            // 添加MeshRenderer
            MeshRenderer groundRenderer = ground.AddComponent<MeshRenderer>();
            groundRenderer.material = new Material(Shader.Find("Standard"));
            groundRenderer.material.color = Color.green;
            groundRenderer.material.SetFloat("_Glossiness", 0.1f);
            groundRenderer.material.SetColor("_SpecColor", Color.gray);
            
            // 添加到碰撞系统
            CollisionSystemBehaviour groundCollision = ground.AddComponent<CollisionSystemBehaviour>();
            groundCollision.isStatic = true;
            groundCollision.autoRegister = true;
        }
        
        /// <summary>
        /// 重新生成碰撞场景
        /// 可以通过UI按钮调用
        /// </summary>
        public void RegenerateScene()
        {
            if (sceneGenerator != null)
            {
                sceneGenerator.RegenerateScene();
            }
        }
        
        /// <summary>
        /// 切换调试可视化
        /// </summary>
        public void ToggleDebugVisualization()
        {
            if (collisionSystem != null)
            {
                collisionSystem.enableDebug = !collisionSystem.enableDebug;
            }
        }
    }
}