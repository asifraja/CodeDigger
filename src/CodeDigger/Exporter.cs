using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeDigger.Models;
using Newtonsoft.Json;

namespace CodeDigger
{
    public class Exporter
    {

        //    // CREATE (Email:Client {id:'1', description:'email client', repo:'localgit:emailClient'})
        //    // "CREATE (:{kind} {id:'{id}', description:'email client', repo:'localgit:emailClient'})"

        private bool _idFixed = false;
        private IDictionary<string, Node> _nodes;
        private IDictionary<string, EdgeNode> _edges;
        public Exporter(IDictionary<string, Node> nodes, IDictionary<string, EdgeNode> edges)
        {
            _nodes = nodes;
            _edges = edges;
        }
        private void FixEdgeIds()
        {
            if (_idFixed) return;

            var edgesNeedFixing = _edges.Values.Where(e => e.SourceId == 0 || e.TargetId == 0);
            foreach (var edge in edgesNeedFixing)
            {
                if (edge.SourceId==0)
                {
                    var n = _nodes.Values.FirstOrDefault(n => n.Key == edge.Source);
                    if(n!=null)
                        edge.SourceId = n.Id;
                }
                if (edge.TargetId == 0)
                {
                    var n = _nodes.Values.FirstOrDefault(n => n.Key == edge.Target);
                    if (n != null)
                        edge.TargetId = n.Id;
                }
            }
            _idFixed = true;
        }

        public Exporter ExportJson(string solutionName)
        {
            FixEdgeIds();
            ExportNodesInJson(solutionName);
            ExportEdgesInJson(solutionName);
            return this;
        }

        public Exporter ExportNeo4J(string solutionName)
        {
            FixEdgeIds();
            ExportNodesInNeo4J(solutionName);
            return this;
        }

        private void ExportNodesInNeo4J(string solutionName)
        {
            var nodeData = _nodes.Values.OrderBy(n => n.Key).ThenBy(y => y.Name);
            var sb = new StringBuilder();
            foreach (var node in nodeData)
            {
                sb.AppendLine($"CREATE (:{node.Kind} {{id:'{node.Kind}', description:'{node.Name}', repo:'{node.Key}'}})");
            }
            File.WriteAllText(@$"D:\temp\{solutionName}-nodes.neo4j.txt", sb.ToString());
        }

        private void ExportNodesInJson(string solutionName)
        {
            var nodeData = JsonConvert.SerializeObject(_nodes.Values.OrderBy(n=>n.Key).ThenBy(y=>y.Name));
            File.WriteAllText(@$"D:\temp\{solutionName}-nodes.json",nodeData.Replace("{\"Id\"", Environment.NewLine+ "{\"Id\""));
        }

        private void ExportEdgesInJson(string solutionName)
        {
            FixEdgeIds();
            var edgeData = JsonConvert.SerializeObject(_edges.Values.OrderBy(n => n.Source).ThenBy(y => y.Target));
            File.WriteAllText(@$"D:\temp\{solutionName}-edges.json", edgeData.Replace("{\"Id\"", Environment.NewLine + "{\"Id\""));
        }



    }
}



