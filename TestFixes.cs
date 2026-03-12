using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("开始验证修复结果...");
        
        string projectPath = Directory.GetCurrentDirectory();
        int totalErrors = 0;
        
        // 检查所有.cs文件
        foreach (string file in Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories))
        {
            // 跳过不需要检查的目录
            if (file.Contains("Library") || file.Contains("Temp") || file.Contains("Packages") || 
                file.Contains("obj") || file.Contains(".vs"))
                continue;
                
            string fileName = Path.GetFileName(file);
            Console.WriteLine($"\n检查文件: {fileName}");
            
            string content = File.ReadAllText(file);
            int fileErrors = 0;
            
            // 检查错误类型
            if (CheckDebugCalls(file, content)) fileErrors++;
            if (CheckCollisionContactPoint(file, content)) fileErrors++;
            if (CheckEdgeVertexIndex(file, content)) fileErrors++;
            if (CheckArrayCount(file, content)) fileErrors++;
            if (CheckIndicesProperty(file, content)) fileErrors++;
            if (CheckSphereIntersects(file, content)) fileErrors++;
            if (CheckNavMeshConverterFaces(file, content)) fileErrors++;
            
            if (fileErrors == 0)
            {
                Console.WriteLine($"  ✓ {fileName}: 没有发现错误");
            }
            else
            {
                Console.WriteLine($"  ✗ {fileName}: 发现 {fileErrors} 个错误");
                totalErrors += fileErrors;
            }
        }
        
        Console.WriteLine($"\n\n验证完成！共发现 {totalErrors} 个错误。");
        if (totalErrors == 0)
        {
            Console.WriteLine("🎉 所有修复都已生效！项目应该可以正常编译了。");
        }
        else
        {
            Console.WriteLine("⚠️ 仍有错误需要修复。");
        }
    }
    
    static bool CheckDebugCalls(string file, string content)
    {
        // 检查Debug.Log调用是否正确
        Regex regex = new Regex(@"Debug\.(Log|LogError|LogWarning)", RegexOptions.Multiline);
        MatchCollection matches = regex.Matches(content);
        
        foreach (Match match in matches)
        {
            // 检查是否在CollisionSystem.Debug命名空间中
            if (content.Contains("namespace CollisionSystem.Debug") && !match.Value.StartsWith("UnityEngine.Debug"))
            {
                Console.WriteLine($"    错误: Debug调用可能使用了错误的命名空间，应使用UnityEngine.Debug");
                return true;
            }
        }
        
        return false;
    }
    
    static bool CheckCollisionContactPoint(string file, string content)
    {
        // 检查CollisionContact.Point属性使用
        if (content.Contains("contact.Point"))
        {
            Console.WriteLine($"    错误: 使用了不存在的contact.Point属性，应使用PointA或PointB");
            return true;
        }
        
        return false;
    }
    
    static bool CheckEdgeVertexIndex(string file, string content)
    {
        // 检查Edge.VertexIndex1和VertexIndex2属性使用
        Regex regex = new Regex(@"edge\.VertexIndex(1|2)", RegexOptions.Multiline);
        if (regex.IsMatch(content))
        {
            Console.WriteLine($"    错误: 使用了不存在的edge.VertexIndex1/VertexIndex2属性，应使用StartIndex/EndIndex");
            return true;
        }
        
        return false;
    }
    
    static bool CheckArrayCount(string file, string content)
    {
        // 检查Array.Count属性使用
        Regex regex = new Regex(@"\.(Vertices|Faces|Edges|Indices)\.Count", RegexOptions.Multiline);
        if (regex.IsMatch(content))
        {
            Console.WriteLine($"    错误: 数组没有Count属性，应使用Length属性");
            return true;
        }
        
        return false;
    }
    
    static bool CheckIndicesProperty(string file, string content)
    {
        // 检查Indices属性使用
        Regex regex = new Regex(@"\.Indices\.(Count|Length)", RegexOptions.Multiline);
        if (regex.IsMatch(content))
        {
            Console.WriteLine($"    错误: 可能使用了不存在的Indices属性");
            return true;
        }
        
        return false;
    }
    
    static bool CheckSphereIntersects(string file, string content)
    {
        // 检查SphereShape.Intersects方法使用
        Regex regex = new Regex(@"(SphereShape|BoxShape|CollisionShape)\.Intersects", RegexOptions.Multiline);
        if (regex.IsMatch(content))
        {
            Console.WriteLine($"    错误: 形状类没有Intersects方法，应使用CollisionDetector类");
            return true;
        }
        
        return false;
    }
    
    static bool CheckNavMeshConverterFaces(string file, string content)
    {
        // 检查NavMeshConverter中GetFaces的使用
        if (file.Contains("NavMeshConverter.cs") && content.Contains("GetFaces()"))
        {
            if (content.Contains("List<Vector3[]> faces = convexHullShape.GetFaces()"))
            {
                Console.WriteLine($"    错误: NavMeshConverter中GetFaces()的返回值类型错误");
                return true;
            }
        }
        
        return false;
    }
}