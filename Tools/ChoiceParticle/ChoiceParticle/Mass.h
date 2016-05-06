/*
Copyright: 2012, ustc All rights reserved.
contact:k283228391@126.com
File name: mass.h
Description:Particle in Physical
Author:Silang Quan
Version: 1.0
Date: 2012.12.21
*/
#ifndef MASS_H  
#define MASS_H  
#include "vector3d.h"  

class Mass
{
public:
	Mass();
	Mass(float _m, float _size){ m = _m; size = _size; };
	void init();
	void simulate(float dt);
	void checkBnd(float x1, float x2, float y1, float y2);
	void setForce(Vector3D _force){ force = _force; };
	void setVel(Vector3D _vel){ vel = _vel; };
	virtual ~Mass();
	inline Vector3D getPos(){ return pos; };
	inline Vector3D getVel(){ return vel; };
	inline Vector3D getForce(){ return force; };
	inline float getSize(){ return size; };


protected:
private:
	float size;                         //大小  
	float m;                            // 质量  
	Vector3D pos;                               // 位置  
	Vector3D vel;                               // 速度  
	Vector3D force;                             // 力  
};

#endif // MASS_H  