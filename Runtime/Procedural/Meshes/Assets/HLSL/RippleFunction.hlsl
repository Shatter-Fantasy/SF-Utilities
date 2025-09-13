void Ripple_float( float3 PositionIn, 
    float3 Origin, 
    float Period, 
    float Speed, 
    float Amplitude,
    out float3 PositionOut,
    out float3 NormalOut,
    out float3 TangentOut
){
  
    // Ripple formula 
    // a = amplitude
    // p = period
    // d = distance to the wave origin
    // s = speed
    // t = time
    // y = current y axis position based on _Time.y
    // y derivative = y` = the derivatives of the y positon based on _Time.y
    // y = a * sin(2 * pi * p(d - (s * t) ) )

    float3 p = PositionIn - Origin;
    float d = length(p);
    float f = 2.0 * PI * Period * (d - (Speed * _Time.y));
    
    /// By doing 0.0 instead of just 0 it starts a float and need no conversion from int to float.
    PositionOut = PositionIn + float3(0.0, Amplitude * sin(f), 0.0);
    
    
    /*  Riiple normal calculations
         y` = ( 2 * PI * a * p * cos(f) ) / d * [x] 
                                                [z]
    */
    float2 derivatives = (2.0 * PI * Amplitude * Period * cos(f) / max(d, 0.0001)) * p.xz;
    TangentOut = float3(1.0, derivatives.x, 0.0);
    NormalOut = cross(float3(0.0, derivatives.y, 1.0), TangentOut);
}