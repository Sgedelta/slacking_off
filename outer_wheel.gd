extends Sprite2D

@export var all_symbols: Array[Texture2D] = [] # keep track of indices?
@export var slot_sprites: Array[TextureRect] = []

func randomize_symbols() -> void:
	var symbols: Array[int] = all_symbols.duplicate(true)
	symbols.shuffle()
	symbols = symbols.slice(0, 4)
	#for i in range(4):
		#slot_sprites[i].texture = all_symbols[symbol ids[i]]
