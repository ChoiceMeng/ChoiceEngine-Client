/*
Copyright: 2012, ustc All rights reserved.
contact:k283228391@126.com
File name: mass.cpp
Description:Particle in Physical
Author:Silang Quan
Version: 1.0
Date: 2012.12.21
*/
#include "mass.h"  

Mass::Mass()
{
	//ctor  
}
void Mass::init()
{
	force = Vector3D::zero();
	vel = Vector3D::zero();
}
void Mass::simulate(float dt)
{
	Vector3D acl = force / m;
	vel = vel + acl*dt;
	pos = pos + vel*dt;
}
Mass::~Mass()
{
	//dtor  
}
void Mass::checkBnd(float x1, float x2, float y1, float y2)
{
	if (pos.x - size < x1 || pos.x + size > x2) vel.x = -vel.x;
	if (pos.y - size < y1 || pos.y + size > y2) vel.y = -vel.y;
}