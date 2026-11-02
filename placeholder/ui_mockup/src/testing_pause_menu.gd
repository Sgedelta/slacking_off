extends  Node

# === Signals === (snake_case)
signal on_close_requested()

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)

# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)

# === Godot Methods ===

func  _ready() -> void:
	pass

func  _physics_process(delta: float) -> void:
	pass

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func request_close():
	emit_signal("on_close_requested")

func show_hide_menu(menu_layer) :
	var menu = get_node(menu_layer)
	menu.visible = true
	
func quit():
	request_close()
	load_scene("res://assets/main_menu/scenes/playtest_menu.tscn", Vector2i(1280,720))
	
func load_scene(scene_path: String, scene_size: Vector2i) :
	var scene = load(scene_path)
	update_vp_size(scene_size)
	get_tree().change_scene_to_packed(scene)
	
# update the size to the given new size. Rescale the window to fit as well
func update_vp_size(new_size: Vector2i) :
	# grab sizes
	var old_content_size = get_window().content_scale_size
