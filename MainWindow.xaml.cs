using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Blockage.LevelEditor {
	public static class Extensions {
		public static string Description(this ObjectType val) {
			DescriptionAttribute[] attributes = (DescriptionAttribute[])val
				.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
			return attributes.Length > 0 ? attributes[0].Description : string.Empty;
		}
	}
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public enum ObjectType {
		[Description("Fixed")]
		Fixed = 0,
		[Description("Block")]
		Block = 1,
		[Description("Block X")]
		BlockX = 2,
		[Description("Block Y")]
		BlockY = 3,
		[Description("Ball")]
		Ball = 4,
		[Description("Ball X")]
		BallX = 5,
		[Description("Ball Y")]
		BallY = 6,
		[Description("Goal")]
		Goal = 7
	};
	public class Item : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private int _x;
		public int X {
			get => _x;
			set {
				_x = value;
				OnPropertyChanged("Name");
				OnPropertyChanged("X");
			}
		}
		private int _y;
		public int Y {
			get => _y;
			set {
				_y = value;
				OnPropertyChanged("Name");
				OnPropertyChanged("Y");
			}
		}
		private int _width;
		public int Width {
			get => _width;
			set {
				_width = value;
				OnPropertyChanged("Name");
				OnPropertyChanged("Width");
			}
		}
		private int _height;
		public int Height {
			get => _height;
			set {
				_height = value;
				OnPropertyChanged("Name");
				OnPropertyChanged("Height");
			}
		}
		private ObjectType _type;
		public ObjectType Type {
			get => _type;
			set {
				_type = value;
				OnPropertyChanged("Name");
				OnPropertyChanged("Type");
			}
		}
		public string Name {
			get {
				return $"{Type.ToString()} ({X.ToString()}, {Y.ToString()}) {Width.ToString()}x{Height.ToString()}";
			}
		}

		//public int DrawX;
		//public int DrawY;
		//public int DrawWidth;
		//public int DrawHeight;
		//public System.Drawing.Color DrawColor;

		public Item Copy() {
			var i = new Item();
			i.X = X;
			i.Y = Y;
			i.Width = Width;
			i.Height = Height;
			i.Type = Type;
			return i;
		}
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	public partial class MainWindow : Window, INotifyPropertyChanged {
		private int CELLS = 16;
		private int _cellWidth;
		private int _cellHeight;
		private int _mouseStartX, _mouseStartY;
		private Item _selectedItem;
		public Item SelectedItem {
			get => _selectedItem;
			set {
				_selectedItem = value;
				OnPropertyChanged("SelectedItem");
			}
		}
		private SlideGrid _game;
		private int _levelWidth = 10,
					_levelHeight = 10;
		public int levelWidth {
			get {return _levelWidth;}
			set {_levelWidth = value; UpdateLevelRect();}
		}
		public int levelHeight {
			get {return _levelHeight;}
			set {_levelHeight = value; UpdateLevelRect();}
		}
		public IEnumerable<string> ObjectTypeValues => Enum.GetValues(typeof(ObjectType)).Cast<ObjectType>().Select(x => x.Description());

		public WriteableBitmap GridBmp;
		public Dictionary<string, ObjectType> TypeList = new Dictionary<string, ObjectType>() {
			{"Fixed", ObjectType.Fixed},
			{"Block", ObjectType.Block},
			{"Block X", ObjectType.BlockX},
			{"Block Y", ObjectType.BlockY},
			{"Goal", ObjectType.Goal},
			{"Ball", ObjectType.Ball},
			{"Ball X", ObjectType.BallX},
			{"Ball Y", ObjectType.BallY}
		};
		private BindingList<Item> Items = new BindingList<Item>();

		public event PropertyChangedEventHandler PropertyChanged;
		public MainWindow() {
			InitializeComponent();

			GridDetails.DataContext = this;
			objType.ItemsSource = TypeList;
			//ItemDetails.DataContext = SelectedItem;
			ItemListBox.DataContext = this;
			ItemListBox.ItemsSource = Items;
			
			var width = (int)Playfield.Width;
			var height = (int)Playfield.Height;
			_cellWidth = width / CELLS;
			_cellHeight = height / CELLS;
			GridBmp = new WriteableBitmap(width, height, 1, 1, PixelFormats.Bgr32, BitmapPalettes.Halftone125);
			RenderOptions.SetBitmapScalingMode(Playfield, BitmapScalingMode.NearestNeighbor);
			RenderOptions.SetEdgeMode(Playfield, EdgeMode.Aliased);
			Playfield.Source = GridBmp;

			DrawOnPlayfield(g => {
				DrawLines(g);
			});
			
			Canvas.SetLeft(LevelRect, _cellWidth - 2);
			Canvas.SetTop(LevelRect, _cellHeight - 2);
			UpdateLevelRect();
		}
		
		private void DrawLines(Graphics g) {
			var pen = Pens.LightGray;
			for (var x = 0; x < CELLS; x++) {
				var newX = x * _cellWidth;
				g.DrawLine(pen, newX, 0, newX, (int)Playfield.Height);
			}
			for (var y = 0; y < CELLS; y++) {
				var newY = y * _cellHeight;
				g.DrawLine(pen, 0, newY, (int)Playfield.Width, newY);
			}
		}

		private void DrawOnPlayfield(Action<Graphics> draw) {
			GridBmp.Lock();
			var b = new Bitmap(GridBmp.PixelWidth,
					GridBmp.PixelHeight,
					GridBmp.BackBufferStride,
					System.Drawing.Imaging.PixelFormat.Format32bppArgb, 
					GridBmp.BackBuffer);
			using(var bitmapGraphics = Graphics.FromImage(b)) {
				bitmapGraphics.SmoothingMode = SmoothingMode.HighSpeed;
				bitmapGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				bitmapGraphics.CompositingMode = CompositingMode.SourceCopy;
				bitmapGraphics.CompositingQuality = CompositingQuality.HighSpeed;
				draw(bitmapGraphics);
			}

			GridBmp.AddDirtyRect(new Int32Rect(0, 0, GridBmp.PixelWidth, GridBmp.PixelHeight));
			GridBmp.Unlock();
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private void RedrawItems() {
			//if (!Items.Any()) return;
			IEnumerable<Item> itemSource = (Playing.IsChecked ?? false) ? (IEnumerable<Item>)_game.Items : Items;

			
			
			DrawOnPlayfield(g => {
				g.Clear(System.Drawing.Color.Black);
				DrawLines(g);
				var brush = System.Drawing.Brushes.Blue;
				var pen = System.Drawing.Pens.LightBlue;
				foreach (var item in itemSource) {
					switch (item.Type) {
						case ObjectType.Fixed:
							brush = System.Drawing.Brushes.Blue;
							pen = System.Drawing.Pens.LightBlue;
							break;
						case ObjectType.Block: case ObjectType.Ball:
							brush = System.Drawing.Brushes.Red;
							pen = System.Drawing.Pens.DarkRed;
							break;
						case ObjectType.BlockX: case ObjectType.BallX:
							brush = System.Drawing.Brushes.Indigo;
							pen = System.Drawing.Pens.Purple;
							break;
						case ObjectType.BlockY: case ObjectType.BallY:
							brush = System.Drawing.Brushes.Green;
							pen = System.Drawing.Pens.LightGreen;
							break;
						case ObjectType.Goal:
							brush = System.Drawing.Brushes.DarkMagenta;
							pen = System.Drawing.Pens.Magenta;
							break;
					}
					switch (item.Type) {
						case ObjectType.BlockX:
						case ObjectType.BlockY:
						case ObjectType.Block:
						case ObjectType.Fixed:
							g.FillRectangle(brush, _cellWidth + _cellWidth * item.X, _cellHeight + _cellHeight * item.Y, _cellWidth * item.Width, _cellHeight * item.Height);
							g.DrawRectangle(pen, _cellWidth + _cellWidth * item.X, _cellHeight + _cellHeight * item.Y, _cellWidth * item.Width, _cellHeight * item.Height);
							break;
						case ObjectType.Ball:
						case ObjectType.BallX:
						case ObjectType.BallY:
						case ObjectType.Goal:
							g.FillEllipse(brush, _cellWidth + _cellWidth * item.X, _cellHeight + _cellHeight * item.Y, _cellWidth * item.Width, _cellHeight * item.Height);
							g.DrawEllipse(pen, _cellWidth + _cellWidth * item.X, _cellHeight + _cellHeight * item.Y, _cellWidth * item.Width, _cellHeight * item.Height);
							break;

					}
				}
			});
		}

		private void UpdateLevelRect() {
			//if (LevelWidth != null && int.TryParse(LevelWidth.Text, out var w)) {
			//	_levelWidth = w;
			//	LevelRect.Width = w * _cellWidth + 5;
			//}
			//if (LevelHeight != null && int.TryParse(LevelHeight.Text, out var h)) {
			//	_levelHeight = h;
			//	LevelRect.Height = h * _cellHeight + 5;
			//}
			LevelRect.Width = levelWidth * _cellWidth + 5;
			LevelRect.Height = levelHeight * _cellHeight + 5;
		}

		private void Playfield_MouseDown(object sender, MouseButtonEventArgs e) {
			var pos = e.GetPosition((IInputElement)Playfield.Parent);
			
			_mouseStartX = (int)Math.Floor(pos.X / _cellWidth);
			_mouseStartY = (int)Math.Floor(pos.Y / _cellHeight);

			Title = $"{_mouseStartX} x {_mouseStartY}";

			var posX = _mouseStartX - 1;
			var posY = _mouseStartY - 1;

			foreach (var item in Items) {
				if (posX >= item.X && posX <= item.X + item.Width - 1) {
					if (posY >= item.Y && posY <= item.Y + item.Height - 1) {
						SelectedItem = item;
						ItemDetails.DataContext = SelectedItem;
						_mouseStartX = item.X + 1;
						_mouseStartY = item.Y + 1;
						return;
					}
				}
			}

			var newItem = new Item();
			newItem.X = posX;
			newItem.Y = posY;
			newItem.Width = 1;
			newItem.Height = 1;
			if (SelectedItem != null && !new ObjectType[]{ObjectType.Ball, ObjectType.BallX, ObjectType.BallY, ObjectType.Goal}.Contains(SelectedItem.Type))
				newItem.Type = SelectedItem.Type;
			//newItem.DrawWidth = _cellWidth;
			//newItem.DrawHeight = _cellHeight;
			//newItem.DrawX = _cellWidth + newItem.X * _cellWidth;
			//newItem.DrawY = _cellHeight + newItem.Y * _cellHeight;
			//newItem.DrawColor = System.Drawing.Color.LightBlue;
			Items.Add(newItem);
			SelectedItem = newItem;

			//ItemDetails.DataContext = SelectedItem;

			RedrawItems();

			//ItemList.ItemsSource = Items;
			//Canvas.SetLeft(SelectedFrame, (x * CELLS) - CELLS);
			//Canvas.SetTop(SelectedFrame, (y * CELLS) - CELLS);

			//SelectedTile.X = x;
			//SelectedTile.Y = y;
			//SelectedIndex = y * 16 + x;
			//DrawTile(TileBmp, Chr, SelectedIndex, 0, 0);
		}

		private void Playfield_MouseMove(object sender, MouseEventArgs e) {
			var pos = e.GetPosition((IInputElement)Playfield.Parent);

			if (_mouseStartX < 1) return;
			if (_mouseStartY < 1) return;
			
			var mouseCurX = (int)Math.Floor(pos.X / _cellWidth);
			var mouseCurY = (int)Math.Floor(pos.Y / _cellHeight);
			
			SelectedItem.Width = mouseCurX < _mouseStartX ? 1 : mouseCurX - _mouseStartX + 1;
			SelectedItem.Height = mouseCurY < _mouseStartY ? 1 : mouseCurY - _mouseStartY + 1;

			RedrawItems();
		}

		private void Playing_Click(object sender, RoutedEventArgs e) {
			if (Playing.IsChecked ?? false) {
				Playing.Content = "Stop Playing";
				StartGame();
			} else {
				Playing.Content = "Play";
			}
			RedrawItems();
		}

		private void Playing_KeyUp(object sender, KeyEventArgs e) {
			if (!Playing.IsChecked ?? true) return;

			switch (e.Key) {
				case Key.Up: _game.MoveUp(); RedrawItems(); break;
				case Key.Down: _game.MoveDown(); RedrawItems(); break;
				case Key.Left: _game.MoveLeft(); RedrawItems(); break;
				case Key.Right: _game.MoveRight(); RedrawItems(); break;
			}
		}

		private void Playfield_MouseUp(object sender, MouseButtonEventArgs e) {
			Playfield_MouseMove(sender, e);

			_mouseStartX = 0;
			_mouseStartY = 0;
		}

		private void Playing_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			if (!Playing.IsChecked ?? true) return;

			e.Handled = true;
		}

		private void Clear(object sender, RoutedEventArgs e) {
			//Items = new BindingList<Item>();
			Items.Clear();
			RedrawItems();
		}
		private void TopRowAdd(object sender, RoutedEventArgs e) {
			LevelHeight.Text = (levelHeight += 1).ToString();
			foreach(var item in Items) {
				item.Y++;
			}
			RedrawItems();
		}
		private void TopRowRemove(object sender, RoutedEventArgs e) {
			LevelHeight.Text = (levelHeight -= 1).ToString();
			foreach(var item in Items) {
				item.Y--;
			}
			RedrawItems();
		}
		private void LeftColAdd(object sender, RoutedEventArgs e) {
			LevelWidth.Text = (levelWidth += 1).ToString();
			foreach(var item in Items) {
				item.X++;
			}
			RedrawItems();
		}
		private void LeftColRemove(object sender, RoutedEventArgs e) {
			LevelWidth.Text = (levelWidth -= 1).ToString();
			foreach(var item in Items) {
				item.X--;
			}
			RedrawItems();
		}

		private void Redraw(object sender, SelectionChangedEventArgs e) {
			RedrawItems();
		}

		private void Redraw(object sender, TextChangedEventArgs e) {
			RedrawItems();
		}

		private void DeleteItem(object sender, RoutedEventArgs e) {
			var index = Items.IndexOf(SelectedItem);
			Items.Remove(SelectedItem);
			SelectedItem = Items[index - 1];
			RedrawItems();
		}

		private void CopyLevel(object sender, RoutedEventArgs e) {
			/*
				{
					"desc": "Rush Hour card 3",
					"solution": "U R D R U L U L D R",
					"width": 7, "height": 6,
					"ball": {"x": 2, "y": 2, "type": "x"},
					"goal": {"x": 6, "y": 2},
					"moversx": [
						{"x": 1, "y": 2},
						{"x": 1, "y": 3, "w": 2},
						{"x": 2, "y": 5, "w": 2}
					],
					"moversy": [
						{"x": 3, "y": 2, "h":  3},
						{"x": 5, "y": 3, "h":  3},
						{"x": 1, "y": 4, "h":  2}
					],
					"fixed": [
						{"x": 6, "y": 0, "h": 2},
						{"x": 6, "y": 3, "h": 3}
					]
				},
			*/
			object getBall() {
				var ball = Items.Where(x => x.Type == ObjectType.Ball || x.Type == ObjectType.BallX || x.Type == ObjectType.BallY).FirstOrDefault();
				if (ball == null) return null;
				return new {
					x = ball.X,
					y = ball.Y,
					type = ball.Type switch {
						ObjectType.BallX => "x",
						ObjectType.BallY => "y",
						_ => null
					}
				};
			}
			object getGoal() {
				var goal = Items.Where(x => x.Type == ObjectType.Goal).FirstOrDefault();
				if (goal == null) return null;
				return new {
					x = goal.X,
					y = goal.Y
				};
			}
			object blockObj(Item i) {
				return new {
					x = i.X,
					y = i.Y,
					w = i.Width > 1 ? i.Width : (int?)null,
					h = i.Height > 1 ? i.Height : (int?)null
				};
			}
			var s = JsonConvert.SerializeObject(new {
					width = levelWidth,
					height = levelHeight,
					ball = getBall(),
					goal = getGoal(),
					movers = Items.Where(x => x.Type == ObjectType.Block).Select(blockObj).ToArray(),
					moversx = Items.Where(x => x.Type == ObjectType.BlockX).Select(blockObj).ToArray(),
					moversy = Items.Where(x => x.Type == ObjectType.BlockY).Select(blockObj).ToArray(),
					Fixed = Items.Where(x => x.Type == ObjectType.Fixed).Select(blockObj).ToArray(),
				},
				Formatting.None, 
				new JsonSerializerSettings { 
					NullValueHandling = NullValueHandling.Ignore
				}
			).Replace("Fixed", "fixed"); //because "fixed" is a reserved word
			Clipboard.SetText(s);
		}

		private void ItemX_PreviewTextInput(object sender, TextCompositionEventArgs e) {

		}

		private void ItemX_PreviewTextInput_1(object sender, TextCompositionEventArgs e) {

		}

		private void GridSizeChanged(object sender, RoutedEventArgs e) {
		}

		private void StartGame() {
			UpdateLevelRect();
			_game = new SlideGrid();
			_game.Init(levelWidth, levelHeight, Items.Select(x => x.Copy()).ToList());
		}
	}
}
