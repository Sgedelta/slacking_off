extends  Node2D

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

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

var input_dir = Vector2i.ZERO
var apple_tile: Vector2i = Vector2i(-1, -1)

var _try_jump := false
var _jump_tick := 0
var _jump_dir := Vector2i.ZERO
var _hop_tween: Tween


# === Private Vars === (_underscored_snake_case)

# === Godot Methods ===

func  _ready() -> void:
	apple_tile = start_tile
	input_dir = Vector2i.ZERO
	_try_jump = false
	_jump_tick = 0
	global_position = grid.to_global(grid.map_to_local(apple_tile))

func _physics_process(delta: float) -> void:
	if Input.is_action_pressed("up"): 
		input_dir = Vector2.UP
	elif Input.is_action_pressed("right"): 
		input_dir = Vector2.RIGHT
	elif Input.is_action_pressed("left"): 
		input_dir = Vector2.LEFT
	elif Input.is_action_pressed("down"): 
		input_dir = Vector2.DOWN
	#might wanna make this 2 ticks? or cooldown
	if Input.is_action_pressed("space") and _jump_tick == 0:
		_try_jump = true

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func move(direction: Vector2i):
	if _jump_tick > 0:
		_jump()
		return
	if direction == Vector2i.ZERO: return
	if not continuous_move: input_dir = Vector2i.ZERO
	
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

func _on_apple_tick_timeout() -> void:
	move(input_dir)
