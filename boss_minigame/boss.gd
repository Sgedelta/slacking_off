extends Sprite2D

@export var talking_wheel: Node
@export var boss_minigame: BossMinigame

func _ready() -> void:
	visible = false
	boss_minigame.boss_speaks.connect(_on_boss_speaks)
	boss_minigame.boss_finished.connect(_on_finished)

func _on_boss_speaks(symbol_idx: int, color: Color) -> void:
	visible = true
	texture = talking_wheel.all_symbols[symbol_idx]
	modulate = color

func _on_finished() -> void:
	visible = false
