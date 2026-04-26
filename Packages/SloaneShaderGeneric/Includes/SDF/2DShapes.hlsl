float smin(float a, float b, float k)
{
    float h = max(k - abs(a - b), 0.0);
    return min(a, b) - h * h * 0.25 / k;
}

float ssub(float a, float b, float k)
{
    float h = clamp( 0.5 - 0.5 * (b + a) / k, 0.0, 1.0 );
    return lerp(a, -b, h) + k * h * (1.0 - h);
}

float sub(float d1, float d2)
{
    return max(d1, -d2);
}

float sdCircle(float2 p, float2 pos, float r )
{
    return length(p - pos) - r;
}

float sdOrientedVesica( float2 p, float2 a, float2 b, float w )
{
    float r = 0.5*length(b-a);
    float d = 0.5*(r*r-w*w)/w;
    float2 v = (b-a)/r;
    float2 c = (b+a)*0.5;
    float2 q = 0.5*abs(mul((p-c), float2x2(v.y,v.x,-v.x,v.y)));
    float3 h = (r*q.x<d*(q.y-r)) ? float3(0.0,r,0.0) : float3(-d,0.0,d+w);
    return length( q-h.xy) - h.z;
}

float sdQuadraticCircle(float2 p, float2 pos, float size)
{
    p = p - pos;
    p = p / size;
    p = abs(p); if( p.y>p.x ) p=p.yx;

    float a = p.x-p.y;
    float b = p.x+p.y;
    float c = (2.0*b-1.0)/3.0;
    float h = a*a + c*c*c;
    float t;
    if( h>=0.0 )
    {   
        h = sqrt(h);
        t = sign(h-a)*pow(abs(h-a),1.0/3.0) - pow(h+a,1.0/3.0);
    }
    else
    {   
        float z = sqrt(-c);
        float v = acos(a/(c*z))/3.0;
        t = -z*(cos(v)+sin(v)*1.732050808);
    }
    t *= 0.5;
    float2 w = float2(-t,t) + 0.75 - t*t - p;
    return length(w) * sign( a*a*0.5+b-1.5 );
}

float sdBox(float2 p, float2 pos, float2 b)
{
    p = p - pos;
    float2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

float cro(float2 a, float2 b ) { return a.x*b.y - a.y*b.x; }\
float dot2(float2 a) { return dot(a,a); }

float sdUnevenCapsule(float2 p, float2 pa, float2 pb, float ra, float rb )
{
    p  -= pa;
    pb -= pa;
    float h = dot(pb,pb);
    float2  q = float2( dot(p,float2(pb.y,-pb.x)), dot(p,pb) )/h;
    
    //-----------
    
    q.x = abs(q.x);
    
    float b = ra-rb;
    float2  c = float2(sqrt(h-b*b),b);
    
    float k = cro(c,q);
    float m = dot(c,q);
    float n = dot(q,q);
    
         if( k < 0.0 ) return sqrt(h*(n            )) - ra;
    else if( k > c.x ) return sqrt(h*(n+1.0-2.0*q.y)) - rb;
                       return m                       - ra;
}

float sdBezier( in float2 pos, in float2 A, in float2 B, in float2 C)
{    
    float2 a = B - A;
    float2 b = A - 2.0*B + C;
    float2 c = a * 2.0;
    float2 d = A - pos;
    float kk = 1.0/dot(b,b);
    float kx = kk * dot(a,b);
    float ky = kk * (2.0*dot(a,a)+dot(d,b)) / 3.0;
    float kz = kk * dot(d,a);      
    float res = 0.0;
    float p = ky - kx*kx;
    float p3 = p*p*p;
    float q = kx*(2.0*kx*kx-3.0*ky) + kz;
    float h = q*q + 4.0*p3;
    if( h >= 0.0) 
    { 
        h = sqrt(h);
        float2 x = (float2(h,-h)-q)/2.0;
        float2 uv = sign(x)*pow(abs(x), float2(1.0, 1.0) / 3.0);
        float t = clamp( uv.x+uv.y-kx, 0.0, 1.0 );
        res = dot2(d + (c + b*t)*t);
    }
    else
    {
        float z = sqrt(-p);
        float v = acos( q/(p*z*2.0) ) / 3.0;
        float m = cos(v);
        float n = sin(v)*1.732050808;
        float3  t = clamp(float3(m+m,-n-m,n-m)*z-kx,0.0,1.0);
        
        float d1 = dot2(d+(c+b*t.x)*t.x);
        float d2 = dot2(d+(c+b*t.y)*t.y);
        if(d1 < d2) 
        {
            res = d1;
        }
        else
        {
            res = d2;
        }
    }
    return sqrt( res );
}

float sdSegment(float2 p, float2 a, float2 b )
{
    float2 pa = p-a, ba = b-a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h );
}