extends Sprite2D

@export var arrows: Array[Sprite2D] = [] 
@export var all_colors: Array[Color] = []

var colors: Array[Color] = []
var _arrow_tweens: Array[Tween] = []
var _base_scale: Vector2
var locked := false

signal selected(position: int, color: Color)
const INPUTS := ["up", "left", "down", "right"]

func _ready() -> void:
	_arrow_tweens.resize(arrows.size())
	_base_scale = arrows[0].scale
	randomize_colors()
	
func _physics_process(_delta):
	if locked: return
	for i in range(4):
		if Input.is_action_just_pressed(INPUTS[i]):
			if _arrow_tweens[i]:
				_arrow_tweens[i].kill()
			_arrow_tweens[i] = create_tween()
			_arrow_tweens[i].tween_property(arrows[i], "scale", _base_scale * 1.4, 0.1)
			_arrow_tweens[i].tween_property(arrows[i], "scale", _base_scale, 0.1)
			selected.emit(i)

func set_locked(value: bool) -> void:
	locked = value
	modulate = Color.WEB_GRAY if locked else Color.WHITE
	
func randomize_colors() -> void:
	colors = all_colors.duplicate(true)
	colors.shuffle()
	colors = colors.slice(0, 4)
	for i in range(4):
		arrows[i].modulate = colors[i]
