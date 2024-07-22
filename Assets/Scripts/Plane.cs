using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class Plane : MonoBehaviour
{
    public Pathfinding pathfinding;
    
    
    public float speedFactor = 2;
    private Tile endTile;
    private int lastPosX;
    private int lastPosY;
    private int numRows;
    private int numCols;

    private Vector3 lastPos;
    private Vector3 endPos;
    private int _posX;
    private int _posY;
    private string _pDirection;
    //private string _pColor;
    
    public void InitPlane(int x, int y, string d /*string c*/)
    {
        _posX = x;
        _posY = y;
        _pDirection = d;
        //_pColor = c;
    }

    private void SetPath(Queue<Tile> path)
    {
        StopAllCoroutines();
        StartCoroutine(MoveAlongPath(path));
    }
    
    private IEnumerator MoveAlongPath(Queue<Tile> path)
    {
        yield return new WaitForSeconds(1);
        var lastPosition = transform.position;
        while (path.Count > 0)
        {
            var nextTile = path.Dequeue();
            float lerpVal = 0;
            transform.LookAt(nextTile.transform,Vector3.up);

            while (lerpVal < 1)
            {
                lerpVal += Time.deltaTime * speedFactor;
                transform.position = Vector3.Lerp(lastPosition, nextTile.transform.position, lerpVal)+Vector3.up * 0.12f;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(0.5f / speedFactor);
            lastPosition = nextTile.transform.position;
            
        }
        var seq = DOTween.Sequence();
        seq.InsertCallback(0f, () =>
        {
            transform.Rotate(0, -90, 0);
        });

        var target = GridManager.Instance.GetTileAtPos(lastPosX, lastPosY).transform.position;
        seq.Insert(0f, transform.DOBlendableMoveBy(target - lastPosition, 1f).SetEase(Ease.InCubic));
        seq.Insert(0.5f, transform.DOBlendableMoveBy(Vector3.up, 0.5f).SetEase(Ease.InCubic));
        seq.OnComplete(() =>
        {
            Destroy(gameObject);
            GridManager.Instance.RemovePlane(this);
        });
        
    }
    
    private void OnMouseDown()
    {
        
        if (pathfinding == null)
        {
            pathfinding = FindObjectOfType<Pathfinding>();
        }

        numCols = GridManager.Instance.NumCols;
        numRows = GridManager.Instance.NumRows;
        var startTile = GridManager.Instance.GetTileAtPos(_posX, _posY);
        var endPosX = _pDirection switch
        {
            "N" => 0,
            "E" => _posX,
            "S" => numRows-1,      
            "W" => _posX,
            
            _ => throw new ArgumentOutOfRangeException()
        };
        var endPosY = _pDirection switch
        {
            "N" => _posY,
            "E" => numCols-1,
            "S" => _posY,      
            "W" => 0,
            
            _ => throw new ArgumentOutOfRangeException()
        };
        endTile = GridManager.Instance.GetTileAtPos(endPosX, endPosY);
        lastPosX = endPosX;
        lastPosY = endPosY;
        switch (_pDirection)
        {
            case "N":
                lastPosY = 0;
                break;
            case "E":
                lastPosX = 0;
                break;
            case "S":
                lastPosY = numCols - 1;
                break;
            case "W":
                lastPosX = numRows - 1;
                break;
        }
        startTile.isOccupied = false;
        
        var path = pathfinding.Dijkstra(startTile, endTile);
        SetPath(path);
        
        Debug.Log($"On: {_posX} {_posY} Direction: {_pDirection}");
    }
}
