extends  Node

# === Signals === (snake_case)

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
func load_scene(scene_path: String) :
	var scene = load(scene_path)
	get_tree().change_scene_to_packed(scene)
	
func show_hide_menu(menu_layer) :
	var menu = get_node(menu_layer)
	menu.visible = true
	
func on_quit() -> void:
	get_tree().quit()
