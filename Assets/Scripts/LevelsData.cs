using System;
using System.Collections.Generic;

    [Serializable]
    public struct LevelData
    {
        public int row_count;
        public int column_count;
        public List<PlaneData> planes;
        public List<TileData> colored_tiles;
    }

    [Serializable]
    public class PlaneData
    {
        public int row;
        public int column;
        public string direction;
        public string color;
        public int number;
    }

    [Serializable]
    public class TileData
    {
        public int row;
        public int column;
        public string color;
    }
    
    public struct GridData
    {
        public int row;
        public int col;
    }
