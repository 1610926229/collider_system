using UnityEngine;
using System.Collections.Generic;
using CollisionSystem.Shape;

namespace CollisionSystem.Converter
{
    /// <summary>
    /// Collider转换器
    /// 将Unity的Collider组件转换为自定义的CollisionShape
    /// </summary>
    public static class ColliderConverter
    {
        /// <summary>
        /// 将Unity Collider转换为CollisionShape
        /// </summary>
        /// <param name="collider">Unity Collider组件</param>
        /// <param name="transform">物体的Transform</param>
        /// <returns>转换后的CollisionShape</returns>
        public static CollisionShape ConvertToCollisionShape(Collider collider, Transform transform)
        {
            if (collider == null)
            {
                UnityEngine.Debug.LogError("Collider is null!");
                return null;
            }
            
            CollisionShape shape = null;
            
            // 根据Collider类型进行转换
            switch (collider)
            {
                case SphereCollider sphereCollider:
                    shape = ConvertSphereCollider(sphereCollider, transform);
                    break;
                
                case BoxCollider boxCollider:
                    shape = ConvertBoxCollider(boxCollider, transform);
                    break;
                
                case MeshCollider meshCollider:
                    shape = ConvertMeshCollider(meshCollider, transform);
                    break;
                
                default:
                    UnityEngine.Debug.LogWarning($"Unsupported Collider type: {collider.GetType().Name}");
                    break;
            }
            
            // 设置形状的基本属性
            if (shape != null)
            {
                shape.Position = transform.position;
                shape.Rotation = transform.rotation;
                shape.UpdateBounds();
            }
            
            return shape;
        }
        
        /// <summary>
        /// 将SphereCollider转换为SphereShape
        /// </summary>
        private static SphereShape ConvertSphereCollider(SphereCollider sphereCollider, Transform transform)
        {
            // 计算世界空间中的半径
            float worldRadius = sphereCollider.radius * Mathf.Max(
                transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z
            );
            
            // 计算世界空间中的中心位置
            Vector3 worldCenter = transform.TransformPoint(sphereCollider.center);
            
            // 创建SphereShape
            SphereShape sphereShape = new SphereShape(worldCenter, transform.rotation, worldRadius);
            
            return sphereShape;
        }
        
        /// <summary>
        /// 将BoxCollider转换为BoxShape
        /// </summary>
        private static BoxShape ConvertBoxCollider(BoxCollider boxCollider, Transform transform)
        {
            // 计算世界空间中的半边长
            Vector3 worldHalfExtents = new Vector3(
                boxCollider.size.x * 0.5f * Mathf.Abs(transform.localScale.x),
                boxCollider.size.y * 0.5f * Mathf.Abs(transform.localScale.y),
                boxCollider.size.z * 0.5f * Mathf.Abs(transform.localScale.z)
            );
            
            // 计算世界空间中的中心位置
            Vector3 worldCenter = transform.TransformPoint(boxCollider.center);
            
            // 创建BoxShape
            BoxShape boxShape = new BoxShape(worldCenter, transform.rotation, worldHalfExtents);
            
            return boxShape;
        }
        
        /// <summary>
        /// 将MeshCollider转换为ConvexHullShape
        /// </summary>
        private static ConvexHullShape ConvertMeshCollider(MeshCollider meshCollider, Transform transform)
        {
            // 检查MeshCollider是否启用了convex选项
            if (!meshCollider.convex)
            {
                UnityEngine.Debug.LogWarning("MeshCollider must be convex to be converted to CollisionShape!");
                return null;
            }
            
            // 获取Mesh
            Mesh mesh = meshCollider.sharedMesh;
            if (mesh == null)
            {
                UnityEngine.Debug.LogWarning("MeshCollider has no sharedMesh!");
                return null;
            }
            
            // 获取Mesh的顶点
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length == 0)
            {
                UnityEngine.Debug.LogWarning("Mesh has no vertices!");
                return null;
            }
            
            // 将顶点转换为世界空间
            List<Vector3> worldVertices = new List<Vector3>();
            for (int i = 0; i < vertices.Length; i++)
            {
                worldVertices.Add(transform.TransformPoint(vertices[i]));
            }
            
            // 创建ConvexHullShape
            ConvexHullShape convexHullShape = new ConvexHullShape(transform.position, transform.rotation, worldVertices);
            
            return convexHullShape;
        }
        
        /// <summary>
        /// 将CollisionShape转换回Unity Collider（仅支持基本形状）
        /// </summary>
        /// <param name="shape">CollisionShape</param>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>创建的Unity Collider</returns>
        public static Collider ConvertToUnityCollider(CollisionShape shape, GameObject gameObject)
        {
            if (shape == null || gameObject == null)
            {
                UnityEngine.Debug.LogError("Shape or GameObject is null!");
                return null;
            }
            
            Collider collider = null;
            
            // 根据CollisionShape类型进行转换
            switch (shape.Type)
            {
                case CollisionShapeType.Sphere:
                    collider = ConvertSphereShape((SphereShape)shape, gameObject);
                    break;
                
                case CollisionShapeType.Box:
                    collider = ConvertBoxShape((BoxShape)shape, gameObject);
                    break;
                
                case CollisionShapeType.ConvexHull:
                    collider = ConvertConvexHullShape((ConvexHullShape)shape, gameObject);
                    break;
                
                default:
                    UnityEngine.Debug.LogWarning($"Unsupported CollisionShape type: {shape.Type}");
                    break;
            }
            
            return collider;
        }
        
        /// <summary>
        /// 将SphereShape转换为SphereCollider
        /// </summary>
        private static SphereCollider ConvertSphereShape(SphereShape sphereShape, GameObject gameObject)
        {
            // 添加SphereCollider组件
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            
            // 计算局部空间中的半径（假设物体缩放均匀）
            float maxScale = Mathf.Max(
                gameObject.transform.localScale.x,
                gameObject.transform.localScale.y,
                gameObject.transform.localScale.z
            );
            float localRadius = sphereShape.Radius / maxScale;
            
            // 设置SphereCollider属性
            sphereCollider.radius = localRadius;
            sphereCollider.center = Vector3.zero;
            
            return sphereCollider;
        }
        
        /// <summary>
        /// 将BoxShape转换为BoxCollider
        /// </summary>
        private static BoxCollider ConvertBoxShape(BoxShape boxShape, GameObject gameObject)
        {
            // 添加BoxCollider组件
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            
            // 计算局部空间中的尺寸
            Vector3 localSize = new Vector3(
                boxShape.HalfExtents.x * 2 / Mathf.Abs(gameObject.transform.localScale.x),
                boxShape.HalfExtents.y * 2 / Mathf.Abs(gameObject.transform.localScale.y),
                boxShape.HalfExtents.z * 2 / Mathf.Abs(gameObject.transform.localScale.z)
            );
            
            // 设置BoxCollider属性
            boxCollider.size = localSize;
            boxCollider.center = Vector3.zero;
            
            return boxCollider;
        }
        
        /// <summary>
        /// 将ConvexHullShape转换为MeshCollider
        /// </summary>
        private static MeshCollider ConvertConvexHullShape(ConvexHullShape convexHullShape, GameObject gameObject)
        {
            // 获取凸包的顶点
            Vector3[] vertices = convexHullShape.GetVertices();
            if (vertices.Length < 4)
            {
                UnityEngine.Debug.LogWarning("ConvexHullShape has too few vertices to create MeshCollider!");
                return null;
            }
            
            // 转换为局部空间
            Vector3[] localVertices = new Vector3[vertices.Length];
            Matrix4x4 worldToLocal = gameObject.transform.worldToLocalMatrix;
            for (int i = 0; i < vertices.Length; i++)
            {
                localVertices[i] = worldToLocal.MultiplyPoint3x4(vertices[i]);
            }
            
            // 创建Mesh
            Mesh mesh = new Mesh();
            mesh.vertices = localVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            
            // 添加MeshCollider组件
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;
            
            return meshCollider;
        }
    }
}
