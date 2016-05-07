/*****************************************************************************
Copyright: 2012, ustc All rights reserved.
contact:k283228391@126.com
File name: particle.h
Description:Partical in opengl.
Author:Silang Quan
Version: 1.0
Date: 2012.12.01
 *****************************************************************************/
#ifndef PARTICLE_H
#define PARTICLE_H
#include "vector3d.h"
typedef struct
{
    float r;
    float g;
    float b;
    float alpha;
}Color;

typedef struct
{
    Vector3D position;
    Vector3D velocity;
    Vector3D acceleration;
    Color color;
    float age;
    float life;
    float size;
}Particle;

#endif // PARTICLE_H
