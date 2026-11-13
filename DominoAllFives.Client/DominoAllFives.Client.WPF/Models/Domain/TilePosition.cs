using System;
using System.Collections.Generic;
using System.Windows;
using DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class TilePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public int TopValue { get; set; }
        public int BottomValue { get; set; }
        public DominoTileType TileType { get; set; }
    }
}
