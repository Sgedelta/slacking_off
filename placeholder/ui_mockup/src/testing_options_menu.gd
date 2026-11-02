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
func set_visible(menu_layer):
	var menu = get_node(menu_layer)
	menu.visible = false
