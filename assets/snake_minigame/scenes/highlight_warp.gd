extends Node2D

@onready var grid: TileMapLayer = $"../Grid"
@onready var game: Node2D = get_parent()
@export var apple_sprite: Texture2D
@export var hide_cursor: bool = false
var size : Vector2
var _last_tile: Vector2i = Vector2i(-1, -1)
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	size = grid.tile_set.tile_size
	if(hide_cursor): Input.mouse_mode = Input.MOUSE_MODE_HIDDEN

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if grid.mouse_tile != _last_tile:
		if grid.mouse_tile in game.snake_body:
			Input.warp_mouse.call_deferred(map_to_local(_last_tile))
		_last_tile = grid.mouse_tile
		queue_redraw()
		
func _draw() -> void:
	if _last_tile.x < 0:
		return
	draw_texture_rect(apple_sprite, Rect2(Vector2(_last_tile) * size, size), false)
