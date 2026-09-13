extends Polygon2D

@export var cols := 10
@export var rows := 8
@export var screen_size := Vector2(1156, 648)
@export var sprite : Texture2D

func _ready() -> void:
	build_grid();
	
func build_grid():
	self.texture = sprite;
	
	var points := PackedVector2Array()
	for y in range(rows + 1):
		for x in range(cols + 1):
			points.append(Vector2(
				screen_size.x / float(cols) * x,
				screen_size.y / float(rows) * y
			))
	self.polygon = points
