using Godot;

namespace Match3;

public partial class Gem : Node2D {
	[Export]
	public Sprite2D _sprite;

	public Slot _slot;
}