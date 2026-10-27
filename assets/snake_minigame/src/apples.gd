extends Node2D

@export var apple_texture: Texture2D
@export var numApples := 8

@onready var grid: TileMapLayer = %Grid

var cells: Array[Vector2i] = []

func scatter(grid_size: Vector2i, blocked: Array) -> void:
	cells.clear()
	var tries := 0
	while cells.size() < numApples and tries < 500:
		tries += 1
		var c := Vector2i(randi_range(1, grid_size.x - 2), randi_range(1, grid_size.y - 2))
		if c in blocked or c in cells:
			continue
		cells.append(c)
	queue_redraw()

# respawn 1 when 1 eaten
func respawn(grid_size: Vector2i, blocked: Array) -> void:
	for i in 200:
		var c := Vector2i(randi_range(1, grid_size.x - 2), randi_range(1, grid_size.y - 2))
		if c in blocked or c in cells:
			continue
		cells.append(c)
		queue_redraw()
		return

func take(cell: Vector2i) -> bool:
	var i := cells.find(cell)
	if i == -1:
		return false
	cells.remove_at(i)
	queue_redraw()
	return true

func _draw() -> void:
	if apple_texture == null:
		return
	var size := Vector2(grid.tile_set.tile_size)
	for cell in cells:
		draw_texture_rect(apple_texture, Rect2(grid.map_to_local(cell) - size * 0.5, size), false)
