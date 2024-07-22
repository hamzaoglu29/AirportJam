using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public int row_count;
    public int column_count;
    public List<PlaneData> planes;
    public List<TileData> colored_tiles;
}
