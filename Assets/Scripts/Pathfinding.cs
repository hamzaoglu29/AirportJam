using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private GridManager gridManager;
    
    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }
    
    
    public Queue<Tile> Dijkstra(Tile start, Tile goal)
    {
        var nextTileToGoal = new Dictionary<Tile, Tile>();
        var costToReachTile = new Dictionary<Tile, int>();
    
        var frontier = new PriorityQueue<Tile>();
    
        frontier.Enqueue(goal, 0);
        costToReachTile[goal] = 0;
    
        while (frontier.Count > 0)
        {
            var curTile = frontier.Dequeue();
            if (curTile == start)
                break;
            
            
            foreach (var neighbor in gridManager.Neighbors(curTile))
            {
                var newCost = costToReachTile[curTile] + neighbor.cost;
                if (costToReachTile.ContainsKey(neighbor) == false || newCost < costToReachTile[neighbor])
                {
                     if (neighbor.isOccupied == false && neighbor.isColored == false)
                     {
                        costToReachTile[neighbor] = newCost;
                        frontier.Enqueue(neighbor, newCost);
                        nextTileToGoal[neighbor] = curTile;
                     }
                }
            }
        }
        if (nextTileToGoal.ContainsKey(start) == false)
        {
            return null;
        }

        var path = new Queue<Tile>();
        var pathTile = start;
        while (goal != pathTile)
        {
            pathTile = nextTileToGoal[pathTile];
            path.Enqueue(pathTile);
        }
        return path;
    }
        
}

