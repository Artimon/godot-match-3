using Godot;

namespace Match3.Extends;

public static class PackedSceneExtend {
	public static T InstantiateTo<T>(this PackedScene packedScene, Node parent) where T : Node {
		var node = packedScene.Instantiate<T>();

		parent.AddChild(node);

		return node;
	}
}