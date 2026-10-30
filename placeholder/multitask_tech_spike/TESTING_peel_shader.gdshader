shader_type canvas_item;

const int PULLBACK_INPUT_SIZE = 16;

uniform vec2 pullback_inputs[PULLBACK_INPUT_SIZE];
uniform float pullback_weights[PULLBACK_INPUT_SIZE];
uniform int pullback_count = 0;

void vertex() {
	vec2 WorldPos = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
	
	for(int i = 0; i < min(pullback_count, PULLBACK_INPUT_SIZE); i++) 
	{
		vec2 change = WorldPos -  pullback_inputs[i];
		VERTEX += smoothstep(500.0, 300.0, length(change)) * pullback_weights[i] * change / max(exp(length(change) / 250.0), 1.0);
	}
}

void fragment() {

}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
