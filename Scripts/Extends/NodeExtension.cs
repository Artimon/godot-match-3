using Godot;

namespace Match3.Extends;

public static class NodeExtension {
	public static void Remove(this Node2D node) {
		node.GetParent().RemoveChild(node);
		node.QueueFree();
	}
}