using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileUtils : Singleton<TileUtils> {
    public bool IsNeighbourTile(Tile a, Tile b) {
        return Mathf.Abs(a.xIndex - b.xIndex) <= 1 && Mathf.Abs(a.yIndex - b.yIndex) <= 1 && a != b;
    }

    public bool IsSameType(Tile a, Tile b) {
        return a != null && b != null && a.piece != null && b.piece != null && a.piece.type == b.piece.type;
    }

    public List<GamePiece> GetPiecesByTileList(List<Tile> tiles) {
        List<GamePiece> gamePieces = new List<GamePiece>();
        foreach (Tile tile in tiles) {
            gamePieces.Add(tile.piece);
        }

        return gamePieces;
    }

    public List<Tile> GetAllNeighourTileWithSameType(Tile tile) {
        List<Tile> neighbours = GetSurroundingTiles(tile);

        return neighbours.FindAll(t => IsSameType(tile, t));
    }

    public List<Tile> GetSurroundingTiles(Tile tile) {
        List<Tile> neighours = new List<Tile>();
        Board board = GameManager.Instance.GetGameBoard();
        int x = tile.xIndex;
        int y = tile.yIndex;

        neighours.Add(board.GetTile(x, y + 1)); // UP
        neighours.Add(board.GetTile(x, y - 1)); // DOWN
        neighours.Add(board.GetTile(x - 1, y)); // LEFT
        neighours.Add(board.GetTile(x + 1, y)); // RIGHT
        neighours.Add(board.GetTile(x - 1, y + 1)); // UP_LEFT
        neighours.Add(board.GetTile(x + 1, y + 1)); // UP_RIGHT
        neighours.Add(board.GetTile(x - 1, y - 1)); // DOWN_LEFT
        neighours.Add(board.GetTile(x + 1, y - 1)); // DOWN_RIGHT

        return neighours.FindAll(t => t != null);
    }

    public DirectionType GetTailDirection(List<Tile> tiles) {
        if (tiles == null || tiles.Count == 1) {
            return DirectionType.CENTER;
        }
        Tile lastTile = tiles[tiles.Count - 1];
        Tile secLast = tiles[tiles.Count - 2];
        int secX = secLast.xIndex;
        int secY = secLast.yIndex;
        int lastX = lastTile.xIndex;
        int lastY = lastTile.yIndex;

        if (secX == lastX && lastY - secY == 1) {
            return DirectionType.UP;
        } else if (secX == lastX && secY - lastY == 1) {
            return DirectionType.DOWN;
        } else if (secY == lastY && lastX - secX == 1) {
            return DirectionType.LEFT;
        } else if (secY == lastY && secX - lastX == 1) {
            return DirectionType.RIGHT;
        } else if (lastY - secY == 1 && lastX - secX == 1) {
            return DirectionType.UP_LEFT;
        } else if (lastY - secY == 1 && secX - lastX == 1) {
            return DirectionType.UP_RIGHT;
        } else if (secY - lastY == 1 && lastX - secX == 1) {
            return DirectionType.DOWN_LEFT;
        } else if (secY - lastY == 1 && secX - lastX == 1) {
            return DirectionType.DOWN_RIGHT;
        } 

        return DirectionType.CENTER;
    }

    private int m_masterId;
    public List<Tile> FindLongestMatchPath(Tile startTile) {
        List<Tile> possibleTiles = FindPossibleLinkedTiles(startTile);
        if (possibleTiles.Count <= 2) {
            return possibleTiles;
        }

        int maxWeight = possibleTiles.Count - 1;
        NodeLink maxWeightLink = null;
        List<Node> nodes = WrapTileToNodeGraph(possibleTiles);
        Node startNode = nodes.Find(n => n.tile == startTile);
        NodeLink initLink = new NodeLink(null, startNode, null, 0);
        initLink.Link(new List<int>(new int[] { 0 }));
        startNode.nodeLinks.Add(initLink);

        Stack<NodeLink> linkQueue = new Stack<NodeLink>();
        linkQueue.Push(initLink);
        int roundCount = 0;

        while (linkQueue.Count > 0 && maxWeightLink == null && roundCount++ < 1000) {
            NodeLink nextLink = linkQueue.Pop();
            List<Node> unconnectedMinNeighbours = FindNodesWithMinimulNeighbours(nextLink);
            if (unconnectedMinNeighbours.Count > 0) {
                List<NodeLink> nodeLinks = CreateNodeLinkWithWeight(nextLink, unconnectedMinNeighbours);
                nodeLinks.ForEach(link => {
                    linkQueue.Push(link);
                    if (link.weight == maxWeight) {
                        maxWeightLink = link;
                    }
                });
            } else {
                if (nextLink.prev != null && nextLink.prevLink != null) {
                    linkQueue.Push(nextLink.prevLink);
                }
            }
        }

        if (maxWeightLink == null) {
            maxWeight = 0;
            foreach (Node node in nodes) {
                foreach (NodeLink link in node.nodeLinks) {
                    if (link.weight > maxWeight) {
                        maxWeightLink = link;
                        maxWeight = link.weight;
                    }
                }
            }
        }

        List<Tile> bestPath = new List<Tile>();
        while (maxWeightLink.prev != null) {
            bestPath.Add(maxWeightLink.current.tile);
            maxWeightLink = maxWeightLink.prevLink;
        }
        bestPath.Add(maxWeightLink.current.tile);
        bestPath.Reverse();
        //StartCoroutine(SlowHighLight(bestPath));

        return bestPath;
    }

    private List<Tile> FindPossibleLinkedTiles(Tile tile) {
        List<Tile> tiles = new List<Tile>();
        Stack<Tile> pendingStack = new Stack<Tile>();
        pendingStack.Push(tile);
        tile.visited = true;

        while (pendingStack.Count > 0) {
            Tile next = pendingStack.Pop();
            List<Tile> neighbours = GetAllNeighourTileWithSameType(next).FindAll(t => !t.visited);
            neighbours.ForEach(t => { pendingStack.Push(t); t.visited = true; });
            tiles.Add(next);
        }
        tiles.ForEach(t => t.visited = false);
        return tiles;
    }

    private void HighLightTiles(List<Tile> tiles) {
        tiles.ForEach(t => t.OnTileSelected());
    }

    IEnumerator SlowHighLight(List<Tile> tiles) {
        foreach (Tile tile in tiles) {
            yield return new WaitForSeconds(0.3f);
            tile.OnTileSelected();
        };
    }

    private List<Node> WrapTileToNodeGraph(List<Tile> tiles) {
        List<Node> nodes = new List<Node>();
        tiles.ForEach(t => nodes.Add(new Node(t)));

        foreach (Node n1 in nodes) {
            foreach (Node n2 in nodes) {
                if (IsNeighbourTile(n1.tile, n2.tile)) {
                    n1.neighbours.Add(n2);
                    n2.neighbours.Add(n1);
                }
            }
        }

        return nodes;
    }


    private string printNodeList(List<Node> nodes) {
        return string.Join("/", nodes.Select(node => "(" + node.tile.xIndex + "," + node.tile.yIndex + ")").ToArray());
    }

    private List<NodeLink> CreateNodeLinkWithWeight(NodeLink nodeLink, List<Node> neighbours) {
        List<NodeLink> links = new List<NodeLink>();
        bool isForkChain = neighbours.Count > 1;
        Node prev = nodeLink.current;
        int nextWeight = nodeLink.weight + 1;

        foreach (Node node in neighbours) {
            NodeLink link = new NodeLink(prev, node, nodeLink, nextWeight);
            link.Link(nodeLink.linkIdChain, isForkChain ? ++m_masterId : -1);
            node.nodeLinks.Add(link);
            links.Add(link);
        }

        return links;
    }

    private bool isUnconnectedNode(NodeLink originLink, Node otherNode) {
        if (otherNode.nodeLinks.Count == 0) {
            return true;
        }
        bool hasAncestorLink = false;
        bool hasSameLink = false;
        bool hasChildLink = false;

        foreach (NodeLink link in otherNode.nodeLinks) {
            if (link.IsAncestorOf(originLink)) {
                hasAncestorLink = true;
                break;
            }

            if (link.IsSameChain(originLink.linkIdChain)) {
                hasSameLink = true;
                break;
            }

            if (link.IsDirectChildOf(originLink)) {
                hasChildLink = true;
                break;
            }
        }
        return !hasAncestorLink && !hasSameLink && !hasChildLink;

    }
    private List<Node> FindNodesWithMinimulNeighbours(NodeLink nodeLink) {
        HashSet<Node> neighbours = nodeLink.current.neighbours;
        List<Node> minNodes = new List<Node>();
        int minCount = 10;
        List<Node> unconnectedNeighbours = neighbours.Where(nei => isUnconnectedNode(nodeLink, nei)).ToList<Node>();

        foreach (Node n in unconnectedNeighbours) {
            int nerghboursCount = n.neighbours.Where(nei =>
                nei != nodeLink.current &&
                !IsNodeAncestorOf(nei, nodeLink))
            .Count();

            if (nerghboursCount < minCount) {
                minCount = nerghboursCount;
                minNodes.Clear();
                minNodes.Add(n);
            } else if (nerghboursCount == minCount) {
                minNodes.Add(n);
            }
        }

        return minNodes;
    }
    private bool IsNodeAncestorOf(Node origin, NodeLink targetLink) {
        foreach (NodeLink originLink in origin.nodeLinks) {
            if (originLink.IsAncestorOf(targetLink)) {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    class Node {
        public int weight = -1;
        public HashSet<Node> neighbours;
        public Node prev;
        public Tile tile;
        public HashSet<NodeLink> nodeLinks;
        public Node(Tile tile) {
            this.tile = tile;
            neighbours = new HashSet<Node>();
            nodeLinks = new HashSet<NodeLink>();
        }
    }

    class NodeLink {
        public List<int> linkIdChain;
        public NodeLink prevLink;
        public Node prev;
        public Node current;
        public int weight;

        public NodeLink(Node prev, Node current, NodeLink prevLink, int weight) {
            this.prev = prev;
            this.current = current;
            this.weight = weight;
            this.prevLink = prevLink;
            linkIdChain = new List<int>();
        }

        public void Link(List<int> existingChain, int nextId = -1) {
            linkIdChain.AddRange(existingChain);
            if (nextId != -1) {
                linkIdChain.Add(nextId);
            }
        }

        public bool IsDirectChildOf(NodeLink otherLink) {
            return prevLink == otherLink;
        }

        public bool IsAncestorOf(NodeLink otherLink) {
            if (weight < otherLink.weight && otherLink.linkIdChain.Count >= linkIdChain.Count) {
                for (int i = 0; i < linkIdChain.Count; i++) {
                    if (linkIdChain[i] != otherLink.linkIdChain[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool IsSameChain(List<int> otherChain) {
            return linkIdChain.SequenceEqual(otherChain);
        }

        override
        public string ToString() {
            Tile tile = current.tile;
            return "[" + tile.xIndex + "," + tile.yIndex + "] " + string.Join(",", linkIdChain);
        }
    }
}


