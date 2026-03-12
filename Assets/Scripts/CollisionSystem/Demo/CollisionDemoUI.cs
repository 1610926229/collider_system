using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace CollisionSystem.Demo
{
    /// <summary>
    /// 碰撞演示UI管理器
    /// 用于在运行时控制碰撞场景生成器的参数
    /// </summary>
    public class CollisionDemoUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Slider staticObjectSlider;
        public Slider dynamicObjectSlider;
        public Dropdown staticColliderDropdown;
        public Dropdown dynamicColliderDropdown;
        public Slider forceSlider;
        public Button regenerateButton;
        public Button toggleDebugButton;
        
        [Header("引用")]
        public CollisionSceneGenerator sceneGenerator;
        public CollisionSystem collisionSystem;
        
        private void Start()
        {
            SetupUI();
        }
        
        /// <summary>
        /// 设置UI组件
        /// </summary>
        private void SetupUI()
        {
            if (staticObjectSlider != null)
            {
                staticObjectSlider.minValue = 0;
                staticObjectSlider.maxValue = 20;
                staticObjectSlider.value = sceneGenerator.staticObjectCount;
                staticObjectSlider.onValueChanged.AddListener(OnStaticObjectCountChanged);
            }
            
            if (dynamicObjectSlider != null)
            {
                dynamicObjectSlider.minValue = 0;
                dynamicObjectSlider.maxValue = 30;
                dynamicObjectSlider.value = sceneGenerator.dynamicObjectCount;
                dynamicObjectSlider.onValueChanged.AddListener(OnDynamicObjectCountChanged);
            }
            
            if (staticColliderDropdown != null)
            {
                staticColliderDropdown.AddOptions(System.Enum.GetNames(typeof(CollisionSceneGenerator.ColliderType)).ToList());
                staticColliderDropdown.value = (int)sceneGenerator.staticColliderType;
                staticColliderDropdown.onValueChanged.AddListener(OnStaticColliderTypeChanged);
            }
            
            if (dynamicColliderDropdown != null)
            {
                dynamicColliderDropdown.AddOptions(System.Enum.GetNames(typeof(CollisionSceneGenerator.ColliderType)).ToList());
                dynamicColliderDropdown.value = (int)sceneGenerator.dynamicColliderType;
                dynamicColliderDropdown.onValueChanged.AddListener(OnDynamicColliderTypeChanged);
            }
            
            if (forceSlider != null)
            {
                forceSlider.minValue = 0;
                forceSlider.maxValue = 100;
                forceSlider.value = sceneGenerator.forceRange.y;
                forceSlider.onValueChanged.AddListener(OnForceRangeChanged);
            }
            
            if (regenerateButton != null)
            {
                regenerateButton.onClick.AddListener(OnRegenerateButtonClicked);
            }
            
            if (toggleDebugButton != null)
            {
                toggleDebugButton.onClick.AddListener(OnToggleDebugButtonClicked);
                UpdateDebugButtonText();
            }
        }
        
        #region UI事件处理
        private void OnStaticObjectCountChanged(float value)
        {
            if (sceneGenerator != null)
            {
                sceneGenerator.staticObjectCount = (int)value;
            }
        }
        
        private void OnDynamicObjectCountChanged(float value)
        {
            if (sceneGenerator != null)
            {
                sceneGenerator.dynamicObjectCount = (int)value;
            }
        }
        
        private void OnStaticColliderTypeChanged(int value)
        {
            if (sceneGenerator != null)
            {
                sceneGenerator.staticColliderType = (CollisionSceneGenerator.ColliderType)value;
            }
        }
        
        private void OnDynamicColliderTypeChanged(int value)
        {
            if (sceneGenerator != null)
            {
                sceneGenerator.dynamicColliderType = (CollisionSceneGenerator.ColliderType)value;
            }
        }
        
        private void OnForceRangeChanged(float value)
        {
            if (sceneGenerator != null)
            {
                sceneGenerator.forceRange = new Vector2(sceneGenerator.forceRange.x, value);
            }
        }
        
        private void OnRegenerateButtonClicked()
        {
            if (sceneGenerator != null)
            {
                sceneGenerator.RegenerateScene();
            }
        }
        
        private void OnToggleDebugButtonClicked()
        {
            if (collisionSystem != null)
            {
                collisionSystem.enableDebug = !collisionSystem.enableDebug;
                UpdateDebugButtonText();
            }
        }
        #endregion
        
        /// <summary>
        /// 更新调试按钮文本
        /// </summary>
        private void UpdateDebugButtonText()
        {
            if (toggleDebugButton != null && collisionSystem != null)
            {
                toggleDebugButton.GetComponentInChildren<Text>().text = 
                    collisionSystem.enableDebug ? "关闭调试可视化" : "开启调试可视化";
            }
        }
    }
}