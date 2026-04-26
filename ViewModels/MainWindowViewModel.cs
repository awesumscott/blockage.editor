using Blockage.LevelEditor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Blockage.Editor.ViewModels;

internal sealed partial class MainWindowViewModel : ObservableObject {
	private static readonly int _CELLS = 16;
	private int _cellWidth;
	private int _cellHeight;
	private SlideGrid _game;

	[ObservableProperty]
	private ItemViewModel _selectedItem;

	[ObservableProperty]
	private Rectangle _levelRect;


	[ObservableProperty]
	private int _levelWidth = 10;
	partial void OnLevelWidthChanged(int value) => UpdateLevelRect();

	[ObservableProperty]
	private int _levelHeight = 10;
	partial void OnLevelHeightChanged(int value) => UpdateLevelRect();



	private BindingList<ItemViewModel> Items = new();



	private void UpdateLevelRect() {
		//if (LevelWidth != null && int.TryParse(LevelWidth.Text, out var w)) {
		//	_levelWidth = w;
		//	LevelRect.Width = w * _cellWidth + 5;
		//}
		//if (LevelHeight != null && int.TryParse(LevelHeight.Text, out var h)) {
		//	_levelHeight = h;
		//	LevelRect.Height = h * _cellHeight + 5;
		//}
		LevelRect.Width = LevelWidth * _cellWidth + 5;
		LevelRect.Height = LevelHeight * _cellHeight + 5;
	}

	[RelayCommand]
	private void _startGame() {
		UpdateLevelRect();
		_game = new SlideGrid();
		_game.Init(LevelWidth, LevelHeight, Items.Select(x => x.Clone()).ToList());
	}
}
