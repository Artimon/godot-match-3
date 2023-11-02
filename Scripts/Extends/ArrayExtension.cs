using Godot;

namespace Match3.Extends;

public static class ArrayExtension {
	public static T GetRandomElement<T>(this T[] array) {
		var index = GD.Randi() % array.Length;

		return array[index];
	}
}