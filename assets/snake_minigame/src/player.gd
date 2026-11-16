extends  Node2D

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)
const DIR_ACTIONS := {
	"left": Vector2i.LEFT,
	"right": Vector2i.RIGHT,
	"up": Vector2i.UP,
	"down": Vector2i.DOWN,
}
# === Exported Vars === (snake_case)
@export var continuous_move: bool = false
@export var start_tile := Vector2i(5, 5)

@export var hop_height := 24.0
@export var stand : CompressedTexture2D
@export var bend : CompressedTexture2D
@export var jump : CompressedTexture2D
# === Public Vars === (snake_case)
@onready var grid: TileMapLayer = %Grid
@onready var game: Node2D = get_parent()
@onready var sprite = $PlayerSprite

var input_dir = Vector2()
var input_dir_stack = []
var apple_tile: Vector2i = Vector2i(-1, -1)
var allow_movement = true

var _try_jump := false
var _jump_tick := 0
var _jump_dir := Vector2i.ZERO
var _hop_tween: Tween
var _last_action := ""
var _pending := Vector2i.ZERO

# === Private Vars === (_underscored_snake_case)

# === Godot Methods ===

func  _ready() -> void:
	apple_tile = start_tile
	input_dir = Vector2i.ZERO
	_try_jump = false
	_jump_tick = 0
	global_position = grid.to_global(grid.map_to_local(apple_tile))

func _physics_process(delta: float) -> void:
	pass

func _process(delta: float) -> void:
	if !allow_movement:
		return
	_update_input_dir()

	if Input.is_action_pressed("space") and _jump_tick == 0:
		_try_jump = true

func _input(event: InputEvent) -> void:
	if not allow_movement:
		return
	for action in DIR_ACTIONS:
		if event.is_action_pressed(action):
			_last_action = action
			_pending = DIR_ACTIONS[action]

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func move(direction: Vector2i):
	if _jump_tick > 0:
		_jump()
		return
	if direction == Vector2i.ZERO: return
	
	if _try_jump:
		_try_jump = false
		_jump_dir = direction
		_jump_tick = 1
		sprite.texture = bend
		return
	
	sprite.texture = stand
	var target_tile := apple_tile + direction
	if(game._is_wall(target_tile) or target_tile in game.snake_body):
		return
	apple_tile = target_tile
	global_position = grid.to_global(grid.map_to_local(target_tile))

func freeze() -> void:
	allow_movement = false
	_pending = Vector2i.ZERO
	_last_action = ""
	input_dir = Vector2i.ZERO
	_try_jump = false

func _jump() -> void:
	if _jump_tick == 1:
		_jump_tick = 2
		var target_tile := apple_tile + _jump_dir * 2
		if not (game._is_wall(target_tile) or target_tile in game.snake_body):
			_hop_to(target_tile)
		sprite.texture = jump
	else:
		_jump_tick = 0
		sprite.texture = bend

func _hop_to(target_tile: Vector2i) -> void:
	#save first pos
	var from_global := global_position
	#new pos
	apple_tile = target_tile
	global_position = grid.to_global(grid.map_to_local(target_tile))

	var start_offset: Vector2 = to_local(from_global)
	sprite.position = start_offset

	if _hop_tween:
		_hop_tween.kill()
	var t: float = game.tick.wait_time * 0.9
	_hop_tween = create_tween().set_parallel(true)
	_hop_tween.tween_property(sprite, "position:x", 0.0, t)
	_hop_tween.tween_method(func(p: float) -> void:
		#parabola
		var arc := -4.0 * hop_height * p * (1.0 - p) 
		sprite.position.y = lerpf(start_offset.y, 0.0, p) + arc, 0.0, 1.0, t)
	#wait then snap sprite to 0
	_hop_tween.chain().tween_callback(func() -> void: sprite.position = Vector2.ZERO)

func _update_input_dir() -> void:
	if _last_action != "" and Input.is_action_pressed(_last_action):
		input_dir = DIR_ACTIONS[_last_action]
		return

	input_dir = Vector2i.ZERO
	for action in DIR_ACTIONS:
		if Input.is_action_pressed(action):
			input_dir = DIR_ACTIONS[action]
			break
		
func _on_apple_tick_timeout() -> void:
	if not allow_movement:
		return
	var dir : Vector2i = _pending if _pending != Vector2i.ZERO else input_dir
	move(dir)
	if _jump_tick == 0:
		_pending = Vector2i.ZERO
