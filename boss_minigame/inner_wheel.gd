extends Sprite2D

@export var arrows: Array[Sprite2D] = [] 
@export var all_colors: Array[Color] = []
var colors: Array[Color] = []

signal selected(position: int, color: Color)
const INPUTS := ["up", "left", "down", "right"]

func _ready() -> void:
	randomize_colors()
	
func _physics_process(_delta):
	for i in range(4):
		if Input.is_action_just_pressed(INPUTS[i]):
			print(i)
			arrows[i].scale = Vector2.ONE
			var tween := create_tween()
			tween.tween_property(arrows[i], "scale", Vector2.ONE * 1.4, 0.1)
			tween.tween_property(arrows[i], "scale", Vector2.ONE, 0.1)
			selected.emit(i, colors[i])
	
func randomize_colors() -> void:
	colors = all_colors.duplicate(true)
	colors.shuffle()
	colors = colors.slice(0, 4)
	for i in range(4):
		arrows[i].modulate = colors[i]
