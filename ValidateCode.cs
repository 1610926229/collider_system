using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始检查代码...");
            string projectPath = Directory.GetCurrentDirectory();
            int errorCount = 0;
            
            // 检查所有.cs文件
            foreach (string file in Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories))
            {
                // 跳过一些不需要检查的目录
                if (file.Contains("Library") || file.Contains("Temp") || file.Contains("Packages") || file.Contains("obj"))
                    continue;
                    
                Console.WriteLine($"\n检查文件: {file}");
                
                string content = File.ReadAllText(file);
                
                // 检查Debug.Log调用是否正确
                errorCount += CheckDebugCalls(file, content);
                
                // 检查CollisionContact.Point属性使用
                errorCount += CheckCollisionContactPoint(file, content);
                
                // 检查Indices属性使用
                errorCount += CheckIndicesProperty(file, content);
                
                // 检查Edge.VertexIndex1和VertexIndex2属性使用
                errorCount += CheckEdgeVertexIndexProperties(file, content);
                
                // 检查Array.Count属性使用
                errorCount += CheckArrayCountProperty(file, content);
                
                // 检查SphereShape.Intersects方法使用
                errorCount += CheckSphereShapeIntersects(file, content);
            }
            
            Console.WriteLine($"\n\n检查完成，共发现 {errorCount} 个错误。");
        }
        
        static int CheckDebugCalls(string file, string content)
        {
            int errorCount = 0;
            Regex regex = new Regex(@"Debug\.(Log|LogError|LogWarning)", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(content);
            
            foreach (Match match in matches)
            {
                // 检查是否在CollisionSystem.Debug命名空间中
                if (content.Contains("namespace CollisionSystem.Debug") && !match.Value.StartsWith("UnityEngine.Debug"))
                {
                    Console.WriteLine($"错误: {file} ({match.Index}) - 可能使用了错误的Debug命名空间，应使用UnityEngine.Debug");
                    errorCount++;
                }
            }
            
            return errorCount;
        }
        
        static int CheckCollisionContactPoint(string file, string content)
        {
            int errorCount = 0;
            Regex regex = new Regex(@"contact\.Point", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(content);
            
            foreach (Match match in matches)
            {
                Console.WriteLine($"错误: {file} ({match.Index}) - CollisionContact.Point属性不存在，应使用PointA或PointB");
                errorCount++;
            }
            
            return errorCount;
        }
        
        static int CheckIndicesProperty(string file, string content)
        {
            int errorCount = 0;
            Regex regex = new Regex(@"Indices\.(Count|Length)", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(content);
            
            foreach (Match match in matches)
            {
                Console.WriteLine($"错误: {file} ({match.Index}) - Indices属性不存在，请检查代码");
                errorCount++;
            }
            
            return errorCount;
        }
        
        static int CheckEdgeVertexIndexProperties(string file, string content)
        {
            int errorCount = 0;
            Regex regex = new Regex(@"edge\.VertexIndex(1|2)", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(content);
            
            foreach (Match match in matches)
            {
                Console.WriteLine($"错误: {file} ({match.Index}) - Edge.VertexIndex1/VertexIndex2属性不存在，应使用StartIndex/EndIndex");
                errorCount++;
            }
            
            return errorCount;
        }
        
        static int CheckArrayCountProperty(string file, string content)
        {
            int errorCount = 0;
            Regex regex = new Regex(@"\.(Vertices|Faces|Edges|Indices)\.Count", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(content);
            
            foreach (Match match in matches)
            {
                Console.WriteLine($"错误: {file} ({match.Index}) - 数组没有Count属性，应使用Length属性");
                errorCount++;
            }
            
            return errorCount;
        }
        
        static int CheckSphereShapeIntersects(string file, string content)
        {
            int errorCount = 0;
            Regex regex = new Regex(@"\.(Intersects)\(", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(content);
            
            foreach (Match match in matches)
            {
                // 检查是否是SphereShape或其他形状的Intersects方法调用
                if (content.Substring(Math.Max(0, match.Index - 20), 20).Contains("sphere") || 
                    content.Substring(Math.Max(0, match.Index - 20), 20).Contains("box") ||
                    content.Substring(Math.Max(0, match.Index - 20), 20).Contains("shape"))
                {
                    Console.WriteLine($"错误: {file} ({match.Index}) - 形状类没有Intersects方法，应使用CollisionDetector类");
                    errorCount++;
                }
            }
            
            return errorCount;
        }
    }
}