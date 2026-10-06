shader_type canvas_item;

const int PULLBACK_INPUT_SIZE = 16;

uniform int pullback_count = 0;
uniform vec4 pullback_loc_dirs[PULLBACK_INPUT_SIZE];
uniform vec3 pullback_weights[PULLBACK_INPUT_SIZE];
uniform sampler2D pullback_texs[PULLBACK_INPUT_SIZE];

varying vec4 vertex_color;


vec2 rotate2D(vec2 v, float angle) {
    float s = sin(angle);
    float c = cos(angle);
    return vec2(c * v.x - s * v.y, s * v.x + c * v.y);
}

void vertex() {
	for(int i = 0; i < min(pullback_count, PULLBACK_INPUT_SIZE); i++) 
	{
		//grab data
		float texSize = vec2(textureSize(pullback_texs[i], 0)).r;
		vec2 local_pos_delta = (VERTEX - pullback_loc_dirs[i].xy) / texSize;
		vec2 offset_dir = normalize(pullback_loc_dirs[i].zw);
		float perp_dist = dot(vec2(-local_pos_delta.y, local_pos_delta.x), offset_dir);
		float parl_dist = dot(local_pos_delta, offset_dir);
		
		//sample 
		vec2 offset_sample = vec2(
			texture(pullback_texs[i], vec2(abs(perp_dist), 0.0)).r,
			texture(pullback_texs[i], vec2(abs(parl_dist), 0.0)).g
			);
		
		//now, get offset
		vec2 offset = offset_dir * offset_sample.x + vec2(offset_dir.y * sign(perp_dist), offset_dir.x * -sign(perp_dist)) * offset_sample.y;

		VERTEX += offset * (smoothstep(pullback_weights[i].z, pullback_weights[i].y, length(local_pos_delta) * texSize) * pullback_weights[i].x);
	}
}

void fragment() {

}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
