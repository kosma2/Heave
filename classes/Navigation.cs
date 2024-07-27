namespace Heave
{
    public class Node
    {
        public string Id { get; }
        public string GeoPoint { get; }
        public List<Edge> Edges { get; set; }     //connections to neighbours

        public Node(string id, string geoPoint)
        {
            Id = id;
            GeoPoint = geoPoint;
            Edges = new List<Edge>();
        }
    }
    public class Edge
    {
        public Node To { get; }       // destination node
        public Node From { get; }     // origin node
        public double Distance { get; }

        public Edge(Node from, Node to, double dist)
        {
            To = to;
            From = from;
            Distance = dist;
        }
    }
    public class Dijkstra
    {
        public Dictionary<Node, double> Distances { get; private set; }      //stores the node's distance from source node
        public Dictionary<Node, Node> Previous { get; private set; }      //stores the node's last node

        public List<Node> ExecuteDij(Node source, List<Node> nodes)
        {
            Distances = new();
            Previous = new Dictionary<Node, Node>();
            //priority queue sorts distances from smallest to source node to largest
            var priorityQueue = new SortedSet<Node>(Comparer<Node>.Create((node1, node2) => Distances[node1] != Distances[node2] ? Distances[node1].CompareTo(Distances[node2]) : node1.Id.CompareTo(node2.Id)));

            foreach (Node node in nodes)
            {
                //System.Console.WriteLine("node " + node.Id + " dictionaries set and added to queue");
                Distances[node] = int.MaxValue;
                Previous[node] = null;
                priorityQueue.Add(node);
            }
            Distances[source] = 0;  //update distance for source and update its position in queue 
            Previous[source] = null;
            priorityQueue.Remove(source);
            priorityQueue.Add(source);

            //System.Console.WriteLine("processing queue. count is " + priorityQueue.Count);
            while (priorityQueue.Count() > 0)
            {
                Node current = priorityQueue.Min;       //get the node with the shortest distance
                //System.Console.WriteLine(" current node is " + current.Id);
                priorityQueue.Remove(current);

                foreach (Edge edge in current.Edges)
                {
                    Node neighbour = edge.To;
                    double alt = Distances[current] + edge.Distance;
                    /*Console.WriteLine("current edge is " + edge.Distance);
                    System.Console.WriteLine("Distances from " + edge.From.Id + " to " + edge.To.Id + " is " + Distances[neighbour]);
                    System.Console.WriteLine("alternative distance is " + alt);*/
                    if (alt < Distances[neighbour])
                    {
                        //System.Console.WriteLine("alt is smaller.. updating.");
                        //System.Console.WriteLine($"Distances for {neighbour.Id} updated to {alt}");
                        priorityQueue.Remove(neighbour);
                        Distances[neighbour] = alt;
                        Previous[neighbour] = current;
                        priorityQueue.Add(neighbour);
                    }
                }
            }
            /*foreach (var node in Previous)
            {
                if (node.Value == null)
                {
                    Console.WriteLine($"Start {node.Key.Id}, End Origin");
                }
                else
                {
                    System.Console.WriteLine($"Start {node.Key.Id}, End {node.Value.Id}");
                }
            }*/
            List<Node> path = new();
            Node currentNode = Previous.Keys.FirstOrDefault(node => node.Id == "n9");
            if (Previous.ContainsKey(currentNode) && Previous[currentNode] != null)
            {
                while (currentNode != null && currentNode != source)
                {
                    path.Add(currentNode);
                    //System.Console.WriteLine(currentNode.Id + " added");
                    currentNode = Previous[currentNode];  // Move to the next node in the path
                }
                path.Add(source);  // Add the source at the end to complete the path
                path.Reverse();    // Reverse to get the path from source to destination

                /*foreach (Node node in path)
                {
                    System.Console.WriteLine(node.Id);
                    
                }*/
                return path;
            }
            else
            {
                System.Console.WriteLine("No valid path from source to destination");
                return null;
            }
          
        }
    }
}