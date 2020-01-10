using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blockage.LevelEditor {
public class SlideGrid {
		public bool Won = true;
		public List<Item> Items;
		public int Width;
		public int Height;
		public Item[] Grid;
		private Item _Goal;

		private static readonly string EMPTY = ".";

		public void Init(int width, int height, List<Item> items) {
			Won = false;
			Width = width;
			Height = height;
			Items = items;

			Grid = new Item[Width * Height];

			foreach (var item in items) {
				if (item.Type == ObjectType.Goal)
					_Goal = item;
				else
					for (var y = 0; y < item.Height; y++)
						for (var x = 0; x < item.Width; x++)
							Grid[CoordToIndex(item.X + x, item.Y + y)] = item;
			}
		}

		private int CoordToIndex(int x, int y) => y * Width + x;

		public override string ToString() {
			return string.Join('\n',
				new string('=', 20),
				string.Join('\n', Enumerable.Range(0, Height).Select(y =>
					string.Join(' ', Enumerable.Range(0, Width).Select(x => 
						Grid[CoordToIndex(x, y)]?.ToString() ?? (
							(x == _Goal.X && y == _Goal.Y)
								? _Goal.ToString()
								: EMPTY
						)
					))
				)),
				new string('=', 20)
			);
		}

		public void MoveUp() {
			if (Won) return;

			Item curItem = null;
			for (var i = 0; i < Grid.Length; i++) {
				if (i < Width) continue;				//top row
				if (Grid[i] == null) continue;			//nothing in cell
				if (Grid[i] == curItem) continue;		//already moved
				if (new ObjectType[]{ ObjectType.Fixed, ObjectType.Goal, ObjectType.BlockX, ObjectType.BallX }.Contains(Grid[i].Type)) continue; //invalid moves
				if (Grid[i].Y == 0) continue;

				curItem = Grid[i];
				while (true) {
					var impeded = false;
					for (var x = 0; x < curItem.Width; x++) {
						var cellAbove = Grid[CoordToIndex(curItem.X + x, curItem.Y - 1)];
						if (cellAbove != null) {
							impeded = true;
							break;
						}
					}
					if (impeded) break;

					//clear bottom row and fill top row
					for (var x = 0; x < curItem.Width; x++) {
						Grid[CoordToIndex(curItem.X + x, curItem.Y + curItem.Height - 1)] = null;
						Grid[CoordToIndex(curItem.X + x, curItem.Y - 1)] = curItem;
					}
					curItem.Y--;

					if (BallOnGoal(curItem)) {
						Won = true;
						return;
					}

					if (curItem.Y == 0) break;
				}
			}
		}
		public void MoveDown() {
			if (Won) return;

			Item curItem = null;
			for (var i = Grid.Length - 1; i >= 0; i--) {
				if (i + Width >= Grid.Length) continue;	//bottom row
				if (Grid[i] == null) continue;			//nothing in cell
				if (Grid[i] == curItem) continue;		//already moved
				if (new ObjectType[]{ ObjectType.Fixed, ObjectType.Goal, ObjectType.BlockX, ObjectType.BallX }.Contains(Grid[i].Type)) continue; //invalid moves
				if (Grid[i].Y + Grid[i].Height >= Height) continue;

				curItem = Grid[i];
				while (true) {
					var impeded = false;
					for (var x = 0; x < curItem.Width; x++) {
						var cellBelow = Grid[CoordToIndex(curItem.X + x, curItem.Y + curItem.Height)];
						if (cellBelow != null) {
							impeded = true;
							break;
						}
					}
					if (impeded) break;

					//clear top row and fill bottom row
					for (var x = 0; x < curItem.Width; x++) {
						Grid[CoordToIndex(curItem.X + x, curItem.Y)] = null;
						Grid[CoordToIndex(curItem.X + x, curItem.Y + curItem.Height)] = curItem;
					}
					curItem.Y++;

					if (BallOnGoal(curItem)) {
						Won = true;
						return;
					}

					if (curItem.Y + curItem.Height - 1 == Height - 1) break;
				}
			}
		}
		public void MoveLeft() {
			if (Won) return;

			Item curItem = null;
			for (var x = 1; x < Width; x++) {
				for (var y = 0; y < Height; y++) {
					var cell = Grid[CoordToIndex(x, y)];
					if (cell == null) continue;			//nothing in cell
					if (cell == curItem) continue;		//already moved
					if (new ObjectType[] { ObjectType.Fixed, ObjectType.Goal, ObjectType.BlockY, ObjectType.BallY }.Contains(cell.Type)) continue; //invalid moves
					if (cell.X == 0) continue;

					curItem = cell;
					while (true) {
						var impeded = false;
						for (var by = 0; by < curItem.Height; by++) {
							var cellLeft = Grid[CoordToIndex(curItem.X - 1, curItem.Y + by)];
							if (cellLeft != null) {
								impeded = true;
								break;
							}
						}
						if (impeded) break;

						//clear bottom row and fill top row
						for (var by = 0; by < curItem.Height; by++) {
							Grid[CoordToIndex(curItem.X + curItem.Width - 1, curItem.Y + by)] = null;
							Grid[CoordToIndex(curItem.X - 1, curItem.Y + by)] = curItem;
						}
						curItem.X--;

						if (BallOnGoal(curItem)) {
							Won = true;
							return;
						}

						if (curItem.X == 0) break;
					}
				}
			}
		}
		private bool BallOnGoal(Item i) => _Goal != null && new ObjectType[]{ ObjectType.Ball, ObjectType.BallX, ObjectType.BallY }.Contains(i.Type) && i.X == _Goal.X && i.Y == _Goal.Y;
		public void MoveRight() {
			if (Won) return;

			Item curItem = null;
			for (var x = Width - 2; x >= 0; x--) {
				for (var y = 0; y < Height; y++) {
					var cell = Grid[CoordToIndex(x, y)];
					if (cell == null) continue;			//nothing in cell
					if (cell == curItem) continue;		//already moved
					if (new ObjectType[] { ObjectType.Fixed, ObjectType.Goal, ObjectType.BlockY, ObjectType.BallY }.Contains(cell.Type)) continue; //invalid moves
					if (cell.X + cell.Width >= Width) continue;

					curItem = cell;
					while (true) {
						var impeded = false;
						for (var by = 0; by < curItem.Height; by++) {
							var cellRight = Grid[CoordToIndex(curItem.X + curItem.Width, curItem.Y + by)];
							if (cellRight != null) {
								impeded = true;
								break;
							}
						}
						if (impeded) break;

						//clear bottom row and fill top row
						for (var by = 0; by < curItem.Height; by++) {
							Grid[CoordToIndex(curItem.X, curItem.Y + by)] = null;
							Grid[CoordToIndex(curItem.X + curItem.Width, curItem.Y + by)] = curItem;
						}
						curItem.X++;

						if (BallOnGoal(curItem)) {
							Won = true;
							return;
						}

						if (curItem.X + curItem.Width - 1 == Width - 1) break;
					}
				}
			}
		}
	}
}
