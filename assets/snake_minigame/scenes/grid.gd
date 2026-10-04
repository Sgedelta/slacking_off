extends  TileMapLayer

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var grid_scale: int = 1
# === Public Vars === (snake_case)
var grid_size: Vector2i
# === Private Vars === (_underscored_snake_case)
var mouse_tile: Vector2i = Vector2i(-1, -1)
var _tile_size
# === Godot Methods ===
func  _ready() -> void:
	_tile_size = tile_set.tile_size
	grid_size = get_viewport().size / grid_scale / _tile_size
	var _pattern = tile_set.get_pattern(0)
	for x in range(0, grid_size.x, 2):
		for y in range(0, grid_size.y, 2):
			set_pattern(Vector2i(x, y), _pattern)

func _process(delta: float) -> void:
	var tile := local_to_map(get_local_mouse_position())
	var inside := tile.x >= 0 and tile.y >= 0 and tile.x < grid_size.x and tile.y < grid_size.y
	mouse_tile = tile if inside else Vector2i(-1, -1)
# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)	
