using UnityEngine;
using System.Collections;

public class FSM : MonoBehaviour 
{
	//玩家位置
	protected Transform playerTransform;

	//下一个巡逻点
	protected Vector3 destPos;

	//巡逻点表单
	protected GameObject[] pointList;

	//子弹信息
	protected float shootRate;
	protected float elapsedTime;

	protected virtual void Initialize() {}
	protected virtual void FSMUpdate() {}
	protected virtual void FSMFixedUpdate() {}

	//初始化信息
	void Start()
	{
		Initialize();
	}

    // 循环执行子类FSMUpdate方法
	void Update () 
	{
		FSMUpdate();	
	}

	void FixedUpdate()
	{
		FSMFixedUpdate();
	}
}
