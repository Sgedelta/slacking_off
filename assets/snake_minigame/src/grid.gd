extends  TileMapLayer

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var grid_size:= Vector2i(32, 18)
# === Public Vars === (snake_case)
# === Private Vars === (_underscored_snake_case)

# === Godot Methods ===
func  _ready() -> void:
	var _pattern = tile_set.get_pattern(0)
	var pattern_size := _pattern.get_size()
	for x in range(0, grid_size.x, pattern_size.x):
		for y in range(0, grid_size.y, pattern_size.y):
			set_pattern(Vector2i(x, y), _pattern)
