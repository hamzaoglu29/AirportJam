using UnityEngine;
using UnityEngine.Serialization;

public class Tile : MonoBehaviour
{
    [FormerlySerializedAs("_posX")] public int posX;
    [FormerlySerializedAs("_posY")] public int posY;
    private string _color;
    
    public bool isOccupied;
    public bool isColored;

    public int cost = 1;
    
    
    
    public void InitTile(int x, int y)
    {
        posX = x;
        posY = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(posX, posY);
    }
    
    private void OnMouseDown()
    {
        if (isOccupied == false)
        {
            Debug.Log("On: "+posX+" "+posY+" Empty");
        }
        else 
        {
            Debug.Log("On: "+posX+" "+posY+" Occupied");
        }
        
        
    }
    
}
