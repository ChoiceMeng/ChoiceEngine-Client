using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is adapted and modified from the FSM implementation class available on UnifyCommunity website
/// The license for the code is Creative Commons Attribution Share Alike.
/// It's originally the port of C++ FSM implementation mentioned in Chapter01 of Game Programming Gems 1
/// You're free to use, modify and distribute the code in any projects including commercial ones.
/// Please read the link to know more about CCA license @http://creativecommons.org/licenses/by-sa/3.0/

/// This class represents the States in the Finite State System.
/// Each state has a Dictionary with pairs (transition-state) showing
/// which state the FSM should be if a transition is fired while this state
/// is the current state.
/// Reason method is used to determine which transition should be fired .
/// Act method has the code to perform the actions the NPC is supposed to do if it磗 on this state.
/// </summary>
public abstract class FSMState
{
    //字典，字典中每一项都记录了一个“转换-状态”对 的信息
    protected Dictionary<Transition, FSMStateID> map = new Dictionary<Transition, FSMStateID>();
    //状态编号ID
    protected FSMStateID stateID;
    public FSMStateID ID { get { return stateID; } }
    //下面需要用到的，与各状态相关的变量
    //目标点的位置
    protected Vector3 destPos;
    //巡逻点的数组，存储巡逻要经过的点
    protected Transform[] waypoints;
    //转向速度
    protected float curRotSpeed;
    //移动速度
    protected float curSpeed;
    //AI与玩家距离小于该值，开始追逐
	protected float chaseDistance = 40.0f;
    //小于这个就攻击
	protected float attackDistance = 20.0f;
    //巡逻小于这值，就认为到达巡逻点了
	protected float arriveDistance = 3.0f;

    /// <summary>
    /// 向字典添加项，每项是一个"转换--状态"对
    /// </summary>
    /// <param name="transition"></param>
    /// <param name="id"></param>
    public void AddTransition(Transition transition, FSMStateID id)
    {
        //检查这个转换(可以看做是字典的关键字)是否在字典里
        if (map.ContainsKey(transition))
        {
            //一个转换只能对应一个新状态
            Debug.LogWarning("FSMState ERROR: transition is already inside the map");
            return;
        }
        //如果不在字典，那么将这个转换和转换后的状态作为一个新的字典项，加入字典
        map.Add(transition, id);
        Debug.Log("Added : " + transition + " with ID : " + id);
    }

/// <summary>
/// 从字典中删除项
/// </summary>
/// <param name="trans"></param>
    public void DeleteTransition(Transition trans)
    {
        // 检查是否在字典中，如果在，移除
        if (map.ContainsKey(trans))
        {
            map.Remove(trans);
            return;
        }
        //如果要删除的项不在字典中，报告错误
        Debug.LogError("FSMState ERROR: Transition passed was not on this State List");
    }


/// <summary>
/// 通过查询字典，确定在当前状态下，发生trans转换时，应该转换到新的状态编号并返回
/// </summary>
/// <param name="trans"></param>
/// <returns></returns>
    public FSMStateID GetOutputState(Transition trans)
    {
		return map[trans];
    }


    /// <summary>
    /// 用来确定是否需要转换到其他状态，应该发生哪个转换
    /// </summary>
    /// <param name="player"></param>
    /// <param name="npc"></param>
    public abstract void Reason(Transform player, Transform npc);

/// <summary>
/// 定义了在本状态的角色行为，移动，动画等
/// </summary>
/// <param name="player"></param>
/// <param name="npc"></param>
    public abstract void Act(Transform player, Transform npc);

    /// <summary>
    /// 寻找下一个巡逻点
    /// </summary>
    public void FindNextPoint()
    {
        //Debug.Log("Finding next point");
        int rndIndex = Random.Range(0, waypoints.Length);
        Vector3 rndPosition = Vector3.zero;
        destPos = waypoints[rndIndex].position + rndPosition;
    }

    /// <summary>
    /// Check whether the next random position is the same as current tank position
    /// </summary>
    /// <param name="pos">position to check</param>
    /*
	protected bool IsInCurrentRange(Transform trans, Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - trans.position.x);
        float zPos = Mathf.Abs(pos.z - trans.position.z);

        if (xPos <= 50 && zPos <= 50)
            return true;

        return false;
    }*/
}
