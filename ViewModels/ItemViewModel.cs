using Blockage.LevelEditor;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Blockage.Editor.ViewModels;

internal sealed partial class ItemViewModel : ObservableObject {
	public event PropertyChangedEventHandler PropertyChanged;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Name))]
	private int _x;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Name))]
	private int _y;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Name))]
	private int _width;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Name))]
	private int _height;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Name))]
	private ObjectType _type;

	public string Name => $"{Type} ({X}, {Y}) {Width}x{Height}";

	public ItemViewModel Clone() => new() {
		X = X,
		Y = Y,
		Width = Width,
		Height = Height,
		Type = Type
	};
}
