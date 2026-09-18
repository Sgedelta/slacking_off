extends ProgressBar

@export var boss_minigame: BossMinigame

func _ready() -> void:
	boss_minigame.meter_changed.connect(_on_meter_changed)
	min_value = 0
	max_value = boss_minigame.meter_max
	value = boss_minigame.meter

func _on_meter_changed(won: bool, new_value: float, new_max_value: float) -> void:
	max_value = new_max_value
	value = new_value
	var tween: Tween = create_tween()
	if won:
		tween.tween_property(self, "modulate", Color.BLACK, 0.5).from(Color.GREEN)
	else:
		tween.tween_property(self, "modulate", Color.BLACK, 0.5).from(Color.RED)
