using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeDigger.Models;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using UsingCollectorCS;

namespace CodeDigger
{
    class Program
    {
        public static IList<Node> Nodes = new List<Node>();
        public static IList<EdgeNode> Edges = new List<EdgeNode>();

        static async Task Main(string[] args)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                var solutionPath = args[0];
                Console.WriteLine($"Loading solution '{solutionPath}'");

                // Attach progress reporter so we print projects as they are loaded.
                var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                var solutionName = Path.GetFileNameWithoutExtension(solutionPath);

                Console.WriteLine($"Finished loading solution '{solutionPath}'");

                var nodes = new Dictionary<string, Node>();
                var edges = new Dictionary<string, EdgeNode>();

                foreach (var project in solution.Projects)//.Where(p=>p.Name.Equals("easyJet.Bookings")))
                {
                    foreach (var document in project.Documents)//.Where(d=>!d.Name.Contains("Dummy")))
                    {
                        var collector = new CodeWalker(document.FilePath, document.Name, nodes, edges);
                        SyntaxTree tree = CSharpSyntaxTree.ParseText(await document.GetTextAsync());
                        var root = (CompilationUnitSyntax)tree.GetRoot();
                        collector.Visit(root);
                    }
                }
                // Save Data files
                BuildDatafile.FixEdgeIds(nodes, edges);
                BuildDatafile.Save(solutionName, nodes.Values.ToList());
                BuildDatafile.Save(solutionName, edges.Values.ToList());
            }
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }

    public class BuildDatafile
    {

        public static void FixEdgeIds(IDictionary<string, Node> nodes, IDictionary<string, EdgeNode> edges)
        {
            var edgesNeedFixing = edges.Values.Where(e => e.SourceId == 0 || e.TargetId == 0);
            foreach (var edge in edgesNeedFixing)
            {
                if (edge.SourceId==0)
                {
                    var n = nodes.Values.FirstOrDefault(n => n.Key == edge.Source);
                    if(n!=null)
                        edge.SourceId = n.Id;
                }
                if (edge.TargetId == 0)
                {
                    var n = nodes.Values.FirstOrDefault(n => n.Key == edge.Target);
                    if (n != null)
                        edge.TargetId = n.Id;
                }
            }
        }
        public static void Save(string solutionName, IList<Node> nodes)
        {
            var nodeData = JsonConvert.SerializeObject(nodes.OrderBy(n=>n.Key).ThenBy(y=>y.Name));
            File.WriteAllText(@$"D:\temp\{solutionName}-nodes.txt",nodeData.Replace("{\"Id\"", Environment.NewLine+ "{\"Id\""));
        }

        public static void Save(string solutionName, IList<EdgeNode> edges)
        {
            var edgeData = JsonConvert.SerializeObject(edges.OrderBy(n => n.Source).ThenBy(y => y.Target));
            File.WriteAllText(@$"D:\temp\{solutionName}-edges.txt", edgeData.Replace("{\"Id\"", Environment.NewLine + "{\"Id\""));
        }
    }
}



