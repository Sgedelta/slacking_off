extends Node2D

@onready var grid: TileMapLayer = $"../Grid"
var size : Vector2
var _last_tile: Vector2i = Vector2i(-1, -1)
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	size = grid.tile_set.tile_size

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if grid.mouse_tile != _last_tile:
		_last_tile = grid.mouse_tile
		queue_redraw()
		
func _draw() -> void:
	if _last_tile.x < 0:
		return
	print(_last_tile)
	draw_rect(Rect2(Vector2(_last_tile) * size, size), Color(255, 0, 0, 1), true)
