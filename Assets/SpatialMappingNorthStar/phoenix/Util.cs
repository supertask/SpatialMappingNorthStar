using System.Collections;
using System.Collections.Generic;

public class Util
{
	public static float Remap(float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;	
	}

    //Expects 0.0 <= t <= 1.0
	//http://marupeke296.com/TIPS_No19_interpolation.html
    public static float EaseIn(float t) {
		return t * t;
	}

	//Expects 0.0 <= t <= 1.0
	//http://marupeke296.com/TIPS_No19_interpolation.html
	public static float EaseOut(float t) {
		return t * (2 - t);
	}
}
