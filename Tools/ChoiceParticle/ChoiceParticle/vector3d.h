#ifndef GVECTOR3_H  
#define GVECTOR3_H  
#include <iostream>  
#include <cmath>  
using namespace std;  
  
  
class Vector3D  
{  
  
public:  
    float x;  
    float y;  
    float z;  
     // 缺省构造函数  
    Vector3D();  
    ~Vector3D();  
     // 用户构造函数  
    Vector3D(float posX, float posY, float posZ);  
    //输出向量信息  
    void getInfo();  
    //矢量加法  
    Vector3D operator+(Vector3D v);  
    //矢量减法  
    Vector3D operator-(Vector3D v);  
    //数乘  
    Vector3D operator*(float n);  
    //数除  
    Vector3D operator/(float n);  
    //向量点积  
    float dotMul(Vector3D v2);  
    //向量叉乘  
    Vector3D crossMul(Vector3D v2);  
    //获取矢量长度  
    float getLength();  
    //向量单位化  
    void Normalize();  

	static Vector3D zero();
};  
  
#endif // GVECTOR3_H  