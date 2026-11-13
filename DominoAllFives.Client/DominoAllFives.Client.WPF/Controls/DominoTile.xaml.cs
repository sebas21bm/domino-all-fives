using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DominoAllFives.Client.WPF.Models;

namespace DominoAllFives.Client.WPF.Controls
{
    /// <summary>
    /// Reusable user control that renders an interactive or visual
    /// domino tile with its corresponding pips.
    /// </summary>
    public partial class DominoTile : UserControl
    {
        /// <summary>
        /// Identifies the TopValue dependency property.
        /// </summary>
        private const int PipsPerHalf = 7;
        public static readonly DependencyProperty TopValueProperty =
            DependencyProperty.Register(
            nameof(
            TopValue),
            typeof(int),
            typeof(DominoTile),
            new PropertyMetadata(
            0,
            OnTilePropertyChanged));
        /// <summary>
        /// Identifies the BottomValue dependency property.
        /// </summary>
        public static readonly DependencyProperty BottomValueProperty =
            DependencyProperty.Register(
            nameof(
            BottomValue),
            typeof(int),
            typeof(DominoTile),
            new PropertyMetadata(
            0,
            OnTilePropertyChanged));
        /// <summary>
        /// Identifies the TileType dependency property.
        /// </summary>
        public static readonly DependencyProperty TileTypeProperty =
            DependencyProperty.Register(
            nameof(
            TileType),
            typeof(DominoTileType),
            typeof(DominoTile),
            new PropertyMetadata(
            DominoTileType.Normal,
            OnTilePropertyChanged));
        /// <summary>
        /// Identifies the PipBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty PipBrushProperty =
            DependencyProperty.Register(
            nameof(
            PipBrush),
            typeof(Brush),
            typeof(DominoTile),
            new PropertyMetadata(
            Brushes.Black));
        /// <summary>
        /// Gets or sets the top numeric value of the domino tile.
        /// </summary>
        public int TopValue { get =>
            (int)GetValue(
            TopValueProperty); set =>
            SetValue(
            TopValueProperty,
            value); }
        /// <summary>
        /// Gets or sets the bottom numeric value of the domino tile.
        /// </summary>
        public int BottomValue { get =>
            (int)GetValue(
            BottomValueProperty); set =>
            SetValue(
            BottomValueProperty,
            value); }
        /// <summary>
        /// Gets or sets the type of the domino tile.
        /// </summary>
        public DominoTileType TileType { get =>
            (DominoTileType)GetValue(
            TileTypeProperty); set =>
            SetValue(
            TileTypeProperty,
            value); }
        /// <summary>
        /// Gets or sets the brush used to paint the pips of the domino tile.
        /// </summary>
        public Brush PipBrush { get =>
            (Brush)GetValue(
            PipBrushProperty); set =>
            SetValue(
            PipBrushProperty,
            value); }

        /// <summary>
        /// Initializes a new instance of the <see cref = "DominoTile"/> class.
        /// </summary>
        public DominoTile()
        {
            InitializeComponent();
            UpdateTileAppearance();
        }

        private static void OnTilePropertyChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is DominoTile tile)
            {
                tile.UpdateTileAppearance();
            }
        }

        private void UpdateTileAppearance()
        {
            if (borTileFrame == null)
            {
                return;
            }

            PipBrush = (Brush)FindResource("PureBlackBrush");
            switch (TileType)
            {
                case DominoTileType.DoublePoints:
                    borTileFrame.Background = (Brush)FindResource("PrimaryRedBrush");
                    break;
                case DominoTileType.Block:
                    borTileFrame.Background = (Brush)FindResource("BlockPurpleBrush");
                    break;
                default:
                    borTileFrame.Background = (Brush)FindResource("CreamLightBrush");
                    break;
            }

            Ellipse[] topPips =
            {
                pipTopTL,
                pipTopTR,
                pipTopML,
                pipTopC,
                pipTopMR,
                pipTopBL,
                pipTopBR
            };
            Ellipse[] bottomPips =
            {
                pipBottomTL,
                pipBottomTR,
                pipBottomML,
                pipBottomC,
                pipBottomMR,
                pipBottomBL,
                pipBottomBR
            };
            RenderPips(TopValue, topPips);
            RenderPips(BottomValue, bottomPips);
        }

        private static void RenderPips(int value, Ellipse[] pips)
        {
            if (pips == null || pips.Length < PipsPerHalf)
            {
                return;
            }

            for (int index = 0; index < pips.Length; index++)
            {
                if (pips[index] != null)
                {
                    pips[index].Visibility = Visibility.Collapsed;
                }
            }

            Ellipse topLeftPip = pips[0];
            Ellipse topRightPip = pips[1];
            Ellipse middleLeftPip = pips[2];
            Ellipse centerPip = pips[3];
            Ellipse middleRightPip = pips[4];
            Ellipse bottomLeftPip = pips[5];
            Ellipse bottomRightPip = pips[6];
            switch (value)
            {
                case 1:
                    centerPip.Visibility = Visibility.Visible;
                    break;
                case 2:
                    topLeftPip.Visibility = Visibility.Visible;
                    bottomRightPip.Visibility = Visibility.Visible;
                    break;
                case 3:
                    topLeftPip.Visibility = Visibility.Visible;
                    centerPip.Visibility = Visibility.Visible;
                    bottomRightPip.Visibility = Visibility.Visible;
                    break;
                case 4:
                    topLeftPip.Visibility = Visibility.Visible;
                    topRightPip.Visibility = Visibility.Visible;
                    bottomLeftPip.Visibility = Visibility.Visible;
                    bottomRightPip.Visibility = Visibility.Visible;
                    break;
                case 5:
                    topLeftPip.Visibility = Visibility.Visible;
                    topRightPip.Visibility = Visibility.Visible;
                    centerPip.Visibility = Visibility.Visible;
                    bottomLeftPip.Visibility = Visibility.Visible;
                    bottomRightPip.Visibility = Visibility.Visible;
                    break;
                case 6:
                    topLeftPip.Visibility = Visibility.Visible;
                    topRightPip.Visibility = Visibility.Visible;
                    middleLeftPip.Visibility = Visibility.Visible;
                    middleRightPip.Visibility = Visibility.Visible;
                    bottomLeftPip.Visibility = Visibility.Visible;
                    bottomRightPip.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
