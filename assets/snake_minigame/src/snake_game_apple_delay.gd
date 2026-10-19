extends Node

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)
const SNAKE = 0
const SMART_CONE_DEPTH := 3
const DUMB_CONE_DEPTH := 2
const START_BODY: Array[Vector2i] = [
				Vector2i(18,10), 
				Vector2i(20-3,10), Vector2i(19-3,10), Vector2i(18-3,10), 
				Vector2i(20-6,10), Vector2i(19-6,10), Vector2i(18-6,10), 
				Vector2i(20-9,10), Vector2i(19-9,10), Vector2i(18-9,10), 
				Vector2i(20-12,10), Vector2i(19-12,10), Vector2i(18-12,10),
				Vector2i(20-15,10), Vector2i(19-15,10), Vector2i(18-15,10), 
				]
const SNAKE_DIR = Vector2i(1, 0)
# === Exported Vars === (snake_case)
@export var snake_body: Array[Vector2i] #= [Vector2i(5,10),Vector2i(4,10),Vector2i(3,10),Vector2i(2,10), Vector2i(1,10)]
@export var snake_direction = Vector2i(1,0)
@export var snake_cone_step = Vector2i(1,1)
# === Public Vars === (snake_case)
@onready var grid: TileMapLayer = $Grid
@onready var tick: Timer = $SnakeTick
@onready var apple: CharacterBody2D = $Apple
@onready var head_area: Area2D = $Snake/HeadArea
var grid_size: Vector2i
var _ended := false
# === Private Vars === (_underscored_snake_case)
# === Godot Methods ===
func  _ready() -> void:
	grid_size = $Grid.grid_size
	_reset_snake_state()
	draw_snake()
	# TODO: replace with something that isn't literally the laziest way to do this
	#snake_body = [ Vector2i(18,10), 
				#Vector2i(20-3,10), Vector2i(19-3,10), Vector2i(18-3,10), 
				#Vector2i(20-6,10), Vector2i(19-6,10), Vector2i(18-6,10), 
				#Vector2i(20-9,10), Vector2i(19-9,10), Vector2i(18-9,10), 
				#Vector2i(20-12,10), Vector2i(19-12,10), Vector2i(18-12,10), 
				#]
	#snake_direction = Vector2i(1, 0)

func _on_snake_tick_timeout() -> void:
	snake_direction = _find_direction()
	move_snake()
	draw_snake()
	if _snake_lose():
		show_win(true)

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func _apple_tile() -> Vector2i:
	return grid.local_to_map(grid.to_local(apple.global_position))
	
func _reset_snake_state() -> void:
	snake_body = START_BODY.duplicate()
	snake_direction = SNAKE_DIR

func _find_direction() -> Vector2i:
	var left := Vector2i(snake_direction.y, -snake_direction.x)
	var goal_unclamped = _apple_tile() - snake_body[0]
	var g_max = goal_unclamped * (Vector2i.RIGHT if goal_unclamped.abs().max_axis_index() == 0 else Vector2i.DOWN)
	var g_min = goal_unclamped * (Vector2i.RIGHT if goal_unclamped.abs().min_axis_index() == 0 else Vector2i.DOWN)
	var dumb_paths: Array[Vector2i] = [g_max.clampi(-1,1), g_min.clampi(-1, 1)] 
	var smart_paths: Array[Vector2i] = [snake_direction, left, -left]
	#[_move_to_mouse(), snake_direction, left, -left]
	var goal_dir = goal_unclamped.clampi(-1, 1)
	for dir in dumb_paths:
		if dir == Vector2i.ZERO or dir == -snake_direction:
			continue
		if dir != -snake_direction && not (_check_cone_path(goal_dir, dir,
		snake_body[0] + dir, snake_cone_step, false, DUMB_CONE_DEPTH
		)):
			print(dir)
			return dir 
		else:
			print(dir, " failed")
	for dir in smart_paths:
		if dir == Vector2i.ZERO or dir == -snake_direction:
			continue
		if dir != -snake_direction && not (_check_cone_path(goal_dir, dir,
		snake_body[0] + dir, snake_cone_step, true, SMART_CONE_DEPTH
		)):
			print(dir)
			return dir 
		else:
			print(dir, " failed")
	return snake_direction

## returns if there are any cells in the given depth in a cone defined by step, start, and dir.
## if is_smart, returns if there are any blockers in the whole cone. 
## If not, will only check along goal_dir, and is essentially "blind" outside of it
func _check_cone_path(goal_dir: Vector2i, dir: Vector2i, 
					start: Vector2i, step: Vector2i, 
					is_smart:bool, depth: int) -> bool:
	if not is_smart && !(goal_dir.x == dir.x or goal_dir.y == dir.y):
		#print("dumb snake hates directions: ", goal_dir, " ", dir)
		return true #goal dir and dir do not align. treat as blocked!
	var body_minus_tail = snake_body.slice(0, -1)
	# step X is forward, Y step is perp. if dumb, do not garuntee perp check - only check in the right dir
	# turn that into a real step w/ change of basis. 
	var perp_dir = Vector2i(-dir.y, dir.x)
	var dir_fixed = dir + perp_dir if is_smart else goal_dir
	#print(("smart snake says: " if is_smart else "dumb snake says"), 
	#step, " along ", dir + perp_dir, " (from ", dir, ") is now ", dir_fixed)
	
	# loop over cells inside cone
	for d in range(depth + 1):
		for x in range(d * step.x, d * step.x + step.x):
			for y in range((step.y * d) + 1):
				# d and w define depth and width. we check the cell along dir_fixed
				# if we're smart, check along dir_fixed * -perp as well
				var cell = start + (Vector2i(x, y) * dir_fixed)
				#print(d, " x:", x, " y: ", y, " -> ", cell)
				if cell in body_minus_tail || (is_smart && _is_wall(cell)):
					#print(cell, " is a blocker")
					return true
				if is_smart:
					cell = start + (Vector2i(x, y) * (dir_fixed + (-2 * perp_dir)))
					if cell in body_minus_tail || (is_smart && _is_wall(cell)):
						#print(cell, " is a blocker")
						return true
				# seen cell is not in body
	return false

func _is_wall(cell: Vector2i) -> bool:
	return cell.x < 0 or cell.y < 0 or cell.x >= grid_size.x or cell.y >= grid_size.y

func move_snake():
	$Snake.clear()
	var new_head = snake_body[0] + snake_direction
	snake_body.pop_back()
	snake_body.push_front(new_head)
	
func draw_snake():
#	for block in snake_body:
#		$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,false,false,false,Vector2i(8,0))

	for block_index in snake_body.size():
		var block = snake_body[block_index]
		
		if block_index == 0:
			var head_dir = relation2(snake_body[1],snake_body[0])
			if head_dir == 'right': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 0)
			if head_dir == 'left': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 1)
			if head_dir == 'top': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 3)
			if head_dir == 'bottom': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 2)
		elif block_index == snake_body.size() - 1:
			var tail_dir = relation2(snake_body[-1],snake_body[-2])
			if tail_dir == 'right': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 0)
			if tail_dir == 'left': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 1)
			if tail_dir == 'top': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 3)
			if tail_dir == 'bottom': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 2)
		
		else:
			var previous_block = snake_body[block_index + 1] - block
			var next_block = snake_body[block_index - 1] - block
			
			if previous_block.x == next_block.x:
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(4,0), 1)
			elif previous_block.y == next_block.y:
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(4,0), 0)
			else:
				if previous_block.x == -1 and next_block.y == -1 or next_block.x == -1 and previous_block.y == -1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 3)
				if previous_block.x == -1 and next_block.y == 1 or next_block.x == -1 and previous_block.y == 1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 1)
				if previous_block.x == 1 and next_block.y == -1 or next_block.x == 1 and previous_block.y == -1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 2)
				if previous_block.x == 1 and next_block.y == 1 or next_block.x == 1 and previous_block.y == 1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 0)
	head_area.position = $Snake.map_to_local(snake_body[0])

func relation2(first_block:Vector2i,second_block:Vector2i):
	var block_relation = second_block - first_block
	if block_relation == Vector2i(-1,0): return 'left'
	if block_relation == Vector2i(1,0): return 'right'
	if block_relation == Vector2i(0,1): return 'bottom'
	if block_relation == Vector2i(0,-1): return 'top'
	return ''

func _snake_lose():
	var head = snake_body[0]
	# snake leaves the screen
	#print(head)
	
	if _is_wall(head):
		print("snake die")
		return true
		
	# snake bites its own tail
	for block in snake_body.slice(1):
		if block == head:
			print("snake die")
			return true
	return false

func show_win(win: bool):
	if _ended:
		return
	_ended = true
	apple.active = false
	tick.stop()
	%EndLabel.text = "YOU WIN" if win else "YOU LOSE"
	%EndLabel.add_theme_color_override("font_color", Color.GREEN if win else Color.RED)
	%EndLayer.visible = true

func _on_again_button_pressed() -> void:
	reset()

func reset():
	_ended = false
	#snake_body = [Vector2i(5,10),Vector2i(4,10),Vector2i(3,10), Vector2i(2,10), Vector2i(1,10)]
	#snake_direction = Vector2i(1,0)	
	_reset_snake_state()
	$Snake.clear()
	draw_snake()
	apple.global_position = grid.to_global(grid.map_to_local(Vector2i(grid_size.x - 1, grid_size.y / 2)))
	apple.active = true
	%EndLayer.visible = false
	tick.start()


func _on_head_area_body_entered(body: Node2D) -> void:
	if body == apple:
		show_win(false)
