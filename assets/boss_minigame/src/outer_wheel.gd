extends Sprite2D

@export var all_symbols: Array[Texture2D] = [] 
@export var slot_sprites: Array[Sprite2D] = []
@export var rotation_speed = 0.15

var _offset := 0
var slot_symbol_ids: Array[int] = []
var _wheel_tween: Tween
var locked := false

func _ready() -> void:
	randomize_symbols()
	
func _physics_process(_delta):
	if locked: return
	if Input.is_action_just_pressed("rotate_cw"):
		rotate_cw()
	if Input.is_action_just_pressed("rotate_ccw"):
		rotate_ccw()

func set_locked(value: bool) -> void:
	locked = value
	modulate = Color.WEB_GRAY if locked else Color.WHITE

func randomize_symbols() -> void:
	var ids: Array[int] = []
	for i in range(all_symbols.size()):
		ids.append(i)
	ids.shuffle()
	slot_symbol_ids = ids.slice(0, 4)
	for i in range(4):
		slot_sprites[i].texture = all_symbols[slot_symbol_ids[i]]
		slot_sprites[i].modulate = Color.BLACK
		
func rotate_cw() -> void:
	_offset += 1
	_spin_to()

func rotate_ccw() -> void:
	_offset -= 1
	_spin_to()
	
func _spin_to() -> void:
	if _wheel_tween:
		_wheel_tween.kill()
	_wheel_tween = create_tween()
	_wheel_tween.tween_property(self, "rotation_degrees", _offset * 90, rotation_speed)
	for slot in slot_sprites:
		_wheel_tween.parallel().tween_property(slot, "rotation_degrees", -_offset * 90, rotation_speed)
	
func get_symbol_at_tone(tone: int) -> int:
	var wheel_index := ((tone + _offset) % 4 + 4) % 4
	return slot_symbol_ids[wheel_index]
