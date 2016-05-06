#include "Vector3D.h"  

Vector3D::Vector3D()
{
}
Vector3D::~Vector3D()
{
}
Vector3D::Vector3D(float posX, float posY, float posZ)
{
	x = posX;
	y = posY;
	z = posZ;
}

Vector3D Vector3D::operator+(Vector3D v)
{
	return Vector3D(x + v.x, v.y + y, v.z + z);
}
Vector3D Vector3D::operator-(Vector3D v)
{
	return Vector3D(x - v.x, y - v.y, z - v.z);
}
Vector3D Vector3D::operator*(float n)
{
	return Vector3D(x*n, y*n, z*n);
}
Vector3D Vector3D::operator/(float n)
{
	return Vector3D(x / n, y / n, z / n);
}
void Vector3D::getInfo()
{
	cout << "x:" << x << " y:" << y << " z:" << z << endl;
}
float Vector3D::dotMul(Vector3D v2)
{
	return (x*v2.x + y*v2.y + z*v2.z);
}
Vector3D Vector3D::crossMul(Vector3D v2)
{
	Vector3D vNormal;
	// ¼ÆËã´¹Ö±Ê¸Á¿  
	vNormal.x = ((y * v2.z) - (z * v2.y));
	vNormal.y = ((z * v2.x) - (x * v2.z));
	vNormal.z = ((x * v2.y) - (y * v2.x));
	return vNormal;
}
float Vector3D::getLength()
{
	return  (float)sqrt(x*x + y*y + z*z);
}
void Vector3D::Normalize()
{
	float length = getLength();
	x = x / length;
	y = y / length;
	z = z / length;
}

Vector3D Vector3D::zero()
{
	return Vector3D(0, 0, 0);
}