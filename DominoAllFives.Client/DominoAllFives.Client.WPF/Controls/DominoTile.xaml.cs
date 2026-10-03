using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DominoAllFives.Client.WPF.Controls
{
    public enum DominoTileType
    {
        Normal,
        DoublePoints,
        Block
    }
    public partial class DominoTile : UserControl
    {
        public static readonly DependencyProperty topValueProperty =
        DependencyProperty.Register(nameof(TopValue), typeof(int), typeof(DominoTile),
            new PropertyMetadata(0, OnTilePropertyChanged));

        public static readonly DependencyProperty bottomValueProperty =
            DependencyProperty.Register(nameof(BottomValue), typeof(int), typeof(DominoTile),
                new PropertyMetadata(0, OnTilePropertyChanged));

        public static readonly DependencyProperty tileTypeProperty =
            DependencyProperty.Register(nameof(TileType), typeof(DominoTileType), typeof(DominoTile),
                new PropertyMetadata(DominoTileType.Normal, OnTilePropertyChanged));

        public static readonly DependencyProperty pipBrushProperty =
            DependencyProperty.Register(nameof(PipBrush), typeof(Brush), typeof(DominoTile),
                new PropertyMetadata(Brushes.Black));

        public int TopValue
        {
            get => (int)GetValue(topValueProperty);
            set => SetValue(topValueProperty, value);
        }

        public int BottomValue
        {
            get => (int)GetValue(bottomValueProperty);
            set => SetValue(bottomValueProperty, value);
        }

        public DominoTileType TileType
        {
            get => (DominoTileType)GetValue(tileTypeProperty);
            set => SetValue(tileTypeProperty, value);
        }

        public Brush PipBrush
        {
            get => (Brush)GetValue(pipBrushProperty);
            set => SetValue(pipBrushProperty, value);
        }

        public DominoTile()
        {
            InitializeComponent();
            UpdateTileAppearance();
        }

        private static void OnTilePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DominoTile tile)
            {
                tile.UpdateTileAppearance();
            }
        }

        private void UpdateTileAppearance()
        {
            if (bdrTileFrame == null)
            {
                return;
            }

            PipBrush = (Brush)FindResource("PureBlackBrush");

            switch (TileType)
            {
                case DominoTileType.DoublePoints:
                    bdrTileFrame.Background = (Brush)FindResource("PrimaryRedBrush");
                    break;
                case DominoTileType.Block:
                    bdrTileFrame.Background = (Brush)FindResource("BlockPurpleBrush");
                    break;
                default:
                    bdrTileFrame.Background = (Brush)FindResource("CreamLightBrush");
                    break;
            }

            RenderPips(TopValue, pipTopTL, pipTopTR, pipTopML, pipTopC, pipTopMR, pipTopBL, pipTopBR);
            RenderPips(BottomValue, pipBottomTL, pipBottomTR, pipBottomML, pipBottomC, pipBottomMR, pipBottomBL, pipBottomBR);
        }

        private static void RenderPips(int value, Ellipse tl, Ellipse tr, Ellipse ml, Ellipse c, Ellipse mr, Ellipse bl, Ellipse br)
        {
            tl.Visibility = Visibility.Collapsed;
            tr.Visibility = Visibility.Collapsed;
            ml.Visibility = Visibility.Collapsed;
            c.Visibility = Visibility.Collapsed;
            mr.Visibility = Visibility.Collapsed;
            bl.Visibility = Visibility.Collapsed;
            br.Visibility = Visibility.Collapsed;

            switch (value)
            {
                case 1:
                    c.Visibility = Visibility.Visible;
                    break;
                case 2:
                    tl.Visibility = Visibility.Visible;
                    br.Visibility = Visibility.Visible;
                    break;
                case 3:
                    tl.Visibility = Visibility.Visible;
                    c.Visibility = Visibility.Visible;
                    br.Visibility = Visibility.Visible;
                    break;
                case 4:
                    tl.Visibility = Visibility.Visible;
                    tr.Visibility = Visibility.Visible;
                    bl.Visibility = Visibility.Visible;
                    br.Visibility = Visibility.Visible;
                    break;
                case 5:
                    tl.Visibility = Visibility.Visible;
                    tr.Visibility = Visibility.Visible;
                    c.Visibility = Visibility.Visible;
                    bl.Visibility = Visibility.Visible;
                    br.Visibility = Visibility.Visible;
                    break;
                case 6:
                    tl.Visibility = Visibility.Visible;
                    tr.Visibility = Visibility.Visible;
                    ml.Visibility = Visibility.Visible;
                    mr.Visibility = Visibility.Visible;
                    bl.Visibility = Visibility.Visible;
                    br.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
