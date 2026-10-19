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
var _tile_size: Vector2i;
var is_blocked: Callable = func(_cell: Vector2i) -> bool: return false

# === Godot Methods ===
func  _ready() -> void:
	_tile_size = tile_set.tile_size
	grid_size = get_viewport().size / grid_scale / _tile_size
	mouse_tile = Vector2i(grid_size.x - 10, int(grid_size.y - 15))
	Input.warp_mouse.call_deferred(map_to_local(mouse_tile))
	var _pattern = tile_set.get_pattern(0)
	for x in range(0, grid_size.x, 2):
		for y in range(0, grid_size.y, 2):
			set_pattern(Vector2i(x, y), _pattern)

func _process(_delta: float) -> void:
	var tile := local_to_map(get_local_mouse_position())
	var inside := tile.x >= 0 and tile.y >= 0 and tile.x < grid_size.x and tile.y < grid_size.y
	if not inside:
		return
	_step_toward(tile)
# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)	
func _step_toward(target: Vector2i) -> void:
	var from := mouse_tile
	var steps := maxi(absi(target.x - from.x), absi(target.y - from.y))
	for i in range(1, steps + 1):
		var cell := Vector2i(Vector2(from).lerp(Vector2(target), float(i) / steps).round())
		if is_blocked.call(cell):
			return
		mouse_tile = cell
