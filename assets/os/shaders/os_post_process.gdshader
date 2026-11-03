shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float scanline_size = 32;
uniform float scanline_darkness = .75;
uniform float px_size : hint_range(1.0, 500.0, 1.0) = 5;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 size = vec2(textureSize(SCREEN_TEXTURE, 0));
	vec2 scale_up = size / px_size;
	vec2 scale_down = px_size / size;
	vec2 resample_uv = floor(SCREEN_UV * scale_up) * scale_down;

	vec4 col = vec4(texture(SCREEN_TEXTURE, resample_uv).rgb, 1.0);
	
	float scanline_mix =  abs(sin(PI * FRAGCOORD.y / scanline_size));
	col.rgb *=  mix(scanline_darkness, 1.0, scanline_mix);
	
	COLOR = col;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
