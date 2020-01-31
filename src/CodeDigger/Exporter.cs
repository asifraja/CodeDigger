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
        private IDictionary<long, Node> _nodes;
        private IDictionary<long, EdgeNode> _edges;
        public Exporter(IDictionary<long, Node> nodes, IDictionary<long, EdgeNode> edges)
        {
            _nodes = nodes;
            _edges = edges;
        }
        private void FixEdgeIds()
        {
            if (_idFixed) return;

            var edgesNeedFixing = _edges.Values.Where(e => e.Source == 0 || e.Target == 0);
            foreach (var edge in edgesNeedFixing)
            {
                if (edge.Source==0)
                {
                    var n = _nodes.Values.FirstOrDefault(n => n.Key == edge.ParentKey);
                    if(n!=null)
                        edge.Source = n.Id;
                }
                if (edge.Target == 0)
                {
                    var n = _nodes.Values.FirstOrDefault(n => n.Key == edge.ChildKey);
                    if (n != null)
                        edge.Target = n.Id;
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

        public Exporter ExportCsv(string solutionName)
        {
            FixEdgeIds();
            ExportNodesInCsv(solutionName);
            ExportEdgesInCsv(solutionName);
            return this;
        }

        private void ExportNodesInCsv(string solutionName)
        {
            var nodeData = _nodes.Values.OrderBy(n => n.Key).ThenBy(y => y.Name);
            var sb = new StringBuilder();
            sb.AppendLine($"Id,Key,Kind,KIndOf,Properties");
            foreach (var node in nodeData)
            {
                sb.AppendLine($"\"{node.Id}\",\"{node.Key}\",\"{node.Kind}\",\"{node.Properties}\"");
            }
            File.WriteAllText(@$"D:\temp\{solutionName}-nodes.csv", sb.ToString());
        }

        private void ExportEdgesInCsv(string solutionName)
        {
            var edgeData = _edges.Values.OrderBy(n => n.ParentKey).ThenBy(y => y.ChildKey);
            var sb = new StringBuilder();
            sb.AppendLine($"Id,Source,Target,Related,ParentKey,ChildKey");
            foreach (var node in edgeData)
            {
                sb.AppendLine($"\"{node.Id}\",\"{node.Source}\",\"{node.Target}\",\"{node.Related}\",\"{node.ParentKey}\",\"{node.ChildKey}\"");
            }
            File.WriteAllText(@$"D:\temp\{solutionName}-edges.csv", sb.ToString());
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
            //var nodeData = JsonConvert.SerializeObject(_nodes.OrderBy(n=>n.Value.Key));
            File.WriteAllText(@$"D:\temp\{solutionName}-nodes.json",nodeData.Replace("{\"Id\"", Environment.NewLine+ "{\"Id\""));
        }

        private void ExportEdgesInJson(string solutionName)
        {
            FixEdgeIds();
            var edgeData = JsonConvert.SerializeObject(_edges.Values.OrderBy(n => n.ParentKey).ThenBy(y => y.ChildKey));
            //var edgeData = JsonConvert.SerializeObject(_edges.OrderBy(n => n.Value.ParentKey).ThenBy(y => y.Value.ChildKey));
            File.WriteAllText(@$"D:\temp\{solutionName}-edges.json", edgeData.Replace("{\"Id\"", Environment.NewLine + "{\"Id\""));
        }
    }
}



