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
     // ȱʡ���캯��  
    Vector3D();  
    ~Vector3D();  
     // �û����캯��  
    Vector3D(float posX, float posY, float posZ);  
    //���������Ϣ  
    void getInfo();  
    //ʸ���ӷ�  
    Vector3D operator+(Vector3D v);  
    //ʸ������  
    Vector3D operator-(Vector3D v);  
    //����  
    Vector3D operator*(float n);  
    //����  
    Vector3D operator/(float n);  
    //�������  
    float dotMul(Vector3D v2);  
    //�������  
    Vector3D crossMul(Vector3D v2);  
    //��ȡʸ������  
    float getLength();  
    //������λ��  
    void Normalize();  

	static Vector3D zero();
};  
  
#endif // GVECTOR3_H  