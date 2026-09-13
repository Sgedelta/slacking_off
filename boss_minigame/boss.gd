extends Sprite2D

@export var talking_wheel: Node
@export var boss_minigame: BossMinigame

func _ready() -> void:
	visible = false
	boss_minigame.boss_speaks.connect(_on_boss_speaks)
	boss_minigame.boss_finished.connect(_on_finished)

func _on_boss_speaks(symbol_idx: int, color: Color) -> void:
	visible = false
	
	texture = talking_wheel.all_symbols[symbol_idx]
	modulate = color
	await get_tree().create_timer(0.05).timeout
	visible = true

func _on_finished() -> void:
	visible = false
